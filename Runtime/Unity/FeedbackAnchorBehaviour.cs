using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Attach to a GameObject to mark it as a feedback anchor point.
    /// Systems can query this component to find the correct transform
    /// for positional feedback effects (VFX, directional audio, etc.).
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackAnchorBehaviour")]
    public sealed class FeedbackAnchorBehaviour : MonoBehaviour, IFeedbackTarget
    {
        [SerializeField] private Transform _anchor;

        public Transform FeedbackAnchor => _anchor != null ? _anchor : transform;

        private void Reset()
        {
            _anchor = transform;
        }
    }
}
