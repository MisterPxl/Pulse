using System.Collections.Generic;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Data-driven feedback definition: a list of typed entries authored as an asset and played with
    /// <see cref="FeedbackServiceExtensions.Play(IFeedbackService, FeedbackProfileSO)"/>, or turned into a
    /// request with <see cref="BuildRequest"/> to add a position, a target or an intensity first.
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackProfile", menuName = "Astra/Pulse/Feedback Profile")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackProfileSO")]
    public sealed class FeedbackProfileSO : ScriptableObject
    {
        [SerializeReference] private List<IFeedbackEntry> _entries = new List<IFeedbackEntry>();

        public IReadOnlyList<IFeedbackEntry> Entries => _entries;

        /// <summary>
        /// Builds a request with the non-empty entries and <paramref name="intensity"/> as its
        /// <see cref="FeedbackRequest.Intensity"/>. The entries are the asset's own instances: players must
        /// not mutate them.
        /// </summary>
        public FeedbackRequest BuildRequest(float intensity = 1f)
        {
            FeedbackRequest request = new FeedbackRequest { Intensity = intensity };
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] != null)
                    request.Add(_entries[i]);
            }

            return request;
        }
    }
}
