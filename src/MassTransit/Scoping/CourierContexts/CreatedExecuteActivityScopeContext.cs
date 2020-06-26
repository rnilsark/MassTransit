﻿namespace MassTransit.Scoping.CourierContexts
{
    using System;
    using Courier;


    public class CreatedExecuteActivityScopeContext<TScope, TActivity, TArguments> :
        IExecuteActivityScopeContext<TActivity, TArguments>
        where TScope : IDisposable
        where TActivity : class, IExecuteActivity<TArguments>
        where TArguments : class
    {
        readonly Action<TActivity> _disposeCallback;
        readonly TScope _scope;

        public CreatedExecuteActivityScopeContext(TScope scope, ExecuteActivityContext<TActivity, TArguments> context, Action<TActivity> disposeCallback = null)
        {
            _scope = scope;
            _disposeCallback = disposeCallback;
            Context = context;
        }

        public void Dispose()
        {
            _disposeCallback?.Invoke(Context.Activity);
            _scope.Dispose();
        }

        public ExecuteActivityContext<TActivity, TArguments> Context { get; }
    }
}
