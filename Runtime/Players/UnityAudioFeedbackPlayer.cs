using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Optional standalone audio player built on <see cref="AudioSource.PlayClipAtPoint(AudioClip, Vector3, float)"/>.
    /// Plays at the entry volume times <see cref="FeedbackRequest.Intensity"/>, clamped to 1.
    /// A consumer registers it explicitly when no audio engine (such as Echo) is available:
    /// <c>service.RegisterPlayer(new UnityAudioFeedbackPlayer());</c>
    /// </summary>
    public sealed class UnityAudioFeedbackPlayer : IFeedbackPlayer<AudioFeedback>
    {
        private readonly IPulseLogger _logger;

        public UnityAudioFeedbackPlayer(IPulseLogger logger = null)
        {
            _logger = logger;
        }

        public FeedbackResult Play(AudioFeedback entry, FeedbackRequest request)
        {
            if (entry.Clip == null)
                return FeedbackResult.Fail("Audio feedback has no clip.");

            try
            {
                AudioSource.PlayClipAtPoint(entry.Clip, request.ResolvePosition(), Mathf.Clamp01(entry.Volume * request.Intensity));
                return FeedbackResult.Succeed($"Audio '{entry.Clip.name}' played.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Audio feedback exception: {ex.Message}");
                return FeedbackResult.Fail($"Audio feedback exception: {ex.Message}", ex);
            }
        }
    }
}
