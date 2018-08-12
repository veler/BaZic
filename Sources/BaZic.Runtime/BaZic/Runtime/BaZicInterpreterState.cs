namespace BaZic.Runtime.BaZic.Runtime
{
    /// <summary>
    /// Defines identifiers for the state of the interpreter
    /// </summary>
    public enum BaZicInterpreterState
    {
        /// <summary>
        /// Actually in pause, ready to continue
        /// </summary>
        Ready = 0,

        /// <summary>
        /// In pause, waiting for an action from the user to continue
        /// </summary>
        Pause = 1,

        /// <summary>
        /// Preparing to an action
        /// </summary>
        Preparing = 2,

        /// <summary>
        /// Working and or interpreting
        /// </summary>
        Running = 3,

        /// <summary>
        /// Is running, but doesn't execute any method on the STA Thread.
        /// </summary>
        Idle = 4,

        /// <summary>
        /// The interpreter is stopped
        /// </summary>
        Stopped = 5,

        /// <summary>
        /// The interpreter is stopped and an error has been thrown
        /// </summary>
        StoppedWithError = 6,

        /// <summary>
        /// A log message is send
        /// </summary>
        Log = 7
    }
}
