using ECommerceStoreUsers.Application.Common.FlowDescriptors;
using ECommerceStoreUsers.Application.Descriptors.Favorites;
using ECommerceStoreUsers.Application.Services.Abstract.Favorites;

namespace ECommerceStoreUsers.Application.Services.Concrete.Favorites
{
    internal sealed class FavoriteFlowDescriptorService : IFavoriteFlowDescriptorService
    {
        public FlowDescriptor GetGetFavoritesByClientIdDescriptor()
        {
            var descriptor = new GetFavoritesByClientIdDescriptor();
            return descriptor.Describe();
        }

        public FlowDescriptor GetAddProductToFavoritesDescriptor()
        {
            var descriptor = new AddProductToFavoritesDescriptor();
            return descriptor.Describe();
        }

        public FlowDescriptor GetRemoveProductFromFavoritesDescriptor()
        {
            var descriptor = new RemoveProductFromFavoritesDescriptor();
            return descriptor.Describe();
        }

        public FlowDescriptor GetClearClientFavoritesDescriptor()
        {
            var descriptor = new ClearClientFavoritesDescriptor();
            return descriptor.Describe();
        }
    }
}
