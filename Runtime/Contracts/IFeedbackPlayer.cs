namespace Astra.Pulse
{
    /// <summary>
    /// Executes a specific type of feedback (audio, shake, HUD, VFX, etc.).
    /// The service iterates registered players and delegates to those
    /// that can handle the request.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "IFeedbackPlayer")]
    public interface IFeedbackPlayer
    {
        bool CanPlay(FeedbackRequest request);
        FeedbackResult Play(FeedbackRequest request);
    }
}
