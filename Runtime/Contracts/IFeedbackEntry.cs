namespace Astra.Pulse
{
    /// <summary>
    /// One piece of feedback carried by a <see cref="FeedbackRequest"/>, holding its own data. The service
    /// plays it through the <see cref="IFeedbackPlayer{TEntry}"/>s registered for its exact runtime type.
    /// A <c>[Serializable]</c> class with a parameterless constructor can also be authored in a
    /// <see cref="FeedbackProfileSO"/>.
    /// </summary>
    public interface IFeedbackEntry
    {
    }
}
