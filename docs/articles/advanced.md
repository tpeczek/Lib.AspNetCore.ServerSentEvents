# Advanced

## Set HTTP response headers for accept response

A `ServerSentEventsOptions` object can be used to set HTTP response headers for accept response.

### HTTP response headers for Nginx proxy configuration

The following code sets HTTP response headers for Nginx proxy configuration.

```cs
public class Startup
{
    ...

    public void Configure(IApplicationBuilder app)
    {
        ...
			
		app.MapServerSentEvents("/default-sse-endpoint", new ServerSentEventsOptions
        {
            OnPrepareAccept = response =>
            {
                response.Headers.Append("Cache-Control", "no-cache");
                response.Headers.Append("X-Accel-Buffering", "no");
            }
        });
			
		...
    }
}
```
