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

            var app = builder.Build();

            await app.Services.InitializeInfrastructureAsync();

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

            app.Run();
        }
    }
}
