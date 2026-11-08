using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>One-shot audio clip. Played by <see cref="UnityAudioFeedbackPlayer"/> or a consumer's audio adapter.</summary>
    [Serializable]
    public sealed class AudioFeedback : IFeedbackEntry
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Min(0f)] private float _volume = 1f;

        public AudioFeedback()
        {
        }

        public AudioFeedback(AudioClip clip, float volume = 1f)
        {
            _clip = clip;
            _volume = volume;
        }

        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }
}
