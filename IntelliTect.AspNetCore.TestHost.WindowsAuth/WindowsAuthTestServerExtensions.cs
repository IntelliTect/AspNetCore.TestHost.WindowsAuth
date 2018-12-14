// Copyright 2018 IntelliTect
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using Microsoft.AspNetCore.TestHost;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth
{
    public static class WindowsAuthTestServerExtensions
    {
        /// <summary>
        ///     Get an <see cref="HttpClient" /> for a <see cref="TestServer" /> that will make requests under the specified user.
        /// </summary>
        /// <param name="server">The <see cref="TestServer" /> to create a client for.</param>
        /// <param name="credential">The credentials of the user that will be logged in.</param>
        /// <returns>An HttpClient for which any requests made through it will use the specified user.</returns>
        public static HttpClient ClientForUser(this TestServer server, NetworkCredential credential)
        {
            return server.CreateClient().SetWindowsAuthHeader(credential);
        }

        /// <summary>
        ///     Get an <see cref="HttpClient" /> for a <see cref="TestServer" /> that will make requests under the current user.
        /// </summary>
        /// <param name="server">The <see cref="TestServer" /> to create a client for.</param>
        /// <returns>An HttpClient for which any requests made through it will use the specified user.</returns>
        public static HttpClient ClientForCurrentUser(this TestServer server)
        {
            using (WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent())
            {
                string[] nameParts = currentIdentity.Name.Split('\\');
                return server
                    .CreateClient()
                    .SetWindowsAuthHeader(new NetworkCredential(
                        nameParts[1],
                        null as string,
                        nameParts[0]
                    ));
            }
        }

        /// <summary>
        ///     Get an <see cref="HttpClient" /> for a <see cref="TestServer" /> that will make unauthenticated requests.
        /// </summary>
        /// <param name="server">The <see cref="TestServer" /> to create a client for.</param>
        /// <returns>An <see cref="HttpClient" /> that will pass no authentication information.</returns>
        public static HttpClient ClientForAnonymous(this TestServer server)
        {
            return server.CreateClient();
        }

        /// <summary>
        ///     Set the <see cref="HttpClient" /> to make requests using the specified user.
        /// </summary>
        /// <param name="client">The client that should start making user-faked requests.</param>
        /// <param name="credentials">The credentials of the user that will be logged in.</param>
        /// <returns>The <see cref="HttpClient" /> that was passed in.</returns>
        public static HttpClient SetWindowsAuthHeader(this HttpClient client, NetworkCredential credentials)
        {
            Guid guid = WindowsIdentityFactory.GetTokenForCredentials(credentials);

            // Set a request header that our authentication handler will use to create a ClaimsPrincipal.
            client.DefaultRequestHeaders.Add(WindowsAuthenticationHandler.CredentialKeyHeader, guid.ToString());

            return client;
        }
    }
}