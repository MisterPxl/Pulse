using System;
using NUnit.Framework;
using UnityEngine;
using static Astra.Pulse.Tests.PulseTestUtil;
using Object = UnityEngine.Object;

namespace Astra.Pulse.Tests
{
    public sealed class FeedbackModelTests
    {
        [Test]
        public void FeedbackResult_Factories()
        {
            InvalidOperationException exception = new InvalidOperationException("x");
            FeedbackResult ok = FeedbackResult.Succeed("fine");
            FeedbackResult ko = FeedbackResult.Fail("nope", exception);
            FeedbackResult bare = FeedbackResult.Fail("bare");

            Assert.That(ok.Success, Is.True);
            Assert.That(ok.Message, Is.EqualTo("fine"));
            Assert.That(ko.Success, Is.False);
            Assert.That(ko.Exception, Is.SameAs(exception));
            Assert.That(bare.Exception, Is.Null);
        }

        [Test]
        public void FeedbackRequest_KeepsEntriesInOrderAndAddChains()
        {
            AlphaEntry first = new AlphaEntry();
            BetaEntry second = new BetaEntry();
            AlphaEntry third = new AlphaEntry();

            FeedbackRequest request = new FeedbackRequest(first, second).Add(third);

            Assert.That(request.Entries, Is.EqualTo(new IFeedbackEntry[] { first, second, third }));
            Assert.That(new FeedbackRequest().Entries, Is.Empty);
        }

        [Test]
        public void FeedbackRequest_RejectsNullEntries()
        {
            Assert.Throws<ArgumentNullException>(() => new FeedbackRequest((IFeedbackEntry)null));
            Assert.Throws<ArgumentNullException>(() => new FeedbackRequest((IFeedbackEntry[])null));
            Assert.Throws<ArgumentNullException>(() => new FeedbackRequest().Add(null));
        }

        [Test]
        public void FeedbackRequest_IntensityDefaultsToOneAndAcceptsZero()
        {
            FeedbackRequest request = new FeedbackRequest(new AlphaEntry());

            Assert.That(request.Intensity, Is.EqualTo(1f));

            request.Intensity = 0f;
            Assert.That(request.Intensity, Is.EqualTo(0f));

            request.Intensity = 2.5f;
            Assert.That(request.Intensity, Is.EqualTo(2.5f));
        }

        [TestCase(-0.1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void FeedbackRequest_RejectsANegativeOrNonFiniteIntensity(float intensity)
        {
            FeedbackRequest request = new FeedbackRequest();

            Assert.Throws<ArgumentOutOfRangeException>(() => request.Intensity = intensity);
            Assert.That(request.Intensity, Is.EqualTo(1f));
        }

        [Test]
        public void FeedbackRequest_TargetPositionWinsOverPosition()
        {
            GameObject anchor = new GameObject("PulseAnchor");
            try
            {
                anchor.transform.position = new Vector3(9f, 8f, 7f);
                FeedbackRequest request = new FeedbackRequest { Position = new Vector3(1f, 2f, 3f) };

                Assert.That(request.ResolvePosition(), Is.EqualTo(new Vector3(1f, 2f, 3f)));

                request.Target = anchor.transform;
                Assert.That(request.ResolvePosition(), Is.EqualTo(new Vector3(9f, 8f, 7f)));
            }
            finally
            {
                Object.DestroyImmediate(anchor);
            }
        }

        [Test]
        public void BuiltInEntries_Defaults()
        {
            Assert.That(new AudioFeedback().Volume, Is.EqualTo(1f));
            Assert.That(new AudioFeedback(null).Volume, Is.EqualTo(1f));
            Assert.That(new CameraShakeFeedback().Frequency, Is.EqualTo(25f));
            Assert.That(new CameraShakeFeedback(0.4f, 0.3f).Frequency, Is.EqualTo(25f));
            Assert.That(new HudFeedback().Message, Is.Null);
            Assert.That(new VfxFeedback().Prefab, Is.Null);
        }

        [Test]
        public void Extensions_PlayEntryWrapsItInARequest()
        {
            RecordingService service = new RecordingService();
            AlphaEntry entry = new AlphaEntry();

            service.Play(entry);

            Assert.That(service.Last.Entries, Is.EqualTo(new IFeedbackEntry[] { entry }));
        }

        [Test]
        public void Extensions_PlayAudioBuildsAnAudioEntry()
        {
            RecordingService service = new RecordingService();
            AudioClip clip = AudioClip.Create("PulseClip", 32, 1, 44100, false);
            try
            {
                FeedbackResult result = service.PlayAudio(clip, 0.25f);

                Assert.That(result.Success, Is.True);
                AudioFeedback audio = (AudioFeedback)service.Last.Entries[0];
                Assert.That(audio.Clip, Is.SameAs(clip));
                Assert.That(audio.Volume, Is.EqualTo(0.25f));
            }
            finally
            {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void Extensions_PlayShakeBuildsAShakeEntry()
        {
            RecordingService service = new RecordingService();

            service.PlayShake(0.5f, 0.2f);

            CameraShakeFeedback shake = (CameraShakeFeedback)service.Last.Entries[0];
            Assert.That(shake.Amplitude, Is.EqualTo(0.5f));
            Assert.That(shake.Duration, Is.EqualTo(0.2f));
            Assert.That(shake.Frequency, Is.EqualTo(25f));
        }

        [Test]
        public void Extensions_NullArgumentsFailInsteadOfThrowing()
        {
            IFeedbackService missing = null;
            Assert.That(missing.PlayShake(1f, 1f).Success, Is.False);
            Assert.That(missing.PlayAudio(null).Success, Is.False);
            Assert.That(missing.Play(new AlphaEntry()).Success, Is.False);

            RecordingService service = new RecordingService();
            Assert.That(service.Play((IFeedbackEntry)null).Message, Is.EqualTo("Feedback entry is null."));
            Assert.That(service.Play((FeedbackProfileSO)null).Message, Is.EqualTo("Feedback profile is null."));
            Assert.That(service.Last, Is.Null);
        }

        private sealed class RecordingService : IFeedbackService
        {
            public FeedbackRequest Last;

            public FeedbackResult Play(FeedbackRequest request)
            {
                Last = request;
                return FeedbackResult.Succeed();
            }

            public void RegisterPlayer<TEntry>(IFeedbackPlayer<TEntry> player) where TEntry : IFeedbackEntry
            {
            }
        }
    }
}
