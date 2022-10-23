using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Test.AspNetCore.ServerSentEvents.Functional.Infrastructure
{
    internal class FakeServerSentEventsServerrApplicationFactory<TFakeServerSentEventsServerStartup> : WebApplicationFactory<TFakeServerSentEventsServerStartup> where TFakeServerSentEventsServerStartup : FakeServerSentEventsServerStartup
    {
#if !NET461
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseStartup<TFakeServerSentEventsServerStartup>();
        }
#else
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder()
                .UseStartup<TFakeServerSentEventsServerStartup>();
        }
#endif

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(Path.GetTempPath());
        }
    }
}
