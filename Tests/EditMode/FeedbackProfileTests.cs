using NUnit.Framework;
using UnityEngine;

namespace Astra.Pulse.Tests
{
    public sealed class FeedbackProfileTests
    {
        private FeedbackProfileSO _profile;

        [SetUp]
        public void SetUp()
        {
            _profile = ScriptableObject.CreateInstance<FeedbackProfileSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_profile);
        }

        private void Configure(string json)
        {
            JsonUtility.FromJsonOverwrite(json, _profile);
        }

        [Test]
        public void BuildRequest_Defaults()
        {
            FeedbackRequest request = _profile.BuildRequest();

            Assert.That(request.Type, Is.EqualTo(FeedbackType.None));
            Assert.That(request.AudioVolume, Is.EqualTo(1f));
            Assert.That(request.Shake.HasValue, Is.False, "no shake without amplitude and duration");
            Assert.That(request.HudOverlayId.HasValue, Is.False);
        }

        [Test]
        public void BuildRequest_ShakeOnlyWhenAmplitudeAndDurationArePositive()
        {
            Configure("{\"_shakeAmplitude\":0.4,\"_shakeDuration\":0}");
            Assert.That(_profile.BuildRequest().Shake.HasValue, Is.False);

            Configure("{\"_shakeAmplitude\":0,\"_shakeDuration\":0.3}");
            Assert.That(_profile.BuildRequest().Shake.HasValue, Is.False);

            Configure("{\"_shakeAmplitude\":0.4,\"_shakeDuration\":0.3,\"_shakeFrequency\":30}");
            FeedbackShake shake = _profile.BuildRequest().Shake.Value;
            Assert.That(shake.Amplitude, Is.EqualTo(0.4f));
            Assert.That(shake.Duration, Is.EqualTo(0.3f));
            Assert.That(shake.Frequency, Is.EqualTo(30f));
        }

        [Test]
        public void BuildRequest_OverlayMapsTheSerializedInteger()
        {
            Configure("{\"_useTargetOverlay\":true,\"_targetOverlay\":1}");
            Assert.That(_profile.BuildRequest().HudOverlayId, Is.EqualTo(1));

            Configure("{\"_useTargetOverlay\":false,\"_targetOverlay\":1}");
            Assert.That(_profile.BuildRequest().HudOverlayId.HasValue, Is.False);
        }

        [Test]
        public void TargetOverlay_DefaultsToTheHistoricalInteractionPromptId()
        {
            Configure("{\"_useTargetOverlay\":true}");
            Assert.That(FeedbackProfileSO.DefaultOverlayId, Is.EqualTo(3));
            Assert.That(_profile.TargetOverlay, Is.EqualTo(3));
        }

        [Test]
        public void BuildRequest_CopiesTypeVolumeAndMessage()
        {
            Configure("{\"_type\":5,\"_volume\":0.5,\"_hudMessage\":\"Hello\"}");

            FeedbackRequest request = _profile.BuildRequest();

            Assert.That(request.Type, Is.EqualTo(FeedbackType.Audio | FeedbackType.Hud));
            Assert.That(request.AudioVolume, Is.EqualTo(0.5f));
            Assert.That(request.HudMessage, Is.EqualTo("Hello"));
        }

        [Test]
        public void ServiceDispatchesTheProfileRequest()
        {
            PulseService service = new PulseService();
            PulseTestUtil.FakePlayer player = new PulseTestUtil.FakePlayer(FeedbackType.Hud);
            service.RegisterPlayer(player);
            Configure("{\"_type\":4,\"_hudMessage\":\"Hi\"}");

            FeedbackResult result = service.Play(_profile);

            Assert.That(result.Success, Is.True);
            Assert.That(player.LastRequest.HudMessage, Is.EqualTo("Hi"));
        }
    }
}
