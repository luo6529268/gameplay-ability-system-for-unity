# NTSD28-Q09-P02-MOTION-HISTORY-CARRIER-001

Status: `PLANNED`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-02`. This is a prerequisite carrier, not the interpolation consumer or a Q09 exit.

Authority: formal Logan `source/ntsd28_core/src/rendering/render_snapshot.cpp` captures slot/OID/generation, precise XYZ, motion XYZ and six relations for occupied entities before sprite visibility gates; `source/ntsd28_playable/src/presentation_interpolation.cpp` consumes adjacent snapshots by value. Formal `main.cpp` samples only above 30 FPS. The approved Unity fixed camera and D-024 movement ratio remain unchanged.

Current Unity: `BattlePresentationCoordinator` publishes into alternating `frameA/frameB`; the old frame is reused on the next tick. `BattlePresentationEntitySnapshot` lacks precise movement and five of the six relation values. `BattlePresentationFrame.CopyFrom` freezes value arrays but catalog lifetime is owned by the submission. A retained old frame reference is unsafe.

Owned script paths and symbols:

- `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`: add a pure-value motion row, current/previous arrays and ticks to `BattlePresentationFrame`; capture registered active rows before body visibility filtering; copy the preceding published frame's current rows only for adjacent ticks; freeze both arrays through `CopyFrom`; clear counts/ticks in `Reset`.
- `Assets/NTSD/Scripts/Test/Editor/BattlePresentationBeginFrameReuseEditorTests.cs`: focused tests for adjacent ticks, A/B reuse, frozen copy, hidden body row, skip tick, reset, and representative source/view coordinates and relations.

Expected effect: only presentation publication carries extra immutable numeric data. No renderer, sampling, sorting, entity state, logic tick, checksum, DAT, Scene, ProjectSettings, input, audio, menu or nonbattle behavior changes. No new manager, worker, resource lease or shutdown phase. Existing coordinator reset is the cleanup owner.

Risks and constraints: `GetPresentationEntitiesNoAlloc` is current-pass registered traversal, not a guaranteed copy of every formal World slot; report any missing-row case as a first difference. D-024 source X/Z and Unity view X/Z coexist; capture both without applying scale. Six Unity relation fields are stored raw, not yet asserted semantically equivalent to the formal fields. Motion carrier is not an interpolation claim. A/B source must be read before overwrite; skipped ticks/world reset must never interpolate later.

Acceptance: generated runtime and Editor C# compilation with zero errors; focused Editor tests must prove same published tick N+1 history survives publications N+2/N+3 and `CopyFrom`, hidden body row persists, skipped tick and reset clear history; existing presentation reuse and purity neighbors should pass if affected. `Tools/Validate-ChangeLedger.ps1` and `git diff --check` pass. Formal source interpolation tests already passed in the preceding Q09 audit and are not rerun unless a later sampler change requires them. No Play/30-60-120 visual claim from this package.

Rollback: remove only this Change ID's reviewed carrier/test hunks, after checking working-tree ownership; do not restore/reset, delete or overwrite unrelated dirty files. If validation cannot reach original Editor, retain `RUNTIME_PENDING` and describe exact compile/test evidence.
