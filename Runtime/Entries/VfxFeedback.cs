using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// VFX prefab spawned at <see cref="FeedbackRequest.ResolvePosition"/>. Played by <see cref="VfxFeedbackPlayer"/>.
    /// </summary>
    [Serializable]
    public sealed class VfxFeedback : IFeedbackEntry
    {
        [SerializeField] private GameObject _prefab;

        public VfxFeedback()
        {
        }

        public VfxFeedback(GameObject prefab)
        {
            _prefab = prefab;
        }

        public GameObject Prefab => _prefab;
    }
}
