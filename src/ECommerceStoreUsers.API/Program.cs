using ECommerceStoreUsers.API.Configuration;
using ECommerceStoreUsers.API.Configuration.Common;
using ECommerceStoreUsers.API.Endpoints;
using ECommerceStoreUsers.Application;
using ECommerceStoreUsers.Domain;
using ECommerceStoreUsers.Infrastructure;
using ECommerceStoreUsers.Infrastructure.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerceStoreUsers.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SupportNonNullableReferenceTypes();
                c.UseAllOfToExtendReferenceSchemas();
                foreach (var assembly in new[]
                {
                    typeof(Program).Assembly,
                    typeof(ECommerceStoreUsers.Application.Common.RequestsDto.Customers.CreateCustomerRequestDto).Assembly,
                    typeof(ECommerceStoreUsers.Domain.Validation.Common.ValidationError).Assembly
                })
                    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml"));
                c.SchemaFilter<UsersContractSchemaFilter>();
                c.OperationFilter<UsersContractParameterFilter>();
            });

            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);
            builder.Services.Configure<RouteHandlerOptions>(options =>
            {
                options.ThrowOnBadRequest = true;
            });
            builder.Services.AddExceptionHandler<ExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddDomain();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();
            builder.Services.AddHostedService<MongoInitializationHostedService>();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.UseStatusCodePages(status => ApiProblemResponse.WriteEmptyStatusAsync(status.HttpContext));
            app.UseRouting();
            app.Use(async (context, next) =>
            {
                RequiredJsonProperties.EnableInspection(context);
                await next(context);
            });
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapDocumentationEndpoints();
            app.MapCustomersEndpoints();
            app.MapAdminsEndpoints();
            app.MapFavoritesEndpoints();
            var liveOptions = new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live")
            };
            app.MapHealthChecks("/health", liveOptions);
            app.MapHealthChecks("/health/live", liveOptions);
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("ready")
            });

            await app.RunAsync();
        }
    }
}

