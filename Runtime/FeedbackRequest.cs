using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Structured request describing which feedback to trigger and with what parameters.
    /// Only populate the fields relevant to the requested <see cref="Type"/>.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackRequest")]
    public sealed class FeedbackRequest
    {
        public FeedbackType Type { get; set; } = FeedbackType.None;

        public AudioClip AudioClip { get; set; }
        public float AudioVolume { get; set; } = 1f;

        /// <summary>Camera-shake cue. The framework adapter maps it onto its camera service.</summary>
        public FeedbackShake? Shake { get; set; }

        public string HudMessage { get; set; }

        /// <summary>
        /// Optional HUD overlay to trigger, as an opaque integer: Pulse never interprets it, the
        /// adapter that owns the HUD maps it onto its own overlay enum. When null the player falls back to its default.
        /// </summary>
        public int? HudOverlayId { get; set; }

        public GameObject VfxPrefab { get; set; }
        public Vector3 VfxPosition { get; set; }

        public Transform Target { get; set; }

        /// <summary>
        /// Opaque data for a custom player (<see cref="FeedbackType.Custom"/>). Pulse
        /// never reads or writes this; the registering pack owns its shape.
        /// </summary>
        public object Payload { get; set; }
    }
}
