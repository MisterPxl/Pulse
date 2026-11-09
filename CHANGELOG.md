# Changelog

## [2.0.0]

Typed feedback entries replace the closed feedback enum. Breaking:

- Removed `FeedbackType`, `FeedbackShake`, `FeedbackRequest.Type`, `AudioClip`, `AudioVolume`, `Shake`, `HudMessage`, `HudOverlayId`, `VfxPrefab`, `VfxPosition`, `Payload`, `FeedbackResult.Type`, `FeedbackProfileSO.DefaultOverlayId` and the profile's flat fields (`_type`, `_clip`, `_volume`, `_shake*`, `_hudMessage`, `_useTargetOverlay`, `_targetOverlay`, `_vfxPrefab`). Profiles authored with 1.x lose their content and must be re-authored.
- Added `IFeedbackEntry` and the built-in entries `AudioFeedback`, `CameraShakeFeedback`, `HudFeedback` (message only) and `VfxFeedback`. A pack adds its own entry type instead of a `Custom` payload.
- `IFeedbackPlayer` is now `IFeedbackPlayer<TEntry>` with a single `Play(TEntry entry, FeedbackRequest request)`; `CanPlay` is gone (a player rejects invalid entry data by returning a failure).
- `IFeedbackService.RegisterPlayer<TEntry>(IFeedbackPlayer<TEntry>)` registers a player for the entries of that exact runtime type; interface or abstract entry types throw. `IFeedbackService.Play(FeedbackProfileSO)` moved to an extension method.
- `FeedbackRequest(params IFeedbackEntry[])`, `Add`, `Entries`, `Target`, `Position`, `ResolvePosition()`. Null entries throw.
- `PulseService` plays every entry, isolates a throwing player instead of aborting the request, and succeeds when at least one entry played (`"n of m feedback entries played."`).
- `FeedbackResult.Succeed(string message = null)` / `Fail(string message, Exception exception = null)`.
- `FeedbackServiceExtensions`: `Play(IFeedbackEntry)` and `Play(FeedbackProfileSO)` added; `PlayAudio` / `PlayShake` build typed entries.
- `FeedbackProfileSO` holds a `[SerializeReference] List<IFeedbackEntry>`; `BuildRequest(float intensity = 1f)` skips empty slots and sets the request intensity.
- `FeedbackRequest.Intensity` (default 1; negative, NaN or infinite values throw): the scale players apply to the strength of their entries. `UnityAudioFeedbackPlayer` plays at `Volume × Intensity` clamped to 1.
- New editor assembly `Astra.Pulse.Editor`: profile inspector with an **Add Entry** type menu (`FeedbackEntryTypes`).

## [1.0.0]

- Extracted from the Astra framework (`Game.Feedback`) as a standalone UPM package.
- Namespace `Astra.Pulse`; every moved type keeps its `[MovedFrom]` hop to `Game.Feedback` and every script keeps its `.meta` GUID.
- `FeedbackRequest` is neutral: `ShakeRequest` became `Shake` (`FeedbackShake`), `TargetOverlay` became `HudOverlayId` (`int?`). `FeedbackProfileSO._targetOverlay` is an `int` (same serialized integer).
- Added `IFeedbackService`, `PulseService`, `FeedbackServiceExtensions` (`PlayAudio`, `PlayShake`), `IPulseLogger` and the optional `UnityAudioFeedbackPlayer`.
- Menu: `Astra/Pulse/Feedback Profile`.
