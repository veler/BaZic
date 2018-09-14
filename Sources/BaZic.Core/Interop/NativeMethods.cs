using BaZic.Core.ComponentModel;
using System;
using System.Runtime.InteropServices;

namespace BaZic.Core.Interop
{
    /// <summary>
    /// Provides a set of native methods
    /// </summary>
    internal static class NativeMethods
    {
        #region Kernel32

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="dllToLoad">The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the LIBRARY keyword in the module-definition (.def) file. If the string specifies a full path, the function searches only that path for the module. If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module; for more information, see the Remarks. If the function cannot find the module, the function fails.When specifying a path, be sure to use backslashes (\), not forward slashes(/). For more information about paths, see Naming a File or Directory. If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension .dll to the module name.To prevent the function from appending .dll to the module name, include a trailing point character (.) in the module name string.</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        /// <summary>
        /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count. When the reference count reaches zero, the module is unloaded from the address space of the calling process and the handle is no longer valid.
        /// </summary>
        /// <param name="hModule">A handle to the loaded library module. The LoadLibrary, LoadLibraryEx, GetModuleHandle, or GetModuleHandleEx function returns this handle.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport(Consts.Kernel32, SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        #endregion
    }
}
