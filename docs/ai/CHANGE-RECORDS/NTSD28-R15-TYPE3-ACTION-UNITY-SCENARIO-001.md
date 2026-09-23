<!-- CHANGE-RECORD
id: NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001
status: FOCUSED_TEST_PASS
change-kind: TRACE_DIAGNOSTIC_UNITY_TYPE3_ACTION_SCENARIO
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable GameSession28 scenario roster/BattleWorld28::spawn_at
evidence: formal type3/action scenario is currently projected as LF2Character/action0 by Unity raw capture
-->

# NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001

Before: the Unity diagnostic scenario DTO has no action field, always constructs `LF2Character` and expects all participants in the exact character frame-tick pass. Formal `spawn_at` accepts type3 combatants and initializes action/action_latch/previous_action/tick_action_snapshot to the authored initial action. Thus the current Unity exporter cannot witness the existing kind-dependent source-model scenario faithfully.

Intended after: diagnostic-only type3/action scene creation with exact catalog type, authored action, physical slot and spawn-at scalar state. Existing type0/default captures remain valid; unsupported catalog type or undefined action fails before output. Gameplay logic, formal source, Scenes, resources and nonbattle code stay outside this Change.

Expected side effects: the new diagnostic trace may expose a real Unity/source-model first difference. That difference must be recorded, not hidden by changing source input or production behavior in this package. The type3 roster entry remains physical-slot based; the exact character-pass count applies only to type0.

Acceptance, boundaries and rollback are in the matching Task. Actual changed symbols, focused tests, trace outcomes and remaining gaps will be appended after implementation. No script has been changed for this Change at record creation.

Actual diagnostic implementation: `UnityRawCombatant.action` defaults to zero for old JSON. `ConfigureWorldAndRoster` reads the selected catalog type, retains the type0 `LF2Character` path, creates a true `LF2SpecialAttack` for type3, verifies its authored action, sets action/latch/previous/snapshot frame carriers, vitals, owner/team/facing/position and required physical slot, and checks its registered World. The exact character-pass count is based on type0 participants. For a type3 capture only, `UnityCurrentDatScope` captures and temporarily publishes the same Logan visual candidate identity to the animation manager, and the diagnostic driver seals battle preparation before selecting logic-only entity materialization. The scope restores the prior manager publication. No production gameplay script changed.

Validation: original Unity Editor PID173216 compiled the two diagnostic files. The first focused run was 1/2: missing-action rejection passed, while the real action176 tick reached `SpawnHitFa7Clone` and hit a null renderer pool. A second 1/2 run showed that setting logic-only before first-tick battle preparation was overwritten. Explicit preparation first exposed the temporary publisher identity mismatch; the scoped same-candidate publication resolved it. Final original-Editor EditMode job `a4c04f68e56949c1b72fc16b0ea5a5aa` passed 3/3: type3/action three ticks, undefined action856 rejection before output, and existing type0 common capture. No all-case sweep was run. Main formal-source runner rebuilt from the current paired playable/core source with EXE SHA and source manifest checks; formal and Unity V3/B0v2/B2 three-tick outputs all validated strictly. `dotnet build Tools/NTSD28Parity --no-restore -v:q` passed 0 warnings/0 errors.

First difference: tick1 formal source-model has slot1 OID213/action40 only. Unity has the same slot1 plus a newly born slot50 OID206/action40. B0 agrees on slot0 death and slot1 same-epoch OID206→213, but Unity adds slot50 birth. The main comparator returns `invalid-capture/entity-slot-set-mismatch`, B2 first difference is `ticks[1].entities.count` (1 versus 2), and B0 reports slots/lifecycle difference along with the known native-vs-Unity RNG stream topology. This is a real same-input source-model/Unity runtime difference, not a passing parity result. Formal source `native_ai.cpp` behavior7 path has target-follow motion, while Unity `LF2Entity.RunHitFa7FrameLogic` calls `SpawnHitFa7Clone` before that motion. A separate production Change must trace and fix the whole behavior7 pass, not alter this diagnostic capture to hide the extra birth. Formal EXE visible behavior remains unmeasured.

Evidence: `Temp/diagnostics/NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001/` build manifest, six captures, five validators and three comparator reports; artifact `artifacts/diagnostics/NTSD28-R15-TYPE3-ACTION-UNITY-SCENARIO-001/REPORT.md`. Saved Menu/Battle Scene SHA stayed 6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1 and 9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0. Q07/R15 and total goal remain open.
