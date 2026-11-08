using System.Collections.Generic;
using Astra.Pulse.Editor;
using NUnit.Framework;
using static Astra.Pulse.Tests.PulseTestUtil;

namespace Astra.Pulse.Tests
{
    public sealed class FeedbackEntryTypesTests
    {
        [Test]
        public void GetAuthorable_ListsBuiltInAndPackEntries()
        {
            IReadOnlyList<System.Type> types = FeedbackEntryTypes.GetAuthorable();

            Assert.That(types, Does.Contain(typeof(AudioFeedback)));
            Assert.That(types, Does.Contain(typeof(CameraShakeFeedback)));
            Assert.That(types, Does.Contain(typeof(HudFeedback)));
            Assert.That(types, Does.Contain(typeof(VfxFeedback)));
            Assert.That(types, Does.Contain(typeof(AlphaEntry)), "an entry from another assembly is listed without registration");
        }

        [Test]
        public void IsAuthorable_RejectsWhatASerializeReferenceSlotCannotHold()
        {
            Assert.That(FeedbackEntryTypes.IsAuthorable(typeof(ValueEntry)), Is.False, "struct");
            Assert.That(FeedbackEntryTypes.IsAuthorable(typeof(RuntimeOnlyEntry)), Is.False, "not [Serializable]");
            Assert.That(FeedbackEntryTypes.IsAuthorable(typeof(AbstractEntry)), Is.False, "abstract");
            Assert.That(FeedbackEntryTypes.IsAuthorable(typeof(NoDefaultConstructorEntry)), Is.False, "no parameterless constructor");
            Assert.That(FeedbackEntryTypes.IsAuthorable(typeof(IFeedbackEntry)), Is.False, "interface");
            Assert.That(FeedbackEntryTypes.IsAuthorable(typeof(BaseEntry)), Is.True);
        }

        private sealed class RuntimeOnlyEntry : IFeedbackEntry
        {
        }

        [System.Serializable]
        private abstract class AbstractEntry : IFeedbackEntry
        {
        }

        [System.Serializable]
        private sealed class NoDefaultConstructorEntry : IFeedbackEntry
        {
            public NoDefaultConstructorEntry(int value)
            {
            }
        }
    }
}
