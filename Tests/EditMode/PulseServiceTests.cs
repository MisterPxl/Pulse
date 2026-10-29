using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Astra.Pulse.Tests
{
    /// <summary>Covers <see cref="PulseService"/> dispatch: type-flag filtering, failure aggregation, ordering, logging.</summary>
    public sealed class PulseServiceTests
    {
        private static PulseService NewService() => new PulseService(null, false);

        [Test]
        public void Play_NullRequest_Fails()
        {
            Assert.That(NewService().Play((FeedbackRequest)null).Success, Is.False);
        }

        [Test]
        public void Play_NoneType_Fails()
        {
            FeedbackRequest request = new FeedbackRequest { Type = FeedbackType.None };
            Assert.That(NewService().Play(request).Success, Is.False);
        }

        [Test]
        public void Play_WithNoRegisteredPlayer_Fails()
        {
            FeedbackRequest request = new FeedbackRequest { Type = FeedbackType.Audio };
            Assert.That(NewService().Play(request).Success, Is.False);
        }

        [Test]
        public void Play_DispatchesOnlyToMatchingPlayers()
        {
            PulseService service = NewService();
            PulseTestUtil.FakePlayer audio = new PulseTestUtil.FakePlayer(FeedbackType.Audio);
            PulseTestUtil.FakePlayer vfx = new PulseTestUtil.FakePlayer(FeedbackType.Vfx);
            service.RegisterPlayer(audio);
            service.RegisterPlayer(vfx);

            FeedbackResult result = service.Play(new FeedbackRequest { Type = FeedbackType.Audio });

            Assert.That(result.Success, Is.True);
            Assert.That(audio.PlayCount, Is.EqualTo(1));
            Assert.That(vfx.PlayCount, Is.EqualTo(0), "non-matching player must not be invoked");
        }

        [Test]
        public void Play_CompoundType_InvokesEachMatchingPlayer()
        {
            PulseService service = NewService();
            PulseTestUtil.FakePlayer audio = new PulseTestUtil.FakePlayer(FeedbackType.Audio);
            PulseTestUtil.FakePlayer vfx = new PulseTestUtil.FakePlayer(FeedbackType.Vfx);
            service.RegisterPlayer(audio);
            service.RegisterPlayer(vfx);

            service.Play(new FeedbackRequest { Type = FeedbackType.Audio | FeedbackType.Vfx });

            Assert.That(audio.PlayCount, Is.EqualTo(1));
            Assert.That(vfx.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisterPlayer_ThroughServiceInterface_DispatchesCustomPayload()
        {
            IFeedbackService service = NewService();
            PulseTestUtil.PayloadCapturingPlayer custom = new PulseTestUtil.PayloadCapturingPlayer();
            service.RegisterPlayer(custom);

            object payload = new object();
            FeedbackResult result = service.Play(new FeedbackRequest { Type = FeedbackType.Custom, Payload = payload });

            Assert.That(result.Success, Is.True);
            Assert.That(custom.LastPayload, Is.SameAs(payload));
        }

        [Test]
        public void Play_CompoundWhereOnePlayerFailsAndAnotherSucceeds_Succeeds()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, succeeds: false));
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Vfx));

            FeedbackResult result = service.Play(new FeedbackRequest { Type = FeedbackType.Audio | FeedbackType.Vfx });

            Assert.That(result.Success, Is.True);
            Assert.That(result.Type, Is.EqualTo(FeedbackType.Audio | FeedbackType.Vfx));
        }

        [Test]
        public void Play_AllMatchingPlayersFail_MessageNamesTheLastFailure()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, false, "first failure"));
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, false, "last failure"));

            FeedbackResult result = service.Play(new FeedbackRequest { Type = FeedbackType.Audio });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("All matching players failed"));
            Assert.That(result.Message, Does.Contain("last failure"));
            Assert.That(result.Message, Does.Not.Contain("first failure"));
            Assert.That(result.Type, Is.EqualTo(FeedbackType.Audio));
        }

        [Test]
        public void Play_NoPlayerForTheType_MessageSaysSo()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Vfx));

            FeedbackResult result = service.Play(new FeedbackRequest { Type = FeedbackType.Hud });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("No player registered for feedback type 'Hud'."));
        }

        [Test]
        public void Play_ThrowingPlayer_IsReportedAsAFailureWithTheException()
        {
            PulseTestUtil.RecordingLogger logger = new PulseTestUtil.RecordingLogger();
            PulseService service = new PulseService(logger, false);
            service.RegisterPlayer(new PulseTestUtil.ThrowingPlayer());

            FeedbackResult result = service.Play(new FeedbackRequest { Type = FeedbackType.Audio });

            Assert.That(result.Success, Is.False);
            Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(logger.Errors, Has.Some.Contains("boom"));
        }

        [Test]
        public void RegisterPlayer_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => NewService().RegisterPlayer(null));
        }

        [Test]
        public void Play_InvokesPlayersInRegistrationOrder()
        {
            List<string> order = new List<string>();
            PulseService service = NewService();
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, order: order, name: "first"));
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, order: order, name: "second"));
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, order: order, name: "third"));

            service.Play(new FeedbackRequest { Type = FeedbackType.Audio });

            Assert.That(order, Is.EqualTo(new List<string> { "first", "second", "third" }));
        }

        [Test]
        public void Play_NullProfile_Fails()
        {
            FeedbackResult result = NewService().Play((FeedbackProfileSO)null);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Feedback profile is null."));
        }

        [Test]
        public void Logger_NullIsAcceptedEverywhere()
        {
            PulseService service = new PulseService(null, true);
            service.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio, false));

            Assert.DoesNotThrow(() => service.Play(new FeedbackRequest { Type = FeedbackType.Audio }));
            Assert.DoesNotThrow(() => service.Play(new FeedbackRequest { Type = FeedbackType.Vfx }));
        }

        [Test]
        public void Logger_VerboseControlsInfoAndFailuresAlwaysWarn()
        {
            PulseTestUtil.RecordingLogger quiet = new PulseTestUtil.RecordingLogger();
            PulseTestUtil.RecordingLogger loud = new PulseTestUtil.RecordingLogger();
            PulseService quietService = new PulseService(quiet, false);
            PulseService loudService = new PulseService(loud, true);

            quietService.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio));
            loudService.RegisterPlayer(new PulseTestUtil.FakePlayer(FeedbackType.Audio));
            quietService.Play(new FeedbackRequest { Type = FeedbackType.Audio });
            loudService.Play(new FeedbackRequest { Type = FeedbackType.Audio });
            quietService.Play(new FeedbackRequest { Type = FeedbackType.Vfx });

            Assert.That(quiet.Infos, Is.Empty);
            Assert.That(loud.Infos, Has.Some.Contains("Feedback player registered"));
            Assert.That(loud.Infos, Has.Some.Contains("Feedback played: Audio"));
            Assert.That(quiet.Warnings, Has.Some.Contains("No player registered"));
        }
    }
}
