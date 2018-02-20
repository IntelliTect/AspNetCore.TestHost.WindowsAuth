using IntelliTect.AspNetCore.TestHost.WindowsAuth.WebExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth.Test
{
    public class WebExampleServerFixture : WindowsAuthServerFixture<Startup>
    {
        protected override string ApplicationName => "IntelliTect.AspNetCore.TestHost.WindowsAuth.WebExample";

        protected override bool AddAuthenticationMiddleware => true;

    }
}
