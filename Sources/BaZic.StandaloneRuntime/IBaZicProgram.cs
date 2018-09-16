namespace BaZic.StandaloneRuntime
{
    /// <summary>
    /// Provides the minimum implementation of a compiled BaZic program.
    /// </summary>
    public interface IBaZicProgram
    {
        /// <summary>
        /// Gets the helper designed to manage the life cycle of the program.
        /// </summary>
        ProgramHelper ProgramHelperInstance { get; }

        /// <summary>
        /// Entry point of the BaZic program.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        dynamic Main(dynamic args);
    }
}
