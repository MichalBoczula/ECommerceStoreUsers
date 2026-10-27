using ECommerceStoreUsers.AcceptanceTests.Features.Common;
using System.Text.Json;

namespace ECommerceStoreUsers.AcceptanceTests.Configuration;

public sealed class JsonContractTests
{
    public static IEnumerable<object[]> Cases()
    {
        yield return ["string type", "{\"type\":\"string\"}", "123", false, false];
        yield return ["boolean type", "{\"type\":\"boolean\"}", "\"true\"", false, false];
        yield return ["integer fraction", "{\"type\":\"integer\"}", "1.5", false, false];
        yield return ["int32 overflow", "{\"type\":\"integer\",\"format\":\"int32\"}", "2147483648", false, false];
        yield return ["int64 overflow", "{\"type\":\"integer\",\"format\":\"int64\"}", "9223372036854775808", false, false];
        yield return ["uuid format", "{\"type\":\"string\",\"format\":\"uuid\"}", "\"broken\"", false, false];
        yield return ["date calendar", "{\"type\":\"string\",\"format\":\"date-time\"}", "\"2026-02-30T10:00:00Z\"", false, false];
        yield return ["date timezone", "{\"type\":\"string\",\"format\":\"date-time\"}", "\"2026-10-27T10:00:00\"", false, false];
        yield return ["date time separator", "{\"type\":\"string\",\"format\":\"date-time\"}", "\"2026-10-27 10:00:00Z\"", false, false];
        yield return ["nonnullable string", "{\"type\":\"string\"}", "null", false, false];
        yield return ["nonnullable optional member", "{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"}}}", "{\"name\":null}", false, false];
        yield return ["required member", "{\"type\":\"object\",\"required\":[\"name\"],\"properties\":{\"name\":{\"type\":\"string\"}}}", "{}", false, false];
        yield return ["array type", "{\"type\":\"array\",\"items\":{\"type\":\"string\"}}", "{}", false, false];
        yield return ["array element", "{\"type\":\"array\",\"items\":{\"type\":\"string\"}}", "[\"ok\",false]", false, false];
        yield return ["nested object type", "{\"type\":\"object\",\"properties\":{\"child\":{\"$ref\":\"#/components/schemas/Child\"}}}", "{\"child\":{\"flag\":\"true\"}}", false, false];
        yield return ["nested required", "{\"$ref\":\"#/components/schemas/Child\"}", "{}", false, false];
        yield return ["null reference", "{\"allOf\":[{\"$ref\":\"#/components/schemas/Child\"}]}", "null", false, false];
        yield return ["dictionary value", "{\"type\":\"object\",\"additionalProperties\":{\"$ref\":\"#/components/schemas/Child\"}}", "{\"entry\":{\"flag\":1}}", false, false];
        yield return ["closed object", "{\"type\":\"object\",\"additionalProperties\":false}", "{\"unexpected\":1}", false, false];
        yield return ["enum", "{\"type\":\"string\",\"enum\":[\"yes\"]}", "\"no\"", false, false];
        yield return ["response case matters", "{\"type\":\"object\",\"required\":[\"name\"],\"properties\":{\"name\":{\"type\":\"string\"}}}", "{\"NAME\":\"ok\"}", false, false];
        yield return ["valid integer", "{\"type\":\"integer\",\"format\":\"int32\"}", "42", true, false];
        yield return ["valid boolean", "{\"type\":\"boolean\"}", "false", true, false];
        yield return ["valid UUID", "{\"type\":\"string\",\"format\":\"uuid\"}", "\"11111111-1111-1111-1111-111111111111\"", true, false];
        yield return ["valid UTC date", "{\"type\":\"string\",\"format\":\"date-time\"}", "\"2026-10-27T10:00:00.1234567Z\"", true, false];
        yield return ["valid offset date", "{\"type\":\"string\",\"format\":\"date-time\"}", "\"2026-10-27T11:00:00+01:00\"", true, false];
        yield return ["nullable string", "{\"type\":\"string\",\"nullable\":true}", "null", true, false];
        yield return ["nullable collection", "{\"type\":\"array\",\"nullable\":true,\"items\":{\"type\":\"string\"}}", "null", true, false];
        yield return ["empty collection", "{\"type\":\"array\",\"items\":{\"type\":\"string\"}}", "[]", true, false];
        yield return ["valid dictionary", "{\"type\":\"object\",\"additionalProperties\":{\"$ref\":\"#/components/schemas/Child\"}}", "{\"entry\":{\"flag\":true}}", true, false];
        yield return ["request casing and ignored fields", "{\"type\":\"object\",\"required\":[\"name\"],\"properties\":{\"name\":{\"type\":\"string\"}},\"additionalProperties\":true}", "{\"NAME\":\"ok\",\"future\":{\"anything\":true}}", true, true];
        yield return ["open problem extension", "{\"type\":\"object\",\"additionalProperties\":{}}", "{\"extension\":null}", true, false];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void ChecksScalarNullableContainerAndFormatContracts(string cause, string schemaJson, string bodyJson, bool conforms, bool request)
    {
        using var schema = JsonDocument.Parse(schemaJson);
        using var body = JsonDocument.Parse(bodyJson);
        using var components = JsonDocument.Parse("""
            {"Child":{"type":"object","required":["flag"],"properties":{"flag":{"type":"boolean"}},"additionalProperties":false}}
            """);
        var errors = JsonContractValidator.Check(schema.RootElement, body.RootElement, components.RootElement, request);
        Assert.True(conforms == (errors.Count == 0), $"{cause}: {string.Join("; ", errors)}");
    }

    [Fact]
    public void UnsupportedCompositionFailsExplicitlyInsteadOfPassingSilently()
    {
        using var schema = JsonDocument.Parse("""{"oneOf":[{"type":"string"},{"type":"integer"}]}""");
        using var body = JsonDocument.Parse("true");
        using var components = JsonDocument.Parse("{}");
        Assert.Throws<InvalidOperationException>(() => JsonContractValidator.Check(schema.RootElement, body.RootElement, components.RootElement));
    }

    [Fact]
    public void ExcessiveRecursionFailsExplicitlyInsteadOfSkippingChildren()
    {
        using var schema = JsonDocument.Parse("""{"$ref":"#/components/schemas/Loop"}""");
        using var body = JsonDocument.Parse("{}");
        using var components = JsonDocument.Parse("""{"Loop":{"$ref":"#/components/schemas/Loop"}}""");
        var error = Assert.Throws<InvalidOperationException>(() => JsonContractValidator.Check(schema.RootElement, body.RootElement, components.RootElement));
        Assert.Contains("nesting limit", error.Message);
    }
}
