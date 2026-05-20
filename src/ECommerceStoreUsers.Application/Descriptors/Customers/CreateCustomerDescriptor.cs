//using ECommerceStoreUsers.Application.Common.FlowDescriptors;
//using ECommerceStoreUsers.Application.Common.RequestsDto.Customers;
//using ECommerceStoreUsers.Application.Common.ResponsesDto.Customers;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
//using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Repositories;
//using ECommerceStoreUsers.Domain.Validation.Abstract;
//using ECommerceStoreUsers.Domain.Validation.Common;

//namespace ECommerceStoreUsers.Application.Descriptors.Customers
//{
//    internal sealed record CreateCustomer;

//    internal sealed class CreateCustomerDescriptor : FlowDescriberBase<CreateCustomer>
//    {
//        [FlowStep(order: 1, bpmnId: "ValidateExternalId")]
//        public async Task<ValidationResult> ValidateExternalId(string externalId, IValidationPolicy<string> stringValidationPolicy)
//        {
//            return await stringValidationPolicy.Validate(externalId);
//        }

//        [FlowStep(order: 2, bpmnId: "IsExternalIdValid")]
//        public void ThrowValidationExceptionIfExternalIdInvalid(ValidationResult validationResult)
//        {
//            if (!validationResult.IsValid)
//            {
//                throw new ValidationException(validationResult);
//            }
//        }

//        [FlowStep(order: 3, bpmnId: "LoadExistingCustomer")]
//        public async Task<Customer?> LoadCustomer(string externalId, ICustomerRepository customerRepository)
//        {
//            return await customerRepository.GetByExternalIdAsync(externalId);
//        }

//        [FlowStep(order: 4, bpmnId: "IsCustomerAlreadyExists")]
//        public void ThrowAlreadyExistsExceptionIfCustomerExists(string externalId, Customer? customer)
//        {
//            if (customer is not null)
//            {
//                throw new ResourceAlreadyExistsException(nameof(LoadCustomer), externalId, nameof(Customer));
//            }
//        }

//        [FlowStep(order: 5, bpmnId: "MapRequestToIndividualData")]
//        public IndividualData MapRequestToIndividualData(CreateCustomerRequestDto request)
//        {
//            return new IndividualData(
//                request.Individual.FirstName,
//                request.Individual.LastName,
//                request.Individual.Email,
//                request.Individual.Phone,
//                CustomerMappingDto.MapAddress(request.Individual.BillingAddress),
//                CustomerMappingDto.MapAddress(request.Individual.ShippingAddress)
//            );
//        }

//        [FlowStep(order: 6, bpmnId: "ValidateIndividualData")]
//        public async Task<ValidationResult> ValidateIndividualData(IndividualData individualData, IValidationPolicy<IndividualData> validationPolicy)
//        {
//            return await validationPolicy.Validate(individualData);
//        }

//        [FlowStep(order: 7, bpmnId: "IsIndividualDataValid")]
//        public void ThrowValidationExceptionIfIndividualDataInvalid(ValidationResult validationResult)
//        {
//            if (!validationResult.IsValid)
//            {
//                throw new ValidationException(validationResult);
//            }
//        }

//        [FlowStep(order: 8, bpmnId: "CreateCustomerAggregate")]
//        public Customer Create(string externalId, IndividualData individualData)
//        {
//            return new Customer(externalId, individualData);
//        }

//        [FlowStep(order: 9, bpmnId: "SaveCustomer")]
//        public async Task<Customer> SaveCustomer(Customer customer, ICustomerRepository customerRepository)
//        {
//            return await customerRepository.CreateCustomer(customer);
//        }

//        [FlowStep(order: 10, bpmnId: "MapCustomerResponse")]
//        public CustomerResponseDto MapToResponse(Customer customer)
//        {
//            return CustomerMappingDto.MapToResponse(customer);
//        }
//    }
//}
