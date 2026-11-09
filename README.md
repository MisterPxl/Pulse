# Astra Pulse — Gameplay Feedback

`com.misterpxl.pulse` — Unity 6000.0+. Depends only on `com.unity.modules.audio` and `com.unity.modules.particlesystem`. No dependency on Echo, a tween package or any framework.

## Concepts

- `IFeedbackEntry`: one piece of feedback holding its own data. Built-in entries: `AudioFeedback` (clip, volume), `CameraShakeFeedback` (amplitude, duration, frequency), `HudFeedback` (message), `VfxFeedback` (prefab).
- `FeedbackRequest`: a list of entries plus the context they share: `Target`, `Position` (`ResolvePosition()` prefers the target) and `Intensity` (default 1, finite and non-negative), the scale a player applies to the strength of its entries.
- `IFeedbackPlayer<TEntry>`: plays one entry type. **This is the extension point**: a pack declares its own entry type and its player, registers the player, and edits nothing in Pulse.
- `PulseService` (`IFeedbackService`): plays each entry through every player registered for the entry's **exact runtime type**, in registration order. A request succeeds when at least one entry played; an entry with no player, a failing or a throwing player is logged and does not stop the other entries.
- `FeedbackProfileSO`: entries authored as an asset (`Astra/Pulse/Feedback Profile`), in a `[SerializeReference]` list.
- Built-in players: `VfxFeedbackPlayer`, and the optional `UnityAudioFeedbackPlayer` (`AudioSource.PlayClipAtPoint`). Camera shake and HUD entries are data only: register a player (or an adapter) that maps them onto your camera and HUD.

## Quick start

```csharp
using Astra.Pulse;

PulseService feedback = new PulseService();
feedback.RegisterPlayer(new VfxFeedbackPlayer());
feedback.RegisterPlayer(new UnityAudioFeedbackPlayer());

feedback.Play(new FeedbackRequest(new VfxFeedback(spark), new AudioFeedback(hitClip, 0.8f)) { Position = hitPoint });
feedback.Play(myProfile);
feedback.PlayAudio(myClip, 0.8f);
```

To play a profile at a position, build its request first: `FeedbackRequest request = myProfile.BuildRequest(); request.Target = enemy;`.

## Intensity

A profile is fixed data; the strength that depends on the gameplay moment travels on the request: `myProfile.BuildRequest(knockback / 10f)` or `request.Intensity = 0.5f`. Each player decides what intensity scales for its entry type: `UnityAudioFeedbackPlayer` plays at `Volume × Intensity` clamped to 1, and a consumer's camera-shake player typically multiplies the amplitude. `VfxFeedbackPlayer` ignores it. A pack player applies it to its own entries the same way.

## Adding a feedback type

```csharp
public sealed class ComboFeedback : IFeedbackEntry
{
    public ComboFeedback(int chain) { Chain = chain; }
    public int Chain { get; }
}

public sealed class ComboFeedbackPlayer : IFeedbackPlayer<ComboFeedback>
{
    public FeedbackResult Play(ComboFeedback entry, FeedbackRequest request)
    {
        // ...
        return FeedbackResult.Succeed();
    }
}

feedback.RegisterPlayer(new ComboFeedbackPlayer());   // TEntry inferred: ComboFeedback
feedback.Play(new ComboFeedback(3));
```

Players are resolved by the entry's exact runtime type: registering a player for an interface or an abstract entry type throws, and a player for `A` does not receive a subclass of `A`. A class implementing several `IFeedbackPlayer<T>` registers once per entry type with an explicit type argument (`RegisterPlayer<A>(player); RegisterPlayer<B>(player);`). Entries may be classes or structs.

## Authoring entries in a profile

The `FeedbackProfileSO` inspector (assembly `Astra.Pulse.Editor`) lists every entry type that a `[SerializeReference]` slot can hold under **Add Entry**: a concrete `[Serializable]` class implementing `IFeedbackEntry` with a parameterless constructor, from any assembly. Runtime-only entries (not `[Serializable]`, or structs) can still be played from code. The picker only draws the profile's list: a `[SerializeReference]` entry field in another component gets Unity's default drawer, which cannot create an instance, so reference a `FeedbackProfileSO` there instead. Profile entries are the asset's own instances; players must not mutate them.

## Tests

`Tests/EditMode` (`Astra.Pulse.Tests.EditMode`) and `Tests/PlayMode` (`Astra.Pulse.Tests.PlayMode`).
