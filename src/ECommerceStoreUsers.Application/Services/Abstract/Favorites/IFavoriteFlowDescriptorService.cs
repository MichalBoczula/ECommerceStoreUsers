using ECommerceStoreUsers.Application.Common.FlowDescriptors;

namespace ECommerceStoreUsers.Application.Services.Abstract.Favorites
{
    public interface IFavoriteFlowDescriptorService
    {
        FlowDescriptor GetGetFavoritesByClientIdDescriptor();
        FlowDescriptor GetAddProductToFavoritesDescriptor();
        FlowDescriptor GetRemoveProductFromFavoritesDescriptor();
        FlowDescriptor GetClearClientFavoritesDescriptor();
    }
}
