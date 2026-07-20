using ECommerceStoreInvoice.API.Configuration;
using ECommerceStoreInvoice.API.Configuration.Extensions;
using ECommerceStoreUsers.API.Endpoints;
using ECommerceStoreUsers.Application;
using ECommerceStoreUsers.Domain;
using ECommerceStoreUsers.Infrastructure;
using ECommerceStoreUsers.Infrastructure.Configuration;

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

            builder.Services.AddHealthChecks();
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
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapDocumentationEndpoints();
            app.MapCustomersEndpoints();
            app.MapAdminsEndpoints();
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
