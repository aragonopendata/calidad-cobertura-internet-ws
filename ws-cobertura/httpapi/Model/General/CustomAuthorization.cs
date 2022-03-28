using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ws_cobertura.httpapi.Model.Configuration;

namespace ws_cobertura.httpapi.Model.General
{
    

    class CustomAuthorization
    {

    }

    public class CustomAuthorizationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScemeName = "CustomAuth";
        public string TokenHeaderName { get; set; } = "Authorization";
    }

    public class CustomAuthorizationHandler : AuthenticationHandler<CustomAuthorizationOptions>
    {
        APIConfiguration _apiConfiguration;
        public CustomAuthorizationHandler(IOptionsMonitor<CustomAuthorizationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IOptions<APIConfiguration> apiConfiguration)
            : base(options, logger, encoder, clock) {
            this._apiConfiguration = apiConfiguration.Value;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            bool permitirAcceso = false;
            
            string tokenApi = _apiConfiguration.token.ToString();

            //si no hubiera definido un token en appsettings, se permite el acceso sin authentication
            if (string.IsNullOrEmpty(tokenApi))
            {
                permitirAcceso = true;
            }
            else {
                if (!Request.Headers.ContainsKey(Options.TokenHeaderName))
                    return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));

                string tokenRecibido = Request.Headers[Options.TokenHeaderName].ToString();
                tokenRecibido = tokenRecibido.Replace("Bearer ", "");

                // al haber login, por ahora realizamos una autenticacion con token fijo
                if (tokenRecibido == tokenApi)
                {
                    permitirAcceso = true;
                }
            }

            if (permitirAcceso == true) { 
                var claims = new[] {
                    new Claim(ClaimTypes.Role, "user")
                };

                var id = new ClaimsIdentity(claims, Scheme.Name);

                var principal = new ClaimsPrincipal(id);

                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            else {
                return Task.FromResult(AuthenticateResult.Fail("Unauthorized"));
            }
        }
    }
}
