# H10 continuation: actual catalog residency and safe loading scope

Date: 2026-10-11 (Asia/Shanghai)
Status: SCOPED_REVIEW_COMPLETE / DIAGNOSTIC_HANDOFF_READY, not H10 complete.

User direction remains: finish current-stage H10 before another optimization. The latest question "为什么不继续处理" confirms continuation; ending an implementation subtask must not silently pause the H10 parent.

## Independent findings

- The previous formal session ownership task is applied. Its fixed-group result, source retirement, cancellation and deferred destruction evidence are retained; no rerun of its cost/resource group is needed.
- Current NTSDSoundPlayer.PrepareBattleCuesCoreAsync calls CharacterAnimtorManager.CollectBattleSoundIds for all loaded definitions, then unions BuiltInBattleSoundIds. It also prepares existing generic paths for formal cues. A measurement that substitutes a nonexistent soundRootFolder does not represent this full path.
- Current CollectBattleSoundIds reads three weapon sounds, frame sounds with the existing cap/legacy branch, and published mode knockout paths. Reuse that exact production collector; do not reconstruct a smaller, easier sound universe.
- Existing metadata supports catalog id/type, frame OPoint dependencies, KindCatalog bound/respond IDs and stage spawn IDs. Existing visual closure is not proof of audio reachability. Arbitrary external OID producers and unproven type0 transitions preclude admission of the previous player/non0 candidate. Do not read active Q06 method bodies or invent weapon-piece field semantics.
- Existing inventory: 330 definitions / 1000 cue IDs / 978 existing formal paths / 72 existing generic paths. Header estimate of final formal float data is 226264216 B. These counts must be freshly reconciled against unchanged inputs; the byte count is not a measured resident total or process peak.
- Previous H10 fixtures use an intentionally nonexistent generic root. Keep that evidence scoped. The next diagnostic must keep the real root, separate existing borrowed/cache objects from new session-owned and newly cached objects, and count each native ID once.
- NTSD_ResourceLoader.Instance is a private static readonly singleton. Do not replace it through reflection. Its current cache key and empty-key APIs remain production authority. A diagnostic may clean only exact new entries/objects introduced by its own otherwise-idle session, after proving identity and no pre-existing borrowers; it may never clear the existing cache.

## Next finite task

See execution-handoff.txt. Extend only the existing Editor diagnostic to obtain a complete audio-only baseline through public production PrepareBattleCuesAsync, a reference-based memory ledger, restoration checks, and a producer coverage report. No production filtering or arbitrary budget enforcement is authorized by this review. Full catalog audio measurement is newly included for this task under the user's whole-H10 continuation request; prior four/five-file task limits remain true for their historical evidence.

This review adds no scripts or production changes, runs no Unity/tests, and makes no new runtime or memory claim. Device, AB and IL2CPP remain deferred. Findings will determine the next accurate H10 change or a concrete budget decision, not another unrelated optimization.
