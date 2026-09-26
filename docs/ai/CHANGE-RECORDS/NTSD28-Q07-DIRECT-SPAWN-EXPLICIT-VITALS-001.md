<!-- CHANGE-RECORD
id: NTSD28-Q07-DIRECT-SPAWN-EXPLICIT-VITALS-001
status: RUNTIME_PENDING
change-kind: Q07_GENERIC_EXPLICIT_DIRECT_SPAWN_VITALS
code-path: Assets/NTSD/Scripts/Animation/LF2Tasks/OPointCreateTask.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleSpawnVitalsWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HitFa5FullDriverEditorTests.cs
authority: formal root NTSD2.8-Logan.exe paired playable NativeAi28 hit_Fa5 direct spawn and indexed w/e.dat frame0
evidence: NTSD28-Q07-HITFA5-CHILD-VITALS-FIRST-DIFF-001 original Editor HP497 versus formal spawn HP0
-->

# NTSD28-Q07-DIRECT-SPAWN-EXPLICIT-VITALS-001

Pre-change: hit_Fa5 direct child spawn is encoded as an OPoint task without a way to represent explicitly zero initial HP/MP. Both Unity logic-only and renderer-backed materializers call the generic writer, which interprets absent/nonpositive DAT OPoint values as default 500. The original Editor full Driver observes HP497 after frame0 drain versus formal direct-spawn HP0. Existing default OPoint vitals must be preserved.

Plan: add an opt-in presence/value carrier to pooled task, set it only from source-confirmed direct-spawn semantics, and pass it through both materializers to one vitals writer. No OID test in writer. Preserve pool reset and current source-rule movement, AI snapshot and pass order. Acceptance, risks, exact files and rollback are in Task Contract. Script edit not yet begun.

2026-09-26 intermediate implementation: five production scripts carry explicit initial HP/MP with a presence bit; `RunHitFa5FrameLogic` writes zeroes. Original Editor recompilation first failed on an accidental `OPointCreateMultipleTask` field copy, corrected without broadening the task. Second compilation compiled runtime but failed existing reflection tests because `PostInitLiving` and `BattleSpawnVitalsWriter.Apply` signatures changed; corrected by preserving their six/two-argument APIs and forwarding new task-aware internal methods. Final Editor runtime and Editor assemblies are newer than scripts, and Q06 spawn-vitals 12/12 passes. Complete-Driver group1 now passes newborn HP0/action1 but stops at later source X formal103/Unity105 on completed tick1; group3 eight-tick control passes 1/1. Add one focused renderer-backed explicit-zero and task-reset test before claiming this vital carrier scope complete. No DAT/Scene/UI or nonbattle edit.

2026-09-26 scoped validation: renderer-backed post-init explicit-zero forwarding and pooled task `Clear()` reset passed 1/1 (`33c2452d6368490bb513295640d9d2fa`). Existing Q06 spawn-vitals full class passed 12/12 (`49e265c4ff78452a8be51926ba04d9b8`), including default OPoint percentage, two materializers, actual definitions and warm zero-allocation checks. Q07 no-birth eight-tick control passed 1/1 (`e2256acc4b5f4d6e9a54961eb13ac79d`). Positive full Driver (`205c82adbe3f4171a3724d228124ce25`) reached its later tick1 source-rule X difference formal103/Unity105; preceding HP0, action1, and zero-postcommit-breach checks passed. The earlier positive run `64f4ffce060d4c2ea24696a78138528e` used stale runtime assemblies because C# compilation had failed; it is not a post-fix result. Both compile failures and the initial Q06 reflection failure `a48bcc60e83e4fafa19ef84714a1115d` remain recorded. Final original Editor assemblies are newer than scripts, Scene/config hashes unchanged, `git diff --check` and ledger validator exit0. The direct-spawn vital carrier is scoped-tested but this Change is `RUNTIME_PENDING` because positive eight-tick parity and natural Battle Play are not complete. Next first difference belongs to a separate source-rule X diagnosis, not this vitals repair.
