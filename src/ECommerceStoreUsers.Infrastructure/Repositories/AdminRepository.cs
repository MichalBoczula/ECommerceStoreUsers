using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
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
        var historyDocument = AdminMapping.MapToHistoryDocument(admin, ActionType.Insert);

        using var session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            await _context.Admins.InsertOneAsync(session, adminDocument, cancellationToken: cancellationToken);
            await _context.AdminsHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return admin;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Admin> UpdateAdmin(Admin admin, CancellationToken cancellationToken)
    {
        var adminDocument = AdminMapping.MapToDocument(admin);
        var historyDocument = AdminMapping.MapToHistoryDocument(admin, ActionType.Update);

        using var session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            await _context.Admins.ReplaceOneAsync(
                session,
                x => x.Id == adminDocument.Id,
                adminDocument,
                cancellationToken: cancellationToken);

            await _context.AdminsHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return admin;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);
            throw;
        }
    }
}