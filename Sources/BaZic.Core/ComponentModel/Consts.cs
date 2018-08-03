using System.Reflection;

namespace BaZic.Core.ComponentModel
{
    /// <summary>
    /// Provides constants
    /// </summary>
    public static class Consts
    {
        // Abstract Syntax Tree
        public const string EntryPointMethodName = "Main";

        // Reflection
        public const BindingFlags LimitedBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        // Optimizer
        public const int RecursivityLimit = 100;

        // Interpreter
        public const int CallLimitBeforeNewThread = 200;
        public const int StackOverflowLimit = 10000;

        // Logs
        public const int LogsFlushInterval = 200;
    }
}
