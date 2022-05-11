using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Middleware.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Middleware
{
    public class ValidationTests
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
        public async Task Invoke_SseRequest_NoValidation_DoesNotPreventAcceptAsync()
        {
            Mock<Action<HttpResponse>> onPrepareAcceptMock = new Mock<Action<HttpResponse>>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(options: new ServerSentEventsOptions {
                OnPrepareAccept = onPrepareAcceptMock.Object
            });
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            onPrepareAcceptMock.Verify(m => m(It.IsAny<HttpResponse>()), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Validation_DoesNotPreventAcceptWhenValidationSucceedsAsync()
        {
            Mock<Action<HttpResponse>> onPrepareAcceptMock = new Mock<Action<HttpResponse>>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(options: new ServerSentEventsOptions {
                OnPrepareAccept = onPrepareAcceptMock.Object,
            }, validateConnectionResult:true);
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            onPrepareAcceptMock.Verify(m => m(It.IsAny<HttpResponse>()), Times.Once);
        }

        [Fact]
        public async Task Invoke_SseRequest_Validation_DoesPreventAcceptWhenValidationFailsAsync()
        {
            Mock<Action<HttpResponse>> onPrepareAcceptMock = new Mock<Action<HttpResponse>>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(options: new ServerSentEventsOptions {
                OnPrepareAccept = onPrepareAcceptMock.Object
            }, validateConnectionResult:false);
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            onPrepareAcceptMock.Verify(m => m(It.IsAny<HttpResponse>()), Times.Never);
        }
        #endregion
    }
}
