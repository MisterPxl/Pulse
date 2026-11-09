using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Feedback to trigger: typed entries, each played by the players registered for its type, and the
    /// context they share. <c>new FeedbackRequest(new AudioFeedback(clip), new VfxFeedback(spark)) { Position = hit, Intensity = 0.5f }</c>.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackRequest")]
    public sealed class FeedbackRequest
    {
        private readonly List<IFeedbackEntry> _entries = new List<IFeedbackEntry>();
        private float _intensity = 1f;

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

        /// <summary>
        /// Scale players apply to the strength of their entries (shake amplitude, volume...); 1 plays the
        /// entries as authored. Finite and non-negative.
        /// </summary>
        public float Intensity
        {
            get => _intensity;
            set
            {
                if (!(value >= 0f) || float.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Feedback intensity must be finite and non-negative.");

                _intensity = value;
            }
        }

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
