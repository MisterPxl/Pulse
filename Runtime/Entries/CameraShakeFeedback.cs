using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>Camera-shake cue. Pulse owns no camera: the consumer registers a player mapping it onto its camera.</summary>
    [Serializable]
    public sealed class CameraShakeFeedback : IFeedbackEntry
    {
        [SerializeField, Min(0f)] private float _amplitude;
        [SerializeField, Min(0f)] private float _duration;
        [SerializeField, Min(0f)] private float _frequency = 25f;

        public CameraShakeFeedback()
        {
        }

        public CameraShakeFeedback(float amplitude, float duration, float frequency = 25f)
        {
            _amplitude = amplitude;
            _duration = duration;
            _frequency = frequency;
        }

        public float Amplitude => _amplitude;
        public float Duration => _duration;
        public float Frequency => _frequency;
    }
}
