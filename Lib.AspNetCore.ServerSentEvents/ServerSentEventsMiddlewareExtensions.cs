using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides extensions for registering middleware which provides support for Server-Sent Events protocol.
    /// </summary>
    public static class ServerSentEventsMiddlewareExtensions
    {
        #region Methods
        /// <summary>
        /// Registers default service which provides operations over Server-Sent Events protocol.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        /// <returns>The collection of service descriptors.</returns>
        public static IServiceCollection AddServerSentEvents(this IServiceCollection services)
        {
            return services.AddServerSentEvents<IServerSentEventsService, ServerSentEventsService>((serverSentEventsServiceOptions) => { });
        }

        /// <summary>
        /// Registers default service which provides operations over Server-Sent Events protocol.
        /// </summary>
        /// <param name="services">The collection of service descriptors.</param>
        /// <param name="configureOptions">A delegate to configure the ServerSentEventsServiceOptions.</param>
        /// <returns>The collection of service descriptors.</returns>
        public static IServiceCollection AddServerSentEvents(this IServiceCollection services, Action<ServerSentEventsServiceOptions<ServerSentEventsService>> configureOptions)
        {
            return services.AddServerSentEvents<IServerSentEventsService, ServerSentEventsService>(configureOptions);
        }

        /// <summary>
        /// Registers custom service which provides operations over Server-Sent Events protocol.
        /// </summary>
        /// <typeparam name="TIServerSentEventsService">The type of service contract.</typeparam>
        /// <typeparam name="TServerSentEventsService">The type of service implementation.</typeparam>
        /// <param name="services">The collection of service descriptors.</param>
        /// <returns>The collection of service descriptors.</returns>
        public static IServiceCollection AddServerSentEvents<TIServerSentEventsService, TServerSentEventsService>(this IServiceCollection services)
            where TIServerSentEventsService : class, IServerSentEventsService
            where TServerSentEventsService : ServerSentEventsService, TIServerSentEventsService
        {
            return services.AddServerSentEvents<TIServerSentEventsService, TServerSentEventsService>((serverSentEventsServiceOptions) => { });
        }

        /// <summary>
        /// Registers custom service which provides operations over Server-Sent Events protocol.
        /// </summary>
        /// <typeparam name="TIServerSentEventsService">The type of service contract.</typeparam>
        /// <typeparam name="TServerSentEventsService">The type of service implementation.</typeparam>
        /// <param name="services">The collection of service descriptors.</param>
        /// <param name="configureOptions">A delegate to configure the ServerSentEventsServiceOptions.</param>
        /// <returns>The collection of service descriptors.</returns>
        public static IServiceCollection AddServerSentEvents<TIServerSentEventsService, TServerSentEventsService>(this IServiceCollection services, Action<ServerSentEventsServiceOptions<TServerSentEventsService>> configureOptions)
            where TIServerSentEventsService : class, IServerSentEventsService
            where TServerSentEventsService : ServerSentEventsService, TIServerSentEventsService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            services.AddSingleton<TServerSentEventsService>();
            services.AddSingleton<TIServerSentEventsService>(serviceProvider => serviceProvider.GetService<TServerSentEventsService>());

            services.Configure(configureOptions);
            services.AddSingleton<IHostedService, ServerSentEventsKeepaliveService<TServerSentEventsService>>();

            return services;
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the pipeline with default service.
        /// </summary>
        /// <param name="app">The pipeline builder.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseServerSentEvents<ServerSentEventsService>();
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the pipeline with custom service.
        /// </summary>
        /// <typeparam name="TServerSentEventsService">The type of custom <see cref="ServerSentEventsService"/>.</typeparam>
        /// <param name="app">The pipeline builder.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder UseServerSentEvents<TServerSentEventsService>(this IApplicationBuilder app)
            where TServerSentEventsService : ServerSentEventsService
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ServerSentEventsMiddleware<TServerSentEventsService>>();
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the pipeline with custom service.
        /// </summary>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="serverSentEventsService">The custom service.</param>
        /// <returns>The pipeline builder.</returns>
        [Obsolete("This method will soon be deprecated. Use UseServerSentEvents<TServerSentEventsService> instead.")]
        public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder app, ServerSentEventsService serverSentEventsService)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (serverSentEventsService == null)
            {
                throw new ArgumentNullException(nameof(serverSentEventsService));
            }

            Type serverSentEventsServiceType = serverSentEventsService.GetType();
            Type serverSentEventsMiddlewareType = typeof(ServerSentEventsMiddleware<>).MakeGenericType(serverSentEventsServiceType);

            return app.UseMiddleware(serverSentEventsMiddlewareType);
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the branch of pipeline with default service.
        /// </summary>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="pathMatch">The request path to match.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder MapServerSentEvents(this IApplicationBuilder app, PathString pathMatch)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Map(pathMatch, branchedApp => branchedApp.UseServerSentEvents());
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the branch of pipeline with custom service.
        /// </summary>
        /// <typeparam name="TServerSentEventsService">The type of custom <see cref="ServerSentEventsService"/>.</typeparam>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="pathMatch">The request path to match.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder MapServerSentEvents<TServerSentEventsService>(this IApplicationBuilder app, PathString pathMatch)
            where TServerSentEventsService : ServerSentEventsService
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Map(pathMatch, branchedApp => branchedApp.UseServerSentEvents<TServerSentEventsService>());
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the branch of pipeline with custom service.
        /// </summary>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="pathMatch">The request path to match.</param>
        /// <param name="serverSentEventsService">The custom service.</param>
        /// <returns>The pipeline builder.</returns>
        [Obsolete("This method will soon be deprecated. Use MapServerSentEvents<TServerSentEventsService> instead.")]
        public static IApplicationBuilder MapServerSentEvents(this IApplicationBuilder app, PathString pathMatch, ServerSentEventsService serverSentEventsService)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (serverSentEventsService == null)
            {
                throw new ArgumentNullException(nameof(serverSentEventsService));
            }

            return app.Map(pathMatch, branchedApp => branchedApp.UseServerSentEvents(serverSentEventsService));
        }
        #endregion
    }
}
