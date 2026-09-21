# NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001

Status: `VERIFIED_STAGING_ONLY` (2026-09-22). Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-04/Q07. Authority: formal NTSD 2.8-Logan release runtime, D-023 and the Q01 frozen catalog/reference inventory. This is a project-local **staging** package, not production activation or Q07 exit.

Scope: copy the 1,343 exact `sourceRelative` files in `artifacts/diagnostics/NTSD28-Q07-CONTENT-MIGRATION-READINESS-001/copy-manifest.csv` from the verified formal `resources/runtime` root to `Assets/NTSD/Content/LoganRuntime`, preserving relative paths and hashes. The manifest contains 330 indexed object DAT, 1,010 distinct object-referenced PNG, catalog.csv, data/data.txt and data/fusion.dat. No other files are in scope. Source EXE/catalog/index identities and the fresh per-file 0-issue check are in READINESS.md.

Invariants: reject any pre-existing target or source/hash mismatch; never overwrite or remove old Unity Config/Sprite, Scene, GameConfig, menu, user files, stage.dat, background/ancillary assets, audio, scripts or third-party files. The running battle source remains the old empty-root GameConfig selection. Unity import may generate `.meta` files for new staged assets; inventory those as part of this package and do not touch unrelated imports.

Validation: independently hash every destination against the manifest, count files and unexpected additions; then capture the staged root with the existing Logan candidate/API and compare its identity against the formal root. Actual caller/publication/Play and configuration switch require a following exact Task/Change and are not inferred from the copy. No all-character battle suite is needed for staging alone.

Rollback: since all destinations were absent before this package, the exact new target root can be removed only after separate approval under AGENTS.md deletion rules. Until then leave the staged tree intact; the empty production root means it is inert. Failure must be reported with files left intact, never silently cleaned or overwritten.

Result: 1,343 destination hashes matched, 46,594,829 bytes, expected/actual set 1,343/1,343, no missing/extra files or stage.dat, no `.meta` at verification time. Production GameConfig remains empty-root. Details and scope limits in the Q07 readiness report.
