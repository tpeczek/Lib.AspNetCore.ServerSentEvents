using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Lib.AspNetCore.ServerSentEvents.Internals;

namespace Test.AspNetCore.ServerSentEvents.Unit
{
    public class ServerSentEventsServiceTests
    {
        #region Prepare SUT
        private static async Task<ServerSentEventsClient> PrepareAndAddServerSentEventsClientAsync(ServerSentEventsService serverSentEventsService)
        {
            HttpContext context = new DefaultHttpContext();
            ServerSentEventsClient serverSentEventsClient = new ServerSentEventsClient(Guid.NewGuid(), context, false);

            await serverSentEventsService.OnConnectAsync(context.Request, serverSentEventsClient);

            serverSentEventsService.AddClient(serverSentEventsClient);

            return serverSentEventsClient;
        }
        #endregion

        #region Tests
        [Fact]
        public void RemoveClient_ClientsBeingAddedToGroupsInParaller_NoRaceCondition()
        {
            // ARRANGE
            ServerSentEventsService serverSentEventsService = new ServerSentEventsService(Options.Create(new ServerSentEventsServiceOptions<ServerSentEventsService>
            {
                OnClientConnected = (service, clientConnectedArgs) =>
                {
                    for (var i = 0; i < 1000; i++)
                    {
                        service.AddToGroup(Guid.NewGuid().ToString(), clientConnectedArgs.Client);
                    }
                }
            }));

            // ACT
            Task[] clientsTasks = new Task[100];
            for (int i = 0; i < clientsTasks.Length; i++)
            {
                clientsTasks[i] = Task.Run(async () =>
                {
                    ServerSentEventsClient serverSentEventsClient = await PrepareAndAddServerSentEventsClientAsync(serverSentEventsService);

                    await Task.Delay(100);

                    serverSentEventsService.RemoveClient(serverSentEventsClient);
                });
            }
            Exception recordedException = Record.Exception(() => Task.WaitAll(clientsTasks));

            // ASSERT
            Assert.False((recordedException as AggregateException)?.InnerExceptions.Any(ex => (ex as InvalidOperationException)?.Message.Contains("Collection was modified") ?? false) ?? false);
        }

        [Fact]
        public async Task GetClients_GroupNameProvidedAndGroupExists_ReturnsGroup()
        {
            // ARRANGE
            const string serverSentEventsClientsGroupName = nameof(GetClients_GroupNameProvidedAndGroupExists_ReturnsGroup);
            ServerSentEventsService serverSentEventsService = new ServerSentEventsService(Options.Create(new ServerSentEventsServiceOptions<ServerSentEventsService>
            {
                OnClientConnected = (service, clientConnectedArgs) =>
                {
                    service.AddToGroup(serverSentEventsClientsGroupName, clientConnectedArgs.Client);
                }
            }));
            ServerSentEventsClient serverSentEventsClient = await PrepareAndAddServerSentEventsClientAsync(serverSentEventsService);

            // ACT
            IReadOnlyCollection<IServerSentEventsClient> serverSentEventsClientsGroup = serverSentEventsService.GetClients(serverSentEventsClientsGroupName);

            // ASSERT
            Assert.Single(serverSentEventsClientsGroup, serverSentEventsClient);
        }

        [Fact]
        public void GetClients_GroupNameProvidedAndGroupNotExists_ReturnsEmptyGroup()
        {
            // ARRANGE
            const string serverSentEventsClientsGroupName = nameof(GetClients_GroupNameProvidedAndGroupNotExists_ReturnsEmptyGroup);
            ServerSentEventsService serverSentEventsService = new ServerSentEventsService(Options.Create<ServerSentEventsServiceOptions<ServerSentEventsService>>(null));

            // ACT
            IReadOnlyCollection<IServerSentEventsClient> serverSentEventsClientsGroup = serverSentEventsService.GetClients(serverSentEventsClientsGroupName);

            // ASSERT
            Assert.Empty(serverSentEventsClientsGroup);
        }
        #endregion
    }
}
