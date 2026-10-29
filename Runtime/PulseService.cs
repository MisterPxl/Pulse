using System;
using System.Collections.Generic;

namespace Astra.Pulse
{
    /// <summary>
    /// Default <see cref="IFeedbackService"/>: dispatches a request to every registered player that
    /// can play it, succeeds when at least one does, and aggregates the failures otherwise.
    /// </summary>
    public sealed class PulseService : IFeedbackService
    {
        private readonly List<IFeedbackPlayer> _players = new List<IFeedbackPlayer>();
        private readonly IPulseLogger _logger;
        private readonly bool _verbose;

        public PulseService(IPulseLogger logger = null, bool verbose = false)
        {
            _logger = logger;
            _verbose = verbose;
        }

        public int PlayerCount => _players.Count;

        public void RegisterPlayer(IFeedbackPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            _players.Add(player);
            LogVerbose($"Feedback player registered: {player.GetType().Name}.");
        }

        public FeedbackResult Play(FeedbackRequest request)
        {
            if (request == null)
                return FeedbackResult.Fail("Feedback request is null.");

            if (request.Type == FeedbackType.None)
                return FeedbackResult.Fail("Feedback request has no type specified.");

            try
            {
                bool anyPlayed = false;
                FeedbackResult lastFailure = null;

                for (int i = 0; i < _players.Count; i++)
                {
                    IFeedbackPlayer player = _players[i];
                    if (!player.CanPlay(request)) continue;

                    FeedbackResult result = player.Play(request);
                    if (result.Success)
                    {
                        anyPlayed = true;
                        LogVerbose($"Feedback played: {request.Type} via {player.GetType().Name}.");
                    }
                    else
                    {
                        lastFailure = result;
                        _logger?.Warn($"Feedback player {player.GetType().Name} failed: {result.Message}");
                    }
                }

                if (!anyPlayed)
                {
                    string message = lastFailure != null
                        ? $"All matching players failed for feedback type '{request.Type}'. Last: {lastFailure.Message}"
                        : $"No player registered for feedback type '{request.Type}'.";

                    _logger?.Warn(message);
                    return FeedbackResult.Fail(message, request.Type);
                }

                return FeedbackResult.Succeed(request.Type, $"Feedback '{request.Type}' played successfully.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Feedback exception: {ex.Message}");
                return FeedbackResult.Fail($"Feedback failed with exception: {ex.Message}", request.Type, ex);
            }
        }

        public FeedbackResult Play(FeedbackProfileSO profile)
        {
            if (profile == null)
                return FeedbackResult.Fail("Feedback profile is null.");

            return Play(profile.BuildRequest());
        }

        private void LogVerbose(string message)
        {
            if (_verbose) _logger?.Info(message);
        }
    }
}
