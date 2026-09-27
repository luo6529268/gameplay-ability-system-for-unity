# NTSD28-Q09-NAMEPLATE-PLATFORM-HEIGHT-001

Status: `FOCUSED_TEST_PASS / NATURAL_GPU_PENDING`. Parent `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-05/Q09/P-10. Q07/Q08 and Q09 remain open.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and paired playable `source/ntsd28_core/src/rendering/render_snapshot.cpp` nameplate projection: `position.z + render_shadow_offset_10c + 3`. The same source's platform operation30 writer and existing `NTSD28-Q09-FORMAL-PLATFORM-SHADOW-CAMERA-WITNESS-001` original Battle Scene full tick6 demonstrate formal OID56/type0 platform target `RenderShadowOffset10C=-5`. Formal `render_snapshot_tests.cpp` also checks zero-offset Z300→303.

Pre-change Unity: `BattlePresentationEntitySnapshot` already captures immutable `RenderShadowOffset10C` and the central/Legacy shadow consumers use it. `BattleEntityOverlayRuntimeSlot` omits this field; `ResolveLabelOrigin` returns `ZInt+3`; `BattlePresentationShadowBuild.BuildCommands` creates the slot without the captured offset. At target Z287/offset-5, the expected nameplate top is285 but the present layout yields290.

Declared paths: `Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs` adds the captured offset to the existing allocation-free runtime slot and applies it only to label Y; `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs` passes the already captured snapshot value to that slot; `Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs` adds a focused command-level positive/zero-offset test and updates its reference helper. Existing dirty hunks in these paths are protected. No DAT/PNG/Scene/Prefab/camera/logic World/nonbattle UI/audio edits.

Acceptance: original Editor RED for platform offset -5, GREEN for the same exact case, zero-offset and existing optimized-writer neighbor; compiled 0 C# errors, command X/sheet/order unchanged, counter Y unaffected, no logic/checksum change. Verify original Editor non-Play and Scene SHA plus Change Ledger/diff. Natural composed Game view/Legacy/EXE same-view remain P-10 aggregate gates; do not claim Q09 completed.

Risk: the shared layout services Central and Legacy; the offset must affect only nameplate origin, not lives counter, shadow, body or logic. Rollback only this ID's exact hunks after reviewing dirty work; no restore/reset/clean.

Original Editor executable RED and scoped GREEN are recorded in `artifacts/diagnostics/NTSD28-Q09-NAMEPLATE-PLATFORM-HEIGHT-001/ACCEPTANCE.md`; mode/visibility and natural same-view pixels remain separate P-10 exits.
