using Ez.Hress.Notifications.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Notifications;

public class DataAccessConstructionTests
{
    private const string UnreachableConnectionString =
        "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:1/devstoreaccount1;QueueEndpoint=http://127.0.0.1:1/devstoreaccount1;BlobEndpoint=http://127.0.0.1:1/devstoreaccount1";

    private static readonly BlobConnectionInfo Info = new(UnreachableConnectionString);

    [Fact]
    public void Constructors_DoNoIO_SoUnreachableStorageDoesNotThrow()
    {
        _ = new NotificationTableAccess(Info, Mock.Of<ILogger<NotificationTableAccess>>());
        _ = new NotificationSeenTableAccess(Info, Mock.Of<ILogger<NotificationSeenTableAccess>>());
        _ = new PushSubscriptionTableAccess(Info, Mock.Of<ILogger<PushSubscriptionTableAccess>>());
        _ = new PushQueueStorageAccess(Info, Mock.Of<ILogger<PushQueueStorageAccess>>());
    }

    [Fact]
    public async Task EnsureCreatedOnce_RunsOnceOnSuccess()
    {
        var calls = 0;
        var sut = new EnsureCreatedOnce(() => { calls++; return Task.CompletedTask; });

        await sut.RunAsync();
        await sut.RunAsync();

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task EnsureCreatedOnce_RetriesAfterFailure()
    {
        var calls = 0;
        var sut = new EnsureCreatedOnce(() =>
        {
            calls++;
            return calls == 1 ? Task.FromException(new InvalidOperationException("down")) : Task.CompletedTask;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(sut.RunAsync);
        await sut.RunAsync();
        await sut.RunAsync();

        Assert.Equal(2, calls);
    }
}
