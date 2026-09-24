# D-024 coordinate-history carrier: state and version gate

Status: `READ_ONLY_VERIFIED / CARRIER_NOT_IMPLEMENTED`, 2026-09-24. This narrows the implementation gate in `COORDINATE-HISTORY-CONTRACT-DESIGN.md` and `POSITION-WRITER-OWNERSHIP-GATE.md`. It does not close the confirmed OID219 or fusion first differences.

## Current ownership

| Boundary | Current source behavior | Required when the source-rule X/Z carrier is introduced |
| --- | --- | --- |
| `NTSDEntityRuntime` | Battle precise `X/Y/Z` and integer `XInt/YInt/ZInt`; `SetPosition` sets precise only, `SyncIntegerPosition` truncates precise to int. | Reference precise and integer X/Z must be independently written/synchronized. A post-hoc inverse of battle coordinates fails the equal-current-gap history counterexample. A direct integer assignment must not be silently overwritten by a later generic sync. |
| Canonical copy and pool reuse | `TryCopyCanonicalStateTo` copies battle precise/int fields; `Reset` clears them. | Copy the new carrier for entity and raw runtime snapshots, and clear it on reuse. A cache outside this path would be lost on restore. |
| Entity-runtime snapshot | `BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion = 17`; capture and restore use `TryCopyCanonicalStateTo` for both entity and raw runtime. | Increment the payload version when fields are added; test entity/raw independence and restore, including values that differ from battle coordinates. |
| Aggregate battle snapshot | `BattleStateSnapshotBuffer.CurrentSchemaVersion = 28`; validity checks the entity-runtime component header. `BattleStateSnapshotRestore` rejects an invalid or wrong-version aggregate. | Increment aggregate version with the payload change. Pre-change snapshots must fail as version-incompatible rather than silently restore without history. |
| Checksum and session | `BattleLockstepChecksumModule.CurrentSchemaVersion = 31`; `AppendEntityRuntime` hashes battle X/XInt/Y/YInt/Z/ZInt only. `BattleLockstepSession` checks the checksum schema at replay/history acceptance. | Hash reference precise/int X/Z in stable order and increment checksum schema. Verify a reference-only change alters checksum while identical seed/input/tick remains deterministic. |

## Test impact and acceptance

The existing `BattleWorldEntityRuntimeSnapshotEditorTests` already tests canonical copy and entity/raw separation, but its field inventory does not yet include a reference coordinate. New focused cases must explicitly set battle and reference positions to different values, capture, mutate, restore and compare. A second case must reset/reuse the runtime. A checksum case must change only the reference coordinate and observe a checksum difference. A replay case must cross a movement plus snapshot boundary, not merely copy a freshly initialized field.

Some historical tests already pin old schema constants (`15/23/26` or `11/23/26`) while current production constants are `17/28/31`, for example `NTSD28B5Kind4SourceCountCarrierEditorTests`, `NTSD28B5SpecialHitLatchCarrierEditorTests` and `NTSD28C23C24WorldClockEditorTests`. Those assertions are stale **before** this package; a global test run would mix pre-existing version failures with the new carrier's result. Change only the focused current-contract tests when the carrier is implemented, and report historical failures separately rather than mass-editing unrelated tests.

The carrier cannot be added as an unsnapshotted intermediate field. The next script package must cover runtime storage, birth/sync writers, canonical copy/reset, component and aggregate snapshot versions, checksum version, and focused state/replay tests in a declared Change Record. It must not switch OID219, fusion or other readers until their writer histories and stage-edge/attachment semantics are validated. Q07 stays behind this unresolved D-024 work.
