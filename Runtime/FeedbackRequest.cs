using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Feedback to trigger: typed entries, each played by the players registered for its type, and the
    /// spatial context they share. <c>new FeedbackRequest(new AudioFeedback(clip), new VfxFeedback(spark)) { Position = hit }</c>.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackRequest")]
    public sealed class FeedbackRequest
    {
        private readonly List<IFeedbackEntry> _entries = new List<IFeedbackEntry>();

        public FeedbackRequest(params IFeedbackEntry[] entries)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));

            for (int i = 0; i < entries.Length; i++)
                Add(entries[i]);
        }

        public IReadOnlyList<IFeedbackEntry> Entries => _entries;

        /// <summary>Anchor for spatial entries; when set, its position wins over <see cref="Position"/>.</summary>
        public Transform Target { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 ResolvePosition()
        {
            return Target != null ? Target.position : Position;
        }

        public FeedbackRequest Add(IFeedbackEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            _entries.Add(entry);
            return this;
        }
    }
}
