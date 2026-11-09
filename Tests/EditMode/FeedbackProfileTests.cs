using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using static Astra.Pulse.Tests.PulseTestUtil;

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

        private static void SetEntries(FeedbackProfileSO profile, params IFeedbackEntry[] entries)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty list = serialized.FindProperty("_entries");
            list.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
                list.GetArrayElementAtIndex(i).managedReferenceValue = entries[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void BuildRequest_EmptyProfileHasNoEntries()
        {
            Assert.That(_profile.Entries, Is.Empty);
            Assert.That(_profile.BuildRequest().Entries, Is.Empty);
        }

        [Test]
        public void BuildRequest_KeepsTheEntryOrderAndSkipsEmptySlots()
        {
            HudFeedback hud = new HudFeedback("Hello");
            CameraShakeFeedback shake = new CameraShakeFeedback(0.4f, 0.3f, 30f);
            SetEntries(_profile, hud, null, shake);

            FeedbackRequest request = _profile.BuildRequest();

            Assert.That(_profile.Entries, Has.Count.EqualTo(3));
            Assert.That(request.Entries, Has.Count.EqualTo(2));
            Assert.That(((HudFeedback)request.Entries[0]).Message, Is.EqualTo("Hello"));
            Assert.That(((CameraShakeFeedback)request.Entries[1]).Frequency, Is.EqualTo(30f));
        }

        [Test]
        public void BuildRequest_CarriesTheIntensityDefaultingToOne()
        {
            SetEntries(_profile, new HudFeedback("Hello"));

            Assert.That(_profile.BuildRequest().Intensity, Is.EqualTo(1f));
            Assert.That(_profile.BuildRequest(0.25f).Intensity, Is.EqualTo(0.25f));
            Assert.That(_profile.BuildRequest(0.25f).Entries, Has.Count.EqualTo(1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _profile.BuildRequest(-1f));
        }

        [Test]
        public void Entries_SurviveSerializationWithTheirTypeAndData()
        {
            SetEntries(_profile, new AudioFeedback(null, 0.5f), new CameraShakeFeedback(0.4f, 0.3f), new AlphaEntry());
            FeedbackProfileSO copy = ScriptableObject.CreateInstance<FeedbackProfileSO>();
            try
            {
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(_profile), copy);

                Assert.That(copy.Entries, Has.Count.EqualTo(3));
                Assert.That(copy.Entries[0], Is.TypeOf<AudioFeedback>());
                Assert.That(((AudioFeedback)copy.Entries[0]).Volume, Is.EqualTo(0.5f));
                Assert.That(((CameraShakeFeedback)copy.Entries[1]).Amplitude, Is.EqualTo(0.4f));
                Assert.That(copy.Entries[2], Is.TypeOf<AlphaEntry>(), "a pack entry type is serialized like a built-in one");
            }
            finally
            {
                Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void ServiceDispatchesTheProfileEntries()
        {
            PulseService service = new PulseService();
            FakePlayer<HudFeedback> player = new FakePlayer<HudFeedback>();
            service.RegisterPlayer(player);
            SetEntries(_profile, new HudFeedback("Hi"));

            FeedbackResult result = service.Play(_profile);

            Assert.That(result.Success, Is.True);
            Assert.That(player.LastEntry.Message, Is.EqualTo("Hi"));
        }
    }
}
