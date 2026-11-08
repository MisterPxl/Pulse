using System;
using UnityEngine;

namespace Astra.Pulse
{
    /// <summary>
    /// Minimal VFX player that instantiates the entry's prefab at <see cref="FeedbackRequest.ResolvePosition"/>,
    /// fire-and-forget. Instances are always given a finite lifetime: a non-looping
    /// ParticleSystem drives the exact duration, anything else falls back to
    /// <see cref="FallbackLifetimeSeconds"/> so spawned objects can never leak.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Game.Feedback", "Game.Feedback", "VfxFeedbackPlayer")]
    public sealed class VfxFeedbackPlayer : IFeedbackPlayer<VfxFeedback>
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

        public FeedbackResult Play(VfxFeedback entry, FeedbackRequest request)
        {
            if (entry.Prefab == null)
                return FeedbackResult.Fail("VFX feedback has no prefab.");

            try
            {
                GameObject instance = UnityEngine.Object.Instantiate(
                    entry.Prefab,
                    request.ResolvePosition(),
                    Quaternion.identity);

                ParticleSystem ps = instance.GetComponent<ParticleSystem>();
                float lifetime = ps != null && !ps.main.loop
                    ? ps.main.duration + ps.main.startLifetime.constantMax
                    : FallbackLifetimeSeconds;

                // Always schedule destruction. A looping/PS-less prefab would otherwise
                // never be cleaned up since the caller holds no handle to it.
                UnityEngine.Object.Destroy(instance, lifetime);

                return FeedbackResult.Succeed($"VFX '{entry.Prefab.name}' spawned.");
            }
            catch (Exception ex)
            {
                _logger?.Error($"VFX feedback exception: {ex.Message}");
                return FeedbackResult.Fail($"VFX feedback failed: {ex.Message}", ex);
            }
        }
    }
}
