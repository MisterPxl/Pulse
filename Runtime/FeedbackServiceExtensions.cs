using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Shortcuts over <see cref="IFeedbackService.Play(FeedbackRequest)"/>: a single entry
    /// (<c>feedback.Play(new MyPackFeedback(...))</c>), a profile, <c>PlayAudio</c> and <c>PlayShake</c>.
    /// </summary>
    public static class FeedbackServiceExtensions
    {
        public static FeedbackResult Play(this IFeedbackService service, IFeedbackEntry entry)
        {
            if (service == null) return FeedbackResult.Fail("Feedback service is null.");
            if (entry == null) return FeedbackResult.Fail("Feedback entry is null.");

            return service.Play(new FeedbackRequest(entry));
        }

        public static FeedbackResult Play(this IFeedbackService service, FeedbackProfileSO profile)
        {
            if (service == null) return FeedbackResult.Fail("Feedback service is null.");
            if (profile == null) return FeedbackResult.Fail("Feedback profile is null.");

            return service.Play(profile.BuildRequest());
        }

        public static FeedbackResult PlayAudio(this IFeedbackService service, AudioClip clip, float volumeScale = 1f)
        {
            return service.Play(new AudioFeedback(clip, volumeScale));
        }

        public static FeedbackResult PlayShake(this IFeedbackService service, float amplitude, float duration, float frequency = 25f)
        {
            return service.Play(new CameraShakeFeedback(amplitude, duration, frequency));
        }
    }
}
