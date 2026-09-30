# NTSD28-Q09-REVIVE-LIVES-CAMERA-LEFT-001

Status: `FOCUSED_TEST_PASS / NATURAL_GPU_PENDING`. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-05/Q09/P-09. Q07/D-024 and Q09 aggregate remain open.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; paired playable `source/ntsd28_core/src/rendering/render_snapshot.cpp`, `RenderSnapshotBuilder28::build`, lines 1835–1838: mode gate not 3/4, revive lives greater than one, and `camera.x <= entity.position.x + 14`. The source is in the playable render snapshot closure.

Current Unity: `BattleEntityOverlayLayout.TryBuild` applies the mode/lives conditions but omits the camera-left condition. Both central command and Legacy glyph paths call the shared layout. The central runtime slot already captures `XInt`, `CameraX` and `VisibleLeftPixel`; the latter is calculated from the active fixed full-background world camera. A fixed-view presentation comparison must use `XInt - CameraX + 14 >= VisibleLeftPixel`. The 14 is the formal glyph visibility margin in display pixels, independent of the user's approved movement-distance scaling.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs`, one focused existing Editor test in `Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs`, and the single conflicting old left-edge expectation in `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`. The old self-check requires an offscreen X=-100 counter to remain visible; formal camera-left gate suppresses that counter while the Com label still clamps to X0. This amendment precedes editing the self-check. No DAT, PNG, Scene, Prefab, camera, gameplay position, collision, ordinary HUD, audio, or nonbattle change.

Acceptance: when lives=2 and mode gate=0, a body at exactly left-14 emits its two counter glyphs and a body one pixel farther left emits no counter. Existing label behavior and mode 3/4 suppression remain intact. Use the existing original Editor for a narrow test if available, generated-project compile, ledger validator and diff check. Record actual evidence before raising status; natural Battle GPU and formal same-viewport A/B remain open.

Focused test chronology: exact edge 1/1 and adjacent mode/viewport 2/2 passed after the first edit; then the inherited `OverlayLayoutSelfCheck_UsesFormalInclusiveRightEdge` failed on its old offscreen-counter expectation. Correct only that expectation and rerun the exact self-check before final acceptance. Until then the earlier pass is limited and the record's final status must reflect the rerun.

Risk: the shared layout serves more than one presentation backend. Change only the counter eligibility expression and add a boundary-focused test. Roll back only these exact hunks after reviewing dirty work; never reset the workspace.
