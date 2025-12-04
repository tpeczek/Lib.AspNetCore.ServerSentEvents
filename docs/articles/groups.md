# Groups

A group is a collection of clients associated with a name. Groups are the recommended way to send to multiple clients, because the groups are managed by the library. A client can be a member of multiple groups.

## Adding to a group

Client can be added to a group via the `IServerSentEventsService.AddToGroup` method (the method will return an information if a client has been added to an existing or a new group).

```cs
_serverSentEventsService.AddToGroup(groupName, client);
```

Group membership isn't preserved when a client reconnects. The client needs to rejoin the group when connection is re-established. One of possible ways to handle this is putting group assigment logic into `ServerSentEventsServiceOptions.OnClientConnected` callback.

```cs
public class Startup
{
    ...

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddServerSentEvents(options =>
        {
            options.OnClientConnected = (service, clientConnectedArgs) =>
            {
                // Logic which determines the client group.
                ...

                service.AddToGroup(groupName, clientConnectedArgs.Client);
            };
        });

        ...
    }

    ...
}
```

Clients within a group can be retrieved via the `IServerSentEventsService.GetClients` method

```cs
IReadOnlyCollection<IServerSentEventsClient> clientsInGroup = _serverSentEventsService.GetClients(groupName);
```

## Sending to a group

Messages can be sent to all clients in a group via the `IServerSentEventsService.SendEventAsync` method.

```cs
await _serverSentEventsService.SendEventAsync(groupName, messageText);
```

If there is a need to sent a message to only some clients in a group, a predicate can be passed to the `IServerSentEventsService.SendEventAsync`.

```cs
await _serverSentEventsService.SendEventAsync(groupName, messageText, client =>
{
    // Logic which determines if message should be send to this client.
    ...

    return shouldSentMessage;
});
```