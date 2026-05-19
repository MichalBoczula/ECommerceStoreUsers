using ECommerceStoreUsers.Infrastructure.Context;

namespace ECommerceStoreUsers.Infrastructure.Configuration
{
    internal class MongoInitializer
    {
        private readonly MongoDbContext _context;

        public MongoInitializer(MongoDbContext context)
        {
            _context = context;
        }
    }
}
