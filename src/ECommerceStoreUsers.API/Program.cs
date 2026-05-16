using ECommerceStoreInvoice.API.Configuration;
using ECommerceStoreInvoice.API.Configuration.Extensions;

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

            var app = builder.Build();

            app.UseExceptionHandler();

            app.UseOpenApi();
            app.UseSwaggerUi();
            app.UseHttpsRedirection();

            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
