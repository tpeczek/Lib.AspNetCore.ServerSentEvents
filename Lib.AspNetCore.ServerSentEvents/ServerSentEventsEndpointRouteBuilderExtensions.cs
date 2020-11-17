#if !NETCOREAPP2_1 && !NET461
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides extension methods for <see cref="IEndpointRouteBuilder"/> to add Server-Sent Events.
    /// </summary>
    public static class ServerSentEventsEndpointRouteBuilderExtensions
    {
        #region Fields
        private const string DEFAULT_DISPLAY_NAME = "Server-Sent Events";
        #endregion

        #region Methods
        /// <summary>
        /// Adds a Server-Sent Events endpoint to the <see cref="IEndpointRouteBuilder"/> with the specified template.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the Server-Sent Events endpoint to.</param>
        /// <param name="pattern">The URL pattern of the Server-Sent Events endpoint.</param>
        /// <returns>A convention routes for the Server-Sent Events endpoint.</returns>
        public static IEndpointConventionBuilder MapServerSentEvents(this IEndpointRouteBuilder endpoints, string pattern)
        {
            return endpoints.MapServerSentEvents<ServerSentEventsService>(pattern);
        }

        /// <summary>
        /// Adds a Server-Sent Events endpoint to the <see cref="IEndpointRouteBuilder"/> with the specified template and options.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the Server-Sent Events endpoint to.</param>
        /// <param name="pattern">The URL pattern of the Server-Sent Events endpoint.</param>
        /// <param name="options">A <see cref="ServerSentEventsOptions"/> used to configure the Server-Sent Events.</param>
        /// <returns>A convention routes for the Server-Sent Events endpoint.</returns>
        public static IEndpointConventionBuilder MapServerSentEvents(this IEndpointRouteBuilder endpoints, string pattern, ServerSentEventsOptions options)
        {
            return endpoints.MapServerSentEvents<ServerSentEventsService>(pattern, options);
        }

        /// <summary>
        /// Adds a Server-Sent Events endpoint to the <see cref="IEndpointRouteBuilder"/> with the specified template.
        /// </summary>
        /// <typeparam name="TServerSentEventsService">The type of custom <see cref="ServerSentEventsService"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the Server-Sent Events endpoint to.</param>
        /// <param name="pattern">The URL pattern of the Server-Sent Events endpoint.</param>
        /// <returns>A convention routes for the Server-Sent Events endpoint.</returns>
        public static IEndpointConventionBuilder MapServerSentEvents<TServerSentEventsService>(this IEndpointRouteBuilder endpoints, string pattern)
            where TServerSentEventsService : ServerSentEventsService
        {
            return endpoints.MapServerSentEvents<TServerSentEventsService>(pattern, new ServerSentEventsOptions());
        }

        /// <summary>
        /// Adds a Server-Sent Events endpoint to the <see cref="IEndpointRouteBuilder"/> with the specified template and options.
        /// </summary>
        /// <typeparam name="TServerSentEventsService">The type of custom <see cref="ServerSentEventsService"/>.</typeparam>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the Server-Sent Events endpoint to.</param>
        /// <param name="pattern">The URL pattern of the Server-Sent Events endpoint.</param>
        /// <param name="options">A <see cref="ServerSentEventsOptions"/> used to configure the Server-Sent Events.</param>
        /// <returns>A convention routes for the Server-Sent Events endpoint.</returns>
        public static IEndpointConventionBuilder MapServerSentEvents<TServerSentEventsService>(this IEndpointRouteBuilder endpoints, string pattern, ServerSentEventsOptions options)
            where TServerSentEventsService : ServerSentEventsService
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            RequestDelegate pipeline = endpoints.CreateApplicationBuilder()
               .UseServerSentEvents<TServerSentEventsService>(options)
               .Build();

            return endpoints.Map(pattern, pipeline).WithDisplayName(DEFAULT_DISPLAY_NAME);
        }
        #endregion
    }
}
#endif