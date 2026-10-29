using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Minimal VFX player that instantiates a prefab at the requested position.
    /// For v1 this is a fire-and-forget instantiation with auto-destroy support
    /// via <see cref="ParticleSystem.main"/> duration. If no prefab is provided
    /// the request is silently skipped.
    /// Fire-and-forget instances are always given a finite lifetime: a non-looping
    /// ParticleSystem drives the exact duration, anything else falls back to
    /// <see cref="FallbackLifetimeSeconds"/> so spawned objects can never leak.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "VfxFeedbackPlayer")]
    public sealed class VfxFeedbackPlayer : IFeedbackPlayer
    {
        /// <summary>
        /// Lifetime used when the prefab has no non-looping ParticleSystem to derive
        /// a duration from. Prevents fire-and-forget VFX from leaking GameObjects.
        /// </summary>
        public const float FallbackLifetimeSeconds = 10f;

        private readonly IPulseLogger _logger;

        public VfxFeedbackPlayer(IPulseLogger logger = null)
        {
            _logger = logger;
        }

        public bool CanPlay(FeedbackRequest request)
        {
            return request != null
                && (request.Type & FeedbackType.Vfx) != 0
                && request.VfxPrefab != null;
        }

        public FeedbackResult Play(FeedbackRequest request)
        {
            try
            {
                Vector3 position = request.Target != null
                    ? request.Target.position
                    : request.VfxPosition;

                GameObject instance = UnityEngine.Object.Instantiate(
                    request.VfxPrefab,
                    position,
                    Quaternion.identity);

                ParticleSystem ps = instance.GetComponent<ParticleSystem>();
                float lifetime = ps != null && !ps.main.loop
                    ? ps.main.duration + ps.main.startLifetime.constantMax
                    : FallbackLifetimeSeconds;

                // Always schedule destruction. A looping/PS-less prefab would otherwise
                // never be cleaned up since the caller holds no handle to it.
                UnityEngine.Object.Destroy(instance, lifetime);

                return FeedbackResult.Succeed(FeedbackType.Vfx, $"VFX '{request.VfxPrefab.name}' spawned.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"VFX feedback exception: {ex.Message}");
                return FeedbackResult.Fail($"VFX feedback failed: {ex.Message}", FeedbackType.Vfx, ex);
            }
        }
    }
}
