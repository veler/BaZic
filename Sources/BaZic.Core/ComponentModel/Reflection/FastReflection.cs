using BaZic.Core.ComponentModel.Assemblies;
using BaZic.Core.IO.Serialization;
using System;
using System.Collections.Generic;

namespace BaZic.Core.ComponentModel.Reflection
{
    /// <summary>
    /// Provides a set of methods and properties designed to help to improve the performances of works that use reflection in the interpreter.
    /// </summary>
    public sealed class FastReflection : MarshalByRefObject, IDisposable
    {
        #region Fields & Constants

        /// <summary>
        /// Dictionary of types, that contains a list of property name associated to a <see cref="FastPropertyReflection"/>.
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, FastPropertyReflection>> _properties = new Dictionary<Type, Dictionary<string, FastPropertyReflection>>();

        private readonly Dictionary<Type, FastInstantiateReflection> _constructors = new Dictionary<Type, FastInstantiateReflection>();
        private readonly Dictionary<Type, FastMethodReflection> _methods = new Dictionary<Type, FastMethodReflection>();
        private readonly EventReflection _eventReflection = new EventReflection();

        private AssemblySandbox _assemblySandbox;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FastReflection"/> class.
        /// </summary>
        public FastReflection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastReflection"/> class.
        /// </summary>
        /// <param name="assemblySandbox">The assembly sandbox.</param>
        public FastReflection(AssemblySandbox assemblySandbox)
        {
            Requires.NotNull(assemblySandbox, nameof(assemblySandbox));
            _assemblySandbox = assemblySandbox;
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~FastReflection()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Dispose()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Get a reference to a type from a loaded assembly.
        /// </summary>
        /// <param name="fullName">The full name (namespace and class name) of the type.</param>
        /// <returns>Returns the type if it has been found. Otherwise, throws a <see cref="TypeLoadException"/>.<returns>
        public Type GetTypeRef(string fullName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            Requires.NotNull(_assemblySandbox, nameof(_assemblySandbox));

            return _assemblySandbox.GetTypeRef(fullName);
        }

        /// <summary>
        /// Register an action to an event of an object.
        /// </summary>
        /// <param name="targetObject">The object that contains the event.</param>
        /// <param name="eventName">The name of the event in the <paramref name="targetObject"/>.</param>
        /// <param name="action">The action to run when the event is raised.</param>
        public void SubscribeEvent(object targetObject, string eventName, Action action)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            _eventReflection.Subscribe(targetObject, eventName, action);
        }

        /// <summary>
        /// Register an action to a event of a class.
        /// </summary>
        /// <param name="targetType">The type that contains the event.</param>
        /// <param name="eventName">The name of the event in the <paramref name="targetType"/>.</param>
        /// <param name="action">The action to run when the event is raised.</param>
        public void SubscribeStaticEvent(Type targetType, string eventName, Action action)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            _eventReflection.SubscribeStatic(targetType, eventName, action);
        }

        /// <summary>
        /// Register an action to a event of a class.
        /// </summary>
        /// <param name="targetTypeFullName">The full name of the type that contains the event.</param>
        /// <param name="eventName">The name of the event in the <paramref name="targetTypeFullName"/>.</param>
        /// <param name="action">The action to run when the event is raised.</param>
        public void SubscribeStaticEvent(string targetTypeFullName, string eventName, Action action)
        {
            var targetType = _assemblySandbox.GetTypeRef(targetTypeFullName);
            SubscribeStaticEvent(targetType, eventName, action);
        }

