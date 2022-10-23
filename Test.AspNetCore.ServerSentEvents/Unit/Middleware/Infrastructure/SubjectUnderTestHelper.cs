using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Lib.AspNetCore.ServerSentEvents;

namespace Test.AspNetCore.ServerSentEvents.Unit.Middleware.Infrastructure
{
    internal static class SubjectUnderTestHelper
    {
        #region Fields
        private const string ACCEPT_HTTP_HEADER = "Accept";
        private const string SSE_CONTENT_TYPE = "text/event-stream";

        private static readonly RequestDelegate NOOP_REQUEST_DELEGATE = (context) => Task.CompletedTask;
        #endregion

        #region Prepare SUT
        internal static ServerSentEventsMiddleware<ServerSentEventsService> PrepareServerSentEventsMiddleware(
            IAuthorizationPolicyProvider authorizationPolicyProvider = null,
            IServerSentEventsClientIdProvider serverSentEventsClientIdProvider = null,
            IServerSentEventsNoReconnectClientsIdsStore serverSentEventsNoReconnectClientsIdsStore = null,
            ServerSentEventsService serverSentEventsService = null,
            ServerSentEventsOptions options = null)
        {
            return new ServerSentEventsMiddleware<ServerSentEventsService>
            (
                NOOP_REQUEST_DELEGATE,
                authorizationPolicyProvider ?? Mock.Of<IAuthorizationPolicyProvider>(),
                serverSentEventsClientIdProvider ?? new NewGuidServerSentEventsClientIdProvider(),
                serverSentEventsNoReconnectClientsIdsStore ?? new NoOpServerSentEventsNoReconnectClientsIdsStore(),
                serverSentEventsService ?? Mock.Of<TestServerSentEventsService>(),
                Options.Create(options ?? new ServerSentEventsOptions()),
                NullLoggerFactory.Instance
            );
        }

        internal static HttpContext PrepareHttpContext(
            string acceptHeaderValue = SSE_CONTENT_TYPE,
            IHttpRequestLifetimeFeature httpRequestLifetimeFeature = null,
            IAuthenticationService authenticationService = null)
        {
            HttpContext context = new DefaultHttpContext();

            if (acceptHeaderValue != null)
            {
                context.Request.Headers.Append(ACCEPT_HTTP_HEADER, acceptHeaderValue);
            }

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(authenticationService ?? Mock.Of<IAuthenticationService>());

            context.RequestServices = serviceCollection.BuildServiceProvider();

            httpRequestLifetimeFeature = httpRequestLifetimeFeature ?? new HttpRequestLifetimeFeature
            {
                RequestAborted = new CancellationToken(true)
            };
            context.Features.Set(httpRequestLifetimeFeature);

            return context;
        }
        #endregion
    }
}
