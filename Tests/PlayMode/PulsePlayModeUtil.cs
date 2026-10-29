using System.Collections.Generic;

namespace Astra.Pulse.Tests
{
    internal sealed class RecordingLogger : IPulseLogger
    {
        public readonly List<string> Errors = new List<string>();

        public bool IsInfoEnabled => true;
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message) { Errors.Add(message); }
    }
}
