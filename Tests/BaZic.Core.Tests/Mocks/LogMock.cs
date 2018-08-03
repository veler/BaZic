using BaZic.Core.Logs;
using System.Text;

namespace BaZic.Core.Tests.Mocks
{
    public class LogMock : Logger
    {
        private StringBuilder _logs;

        public LogMock()
        {
        }

        public override void Persist(StringBuilder logs)
        {
            _logs.Append(logs);
        }

        public override void SessionStarted()
        {
            _logs = new StringBuilder();
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
            return _logs;
        }
    }
}
