using Microsoft.Extensions.Options;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The <see cref="ServerSentEventsServiceOptions{TServerSentEventsService}"/> options related extensions.
    /// </summary>
    public static class OptionsExtensions
    {
        /// <summary>
        /// Converts an instace of <see cref="ServerSentEventsServiceOptions{TServerSentEventsService}"/> to <see cref="ServerSentEventsServiceOptions{ServerSentEventsService}"/>.
        /// </summary>
        /// <typeparam name="TServerSentEventsService">The type of <see cref="ServerSentEventsService"/> for the options instance.</typeparam>
        /// <param name="options">The options instance.</param>
        /// <returns>The option instance</returns>
        public static IOptions<ServerSentEventsServiceOptions<ServerSentEventsService>> ToBaseServerSentEventsServiceOptions<TServerSentEventsService>(this IOptions<ServerSentEventsServiceOptions<TServerSentEventsService>> options) where TServerSentEventsService : ServerSentEventsService
        {
            if (options is null)
            {
                return null;
            }

            if (options.Value is null)
            {
                return Options.Create<ServerSentEventsServiceOptions<ServerSentEventsService>>(null);
            }

            return Options.Create(new ServerSentEventsServiceOptions<ServerSentEventsService>
            {
                KeepaliveInterval = options.Value.KeepaliveInterval,
                KeepaliveMode = options.Value.KeepaliveMode,
                ReconnectInterval = options.Value.ReconnectInterval,
                OnClientConnected = options.Value.OnClientConnected,
                OnClientDisconnected = options.Value.OnClientDisconnected
            });
        }
    }
}
