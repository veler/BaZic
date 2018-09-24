using BaZic.Core.ComponentModel;
using BaZic.Core.Enums;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;

namespace BaZic.Core.Logs
{
    /// <summary>
    /// Provides a set of functions designed to log information about how the application run.
    /// </summary>
    public abstract class Logger : MarshalByRefObject, ComponentModel.IDisposable
    {
        #region Fields & Constants

        private static Logger instance;

        private readonly ThreadLocal<StringBuilder> _logs = new ThreadLocal<StringBuilder>(() => new StringBuilder());
        private bool _fatalOccured;
        private int _logsCountSinceLastFlush;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current instance of the <see cref="Logger"/> class.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                return instance;
            }
            set
            {
                if (instance != null && Application.Current != null)
                {
                    throw new InvalidOperationException($"Cannot change the value once a '{nameof(Logger)}' has been initialized.");
                }

                instance = value;
            }
        }

        /// <inheritdoc/>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Defines whether the logs must be displayed in the console or not.
        /// </summary>
        public bool RedirectToConsole { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Raised when a log has been added.
        /// </summary>
        public event EventHandler<LogAddedEventArgs> LogAdded;

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        public Logger()
        {
        }

        /// <summary>
        /// Finalizes the instance of the class.
        /// </summary>
        ~Logger()
        {
            OnDispose(false);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the logger with the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The optional arguments to pass to the type constructor.</param>
        public static void Initialize<T>(params object[] args) where T : Logger
        {
            var flags = System.Reflection.BindingFlags.CreateInstance | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
            var type = typeof(T);
            Instance = (T)AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(type.Assembly.Location, type.FullName, false, flags, null, args, null, null);
        }

        /// <summary>
        /// Called when a new instance of a <see cref="Logger"/> in created.
        /// </summary>
        public abstract void SessionStarted();

        /// <summary>
        /// Called when an instance of <see cref="Logger"/> is disposed or if a <see cref="LogType.Fatal"/> log is added, after flushing it.
        /// </summary>
        public abstract void SessionStopped();

        /// <summary>
        /// Must persist the data from the log.
        /// </summary>
        public abstract void Persist(StringBuilder logs);

        /// <summary>
        /// Returns a set of additional information like the operating system or the executable version when a fatal error occur.
        /// </summary>
        /// <returns>Some additional information.</returns>
        public abstract string GetFatalErrorAdditionalInfo();

        /// <summary>
        /// Add an <see cref="LogType.Information"/> log.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Information(string message, [CallerMemberName] string callerName = null)
        {
            Log(LogType.Information, message, callerName);
        }

        /// <summary>
        /// Add an <see cref="LogType.Debug"/> log. It will be performed only if the debugger is attached, not in production mode.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Debug(string message, [CallerMemberName] string callerName = null)
        {
#if DEBUG
            Log(LogType.Debug, message, callerName);
#endif
        }

        /// <summary>
        /// Add an <see cref="LogType.Warning"/> log.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Warning(string message, [CallerMemberName] string callerName = null)
        {
            Log(LogType.Warning, message, callerName);
        }

        /// <summary>
        /// Add an <see cref="LogType.Error"/> log.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> associated to the error.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        public void Error(Exception exception, [CallerMemberName] string callerName = null)
        {
            Log(LogType.Error, exception.Message, callerName);
        }

        /// <summary>
        /// Add a <see cref="LogType.Fatal"/> log, flush the logs, stop the session and throw the given exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> associated to the fatal error.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        /// <param name="sourceFilePath">(optional) The path of the file who call this method.</param>
        /// <param name="sourceLineNumber">(optional) The line number of the fatal error.</param>
        public void Fatal(Exception exception, [CallerMemberName] string callerName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Fatal(string.Empty, exception, callerName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Add a <see cref="LogType.Fatal"/> log, flush the logs, stop the session and throw the given exception.
        /// </summary>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="exception">The <see cref="Exception"/> associated to the fatal error.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        /// <param name="sourceFilePath">(optional) The path of the file who call this method.</param>
        /// <param name="sourceLineNumber">(optional) The line number of the fatal error.</param>
        public void Fatal(string message, Exception exception, [CallerMemberName] string callerName = null, [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            Log(LogType.Fatal, message, $"{callerName} in {Path.GetFileName(sourceFilePath)}, line {sourceLineNumber}");
            Log(LogType.Fatal, GetFatalErrorAdditionalInfo(), "Additional Information");
            Flush();
            SessionStopped();
            _fatalOccured = true;

            if (exception != null)
            {
                throw exception;
            }
        }

        /// <summary>
        /// Persists the logs and clear the buffer.
        /// </summary>
        internal void Flush()
        {
            Persist(_logs.Value);
            _logs.Value.Clear();
            _logsCountSinceLastFlush = 0;
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
                    if (!_fatalOccured)
                    {
                        Flush();
                        SessionStopped();
                    }
                }
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Add a log to the buffer in memory.
        /// </summary>
        /// <param name="type">The type of log.</param>
        /// <param name="message">The message associated to the log.</param>
        /// <param name="callerName">(optional) The name of the caller member.</param>
        private void Log(LogType type, string message, string callerName)
        {
            if (_fatalOccured)
            {
                throw new OperationCanceledException("Unable to log something when the logger did a fatal.");
            }

            var fullMessage = $"[{type}] [{callerName}] [{DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss")}] {message}";
            _logs.Value.AppendLine(fullMessage);

            LogAdded?.Invoke(this, new LogAddedEventArgs(fullMessage));

#if DEBUG
            System.Diagnostics.Debug.WriteLine(fullMessage);
#endif

            if (RedirectToConsole)
            {
                Console.WriteLine(message);
            }

            Interlocked.Increment(ref _logsCountSinceLastFlush);
            if (_logsCountSinceLastFlush > Consts.LogsFlushInterval)
            {
                Flush();
            }
        }

        #endregion
    }
}
