namespace Astra.Pulse
{
    /// <summary>Sink for Pulse diagnostics. Null is accepted everywhere.</summary>
    public interface IPulseLogger
    {
        bool IsInfoEnabled { get; }

        void Info(string message);
        void Warn(string message);
        void Error(string message);
    }
}
