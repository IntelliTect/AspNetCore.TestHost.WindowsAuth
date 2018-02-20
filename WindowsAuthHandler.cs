using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IntelliTect.AspNetCore.IntegrationTesting.WindowsAuth
{
    /// <summary>
    /// ASP.NET Core Authentication handler for authenticating as arbitrary users for integration tests.
    /// Provides behavior similar to the ASP.NET Core IIS integration AuthenticationHandler.
    /// </summary>
    /// <remarks>
    /// This is inspired largely by 
    /// https://github.com/aspnet/IISIntegration/blob/release/2.0/src/Microsoft.AspNetCore.Server.IISIntegration/AuthenticationHandler.cs
    /// It aims to be as close to the IIS handler as possible.
    /// </remarks>
    internal class WindowsAuthenticationHandler : IAuthenticationHandler
    {
        public const string CredentialKeyHeader = "X-IntegrationTest-WindowsCredentialKey";
        public const string AuthenticationScheme = "IntegrationTestWindowsAuth";

        private AuthenticationScheme _scheme;
        private HttpContext _context;

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            if (_context.Request.Headers.Keys.Contains(CredentialKeyHeader))
            {
                var credentialGuidString = _context.Request.Headers[CredentialKeyHeader].First();
                var credentialKey = Guid.Parse(credentialGuidString);
                var identity = WindowsIdentityFactory.LogInAs(credentialKey);

                var user = new WindowsPrincipal(identity);

                // Ensure that the identity is disposed of when the request is done
                // to avoid leaking unmanaged handles.
                _context.Response.RegisterForDispose(identity);
                
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, AuthenticationScheme)));
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            _context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            _context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            // TODO: Since scheme isn't being used to create the AuthenticationTicket, do we need to keep it around?
            _scheme = scheme;
            _context = context;
            return Task.CompletedTask;
        }
    }
}