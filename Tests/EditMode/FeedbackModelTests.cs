using NUnit.Framework;
using UnityEngine;

namespace Astra.Pulse.Tests
{
    public sealed class FeedbackModelTests
    {
        [Test]
        public void FeedbackType_IntegersArePinned()
        {
            Assert.That((int)FeedbackType.None, Is.EqualTo(0));
            Assert.That((int)FeedbackType.Audio, Is.EqualTo(1));
            Assert.That((int)FeedbackType.CameraShake, Is.EqualTo(2));
            Assert.That((int)FeedbackType.Hud, Is.EqualTo(4));
            Assert.That((int)FeedbackType.Vfx, Is.EqualTo(8));
            Assert.That((int)FeedbackType.Custom, Is.EqualTo(16));
        }

        [Test]
        public void FeedbackResult_Factories()
        {
            System.InvalidOperationException exception = new System.InvalidOperationException("x");
            FeedbackResult ok = FeedbackResult.Succeed(FeedbackType.Hud, "fine");
            FeedbackResult ko = FeedbackResult.Fail("nope", FeedbackType.Vfx, exception);
            FeedbackResult bare = FeedbackResult.Fail("bare");

            Assert.That(ok.Success, Is.True);
            Assert.That(ok.Type, Is.EqualTo(FeedbackType.Hud));
            Assert.That(ok.Message, Is.EqualTo("fine"));
            Assert.That(ko.Success, Is.False);
            Assert.That(ko.Exception, Is.SameAs(exception));
            Assert.That(bare.Type, Is.EqualTo(FeedbackType.None));
        }

        [Test]
        public void FeedbackRequest_Defaults()
        {
            FeedbackRequest request = new FeedbackRequest();

            Assert.That(request.Type, Is.EqualTo(FeedbackType.None));
            Assert.That(request.AudioVolume, Is.EqualTo(1f));
            Assert.That(request.Shake.HasValue, Is.False);
            Assert.That(request.HudOverlayId.HasValue, Is.False);
            Assert.That(request.Payload, Is.Null);
        }

        [Test]
        public void FeedbackShake_DefaultFrequencyIs25()
        {
            FeedbackShake shake = new FeedbackShake(0.4f, 0.3f);
            Assert.That(shake.Amplitude, Is.EqualTo(0.4f));
            Assert.That(shake.Duration, Is.EqualTo(0.3f));
            Assert.That(shake.Frequency, Is.EqualTo(25f));
        }

        [Test]
        public void Extensions_PlayAudioBuildsAnAudioRequest()
        {
            RecordingService service = new RecordingService();
            AudioClip clip = AudioClip.Create("PulseClip", 32, 1, 44100, false);
            try
            {
                FeedbackResult result = service.PlayAudio(clip, 0.25f);

                Assert.That(result.Success, Is.True);
                Assert.That(service.Last.Type, Is.EqualTo(FeedbackType.Audio));
                Assert.That(service.Last.AudioClip, Is.SameAs(clip));
                Assert.That(service.Last.AudioVolume, Is.EqualTo(0.25f));
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void Extensions_PlayShakeBuildsAShakeRequest()
        {
            RecordingService service = new RecordingService();

            service.PlayShake(0.5f, 0.2f);

            Assert.That(service.Last.Type, Is.EqualTo(FeedbackType.CameraShake));
            Assert.That(service.Last.Shake.HasValue, Is.True);
            Assert.That(service.Last.Shake.Value.Amplitude, Is.EqualTo(0.5f));
            Assert.That(service.Last.Shake.Value.Duration, Is.EqualTo(0.2f));
            Assert.That(service.Last.Shake.Value.Frequency, Is.EqualTo(25f));
        }

        [Test]
        public void Extensions_NullServiceFailsInsteadOfThrowing()
        {
            IFeedbackService service = null;
            Assert.That(service.PlayShake(1f, 1f).Success, Is.False);
            Assert.That(service.PlayAudio(null).Success, Is.False);
        }

        private sealed class RecordingService : IFeedbackService
        {
            public FeedbackRequest Last;

            public FeedbackResult Play(FeedbackRequest request)
            {
                Last = request;
                return FeedbackResult.Succeed(request.Type);
            }

            public FeedbackResult Play(FeedbackProfileSO profile) => Play(profile.BuildRequest());
            public void RegisterPlayer(IFeedbackPlayer player) { }
        }
    }
}
