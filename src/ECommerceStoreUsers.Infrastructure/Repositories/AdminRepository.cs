using ECommerceStoreUsers.Domain.AggregatesModel.Employees;
using ECommerceStoreUsers.Domain.AggregatesModel.Employees.Repositories;
using ECommerceStoreUsers.Domain.Common.Enums;
using ECommerceStoreUsers.Domain.Validation.Common;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistence.Admins;
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

        var currentInserted = false;
        try
        {
            await _context.Admins.InsertOneAsync(session, adminDocument, cancellationToken: cancellationToken);
            currentInserted = true;
            await _context.AdminsHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            return admin;
        }
        catch (MongoWriteException exception) when (!currentInserted && exception.WriteError.Code == 11000 &&
            exception.WriteError.Message.Contains("index: UX_Admin_ExternalId dup key:", StringComparison.Ordinal))
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            // Confirm the committed target after abort; unrelated unique keys are server errors.
            if (await _context.Admins.Find(x => x.ExternalId == admin.ExternalId).AnyAsync(cancellationToken))
                throw new ResourceAlreadyExistsException(nameof(CreateAdmin), admin.ExternalId, nameof(Admin));
            throw;
        }
        catch (MongoCommandException exception) when (exception.Code == 112)
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw new ConcurrencyConflictException();
        }
        catch (MongoWriteException exception) when (exception.WriteError.Code == 112)
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw new ConcurrencyConflictException();
        }
        catch
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw;
        }
    }

    public async Task<Admin> UpdateAdmin(Admin admin, CancellationToken cancellationToken)
    {
        using var session = await _context.Client.StartSessionAsync(cancellationToken: cancellationToken);
        session.StartTransaction();

        try
        {
            var current = await _context.Admins.Find(session, x => x.Id == admin.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (current is null)
                throw new ResourceNotFoundException(nameof(UpdateAdmin), admin.Id.ToString(), nameof(Admin));

            if (current.Version != admin.Version)
                throw new ConcurrencyConflictException();

            // A profile PUT with the same values is a successful no-op, not a new history event.
            if (current.ExternalId == admin.ExternalId && current.FullName == admin.FullName &&
                current.Email == admin.Email && current.IsActive == admin.IsActive &&
                current.LastLoginAt == admin.LastLoginAt)
            {
                await session.CommitTransactionAsync(cancellationToken);
                return admin;
            }

            var adminDocument = AdminMapping.MapToDocument(admin) with { Version = checked(admin.Version + 1) };
            var filter = Builders<AdminDocument>.Filter;
            var versionFilter = filter.Eq(x => x.Version, admin.Version);
            // Older documents have no Version field; MongoDB deserializes them as version zero.
            if (admin.Version == 0)
                versionFilter = filter.Or(versionFilter, filter.Exists(x => x.Version, false));

            var result = await _context.Admins.ReplaceOneAsync(
                session,
                filter.And(filter.Eq(x => x.Id, adminDocument.Id), versionFilter),
                adminDocument,
                cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
                throw new ConcurrencyConflictException();

            var historyDocument = AdminMapping.MapToHistoryDocument(admin, ActionType.Update, adminDocument.Version);
            await _context.AdminsHistory.InsertOneAsync(session, historyDocument, cancellationToken: cancellationToken);

            await session.CommitTransactionAsync(cancellationToken);
            admin.MarkPersisted();
            return admin;
        }
        catch (MongoCommandException exception) when (exception.Code == 112)
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw new ConcurrencyConflictException();
        }
        catch (MongoWriteException exception) when (exception.WriteError.Code == 112)
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw new ConcurrencyConflictException();
        }
        catch
        {
            await MongoTransactionAbort.TryAbortAsync(session.AbortTransactionAsync);
            throw;
        }
    }
}

