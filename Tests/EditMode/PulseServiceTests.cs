using System;
using System.Collections.Generic;
using NUnit.Framework;
using static Astra.Pulse.Tests.PulseTestUtil;

namespace Astra.Pulse.Tests
{
    /// <summary>Covers <see cref="PulseService"/> dispatch: typed resolution, failure aggregation, ordering, logging.</summary>
    public sealed class PulseServiceTests
    {
        private static PulseService NewService() => new PulseService(null, false);

        [Test]
        public void Play_NullRequest_Fails()
        {
            Assert.That(NewService().Play((FeedbackRequest)null).Success, Is.False);
        }

        [Test]
        public void Play_EmptyRequest_Fails()
        {
            FeedbackResult result = NewService().Play(new FeedbackRequest());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Feedback request has no entries."));
        }

        [Test]
        public void Play_NoPlayerForTheEntry_MessageNamesTheEntryType()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new FakePlayer<BetaEntry>());

            FeedbackResult result = service.Play(new FeedbackRequest(new AlphaEntry()));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("No player registered for feedback entry 'AlphaEntry'."));
        }

        [Test]
        public void Play_DispatchesOnlyToPlayersOfTheEntryType()
        {
            PulseService service = NewService();
            FakePlayer<AlphaEntry> alpha = new FakePlayer<AlphaEntry>();
            FakePlayer<BetaEntry> beta = new FakePlayer<BetaEntry>();
            service.RegisterPlayer(alpha);
            service.RegisterPlayer(beta);

            FeedbackResult result = service.Play(new FeedbackRequest(new AlphaEntry()));

            Assert.That(result.Success, Is.True);
            Assert.That(alpha.PlayCount, Is.EqualTo(1));
            Assert.That(beta.PlayCount, Is.EqualTo(0), "a player of another entry type must not be invoked");
        }

        [Test]
        public void Play_HandsThePlayerTheTypedEntryAndTheRequest()
        {
            PulseService service = NewService();
            FakePlayer<AlphaEntry> player = new FakePlayer<AlphaEntry>();
            service.RegisterPlayer(player);
            AlphaEntry entry = new AlphaEntry();
            FeedbackRequest request = new FeedbackRequest(entry);

            service.Play(request);

            Assert.That(player.LastEntry, Is.SameAs(entry));
            Assert.That(player.LastRequest, Is.SameAs(request));
        }

        [Test]
        public void Play_SeveralEntries_InvokesEachEntryPlayer()
        {
            PulseService service = NewService();
            FakePlayer<AlphaEntry> alpha = new FakePlayer<AlphaEntry>();
            FakePlayer<BetaEntry> beta = new FakePlayer<BetaEntry>();
            service.RegisterPlayer(alpha);
            service.RegisterPlayer(beta);

            FeedbackResult result = service.Play(new FeedbackRequest(new AlphaEntry(), new BetaEntry(), new AlphaEntry()));

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("3 of 3 feedback entries played."));
            Assert.That(alpha.PlayCount, Is.EqualTo(2));
            Assert.That(beta.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void Play_ResolvesByTheExactRuntimeType()
        {
            PulseService service = NewService();
            FakePlayer<BaseEntry> basePlayer = new FakePlayer<BaseEntry>();
            service.RegisterPlayer(basePlayer);

            FeedbackResult result = service.Play(new FeedbackRequest(new DerivedEntry()));

            Assert.That(result.Success, Is.False);
            Assert.That(basePlayer.PlayCount, Is.EqualTo(0));
        }

        [Test]
        public void Play_ValueTypeEntry_ReachesItsPlayer()
        {
            PulseService service = NewService();
            FakePlayer<ValueEntry> player = new FakePlayer<ValueEntry>();
            service.RegisterPlayer(player);

            Assert.That(service.Play(new ValueEntry(7)).Success, Is.True);
            Assert.That(player.LastEntry.Value, Is.EqualTo(7));
        }

        [Test]
        public void RegisterPlayer_ThroughServiceInterface_DispatchesAPackEntry()
        {
            IFeedbackService service = NewService();
            FakePlayer<AlphaEntry> player = new FakePlayer<AlphaEntry>();
            service.RegisterPlayer(player);

            Assert.That(service.Play(new AlphaEntry()).Success, Is.True);
            Assert.That(player.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisterPlayer_ForAnInterfaceOrAbstractEntryType_Throws()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => NewService().RegisterPlayer(new FakePlayer<IFeedbackEntry>()));
            Assert.That(exception.Message, Does.Contain("not a concrete entry type"));
        }

        [Test]
        public void RegisterPlayer_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => NewService().RegisterPlayer<AlphaEntry>(null));
        }

        [Test]
        public void PlayerCount_CountsEveryRegistration()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new FakePlayer<AlphaEntry>());
            service.RegisterPlayer(new FakePlayer<AlphaEntry>());
            service.RegisterPlayer(new FakePlayer<BetaEntry>());

            Assert.That(service.PlayerCount, Is.EqualTo(3));
        }

        [Test]
        public void Play_OneEntryFailsAnotherPlays_Succeeds()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(succeeds: false));
            service.RegisterPlayer(new FakePlayer<BetaEntry>());

            FeedbackResult result = service.Play(new FeedbackRequest(new AlphaEntry(), new BetaEntry()));

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("1 of 2 feedback entries played."));
        }

        [Test]
        public void Play_OnePlayerOfAnEntryFailsAnotherSucceeds_EntryPlays()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(succeeds: false));
            service.RegisterPlayer(new FakePlayer<AlphaEntry>());

            Assert.That(service.Play(new AlphaEntry()).Success, Is.True);
        }

        [Test]
        public void Play_AllPlayersOfAnEntryFail_MessageNamesTheLastFailure()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(false, "first failure"));
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(false, "last failure"));

            FeedbackResult result = service.Play(new AlphaEntry());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("All players failed for feedback entry 'AlphaEntry'"));
            Assert.That(result.Message, Does.Contain("last failure"));
            Assert.That(result.Message, Does.Not.Contain("first failure"));
        }

        [Test]
        public void Play_ThrowingPlayer_IsReportedAsAFailureWithTheException()
        {
            RecordingLogger logger = new RecordingLogger();
            PulseService service = new PulseService(logger, false);
            service.RegisterPlayer(new ThrowingPlayer<AlphaEntry>());

            FeedbackResult result = service.Play(new AlphaEntry());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
            Assert.That(logger.Errors, Has.Some.Contains("boom"));
        }

        [Test]
        public void Play_ThrowingPlayer_DoesNotStopTheOtherEntries()
        {
            PulseService service = NewService();
            FakePlayer<BetaEntry> beta = new FakePlayer<BetaEntry>();
            service.RegisterPlayer(new ThrowingPlayer<AlphaEntry>());
            service.RegisterPlayer(beta);

            FeedbackResult result = service.Play(new FeedbackRequest(new AlphaEntry(), new BetaEntry()));

            Assert.That(result.Success, Is.True);
            Assert.That(beta.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void Play_PlayerReturningNoResult_IsAFailure()
        {
            PulseService service = NewService();
            service.RegisterPlayer(new NullResultPlayer<AlphaEntry>());

            FeedbackResult result = service.Play(new AlphaEntry());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("returned no result"));
        }

        [Test]
        public void Play_InvokesPlayersInRegistrationOrder()
        {
            List<string> order = new List<string>();
            PulseService service = NewService();
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(order: order, name: "first"));
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(order: order, name: "second"));
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(order: order, name: "third"));

            service.Play(new AlphaEntry());

            Assert.That(order, Is.EqualTo(new List<string> { "first", "second", "third" }));
        }

        [Test]
        public void Logger_NullIsAcceptedEverywhere()
        {
            PulseService service = new PulseService(null, true);
            service.RegisterPlayer(new FakePlayer<AlphaEntry>(false));
            service.RegisterPlayer(new ThrowingPlayer<AlphaEntry>());

            Assert.DoesNotThrow(() => service.Play(new AlphaEntry()));
            Assert.DoesNotThrow(() => service.Play(new BetaEntry()));
        }

        [Test]
        public void Logger_VerboseControlsInfoAndFailuresAlwaysWarn()
        {
            RecordingLogger quiet = new RecordingLogger();
            RecordingLogger loud = new RecordingLogger();
            PulseService quietService = new PulseService(quiet, false);
            PulseService loudService = new PulseService(loud, true);

            quietService.RegisterPlayer(new FakePlayer<AlphaEntry>());
            loudService.RegisterPlayer(new FakePlayer<AlphaEntry>());
            quietService.Play(new AlphaEntry());
            loudService.Play(new AlphaEntry());
            quietService.Play(new BetaEntry());

            Assert.That(quiet.Infos, Is.Empty);
            Assert.That(loud.Infos, Has.Some.StartsWith("Feedback player registered: "));
            Assert.That(loud.Infos, Has.Some.EndsWith(" for AlphaEntry."));
            Assert.That(loud.Infos, Has.Some.Contains("Feedback played: AlphaEntry"));
            Assert.That(quiet.Warnings, Has.Some.Contains("No player registered"));
        }
    }
}
