using IntelliTect.AspNetCore.TestHost.WindowsAuth;
using IntelliTect.AspNetCore.TestHost.WindowsAuth.Test;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace TestHost.WindowsAuth.Test
{
    public class BasicTests : IClassFixture<WebExampleServerFixture>
    {
        private readonly TestServer server;

        public BasicTests(WebExampleServerFixture fixture)
        {
            server = fixture.Server;
        }

        [Fact]
        public async Task AnonymousRequest_SucceedsForAnonymousEndpoint()
        {
            var client = server.ClientForAnonymous();
            var result = await client.GetAsync("/api/values/anonymous");
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal("success", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task AnonymousRequest_FailsForAuthenticatedEndpoint()
        {
            var client = server.ClientForAnonymous();
            var result = await client.GetAsync("/api/values/whoami");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task CurrentUserRequest_SucceedsForAuthenticatedEndpoint()
        {
            var client = server.ClientForCurrentUser();
            var result = await client.GetAsync("/api/values/whoami");
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal(WindowsIdentity.GetCurrent().Name, await result.Content.ReadAsStringAsync());
        }

        [Fact(Skip = "Ad-hoc test for authenticating with specific username/password. Requires real credentials.")]
        public async Task SpecificUserRequest_SucceedsForAuthenticatedEndpoint()
        {
            string userName = "USERNAME";
            string password = "PASSWORD";
            string domain = "DOMAIN";

            var client = server.ClientForUser(new NetworkCredential(userName, password, domain));
            var result = await client.GetAsync("/api/values/whoami");
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal($"{domain}\\{userName}", await result.Content.ReadAsStringAsync());
        }
    }
}
