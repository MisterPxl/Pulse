using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Optional standalone audio player built on <see cref="AudioSource.PlayClipAtPoint(AudioClip, Vector3, float)"/>.
    /// A consumer registers it explicitly when no audio engine (such as Echo) is available:
    /// <c>service.RegisterPlayer(new UnityAudioFeedbackPlayer());</c>
    /// </summary>
    public sealed class UnityAudioFeedbackPlayer : IFeedbackPlayer
    {
        private readonly IPulseLogger _logger;

        public UnityAudioFeedbackPlayer(IPulseLogger logger = null)
        {
            _logger = logger;
        }

        public bool CanPlay(FeedbackRequest request)
        {
            return request != null
                && (request.Type & FeedbackType.Audio) != 0
                && request.AudioClip != null;
        }

        public FeedbackResult Play(FeedbackRequest request)
        {
            try
            {
                Vector3 position = request.Target != null ? request.Target.position : request.VfxPosition;
                AudioSource.PlayClipAtPoint(request.AudioClip, position, Mathf.Clamp01(request.AudioVolume));
                return FeedbackResult.Succeed(FeedbackType.Audio, $"Audio '{request.AudioClip.name}' played.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Audio feedback exception: {ex.Message}");
                return FeedbackResult.Fail($"Audio feedback exception: {ex.Message}", FeedbackType.Audio, ex);
            }
        }
    }
}
