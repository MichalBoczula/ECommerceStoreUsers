using ECommerceStoreUsers.Application.Descriptors.Customers;
using Shouldly;

namespace ECommerceStoreUsers.Application.UnitTests.Services;

public class UpdateCompanyFlowDescriptionTests
{
    [Fact]
    public void Describe_ShouldValidateBothIdentifiersBeforeLoadingCustomer()
    {
        var steps = new UpdateCompanyDescriptor().Describe().Steps;

        steps.Select(step => step.Order).ShouldBe(Enumerable.Range(1, steps.Count));
        steps.Select(step => step.StepName).Take(6).ShouldBe(new[]
        {
            nameof(UpdateCompanyDescriptor.ValidateCustomerId),
            nameof(UpdateCompanyDescriptor.ThrowValidationExceptionIfCustomerIdInvalid),
            nameof(UpdateCompanyDescriptor.ValidateCompanyId),
            nameof(UpdateCompanyDescriptor.ThrowValidationExceptionIfCompanyIdInvalid),
            nameof(UpdateCompanyDescriptor.LoadCustomer),
            nameof(UpdateCompanyDescriptor.ThrowNotFoundExceptionIfCustomerMissing)
        });
    }
}
