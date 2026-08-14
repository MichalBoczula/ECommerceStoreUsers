using ECommerceStoreUsers.Domain.AggregatesModel.Favorites;
using ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories;
using ECommerceStoreUsers.Infrastructure.Context;
using ECommerceStoreUsers.Infrastructure.Mapping;
using ECommerceStoreUsers.Infrastructure.Persistance.Favorites;
using MongoDB.Driver;

namespace ECommerceStoreUsers.Infrastructure.Repositories
{
    internal sealed class FavoriteRepository : IFavoriteRepository
    {
        private readonly MongoDbContext _context;

        public FavoriteRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Favorite>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<FavoriteDocument>.Filter.Eq(x => x.ClientId, clientId);

            var documents = await _context.Favorites
                .Find(filter)
                .SortByDescending(x => x.AddedAt)
                .ToListAsync(cancellationToken);

            return documents.Select(FavoriteMapping.MapToDomain).ToList();
        }

        public async Task<Favorite?> GetByClientAndProductIdAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<FavoriteDocument>.Filter.And(
                Builders<FavoriteDocument>.Filter.Eq(x => x.ClientId, clientId),
                Builders<FavoriteDocument>.Filter.Eq(x => x.ProductId, productId)
            );

            var document = await _context.Favorites
                .Find(filter)
                .FirstOrDefaultAsync(cancellationToken);

            return document is not null ? FavoriteMapping.MapToDomain(document) : null;
        }

        public async Task<bool> ExistsAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<FavoriteDocument>.Filter.And(
                Builders<FavoriteDocument>.Filter.Eq(x => x.ClientId, clientId),
                Builders<FavoriteDocument>.Filter.Eq(x => x.ProductId, productId)
            );

            return await _context.Favorites
                .Find(filter)
                .AnyAsync(cancellationToken);
        }

        public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
        {
            var document = FavoriteMapping.MapToDocument(favorite);
            await _context.Favorites.InsertOneAsync(document, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<FavoriteDocument>.Filter.And(
                Builders<FavoriteDocument>.Filter.Eq(x => x.ClientId, clientId),
                Builders<FavoriteDocument>.Filter.Eq(x => x.ProductId, productId)
            );

            await _context.Favorites.DeleteOneAsync(filter, cancellationToken);
        }

        public async Task DeleteAllByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<FavoriteDocument>.Filter.Eq(x => x.ClientId, clientId);
            await _context.Favorites.DeleteManyAsync(filter, cancellationToken);
        }
    }
}
