using Ez.Hress.Shared.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
//using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ez.Hress.Shared.UseCases
{
    public class AuthenticationInteractor
    {
        private const string _class = nameof(AuthenticationInteractor);
        private readonly AuthenticationInfo _authenticationInfo;
        private readonly IAuthenticationDataAccess _authenticationDataAccess;
        private readonly ILogger<AuthenticationInteractor> _log;
        

        public AuthenticationInteractor(AuthenticationInfo authenticationInfo, IAuthenticationDataAccess authenticationDataAccess, ILogger<AuthenticationInteractor> log)
        {
            if (authenticationInfo == null)
                throw new ArgumentNullException(nameof(authenticationInfo));

            authenticationInfo.Validate();

            _authenticationInfo = authenticationInfo;
            _authenticationDataAccess = authenticationDataAccess;
            _log = log;
        }

        public Tuple<bool, int> GetUserIdFromToken(string scheme, string value)
        {
            if (string.IsNullOrWhiteSpace(scheme) || string.IsNullOrWhiteSpace(value))
            {
                _log.LogInformation("[{Class}] Scheme or token value is missing. Scheme: '{Scheme}'", _class, scheme);
                return new Tuple<bool, int>(false, -1);
            }

            if (scheme.Trim().ToLower() != "token")
            {
                _log.LogInformation("[{Class}] Scheme is not token. Scheme: '{Scheme}'", _class, scheme);
                return new Tuple<bool, int>(false, -1);
            }

            try
            {
                var userIdString = DecryptToken(value);
                if (string.IsNullOrWhiteSpace(userIdString))
                {
                    _log.LogInformation("[{Class}] UserId is missing. userIdString: '{userIdString}'", _class, userIdString);
                    return new Tuple<bool, int>(false, -1);
                }

                return new Tuple<bool, int>(true, int.Parse(userIdString, CultureInfo.CurrentCulture));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "[{Class}] Error in GetUserIdFromToken(..)", _class);
                return new Tuple<bool, int>(false, -1);
            }
        }

        public async Task<string> Login(string username, string password, string ipAddress)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                _log.LogInformation("[{Class}] Username is null or empty", _class);
                throw new ArgumentNullException(nameof(username));
            }

            if(string.IsNullOrWhiteSpace(password))
            {
                _log.LogInformation("[{Class}] Password is null or empty", _class);
                throw new ArgumentNullException(nameof(password));
            }

            string hashed = HashPassword(password, Encoding.UTF32.GetBytes(_authenticationInfo.Salt));

            var userID = await _authenticationDataAccess.GetUserID(username, hashed);
            
            if(userID <= 0)
            {
                _log.LogWarning("[{Class}] User '{username}' not authenticated.", _class, username);
                throw new UnauthorizedAccessException();
            }

            var loginTask = _authenticationDataAccess.SaveLoginInformation(userID, ipAddress);

            var jwt = GetToken(userID.ToString());

            loginTask.Wait();

            return jwt;
        }

        public async Task<string> Login(string code, string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _log.LogInformation("[{Class}] Code is null or empty", _class);
                throw new ArgumentNullException(nameof(code));
            }

            var userID = await _authenticationDataAccess.GetUserID(code);

            if (userID <= 0)
            {
                _log.LogWarning("[{Class}] Code is not valid.", _class);
                throw new UnauthorizedAccessException();
            }

            var loginTask = _authenticationDataAccess.SaveLoginInformation(userID, ipAddress);

            var jwt = GetToken(userID.ToString());

            loginTask.Wait();

            return jwt;
        }

        public async Task ChangePassword(int userID, string password, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _log.LogInformation("[{Class}] Password is null or empty", _class);
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _log.LogInformation("[{Class}] New password is null or empty", _class);
                throw new ArgumentNullException(nameof(newPassword));
            }

            string hashed = HashPassword(password, Encoding.UTF32.GetBytes(_authenticationInfo.Salt));

            if (!await _authenticationDataAccess.VerifyPassword(userID, hashed))
            {
                _log.LogWarning("[{Class}] Invalid password for user '{UserID}'", _class, userID);
                throw new UnauthorizedAccessException("Invalid password");
            }

            string hashedNew = HashPassword(newPassword, Encoding.UTF32.GetBytes(_authenticationInfo.Salt));
            await _authenticationDataAccess.SavePassword(userID, hashedNew);
        }

        public async Task<string> CreateMagicCode(int userID)
        {
            if(userID <= 0)
                throw new ArgumentException("UserID must be greater than 0", nameof(userID));

            var code = Guid.NewGuid().ToString("N");
            int affected = await _authenticationDataAccess.SaveMagicCode(userID, code, DateTime.UtcNow.AddSeconds(60));
            if (affected == 0)
                throw new Exception("Creating magic code failed");
            return code;
        }

        private static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, 10000, 256/8));
        }

        private string DecryptToken(string token)
        {
            _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Starting DecryptToken(..)");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationInfo.Key));
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _authenticationInfo.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _authenticationInfo.Audience
                };

                if (handler.ValidateToken(token, validations, out var tokenSecure).Identity is not ClaimsIdentity identity)
                {
                    throw new Exception("boom - Identity is not valid");
                }
                return identity.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            }
            catch (SecurityTokenExpiredException steeex)
            {
                _log.LogError(steeex, $"[{nameof(AuthenticationInteractor)}] Token expired");
                return string.Empty;
            }
        }

        private string GetToken(string identifier)
        {
            _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Starting GetToken(..)");
            var claims = new[]
            {
                new Claim("sub", identifier)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationInfo.Key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(_authenticationInfo.Issuer, _authenticationInfo.Audience, claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }        
    }
}