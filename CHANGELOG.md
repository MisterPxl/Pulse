# Changelog

## [1.0.0]

- Extracted from the Astra framework (`Game.Feedback`) as a standalone UPM package.
- Namespace `Astra.Pulse`; every moved type keeps its `[MovedFrom]` hop to `Game.Feedback` and every script keeps its `.meta` GUID.
- `FeedbackRequest` is neutral: `ShakeRequest` became `Shake` (`FeedbackShake`), `TargetOverlay` became `HudOverlayId` (`int?`). `FeedbackProfileSO._targetOverlay` is an `int` (same serialized integer).
- Added `IFeedbackService`, `PulseService`, `FeedbackServiceExtensions` (`PlayAudio`, `PlayShake`), `IPulseLogger` and the optional `UnityAudioFeedbackPlayer`.
- Menu: `Astra/Pulse/Feedback Profile`.
