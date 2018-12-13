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

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.AspNetCore.TestHost.WindowsAuth
{
    public static class WindowsAuthServiceCollectionExtensions
    {
        /// <summary>
        ///     Add services and configuration needed for <see cref="WindowsAuthenticationHandler" />
        ///     to work for integration tests using <see cref="Microsoft.AspNetCore.TestHost.TestServer" />
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