namespace Ez.Hress.Notifications.DataAccess;

/// <summary>
/// Runs a create-if-not-exists action once per instance, on first use, so constructors do no I/O.
/// A failed attempt is not cached: the next call retries.
/// </summary>
internal sealed class EnsureCreatedOnce(Func<Task> create)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile bool _done;

    public async Task RunAsync()
    {
        if (_done)
        {
            return;
        }

        await _gate.WaitAsync();
        try
        {
            if (!_done)
            {
                await create();
                _done = true;
            }
        }
        finally
        {
            _gate.Release();
        }
    }
}
