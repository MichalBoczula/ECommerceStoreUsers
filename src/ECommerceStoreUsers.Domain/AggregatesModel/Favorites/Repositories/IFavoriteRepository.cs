namespace ECommerceStoreUsers.Domain.AggregatesModel.Favorites.Repositories
{
    public interface IFavoriteRepository
    {
        Task<IReadOnlyList<Favorite>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task<Favorite?> GetByClientAndProductIdAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default);
        Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid clientId, Guid productId, CancellationToken cancellationToken = default);
        Task DeleteAllByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    }
}
