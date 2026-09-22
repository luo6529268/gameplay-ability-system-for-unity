# NTSD28-Q07-GLOBAL-SPARK-CONTENT-ENTRY-001

Status: `FILES_STAGED_HASH_VERIFIED / RUNTIME_PENDING` (2026-09-22). Parent: BATCH-04/Q07; downstream consumer BATCH-05/Q09. This package stages exactly three formal files in the **existing original Unity project**. It does not switch a runtime reader, change battle rules, touch Menu/Scene/ProjectSettings, or remove an old asset.

## Authority and source

Formal root: `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime` under the locked release. `GameSession28` loads `data/resource.dat` and `data/system.dat`; native `resource.dat` index 43 resolves the battle global `sprite\UI\SPARK.png`, and `system.dat` supplies `spark_w: 99`, `spark_h: 79`. The exact call chain and Unity first difference are in `artifacts/diagnostics/NTSD28-Q09-GLOBAL-SPARK-RESOURCE-BOUNDARY-001/REPORT.md`.

| Relative path in formal/runtime and staged LoganRuntime | Formal bytes | Formal SHA-256 | Existing staged target before work |
|---|---:|---|---|
| `decoded_dat/data/resource.dat` | 1,636 | `B8E31B33A2D42924C57BB6DEC8571F494DD1E3FDDC38916A77471119526090E6` | absent |
| `decoded_dat/data/system.dat` | 3,091 | `D848CC6D711A01D12A054E2B5ADB903938D7AFB182854EB39DDBBF2236C31AE2` | absent |
| `vfs/sprite/UI/SPARK.png` | 3,600 | `15D8843E0CE87FF63F46DFF7170D30C23BAEA0F2799434B26717AADFD5EC881B` | absent |

The DATs are whole formal files and contain other native configuration entries. Staging them is part of the user's formal-DAT choice, but this package does not activate any new nonbattle reader. The PNG is the precise global battle-hit resource, not a blanket UI image migration. `vfs/c/kid/a/spark.png` is a different already-staged character skill image and must not be substituted.

## Change boundary, verification, rollback

- Copy only those three absent files into `Assets/NTSD/Content/LoganRuntime` at the same relative paths. Fail closed if any target appears before copy. Do not overwrite, delete, move, or touch legacy `Assets/NTSD/Sprite/UIPanels/SPARK.bmp` or default `stage.dat`.
- Verify each staged file byte count and SHA-256 against the table; verify the old BMP remains; inspect Git status/diff and confirm no unexpected path changed. No Editor/Player result is claimed by a content-only copy.
- If the package must be rolled back, obtain the deletion approval required by the repository `AGENTS.md` and remove only these newly staged targets after verifying their hashes and references. Do not automatically delete them or their generated `.meta` files.
- Q09's later, separate Task/Change must wire the correct loader/publication and verify same-hit source rectangles and pixels in the original Unity project. Q06 producer and lifecycle evidence remains intact.

## Actual result (2026-09-22)

The locked formal EXE SHA-256 was rechecked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The three exact absent targets were copied with `Copy-Item -LiteralPath` after checking all formal sources and all target absence. Each staged file matches the byte count and SHA-256 in the table. Staged content now has 333 DAT and 1,011 PNG. The old `SPARK.bmp` remains. Scoped `git status` lists only the three new target files; no Scene or ProjectSettings change was observed in that check. `git diff --check` passed. No runtime reader was changed and no Unity compile, test, Play or visual comparison was run. Q07 and Q09 remain open.
