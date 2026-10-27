using ECommerceStoreUsers.API;
using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Domain;
using ECommerceStoreUsers.Domain.Validation.Abstract;
using ECommerceStoreUsers.Domain.Validation.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Features.Common;

/// <summary>Joins declared source knowledge and scenario intent to actual HTTP; never claims step tracing.</summary>
internal sealed class DocumentationEvidence
{
    private static readonly Lazy<Catalogue> Source = new(Catalogue.Read);
    private readonly CaseLink? _case;
    private readonly string? _technicalBranch;
    private int _primaryCount;

    internal DocumentationEvidence(string scenarioId, string? technicalBranch = null)
    {
        Source.Value.Cases.TryGetValue(scenarioId, out _case);
        if (technicalBranch is not null)
        {
            if (!Source.Value.Branches.TryGetValue(technicalBranch, out var branch) || branch.FlowId is not null)
                throw new InvalidOperationException("Unknown or nontechnical branch binding.");
            if (_case is not null && _case.BranchId != technicalBranch)
                throw new InvalidOperationException("Technical branch conflicts with the matrix.");
        }
        _technicalBranch = _case is null ? technicalBranch : null;
    }

    internal void Complete()
    {
        if ((_case is not null || _technicalBranch is not null) && _primaryCount != 1)
            throw new InvalidOperationException($"Expected one primary When exchange, observed {_primaryCount}.");
    }

