using ECommerceStoreInvoice.API.Configuration;
using ECommerceStoreInvoice.API.Configuration.Extensions;
using ECommerceStoreUsers.API.Endpoints;
using ECommerceStoreUsers.Application;
using ECommerceStoreUsers.Domain;
using ECommerceStoreUsers.Infrastructure;

namespace ECommerceStoreUsers.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddOpenApiDocument(options =>
            {
                options.PostProcess = document =>
                {
                    foreach (var schema in document.Components.Schemas.Values)
                    {
                        schema.FixGuidFormats();
                    }
                };
            });

            builder.Services.AddHealthChecks();
            builder.Services.AddExceptionHandler<ExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddDomain();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();

            var app = builder.Build();

            app.UseExceptionHandler();
            app.UseOpenApi();
            app.UseSwaggerUi();
            app.UseHttpsRedirection();
            app.MapDocumentationEndpoints();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
