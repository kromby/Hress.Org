using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UnitTest.Shared
{
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
        public async Task LoginOK_Test()
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
            var jwt = await interactor.Login(username, password, ipAddress);

            // ASSERT
            Assert.False(string.IsNullOrEmpty(jwt));
        }

        [Fact]
        public async Task LoginErrorMissingUsername_Test()
        {
            // SETUP
            var username = string.Empty;
            var password = "password";
            var ipAddress = "127.0.0.1";
            var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");
            var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Login(username, password, ipAddress));           
        }

        [Fact]
        public async Task LoginErrorMissingPassword_Test()
        {
            // SETUP
            var username = "username";
            var password = string.Empty;
            var ipAddress = "127.0.0.1";
            var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");            
            var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => interactor.Login(username, password, ipAddress));
        }

        [Fact]
        public async Task LoginErrorUnauthorized_Test()
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
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => interactor.Login(username, password, ipAddress));
        }

        [Fact]
        public async Task CreateMagicCodeOK_Test()
        {
            // ARRANGE
            var userID = 220;
            _authMock.Setup(x => x.SaveMagicCode(userID, It.IsAny<string>(), It.Is<DateTime>(x => x > DateTime.UtcNow && x < DateTime.UtcNow.AddMinutes(3)))).Returns(Task.FromResult(1));
            var authInfo = new AuthenticationInfo("keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#keyKEYkey1234.#", "issuer", "audience", "salt");
            var interactor = new AuthenticationInteractor(authInfo, _authMock.Object, _log.Object);

            // ACT
            var result = await interactor.CreateMagicCode(userID);

            // ASSERT
            Assert.False(string.IsNullOrWhiteSpace(result));
        }
    }
}
