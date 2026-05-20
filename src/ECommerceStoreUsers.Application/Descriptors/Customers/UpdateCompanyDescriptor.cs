//using ECommerceStoreUsers.Application.Common.FlowDescriptors;
//using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
//using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;
//using ECommerceStoreUsers.Domain.Validation.Abstract;
//using ECommerceStoreUsers.Domain.Validation.Common;

//namespace ECommerceStoreUsers.Application.Descriptors.Customers
//{
//    internal sealed record UpdateCompany;

//    internal sealed class UpdateCompanyDescriptor : FlowDescriberBase<UpdateCompany>
//    {
//        [FlowStep(order: 1, bpmnId: "ValidateCustomerId")]
//        public async Task<ValidationResult> ValidateCustomerId(Guid id, IValidationPolicy<Guid> guidValidationPolicy)
//        {
//            return await guidValidationPolicy.Validate(id);
//        }

//        [FlowStep(order: 2, bpmnId: "ValidateCompanyId")]
//        public async Task<ValidationResult> ValidateCompanyId(Guid id, IValidationPolicy<Guid> guidValidationPolicy)
//        {
//            return await guidValidationPolicy.Validate(id);
//        }

//        [FlowStep(order: 3, bpmnId: "AreIdentifiersValid")]
//        public void ThrowValidationExceptionIfIdentifiersInvalid(ValidationResult companyResult)
//        {
//            if (!companyResult.IsValid) throw new ValidationException(companyResult);
//        }

//        [FlowStep(order: 4, bpmnId: "LoadCustomerProfile")]
//        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository, CancellationToken cancellationToken)
//        {
//            return await customerRepository.GetByExternalIdAsync(externalId, cancellationToken);
//        }

//        [FlowStep(order: 5, bpmnId: "VerifyCustomerExists")]
//        public void ThrowNotFoundExceptionIfCustomerMissing(string externalId, Customer? customer)
//        {
//            if (customer is null)
//            {
//                throw new ResourceNotFoundException(nameof(LoadCustomer), externalId, nameof(Customer));
//            }
//        }

//        [FlowStep(order: 6, bpmnId: "MapAddress")]
//        public Address MapAddress(AddressRequestDto addressDto)
//        {
//            return CustomerMappingDto.MapAddress(addressDto);
//        }

//        [FlowStep(order: 7, bpmnId: "UpdateCompanyInsideAggregate")]
//        public void UpdateCompanyInsideAggregate(Customer customer, Guid companyId, string taxId, string companyName, Address billing, Address shipping)
//        {
//            var company = customer.Companies.FirstOrDefault(x => x.Id == companyId);
//            if (company is null)
//            {
//                throw new ResourceNotFoundException(nameof(UpdateCompanyInsideAggregate), companyId, nameof(CompanyData));
//            }

//            company.UpdateCompanyDetails(taxId, companyName, billing, shipping);
//        }

//        [FlowStep(order: 8, bpmnId: "SaveCustomer")]
//        public async Task<Customer> SaveCustomer(Customer customer, ICustomerRepository customerRepository)
//        {
//            return await customerRepository.UpdateCustomer(customer);
//        }

//        [FlowStep(order: 9, bpmnId: "MapCustomerResponse")]
//        public CustomerResponseDto MapToResponse(Customer customer)
//        {
//            return CustomerMappingDto.MapToResponse(customer);
//        }
//    }
//}
