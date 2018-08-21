using BaZic.Core.Logs;
using System.Text;
using System.Threading;

namespace BaZic.Core.Tests.Mocks
{
    public class LogMock : Logger
    {
        private ThreadLocal<StringBuilder> _logs;

        public LogMock()
        {
        }

        public override void Persist(StringBuilder logs)
        {
            _logs.Value.Append(logs);
        }

        public override void SessionStarted()
        {
            _logs = new ThreadLocal<StringBuilder>(() => new StringBuilder());
        }

        public override void SessionStopped()
        {
        }

        public override string GetFatalErrorAdditionalInfo()
        {
            return "Additional info...";
        }

        public StringBuilder GetLogs()
        {
            return _logs.Value;
        }
    }
}
