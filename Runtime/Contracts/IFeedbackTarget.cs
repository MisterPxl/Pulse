using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Optional contract for objects that can serve as the spatial
    /// anchor for feedback effects (VFX position, directional audio, etc.).
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "IFeedbackTarget")]
    public interface IFeedbackTarget
    {
        Transform FeedbackAnchor { get; }
    }
}
