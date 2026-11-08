using System;
using System.Collections.Generic;

namespace Astra.Pulse
{
    /// <summary>
    /// Default <see cref="IFeedbackService"/>: plays each entry of a request through every player registered
    /// for the entry's exact runtime type, in registration order. A request succeeds when at least one entry
    /// played; failures are logged and the last one is returned otherwise.
    /// </summary>
    public sealed class PulseService : IFeedbackService
    {
        private readonly Dictionary<Type, List<IEntryPlayer>> _players = new Dictionary<Type, List<IEntryPlayer>>();
        private readonly IPulseLogger _logger;
        private readonly bool _verbose;

        public PulseService(IPulseLogger logger = null, bool verbose = false)
        {
            _logger = logger;
            _verbose = verbose;
        }

        public int PlayerCount { get; private set; }

        public void RegisterPlayer<TEntry>(IFeedbackPlayer<TEntry> player) where TEntry : IFeedbackEntry
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            Type entryType = typeof(TEntry);
            if (entryType.IsInterface || entryType.IsAbstract)
            {
                throw new ArgumentException(
                    $"Feedback players are resolved by the entry's runtime type; '{entryType.Name}' is not a concrete entry type.",
                    nameof(player));
            }

            if (!_players.TryGetValue(entryType, out List<IEntryPlayer> players))
            {
                players = new List<IEntryPlayer>();
                _players.Add(entryType, players);
            }

            players.Add(new EntryPlayer<TEntry>(player));
            PlayerCount++;
            LogVerbose($"Feedback player registered: {player.GetType().Name} for {entryType.Name}.");
        }

        public FeedbackResult Play(FeedbackRequest request)
        {
            if (request == null)
                return FeedbackResult.Fail("Feedback request is null.");

            if (request.Entries.Count == 0)
                return FeedbackResult.Fail("Feedback request has no entries.");

            int played = 0;
            FeedbackResult lastFailure = null;

            for (int i = 0; i < request.Entries.Count; i++)
            {
                FeedbackResult result = PlayEntry(request.Entries[i], request);
                if (result.Success)
                    played++;
                else
                    lastFailure = result;
            }

            return played > 0
                ? FeedbackResult.Succeed($"{played} of {request.Entries.Count} feedback entries played.")
                : lastFailure;
        }

        private FeedbackResult PlayEntry(IFeedbackEntry entry, FeedbackRequest request)
        {
            string entryName = entry.GetType().Name;
            if (!_players.TryGetValue(entry.GetType(), out List<IEntryPlayer> players))
            {
                string message = $"No player registered for feedback entry '{entryName}'.";
                _logger?.Warn(message);
                return FeedbackResult.Fail(message);
            }

            bool anyPlayed = false;
            FeedbackResult lastFailure = null;

            for (int i = 0; i < players.Count; i++)
            {
                IEntryPlayer player = players[i];
                FeedbackResult result;
                try
                {
                    result = player.Play(entry, request)
                        ?? FeedbackResult.Fail($"Feedback player {player.Name} returned no result.");
                }
                catch (Exception ex)
                {
                    _logger?.Error($"Feedback player {player.Name} threw: {ex.Message}");
                    lastFailure = FeedbackResult.Fail($"Feedback player {player.Name} threw: {ex.Message}", ex);
                    continue;
                }

                if (result.Success)
                {
                    anyPlayed = true;
                    LogVerbose($"Feedback played: {entryName} via {player.Name}.");
                }
                else
                {
                    lastFailure = result;
                    _logger?.Warn($"Feedback player {player.Name} failed: {result.Message}");
                }
            }

            return anyPlayed
                ? FeedbackResult.Succeed()
                : FeedbackResult.Fail(
                    $"All players failed for feedback entry '{entryName}'. Last: {lastFailure.Message}",
                    lastFailure.Exception);
        }

        private void LogVerbose(string message)
        {
            if (_verbose) _logger?.Info(message);
        }

        private interface IEntryPlayer
        {
            string Name { get; }
            FeedbackResult Play(IFeedbackEntry entry, FeedbackRequest request);
        }

        private sealed class EntryPlayer<TEntry> : IEntryPlayer where TEntry : IFeedbackEntry
        {
            private readonly IFeedbackPlayer<TEntry> _player;

            public EntryPlayer(IFeedbackPlayer<TEntry> player)
            {
                _player = player;
                Name = player.GetType().Name;
            }

            public string Name { get; }

            public FeedbackResult Play(IFeedbackEntry entry, FeedbackRequest request)
            {
                return _player.Play((TEntry)entry, request);
            }
        }
    }
}
