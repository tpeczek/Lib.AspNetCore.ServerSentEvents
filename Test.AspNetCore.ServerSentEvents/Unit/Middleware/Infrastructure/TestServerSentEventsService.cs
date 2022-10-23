using Lib.AspNetCore.ServerSentEvents;
using Microsoft.Extensions.Options;

namespace Test.AspNetCore.ServerSentEvents.Unit.Middleware.Infrastructure
{
    internal class TestServerSentEventsService : ServerSentEventsService
    {
        public TestServerSentEventsService()
            : base(Options.Create<ServerSentEventsServiceOptions<ServerSentEventsService>>(null))
        { }
    }
}
