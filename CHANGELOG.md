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