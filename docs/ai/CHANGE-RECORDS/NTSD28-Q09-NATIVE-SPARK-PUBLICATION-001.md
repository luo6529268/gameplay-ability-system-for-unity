<!-- CHANGE-RECORD
id: NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/SparkRenderer.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCommonAtlasBindingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedPublicationEditorTests.cs
authority: NTSD 2.8-Logan formal playable raw spark ID, 99x79 source rectangles, black-key presentation; BATCH-05 Q09
evidence: docs/ai/TASKS/NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001.md
-->

# NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001

Pre-change: formal PNG/dimensions are captured but production still publishes old BMP and collapses raw IDs through historical 20-picture mapping. Existing central atlas uses 20 common-spark keys and requires complete bindings, so the formal 20 in-bounds cells will be compacted only for resource lookup while the raw event age remains unchanged. Scope, risks, tests and rollback are in the Task Contract; user-owned files and existing old resource remain protected.

Post-change: configured formal-content prewarm now decodes the captured SHA-verified SPARK.png on a worker, validates the formal 500x320 PNG and 99x79 system geometry, applies exact RGB-black color key to staging pixels, creates the 20 in-bounds 99x79 sprites, and publishes them through the existing atomic common catalog/atlas/lease path. The legacy content branch keeps the previous SPARK.bmp publication. `BattleCommonVisualCatalog` retains a native-spark mode through WORDS/central remapping, maps raw ID 0..99 to a compact 20-key resource only when the native 10x10 rect is drawable and inside the PNG, and rejects terminal/out-of-bounds IDs without touching C01 age. Both `SparkRenderer` and central HitRecord command construction now use that same catalog lookup. Native odd-size pivot uses integer width/2 and height/2 top-left offsets, not a geometric half-pixel center. No DAT/image, Scene/Prefab, HUD, noncombat code, RNG, hit producer or lifecycle writer changed.

Validation: formal EXE SHA-256 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 reverified. Original Editor compiled with 0 errors after correcting a misplaced method (the initial 29 CS errors were not runtime evidence). Native raw-ID geometry focused job `83facf6049794962bc869be187f01708` passed 1/1; exact black-key and native geometry job `d288d47d9c9140588536f9b0ef8f0601` passed 2/2; whole adjacent common-atlas class job `c371eda3b0b849d0bc2a85313dbbc684` passed 6/6. The actual staged 330-object formal publication/recycle job `1b4c695b9f524365bbcc6c2ec9cec7c1` passed 1/1 before the integer-pivot correction and job `a1e1991531a2486285c5685203a90680` passed 1/1 after it, including native binding, central binding and zero survivors. Full `BattleRuntimeSelfCheck` returned fresh PASS again after the final pivot edit at 2026-09-24T11:13:05Z. Battle/Menu Scene SHA remained 9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39 / 3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228; original Editor exited Play to idle. Ledger validator passed, 786 Records and 25 current code diffs covered; `git -c core.safecrlf=false diff --check` passed. See the acceptance report.

Runtime first difference: original Battle Scene C01 natural-hit Play probe `Temp/NTSD28_B3_NativeSparkC01.result.json` was run once and returned FAIL at tick 3573 before any visual assertion: RNG call delta 1 versus its expected 3. Its cleanupCompleted=true, and the Scene SHA stayed unchanged. The full result is archived as `artifacts/diagnostics/NTSD28-Q09-NATIVE-SPARK-PUBLICATION-001/c01-natural-hit-play-first-difference.json`. This is not evidence that SPARK pixels failed or passed. The first difference needs current-authority Q06/hit-producer versus probe-fixture audit; do not suppress it by changing an expected count or by modifying C01 logic for presentation. A separate targeted Play render path and original-EXE pixel comparison, including 30/60/120 sampling, remain required before Q09/R14/R17 close. Status is `RUNTIME_PENDING`, not verified full parity.

2026-09-24 follow-up: `NTSD28-Q09-C01-PLAY-PROBE-RNG-001` corrected that obsolete test oracle against the already verified Q06 dual-RNG producer, without changing production logic. Original Battle Scene CentralOnly Play then passed four ticks with native CRT2/shared RNG1, C01 ages, materialized commands 1/2/3/0 and full cleanup. See its acceptance report and archived result SHA `147D7F58...F119161A74`. This closes the old probe first difference only; legacy renderer Play, 30/60/120 sampling and formal-EXE pixel parity still keep this parent `RUNTIME_PENDING`.
