# Q07/C040 controlled full-tick original Battle Scene comparison

Authority: the current formal NTSD 2.8-Logan EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and its corresponding playable `GameSession28::step` / `BattleWorld28::settle_catch_relations`. The four source expectations are the retained [controlled full-tick CSV](../NTSD28-336B44-Q07-C040-FULL-TICK-CONTROL-001/run-01/full-tick.csv), independently rerun with the same CSV SHA. This package compares those controlled conditions with the original Unity project's Battle Scene production Driver.

Original Editor: Unity 2022.3.62f3, original project instance `gameplay-ability-system-for-unity@b1b02287`, sole saved clean `NTSD_Battle` scene. The test-only request probe sets Hinata OID41/action125 at source (500,0,400), Bee OID75/action132 or137 at (550,0,400), reciprocal catch slots 1/0, seed 682973786 and neutral input. Each run executes one complete production `SimulationTickDriver.StepOneTick`, global tick 5→6, from a separately configured Play clone. Requests and outputs use unique names; old request/result files were retained.

| Initial Bee action / hold | Formal source next action / hold / XYZ | Original Scene next action / hold / XYZ | Reciprocal slots | Result |
| --- | --- | --- | --- | --- |
| 132 / 5 | 132 / 4 / (529,1,399) | 132 / 4 / (529,1,399) | 1 / 0 | PASS |
| 132 / 0 | 130 / 0 / (529,1,399) | 130 / 0 / (529,1,399) | 1 / 0 | PASS |
| 137 / 5 | 137 / 4 / (529,-7,399) | 137 / 4 / (529,-7,399) | 1 / 0 | PASS |
| 137 / 0 | 130 / 0 / (529,1,399) | 130 / 0 / (529,1,399) | 1 / 0 | PASS |

Raw original Scene results: `hin41-a125-bee75-a132-h5-scene-01.json`, `hin41-a125-bee75-a132-h0-scene-01.json`, `hin41-a125-bee75-a137-h5-scene-01.json`, `hin41-a125-bee75-a137-h0-scene-01.json` in this directory. Each says `CAPTURED/DONE`, one sample, `exitedPlay=true`, `sceneCleanAfter=true`, no error. The four rows above were checked against the source CSV fields for actor/target action, target hold/source XYZ, and both relationship slots; all 4/4 pass. Unity physical-view coordinates are separately recorded in each raw JSON and do not replace the source-rule comparison.

Build and protection: first generated Editor build failed on three new diagnostic `float` carriers for runtime `double` view coordinates; raw failure retained as `artifacts/diagnostics/NTSD28-336B44-Q07-C040-FULL-TICK-UNITY-SCENE-001-generated-build.txt`. The carrier fix and final four-path request probe built with 0 errors/256 warnings (`...-generated-build-v3.txt`); the original Editor subsequently compiled and ran all four cases. `protected-before.json` was compared after all four runs: Battle, Menu, GameConfig, and ProjectBattleModeConfig SHA-256 hashes all match. The Battle Scene remained clean after each Play. No production, DAT, resource-content, Scene, camera, menu/result or non-battle file was changed by this package.

Scope: `VERIFIED_SCOPED_CONTROLLED_SCENE_TICK` for this four-condition controlled full-tick comparison only. The root formal EXE's replay carrier does not expose internal hold/relation as an equivalent initial-state control; natural physical-input reachability, Game View, and parent C040/Q07/full battle alignment remain open. No percentage or whole-stage completion is inferred from this subexit.

Final checks (2026-10-03): `Tools/Validate-ChangeLedger.ps1` exit 0, `Change ledger validation PASSED`, the new probe covered by this Change ID; existing historical declared-path warnings remain. `git diff --check` exit 0, with only Git line-ending conversion warnings. Final original Editor state via local Unity MCP was idle, non-Play, noncompiling, no tests, sole `NTSD_Battle` scene clean. No full self-check or whole-game suite was rerun for this test-only probe; those layers are not claimed by this certificate.
