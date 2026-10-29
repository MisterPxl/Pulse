# Astra Pulse — Gameplay Feedback

`com.misterpxl.pulse` — Unity 6000.0+. Depends only on `com.unity.modules.audio` and `com.unity.modules.particlesystem`. No dependency on Echo, a tween package or any framework.

## Concepts

- `FeedbackRequest`: what to trigger (`FeedbackType` flags, audio clip, `FeedbackShake`, HUD message and opaque `HudOverlayId`, VFX prefab and position, custom `Payload`).
- `FeedbackProfileSO`: the same data authored as an asset (`Astra/Pulse/Feedback Profile`).
- `IFeedbackPlayer`: executes one kind of feedback. **This is the extension point**: write a player, register it, no enum or switch to edit. Use `FeedbackType.Custom` with a typed `Payload` for new channels.
- `PulseService` (`IFeedbackService`): dispatches to every player that can play the request; succeeds if at least one does.
- Built-in players: `VfxFeedbackPlayer`, and the optional `UnityAudioFeedbackPlayer` (`AudioSource.PlayClipAtPoint`).

## Quick start

```csharp
using Astra.Pulse;

PulseService feedback = new PulseService();
feedback.RegisterPlayer(new VfxFeedbackPlayer());
feedback.RegisterPlayer(new UnityAudioFeedbackPlayer());

feedback.Play(myProfile);
feedback.PlayAudio(myClip, 0.8f);
```

Camera shake and HUD overlay are data only: register a player (or an adapter) that maps `FeedbackShake` / `HudOverlayId` onto your camera and HUD.

## Tests

`Tests/EditMode` (`Astra.Pulse.Tests.EditMode`) and `Tests/PlayMode` (`Astra.Pulse.Tests.PlayMode`).
