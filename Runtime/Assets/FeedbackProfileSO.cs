using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Data-driven feedback definition. Authored as an asset and consumed by
    /// <see cref="IFeedbackService.Play(FeedbackProfileSO)"/>, which converts
    /// it into a <see cref="FeedbackRequest"/> at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "FeedbackProfile", menuName = "Astra/Pulse/Feedback Profile")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "FeedbackProfileSO")]
    public sealed class FeedbackProfileSO : ScriptableObject
    {
        /// <summary>Historical default overlay (the framework's <c>InteractionPrompt</c>).</summary>
        public const int DefaultOverlayId = 3;

        [Header("Type")]
        [SerializeField] private FeedbackType _type = FeedbackType.None;

        [Header("Audio")]
        [SerializeField] private AudioClip _clip;
        [SerializeField, Min(0f)] private float _volume = 1f;

        [Header("Camera Shake")]
        [SerializeField, Min(0f)] private float _shakeAmplitude;
        [SerializeField, Min(0f)] private float _shakeDuration;
        [SerializeField, Min(0f)] private float _shakeFrequency = 25f;

        [Header("HUD")]
        [SerializeField] private string _hudMessage;
        [SerializeField] private bool _useTargetOverlay;
        [Tooltip("Opaque overlay id forwarded to the HUD adapter (framework HudOverlayType: None 0, Pause 1, Fade 2, InteractionPrompt 3, GameplayBlocker 4).")]
        [SerializeField] private int _targetOverlay = DefaultOverlayId;

        [Header("VFX")]
        [SerializeField] private GameObject _vfxPrefab;

        public FeedbackType Type => _type;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
        public float ShakeAmplitude => _shakeAmplitude;
        public float ShakeDuration => _shakeDuration;
        public float ShakeFrequency => _shakeFrequency;
        public string HudMessage => _hudMessage;
        public int? TargetOverlay => _useTargetOverlay ? _targetOverlay : (int?)null;
        public GameObject VfxPrefab => _vfxPrefab;

        public FeedbackRequest BuildRequest()
        {
            FeedbackRequest request = new FeedbackRequest
            {
                Type = _type,
                AudioClip = _clip,
                AudioVolume = _volume,
                HudMessage = _hudMessage,
                HudOverlayId = TargetOverlay,
                VfxPrefab = _vfxPrefab
            };

            if (_shakeAmplitude > 0f && _shakeDuration > 0f)
            {
                request.Shake = new FeedbackShake(
                    _shakeAmplitude, _shakeDuration, _shakeFrequency);
            }

            return request;
        }
    }
}
