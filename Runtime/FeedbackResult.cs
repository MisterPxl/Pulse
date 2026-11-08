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

        private FeedbackResult(bool success, string message, Exception exception)
        {
            Success = success;
            Message = message;
            Exception = exception;
        }

        public static FeedbackResult Succeed(string message = null)
        {
            return new FeedbackResult(true, message, null);
        }

        public static FeedbackResult Fail(string message, Exception exception = null)
        {
            return new FeedbackResult(false, message, exception);
        }
    }
}
