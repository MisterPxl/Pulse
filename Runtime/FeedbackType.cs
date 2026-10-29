using System;

namespace Astra.Pulse
{
    /// <summary>
    /// Types of feedback that can be requested.
    /// A single request may carry multiple flags to trigger compound feedback.
    /// </summary>
    [Flags]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackType")]
    public enum FeedbackType
    {
        None = 0,
        Audio = 1 << 0,
        CameraShake = 1 << 1,
        Hud = 1 << 2,
        Vfx = 1 << 3,

        /// <summary>
        /// Dispatched to custom players registered by a pack. The framework never
        /// implements a player for this type; only <see cref="FeedbackRequest.Payload"/>
        /// carries meaning for it.
        /// </summary>
        Custom = 1 << 4
    }
}
