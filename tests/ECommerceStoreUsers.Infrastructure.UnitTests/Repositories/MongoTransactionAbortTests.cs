using ECommerceStoreUsers.Infrastructure.Repositories;
using Shouldly;

namespace ECommerceStoreUsers.Infrastructure.UnitTests.Repositories;

public sealed class MongoTransactionAbortTests
{
    [Fact]
    public async Task AbortUsesIndependentTokenWhenRequestWasCanceled()
    {
        using var request = new CancellationTokenSource();
        request.Cancel();
        var abortWasCalled = false;

        await MongoTransactionAbort.TryAbortAsync(token =>
        {
            abortWasCalled = true;
            token.CanBeCanceled.ShouldBeTrue();
            token.IsCancellationRequested.ShouldBeFalse();
            token.ShouldNotBe(request.Token);
            return Task.CompletedTask;
        });

        abortWasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task AbortFailureDoesNotReplaceWriteFailure()
    {
        var writeFailure = new InvalidOperationException("write failed");
        Exception? propagated = null;

        try
        {
            try
            {
                throw writeFailure;
            }
            catch
            {
                await MongoTransactionAbort.TryAbortAsync(_ =>
                    Task.FromException(new OperationCanceledException("abort failed")));
                throw;
            }
        }
        catch (Exception exception)
        {
            propagated = exception;
        }

        propagated.ShouldBeSameAs(writeFailure);
    }

    [Fact]
    public async Task AbortStopsWaitingAfterCleanupTimeout()
    {
        var abortWasCanceled = false;

        await MongoTransactionAbort.TryAbortAsync(async token =>
        {
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
            }
            catch (OperationCanceledException)
            {
                abortWasCanceled = true;
                throw;
            }
        }, TimeSpan.FromMilliseconds(100));

        abortWasCanceled.ShouldBeTrue();
    }
}
