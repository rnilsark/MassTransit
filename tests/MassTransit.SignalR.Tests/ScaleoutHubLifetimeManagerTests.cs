﻿namespace MassTransit.SignalR.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.AspNetCore.SignalR.Protocol;
    using NUnit.Framework;
    using OfficialFramework;
    using Testing;


    public class ScaleoutHubLifetimeManagerTests : DoubleScaleoutBackplaneTestFixture<MyHub>
    {
        async Task AssertMessageAsync(TestClient client)
        {
            var message = await client.ReadAsync().OrTimeout() as InvocationMessage;
            Assert.NotNull(message);
            Assert.AreEqual("Hello", message.Target);
            Assert.AreEqual(1, message.Arguments.Length);
            Assert.AreEqual("World", (string)message.Arguments[0]);
        }

        [Test]
        public async Task InvokeAllAsyncWithMultipleServersWritesToAllConnectionsOutput()
        {
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager1.OnConnectedAsync(connection1).OrTimeout(Harness.TestTimeout);
                await manager2.OnConnectedAsync(connection2).OrTimeout(Harness.TestTimeout);

                await manager1.SendAllAsync("Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.All.Consumed.Select<All<MyHub>>().Any());
                Assert.IsTrue(Backplane2Harness.All.Consumed.Select<All<MyHub>>().Any());

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);
            }
        }

        [Test]
        public async Task InvokeAllAsyncWithMultipleServersDoesNotWriteToDisconnectedConnectionsOutput()
        {
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager1.OnConnectedAsync(connection1).OrTimeout(Harness.TestTimeout);
                await manager2.OnConnectedAsync(connection2).OrTimeout(Harness.TestTimeout);

                await manager2.OnDisconnectedAsync(connection2).OrTimeout(Harness.TestTimeout);

                await manager2.SendAllAsync("Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                await AssertMessageAsync(client1);

                Assert.Null(client2.TryRead());
            }
        }

        [Test]
        public async Task InvokeConnectionAsyncOnServerWithoutConnectionWritesOutputToConnection()
        {
            using (var client = new TestClient())
            {
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager2.SendConnectionAsync(connection.ConnectionId, "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.Connection.Consumed.Select<Connection<MyHub>>().Any());

                await AssertMessageAsync(client);
            }
        }

        [Test]
        public async Task InvokeGroupAsyncOnServerWithoutConnectionWritesOutputToGroupConnection()
        {
            using (var client = new TestClient())
            {
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager1.AddToGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);

                await manager2.SendGroupAsync("name", "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.Group.Consumed.Select<Group<MyHub>>().Any());

                await AssertMessageAsync(client);
            }
        }

        [Test]
        public async Task RemoveGroupFromConnectionOnDifferentServerNotInGroupDoesNothing()
        {
            using (var client = new TestClient())
            {
                Task<ConsumeContext<Ack<MyHub>>> ackHandler =
                    Harness.SubscribeHandler<Ack<MyHub>>(); // Lets us verify that the ack was sent back to our bus endpoint
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager2.RemoveFromGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.GroupManagement.Consumed.Select<GroupManagement<MyHub>>().Any());

                ConsumeContext<Ack<MyHub>> responseContext = await ackHandler;

                Assert.AreEqual(manager1.ServerName, responseContext.Message.ServerName);
            }
        }

        [Test]
        public async Task AddGroupAsyncForConnectionOnDifferentServerWorks()
        {
            using (var client = new TestClient())
            {
                Task<ConsumeContext<Ack<MyHub>>> ackHandler =
                    Harness.SubscribeHandler<Ack<MyHub>>(); // Lets us verify that the ack was sent back to our bus endpoint
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager2.AddToGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.GroupManagement.Consumed.Select<GroupManagement<MyHub>>().Any());

                ConsumeContext<Ack<MyHub>> responseContext = await ackHandler;

                Assert.AreEqual(manager1.ServerName, responseContext.Message.ServerName);

                await manager2.SendGroupAsync("name", "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.Group.Consumed.Select<Group<MyHub>>().Any());

                await AssertMessageAsync(client);
            }
        }

        [Test]
        public async Task AddGroupAsyncForConnectionOnDifferentServerAlreadyInGroupDoesNothing()
        {
            using (var client = new TestClient())
            {
                Task<ConsumeContext<Ack<MyHub>>> ackHandler =
                    Harness.SubscribeHandler<Ack<MyHub>>(); // Lets us verify that the ack was sent back to our bus endpoint
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager1.AddToGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);
                await manager2.AddToGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.GroupManagement.Consumed.Select<GroupManagement<MyHub>>().Any());

                ConsumeContext<Ack<MyHub>> responseContext = await ackHandler;

                Assert.AreEqual(manager1.ServerName, responseContext.Message.ServerName);

                await manager2.SendGroupAsync("name", "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.Group.Consumed.Select<Group<MyHub>>().Any());

                await AssertMessageAsync(client);
                Assert.Null(client.TryRead());
            }
        }

        [Test]
        public async Task RemoveGroupAsyncForConnectionOnDifferentServerWorks()
        {
            using (var client = new TestClient())
            {
                Task<ConsumeContext<Ack<MyHub>>> ackHandler =
                    Harness.SubscribeHandler<Ack<MyHub>>(); // Lets us verify that the ack was sent back to our bus endpoint
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager1.AddToGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);

                await manager2.SendGroupAsync("name", "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                IReceivedMessage<Group<MyHub>> firstMessage = Backplane1Harness.Group.Consumed.Select<Group<MyHub>>().FirstOrDefault();

                Assert.NotNull(firstMessage);

                await AssertMessageAsync(client);

                await manager2.RemoveFromGroupAsync(connection.ConnectionId, "name").OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane1Harness.GroupManagement.Consumed.Select<GroupManagement<MyHub>>().Any());

                ConsumeContext<Ack<MyHub>> responseContext = await ackHandler;

                Assert.AreEqual(manager1.ServerName, responseContext.Message.ServerName);

                await manager2.SendGroupAsync("name", "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                IReceivedMessage<Group<MyHub>> secondMessage = Backplane1Harness.Group.Consumed.Select<Group<MyHub>>().Skip(1).FirstOrDefault();

                Assert.NotNull(secondMessage);

                Assert.Null(client.TryRead());
            }
        }

        [Test]
        public async Task InvokeConnectionAsyncForLocalConnectionDoesNotPublishToBackplane()
        {
            using (var client = new TestClient())
            {
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                var connection = HubConnectionContextUtils.Create(client.Connection);

                // Add connection to both "servers" to see if connection receives message twice
                await manager1.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);
                await manager2.OnConnectedAsync(connection).OrTimeout(Harness.TestTimeout);

                await manager1.SendConnectionAsync(connection.ConnectionId, "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsFalse(Backplane1Harness.Connection.Consumed.Select<Connection<MyHub>>().Any());
                Assert.IsFalse(Backplane2Harness.Connection.Consumed.Select<Connection<MyHub>>().Any());

                await AssertMessageAsync(client);
                Assert.Null(client.TryRead());
            }
        }

        [Test]
        public async Task WritingToRemoteConnectionThatFailsDoesNotThrow()
        {
            using (var client = new TestClient())
            {
                MassTransitHubLifetimeManager<MyHub> manager1 = Backplane1Harness.HubLifetimeManager;
                MassTransitHubLifetimeManager<MyHub> manager2 = Backplane2Harness.HubLifetimeManager;

                // Force an exception when writing to connection
                var connectionMock = HubConnectionContextUtils.CreateMock(client.Connection);

                await manager2.OnConnectedAsync(connectionMock).OrTimeout(Harness.TestTimeout);

                // This doesn't throw because there is no connection.ConnectionId on this server so it has to publish to the backplane.
                // And once that happens there is no way to know if the invocation was successful or not.
                await manager1.SendConnectionAsync(connectionMock.ConnectionId, "Hello", new object[] {"World"}).OrTimeout(Harness.TestTimeout);

                Assert.IsTrue(Backplane2Harness.Connection.Consumed.Select<Connection<MyHub>>().Any());
            }
        }
    }


    public class MyHub : Hub
    {
    }
}
