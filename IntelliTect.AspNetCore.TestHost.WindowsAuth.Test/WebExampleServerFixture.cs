using IntelliTect.AspNetCore.TestHost.WindowsAuth.WebExample;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth.Test
{
    public class WebExampleServerFixture : WindowsAuthServerFixture<Startup>
    {
        protected override string ApplicationName => "IntelliTect.AspNetCore.TestHost.WindowsAuth.WebExample";

        protected override bool AddAuthenticationMiddleware => true;
    }
}