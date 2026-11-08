using System;
using System.Collections.Generic;

namespace Astra.Pulse.Tests
{
    internal static class PulseTestUtil
    {
        /// <summary>Records every message; <see cref="InfoEnabled"/> is configurable.</summary>
        public sealed class RecordingLogger : IPulseLogger
        {
            public readonly List<string> Infos = new List<string>();
            public readonly List<string> Warnings = new List<string>();
            public readonly List<string> Errors = new List<string>();

            public bool InfoEnabled = true;

            public bool IsInfoEnabled => InfoEnabled;
            public void Info(string message) { Infos.Add(message); }
            public void Warn(string message) { Warnings.Add(message); }
            public void Error(string message) { Errors.Add(message); }
        }

        [Serializable]
        public sealed class AlphaEntry : IFeedbackEntry
        {
        }

        [Serializable]
        public sealed class BetaEntry : IFeedbackEntry
        {
        }

        [Serializable]
        public class BaseEntry : IFeedbackEntry
        {
        }

        [Serializable]
        public sealed class DerivedEntry : BaseEntry
        {
        }

        public readonly struct ValueEntry : IFeedbackEntry
        {
            public readonly int Value;

            public ValueEntry(int value)
            {
                Value = value;
            }
        }

        /// <summary>Plays <typeparamref name="TEntry"/> and records its calls into a shared order list.</summary>
        public sealed class FakePlayer<TEntry> : IFeedbackPlayer<TEntry> where TEntry : IFeedbackEntry
        {
            private readonly bool _succeeds;
            private readonly string _failureMessage;
            private readonly List<string> _order;
            private readonly string _name;

            public int PlayCount { get; private set; }
            public TEntry LastEntry { get; private set; }
            public FeedbackRequest LastRequest { get; private set; }

            public FakePlayer(bool succeeds = true, string failureMessage = "failed", List<string> order = null, string name = null)
            {
                _succeeds = succeeds;
                _failureMessage = failureMessage;
                _order = order;
                _name = name;
            }

            public FeedbackResult Play(TEntry entry, FeedbackRequest request)
            {
                PlayCount++;
                LastEntry = entry;
                LastRequest = request;
                _order?.Add(_name);
                return _succeeds ? FeedbackResult.Succeed() : FeedbackResult.Fail(_failureMessage);
            }
        }

        public sealed class ThrowingPlayer<TEntry> : IFeedbackPlayer<TEntry> where TEntry : IFeedbackEntry
        {
            public FeedbackResult Play(TEntry entry, FeedbackRequest request) => throw new InvalidOperationException("boom");
        }

        public sealed class NullResultPlayer<TEntry> : IFeedbackPlayer<TEntry> where TEntry : IFeedbackEntry
        {
            public FeedbackResult Play(TEntry entry, FeedbackRequest request) => null;
        }
    }
}
