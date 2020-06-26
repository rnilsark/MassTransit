namespace MassTransit.JobService.Pipeline
{
    using System;
    using Components;
    using Components.Consumers;
    using Configuration;
    using ConsumeConnectors;
    using ConsumerSpecifications;
    using GreenPipes;
    using MassTransit.Pipeline;
    using MassTransit.Pipeline.ConsumerFactories;
    using MassTransit.Pipeline.Filters;
    using Util;


    public class JobConsumerMessageConnector<TConsumer, TJob> :
        IConsumerMessageConnector<TConsumer>
        where TConsumer : class, IJobConsumer<TJob>
        where TJob : class
    {
        readonly IConsumerConnector _cancelJobConsumerConnector;
        readonly IConsumerConnector _startJobConsumerConnector;
        readonly IConsumerConnector _submitJobConsumerConnector;

        public JobConsumerMessageConnector()
        {
            _submitJobConsumerConnector = ConsumerConnectorCache<SubmitJobConsumer<TJob>>.Connector;
            _startJobConsumerConnector = ConsumerConnectorCache<StartJobConsumer<TJob>>.Connector;
            _cancelJobConsumerConnector = ConsumerConnectorCache<CancelJobConsumer<TJob>>.Connector;
        }

        public Type MessageType => typeof(TJob);

        public IConsumerMessageSpecification<TConsumer> CreateConsumerMessageSpecification()
        {
            return new JobConsumerMessageSpecification<TConsumer, TJob>();
        }

        public ConnectHandle ConnectConsumer(IConsumePipeConnector consumePipe, IConsumerFactory<TConsumer> consumerFactory,
            IConsumerSpecification<TConsumer> specification)
        {
            var jobServiceOptions = specification.Options<JobServiceOptions>();

            var options = specification.Options<JobOptions<TJob>>();

            IConsumerMessageSpecification<TConsumer, TJob> messageSpecification = specification.GetMessageSpecification<TJob>();

            var turnoutSpecification = messageSpecification as JobConsumerMessageSpecification<TConsumer, TJob>;
            if (turnoutSpecification == null)
                throw new ArgumentException("The consumer specification did not match the message specification type");

            var submitJobHandle = ConnectSubmitJobConsumer(consumePipe, turnoutSpecification.SubmitJobSpecification, options);

            var startJobHandle = ConnectStartJobConsumer(consumePipe, turnoutSpecification.StartJobSpecification, options, jobServiceOptions.JobService,
                CreateJobPipe(consumerFactory, specification));

            var cancelJobHandle = ConnectCancelJobConsumer(consumePipe, turnoutSpecification.CancelJobSpecification, jobServiceOptions.JobService);

            return new MultipleConnectHandle(submitJobHandle, startJobHandle, cancelJobHandle);
        }

        public IPipe<ConsumeContext<TJob>> CreateJobPipe(IConsumerFactory<TConsumer> consumerFactory, IConsumerSpecification<TConsumer> specification)
        {
            IConsumerMessageSpecification<TConsumer, TJob> messageSpecification = specification.GetMessageSpecification<TJob>();

            var options = specification.Options<JobOptions<TJob>>();

            var jobFilter = new JobConsumerMessageFilter<TConsumer, TJob>(options.RetryPolicy);

            IPipe<ConsumerConsumeContext<TConsumer, TJob>> consumerPipe = messageSpecification.Build(jobFilter);

            IPipe<ConsumeContext<TJob>> messagePipe = messageSpecification.BuildMessagePipe(x =>
            {
                x.UseFilter(new ConsumerMessageFilter<TConsumer, TJob>(consumerFactory, consumerPipe));
            });

            return messagePipe;
        }

        ConnectHandle ConnectSubmitJobConsumer(IConsumePipeConnector consumePipe,
            IConsumerSpecification<SubmitJobConsumer<TJob>> specification, JobOptions<TJob> options)
        {
            var consumerFactory = new DelegateConsumerFactory<SubmitJobConsumer<TJob>>(() => new SubmitJobConsumer<TJob>(options));

            return _submitJobConsumerConnector.ConnectConsumer(consumePipe, consumerFactory, specification);
        }

        ConnectHandle ConnectStartJobConsumer(IConsumePipeConnector consumePipe, IConsumerSpecification<StartJobConsumer<TJob>> specification,
            JobOptions<TJob> options, IJobService jobService, IPipe<ConsumeContext<TJob>> pipe)
        {
            var consumerFactory = new DelegateConsumerFactory<StartJobConsumer<TJob>>(() => new StartJobConsumer<TJob>(jobService, options, pipe));

            return _startJobConsumerConnector.ConnectConsumer(consumePipe, consumerFactory, specification);
        }

        ConnectHandle ConnectCancelJobConsumer(IConsumePipeConnector consumePipe, IConsumerSpecification<CancelJobConsumer<TJob>> specification,
            IJobService jobService)
        {
            var consumerFactory = new DelegateConsumerFactory<CancelJobConsumer<TJob>>(() => new CancelJobConsumer<TJob>(jobService));

            return _cancelJobConsumerConnector.ConnectConsumer(consumePipe, consumerFactory, specification);
        }
    }
}
