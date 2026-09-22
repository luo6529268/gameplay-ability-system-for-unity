# NTSD28-Q07-MODE-COMBO-R15-IDENTITY-IMPACT-001

Status: `R15_TRIGGER_CONFIRMED / VERSIONED_INTEGRATION_NOT_STARTED` (2026-09-22). Read-only impact audit after `NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001`; no production identity, World, Scene, menu or trace comparator was changed here.

## 2026-09-22 follow-up: published activation and current trace blockers

The status above records the pre-activation audit and is historical. `NTSD28-Q07-MODE-COMBO-PUBLISHED-ACTIVATION-001` has since written a five-component V2 production identity and applies the published tuple at the shared pre-tick seal. Its external pure-source check passed, but the original Unity Editor has not compiled or run the new scripts or focused test. Q07 and R15 remain open.

Read-only tracing of the current emitters found two independent same-version blockers:

1. `Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs` emits the V2 composite digest from `LoganContentIdentity` while retaining the V1 object/fusion-only property set and `catalog-object-fusion-definitions` scope. `Tools/NTSD28Parity/TraceContentIdentity.cs` validates only the exact three-component V1 header. `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp` also captures only object/fusion V1, although the formally built `game_session.cpp` loads and applies the selected mode tuple. The native runner is source-model diagnostic evidence, not a formal EXE trace declaration. A common-component report may describe the missing native mode counterpart; it cannot allow cross-version tick comparison.
2. Current Unity `BattleStateSnapshotBuffer.CurrentSchemaVersion` is 26 and `BattleLockstepChecksumModule.CurrentSchemaVersion` is 29. The parity tool and native diagnostic header still require aggregate 25/checksum 28 (entity 17, character/base shell 2/2 agree). Even a correct V2 mode header would therefore fail the same-schema gate. Historical 25/28 captures must not be relabeled as 26/29.

The next bounded R15 code package must declare the Unity emitter, parity validator/self-tests and fixed formal-vector probes before edits. It should make V1 and V2 header/property sets explicit and strict, preserve V1 fixtures, and reject V1/V2 or schema mismatches before tick comparison. A separate declared native diagnostic-capture update must capture mode bytes and freshness from the formal selected inputs and establish a supported schema contract before a new same-version source-model trace can be produced. Only original-project Unity compilation, focused tests and new same-seed/input evidence can close R15; neither the external Add-Type check nor old V1 capture does so.

## Why the tuple cannot be activated alone

The formal selected `data/mode/ntsd.dat` `<combo>` record supplies `bound=1, facing=1, respond=50, caughtact=1`. The pure `LoganModeComboInput` projection captures the selected parent/child DAT and produces input SHA-256 `9E9FAC26E92E91DE1F8823E3CDFEF7A38C444D90724566BC38F5CACF068075DD` and semantic SHA-256 `B92B74AD174178FCBB8C7B11834964BA784A487449AE4EFE401A8EE50B76079B`. `SimulationTickDriver.ApplyMatchConfig` resets World state before first tick, and the current production combo carrier stays disabled unless an explicit writer applies this tuple. If that writer changes gameplay while the published content identity still covers only object DAT and fusion, two differently configured battles can share the same content/session identity.

`LoganObjectCatalog` currently composes `LoganContentIdentity` from the object-definition, fusion-input and fusion-semantic fingerprints only. `LoganVisualContentCandidate.AssertInputsCurrent` rechecks object/fusion/images, not the new mode input. `LoganContentIdentity.CatalogFingerprint` enters `LockstepSessionIdentity`; its `IdentityFingerprint` is recorded across battle snapshots and checked on restore. This is the Q07 content-fingerprint-change trigger of alignment §0.14 R15, not a reason to redo Q05's closed schema migration or Q06 combo producer.

## Independent identity vectors for a versioned extension

