# IntelliTect.AspNetCore.TestHost.WindowsAuth

This project aims to emulate the functionality provided by IIS Integration in an ASP.NET Core project that uses Windows Authentication for the purposes of integration testing with ASP.NET Core's `TestServer` from `Microsoft.AspNetCore.TestHost`.

It provides real, authenticated Windows Auth capabilities - not just a mock of such. The `WindowsIdentity` of the `WindowsPrincipal` that will be signed into your application can use all normal behaviors, including `.Impersonate()`.


## Getting Started

There is currently no NuGet package for this project. Open an issue if this project is of interest to you and we'll look at publishing a NuGet package. Otherwise, you can add this project as a git submodule to try it out.

## Usage

This project is designed to be used with `xunit`, but has no hard dependencies on it. The below example assumes `xunit` usage, but could easily be adapted for other test frameworks.

In your test project:

``` c# 
public class MyProjectServerFixture : WindowsAuthServerFixture<Startup>
{
    // Override members as desired. Likely culprits are:

    protected override string ApplicationName => "MyCompany.MyProject.Web";

    // Path to the web project, relative to the working directory of the running test assembly. 
    // Default value (seen below) assumes test and web project live side-by-side.
    // If projects are nested differently (e.g. separate 'src' and 'test' directories), modify as needed.
    protected override string ContentRoot => $"../../../../{ApplicationName}";
}
```

For more info about overridable members of `WindowsAuthServerFixture`, view this project's source code.



Then, in an `xunit` test:

``` c#
public class MyTests : IClassFixture<MyProjectServerFixture>
{
    private readonly TestServer _server;

    public MyTests(MyProjectServerFixture fixture)
    {
        _server = fixture.Server;
    }

    [Fact]
    public async Task MyTest() 
    {
        HttpClient client;
        // Choose one:
        client = _server.ClientForAnonymous();
        client = _server.ClientForCurrentUser(); // Effectively: UseDefaultCredentials = true
        client = _server.ClientForUser(new NetworkCredential("userName", "password", "DOMAIN"));

        // Make requests against the HttpClient. The requests will be appropriately authenticated when they are handled by your web application, despite not running with IIS.
    }
}

```


## Troubleshooting

* If requests aren't being authenticated, ensure that the standard `AuthenticationMiddleware` is part of your request pipeline. Typically, this middleware is added by calling `app.UseAuthentication()` in the `Configure` method of `Startup.cs`. This middleware isn't strictly required when using IIS integration with Windows auth enabled and anonymous auth disabled, but it *is* required for this library. There is an overridable property, `AddAuthenticationMiddleware`, on `WindowsAuthServerFixture` that if overridden to `true` will add the middleware on your behalf.
* This library sets its authentication handler as the default authentication scheme, overriding any other scheme configured in your `Startup.cs`. If this is not desired, check out `WindowsAuthServiceCollectionExtensions.cs` for how to add in the required services yourself in a different manner.