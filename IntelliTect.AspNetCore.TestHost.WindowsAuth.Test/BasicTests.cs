using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth.Test
{
    public class BasicTests : IClassFixture<WebExampleServerFixture>
    {
        public BasicTests(WebExampleServerFixture fixture)
        {
            _server = fixture.Server;
        }

        private readonly TestServer _server;

        [Fact]
        public async Task AnonymousRequest_FailsForAuthenticatedEndpoint()
        {
            HttpClient client = _server.ClientForAnonymous();
            HttpResponseMessage result = await client.GetAsync("/api/values/whoami");
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task AnonymousRequest_SucceedsForAnonymousEndpoint()
        {
            HttpClient client = _server.ClientForAnonymous();
            HttpResponseMessage result = await client.GetAsync("/api/values/anonymous");
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal("success", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CurrentUserRequest_SucceedsForAuthenticatedEndpoint()
        {
            HttpClient client = _server.ClientForCurrentUser();
            HttpResponseMessage result = await client.GetAsync("/api/values/whoami");
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal(WindowsIdentity.GetCurrent().Name, await result.Content.ReadAsStringAsync());
        }

        [Fact(Skip = "Ad-hoc test for authenticating with specific username/password. Requires real credentials.")]
        public async Task SpecificUserRequest_SucceedsForAuthenticatedEndpoint()
        {
            var userName = "USERNAME";
            var password = "PASSWORD";
            var domain = "DOMAIN";

            HttpClient client = _server.ClientForUser(new NetworkCredential(userName, password, domain));
            HttpResponseMessage result = await client.GetAsync("/api/values/whoami");
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal($"{domain}\\{userName}", await result.Content.ReadAsStringAsync());
        }
    }
}