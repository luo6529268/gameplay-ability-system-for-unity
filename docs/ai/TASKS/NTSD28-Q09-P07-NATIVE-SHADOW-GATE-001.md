# NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001

Status: `RUNTIME_PENDING / FOCUSED_TEST_PASS` (2026-09-28). Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-07`. Q07/D-024 and Q08 retain their separate exits.

Authority: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `render_snapshot.cpp` shadow gate (`definition->bmp.integer("shadow")` and `frame->values.integer("shadow")`, each suppressing at value 1). Formal indexed OID518 `c/nar/a/atk.dat` has BMP `shadow: 1`, visible frame 1, and is referenced by Naruto OPoints. Current non-excluded DAT inventory has no `shadow_pic`; `shadowsize` occurs only in 24 excluded original background DATs. This task does not implement an unreachable custom shadow picture/size path.

Unity before change: `LF2Entity.UpdateShadow` and `UpdateShadowManagedState` apply state/link/OID/hit-stop gates but not definition/frame `shadow`; `BattlePresentationShadowBuild` captures sprite shadow visibility and has no independent DAT suppression for logic-only entities. BMP fields are already preserved in `LF2CharacterData.NativeMetadata.Bmp`, and formal frame properties remain in `LF2FrameData.rawProperties`.

Exact script paths: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`, `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`, and `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NativeShadowGateEditorTests.cs` (plus its Unity `.meta` after import). No DAT/parser data edits, Scene/Prefab/ProjectSettings, background/mode/HUD, or nonbattle changes.

Implement one shared read of BMP and frame `shadow == 1` for all entities, not an OID518 special case. Apply it to Legacy shadow visibility and central snapshot capture. Preserve all existing mode/state/object/hit-stop gates and body display. Do not mutate frame/definition data. The `shadow_pic`/`shadowsize` fields remain conditional future work only if relevant approved content actually uses them.

Validation: focused RED before implementation if the original Editor can run it; GREEN after implementation, generated project compile, original Editor compile and focused tests, and a targeted real Battle Play example if safe. Record the actual layer reached. Run `Tools/Validate-ChangeLedger.ps1` and `git diff --check`; verify original Menu/Battle Scene hashes. Do not run all characters or all test scenes for this shared gate.

Rollback: review the exact scoped diff and revert only this package with required approval; preserve all other dirty work. No new runtime owner, worker, pool or shutdown stage is introduced.
