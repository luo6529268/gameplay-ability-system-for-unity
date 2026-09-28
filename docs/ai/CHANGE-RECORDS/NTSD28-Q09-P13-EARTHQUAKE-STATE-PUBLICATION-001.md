<!-- CHANGE-RECORD
id: NTSD28-Q09-P13-EARTHQUAKE-STATE-PUBLICATION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09EarthquakeStatePublicationEditorTests.cs
authority: formal root NTSD2.8-Logan EXE plus paired playable GameSession28 NativeEarthquake28 and current Han DAT
evidence: docs/ai/TASKS/NTSD28-Q09-P13-EARTHQUAKE-STATE-PUBLICATION-001.md
-->

# NTSD28-Q09-P13-EARTHQUAKE-STATE-PUBLICATION-001

`RUNTIME_PENDING` after the scoped producer implementation. Unity originally had only the `SessionEarthquake` pass descriptor, no producer or frame scalar. `BattlePresentation.BeginFrame` can be skipped for a successful tick, and snapshot restore resets the presentation coordinator; retaining offsets solely in that coordinator would break the formal owner-release rule. The exact Task defines eight self-authored script paths, authority order, field semantics, data/schema/worker/reset dependencies, protected paths, test exits and rollback. Only the battle-scoped producer and frame publication are included here, not project-background drawing or pixel acceptance.

Implementation: `NTSD28EarthquakeRuntimeState` now owns the slot and offsets. The successful tick tail advances it after result/KO work and before render publication even with `buildPresentation=false`. Core and aggregate snapshot schemas, restore validation, checksum schema, and `BattlePresentationFrame` frozen copy/reset carry this state. The new Editor test covers ascending slot priority, owner release without offset clearing, state reset, snapshot/restore/checksum, frozen copy, and a successful no-presentation tick. No DAT value, original background/mode DAT, camera, entity coordinate, Scene, Prefab, ProjectSettings, or nonbattle script changed in this package.

Validation: generated `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` builds both completed with zero errors (warnings remain); original project Editor refreshed and ran the exact three `NTSD28Q09EarthquakeStatePublicationEditorTests` EditMode tests, job `9a34378e4636499b8b03680a97bc997b`, `Passed 3/3`, failed 0, skipped 0. Initial two-test job `792b04746f3b47f8b537a93fede15018` also passed 2/2; its later post-reload requery lost detailed results and is not used as the final certificate. `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` rerun exited 0 with `Change ledger validation PASSED`, 938 records and 10 changed code files covered; `git diff --check` exited 0. Original Menu/Battle Scene disk SHA remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`.

Remaining: the published scalar has no project-background draw consumer yet. Natural Battle Play pixels, formal-root same-frame pixels, and the complete P-13/Q09 exits remain unverified. Worker publication receives the scalar through the shared frame capture/copy path, but no separate live worker case was run in this package. See `artifacts/diagnostics/NTSD28-Q09-P13-EARTHQUAKE-STATE-PUBLICATION-001/ACCEPTANCE-PENDING.md`.

2026-09-27 correction: the project Map draw consumer has since been implemented and passed its isolated GPU test. A later original Battle Scene probe confirmed the dedicated worker remains ineligible because roster Unity presentation bindings are attached, matching the earlier B1 audit; it is not a current Q09 production-path gate. Current P-13 evidence still requires natural inline Battle Play pixels after the Q07/D-024 catch-domain decision and a formal-root visual comparison. See `NTSD28-Q09-P13-WORKER-BACKGROUND-PLAY-001/ACCEPTANCE.md`.
