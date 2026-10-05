# Q07 behavior3 four-tick integer precision

Change: NTSD28-336B44-Q07-HITFA3-INTEGER-PRECISION-001.
Status: RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS. Parent Q07/Q09/Q12 and the overall goal remain open.

## Proven difference and repair

Current formal 336B44 paired native_ai.cpp behavior3 uses double increments +/-0.7 and +/-0.17. Unity RunHitFa3FrameLogic used float literals promoted to double. Formal indexed OID206/type3, cla2.dat frame54 has wait3/next999/hit_Fa3. Starting with counter0, the full driver consumes that frame four times, integrating before the fourth frame transition to action0.

With subject source X500/Y-100/Z600/V0/HP500, valid target99/slot0 at source X1000/Y0/Z604, the native complete Core driver reached X507/integer507/Vx2.8 on tick4. Unity before repair reached X506.9999998807907/integer506/Vx2.799999952316284. Both produced frames54/54/54/0 and counters1/2/3/0. The first precise position difference was tick1; the first source integer difference was tick4. See red-first-difference.json and both original JSONL files.

Production changed only four increment literals and one contract comment in RunHitFa3FrameLogic. The target/HP/no-target branches, source coordinate reader, depth deadzone10, velocity clamps16/2.4, frame order, integration and common spatial projection remain unchanged. Function-exterior bytes are identical to the pre-package dirty backup. No DAT edits or entity-specific exception were introduced.

## Actual validation

- Generated RED build: dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly; exit0, 301 warnings, 0 errors, 12.57s.
- Original Editor RED job8cdfad1d55d247afb27a418bc262ae3a terminated failed with exactly3 selected cases completed. Two depth cases observed +/-0.17000000178813934 instead of native double +/-0.17; the four-tick case observed integer506 instead of507. An earlier observation timed out; the same job was re-polled, not restarted.
- Current source-model native v2: complete SimulationTickDriver28::step four times, actual behavior3 applicable/special-tail guards and unsupported hitFa0 each tick; compile and run exit0, no compiler diagnostics. C++17/O2/no fast-math; exact command, compiler and69 declared input hashes in source-input-manifest-v2.json. Its28 Core source files match the current observed playable build's Core declaration set.
- Generated GREEN build: same command; exit0, 334 warnings, 0 errors, 13.72s.
- Original Editor GREEN jobd4ddaf3ac2dd47a8826f24773d842c6e: 4/4 passed,0 failed,0 skipped,43.9026606s. Exactly the same three cases plus ExistingLiveHitFaRepresentativeRoutingRemainsValid, which invokes the shared BattleRuntimeSelfCheck route check. No full test suite or role matrix was run. The job progress's total8887 is the discovered suite inventory, not the executed case count.
- GREEN subject trace: five rows including initial state,13 declared fields each,65/65 strict values equal to native. Final frame0/counter0/source integer507/Vx2.8/Y-100/Vy0/Vz0/target0/HP500. View X follows source X times2048/1333; maximum recorded residual1.1368683772161603e-13 pixels. See green-declared-field-comparison.json.
- The normal complete-driver callback returned through the existing replay scope's objects/claimed-slots/logic-borrowers zero assertions. RED callback exceptions do not establish the scope's post-callback zero-count assertion.
- Original MCP requested one refresh per RED/GREEN batch and checked corresponding source/DLL timestamps, idle nonPlay Editor and clean original Battle Scene before launch. Final Editor is idle/nonPlay, clean Battle/root11, Console0 errors. No scene save/switch, second Editor or computer-use.

## Evidence limits

The native runner is a diagnostic linked to current formal Core sources, not the formal root EXE or GameSession host. Native default options explicitly use next999 return0, no stage bounds/Kind/Fusion, drop mode2 and no selected combo. Unity reuses the old scenario schema/seed/diagnostic Stage23 and partial target initial state while reading current LoganRuntime and project mode Asset. Mode, stage, seed, other entities and complete world initial state are not identical. The65 comparisons cover only the declared subject fields.

Formal Deidara58/frame262 ->206/action50 ->51 ->52 ->53 ->54 is a static reachable resource chain. The direct controlled frame54/Vy0 witness does not validate natural birth dvy-2, frame51's706 child, a natural three-object chain, physical keys, actual Scene Play, formal root EXE or GPU presentation. Positive X accumulation and +/-Z single-call increments are covered; negative X four-tick accumulation is not separately measured. No further matrix is scheduled absent a relevant change or reproducible non-exception difference.

The native output rejects unsupported_object_hit_fa; it does not output or reject every other Core diagnostic category. No claim that all Core diagnostics are empty is made.

## Preserved failures and boundaries

The first manifest preparation omitted decoded_dat and failed before launching the compiler. Exact paths were taken from the before-manifest; native-preparation-path-correction.json retains that correction. The first native compiler run used a nonexistent EntityState28.hp member; native-build-result.json preserves the failure. The CPP was backed up and only that diagnostic field was corrected to current_hp for v2; both manifests and old source remain. These are diagnostic errors, not production gameplay differences.

The pre-package registration separator failures and premature readiness statement are retained and corrected in the Operation Record. All required registrations existed before scripts were edited. The Operation Index's new row column order was corrected to type/status before execution.

Pre-existing dirty work is backed up and retained. Four protected files, both sides of the three declared DAT inputs, formal EXE336B44, native_ai/driver/physics and observed build script were unchanged at the scope check. The69 native input hashes were unchanged at that observation; this is not a statement about the entire external source tree. Final governance checks are in GOVERNANCE-CHECK.md and final-scope-check.json.

This bounded repair returns to evidence reuse. Current scoped record inventory is225, with64 open: REUSE50/TRIGGER14/P0=DEP=ONE=0. These64 records are not64 mandatory new tests. Natural/host/GPU gaps stay conditional. Existing resource deletions, WORDS skip, full-background camera, project map/mode,1.5 sprite scale,33ms/F5 cadence, pass order and eleven-stage shutdown remain protected.

Final audit correction 2026-10-05T14:48:25.688224+00:00: use final-scope-check-v2.json for route counts. The initial final-scope-check.json passed all protected/source/DAT checks but its3-column-only route parser missed two valid6-column rows; v2 confirms all64 routes REUSE50/TRIGGER14. Original failure and parser correction preserved, no gameplay test rerun. Operation Index is now VERIFIED; production Record remains RUNTIME_PENDING.
