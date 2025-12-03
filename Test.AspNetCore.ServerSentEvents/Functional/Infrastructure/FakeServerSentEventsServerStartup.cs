using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lib.AspNetCore.ServerSentEvents;

namespace Test.AspNetCore.ServerSentEvents.Functional.Infrastructure
{
    internal abstract class FakeServerSentEventsServerStartup
    {
        public const string SERVER_SENT_EVENTS_ENDPOINT = "/sse";

        protected abstract Action<ServerSentEventsServiceOptions<ServerSentEventsService>> ConfigureServerSentEventsOption { get; }

        public IConfiguration Configuration { get; }

        public FakeServerSentEventsServerStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddServerSentEvents(ConfigureServerSentEventsOption);
        }

#if !NET462
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapServerSentEvents(SERVER_SENT_EVENTS_ENDPOINT);
                });
        }
#else
        public void Configure(IApplicationBuilder app)
        {
            app.MapServerSentEvents(SERVER_SENT_EVENTS_ENDPOINT);
        }
#endif
    }
}
