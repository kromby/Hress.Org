using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Shared;

public class AuthenticationInteractorTests
{
    private readonly Mock<IAuthenticationDataAccess> _authMock;
    private readonly Mock<ILogger<AuthenticationInteractor>> _log;
    public AuthenticationInteractorTests()
    {
        _authMock = new();
        _log = new();
    }

    [Fact]
    public async Task LoginOK_TestAsync()
    {
        // SETUP
        var username = "username";
        var password = "password";
        var ipAddress = "127.0.0.1";
        var userID = 220;
        _authMock.Setup(x => x.GetUserID(username, It.IsAny<string>())).Returns(Task.FromResult<int>(userID));
        _authMock.Setup(x => x.SaveLoginInformation(userID, ipAddress)).Returns(Task.Delay(100));
        var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");
        var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

        // ACT
        var jwt = await interactor.LoginAsync(username, password, ipAddress);

        // ASSERT
        Assert.False(string.IsNullOrEmpty(jwt));
    }

    [Fact]
    public async Task LoginErrorMissingUsername_TestAsync()
    {
        // SETUP
        var username = string.Empty;
        var password = "password";
        var ipAddress = "127.0.0.1";
        var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");
        var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.LoginAsync(username, password, ipAddress));           
    }

    [Fact]
    public async Task LoginErrorMissingPassword_TestAsync()
    {
        // SETUP
        var username = "username";
        var password = string.Empty;
        var ipAddress = "127.0.0.1";
        var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");            
        var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.LoginAsync(username, password, ipAddress));
    }

    [Fact]
    public async Task LoginErrorUnauthorized_TestAsync()
    {
        // SETUP
        var username = "username";
        var password = "password";
        var ipAddress = "127.0.0.1";
        var userID = -1;
        _authMock.Setup(x => x.GetUserID(username, It.IsAny<string>())).Returns(Task.FromResult<int>(userID));            
        var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");
        var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => interactor.LoginAsync(username, password, ipAddress));
    }

    [Fact]
    public async Task CreateMagicCodeOK_TestAsync()
    {
        // ARRANGE
        var userID = 220;
        _authMock.Setup(x => x.SaveMagicCode(userID, It.IsAny<string>(), It.Is<DateTime>(x => x > DateTime.UtcNow && x < DateTime.UtcNow.AddMinutes(3)))).Returns(Task.FromResult(1));
        var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");
        var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

        // ACT
        var result = await interactor.CreateMagicCodeAsync(userID);

        // ASSERT
        Assert.False(string.IsNullOrWhiteSpace(result));
    }
}
