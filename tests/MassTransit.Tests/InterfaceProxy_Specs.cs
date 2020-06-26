﻿namespace MassTransit.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Shouldly;
    using TestFramework;


    [TestFixture]
    public class When_publishing_an_interface_message :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_have_address_value()
        {
            ConsumeContext<IProxyMe> message = await _handler;

            message.Message.Address.OriginalString.ShouldBe(UriString);
        }

        [Test]
        public async Task Should_have_correlation_id()
        {
            ConsumeContext<IProxyMe> message = await _handler;

            message.Message.CorrelationId.ShouldBe(_correlationId);
        }

        [Test]
        public async Task Should_have_integer_value()
        {
            ConsumeContext<IProxyMe> message = await _handler;

            message.Message.IntValue.ShouldBe(IntValue);
        }

        [Test]
        public async Task Should_have_received_message()
        {
            await _handler;
        }

        [Test]
        public async Task Should_have_string_value()
        {
            ConsumeContext<IProxyMe> message = await _handler;

            message.Message.StringValue.ShouldBe(StringValue);
        }

        const int IntValue = 42;
        const string StringValue = "Hello";
        readonly Guid _correlationId = Guid.NewGuid();
        Task<ConsumeContext<IProxyMe>> _handler;
        const string UriString = "http://localhost/";

        [OneTimeSetUp]
        public void Setup()
        {
            InputQueueSendEndpoint.Send<IProxyMe>(new
                {
                    IntValue,
                    StringValue,
                    Address = new Uri(UriString),
                    CorrelationId = _correlationId
                })
                .Wait(TestCancellationToken);
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            _handler = Handled<IProxyMe>(configurator);
        }


        public interface IProxyMe :
            CorrelatedBy<Guid>
        {
            int IntValue { get; }
            string StringValue { get; }
            Uri Address { get; }
        }
    }
}
