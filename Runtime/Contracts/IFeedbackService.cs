namespace Astra.Pulse
{
    /// <summary>
    /// Central service for triggering gameplay feedback. Routes requests to registered
    /// <see cref="IFeedbackPlayer"/>s. Does not decide when feedback should occur — only executes it.
    /// </summary>
    public interface IFeedbackService
    {
        FeedbackResult Play(FeedbackRequest request);
        FeedbackResult Play(FeedbackProfileSO profile);

        /// <summary>
        /// Registers a feedback player. A consumer calls this against the resolved service
        /// interface, without casting to the concrete implementation.
        /// </summary>
        void RegisterPlayer(IFeedbackPlayer player);
    }
}
