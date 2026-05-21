namespace ECommerceStoreUsers.Domain.Validation.Common
{
    public sealed class ResourceNotFoundException : Exception
    {
        public string ActionName { get; private set; }
        public string ResourceId { get; private set; }
        public string ResourceType { get; private set; }

        public ResourceNotFoundException(string actionName, string resourceId, string resourceType)
        {
            ActionName = actionName;
            ResourceId = resourceId;
            ResourceType = resourceType;
        }
    }
}
