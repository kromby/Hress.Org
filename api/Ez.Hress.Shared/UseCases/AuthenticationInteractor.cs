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
                _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Scheme or token value is missing. Scheme: '{scheme}'");
                return new Tuple<bool, int>(false, -1);
            }

            if (scheme.Trim().ToLower() != "token")
            {
                _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Scheme is not token. Scheme: '{scheme}'");
                return new Tuple<bool, int>(false, -1);
            }

            try
            {
                var userIdString = DecryptToken(value);
                if (string.IsNullOrWhiteSpace(userIdString))
                {
                    _log.LogInformation($"[{nameof(AuthenticationInteractor)}] UserId is missing. userIdString: '{userIdString}'");
                    return new Tuple<bool, int>(false, -1);
                }

                return new Tuple<bool, int>(true, int.Parse(userIdString, CultureInfo.CurrentCulture));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, $"[{nameof(AuthenticationInteractor)}] Error in GetUserIdFromToken(..)");
                return new Tuple<bool, int>(false, -1);
            }
        }

        public async Task<string> Login(string username, string password, string ipAddress)
        {
            if(string.IsNullOrWhiteSpace(username))
            {
                _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Username is null or empty");
                throw new ArgumentNullException(nameof(username));
            }

            if(string.IsNullOrWhiteSpace(password))
            {
                _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Password is null or empty");
                throw new ArgumentNullException(nameof(password));
            }

            string hashed = HashPassword(password, Encoding.UTF32.GetBytes(_authenticationInfo.Salt));

            var userID = await _authenticationDataAccess.GetUserID(username, hashed);
            
            if(userID <= 0)
            {
                _log.LogWarning($"[{nameof(AuthenticationInteractor)}] User '{username}' not authenticated.");
                throw new UnauthorizedAccessException();
            }

            var loginTask = _authenticationDataAccess.SaveLoginInformation(userID, ipAddress);

            var jwt = GetToken(userID.ToString());

            loginTask.Wait();

            return jwt;
        }

        private string HashPassword(string password, byte[] salt)
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

                if (!(handler.ValidateToken(token, validations, out var tokenSecure).Identity is ClaimsIdentity identity))
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

            var token = new JwtSecurityToken(_authenticationInfo.Issuer, _authenticationInfo.Audience, claims, expires: DateTime.Now.AddMinutes(240), signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }        
    }
}