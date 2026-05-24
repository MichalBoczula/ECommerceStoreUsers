using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Repositories;

internal sealed class AdminRepository : IAdminRepository
{
    private readonly MongoDbContext _context;

    public AdminRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Admin?> GetByIdAsync(Guid adminId, CancellationToken cancellationToken)
    {
        var adminDocument = await _context.Admins
            .Find(x => x.Id == adminId)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminDocument is null)
            return null;

        return AdminMapping.MapToDomain(adminDocument);
    }

    public async Task<Admin?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken)
    {
        var adminDocument = await _context.Admins
            .Find(x => x.ExternalId == externalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (adminDocument is null)
            return null;

        return AdminMapping.MapToDomain(adminDocument);
    }

    public async Task<Admin> CreateAdmin(Admin admin, CancellationToken cancellationToken)
    {
        var adminDocument = AdminMapping.MapToDocument(admin);

        await _context.Admins.InsertOneAsync(adminDocument, cancellationToken: cancellationToken);

        return admin;
    }

    public async Task<Admin> UpdateAdmin(Admin admin, CancellationToken cancellationToken)
    {
        var adminDocument = AdminMapping.MapToDocument(admin);

        await _context.Admins.ReplaceOneAsync(
            x => x.Id == adminDocument.Id,
            adminDocument,
            cancellationToken: cancellationToken);

        return admin;
    }
}
