namespace Astra.Pulse
{
    /// <summary>
    /// Central service for triggering gameplay feedback. Routes each entry of a request to the
    /// <see cref="IFeedbackPlayer{TEntry}"/>s registered for its type. Does not decide when feedback
    /// should occur — only executes it. Entry and profile shortcuts are in <see cref="FeedbackServiceExtensions"/>.
    /// </summary>
    public interface IFeedbackService
    {
        FeedbackResult Play(FeedbackRequest request);

        /// <summary>
        /// Registers a player for the entries whose runtime type is exactly <typeparamref name="TEntry"/>.
        /// A consumer calls this against the resolved service interface, without casting to the concrete implementation.
        /// </summary>
        void RegisterPlayer<TEntry>(IFeedbackPlayer<TEntry> player) where TEntry : IFeedbackEntry;
    }
}
