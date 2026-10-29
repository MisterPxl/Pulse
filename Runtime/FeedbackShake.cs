namespace Astra.Pulse
{
    /// <summary>Camera-shake cue. The framework adapter maps it onto its own camera service.</summary>
    public readonly struct FeedbackShake
    {
        public readonly float Amplitude;
        public readonly float Duration;
        public readonly float Frequency;

        public FeedbackShake(float amplitude, float duration, float frequency = 25f)
        {
            Amplitude = amplitude;
            Duration = duration;
            Frequency = frequency;
        }
    }
}
