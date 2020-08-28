using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Exemplar_Mining.Authentication
{
    public class OktaOAuthOptions
    {

        public static OktaOAuthOptions _instance { get; protected set; } = new OktaOAuthOptions();

        public static string Domain { get; set; }
        public static string ClientId { get; set; }
        public static string ClientSecret { get; set; }

        public string PostLogoutRedirectUri { get; set; }

        public static Action<OAuthOptions> action { get; set; }

        public OktaOAuthOptions()
        {
        }

        public static Action<OAuthOptions> GetAuthOptions()
        {
            return (options) =>
           {

               options.AuthorizationEndpoint = $"{Domain}/oauth2/default/v1/authorize";

               options.Scope.Add("openid");
               options.Scope.Add("profile");
               options.Scope.Add("email");

               options.CallbackPath = new PathString("/authorization-code/callback");

               options.ClientId = ClientId;
               options.ClientSecret = ClientSecret;
               options.TokenEndpoint = $"{Domain}/oauth2/default/v1/token";

               options.UserInformationEndpoint = $"{Domain}/oauth2/default/v1/userinfo";

               options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
               options.ClaimActions.MapJsonKey(ClaimTypes.Name, "given_name");
               options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

               options.Events = new OAuthEvents
               {
                   OnCreatingTicket = async context =>
                   {

                       var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                       request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                       request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                       var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                       response.EnsureSuccessStatusCode();

                       var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                       context.RunClaimActions(user.RootElement);
                   }
               };
           };
        }

    }
}
