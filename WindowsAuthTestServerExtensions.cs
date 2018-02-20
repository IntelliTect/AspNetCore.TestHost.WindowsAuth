using Microsoft.AspNetCore.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;

namespace IntelliTect.AspNetCore.IntegrationTesting.WindowsAuth
{
    public static class WindowsAuthTestServerExtensions
    {
        /// <summary>
        /// Get an <see cref="HttpClient"/> for a <see cref="TestServer"/> that will make requests under the specified user.
        /// </summary>
        /// <param name="server">The <see cref="TestServer"/> to create a client for.</param>
        /// <param name="credential">The credentials of the user that will be logged in.</param>
        /// <returns>An HttpClient for which any requests made through it will use the specified user.</returns>
        public static HttpClient ClientForUser(this TestServer server, NetworkCredential credential)
            => server.CreateClient().SetWindowsAuthHeader(credential);

        /// <summary>
        /// Get an <see cref="HttpClient"/> for a <see cref="TestServer"/> that will make requests under the current user.
        /// </summary>
        /// <param name="server">The <see cref="TestServer"/> to create a client for.</param>
        /// <returns>An HttpClient for which any requests made through it will use the specified user.</returns>
        public static HttpClient ClientForCurrentUser(this TestServer server)
        {
            using (var currentIdentity = WindowsIdentity.GetCurrent())
            {
                var nameParts = currentIdentity.Name.Split('\\');
                return server
                    .CreateClient()
                    .SetWindowsAuthHeader(new NetworkCredential(
                        userName: nameParts[1],
                        password: null as string,
                        domain: nameParts[0]
                    ));
            }
        }

        /// <summary>
        /// Get an <see cref="HttpClient"/> for a <see cref="TestServer"/> that will make unauthenticated requests.
        /// </summary>
        /// <param name="server">The <see cref="TestServer"/> to create a client for.</param>
        /// <returns>An <see cref="HttpClient"/> that will pass no authentication information.</returns>
        public static HttpClient ClientForAnonymous(this TestServer server)
            => server.CreateClient();

        /// <summary>
        /// Set the <see cref="HttpClient"/> to make requests using the specified user.
        /// </summary>
        /// <param name="client">The client that should start making user-faked requests.</param>
        /// <param name="credential">The credentials of the user that will be logged in.</param>
        /// <returns>The <see cref="HttpClient"/> that was passed in.</returns>
        public static HttpClient SetWindowsAuthHeader(this HttpClient client, NetworkCredential credentials)
        {
            var guid = WindowsIdentityFactory.GetTokenForCredentials(credentials);

            // Set a request header that our authentication handler will use to create a ClaimsPrincipal.
            client.DefaultRequestHeaders.Add(WindowsAuthenticationHandler.CredentialKeyHeader, guid.ToString());

            return client;
        }
    }
}
