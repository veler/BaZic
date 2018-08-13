using BaZic.Core.ComponentModel.Comparers;
using BaZic.Core.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace BaZic.Core.ComponentModel.Reflection
{
    /// <summary>
    /// Provides a fast way to instantiate dynamically a class.
    /// </summary>
    internal sealed class FastInstantiateReflection : MarshalByRefObject, IDisposable
    {
        #region Fields & Constants

        private readonly Lazy<MethodInfo> _instantiateDefaultConstructor = new Lazy<MethodInfo>(() => typeof(FastInstantiateReflection).GetMethod(nameof(InstantiateDefaultConstructor), BindingFlags.Instance | BindingFlags.NonPublic));
        private readonly Dictionary<Type[], TypeCreatorDelegate> _constructors;
        private readonly Type _createType;

        #endregion

        #region Delegates

        private delegate object TypeCreatorDelegate(params object[] args);

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FastInstantiateReflection"/> class.
        /// </summary>
        /// <param name="createType">The type of class to instantiate.</param>
        internal FastInstantiateReflection(Type createType)
        {
            Requires.NotNull(createType, nameof(createType));
            _createType = createType;

            _constructors = new Dictionary<Type[], TypeCreatorDelegate>(new TypeArrayComparer());
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Dispose()
        {
            IsDisposed = true;
            _constructors.Clear();
        }

        /// <summary>
        /// Creates a new instance of a class.
        /// </summary>
        /// <param name="arguments">The arguments to send to the constructor.</param>
        /// <returns>Returns a new instance of the specified class, or throw an exception if it is not possible.</returns>
        internal object Instantiate(params object[] arguments)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(FastInstantiateReflection));
            }

            if (arguments == null || arguments.Length == 0)
            {
                return _instantiateDefaultConstructor.Value.MakeGenericMethod(_createType).Invoke(this, null);
            }

            TypeCreatorDelegate constructor = null;

            lock (_constructors)
            {
                var argumentTypes = arguments.Select(arg => arg.GetType()).ToArray();
                _constructors.TryGetValue(argumentTypes, out constructor);
                if (constructor == null)
                {
                    constructor = GetConstructorDelegate(argumentTypes);
                    _constructors.Add(argumentTypes, constructor);
                }
            }

            return constructor(arguments);
        }

        /// <summary>
        /// Creates a delegate designed to instantiate a class with a specific list of arguments.
        /// </summary>
        /// <param name="argumentTypes">The list of argument types the constructor must take.</param>
        /// <returns>Returns a delegate or throw an error.</returns>
        private TypeCreatorDelegate GetConstructorDelegate(Type[] argumentTypes)
        {
            var constructors = _createType.GetConstructors(Consts.LimitedBindingFlags);

            var constructor = constructors.SingleOrDefault(constr => constr.GetParameters().Select(p => p.ParameterType).ToArray().SequenceEqual(argumentTypes));

            if (constructor == null)
            {
                var argumentsTypesString = string.Join(", ", argumentTypes.Select(ar => ar.Name));
                throw new TypeLoadException(L.Reflection.FormattedFastInstantiateReflectionNoConstructorFound(_createType.FullName, argumentsTypesString));
            }

            var paramsInfo = constructor.GetParameters();

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

            // Make a NewExpression that calls the constructor with the args we just created.
            var newExpression = Expression.New(constructor, argsExpressions);

            // Create a lambda with the NewExpression as body and our param object[] as arg.
            var lambda = Expression.Lambda(typeof(TypeCreatorDelegate), newExpression, param);

            // Compile it
            return (TypeCreatorDelegate)lambda.Compile();
        }

        /// <summary>
        /// Creates a new instance of a class by using the default constructor.
        /// </summary>
        /// <typeparam name="T">The class to instantiate.</typeparam>
        /// <returns>Returns a new instance of <typeparamref name="T"/>.</returns>
        private T InstantiateDefaultConstructor<T>()
        {
            var type = typeof(T);
            if (type == typeof(string))
            {
                return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile()();
            }

            if (HasDefaultConstructor(type))
            {
                return Expression.Lambda<Func<T>>(Expression.New(type)).Compile()();
            }

            // Creates an instance without calling the constructor.
            return (T)FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// Determines whether a type has a default constructor.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>Returns True if the type has a default constructor with no argument.</returns>
        private bool HasDefaultConstructor(Type type)
        {
            return type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        #endregion
    }
}
