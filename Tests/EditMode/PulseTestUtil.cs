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

        /// <summary>Player that handles the given flags and records its calls into a shared order list.</summary>
        public sealed class FakePlayer : IFeedbackPlayer
        {
            private readonly FeedbackType _handles;
            private readonly bool _succeeds;
            private readonly string _failureMessage;
            private readonly List<string> _order;
            private readonly string _name;

            public int PlayCount { get; private set; }
            public FeedbackRequest LastRequest { get; private set; }

            public FakePlayer(FeedbackType handles, bool succeeds = true, string failureMessage = "failed",
                List<string> order = null, string name = null)
            {
                _handles = handles;
                _succeeds = succeeds;
                _failureMessage = failureMessage;
                _order = order;
                _name = name;
            }

            public bool CanPlay(FeedbackRequest request) => (request.Type & _handles) != 0;

            public FeedbackResult Play(FeedbackRequest request)
            {
                PlayCount++;
                LastRequest = request;
                _order?.Add(_name);
                return _succeeds
                    ? FeedbackResult.Succeed(request.Type)
                    : FeedbackResult.Fail(_failureMessage, request.Type);
            }
        }

        public sealed class ThrowingPlayer : IFeedbackPlayer
        {
            public bool CanPlay(FeedbackRequest request) => true;
            public FeedbackResult Play(FeedbackRequest request) => throw new InvalidOperationException("boom");
        }

        public sealed class PayloadCapturingPlayer : IFeedbackPlayer
        {
            public object LastPayload { get; private set; }

            public bool CanPlay(FeedbackRequest request) => (request.Type & FeedbackType.Custom) != 0;

            public FeedbackResult Play(FeedbackRequest request)
            {
                LastPayload = request.Payload;
                return FeedbackResult.Succeed(request.Type);
            }
        }
    }
}
