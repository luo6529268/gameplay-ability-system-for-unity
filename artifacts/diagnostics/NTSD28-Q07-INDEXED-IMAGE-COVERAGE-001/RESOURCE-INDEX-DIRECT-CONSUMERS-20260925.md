# Q07 direct playable `resource.dat` consumer check (2026-09-25)

Read-only follow-up mapped zero-based `pic:` indices from the formal decoded `data/resource.dat` to the current staged VFS and inspected `GameSession28` call sites in the declared playable closure. The formal root EXE retains SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` from the contemporaneous Q07 release replay; this check read formal source/DAT and Unity staged files, without executing a new Unity run or formal EXE call.

| Consumer | `resource.dat` indices | Current VFS presence | Source |
| --- | --- | --- | --- |
| Native battle nameplate glyph atlas | 16–21, `WORDS0..5` | 6/6 present | `game_session.cpp` 1215–1240 |
| Native battle hit spark | 43, `SPARK` | 1/1 present | `game_session.cpp` 1241–1246 |
| Scoreboard/result screen | 24–26 and 28–31 | 0/7 present | `game_session.cpp` 3350–3374; gate `battle_mode == 0 && phase == result_visible` |
| Story result image | 3, `MENU_CLIP3` | absent | `game_session.cpp` 3375–3390 |
| Selection view projection | 0, 33–38, 41–42 | 0/9 present | `game_session.cpp` 3500–3526 |

The **7/7 directly selected active-battle WORDS/SPARK PNGs are on disk**. This proves neither Unity publication nor drawn-pixel parity, which remain Q09/R17 work. The other listed direct consumers enter protected selection or result surfaces; the project's explicit result-page exclusion must be applied before any staging decision. `system.dat`, the renderer and other dynamic paths may resolve further assets, so this is a direct-call-site classification, not a proof that all 114 remaining `sprite/*` PNGs are nonbattle or approved exceptions. No DAT, PNG, Scene or code was modified.
