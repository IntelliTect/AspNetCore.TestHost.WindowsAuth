using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.AspNetCore.IntegrationTesting.WindowsAuth
{
    public static class WindowsAuthServiceCollectionExtensions
    {
        /// <summary>
        /// Add services and configuration needed for <see cref="WindowsAuthenticationHandler"/> 
        /// to work for integration tests using <see cref="Microsoft.AspNetCore.TestHost.TestServer"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWindowsAuthenticationForTesting(this IServiceCollection services)
        {
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                const string scheme = WindowsAuthenticationHandler.AuthenticationScheme;

                // Set the default auth scheme to our windows auth scheme.
                // Normally, this would be done in the call to .AddAuthentication() in Startup.cs,
                // but because we don't want to require modifications to web projects to be aware of integration tests,
                // we can instead modify the options after the fact to use our scheme.
                options.DefaultScheme = scheme;
                options.DefaultChallengeScheme = scheme;
                options.AddScheme(scheme, s =>
                {
                    s.HandlerType = typeof(WindowsAuthenticationHandler);
                    s.DisplayName = scheme;
                });
            });
            services.AddTransient<WindowsAuthenticationHandler>();

            return services;
        }
    }
}
