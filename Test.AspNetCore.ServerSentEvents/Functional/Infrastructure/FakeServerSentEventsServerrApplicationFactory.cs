using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Test.AspNetCore.ServerSentEvents.Functional.Infrastructure
{
    internal class FakeServerSentEventsServerrApplicationFactory<TFakeServerSentEventsServerStartup> : WebApplicationFactory<TFakeServerSentEventsServerStartup> where TFakeServerSentEventsServerStartup : FakeServerSentEventsServerStartup
    {
#if !NET462
        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TFakeServerSentEventsServerStartup>();
                });
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
