# Getting Started

## Configuration

In order to add the Server-Sent Events (SSE) support to an application a required service and middleware must be registered. The library provides [extensions](../api/Lib.AspNetCore.ServerSentEvents.ServerSentEventsMiddlewareExtensions.html) which make it really simple.

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...

		services.AddServerSentEvents();

		...
    }

    public void Configure(IApplicationBuilder app)
    {
        ...
			
		app.MapServerSentEvents("/default-sse-endpoint");
			
		...
    }
}
```

The `AddServerSendEvents()` method registers default service which provides operations over Server-Sent Events protocol, while `MapServerSentEvents()` maps this service to a specific endpoint. The `MapServerSentEvents()` method can be called multiple times for different endpoints but all of them will be mapped to same service which will make them indistinguishable from application perspective. In order to create distinguishable endpoints a delivered version of [`Server​Sent​Events​Service`](../api/Lib.AspNetCore.ServerSentEvents.ServerSentEventsService.html) should be registered (it doesn't need to override any of the default service functionality). Below is an example of such configuration.

```cs
internal interface INotificationsServerSentEventsService : IServerSentEventsService
{ }

internal class NotificationsServerSentEventsService : ServerSentEventsService, INotificationsServerSentEventsService
{ }

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...

		services.AddServerSentEvents();
		services.AddServerSentEvents<INotificationsServerSentEventsService, NotificationsServerSentEventsService>();

		...
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
    {
        ...
			
		app.MapServerSentEvents("/default-sse-endpoint");
		app.MapServerSentEvents<NotificationsServerSentEventsService>("/notifications-sse-endpoint");
			
		...
    }
}
```

## Sending Messages

In order to send a message to an endpoint the instance of the service registered for that endpoint should be obtained. The easiest way is to rely on dependency injection.

```cs
public class NotificationsController : Controller
{
    private readonly INotificationsServerSentEventsService _serverSentEventsService;

    public NotificationsController(INotificationsServerSentEventsService serverSentEventsService)
    {
        _serverSentEventsService = serverSentEventsService;
    }

	...
}
```

### Sending to all clients

Sending message to all clients is as simple as calling `IServerSentEventsService.SendEventAsync`. Under the hood the service will take care of distributing the message to all connected clients.

### Sending to specific client

The `IServerSentEventsService.GetClients()` method returns list of all currently connected clients. Every client (represented by [`IServerSentEventsClient`](../api/Lib.AspNetCore.ServerSentEvents.IServerSentEventsClient.html)) exposes `User` property and `SendEventAsync` method. The `User` property contains a `System.Security.Claims.ClaimsPrincipal` which can be used to filter a specific client. Additionally there is an `Id` property available which value can be stored and later passed to `IServerSentEventsService.GetClient()` in order to retrieve a specific client.

## Handling Auto Reconnect

Server-Sent Events provide auto reconnect and tracking of the last seen message functionality. If the connection is dropped, client will automatically reconnect to the server and optionally advertise the identifier of the last seen message. This allows for retransmission of lost messages.

In order to use this mechanism one can subscribe to `IServerSentEventsService.ClientConnected` event. When client reconnects the event will be raised and `ServerSentEventsClientConnectedArgs.LastEventId` will provide an identifier of last message which client has received. At this point `ServerSentEventsClientConnectedArgs.Client` can be used to send any missed messages.

Alternative approach is to override `OnReconnectAsync` method when delivering from `Server​Sent​Events​Service`. When client reconnects the method will be called with `IServerSentEventsClient` representing the client and an identifier of last message which client has received. When overriding `OnReconnectAsync` the base implementation should be called as it's responsible for triggering `ClientConnected` event.

### Changing Reconnect Interval

The interval after which client attempts to reconnect can be controlled by the application. In order to change the interval for specific endpoint it is enough to call `IServerSentEventsService.ChangeReconnectIntervalAsync`.

## Keepalives

Keepalives are supported in three [modes](../api/Lib.AspNetCore.ServerSentEvents.ServerSentEventsKeepaliveMode.html). By default the will be automatically send if ANCM is detected, but both the mode and interval can be changed per `ServerSentEventsService` type.

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        ...

		services.AddServerSentEvents<INotificationsServerSentEventsService, NotificationsServerSentEventsService>(options =>
        {
            options.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;
            options.KeepaliveInterval = 15;
        });

		...
    }

    ...
}
```