# NTSD28-Q09-P13-EARTHQUAKE-STATE-PUBLICATION-001

Status: `IN_PROGRESS` before any script edit, 2026-09-27. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-13`; Q07 remains the earliest aggregate open group. This package owns only the deterministic earthquake state producer, snapshot/checksum carrier and immutable frame publication. The later project-background drawing consumer, Unity Play pixels and formal-root same-frame pixels remain separate P-13 exits.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; paired playable `GameSession28::step` calls `earthquake_.step(*world_)` after World/KO work and before snapshot (`game_session.cpp:3305,3352`); `NativeEarthquake28::step` releases an owner outside 50000..59999 without clearing offsets, then scans active slots ascending and accepts state `5XXYY` only for unowned/current owner (`render_snapshot.cpp:1056-1097`). Current formal Han action0→catch→frame150/151 paired Session has background (+2,0)→(0,0), selected root LFR 400/400 entity/relationship fields, with root pixel/offset trace limitation in `artifacts/diagnostics/NTSD28-Q09-HAN-NATURAL-EARTHQUAKE-LFR-001/ACCEPTANCE.md`.

Exact script ownership:

- `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`: battle-scoped owner/offset state, reset and restore.
- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`: advance once after the complete successful World/result tail and before `RenderDispatch`, independent of `buildPresentation`; no update on non-completed/transition early return.
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`, `BattleStateSnapshot.cs`, `BattleStateSnapshotRestore.cs`: versioned owner/offset capture and exact restore across owner release.
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`: deterministic owner/offset checksum and schema revision.
- `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`: copy the completed-tick owner/offset into the published and frozen frame, including worker frame capture; reset stale fields.
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeStatePublicationEditorTests.cs` and its Unity `.meta`: focused owner priority, offset retention, snapshot/restore/checksum, frame copy/reset and no-presentation tick. The test may use a formal-state fixture but cannot claim natural Battle Play.

Invariants: world/entity coordinates, camera, collision, stage boundary and other battle render commands remain unchanged; background offset is not multiplied by the user's entity-motion display ratio because it belongs to the formal background-only draw domain. No original background/mode DAT or old resource is loaded/moved/deleted. No DAT values, Scene, Prefab, ProjectSettings, nonbattle code or formal source/EXE edit. The existing fixed-background camera and project map remain.

Validation: test-first focused RED and GREEN if original Editor route is available; otherwise record compile/focused as pending. Generated `Assembly-CSharp` build is a lower layer than Editor compile. Compare selected formal Han tick offsets in a focused Unity complete-tick fixture, verify snapshot capture→owner release→restore and checksum, and confirm frozen-frame copy. Run `Tools/Validate-ChangeLedger.ps1`, `git diff --check`, and original Menu/Battle Scene SHA check. Do not run every character/case; only shared-writer neighbors if a shared writer changed. Unity project background pixels and formal-root same-frame pixels remain P-13 pending even if this package passes.

Rollback: revert only this declared code/test package via a reviewed diff and explicit approval for any protected deletion/revert operation; preserve all pre-existing dirty work. Shutdown/reset must clear this state through existing `BattleRuntimeState.Reset`, with no new manager, queue, worker, object or lifecycle stage.