    internal async Task Observe(ScenarioHttpEvidence evidence, HttpRequestMessage request, HttpResponseMessage response, string? operationId, CancellationToken cancellation)
    {
        var catalogue = Source.Value;
        var status = (int)response.StatusCode;
        request.Options.TryGetValue(HttpEvidenceHandler.ExchangeIdKey, out var exchangeId);
        request.Options.TryGetValue(HttpEvidenceHandler.StepPhaseKey, out var phase);
        request.Options.TryGetValue(HttpEvidenceHandler.SupportingExchangeKey, out var supporting);
        var primary = phase == "When" && !supporting && (_case is not null
            ? operationId == _case.OperationId && status == _case.Status : _technicalBranch is not null);
        var branchId = primary ? _case?.BranchId ?? _technicalBranch : null;
        var branch = branchId is null ? null : catalogue.Branches[branchId];
        catalogue.OperationFlows.TryGetValue(operationId ?? "", out var flow);
        var observedRules = new HashSet<string>(StringComparer.Ordinal);
        string? problemCode = null;
        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            using var body = JsonDocument.Parse(await response.Content.ReadAsByteArrayAsync(cancellation));
            problemCode = body.RootElement.GetProperty("code").GetString();
            foreach (var error in body.RootElement.GetProperty("errors").EnumerateArray())
            {
                var name = error.GetProperty("name").GetString()!;
                if (!catalogue.RuleNames.TryGetValue(name, out var identity))
                    throw new InvalidOperationException($"Undocumented actual validation rule: {name}");
                observedRules.Add(identity);
            }
        }
        var expectedRules = primary ? _case?.RuleIds ?? [] : [];
        var rulesDeclared = observedRules.Count == 0 || flow is not null && !observedRules.Except(flow.RuleIds).Any();
        evidence.Record("HTTP documentation links", new
        {
            service = "users",
            scenarioId = evidence.ScenarioId,
            instanceId = evidence.InstanceId,
            exchangeId,
            operationId,
            method = request.Method.Method,
            path = request.RequestUri!.AbsolutePath,
            status,
            stepPhase = phase,
            explicitlySupporting = supporting,
            role = primary ? "primary-scenario-outcome" : "supporting-or-technical-exchange",
            linkBasis = "Declared source metadata and scenario expectation joined to actual HTTP; not runtime branch/step telemetry.",
            nominalFlowId = flow?.Id,
            declaredStepIds = flow?.StepIds ?? [],
            declaredPolicyIds = flow?.PolicyIds ?? [],
            expectedBranchId = branchId,
            expectedBranchStage = branch?.Stage,
            expectedRuleIds = expectedRules,
            branchContextRuleIds = branch?.RuleIds ?? [],
            observedErrorRuleIds = observedRules.Order(StringComparer.Ordinal).ToArray(),
            observedRulesDeclaredByNominalFlow = rulesDeclared,
            problemCode
        });
        if (!rulesDeclared)
            throw new InvalidOperationException("Actual rejection contains an uninstalled or unrelated rule.");
        if (!primary)
            return;
        Interlocked.Increment(ref _primaryCount);
        if (status != branch!.Status || (problemCode is null ? branch.Codes.Length != 0 : !branch.Codes.Contains(problemCode)))
            throw new InvalidOperationException("Actual status/problem code differs from the declared scenario outcome.");
        if (expectedRules.Except(observedRules).Any())
            throw new InvalidOperationException("Expected rule is absent from the actual rejection response.");
    }

    internal void ObserveFailure(ScenarioHttpEvidence evidence, HttpRequestMessage request, string? operationId, string exceptionType)
    {
        Source.Value.OperationFlows.TryGetValue(operationId ?? "", out var flow);
        request.Options.TryGetValue(HttpEvidenceHandler.ExchangeIdKey, out var exchangeId);
        request.Options.TryGetValue(HttpEvidenceHandler.StepPhaseKey, out var phase);
        evidence.Record("HTTP documentation links", new
        {
            service = "users",
            scenarioId = evidence.ScenarioId,
            instanceId = evidence.InstanceId,
            exchangeId,
            operationId,
            method = request.Method.Method,
            path = request.RequestUri!.AbsolutePath,
            stepPhase = phase,
            role = "transport-failure-without-response",
            exceptionType,
            nominalFlowId = flow?.Id,
            declaredStepIds = flow?.StepIds ?? [],
            declaredPolicyIds = flow?.PolicyIds ?? [],
            linkBasis = "Nominal source flow only; the terminal transport failure is not an HTTP response or proof of executed steps."
        });
    }

    internal static Catalogue SourceCatalogue => Source.Value;
    internal sealed record CaseLink(string ScenarioId, string OperationId, int Status, string BranchId, string[] RuleIds);
    internal sealed record Branch(string Id, string? FlowId, string Stage, int Status, string[] Codes, string[] RuleIds);
    internal sealed record Flow(string Id, string[] StepIds, string[] PolicyIds, string[] RuleIds, Dictionary<string, string> StepDescriptions);

    internal sealed class Catalogue
    {
        internal Dictionary<string, CaseLink> Cases { get; } = new(StringComparer.Ordinal);
        internal Dictionary<string, Branch> Branches { get; } = new(StringComparer.Ordinal);
        internal Dictionary<string, Flow> OperationFlows { get; } = new(StringComparer.Ordinal);
        internal Dictionary<string, string> RuleNames { get; } = new(StringComparer.Ordinal);

        internal static Catalogue Read()
        {
            var result = new Catalogue();
            var domainTypes = typeof(DocumentationAttribute).Assembly.GetTypes();
            var services = new ServiceCollection().AddDomain();
            var policies = services.Where(service => service.ServiceType == typeof(IValidationPolicyDescriptorProvider)).Select(service => service.ImplementationType!).ToArray();
            var policyRules = new Dictionary<Type, string[]>();
            foreach (var type in domainTypes.Where(type => type.GetCustomAttribute<DocumentationAttribute>() is not null &&
                         type.GetInterfaces().Any(item => item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IValidationRule<>))))
                result.RuleNames.Add(type.Name, type.GetCustomAttribute<DocumentationAttribute>()!.Id);
            foreach (var policy in policies)
            {
                var descriptor = ((IValidationPolicyDescriptorProvider)Activator.CreateInstance(policy)!).Describe();
                policyRules.Add(policy, descriptor.Rules.Select(rule => result.RuleNames[rule.RuleName]).Distinct().ToArray());
            }
            var flows = new Dictionary<string, Flow>(StringComparer.Ordinal);
            foreach (var type in typeof(FlowOutcomeAttribute).Assembly.GetTypes().Concat(typeof(Program).Assembly.GetTypes()))
            {
                var isFlow = type.BaseType?.IsGenericType == true && type.BaseType.GetGenericTypeDefinition().Name == "FlowDescriberBase`1";
                var doc = type.GetCustomAttribute<DocumentationAttribute>();
                if (isFlow)
                {
                    if (doc is null) throw new InvalidOperationException("Flow lacks stable source metadata.");
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    var steps = methods.Select(method => method.GetCustomAttribute<FlowStepAttribute>()).OfType<FlowStepAttribute>().OrderBy(step => step.Order).ToArray();
                    foreach (var step in steps.Where(step => step.RuleId is not null))
                    {
                        if (result.RuleNames.TryGetValue(step.RuleName!, out var existing) && existing != step.RuleId)
                            throw new InvalidOperationException("Ambiguous inline guard identity.");
                        result.RuleNames[step.RuleName!] = step.RuleId!;
                    }
                    var kinds = methods.SelectMany(method => method.GetParameters()).Select(parameter => parameter.ParameterType)
                        .Where(kind => kind.IsGenericType && kind.GetGenericTypeDefinition() == typeof(IValidationPolicy<>))
                        .Select(kind => kind.GenericTypeArguments.Single()).Distinct().ToArray();
                    var invoked = kinds.Select(kind => policies.Single(policy => policy.GetInterfaces().Any(item => item.IsGenericType &&
                        item.GetGenericTypeDefinition() == typeof(IValidationPolicy<>) && item.GenericTypeArguments.Single() == kind))).ToArray();
                    flows.Add(doc.Id, new Flow(doc.Id, steps.Select(step => doc.Id + "." + step.BpmnId).ToArray(),
                        invoked.Select(policy => policy.GetCustomAttribute<DocumentationAttribute>()!.Id).Order(StringComparer.Ordinal).ToArray(),
                        invoked.SelectMany(policy => policyRules[policy]).Concat(steps.Where(step => step.RuleId is not null).Select(step => step.RuleId!)).Distinct().ToArray(),
                        steps.ToDictionary(step => doc.Id + "." + step.BpmnId, step => step.Description)));
                }
                foreach (var outcome in type.GetCustomAttributes<FlowOutcomeAttribute>())
                    result.Branches.Add(outcome.Id, new Branch(outcome.Id, isFlow ? doc!.Id : null, outcome.Stage, outcome.Status,
                        outcome.Code?.Split('|') ?? [], outcome.RuleIds));
            }
            using var stream = typeof(DocumentationEvidence).Assembly.GetManifestResourceStream("Users.AcceptanceMatrix")
                ?? throw new InvalidOperationException("Missing embedded acceptance matrix.");
            using var reader = new StreamReader(stream);
            var header = reader.ReadLine()!.Split('\t');
            while (reader.ReadLine() is { } line)
            {
                var cells = line.Split('\t');
                var row = header.Select((name, index) => (name, value: cells[index])).ToDictionary(pair => pair.name, pair => pair.value);
                var entry = new CaseLink(row["scenarioId"], row["operationId"], int.Parse(row["status"], System.Globalization.CultureInfo.InvariantCulture), row["branchId"], row["ruleIds"].Split(',', StringSplitOptions.RemoveEmptyEntries));
                var branch = result.Branches[entry.BranchId];
                if (entry.Status != branch.Status || entry.RuleIds.Except(branch.RuleIds).Any())
                    throw new InvalidOperationException("Scenario status/rules disagree with its source branch.");
                result.Cases.Add(entry.ScenarioId, entry);
            }
            foreach (var group in result.Cases.Values.GroupBy(entry => entry.OperationId))
            {
                var identities = group.Select(entry => result.Branches[entry.BranchId].FlowId).OfType<string>().Distinct().ToArray();
                if (identities.Length > 1) throw new InvalidOperationException("Operation maps to multiple nominal flows.");
                if (identities.Length == 1) result.OperationFlows.Add(group.Key, flows[identities[0]]);
            }
            return result;
        }
    }
}
