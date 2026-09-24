# NTSD28-Q07-FORMAL-BACKGROUND-DAT-STAGING-001

Status: `FILES_STAGED_HASH_VERIFIED / RUNTIME_READER_PENDING` (2026-09-24). Parent: BATCH-04/Q07. This is an exact formal-DAT content staging package in the existing Unity project, not a map renderer or scene change.

## Authority and bounded inputs

- User D-023 requires NTSD 2.8-Logan DAT bytes; user forbids DAT token/value edits. The locked formal executable SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`.
- Formal playable `GameSession28` uses `data/data.txt` background IDs 0..23 to load `b/*/b.dat`; after post-roster selection it reads selected background width and Z range when placing combatants. This makes the 24 DATs battle-reachable data, even though their multilayer/cycle image presentation is explicitly excluded by alignment item P-18.
- Exact source paths, formal SHA-256, names and dimensions are the 24 rows in `../NTSD28-Q07-BACKGROUND-HUD-CALLER-AUDIT-001/formal-background-catalog.csv` (SHA-256 `848A362860F6F7AAF187C2AE6BFE7C88645CB2BB5B2513946C81FB4FC9CBBF1C`). Each target is the same relative path under `Assets/NTSD/Content/LoganRuntime/decoded_dat`.

## Change boundary and exit

1. Confirm each of the 24 formal sources exists and each staged target is absent immediately before copying. Copy exact bytes only. No overwrite, mutation, removal, or regeneration of a DAT file.
2. Do not copy the 110 background PNGs: the 24 DATs reference them, but P-18 excludes the native multilayer/cycle renderer, and D-023 does not automatically extend picture authority to noncharacter images. Do not change Menu, Scene, Prefab, map definitions, camera, scripts, legacy content, default `stage.dat`, or the project's walkable polygon.
3. Compare staged length and SHA-256 against each formal source and the manifest; inspect Git status for unexpected paths. This proves content staging only. It does not prove that Unity reads these DATs, applies original placement bounds, or draws native background layers.
4. If rollback is needed, obtain the repository-required approval for deletion, then remove only package-owned targets after rechecking their hashes and references; do not perform automatic cleanup.

Downstream Q07 work must separately decide how formal integer background IDs connect to Unity's fixed scene map and how the user's full-view/walkable-area exceptions affect spawn/bounds. Neither the current Menu default ID 0 nor Scene `Sunagakure` may be silently reinterpreted as formal ID 1.

## Actual result (2026-09-24)

The locked formal EXE hash was rechecked. All 24 source hashes matched the frozen catalog, all targets were absent before copying, and all 24 staged DAT files now match their formal source length and SHA-256. The staged DAT count rose from 338 to 362; 43 formal DAT names remain unstaged. The original project Editor refreshed successfully and created 24 `b.dat.meta` files with 24 distinct GUIDs, each unique among current `Assets` meta files. The Editor returned to idle/non-Play after domain reload; no focused runtime reader or background-selection test was run. Saved Battle/Menu Scene SHA-256 remained `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39` / `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. No PNG, legacy content, menu, map, camera or Scene file was copied or edited by this package.
