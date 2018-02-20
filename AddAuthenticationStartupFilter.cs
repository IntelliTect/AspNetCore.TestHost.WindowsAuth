using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace IntelliTect.AspNetCore.IntegrationTesting.WindowsAuth
{
    /// <summary>
    /// ASP.NET Core IStartupFilter for ensuring that ASP.NET Core Authentication is added to the request pipeline
    /// of the application when running in an integration test project.
    /// </summary>
    internal class AddAuthenticationStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseAuthentication();
                next(app);
            };
        }
    }
}