﻿namespace MassTransit.Tests.AutomatonymousIntegration
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit.Saga;
    using NUnit.Framework;
    using TestFramework;


    [TestFixture]
    public class Catching_a_fault :
        InMemoryTestFixture
    {
        [Test]
        public async Task Should_receive_the_caught_fault_response()
        {
            var message = new Start();

            Task<ConsumeContext<ServiceFaulted>> serviceFaulted = ConnectPublishHandler<ServiceFaulted>();

            Response<StartFaulted> startFaulted = await Bus.Request<Start, StartFaulted>(InputQueueAddress, message, TestCancellationToken, TestTimeout);

            Assert.AreEqual(message.CorrelationId, startFaulted.CorrelationId);

            ConsumeContext<ServiceFaulted> context = await serviceFaulted;

            Assert.AreEqual(message.CorrelationId, context.CorrelationId);
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            _machine = new TestStateMachine();
            _repository = new InMemorySagaRepository<Instance>();

            configurator.StateMachineSaga(_machine, _repository);
        }

        State GetCurrentState(Instance state)
        {
            return _machine.GetState(state).Result;
        }

        TestStateMachine _machine;
        InMemorySagaRepository<Instance> _repository;


        class Instance :
            SagaStateMachineInstance
        {
            public Instance(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            protected Instance()
            {
            }

            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class TestStateMachine :
            MassTransitStateMachine<Instance>
        {
            public TestStateMachine()
            {
                InstanceState(x => x.CurrentState);

                Initially(
                    When(Started)
                        .Then(context =>
                        {
                            throw new NotSupportedException("This is expected, but nonetheless exceptional");
                        })
                        .Publish(context => new Started(context.Instance.CorrelationId))
                        .TransitionTo(Running)
                        .Catch<NotSupportedException>(ex => ex
                            .Respond(context => new StartFaulted(context.Instance.CorrelationId))
                            .Publish(context => new ServiceFaulted(context.Instance.CorrelationId))
                            .TransitionTo(FailedToStart)));
            }

            public State FailedToStart { get; private set; }
            public State Running { get; private set; }

            public Event<Start> Started { get; private set; }
        }


        public class Start :
            CorrelatedBy<Guid>
        {
            public Start()
            {
                CorrelationId = NewId.NextGuid();
            }

            public Start(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; set; }
        }


        public class Started :
            CorrelatedBy<Guid>
        {
            public Started()
            {
                CorrelationId = NewId.NextGuid();
            }

            public Started(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; set; }
        }


        public class StartFaulted :
            CorrelatedBy<Guid>
        {
            public StartFaulted()
            {
                CorrelationId = NewId.NextGuid();
            }

            public StartFaulted(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; set; }
        }


        public class ServiceFaulted :
            CorrelatedBy<Guid>
        {
            public ServiceFaulted()
            {
                CorrelationId = NewId.NextGuid();
            }

            public ServiceFaulted(Guid correlationId)
            {
                CorrelationId = correlationId;
            }

            public Guid CorrelationId { get; set; }
        }
    }
}
