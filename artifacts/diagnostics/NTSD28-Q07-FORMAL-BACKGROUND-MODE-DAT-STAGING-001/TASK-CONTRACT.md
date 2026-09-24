# NTSD28-Q07-FORMAL-BACKGROUND-MODE-DAT-STAGING-001

Status: `FILES_STAGED_HASH_VERIFIED / RUNTIME_READER_PENDING` (2026-09-24). Parent: BATCH-04/Q07; downstream mode-record binding belongs to separate Q07/Q08 production work.

## Authority and exact scope

The locked formal NTSD 2.8-Logan playable `GameSession28` loads `data/bg_mode.dat` and its 25 child `data/bg/*.dat` files. At `start_battle`, `apply_native_background_mode_record` projects the selected record into battle configuration: hit gate, attacking percent, HP/MP regeneration, injury MP gain, dynamic boundary, weapon drop, stage/revive, `recmp` and `caughtact` inputs. The child DATs therefore hold battle rules even where the surrounding native selection UI is excluded. The user's D-023 formal-DAT authority applies; DAT token/value edits remain forbidden.

`MANIFEST.csv` (26 rows, SHA-256 `9558E6CC9DF07ECB12FB86DDDB28F752EF703AD0028A74F5901486BDE97E933B`) lists each formal relative DAT path, byte length and SHA-256. Sources are under `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime\decoded_dat`; targets use the same relative paths under `Assets/NTSD/Content/LoganRuntime/decoded_dat`.

## Change boundary and verification

1. Confirm all 26 formal source hashes and all 26 target absences before copying any file. Copy only those exact bytes. No overwrite or token editing.
2. Do not change Menu/selection UI, Scene, Prefab, camera, boundary geometry, scripts, existing content, background PNGs, audio, or default `stage.dat`.
3. Verify all staged sizes and SHA-256 against the manifest; refresh only the existing Unity project Editor to obtain GUID metas and verify they are unique under `Assets`; confirm Battle/Menu Scene disk hashes unchanged.
4. This closes content staging only. It does not prove active record selection, Unity production consumption, same-seed battle parity, or Play behavior. A separate script Task/Change must audit and bind only required mode fields in the correct pre-tick order, preserving user exceptions.
5. If rollback becomes necessary, seek the repository-required deletion approval and remove only package-owned files after checking hashes and references; no automatic cleanup.

## Actual result (2026-09-24)

The manifest had 26 entries. All 26 formal sources passed size/SHA verification and all staged targets were absent before copy; all destination files then matched the manifest. The existing original Unity Editor refreshed, generated 26 DAT meta GUIDs, and returned idle/non-Play after domain reload. All 26 new GUIDs are distinct and each appears once under current `Assets`. Saved Battle/Menu Scene SHA-256 remained `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39` / `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. A fresh comparison of all formal/staged DATs gave formal405, staged388, SHA-equal388, missing17, differing0. The 17 exact remaining names and hashes are in `../NTSD28-Q07-BACKGROUND-HUD-CALLER-AUDIT-001/current-formal-dat-not-staged-after-bg-mode.csv`. No production reader, UI, Scene, picture, audio, DAT token, or prior asset was changed; no battle-mode runtime parity is claimed.
