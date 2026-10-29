using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>Shortcuts that keep <c>feedback.PlayShake(...)</c> / <c>feedback.PlayAudio(...)</c> call sites unchanged.</summary>
    public static class FeedbackServiceExtensions
    {
        public static FeedbackResult PlayAudio(this IFeedbackService service, AudioClip clip, float volumeScale = 1f)
        {
            if (service == null) return FeedbackResult.Fail("Feedback service is null.", FeedbackType.Audio);

            return service.Play(new FeedbackRequest
            {
                Type = FeedbackType.Audio,
                AudioClip = clip,
                AudioVolume = volumeScale
            });
        }

        public static FeedbackResult PlayShake(this IFeedbackService service, float amplitude, float duration, float frequency = 25f)
        {
            if (service == null) return FeedbackResult.Fail("Feedback service is null.", FeedbackType.CameraShake);

            return service.Play(new FeedbackRequest
            {
                Type = FeedbackType.CameraShake,
                Shake = new FeedbackShake(amplitude, duration, frequency)
            });
        }
    }
}