        /// <summary>
        /// Unregister all the registered events.
        /// </summary>
        public void UnsubscribeAllEvents()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            _eventReflection.UnsubscribeAll();
        }

        /// <summary>
        /// Creates a new instance of a class by finding its corresponding constructor.
        /// </summary>
        /// <param name="createType">The type of the class to instantiate.</param>
        /// <param name="arguments">The list of arguments to send to the class constructor.</param>
        /// <returns>Returns a new instance of the specified class.</returns>
        public object Instantiate(Type createType, params object[] arguments)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            FastInstantiateReflection constructor = null;

            lock (_constructors)
            {
                _constructors.TryGetValue(createType, out constructor);
                if (constructor == null)
                {
                    constructor = new FastInstantiateReflection(createType);
                    _constructors.Add(createType, constructor);
                }
            }

            return constructor.Instantiate(arguments);
        }

        /// <summary>
        /// Invoke a method by using a fast reflection way.
        /// </summary>
        /// <param name="targetObject">The object that contains the method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The list of arguments to send to the method.</param>
        /// <returns>Returns the value returned by the method. If it's a void, returns a <see cref="NotAssignable"/> value.</returns>
        public object InvokeMethod(object targetObject, string methodName, params object[] arguments)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            Requires.NotNull(targetObject, nameof(targetObject));
            Requires.NotNullOrWhiteSpace(methodName, nameof(methodName));

            FastMethodReflection method = null;

            var type = targetObject.GetType();
            lock (_methods)
            {
                _methods.TryGetValue(type, out method);
                if (method == null)
                {
                    method = new FastMethodReflection(type);
                    _methods.Add(type, method);
                }
            }

            return method.Invoke(targetObject, methodName, arguments);
        }

        /// <summary>
        /// Invoke a static method of a class by using a fast reflection way.
        /// </summary>
        /// <param name="targetType">The type that contains the static method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The list of arguments to send to the method.</param>
        /// <returns>Returns the value returned by the method. If it's a void, returns a <see cref="NotAssignable"/> value.</returns>
        public object InvokeStaticMethod(Type targetType, string methodName, params object[] arguments)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            Requires.NotNull(targetType, nameof(targetType));
            Requires.NotNullOrWhiteSpace(methodName, nameof(methodName));

            FastMethodReflection method = null;

            lock (_methods)
            {
                _methods.TryGetValue(targetType, out method);
                if (method == null)
                {
                    method = new FastMethodReflection(targetType);
                    _methods.Add(targetType, method);
                }
            }

            return method.InvokeStatic(methodName, arguments);
        }

        /// <summary>
        /// Invoke a static method of a class by using a fast reflection way.
        /// </summary>
        /// <param name="targetTypeFullName">The full name of the type that contains the static method.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The list of arguments to send to the method.</param>
        /// <returns>Returns the value returned by the method. If it's a void, returns a <see cref="NotAssignable"/> value.</returns>
        public object InvokeStaticMethod(string targetTypeFullName, string methodName, params object[] arguments)
        {
            var targetType = GetTypeRef(targetTypeFullName);

            return InvokeStaticMethod(targetType, methodName, arguments);
        }

        /// <summary>
        /// Determines whether a property of an object has a getter and a setter.
        /// </summary>
        /// <param name="targetObject">The object that contains the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Returns True if the property has a getter and a setter.</returns>
        public bool PropertyHasGetterAndSetter(object targetObject, string propertyName)
        {
            Requires.NotNull(targetObject, nameof(targetObject));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            return GetProperty(targetObject.GetType(), propertyName).HasGetterAndSetter;
        }

        /// <summary>
        /// Gets the value of a property of an object by using a fast reflection way.
        /// </summary>
        /// <param name="targetObject">The object that contains the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Returns the value of the property.</returns>
        public object GetProperty(object targetObject, string propertyName)
        {
            Requires.NotNull(targetObject, nameof(targetObject));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            return GetProperty(targetObject.GetType(), propertyName).Get(targetObject);
        }

        /// <summary>
        /// Gets the binary serialized value of a property of an object by using a fast reflection way.
        /// </summary>
        /// <param name="targetObject">The object that contains the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Returns the value of the property as an array of byte.</returns>
        public byte[] GetPropertySerialized(object targetObject, string propertyName)
        {
            Requires.NotNull(targetObject, nameof(targetObject));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            var propertyValue = GetProperty(targetObject.GetType(), propertyName).Get(targetObject);
            return SerializationHelper.ConvertToBinary(propertyValue);
        }

        /// <summary>
        /// Gets the value of a static property of a class by using a fast reflection way.
        /// </summary>
        /// <param name="targetType">The type that contains the static property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Returns the value of the property.</returns>
        public object GetStaticProperty(Type targetType, string propertyName)
        {
            Requires.NotNull(targetType, nameof(targetType));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            return GetProperty(targetType, propertyName).Get(null);
        }


        /// <summary>
        /// Gets the value of a static property of a class by using a fast reflection way.
        /// </summary>
        /// <param name="targetTypeFullName">The full name of the type that contains the static property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>Returns the value of the property.</returns>
        public object GetStaticProperty(string targetTypeFullName, string propertyName)
        {
            var targetType = GetTypeRef(targetTypeFullName);
            return GetStaticProperty(targetType, propertyName);
        }
        /// <summary>
        /// Sets the value of a property of an object by using a fast reflection way.
        /// </summary>
        /// <param name="targetObject">The object that contains the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to give to the property.</param>
        public void SetProperty(object targetObject, string propertyName, object value)
        {
            Requires.NotNull(targetObject, nameof(targetObject));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            GetProperty(targetObject.GetType(), propertyName).Set(targetObject, value);
        }

        /// <summary>
        /// Sets the value of a static property of a class by using a fast reflection way.
        /// </summary>
        /// <param name="targetType">The type that contains the static property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to give to the property.</param>
        public void SetStaticProperty(Type targetType, string propertyName, object value)
        {
            Requires.NotNull(targetType, nameof(targetType));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            GetProperty(targetType, propertyName).Set(null, value);
        }

        /// <summary>
        /// Retrieves or add a <see cref="FastPropertyReflection"/> from/to the poperties cache.
        /// </summary>
        /// <param name="type">The type that contains the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>A <see cref="FastPropertyReflection"/> that can be use to get or set the property.</returns>
        private FastPropertyReflection GetProperty(Type type, string propertyName)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastReflection));
            }

            FastPropertyReflection property = null;

            lock (_properties)
            {
                _properties.TryGetValue(type, out Dictionary<string, FastPropertyReflection> typeProperties);
                if (typeProperties == null)
                {
                    typeProperties = new Dictionary<string, FastPropertyReflection>();
                    _properties.Add(type, typeProperties);
                }

                typeProperties.TryGetValue(propertyName, out property);
                if (property == null)
                {
                    property = new FastPropertyReflection(type, propertyName);
                    typeProperties.Add(propertyName, property);
                }
            }

            return property;
        }

        /// <summary>
        /// Should be called when the object is being disposed.
        /// </summary>
        /// <param name="disposing">Was Dispose() called or did we get here from the finalizer?</param>
        private void OnDispose(bool disposing)
        {
            if (disposing && !IsDisposed)
            {
                _eventReflection.UnsubscribeAll();

                _properties.Clear();

                foreach (var constuctor in _constructors)
                {
                    constuctor.Value.Dispose();
                }
                _constructors.Clear();

                foreach (var method in _methods)
                {
                    method.Value.Dispose();
                }
                _methods.Clear();
            }

            IsDisposed = true;
        }

        #endregion
    }
}
