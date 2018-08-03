using BaZic.Core.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides a set of methods designed to manage assemblies at runtime.
    /// </summary>
    internal class AssemblyManager : MarshalByRefObject, IDisposable
    {
        #region Fields & Constants

        private readonly ResolveEventHandler AssemblyResolveEventHandler;
        private readonly List<Assembly> _explicitLoadedAssemblies;

        private DirectoryInfo _assemblyResolutionDirectory = null;
        private Exception _exceptionThrown;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyManager"/> class.
        /// </summary>
        public AssemblyManager(object logger, object culture) // Public constructor is important here !
        {
            Logger.Instance = logger as Logger;
            Localization.LocalizationHelper.SetCurrentCulture(culture as CultureInfo, false);

            _explicitLoadedAssemblies = new List<Assembly>();

            AssemblyResolveEventHandler = (s, e) =>
            {
                return OnReflectionOnlyResolve(e, _assemblyResolutionDirectory);
            };

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolveEventHandler;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Dispatcher.CurrentDispatcher.UnhandledException += CurrentDispatcher_UnhandledException;
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~AssemblyManager()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Attempt to load the specified Assembly from its full name or location on the hard drive.
        /// </summary>
        /// <param name="assemblyByteArray">The assembly's full name or location on the hard drive</param>
        /// <param name="forReflectionPurpose">Defines whether the assembly must be load for reflection only or also execution.</param>
        /// <returns>If succeeded, returns the loaded assembly.</returns>
        internal void LoadAssembly(string assemblyPath, bool forReflectionPurpose)
        {
            var assemblies = GetAssembliesInternal();
            Assembly assembly = null;

            if (File.Exists(assemblyPath))
            {
                assembly = assemblies.SingleOrDefault(asm => string.Compare(asm.Location, assemblyPath, StringComparison.OrdinalIgnoreCase) == 0);
                if (assembly == null)
                {
                    if (forReflectionPurpose)
                    {
                        assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
                    }
                    else
                    {
                        assembly = Assembly.LoadFrom(assemblyPath);
                    }
                }
            }
            else
            {
                assembly = assemblies.SingleOrDefault(asm => string.Compare(asm.FullName, assemblyPath, StringComparison.OrdinalIgnoreCase) == 0);
                if (assembly == null)
                {
                    if (forReflectionPurpose)
                    {
                        assembly = Assembly.ReflectionOnlyLoad(assemblyPath);
                    }
                    else
                    {
                        assembly = Assembly.Load(assemblyPath);
                    }
                }
            }

            _explicitLoadedAssemblies.Add(assembly);
        }

        /// <summary>
        /// Attempt to load the specified Assembly.
        /// </summary>
        /// <param name="assemblyByteArray">A byte array that represents the assembly.</param>
        /// <returns>If succeeded, returns the loaded assembly.</returns>
        internal void LoadAssembly(byte[] assemblyByteArray)
        {
            _explicitLoadedAssemblies.Add(Assembly.Load(assemblyByteArray));
        }

        /// <summary>
        /// Gets the assemblies that have been loaded.
        /// </summary>
        /// <returns>Returns the assemblies that have been loaded.</returns>
        internal ReadOnlyCollection<AssemblyDetails> GetAssemblies()
        {
            var details = new List<AssemblyDetails>();

            foreach (var assembly in GetAssembliesInternal())
            {
                details.Add(new AssemblyDetails
                {
                    Culture = assembly.GetName().CultureName,
                    Version = assembly.GetName().Version.ToString(),
                    FullName = assembly.FullName,
                    Location = assembly.Location,
                    Name = assembly.GetName().Name,
                    ProcessorArchitecture = assembly.GetName().ProcessorArchitecture
                });
            }

            return details.AsReadOnly();
        }

        /// <summary>
        /// Get the list of types in a loaded assembly.
        /// </summary>
        /// <param name="assemblyDetails">The assembly details.</param>
        /// <returns>Returns the list of types. Returns null if the assembly is not found.</returns>
        internal ReadOnlyCollection<TypeDetails> GetTypes(AssemblyDetails assemblyDetails)
        {
            var assemblies = GetAssembliesInternal();

            var assembly = assemblies.FirstOrDefault(a => assemblyDetails.Name == a.GetName().Name && assemblyDetails.Version == a.GetName().Version.ToString() && (string.IsNullOrEmpty(assemblyDetails.Culture) ? "neutral" : assemblyDetails.Culture) == (string.IsNullOrEmpty(a.GetName().CultureName) ? "neutral" : a.GetName().CultureName));

            if (assembly == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(assembly.Location))
            {
                _assemblyResolutionDirectory = null;
            }
            else
            {
                _assemblyResolutionDirectory = new FileInfo(assembly.Location).Directory;
            }

            var types = new ReadOnlyCollection<Type>(assembly.GetExportedTypes().ToList());

            var details = new List<TypeDetails>();

            foreach (var type in types)
            {
                details.Add(new TypeDetails
                {
                    Assembly = assemblyDetails,
                    Name = type.Name,
                    Namespace = type.Namespace,
                    IsClass = type.IsClass,
                    IsGenericType = type.IsGenericType,
                    IsGenericTypeDefinition = type.IsGenericTypeDefinition,
                    IsInterface = type.IsInterface,
                    IsPublic = type.IsPublic,
                    IsValueType = type.IsValueType,
                    IsStatic = type.IsAbstract && type.IsSealed,
                    IsAbstract = type.IsAbstract
                });
            }

            _assemblyResolutionDirectory = null;
            return details.AsReadOnly();
        }

        /// <summary>
        /// Get a reference to a type from a loaded assembly.
        /// </summary>
        /// <param name="fullName">The full name (namespace and class name) of the type.</param>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns>Returns the type if it has been found. Otherwise, throws a <see cref="TypeLoadException"/>.<returns>
        internal Type GetTypeRef(string fullName, string assemblyPath)
        {
            Type result = null;

            if (!string.IsNullOrWhiteSpace(assemblyPath))
            {
                result = Type.GetType($"{fullName}, {assemblyPath}", true, false);
            }
            else
            {
                var assemblies = GetAssembliesInternal();

                var i = 0;
                while (i < assemblies.Count && result == null)
                {
                    var assembly = assemblies[i];
                    if (string.IsNullOrWhiteSpace(assembly.Location))
                    {
                        _assemblyResolutionDirectory = null;
                    }
                    else
                    {
                        _assemblyResolutionDirectory = new FileInfo(assembly.Location).Directory;
                    }
                    result = assembly.GetTypes().SingleOrDefault(type => string.Compare(type.FullName, fullName, StringComparison.Ordinal) == 0);

                    i++;
                }
            }

            _assemblyResolutionDirectory = null;

            if (result == null)
            {
                throw new TypeLoadException($"Unable to load the type '{fullName}'. Does an assembly is missing?");
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of the specified class and invoke the given method with its arguments.
        /// </summary>
        /// <param name="fullName">The name of the class.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        /// <returns>Returns the result of the method.</returns>
        internal object CreateInstanceAndInvoke(string fullName, string methodName, object[] arguments)
        {
            var type = GetTypeRef(fullName, string.Empty);
            var instance = type.Assembly.CreateInstance(type.FullName);
            var method = type.GetRuntimeMethods().SingleOrDefault(m => string.Compare(m.Name, methodName, StringComparison.Ordinal) == 0);

            var methodResult = method.Invoke(instance, arguments);

            if (_exceptionThrown != null)
            {
                throw _exceptionThrown;
            }
            return methodResult;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Gets the assemblies that have been explicitely loaded.
        /// </summary>
        /// <returns>Returns the assemblies that have been loaded.</returns>
        private ReadOnlyCollection<Assembly> GetAssembliesInternal()
        {
            lock (_explicitLoadedAssemblies)
            {
                return _explicitLoadedAssemblies
                .Distinct()
                .ToList().AsReadOnly();
            }
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
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= AssemblyResolveEventHandler;
                }
            }

            IsDisposed = true;
        }

        #endregion

        #region Handled Methods

        /// <summary>
        /// Occurs when an exception is not caught.
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _exceptionThrown = e.ExceptionObject as Exception;
        }

        /// <summary>
        /// Occurs when a thread exception is thrown and uncaught during execution of a delegate by way of Overload:System.Windows.Threading.Dispatcher.Invoke or Overload:System.Windows.Threading.Dispatcher.BeginInvoke.
        /// </summary>
        private void CurrentDispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _exceptionThrown = e.Exception;
            e.Handled = true;
        }

        /// <summary>
        /// Attempts ReflectionOnlyLoad of current Assemblies dependants
        /// </summary>
        /// <param name="args">ReflectionOnlyAssemblyResolve event args</param>
        /// <param name="directory">The current Assemblies Directory</param>
        /// <returns>ReflectionOnlyLoadFrom loaded dependant Assembly</returns>
        private Assembly OnReflectionOnlyResolve(ResolveEventArgs args, DirectoryInfo directory)
        {
            var loadedAssembly = GetAssembliesInternal().FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            var assemblyName = new AssemblyName(args.Name);

            if (directory != null)
            {
                var dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

                if (File.Exists(dependentAssemblyFilename))
                {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);
                    _explicitLoadedAssemblies.Add(assembly);
                    return assembly;
                }
            }

            var assembly2 = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
            _explicitLoadedAssemblies.Add(assembly2);
            return assembly2;
        }

        #endregion
    }
}
