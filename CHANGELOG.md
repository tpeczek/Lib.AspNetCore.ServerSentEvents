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