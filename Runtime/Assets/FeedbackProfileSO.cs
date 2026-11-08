using System.Collections.Generic;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Data-driven feedback definition: a list of typed entries authored as an asset and played with
    /// <see cref="FeedbackServiceExtensions.Play(IFeedbackService, FeedbackProfileSO)"/>, or turned into a
    /// request with <see cref="BuildRequest"/> to add a position or a target first.
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackProfile", menuName = "Astra/Pulse/Feedback Profile")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackProfileSO")]
    public sealed class FeedbackProfileSO : ScriptableObject
    {
        [SerializeReference] private List<IFeedbackEntry> _entries = new List<IFeedbackEntry>();

        public IReadOnlyList<IFeedbackEntry> Entries => _entries;

        /// <summary>
        /// Builds a request with the non-empty entries. The entries are the asset's own instances:
        /// players must not mutate them.
        /// </summary>
        public FeedbackRequest BuildRequest()
        {
            FeedbackRequest request = new FeedbackRequest();
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i] != null)
                    request.Add(_entries[i]);
            }

            return request;
        }
    }
}
