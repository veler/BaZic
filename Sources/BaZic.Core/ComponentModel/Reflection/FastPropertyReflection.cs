using BaZic.Core.Localization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Reflection
{
    /// <summary>
    /// Provides a fast way to get or set dynamically a property.
    /// </summary>
    internal sealed class FastPropertyReflection : MarshalByRefObject
    {
        #region Fields & Constants

        private readonly PropertyInfo _propertyInfo;
        private readonly Func<object, object> _getDelegate;
        private readonly Action<object, object> _setDelegate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value that indicates whether a getter AND a setter are available.
        /// </summary>
        internal bool HasGetterAndSetter => _getDelegate != null && _setDelegate != null;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FastPropertyReflection"/> class.
        /// </summary>
        /// <param name="objectType">The type of the class that own the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        internal FastPropertyReflection(Type objectType, string propertyName)
        {
            Requires.NotNull(objectType, nameof(objectType));
            Requires.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            _propertyInfo = objectType.GetProperty(propertyName, Consts.LimitedBindingFlags);

            if (_propertyInfo == null)
            {
                throw new MemberAccessException(L.Reflection.FastPropertyReflection.FormattedNoAccess(propertyName, objectType.FullName));
            }

            var getMethod = _propertyInfo.GetGetMethod(false);
            var setMethod = _propertyInfo.GetSetMethod(false);

            if (getMethod != null)
            {
                var genericMethod = typeof(FastPropertyReflection).GetMethod(nameof(CreateGetterGeneric), BindingFlags.Instance | BindingFlags.NonPublic);
                var genericHelper = genericMethod.MakeGenericMethod(_propertyInfo.DeclaringType, _propertyInfo.PropertyType);
                _getDelegate = (Func<object, object>)genericHelper.Invoke(this, new object[] { getMethod });
            }

            if (setMethod != null)
            {
                var genericMethod = typeof(FastPropertyReflection).GetMethod(nameof(CreateSetterGeneric), BindingFlags.Instance | BindingFlags.NonPublic);
                var genericHelper = genericMethod.MakeGenericMethod(_propertyInfo.DeclaringType, _propertyInfo.PropertyType);
                _setDelegate = (Action<object, object>)genericHelper.Invoke(this, new object[] { setMethod });
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        /// <param name="targetObject">The object from where it should retrieves the value of the property.</param>
        /// <returns>Returns the value of the property or throw an exception if there is no accessible getter.</returns>
        internal object Get(object targetObject)
        {
            if (_getDelegate == null)
            {
                throw new MemberAccessException(L.Reflection.FastPropertyReflection.FormattedNoGetter(_propertyInfo.Name));
            }

            return _getDelegate(targetObject);
        }

        /// <summary>
        /// Sets the value of the property.
        /// </summary>
        /// <param name="targetObject">The object from where it should sets the value of the property.</param>
        /// <param name="value">The value to set to the property.</param>
        internal void Set(object targetObject, object value)
        {
            if (_setDelegate == null)
            {
                throw new MemberAccessException(L.Reflection.FastPropertyReflection.FormattedNoSetter(_propertyInfo.Name));
            }

            _setDelegate(targetObject, value);
        }

        /// <summary>
        /// Generates a generic access to a getter.
        /// </summary>
        /// <typeparam name="T">The type of the target object.</typeparam>
        /// <typeparam name="R">The type of the returned value.</typeparam>
        /// <param name="getter">The getter's method information.</param>
        /// <returns>Returns a function that can be use in a delegate.</returns>
        private Func<object, object> CreateGetterGeneric<T, R>(MethodInfo getter) where T : class
        {
            if (getter.IsStatic)
            {
                var types = Expression.GetDelegateType((from parameter in getter.GetParameters() select parameter.ParameterType).Concat(new[] { getter.ReturnType }).ToArray());
                var getterTypedDelegate = (Func<R>)Delegate.CreateDelegate(types, getter.DeclaringType, getter.Name);
                return (object instance) => getterTypedDelegate();
            }
            else
            {
                var getterTypedDelegate = (Func<T, R>)Delegate.CreateDelegate(typeof(Func<T, R>), getter);
                return (object instance) => getterTypedDelegate((T)instance);
            }
        }

        /// <summary>
        /// Generates a generic access to a setter.
        /// </summary>
        /// <typeparam name="T">The type of the target object.</typeparam>
        /// <typeparam name="V">The type of the value to set.</typeparam>
        /// <param name="setter">The setter's method information.</param>
        /// <returns>Returns an action that can be use in a delegate.</returns>
        private Action<object, object> CreateSetterGeneric<T, V>(MethodInfo setter) where T : class
        {
            if (setter.IsStatic)
            {
                var types = Expression.GetDelegateType((from parameter in setter.GetParameters() select parameter.ParameterType).Concat(new[] { setter.ReturnType }).ToArray());
                var setterTypedDelegate = (Action<V>)Delegate.CreateDelegate(types, setter.DeclaringType, setter.Name);
                return (object instance, object value) => { setterTypedDelegate((V)value); };
            }
            else
            {
                var setterTypedDelegate = (Action<T, V>)Delegate.CreateDelegate(typeof(Action<T, V>), setter);
                return (object instance, object value) => { setterTypedDelegate((T)instance, (V)value); };
            }
        }

        #endregion
    }
}
