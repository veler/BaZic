using BaZic.Core.Interop;
using BaZic.Core.Logs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides a set of methods designed to manage assemblies at runtime.
    /// </summary>
    internal class AssemblyManager : MarshalByRefObject, IDisposable
    {
        #region Fields & Constants

        private readonly ResolveEventHandler AssemblyReflectionOnlyResolveEventHandler;
        private readonly ResolveEventHandler AssemblyResolveEventHandler;
        private readonly List<LoadedAssemblyDetails> _explicitLoadedAssemblies;
        private readonly List<IntPtr> _win32ModuleHandlers;

        private string _currentResovlingAssembly;
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

            _explicitLoadedAssemblies = new List<LoadedAssemblyDetails>();
            _win32ModuleHandlers = new List<IntPtr>();

            AssemblyReflectionOnlyResolveEventHandler = (s, e) =>
            {
                var assembly = OnAssemblyResolve(e, _assemblyResolutionDirectory, isReflectionOnly: true);
                _currentResovlingAssembly = string.Empty;
                return assembly;
            };

            AssemblyResolveEventHandler = (s, e) =>
            {
                var assembly = OnAssemblyResolve(e, _assemblyResolutionDirectory, isReflectionOnly: false);
                _currentResovlingAssembly = string.Empty;
                return assembly;
            };

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyReflectionOnlyResolveEventHandler;
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveEventHandler;
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
        /// <param name="assemblyDetails">The assembly's informations</param>
        /// <param name="forReflectionPurpose">Defines whether the assembly must be load for reflection only or also execution.</param>
        internal void LoadAssembly(AssemblyDetails assemblyDetails, bool forReflectionPurpose)
        {
            if (!assemblyDetails.IsDotNetAssembly)
            {
                if (!forReflectionPurpose && File.Exists(assemblyDetails.Location))
                {
                    var handler = NativeMethods.LoadLibrary(assemblyDetails.Location);
                    if (handler == IntPtr.Zero)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Exception($"Failed to load library '{Path.GetFileName(assemblyDetails.Location)}' (ErrorCode: {errorCode})");
                    }

                    _win32ModuleHandlers.Add(handler);
                }
                return;
            }

            var assemblies = GetAssembliesInternal();
            var assemblyLoaded = false;
            Assembly assembly = null;

            if (File.Exists(assemblyDetails.Location))
            {
                assembly = assemblies.SingleOrDefault(asm => string.Compare(asm.Assembly.Location, assemblyDetails.Location, StringComparison.OrdinalIgnoreCase) == 0)?.Assembly;
                if (assembly == null)
                {
                    assemblyLoaded = true;
                    if (forReflectionPurpose)
                    {
                        assembly = Assembly.ReflectionOnlyLoadFrom(assemblyDetails.Location);
                    }
                    else
                    {
                        assembly = Assembly.LoadFrom(assemblyDetails.Location);
                    }
                }
            }
            else
            {
                assembly = assemblies.SingleOrDefault(asm => string.Compare(asm.Assembly.FullName, assemblyDetails.FullName, StringComparison.OrdinalIgnoreCase) == 0)?.Assembly;
                if (assembly == null)
                {
                    assemblyLoaded = true;
                    if (forReflectionPurpose)
                    {
                        assembly = Assembly.ReflectionOnlyLoad(assemblyDetails.FullName);
                    }
                    else
                    {
                        assembly = Assembly.Load(assemblyDetails.FullName);
                    }

                    assemblyDetails.Location = assembly.Location;
                }
            }

            if (assemblyLoaded)
            {
                _explicitLoadedAssemblies.Add(new LoadedAssemblyDetails
                {
                    Assembly = assembly,
                    Details = assemblyDetails
                });
            }
        }

        /// <summary>
        /// Attempt to load the specified Assembly.
        /// </summary>
        /// <param name="assemblyByteArray">A byte array that represents the assembly.</param>
        /// <param name="forReflectionPurpose">Defines whether the assembly must be load for reflection only or also execution.</param>
        internal void LoadAssembly(byte[] assemblyByteArray, bool forReflectionPurpose)
        {
            Assembly assembly = null;
            if (forReflectionPurpose)
            {
                assembly = Assembly.ReflectionOnlyLoad(assemblyByteArray);
            }
            else
            {
                assembly = Assembly.Load(assemblyByteArray);
            }

            var details = AssemblyInfoHelper.GetAssemblyDetailsFromNameOrLocation(assembly.FullName);
            details.ProcessorArchitecture = assembly.GetName().ProcessorArchitecture;

            _explicitLoadedAssemblies.Add(new LoadedAssemblyDetails
            {
                Assembly = assembly,
                Details = details
            });
        }

        /// <summary>
        /// Gets the assemblies that have been loaded.
        /// </summary>
        /// <returns>Returns the assemblies that have been loaded.</returns>
        internal ReadOnlyCollection<AssemblyDetails> GetAssemblies()
        {
            return GetAssembliesInternal().Select(a => a.Details).ToList().AsReadOnly();
        }

        /// <summary>
        /// Get the list of types in a loaded assembly.
        /// </summary>
        /// <param name="assemblyDetails">The assembly details.</param>
        /// <returns>Returns the list of types. Returns null if the assembly is not found.</returns>
        internal ReadOnlyCollection<TypeDetails> GetTypes(AssemblyDetails assemblyDetails)
        {
            if (!assemblyDetails.IsDotNetAssembly)
            {
                return new ReadOnlyCollection<TypeDetails>(new List<TypeDetails>());
            }

            var loadedAssemblies = GetAssembliesInternal();

            var loadedAssembly = loadedAssemblies.FirstOrDefault(a => assemblyDetails.Name == a.Assembly.GetName().Name && assemblyDetails.Version == a.Assembly.GetName().Version.ToString() && (string.IsNullOrEmpty(assemblyDetails.Culture) ? "neutral" : assemblyDetails.Culture) == (string.IsNullOrEmpty(a.Assembly.GetName().CultureName) ? "neutral" : a.Assembly.GetName().CultureName));

            if (loadedAssembly == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(loadedAssembly.Assembly.Location))
            {
                _assemblyResolutionDirectory = null;
            }
            else
            {
                _assemblyResolutionDirectory = new FileInfo(loadedAssembly.Assembly.Location).Directory;
            }

            var types = new ReadOnlyCollection<Type>(loadedAssembly.Assembly.GetExportedTypes().ToList());

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
        /// <param name="assemblyFullName">The assembly full name.</param>
        /// <returns>Returns the type if it has been found. Otherwise, throws a <see cref="TypeLoadException"/>.<returns>
        internal Type GetTypeRef(string fullName, string assemblyFullName)
        {
            Type result = null;

            if (!string.IsNullOrWhiteSpace(assemblyFullName))
            {
                result = Type.GetType($"{fullName}, {assemblyFullName}", throwOnError: false, ignoreCase: false);
            }
            else
            {
                var loadedAssemblies = GetAssembliesInternal();

                var i = 0;
                while (i < loadedAssemblies.Count && result == null)
                {
                    var loadedAssembly = loadedAssemblies[i];
                    if (string.IsNullOrWhiteSpace(loadedAssembly.Assembly.Location))
                    {
                        _assemblyResolutionDirectory = null;
                    }
                    else
                    {
                        _assemblyResolutionDirectory = new FileInfo(loadedAssembly.Assembly.Location).Directory;
                    }
                    result = loadedAssembly.Assembly.GetTypes().FirstOrDefault(type => type.IsPublic && string.Compare(type.FullName, fullName, StringComparison.Ordinal) == 0);

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
        /// <param name="assemblyFullName">(optional) The full name of the assembly.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="arguments">The arguments to pass to the method.</param>
        /// <returns>Returns the result of the method.</returns>
        internal object CreateInstanceAndInvoke(string fullName, string assemblyFullName, string methodName, object[] arguments)
        {
            var type = GetTypeRef(fullName, assemblyFullName);
            var instance = type.Assembly.CreateInstance(type.FullName);
            var method = type.GetRuntimeMethods().SingleOrDefault(m => m.IsPublic && string.Compare(m.Name, methodName, StringComparison.Ordinal) == 0);

            if (method == null)
            {
                throw new MissingMethodException($"Unable to find a method called '{methodName}'.");
            }

            var methodResult = method.Invoke(instance, arguments);
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
        private ReadOnlyCollection<LoadedAssemblyDetails> GetAssembliesInternal()
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
                    foreach (var win32ModuleHandler in _win32ModuleHandlers)
                    {
                        NativeMethods.FreeLibrary(win32ModuleHandler);
                    }

                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= AssemblyReflectionOnlyResolveEventHandler;
                    AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolveEventHandler;
                    AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
                    Dispatcher.CurrentDispatcher.UnhandledException -= CurrentDispatcher_UnhandledException;
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
        /// <param name="isReflectionOnly">Defines whether the assembly must be load for reflection only or also execution.</param>
        /// <returns>ReflectionOnlyLoadFrom loaded dependant Assembly</returns>
        private Assembly OnAssemblyResolve(ResolveEventArgs args, DirectoryInfo directory, bool isReflectionOnly)
        {
            var loadedAssembly = GetAssembliesInternal().FirstOrDefault(asm => string.Compare(asm.Assembly.FullName, args.Name, StringComparison.InvariantCulture) == 0);

            if (loadedAssembly != null)
            {
                return loadedAssembly.Assembly;
            }

            var assemblyName = new AssemblyName(args.Name);

            if (string.Compare(_currentResovlingAssembly, assemblyName.FullName) == 0)
            {
                return null;
            }

            _currentResovlingAssembly = assemblyName.FullName;

            if (directory != null)
            {
                var dependentAssemblyFilenames = new string[] { Path.Combine(directory.FullName, assemblyName.Name + ".dll"), Path.Combine(directory.FullName, assemblyName.Name + ".exe") };

                foreach (var dependentAssemblyFilename in dependentAssemblyFilenames)
                {
                    if (File.Exists(dependentAssemblyFilename))
                    {
                        Assembly assembly = null;
                        if (isReflectionOnly)
                        {
                            assembly = Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);
                        }
                        else
                        {
                            assembly = Assembly.LoadFrom(dependentAssemblyFilename);
                        }
                        _explicitLoadedAssemblies.Add(new LoadedAssemblyDetails
                        {
                            Assembly = assembly,
                            Details = AssemblyInfoHelper.GetAssemblyDetailsFromNameOrLocation(dependentAssemblyFilename)
                        });
                        return assembly;
                    }
                }
            }

            Assembly assembly2 = null;
            if (isReflectionOnly)
            {
                assembly2 = Assembly.ReflectionOnlyLoad(assemblyName.FullName);
            }
            else
            {
                assembly2 = Assembly.Load(assemblyName.FullName);
            }
            _explicitLoadedAssemblies.Add(new LoadedAssemblyDetails
            {
                Assembly = assembly2,
                Details = AssemblyInfoHelper.GetAssemblyDetailsFromNameOrLocation(assemblyName.FullName)
            });
            return assembly2;
        }

        #endregion
    }
}
