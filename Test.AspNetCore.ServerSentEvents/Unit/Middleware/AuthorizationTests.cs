using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Unit.Middleware.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Unit.Middleware
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
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(authorizationPolicyProvider: PrepareAuthorizationPolicyProvider());
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), context), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_NoAuthorization_DoesNotCallAuthorizeAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(authorizationPolicyProvider: PrepareAuthorizationPolicyProvider());
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), DEFAULT_AUTHENTICATE_RESULT, context, null), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_CallsAuthenticateAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = ServerSentEventsAuthorization.Default });

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthenticateAsync(It.IsAny<AuthorizationPolicy>(), context), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_CallsAuthorizeAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = ServerSentEventsAuthorization.Default });

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context);

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            policyEvaluatorMock.Verify(m => m.AuthorizeAsync(It.IsAny<AuthorizationPolicy>(), DEFAULT_AUTHENTICATE_RESULT, context, null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultSuccess_DoesNotCallChallengeAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = ServerSentEventsAuthorization.Default });

            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(authenticationService: authenticationServiceMock.Object);
            
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Success());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ChallengeAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultSuccess_DoesNotCallForbidAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = ServerSentEventsAuthorization.Default });

            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(authenticationService: authenticationServiceMock.Object);

            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Success());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ForbidAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultChallenge_CallsChallengeAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = ServerSentEventsAuthorization.Default });

            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(authenticationService: authenticationServiceMock.Object);

            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Challenge());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ChallengeAsync(context, null, null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_AuthorizationWithSchemes_PolicyAuthorizationResultChallenge_CallsChallengeAsyncForEveryScheme()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = new ServerSentEventsAuthorization { AuthenticationSchemes = "schema1,schema2" } });

            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(authenticationService: authenticationServiceMock.Object);

            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Challenge());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ChallengeAsync(context, "schema1", null), Times.Once);
            authenticationServiceMock.Verify(m => m.ChallengeAsync(context, "schema2", null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Authorization_PolicyAuthorizationResultForbid_CallsForbidAsync()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = ServerSentEventsAuthorization.Default });

            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(authenticationService: authenticationServiceMock.Object);

            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Forbid());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ForbidAsync(context, null, null), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_AuthorizationWithSchemes_PolicyAuthorizationResultForbid_CallsForbidAsyncForEveryScheme()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                authorizationPolicyProvider: PrepareAuthorizationPolicyProvider(),
                options: new ServerSentEventsOptions { Authorization = new ServerSentEventsAuthorization { AuthenticationSchemes = "schema1,schema2" } });

            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(authenticationService: authenticationServiceMock.Object);
            
            Mock<IPolicyEvaluator> policyEvaluatorMock = PreparePolicyEvaluatorMock(context, policyAuthorizationResult: PolicyAuthorizationResult.Forbid());

            await serverSentEventsMiddleware.Invoke(context, policyEvaluatorMock.Object);

            authenticationServiceMock.Verify(m => m.ForbidAsync(context, "schema1", null), Times.Once);
            authenticationServiceMock.Verify(m => m.ForbidAsync(context, "schema2", null), Times.Once);
        }
        #endregion
    }
}
