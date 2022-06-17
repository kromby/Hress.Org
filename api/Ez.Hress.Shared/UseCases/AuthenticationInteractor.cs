using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ez.Hress.Shared.UseCases
{
    public class AuthenticationInteractor
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly ILogger<AuthenticationInteractor> _log;
        

        public AuthenticationInteractor(AuthenticationInfo authenticationInfo, ILogger<AuthenticationInteractor> log)
        {
            if (authenticationInfo == null)
                throw new ArgumentNullException(nameof(authenticationInfo));

            authenticationInfo.Validate();
            
            _key = authenticationInfo.Key;
            _issuer = authenticationInfo.Issuer;
            _audience = authenticationInfo.Audience;

            _log = log;
        }

        private string DecryptToken(string token)
        {
            _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Starting DecryptToken(..)");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var validations = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience
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

        public string GetToken(string identifier)
        {
            _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Starting GetToken(..)");
            var claims = new[]
            {
                new Claim("sub", identifier)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(_issuer, _audience, claims, expires: DateTime.Now.AddMinutes(240), signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
    }
}