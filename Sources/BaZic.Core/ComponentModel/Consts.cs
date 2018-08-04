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
