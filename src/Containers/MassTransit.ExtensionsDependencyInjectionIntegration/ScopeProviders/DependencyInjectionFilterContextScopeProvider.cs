namespace MassTransit.ExtensionsDependencyInjectionIntegration.ScopeProviders
{
    using System;
    using GreenPipes;
    using Microsoft.Extensions.DependencyInjection;
    using Scoping.Filters;


    public class DependencyInjectionFilterContextScopeProvider<TFilter, TContext> :
        IFilterContextScopeProvider<TContext>
        where TFilter : IFilter<TContext>
        where TContext : class, PipeContext
    {
        readonly IServiceProvider _serviceProvider;

        public DependencyInjectionFilterContextScopeProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IFilterContextScope<TContext> Create(TContext context)
        {
            var scope = new DependencyInjectionFilterContextScope(context, _serviceProvider);
            return scope;
        }


        class DependencyInjectionFilterContextScope :
            IFilterContextScope<TContext>
        {
            readonly IServiceScope _scope;

            public DependencyInjectionFilterContextScope(TContext context, IServiceProvider serviceProvider)
            {
                Context = context;
                _scope = context.TryGetPayload(out IServiceProvider provider) ? new NoopScope(provider) : serviceProvider.CreateScope();
            }

            public void Dispose()
            {
                _scope.Dispose();
            }

            public IFilter<TContext> Filter => _scope.ServiceProvider.GetService<TFilter>();

            public TContext Context { get; }


            class NoopScope :
                IServiceScope
            {
                public NoopScope(IServiceProvider serviceProvider)
                {
                    ServiceProvider = serviceProvider;
                }

                public void Dispose()
                {
                }

                public IServiceProvider ServiceProvider { get; }
            }
        }
    }
}
