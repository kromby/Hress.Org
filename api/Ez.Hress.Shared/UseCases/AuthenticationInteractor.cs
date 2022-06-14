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

        public string GetSubject(string token)
        {
            _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Starting GetSubject(..)");
            return DecryptToken(token);
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
            catch (SecurityTokenExpiredException)
            {
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
            _log.LogInformation($"[{nameof(AuthenticationInteractor)}] Starting GetUserIdFromToken(..)");
            
            if (string.IsNullOrWhiteSpace(scheme) || string.IsNullOrWhiteSpace(value))
                return new Tuple<bool, int>(false, -1);

            if (scheme != "token")
                return new Tuple<bool, int>(false, -1);

            try
            {
                var userIdString = GetSubject(value);
                if (string.IsNullOrWhiteSpace(userIdString))
                    return new Tuple<bool, int>(false, -1);

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