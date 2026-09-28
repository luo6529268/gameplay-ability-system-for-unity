# Q07 formal content current-state hash audit (2026-09-28)

Read-only comparison of formal `NTSD 2.8-Logan/resources/runtime` with Unity `Assets/NTSD/Content/LoganRuntime`. Formal root EXE SHA-256 was rechecked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Paths were normalized case-insensitively, and every staged same-path DAT/PNG was compared by length and full-file SHA-256; no DAT value, asset, script, Scene or configuration was changed.

| Scope | Formal files | Staged files | Exact same-path SHA | Missing | Different | Extra staged |
|---|---:|---:|---:|---:|---:|---:|
| DAT | 405 | 338 | 338 | 67 | 0 | 0 |
| PNG | 1255 | 1031 | 1031 | 224 | 0 | 0 |

The 67 missing DAT divide into 50 user-excluded background/background-mode files (`b/*/b.dat`, `data/bg_mode.dat`, `data/bg/*.dat`), two user-excluded mode files (`data/mode.dat`, `data/mode/ntsd.dat`), and 15 files with separate owners: `data/frame/inkhud2.dat` (1), `data/menu.dat` (1), `data/minibar.dat` plus four `data/minibar/*` (5), user-held default `data/stage.dat` (1), and its seven `s/*` story children (7). The non-excluded total is therefore 353, with 338 identical staged files and 15 missing. Compared with the 2026-09-25 337/353 audit, `data/bgm.dat` is now staged; its content is identical to the formal file. This does not establish BGM playback.

The 224 missing PNG divide into 110 user-excluded background images under `vfs/b/*` and 114 `vfs/sprite/*` menu/HUD/radar/loading/small resources whose individual consumers must follow the existing Q07 indexed-image audit. All character-related `vfs/c/*` (587) and `vfs/custom/*` (13) PNGs are staged and included in the 1031 exact SHA matches. These counts prove current disk bytes only; they do not prove every runtime binding, sprite publication, natural skill pixel, old-asset retirement, or all Q07 exits.

The immediately actionable Q07 blocker remains D-024 collision-domain policy and its shared consumer repair. Do not recopy the 52 excluded DAT/110 background PNG or deploy the default stage/story files merely to make counts equal. The 114 `sprite/*` items remain on their named consumer/exception audit, not an automatic character-image migration. Prior owner evidence: `NTSD28-Q07-INDEXED-IMAGE-COVERAGE-001/CURRENT-FULL-VFS-PNG-BYTE-AUDIT-20260925.md` and `NTSD28-Q07-REMAINING-DAT-REACHABILITY-001/CURRENT-NONEXCLUDED-353-INVENTORY-20260925.md`.
