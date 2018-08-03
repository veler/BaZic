using BaZic.Core.ComponentModel.Comparers;
using BaZic.Core.Exceptions;
using BaZic.Core.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Reflection
{
    /// <summary>
    /// Provides a fast way to get or set dynamically a method.
    /// </summary>
    internal sealed class FastMethodReflection : MarshalByRefObject, IDisposable
    {
        #region Fields & Constants

        private readonly Dictionary<MethodInfo, ReturnMethodInvocatorDelegate> _methods;
        private readonly Type _baseType;
        private readonly Lazy<MethodInfo[]> _methodInfos;

        #endregion

        #region Delegates

        private delegate object ReturnMethodInvocatorDelegate(object instance, params object[] args);
        private delegate void VoidMethodInvocatorDelegate(object instance, params object[] args);

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FastMethodReflection"/> class.
        /// </summary>
        /// <param name="baseType">The type of class that contains the method that will be invoked.</param>
        internal FastMethodReflection(Type baseType)
        {
            Requires.NotNull(baseType, nameof(baseType));
            _baseType = baseType;
            _methodInfos = new Lazy<MethodInfo[]>(() => _baseType.GetMethods(Consts.LimitedBindingFlags));

            _methods = new Dictionary<MethodInfo, ReturnMethodInvocatorDelegate>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Dispose()
        {
            IsDisposed = true;
            _methods.Clear();
        }

        /// <summary>
        /// Invoke the specified method.
        /// </summary>
        /// <param name="instance">The instance of a value of the same type than <seealso cref="_baseType"/>.</param>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="arguments">The arguments to send to the method.</param>
        /// <returns>Returns the result of the method invocation.</returns>
        internal object Invoke(object instance, string methodName, params object[] arguments)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastMethodReflection));
            }

            Requires.NotNull(instance, nameof(instance));

            lock (this)
            {
                if (instance.GetType() != _baseType)
                {
                    throw new BadTypeException(L.Reflection.FastMethodReflection.FormattedBadInstanceType(_baseType.FullName));
                }

                Requires.NotNullOrWhiteSpace(methodName, nameof(methodName));

                var argumentTypes = arguments.Select(arg => arg.GetType()).ToArray();

                var methodInfo = _methods.Select(met => met.Key).SingleOrDefault(met => string.Compare(met.Name, methodName, StringComparison.Ordinal) == 0 && met.GetParameters().Select(p => p.ParameterType).ToArray().SequenceEqual(argumentTypes, new TypeAssignableComparer()));

                if (methodInfo == null)
                {
                    var method = GetMethodDelegate(methodName, argumentTypes);
                    _methods.Add(method.Item1, method.Item2);
                    methodInfo = method.Item1;
                }

                return _methods[methodInfo](instance, arguments);
            }
        }

        /// <summary>
        /// Invoke the specified method.
        /// </summary>
        /// <param name="methodName">The name of the method to invoke.</param>
        /// <param name="arguments">The arguments to send to the method.</param>
        /// <returns>Returns the result of the method invocation.</returns>
        internal object InvokeStatic(string methodName, params object[] arguments)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastMethodReflection));
            }

            Requires.NotNullOrWhiteSpace(methodName, nameof(methodName));

            lock (this)
            {
                var argumentTypes = arguments.Select(arg => arg.GetType()).ToArray();

                var methodInfo = _methods.Select(met => met.Key).SingleOrDefault(met => string.Compare(met.Name, methodName, StringComparison.Ordinal) == 0 && met.GetParameters().Select(p => p.ParameterType).ToArray().SequenceEqual(argumentTypes, new TypeAssignableComparer()));

                if (methodInfo == null)
                {
                    var method = GetMethodDelegate(methodName, argumentTypes);
                    _methods.Add(method.Item1, method.Item2);
                    methodInfo = method.Item1;
                }

                return _methods[methodInfo](null, arguments);
            }
        }

        /// <summary>
        /// Creates a delegate designed to call a method that takes the specified list of arguments type.
        /// </summary>
        /// <param name="methodName">The name of the method to find.</param>
        /// <param name="argumentTypes">The list of argument types the method must take.</param>
        /// <returns>Returns a delegate or throw an error.</returns>
        private Tuple<MethodInfo, ReturnMethodInvocatorDelegate> GetMethodDelegate(string methodName, Type[] argumentTypes)
        {
            var methodInfos = _methodInfos.Value.Where(met => string.Compare(met.Name, methodName, StringComparison.Ordinal) == 0);

            if (!methodInfos.Any())
            {
                throw new TypeLoadException(L.Reflection.FastMethodReflection.FormattedMethodNotFound(methodName, _baseType.FullName));
            }

            var methodInfo = methodInfos.FirstOrDefault(met => met.GetParameters().Select(p => p.ParameterType).ToArray().SequenceEqual(argumentTypes, new TypeAssignableComparer()));

            if (methodInfo == null)
            {
                var argumentsTypesString = string.Join(", ", argumentTypes.Select(ar => ar.Name));
                throw new TypeLoadException(L.Reflection.FastMethodReflection.FormattedNoMethodMatchArguments(methodName, _baseType.FullName, argumentsTypesString));
            }

            var paramsInfo = methodInfo.GetParameters();

            // Create a param "instance"
            var instanceExpression = Expression.Parameter(typeof(object), "instance");

            // Create a single param of type object[].
            var param = Expression.Parameter(typeof(object[]), "args");

            // Pick each arg from the params array and create a typed expression of them.
            var argsExpressions = new Expression[paramsInfo.Length];

            for (var i = 0; i < paramsInfo.Length; i++)
            {
                var index = Expression.Constant(i);
                var paramType = paramsInfo[i].ParameterType;
                var paramAccessorExp = Expression.ArrayIndex(param, index);
                var paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExpressions[i] = paramCastExp;
            }

            UnaryExpression callerInstance = null;
            if (!methodInfo.IsStatic)
            {
                callerInstance = Expression.Convert(instanceExpression, methodInfo.ReflectedType);
            }

            var callExpression = Expression.Call(callerInstance, methodInfo, argsExpressions);

            if (callExpression.Type == typeof(void))
            {
                var voidDelegate = Expression.Lambda<VoidMethodInvocatorDelegate>(callExpression, instanceExpression, param).Compile();
                return new Tuple<MethodInfo, ReturnMethodInvocatorDelegate>(methodInfo, (instance, arguments) =>
                {
                    voidDelegate(instance, arguments);
                    return null;
                });
            }
            else
            {
                return new Tuple<MethodInfo, ReturnMethodInvocatorDelegate>(methodInfo, Expression.Lambda<ReturnMethodInvocatorDelegate>(Expression.Convert(callExpression, typeof(object)), instanceExpression, param).Compile());
            }
        }

        #endregion
    }
}
