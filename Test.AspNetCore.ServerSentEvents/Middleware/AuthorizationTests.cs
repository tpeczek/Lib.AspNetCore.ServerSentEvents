using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;

namespace Test.AspNetCore.ServerSentEvents.Middleware
{
    public class AuthorizationTests
    {
        #region Fields
        private const string ACCEPT_HTTP_HEADER = "Accept";
        private const string SSE_CONTENT_TYPE = "text/event-stream";

        private static readonly RequestDelegate NOOP_REQUEST_DELEGATE = (context) => Task.CompletedTask;
        private static readonly AuthorizationPolicy DEFAULT_AUTHORIZATION_POLICY = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        private static readonly AuthenticateResult DEFAULT_AUTHENTICATE_RESULT = AuthenticateResult.NoResult();
        private static readonly PolicyAuthorizationResult DEFAULT_POLICY_AUTHORIZATION_RESULT = PolicyAuthorizationResult.Success();
        #endregion

        #region Prepare SUT
        private IAuthorizationPolicyProvider PrepareAuthorizationPolicyProvider()
        {
            Mock<IAuthorizationPolicyProvider> policyProviderMock = new Mock<IAuthorizationPolicyProvider>();
            policyProviderMock.Setup(m => m.GetDefaultPolicyAsync()).ReturnsAsync(DEFAULT_AUTHORIZATION_POLICY);

            return policyProviderMock.Object;
        }

        private ServerSentEventsService PrepareServerSentEventsService()
        {
            Mock<ServerSentEventsService> serverSentEventsServiceMock = new Mock<ServerSentEventsService>()
            {
                CallBase = false
            };

            return serverSentEventsServiceMock.Object;
        }

        private ServerSentEventsMiddleware<ServerSentEventsService> PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization authorization = null)
        {
            return new ServerSentEventsMiddleware<ServerSentEventsService>
            (
                NOOP_REQUEST_DELEGATE,
                PrepareAuthorizationPolicyProvider(),
                PrepareServerSentEventsService(),
                Options.Create(new ServerSentEventsOptions { Authorization = authorization })
            );
        }

        private HttpContext PrepareHttpContext(Mock<IAuthenticationService> authenticationServiceMock = null)
        {
            HttpContext context = new DefaultHttpContext();

            context.Request.Headers.Append(ACCEPT_HTTP_HEADER, SSE_CONTENT_TYPE);
            context.RequestAborted = new CancellationToken(true);

            authenticationServiceMock = authenticationServiceMock ?? new Mock<IAuthenticationService>();

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(authenticationServiceMock.Object);

            context.RequestServices = serviceCollection.BuildServiceProvider();

            return context;
        }

        private Mock<IPolicyEvaluator> PreparePolicyEvaluatorMock(HttpContext context, AuthenticateResult authenticateResult = null, PolicyAuthorizationResult policyAuthorizationResult = null)
        {
            authenticateResult = authenticateResult ?? DEFAULT_AUTHENTICATE_RESULT;
            policyAuthorizationResult = policyAuthorizationResult ?? DEFAULT_POLICY_AUTHORIZATION_RESULT;

            Mock<IPolicyEvaluator> policyEvaluatorMock = new Mock<IPolicyEvaluator>();
            policyEvaluatorMock.Setup(m => m.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), context)).ReturnsAsync(authenticateResult);
            policyEvaluatorMock.Setup(m => m.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), authenticateResult, context, null)).ReturnsAsync(policyAuthorizationResult);

            return policyEvaluatorMock;
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_SseRequest_NoAuthorization_DoesNotCallAuthenticateAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware();
            HttpContext context = PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), context), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_NoAuthorization_DoesNotCallAuthorizeAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware();
            HttpContext context = PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), DEFAULT_AUTHENTICATE_RESULT, context, null), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_CallsAuthenticateAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization.Default);
            HttpContext context = PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), context), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_CallsAuthorizeAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization.Default);
            HttpContext context = PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), DEFAULT_AUTHENTICATE_RESULT, context, null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultSuccess_DoesNotCallChallengeAsync()
        {
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();

            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization.Default);
            HttpContext context = PrepareHttpContext(authenticationServiceMock);
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Success());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ChallengeAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultSuccess_DoesNotCallForbidAsync()
        {
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();

            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization.Default);
            HttpContext context = PrepareHttpContext(authenticationServiceMock);
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Success());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ForbidAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultChallenge_CallsChallengeAsync()
        {
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();

            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization.Default);
            HttpContext context = PrepareHttpContext(authenticationServiceMock);
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Challenge());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ChallengeAsync(context, null, null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_AuthorizationWithSchemes_PolicyAuthorizationResultChallenge_CallsChallengeAsyncForEveryScheme()
        {
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();

            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(new ServerSentEventsAuthorization { AuthenticationSchemes = "schema1,schema2" });
            HttpContext context = PrepareHttpContext(authenticationServiceMock);
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Challenge());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ChallengeAsync(context, "schema1", null), Times.Once);
            authenticationServiceMock.Verify(m => m.ChallengeAsync(context, "schema2", null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultForbid_CallsForbidAsync()
        {
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();

            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(ServerSentEventsAuthorization.Default);
            HttpContext context = PrepareHttpContext(authenticationServiceMock);
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Forbid());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ForbidAsync(context, null, null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_AuthorizationWithSchemes_PolicyAuthorizationResultForbid_CallsForbidAsyncForEveryScheme()
        {
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();

            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(new ServerSentEventsAuthorization { AuthenticationSchemes = "schema1,schema2" });
            HttpContext context = PrepareHttpContext(authenticationServiceMock);
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Forbid());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ForbidAsync(context, "schema1", null), Times.Once);
            authenticationServiceMock.Verify(m => m.ForbidAsync(context, "schema2", null), Times.Once);
        }
        #endregion
    }
}
