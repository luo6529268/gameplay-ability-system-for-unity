<!-- CHANGE-RECORD
id: NTSD28-Q08-ARMOR-LETHAL-FULL-TICK-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q08Diagnostics/selected_armor_lethal_lfr_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: root Logan EXE and paired playable GameSessionLfr28 selected-armor KO branch; BATCH-04 Q08
evidence: docs/ai/TASKS/NTSD28-Q08-ARMOR-LETHAL-FULL-TICK-001.md
-->

# NTSD28-Q08-ARMOR-LETHAL-FULL-TICK-001

Before edit: shipped OID87 selected type-1 armor hit route was observed at HP500 (two reduced HP3 hits at tick16), while the Q08 focused synthetic production entry HP10/11 passed2/2 without a root-EXE lethal same-state witness. Existing Q07 C++ LFR producer and Unity raw exporter enforce fixed HP500; mutating old files or evidence would destroy provenance. Formal branch records KO using reduced effective damage before HP subtraction when initial HP is positive and at most damage, with valid credit/gate.

Declared paths and roles: new `Tools/NTSD28Q08Diagnostics/selected_armor_lethal_lfr_probe.cpp` generates HP3 source-authored LFR and narrow CSV; existing `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs` receives a separate strict HP3 schema/KO sidecar gate only after native root replay proves this candidate. Side effects limited to diagnostic output and Editor recompilation. No production, DAT, Scene, ProjectSettings, mode Asset, source-authority or nonbattle change. Exact acceptance, risks and rollback in the Task. Validation pending.

After first script edit: the new C++ producer copies the existing proven OID11→OID87 26-tick input/seed/Stage/position LFR sequence, changes only target initial HP500→3, fixes X550 and uses new output names. Its CSV adds attacker KO count to target HP/action/MP/armor-HP rows. The old Q07 producer and all prior traces remain untouched. Native compile and root-EXE playback pending; Unity exporter remains unedited until this candidate proves selected-armor lethal reachability.

Formal viability: paired-source diagnostic compiled with 28 core sources and the playable session/LFR sources. Both resource-root arguments pointed to the formal `resources/runtime`; the producer emitted 26 ticks and the unchanged root EXE replay returned exit 0, `passed=true`, `completedTicks=27`. At completed tick 16, its first OID440 type-3 hit applies reduced HP damage 3 to slot 1, records one KO credited to slot 0 at battle time 15, and attacker KO count becomes 1. The root playback report explicitly sets `nativeParityClaim=false`; this replay is a source-authored same-state fixture and requires the Unity comparison below.

After second script edit: `NTSD28UnityRawCaptureEditor.cs` now admits only a new `ntsd28-q08-selected-armor-lethal/1.0` schema in its formal content route. The schema fixes both participants, target HP/baseHP 3, seed/ticks/stage/difficulty via existing formal constraints, and exactly six L,L,D,D,J,J input rows. KO sidecar permission is extended to this exact schema while preserving the Q07 Naruto exception. No production runtime, DAT, Scene, ProjectSettings, or mode Asset was changed by this edit. Original Editor compilation, complete Driver raw/KO capture, field comparison, ledger validation and protected hash recheck remain pending.

Validation and exit: original Editor source/assembly timestamps `2026-09-27T03:07:08Z` / `03:08:19Z`, idle/non-Play after refresh, checked console no C# compilation error. Exact request exported 26 completed ticks and KO sidecar with `PASS`. First root replay intentionally retained: it omitted source initial action 110, producing six early fixture differences. Root replay with `--lfr-slot0-action 110` returned exit 0/PASS. Corrected 26-tick comparison contains 68 entity rows × 25 common fields = 1,700 equal comparisons; first KO event appears at tick 16 with identical battle time 15, source type3/slot50, victim1, owner0, credit0. KO sidecar rows 17–26 retain the one event rather than append new credits. Exact evidence and scope limits: `artifacts/diagnostics/NTSD28-Q08-ARMOR-LETHAL-FULL-TICK-001/ACCEPTANCE.md`. Battle/Menu/ProjectBattleModeConfig SHA-256 stable. This is a scoped Q08 diagnostic exit; Q08/BATCH-04 and natural physical Play remain open. Ledger validator and final diff check are recorded after execution below.

Governance check: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <workspace>` exit 0 (its existing unrelated stale-declaration warnings remain in `change-ledger-validation.txt`); `git diff --check` exit 0 with only line-ending conversion notices. Original Editor returned idle/non-Play with no compilation in progress; protected Scene/Asset hashes rechecked unchanged.
