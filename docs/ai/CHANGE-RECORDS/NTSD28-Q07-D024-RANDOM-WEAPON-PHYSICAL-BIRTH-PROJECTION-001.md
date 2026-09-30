<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-RANDOM-WEAPON-PHYSICAL-BIRTH-PROJECTION-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Passes/RandomWeapon/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0F8Owner99ProductionEditorTests.cs
authority: user D-024 all-entity proportional distance adaptation and W-07 retained Unity random-weapon exception; formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033
evidence: docs/ai/TASKS/NTSD28-Q07-D024-RANDOM-WEAPON-PHYSICAL-BIRTH-PROJECTION-001.md; artifacts/diagnostics/NTSD28-Q07-D024-COLLISION-GEOMETRY-DOMAIN-AUDIT-001/MAP-ORIGIN-AND-BIRTH-DOMAIN-CONTRACT-20260929.md
-->

# NTSD28-Q07-D024-RANDOM-WEAPON-PHYSICAL-BIRTH-PROJECTION-001

Status: `FOCUSED_TEST_PASS / NATURAL_PLAY_PENDING`.

Before: both parent-null random-weapon task writers select a project-stage physical point but copy it into source rule X/Z. Mode-2 factory then adds the same raw +1 Z in both domains, contrary to the user's later ratio choice at configured 2048×1152. The earlier W-07 carrier-birth record remains valid as evidence for carrier existence and identity-scale behavior only; it does not establish proportional placement.

Planned: use the one `world.SpatialProjection` inverse for source birth in normal and mode-2 routes. For mode-2, map the source +1 Z through its common depth-distance API and skip the factory's raw post-init increment. Keep all chance/selection/slot/owner/RNG and project-stage sampling unchanged. The existing deterministic test class gets configured-view cases. Only the two exact code paths in the header may be edited; no DAT, map, Scene, camera, mode asset or nonbattle changes.

Expected side effects: normal-drop physical position identical but source carrier changes under enlarged view; mode-2 final physical Z changes from +1 to +rZ and source from +1 relative to inverse physical base. Identity-scale results remain exact. Rollback and narrow acceptance are specified in the Task. Actual code and verification results will be appended after the edit.

Test-first script edit (2026-09-29): only `NTSD28B0F8Owner99ProductionEditorTests.cs` has changed so far. The normal and lowest-slot mode-2 cases now each have identity/configured-view variants; both expect the common inverse source birth, and mode-2 expects `SourceDeltaToViewZ(1)` actual post-init offset with source +1. Existing occupied-prefix identity control remains. Generated/Editor compile and RED run are pending; production writer has not been edited yet.

RED validation: generated `Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly` compiled with 0 errors (214 existing warnings). Original Editor PID11944/6401 forced scripts refresh copied a new `Assembly-CSharp-Editor.dll` after the test source. First single-class MCP job `5f74b12a41a144c69f68746aa3067c59` timed out at initialization during reload; its later runner execution had no attributable count and is excluded. Once idle, exact single-class job `16e507aef0cb46ecaf63360a77d93d1a` executed five cases: three identity/occupied-prefix cases passed, two configured-view cases failed as expected. Mode-2 expected final physical Z `281.57808219178082`, got `281.0`; normal source X expected `78.75634765625`, got `121.0`. No production edit yet. This is the relevant RED, not a Unity compile or transport failure.

Production edit (2026-09-29): both random-weapon task writers now call `world.SpatialProjection.ViewToSourceX/Z` for the sampled physical point. Mode-2 computes final physical Z as sampled physical Z plus `SourceDeltaToViewZ(1)`, stores source Z as inverse sampled Z plus one source unit, and sets `skipPostInitZOffset=true` with direct runtime position so the factory does not add a second raw +1. Normal physical coordinates and all chance/candidate/RNG/owner/slot branches are untouched. Only the two declared scripts changed. Generated/Editor compile and GREEN are pending; no DAT, map, Scene, camera, mode or nonbattle edit.

GREEN validation (2026-09-29): generated `Assembly-CSharp-Editor.csproj --no-restore -clp:ErrorsOnly` completed with 0 errors (245 warnings). After the original Editor PID11944/port6401 script reload, first exact-class job `7303425604ef4d1faeb52436a3ab1f81` timed out during initialization and is excluded. With the same Editor idle, exact-class job `69e32163d83b464baf20cb3ab7168986` succeeded: 5/5 cases passed, including normal and mode-2 identity/configured-view variants and occupied-prefix control. `Tools/Validate-ChangeLedger.ps1` passed. Battle/Menu Scenes, SunagakureMap, GameConfig and ProjectBattleModeConfig SHA-256 values stayed at their pre-edit baseline. This proves focused birth conversion and branch preservation; direct natural Play and formal nonempty-candidate route are still pending. Q07 and the overall goal remain open.
