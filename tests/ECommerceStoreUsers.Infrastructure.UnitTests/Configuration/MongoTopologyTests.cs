using ECommerceStoreUsers.Infrastructure.Health;
using MongoDB.Bson;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Configuration;

public sealed class MongoTopologyTests
{
    [Theory]
    [InlineData("{ setName: 'rs0', isWritablePrimary: true, logicalSessionTimeoutMinutes: 30 }", true)]
    [InlineData("{ msg: 'isdbgrid', isWritablePrimary: true, logicalSessionTimeoutMinutes: 30 }", true)]
    [InlineData("{ isWritablePrimary: true, logicalSessionTimeoutMinutes: 30 }", false)]
    [InlineData("{ setName: 'rs0', isWritablePrimary: false, logicalSessionTimeoutMinutes: 30 }", false)]
    [InlineData("{ msg: 'isdbgrid', isWritablePrimary: false, logicalSessionTimeoutMinutes: 30 }", false)]
    [InlineData("{ setName: 'rs0', isWritablePrimary: true }", false)]
    [InlineData("{ msg: 'isdbgrid', isWritablePrimary: true, logicalSessionTimeoutMinutes: null }", false)]
    [InlineData("{ msg: 'isdbgrid', isWritablePrimary: true, logicalSessionTimeoutMinutes: 0 }", false)]
    [InlineData("{ setName: '', isWritablePrimary: true, logicalSessionTimeoutMinutes: 30 }", false)]
    [InlineData("{ setName: 42, isWritablePrimary: 'true', logicalSessionTimeoutMinutes: '30' }", false)]
    [InlineData("{ msg: 'other-router', isWritablePrimary: true, logicalSessionTimeoutMinutes: 30 }", false)]
    public void OnlyWritableReplicaSetsAndMongoRoutersWithSessionsAreEligible(string response, bool expected)
    {
        MongoReadinessHealthCheck.SupportsTransactionTopology(BsonDocument.Parse(response)).ShouldBe(expected);
    }
}
