<!-- CHANGE-RECORD
id: NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001
status: VERIFIED
change-kind: BATTLE_EDITOR_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KindDatLiveSceneTickProbeEditor.cs
authority: formal playable type3 OID213 to OID206 kind catalog fixture and current selected formal kind.dat
evidence: focused B5/ECS tests pass but kind-dependent live Battle Scene full tick is missing
-->

# NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001

Before: the selected kind catalog and production combat consumers are implemented; only isolated focused tests prove their behavior. No live Battle Scene full-tick witness uses a kind-dependent type-3 pair.

After: one Editor-only request-file probe drives the existing Battle Scene World through a complete tick with temporary type-3 entities. It verifies the selected kind record, runtime identity transfer and cleanup; it does not alter production logic or saved assets.

Invariants: original Editor only; no computer-use or second Unity project; no Scene save or user-dirty overwrite; no broadened all-case run; source fixture and selected DAT are checked before any behavior claim. A failure is retained as evidence and must not be called a pass.

Validation: original Editor compile, one target Play probe, result/report inspection, Scene SHA/dirty check and `Tools/Validate-ChangeLedger.ps1`. Runtime and formal EXE equivalence remain separate.

Rollback: delete only the new diagnostic script/meta after the explicit deletion approval required by AGENTS.md, after reviewing current dirty state.

Written: added only the declared Editor probe script. It reads the live prepared kind catalog, creates temporary LF2Character shells carrying type-3 simulation identity and the formal OID213/OID206 frame/itr/bdy geometry, advances one full production driver tick, records transformed identity/action/latch/owner, unregisters the temporary entities, and exits Play. No production script or Scene was changed. Original Editor PID173216 is live at local bridge port6404 and currently idle on the saved Battle Scene; the new script is not yet confirmed compiled or run.

Compile: original Editor PID173216 imported the new script and generated its meta GUID ecc23af7606e10f42b1b2e3b85fcebee. Editor.log reports Tundra build success (1662 evaluated), Assembly-CSharp-Editor.dll updated, and successful domain reload. No error CS line appeared in this build tail; existing unrelated compiler warnings remain. Play result is pending.

Focused Play: `result.json` PASS from the original Editor's saved Battle Scene. Live selected catalog had one record, effect209/frame40 bound213/respond206; full production driver tick5→6 transferred temporary target OID206 to OID213/type3 with frame N/Prev40, team4, owner7, special-hit latch and attacker definition. Test registration used slots50/51. World object count 4→4 and pool borrowers 2→2 after unregister. Request ended `done`, Editor state returned idle EditMode; Menu and Battle Scene SHA-256 stayed 6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1 / 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0. This verifies only the declared controlled Scene tick. It does not prove natural roster reachability, Player cold start, formal EXE visual equivalence, or full Q07.

Final checks: `& ./Tools/Validate-ChangeLedger.ps1` PASSED (701 records, 61 governed code files in current diff). Scoped `git diff --check` passed. Formal EXE SHA-256 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 was rechecked, and formal/staged selected `kind.dat` both hash 39E30DF8D86A5FC374B26C80BE358A6503B2C3096D8681C586478D73A0900011. No full suite or second Editor was run.
