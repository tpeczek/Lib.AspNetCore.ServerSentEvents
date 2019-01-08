# Authorization

It's possible to provide authorization requirements for a Server-Sent Events endpoint.

## Simple authorization

Authorization for a Server-Sent Events endpoint is controlled through the `ServerSentEventsOptions.Authorization` and its various properties. At its simplest, setting the `ServerSentEventsOptions.Authorization` to `ServerSentEventsAuthorization.Default` limits access to any authenticated user.

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
		...

        services.AddServerSentEvents();

		...
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        ...

        app.MapServerSentEvents("/default-sse-endpoint", new ServerSentEventsOptions
        {
            Authorization = ServerSentEventsAuthorization.Default
        });
			
		...
    }
}
```

## Adding role checks

To limit access to users who are a member of a specific role, it's enough to set `ServerSentEventsOptions.Authorization.Roles`.

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
		...

        services.AddServerSentEvents();

		...
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        ...

        app.MapServerSentEvents("/default-sse-endpoint", new ServerSentEventsOptions
        {
            Authorization = new ServerSentEventsAuthorization { Roles = "Employee" }
        });
			
		...
    }
}
```

Multiple roles can be specified as a comma separated list.

## Adding claims checks

To limit access to users who possess specific claims (optionally with specific values), the developer must build and register a policy expressing the claims requirements. Then the policy can be applied by using the `Policy` property on the `ServerSentEventsOptions.Authorization` to specify the policy name.

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
		...

        services.AddServerSentEvents();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
        });

		...
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
        ...

        app.MapServerSentEvents("/default-sse-endpoint", new ServerSentEventsOptions
        {
            Authorization = new ServerSentEventsAuthorization { Policy = "EmployeeOnly" }
        });
			
		...
    }
}
```

## Adding custom requirements

If needed, the full power of policy-based authorization can be utilized by implementing `IAuthorizationRequirement` and `AuthorizationHandler<TRequirement>` or `IAuthorizationHandler`, building and registering a policy, and appling it by using the `Policy` property on the `ServerSentEventsOptions.Authorization`. More information can be found [here](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies).