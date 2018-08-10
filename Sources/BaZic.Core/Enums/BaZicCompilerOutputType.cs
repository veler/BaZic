namespace BaZic.Core.Enums
{
    /// <summary>
    /// Describes what type of assembly must be compiled by the BaZic runtime.
    /// </summary>
    public enum BaZicCompilerOutputType
    {
        ConsoleApp = 0,
        WindowsApp = 1,
        DynamicallyLinkedLibrary = 3
    }
}
