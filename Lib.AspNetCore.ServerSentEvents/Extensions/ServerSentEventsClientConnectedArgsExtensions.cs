using System.Linq;
using System.Text.RegularExpressions;

namespace Lib.AspNetCore.ServerSentEvents.Extensions
{
    /// <summary>
    /// Extension methods utility class that simplifies operations on the client at connection time 
    /// </summary>
    public static class ServerSentEventsClientConnectedArgsExtensions
    {
        /// <summary>
        /// Add a property to the client from the specified header in the request object.
        /// </summary>
        /// <param name="args">The client connected args object that contains reference to both the client and the request object.</param>
        /// <param name="header">The name of the request header that contains the property value to add to the client. The property name will be equal to the header name.</param>
        /// <param name="overwrite">If true the value of the existing property will be updated. If false the current value of the existing property will be retained.</param>
        public static void AddPropertyToClientFromHeader(this ServerSentEventsClientConnectedArgs args, string header, bool overwrite = false)
        {
            if (!args.Request.Headers.ContainsKey(header))
                return;

            if (overwrite)
            {
                args.Client.Properties.AddOrUpdate(header, args.Request.Headers[header], (k, v) => args.Request.Headers[k]);
                return;
            }

            args.Client.Properties.TryAdd(header, args.Request.Headers[header]);
        }

        /// <summary>
        /// Add a set of properties to the client from the headers in the request object whose name matches the specified regular expression.
        /// </summary>
        /// <param name="args">The client connected args object that contains reference to both the client and the request object.</param>
        /// <param name="headerSelectorRegEx">The regular expression used to select the headers in the request object.</param>
        /// <param name="overwrite">If true the value of any existing property will be updated. If false the current value of any existing property will be retained.</param>
        public static void AddPropertiesToClientFromHeaders(this ServerSentEventsClientConnectedArgs args, Regex headerSelectorRegEx, bool overwrite = false)
        {
            foreach (var header in args.Request.Headers.Where(h => headerSelectorRegEx.IsMatch(h.Key)))
            {
                if (overwrite && args.Client.Properties.ContainsKey(header.Key))
                {
                    args.Client.Properties.AddOrUpdate(header.Key, header.Value, (k, v) => header.Value);
                    continue;
                }

                args.Client.Properties.TryAdd(header.Key, header.Value);

            }
        }
    }
}
