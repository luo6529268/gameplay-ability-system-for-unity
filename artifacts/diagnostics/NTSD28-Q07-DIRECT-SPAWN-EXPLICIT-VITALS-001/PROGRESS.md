# Q07 direct-spawn explicit vitals: scoped production result

2026-09-26. Formal paired playable `NativeAi28::step_non_character_hit_fa` creates the hit_Fa5 child at HP0/MP0. Unity previously defaulted to HP500 and finished tick1 at HP497/action0. This package adds an opt-in explicit-vitals carrier on pooled `OPointCreateTask`; the source-confirmed direct-spawn producer sets both values to zero. Both logic-only and renderer-backed materializers pass the carrier through a shared writer. The ordinary two-argument `BattleSpawnVitalsWriter.Apply` and six-argument `PostInitLiving` remain available for existing callers and reflection tests. No OID branch was added to the writer.

Original Editor validation:

| Job | Result |
| --- | --- |
| `d98a731d057f453990db1f51c5d2502d` | Pre-fix positive RED: child HP497 versus formal0. |
| `205c82adbe3f4171a3724d228124ce25` | Post-fix positive passes zero-breach, child HP0 and action1; next first difference at completed tick1 source-rule X formal103/Unity105. Eight-tick positive parity **not** achieved. |
| `e2256acc4b5f4d6e9a54961eb13ac79d` | No-birth group3 control all eight source rows, 1/1 PASS. |
| `33c2452d6368490bb513295640d9d2fa` | Renderer-backed post-init explicit-zero HP/PP and pooled task reset, 1/1 PASS. |
| `49e265c4ff78452a8be51926ba04d9b8` | Existing Q06 spawn-vitals class including default OPoint, both paths and formal birth corpus, 12/12 PASS. |

Initial compile was blocked by three accidental references to fields absent from `OPointCreateMultipleTask`; corrected. A second compile changed existing reflected method signatures; corrected by restoring the original APIs and adding separately named task-aware methods. The intermediate Q06 job `a48bcc60e83e4fafa19ef84714a1115d` failed on that temporary signature break, and stale-assembly Q07 job `64f4ffce060d4c2ea24696a78138528e` is excluded from final evidence. Final `Assembly-CSharp.dll` and `Assembly-CSharp-Editor.dll` timestamps exceed edited script timestamps; no new C# compile error in the latest Editor log. `Tools/Validate-ChangeLedger.ps1` passed (888 records/26 governed diff files) and `git diff --check` exited 0. Protected Menu, Battle, GameConfig and project mode Asset hashes remained identical to the pretest values.

This package remains `RUNTIME_PENDING`: it closes only the explicit newborn-vitals first difference. The later source X mismatch needs a separate authority/Unity same-tick motion trace. Q07, D-024, old-resource disposition, Q08 and the overall alignment goal remain open. DAT values, images, Scene, camera and nonbattle logic were not edited here.
