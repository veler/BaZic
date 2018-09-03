using System.Reflection;

namespace BaZic.Core.ComponentModel
{
    /// <summary>
    /// Provides constants
    /// </summary>
    public static class Consts
    {
        // Abstract Syntax Tree

        /// <summary>
        /// Entry Point Method Name
        /// </summary>
        public const string EntryPointMethodName = "Main";

        /// <summary>
        /// Names of the main namespace and class generated when a BaZic program is compiled.
        /// </summary>
        public const string CompiledProgramClassName = "BaZicProgramReleaseMode.Program";

        public const string CompiledProgramIdleStateOccuredEvent = "IdleStateOccured";
        public const string CompiledProgramHelperInstance = "ProgramHelperInstance";
        public const string CompiledCloseUserInterface = "CloseUserInterface";

        // Assembly Properties
        internal const string AssemblyPropertyVersion = "Version=";
        internal const string AssemblyPropertyCulture = "Culture=";
        internal const string AssemblyPropertyProcessorArchitecture = "processorArchitecture=";
        internal const string AssemblyPropertyX86 = "x86";
        internal const string AssemblyPropertyX64 = "AMD64";
        internal const string AssemblyPropertyAnyCPU = "MSIL";
        internal const string AssemblyPropertyPublicKeyToken = "PublicKeyToken=";
        internal const string AssemblyPropertyCustom = "Custom=";

        // Resources
        public static readonly string[] ImageResourcesType = new string[] { ".jpg", ".jpeg", ".png", ".gif" };

        // Reflection

        /// <summary>
        /// Limited Binding Flags
        /// </summary>
        public const BindingFlags LimitedBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        // Optimizer

        /// <summary>
        /// Recursivity Limit
        /// </summary>
        public const int RecursivityLimit = 100;

        // Interpreter

        /// <summary>
        /// Call Limit Before New Thread
        /// </summary>
        public const int CallLimitBeforeNewThread = 200;

        /// <summary>
        /// StackOverflow Limit
        /// </summary>
        public const int StackOverflowLimit = 10000;

        // Logs

        /// <summary>
        /// Logs Flush Interval
        /// </summary>
        public const int LogsFlushInterval = 200;
    }
}
