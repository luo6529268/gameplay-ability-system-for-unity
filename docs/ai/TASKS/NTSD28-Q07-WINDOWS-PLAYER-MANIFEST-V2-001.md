# NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001

Status: VERIFIED_PLAYER_CONTENT_COPY_GATE. Parent: BATCH-04/Q07 Player cold-start dependency. Original Unity repository only.

Authority: D-023 formal `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime` DAT/character-image content; formal EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The existing 1343-row `copy-manifest.csv` is a preserved historical staged subset, not the present production-root closure.

Pre-change first difference: current `Assets/NTSD/Content/LoganRuntime` has 1371 non-meta files / 46,883,057 bytes, while `NTSD28Q07WindowsContentBuildProcessor` hardcodes 1343 / 46,594,829 and rejects additional files. The 28 added files are 7 DAT and 21 PNG; all 1371 staged files were freshly checked against formal same-relative-path bytes and SHA-256 with zero differences. Direct Windows Player builds would fail at the postbuild source-set gate.

Exact write scope: new `artifacts/diagnostics/NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001/copy-manifest-v2.csv` plus `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs` constants selecting that manifest/count/byte total. Task/Change/Ledger/STATE/handoff/alignment entries. Preserve original `copy-manifest.csv`, nonbattle Menu code, gameplay scripts, Scenes, Prefabs, ProjectSettings, old content and all preexisting dirty files. This package does not launch Player or edit menu flow.

Acceptance: generated v2 manifest has exactly one row per staged non-meta file, no duplicate/escape path, sorted deterministic order, current size and SHA, all rows matching formal root; old manifest remains unchanged. Original Editor compiles updated postbuild processor; a later separately governed Menu-first Development Player build must pass processor's source/destination checks. Until that build, status is only `COMPILE_PASS / PLAYER_BUILD_PENDING`. Run scoped diff and Change Ledger validator.

Rollback: restore only this processor's constant changes after reviewing current diff and following the repository's explicit approval rule for restore/deletion; preserve the v1 manifest and other user work. The new v2 artifact is removable only after the same explicit approval.
