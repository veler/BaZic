﻿using BaZic.Core.ComponentModel.Reflection;
using BaZic.Core.Logs;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides a closed sandbox where assemblies can be loaded an exploited.
    /// </summary>
    [Serializable]
    public sealed class AssemblySandbox : IDisposable
    {
        #region Fields & Constants

        private readonly AppDomain _appDomain;
        private readonly AssemblyManager _assemblyManager;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the reflection helper.
        /// </summary>
        public FastReflection Reflection { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySandbox"/> class.
        /// </summary>
        public AssemblySandbox()
        {
            _appDomain = AppDomainManager.CreateUniqueAppDomain();

            _assemblyManager = CreateInstanceMarshalByRefObject<AssemblyManager>(Logger.Instance, Localization.LocalizationHelper.GetCurrentCulture());
            Reflection = CreateInstanceMarshalByRefObject<FastReflection>(this);
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~AssemblySandbox()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attempt to load the specified Assembly from its full name or location on the hard drive.
        /// </summary>
        /// <param name="assemblyDetails">The assembly's information</param>
        /// <param name="forReflectionPurpose">(optional) Defines whether the assembly must be load for reflection only or also execution. By default, the value is true.</param>
        public void LoadAssembly(AssemblyDetails assemblyDetails, bool forReflectionPurpose = true)
        {
            Requires.NotNull(assemblyDetails, nameof(assemblyDetails));
            _assemblyManager.LoadAssembly(assemblyDetails, forReflectionPurpose);
        }

        /// <summary>
        /// Attempt to load the specified Assembly from its full name or location on the hard drive.
        /// </summary>
        /// <param name="assemblyPath">The assembly's full name or location on the hard drive</param>
        /// <param name="forReflectionPurpose">(optional) Defines whether the assembly must be load for reflection only or also execution. By default, the value is true.</param>
        public void LoadAssembly(string assemblyPath, bool forReflectionPurpose = true)
        {
            Requires.NotNullOrWhiteSpace(assemblyPath, nameof(assemblyPath));
            _assemblyManager.LoadAssembly(AssemblyInfoHelper.GetAssemblyDetailsFromName(assemblyPath), forReflectionPurpose);
        }

        /// <summary>
        /// Attempt to load the specified Assembly from its full name or location on the hard drive.
        /// </summary>
        /// <param name="assemblyStream">The assembly stream</param>
        public void LoadAssembly(MemoryStream assemblyStream)
        {
            Requires.NotNull(assemblyStream, nameof(assemblyStream));
            assemblyStream.Seek(0, SeekOrigin.Begin);

            _assemblyManager.LoadAssembly(assemblyStream.ToArray());
        }

        /// <summary>
        /// Gets the assemblies that have been loaded.
        /// </summary>
        /// <returns>Returns the assemblies that have been loaded.</returns>
        public ReadOnlyCollection<AssemblyDetails> GetAssemblies()
        {
            return _assemblyManager.GetAssemblies();
        }

        /// <summary>
        /// Get the list of types in a loaded assembly.
        /// </summary>
        /// <param name="assemblyDetails">The assembly details.</param>
        /// <returns>Returns the list of types. Returns null if the assembly is not found.</returns>
        public ReadOnlyCollection<TypeDetails> GetTypes(AssemblyDetails assemblyDetails)
        {
            Requires.NotNull(assemblyDetails, nameof(assemblyDetails));
            return _assemblyManager.GetTypes(assemblyDetails);
        }

        /// <summary>
        /// Get a reference to a type from a loaded assembly.
        /// </summary>
        /// <param name="fullName">The full name (namespace and class name) of the type.</param>
        /// <param name="assemblyPath">(optional) The assembly path.</param>
        /// <returns>Returns the type if it has been found. Otherwise, throws a <see cref="TypeLoadException"/>.<returns>
        public Type GetTypeRef(string fullName, string assemblyPath = "")
        {
            Requires.NotNullOrWhiteSpace(fullName, nameof(fullName));
            return _assemblyManager.GetTypeRef(fullName, assemblyPath);
        }

        /// <summary>
        /// Creates an instance of the specified class and invoke the given method with its arguments.
        /// </summary>
        /// <param name="fullName">The name of the class.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        /// <returns>Returns the result of the method.</returns>
        public object CreateInstanceAndInvoke(string fullName, string methodName, object[] arguments)
        {
            Requires.NotNullOrWhiteSpace(fullName, nameof(fullName));
            Requires.NotNullOrWhiteSpace(methodName, nameof(methodName));
            return _assemblyManager.CreateInstanceAndInvoke(fullName, methodName, arguments);
        }

        /// <summary>
        /// Creates the instance marshal by reference object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public T CreateInstanceMarshalByRefObject<T>(params object[] args) where T : MarshalByRefObject
        {
            var flags = System.Reflection.BindingFlags.CreateInstance | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
            var type = typeof(T);
            return (T)_appDomain.CreateInstanceFromAndUnwrap(type.Assembly.Location, type.FullName, false, flags, null, args, null, null);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Should be called when the object is being disposed.
        /// </summary>
        /// <param name="disposing">Was Dispose() called or did we get here from the finalizer?</param>
        private void OnDispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsDisposed)
                {
                    Reflection.Dispose();
                    _assemblyManager.Dispose();
                    AppDomainManager.UnloadAppDomain(_appDomain);
                }
            }

            IsDisposed = true;
        }

        #endregion
    }
}
