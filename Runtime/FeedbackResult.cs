using System;

namespace Astra.Pulse
{
    /// <summary>
    /// Structured result of a feedback operation.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackResult")]
    public sealed class FeedbackResult
    {
        public bool Success { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public FeedbackType Type { get; }

        private FeedbackResult(bool success, string message, Exception exception, FeedbackType type)
        {
            Success = success;
            Message = message;
            Exception = exception;
            Type = type;
        }

        public static FeedbackResult Succeed(FeedbackType type, string message = null)
        {
            return new FeedbackResult(true, message, null, type);
        }

        public static FeedbackResult Fail(string message, FeedbackType type = FeedbackType.None, Exception exception = null)
        {
            return new FeedbackResult(false, message, exception, type);
        }
    }
}
