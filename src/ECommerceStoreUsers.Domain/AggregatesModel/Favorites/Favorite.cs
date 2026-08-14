namespace ECommerceStoreUsers.Domain.AggregatesModel.Favorites
{
    public sealed class Favorite
    {
        public Guid Id { get; init; }
        public Guid ClientId { get; init; }
        public Guid ProductId { get; init; }
        public DateTime AddedAt { get; private set; }

        public Favorite(Guid clientId, Guid productId)
        {
            Id = Guid.NewGuid();
            ClientId = clientId;
            ProductId = productId;
            AddedAt = DateTime.UtcNow;
        }

        private Favorite(
            Guid id,
            Guid clientId,
            Guid productId,
            DateTime addedAt)
        {
            Id = id;
            ClientId = clientId;
            ProductId = productId;
            AddedAt = addedAt;
        }

        public static Favorite Rehydrate(
            Guid id,
            Guid clientId,
            Guid productId,
            DateTime addedAt)
        {
            return new Favorite(
                id,
                clientId,
                productId,
                addedAt);
        }
    }
}