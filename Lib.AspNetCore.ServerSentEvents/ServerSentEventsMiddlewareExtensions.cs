using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            services.AddAuthorization();
            services.AddAuthorizationPolicyEvaluator();

            services.Configure(configureOptions);

            services.AddSingleton<TServerSentEventsService>();
            services.AddSingleton<TIServerSentEventsService>(serviceProvider => serviceProvider.GetService<TServerSentEventsService>());

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
            return app.UseServerSentEvents<ServerSentEventsService>();
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the pipeline with default service.
        /// </summary>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="options">The options.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder app, ServerSentEventsOptions options)
        {
            return app.UseServerSentEvents<ServerSentEventsService>(options);
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
            return app.UseServerSentEvents<TServerSentEventsService>(new ServerSentEventsOptions());
        }

        /// <summary>
        /// Adds the middleware which provides support for Server-Sent Events protocol to the pipeline with custom service.
        /// </summary>
        /// <typeparam name="TServerSentEventsService">The type of custom <see cref="ServerSentEventsService"/>.</typeparam>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="options">The options.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder UseServerSentEvents<TServerSentEventsService>(this IApplicationBuilder app, ServerSentEventsOptions options)
            where TServerSentEventsService : ServerSentEventsService
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<ServerSentEventsMiddleware<TServerSentEventsService>>(Options.Create(options));
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
        /// Adds the middleware which provides support for Server-Sent Events protocol to the branch of pipeline with default service.
        /// </summary>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="pathMatch">The request path to match.</param>
        /// <param name="options">The options.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder MapServerSentEvents(this IApplicationBuilder app, PathString pathMatch, ServerSentEventsOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Map(pathMatch, branchedApp => branchedApp.UseServerSentEvents(options));
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
        /// <typeparam name="TServerSentEventsService">The type of custom <see cref="ServerSentEventsService"/>.</typeparam>
        /// <param name="app">The pipeline builder.</param>
        /// <param name="pathMatch">The request path to match.</param>
        /// <param name="options">The options.</param>
        /// <returns>The pipeline builder.</returns>
        public static IApplicationBuilder MapServerSentEvents<TServerSentEventsService>(this IApplicationBuilder app, PathString pathMatch, ServerSentEventsOptions options)
            where TServerSentEventsService : ServerSentEventsService
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Map(pathMatch, branchedApp => branchedApp.UseServerSentEvents<TServerSentEventsService>(options));
        }
        #endregion
    }
}
