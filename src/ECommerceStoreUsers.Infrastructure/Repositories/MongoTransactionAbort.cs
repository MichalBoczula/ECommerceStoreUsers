using System.Diagnostics;

namespace ECommerceStoreUsers.Infrastructure.Repositories;

internal static class MongoTransactionAbort
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    internal static async Task TryAbortAsync(
        Func<CancellationToken, Task> abort,
        TimeSpan? timeout = null)
    {
        // The request token may already be canceled when the write fails.
        using var cleanup = new CancellationTokenSource(timeout ?? DefaultTimeout);

        try
        {
            await abort(cleanup.Token);
        }
        catch (Exception abortException)
        {
            // Preserve the original write/commit failure, including its stack trace.
            try
            {
                Trace.TraceWarning("MongoDB transaction abort failed: {0}", abortException.GetType().Name);
            }
            catch
            {
                // Diagnostic listeners must not replace the original failure either.
            }
        }
    }
}
