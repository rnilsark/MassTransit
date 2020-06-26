﻿namespace MassTransit.ConsumePipeSpecifications
{
    using System.Collections.Generic;
    using GreenPipes;
    using GreenPipes.Filters;
    using PipeBuilders;


    public class MessageConsumePipeSplitFilterSpecification<TMessage, T> :
        ISpecificationPipeSpecification<ConsumeContext<TMessage>>
        where TMessage : class
        where T : class
    {
        readonly ISpecificationPipeSpecification<ConsumeContext<T>> _specification;

        public MessageConsumePipeSplitFilterSpecification(ISpecificationPipeSpecification<ConsumeContext<T>> specification)
        {
            _specification = specification;
        }

        public void Apply(ISpecificationPipeBuilder<ConsumeContext<TMessage>> builder)
        {
            var splitBuilder = new Builder(builder);

            _specification.Apply(splitBuilder);
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_specification == null)
                yield return this.Failure("Specification", "must not be null");
        }


        class Builder :
            ISpecificationPipeBuilder<ConsumeContext<T>>
        {
            readonly ISpecificationPipeBuilder<ConsumeContext<TMessage>> _builder;

            public Builder(ISpecificationPipeBuilder<ConsumeContext<TMessage>> builder)
            {
                _builder = builder;
            }

            public void AddFilter(IFilter<ConsumeContext<T>> filter)
            {
                var splitFilter = new SplitFilter<ConsumeContext<TMessage>, ConsumeContext<T>>(filter, ContextProvider, InputContextProvider);

                _builder.AddFilter(splitFilter);
            }

            public bool IsDelegated => _builder.IsDelegated;
            public bool IsImplemented => _builder.IsImplemented;

            public ISpecificationPipeBuilder<ConsumeContext<T>> CreateDelegatedBuilder()
            {
                return new ChildSpecificationPipeBuilder<ConsumeContext<T>>(this, IsImplemented, true);
            }

            public ISpecificationPipeBuilder<ConsumeContext<T>> CreateImplementedBuilder()
            {
                return new ChildSpecificationPipeBuilder<ConsumeContext<T>>(this, true, IsDelegated);
            }

            ConsumeContext<TMessage> ContextProvider(ConsumeContext<TMessage> context, ConsumeContext<T> splitContext)
            {
                return context;
            }

            ConsumeContext<T> InputContextProvider(ConsumeContext<TMessage> context)
            {
                return context as ConsumeContext<T>;
            }
        }
    }
}
