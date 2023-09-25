## Lib.AspNetCore.ServerSentEvents 8.3.0
### Additions and Changes
- Added support for scenario when multiple types are provided as `Accept` header value (thanks to @krebil)
- Added support for wildcard (`*/*` and `text/*`) mime types (thanks to @krebil)

## Lib.AspNetCore.ServerSentEvents 8.2.0
### Additions and Changes
- Added options for controlling the format of [keepalives](https://tpeczek.github.io/Lib.AspNetCore.ServerSentEvents/articles/getting-started.html#keepalives).
- Added options for controlling the [`Accept` request header validation](https://tpeczek.github.io/Lib.AspNetCore.ServerSentEvents/articles/getting-started.html#accept-request-header-validation).

## Lib.AspNetCore.ServerSentEvents 8.1.0
### Additions and Changes
- Removed upper bounds on some of the .NET Framework dependencies versions

## Lib.AspNetCore.ServerSentEvents 8.0.0
### Additions and Changes
- Dropped support for ASP.NET Core 2.1.0 (the lowest supported version is now ASP.NET Core 3.1.0)
- Added support for ASP.NET Core 6

## Lib.AspNetCore.ServerSentEvents 7.0.0
### Additions and Changes
- Added support for disconnecting clients. You can read more here: [Server-Sent Events and ASP.NET Core - Disconnecting a Client](https://www.tpeczek.com/2021/11/server-sent-events-and-aspnet-core.html)
### Bug Fixes
- Fixed race condition in groups management. This has caused method signature change (`IServerSentEventsService.AddToGroupAsync` to `IServerSentEventsService.AddToGroup`)

## Lib.AspNetCore.ServerSentEvents 6.0.0
### Additions and Changes
- Changed support for ASP.NET Core 3.0.0 to ASP.NET Core 3.1.0
- Added support for ASP.NET Core 5

## Lib.AspNetCore.ServerSentEvents 5.1.0
### Additions and Changes
- Exposed way to provide ReconnectInterval through ServerSentEventsServiceOptions.

## Lib.AspNetCore.ServerSentEvents 5.0.0
### Additions and Changes
- Changed the collection of key/value pairs to store additional information for clients to thread safe methods (thank you @sebastiano1972)
### Bug Fixes
- Fixed Accept request header validation so request without it are processed.
- Fixed ClientConnected and ClientDisconnected events not firing for custom ServerSentEventsService implementations.

## Lib.AspNetCore.ServerSentEvents 4.1.0
### Additions and Changes
- Added a collection of key/value pairs to store additional information for clients (thank you @sebastiano1972)

## Lib.AspNetCore.ServerSentEvents 4.0.0
### Additions and Changes
- Dropped support for obsolete APIs
- Dropped support for ASP.NET Core 2.0.0 (the lowest supported version is now ASP.NET Core 2.1.0)
- Added support for ASP.NET Core 3.0.0
- Added support for Endpoint Routing

## Lib.AspNetCore.ServerSentEvents 3.3.0
### Additions and Changes
- Added method for getting clients in a specified group.
- Added SendEventAsync overload which takes a predicate against IServerSentEventsClient as a parameter.

## Lib.AspNetCore.ServerSentEvents 3.2.0
### Additions and Changes
- Exposed way to provide actions for ClientConnected and ClientDisconnected events through ServerSentEventsServiceOptions.
- Added method for adding to group.
- Added methods for sending to group.

## Lib.AspNetCore.ServerSentEvents 3.1.0
### Additions and Changes
- Added support for authorization.
- Added support for setting HTTP response headers for accept response.

## Lib.AspNetCore.ServerSentEvents 3.0.0
### Additions and Changes
- Added support for client connected and disconnected events.
- Improved support for CancellationToken in asynchronous operations.
- Hot synchronous path performance improvements.

## Lib.AspNetCore.ServerSentEvents 2.0.0
### Additions and Changes
- Upgraded to .NET Standard 2.0 and ASP.NET Core 2.0.
- Added generic versions of UseServerSentEvents and MapServerSentEvents.
- Marked obsolete versions of UseServerSentEvents and MapServerSentEvents which take instance of ServerSentEventsService as parameter.
- Added support for keepalives

## Lib.AspNetCore.ServerSentEvents 1.3.0
### Additions and Changes
- General performance improvements.

## Lib.AspNetCore.ServerSentEvents 1.2.0
### Additions and Changes
- SendEventAsync performance improvements.
- ChangeReconnectIntervalAsync performance improvements.

## Lib.AspNetCore.ServerSentEvents 1.1.1
### Bug Fixes
- Fix for IIS (and potentially other reverse proxies) adding uncontrolled response compression.
### Additions and Changes
- Generic performance improvements.

## Lib.AspNetCore.ServerSentEvents 1.1.0
### Bug Fixes
- Fix for events not reaching clients when response compression is enabled (.NET Framework).
### Additions and Changes
- Added capability of sending events to specific clients.