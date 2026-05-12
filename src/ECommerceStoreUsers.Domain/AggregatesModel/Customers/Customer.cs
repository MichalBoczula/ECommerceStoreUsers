using ECommerceStoreUsers.Domain.AggregatesModel.Customers.Entities;
using ECommerceStoreUsers.Domain.AggregatesModel.Customers.ValueObjects;

namespace ECommerceStoreUsers.Domain.AggregatesModel.Customers
{
    public sealed class Customer
    {
        public Guid Id { get; init; }
        public string ExternalId { get; init; } 

        public IndividualData Individual { get; private set; }

        private readonly List<CompanyData> _companies = new();
        public IReadOnlyCollection<CompanyData> Companies => _companies.AsReadOnly();

        public DateTime UpdatedAt { get; private set; }

        public Customer(string externalId, IndividualData individual)
        {
            Id = Guid.NewGuid();
            ExternalId = externalId;
            Individual = individual;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateIndividualData(IndividualData newData)
        {
            Individual = newData;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddCompany(string name, string taxId, Address billing, Address shipping)
        {
            if (_companies.Any(x => x.TaxId == taxId))
                throw new InvalidOperationException("Firma o tym NIP jest już zarejestrowana.");

            _companies.Add(new CompanyData(Guid.NewGuid(), name, taxId, billing, shipping));
            UpdatedAt = DateTime.UtcNow;
        }

        public static Customer Rehydrate(Guid id, string externalId, IndividualData individual, List<CompanyData> companies)
        {
            var customer = new Customer(externalId, individual) { Id = id };
            customer._companies.AddRange(companies);
            return customer;
        }
    }
}
