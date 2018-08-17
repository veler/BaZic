using BaZic.Core.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Reflection
{
    /// <summary>
    /// Provides a fast way to register or unregister dynamically to an event.
    /// </summary>
    internal sealed class EventReflection : MarshalByRefObject
    {
        #region Fields & Constants

        private readonly List<Tuple<object, EventInfo, Delegate>> _events = new List<Tuple<object, EventInfo, Delegate>>();
        
        #endregion

        #region Methods

        /// <summary>
        /// Register an action to an event of an object.
        /// </summary>
        /// <param name="targetObject">The object that contains the event.</param>
        /// <param name="eventName">The name of the event in the <paramref name="targetObject"/>.</param>
        /// <param name="action">The action to run when the event is raised.</param>
        internal void Subscribe(object targetObject, string eventName, Action action)
        {
            Requires.NotNull(targetObject, nameof(targetObject));
            Requires.NotNullOrWhiteSpace(eventName, nameof(eventName));

            var eventInfo = targetObject.GetType().GetEvent(eventName, Consts.LimitedBindingFlags);

            if (eventInfo == null)
            {
                throw new MemberAccessException(L.Reflection.FormattedEventReflectionAccess(eventName, targetObject.GetType().FullName));
            }

            var raiseDelegate = GetEventMethodDelegate(eventInfo, action);

            var eventItem = new Tuple<object, EventInfo, Delegate>(targetObject, eventInfo, raiseDelegate);
            _events.Add(eventItem);

            eventItem.Item2.AddEventHandler(eventItem.Item1, eventItem.Item3);
        }

        /// <summary>
        /// Register an action to a static event of a class.
        /// </summary>
        /// <param name="targetType">The object that contains the event.</param>
        /// <param name="eventName">The name of the event in the <paramref name="targetType"/>.</param>
        /// <param name="action">The action to run when the event is raised.</param>
        internal void SubscribeStatic(Type targetType, string eventName, Action action)
        {
            Requires.NotNull(targetType, nameof(targetType));
            Requires.NotNullOrWhiteSpace(eventName, nameof(eventName));

            var eventInfo = targetType.GetEvent(eventName, Consts.LimitedBindingFlags);

            if (eventInfo == null)
            {
                throw new MemberAccessException(L.Reflection.FormattedEventReflectionAccess(eventName, targetType.FullName));
            }

            var raiseDelegate = GetEventMethodDelegate(eventInfo, action);

            var eventItem = new Tuple<object, EventInfo, Delegate>(null, eventInfo, raiseDelegate);
            _events.Add(eventItem);

            eventItem.Item2.AddEventHandler(eventItem.Item1, eventItem.Item3);
        }

        /// <summary>
        /// Unregister all the registered events.
        /// </summary>
        internal void UnsubscribeAll()
        {
            foreach (var eventItem in _events)
            {
                eventItem.Item2.RemoveEventHandler(eventItem.Item1, eventItem.Item3);
            }

            _events.Clear();
        }

        /// <summary>
        /// Creates a delegate designed to call a method that takes the specified list of arguments type.
        /// </summary>
        /// <param name="eventInfo">The information on the event.</param>
        /// <param name="action">The action to run.</param>
        /// <returns>Returns a delegate or throw an error.</returns>
        private Delegate GetEventMethodDelegate(EventInfo eventInfo, Action action)
        {
            var handlerType = eventInfo.EventHandlerType;
            var eventParams = handlerType.GetMethod("Invoke").GetParameters();

            var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, "x"));
            var body = Expression.Call(Expression.Constant(action), action.GetType().GetMethod("Invoke"));
            var lambda = Expression.Lambda(body, parameters.ToArray());
            return Delegate.CreateDelegate(handlerType, lambda.Compile(), "Invoke", false);
        }

        #endregion
    }
}
