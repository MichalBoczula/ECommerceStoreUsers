namespace ECommerceStoreUsers.Domain.Validation.Common
{
    public class ResourceAlreadyExistsException : Exception
    {
        public string ActionName { get; private set; }
        public string ResourceId { get; private set; }
        public string ResourceType { get; private set; }

        public ResourceAlreadyExistsException(string actionName, string resourceId, string resourceType)
        {
            ActionName = actionName;
            ResourceId = resourceId;
            ResourceType = resourceType;
        }
    }
}