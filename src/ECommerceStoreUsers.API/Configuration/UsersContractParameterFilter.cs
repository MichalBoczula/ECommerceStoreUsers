using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ECommerceStoreUsers.API.Configuration;

/// <summary>Describes the existing route parameters without adding binding or validation rules.</summary>
public sealed class UsersContractParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
            return;
        foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
        {
            parameter.Description = parameter.Name switch
            {
                "externalId" => "External account reference used to find the Customer or Admin profile.",
                "id" or "customerId" => "Customer resource UUID.",
                "adminId" => "Admin resource UUID.",
                "companyId" => "Company UUID within the identified Customer.",
                "clientId" => "Client UUID for Favorites; Users does not check for an existing Customer.",
                "productId" => "Product UUID for Favorites; Users does not query the catalog.",
                _ => parameter.Description
            };
            parameter.Example = JsonValue.Create(parameter.Name == "externalId" ? "account-123" : "11111111-1111-1111-1111-111111111111");
        }
    }
}