The existing three components are object `4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C`, fusion input `28E1809EDE9C18E49629E6CBEC20CBD11FCB6E0175DF9DEE6ED90CE542886D5F`, and fusion semantic `81CA495386950C3F8D5F00B43A62934410D4F6F738CD7B720610241E88F8D369`. PowerShell SHA-256 over ASCII tag + NUL + component bytes reproduced the checked-in V1 vector exactly:

| Contract | Raw SHA-256 | Semantic SHA-256 under `NTSD28_LOGAN_DAT_SEMANTICS_V3` | LE first-eight-byte catalog fingerprint |
|---|---|---|---|
| existing `NTSD28_LOGAN_BATTLE_INPUTS_V1`, 3 components | `3A7FF5A15521B9766FC35BBF04B8FA9D3F4BDEA5C5D0045A578BA8B523C37CC4` | `FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147` | `0FEFD4B968D618FD` |
| proposed `NTSD28_LOGAN_BATTLE_INPUTS_V2`, append mode input + semantic | `33B0341C58D2A708B208F2CA4A0C4DA740E942D49A8A9D82E106F1D1D9C419BE` | `FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB` | `9F40EB3FFF1812FF` |

The V2 row is a **candidate contract calculation**, not implemented behavior or a claim about the formal native trace header. It keeps the current entity/aggregate/checksum/shell schema numbers; an actual migration must prove old content/session identity rejection and seed/input replay without silently changing those schemas. The three-argument `FromBattleComponents` path used by legacy/unconfigured fixtures must remain the V1 vector.

## Exact affected surfaces and exit

1. Capture `LoganModeComboInput` with the published `LoganObjectCatalog`, include its raw/semantic hashes in a versioned battle-content identity only when selected mode input exists, and recheck those inputs in `LoganVisualContentCandidate.AssertInputsCurrent`. This changes the formal candidate source cache key as a deliberate versioned content change; verify all publication owners still share one identity.
2. After `SimulationTickDriver.ApplyMatchConfig` resets the World and before any entity/tick, apply the published, current mode tuple to the existing `NTSD28NativeComboRuntimeState`. Do not alter Q06 ordinary/caughtact/expiry implementations. Preserve no-mode legacy behavior and protect reset/re-entry. `AppManager` already validates content before battle; direct-battle/bootstrap callers need a matching fail-closed check rather than silently using stale publication.
3. Review trace identity emitters `Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs` and `Tools/NTSD28Parity/TraceContentIdentity.cs`, plus `Tools/NTSD28Parity/TraceContractSelfTest.cs`. The native old object/fusion header must not be presented as if it already contains mode; compare the common object/fusion components and report the Unity-only mode component explicitly until an authoritative counterpart exists. Preserve first-difference diagnostics rather than filling a missing native mode field with zero.
4. Update fixed formal-fingerprint probes only with a freshly computed, versioned witness: `BattleComboPlayModeProbeEditor`, `NTSD28Q07CloneCentralPixelProbeEditor`, `NTSD28Q07MenuSceneCallbackPlayProbeEditor`, `NTSD28Q08FormalSourcePixelPlayModeTests`, `NTSD28B11SourceCallerPlayProbeEditor`, `NTSD28Q07WindowsPlayerRuntimeProbe`, `NTSD28Q07WindowsNaturalSkillProbe`, and formal-vector tests `NTSD28Q05TraceIdentityEditorTests` / `NTSD28Q06FusionCompositeContentIdentityEditorTests`. Keep synthetic V1 fixture expectations where mode is absent; inspect Q06 formal catalog-fingerprint assertions and generated trace fixtures for the same scope before editing.
5. Build and run focused parser/identity/publication/reset/ordinary/caughtact/restore checks, then the relevant original-project Scene/Player path and same seed/input formal trace. The original Editor assembly currently predates the new parser; external `Add-Type` is only a pure-code proxy. No second Unity project or computer-use is authorized.

Before any script edit, create one accurate Task/Change with all code and test paths actually selected, rollback and nonbattle boundaries. Do not treat the candidate V2 calculation or staged DAT hashes as a completed R15 return.
