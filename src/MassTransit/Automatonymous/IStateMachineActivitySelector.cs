﻿namespace Automatonymous
{
    using Binders;


    public interface IStateMachineActivitySelector<TInstance, TData>
        where TInstance : class, SagaStateMachineInstance
        where TData : class
    {
        /// <summary>
        /// An activity which accepts the instance and data from the event
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <returns></returns>
        EventActivityBinder<TInstance, TData> OfType<TActivity>()
            where TActivity : Activity<TInstance, TData>;

        /// <summary>
        /// An activity that only accepts the instance, and does not require the event data
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <returns></returns>
        EventActivityBinder<TInstance, TData> OfInstanceType<TActivity>()
            where TActivity : Activity<TInstance>;
    }


    public interface IStateMachineActivitySelector<TInstance>
        where TInstance : class, SagaStateMachineInstance
    {
        /// <summary>
        /// An activity which accepts the instance and data from the event
        /// </summary>
        /// <typeparam name="TActivity"></typeparam>
        /// <returns></returns>
        EventActivityBinder<TInstance> OfType<TActivity>()
            where TActivity : Activity<TInstance>;
    }
}
