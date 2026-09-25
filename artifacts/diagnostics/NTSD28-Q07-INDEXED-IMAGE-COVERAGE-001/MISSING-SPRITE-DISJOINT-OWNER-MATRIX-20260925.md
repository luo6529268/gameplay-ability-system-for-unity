# Q07 disjoint DAT-reference owner matrix for 114 absent sprite PNGs

Read-only current-disk audit, 2026-09-25. Enumerated formal `resources/runtime/vfs/sprite/**/*.png` absent from `Assets/NTSD/Content/LoganRuntime/vfs`, normalized relative paths case-insensitively, and searched each exact path in the nine formal decoded DAT files identified by the prior all-DAT backtrace. Group keys below are the **complete set of referring DAT files** for each path, so rows do not overlap. No file was copied or edited by the audit.

| Exact DAT-reference set | Unique absent PNGs | Owner interpretation |
| --- | ---: | --- |
| `data/resource.dat` only | 46 | Global index/text owner. Includes 39 absent `<bmp_begin>` entries and seven `UI/extra/*` paths in `<frame>` that the selected 48-entry catalog does not collect. Index-specific selection/result/pause/other reachability still matters. |
| `data/mode/ntsd.dat` only | 21 | User-excluded original mode DAT; prior mode-child audit assigns its 23 total missing referenced PNGs to menu/selection fields, including two shared with `resource.dat`. Mode Asset battle KO/combo images are staged 4/4. |
| Both `data/frame/INKHUD.dat` and `data/frame/INKHUD2.dat` | 16 | Native radar images. Selected frame HUD has `bound:0`; full native HUD is user-excluded. No observed selected battle pixel defect from disk absence alone. |
| `data/menu.dat` only | 14 | Protected nonbattle Menu content. |
| `data/system.dat` only | 9 | Eight `menu_back` layers plus `Loading/menu_wait.png`; Menu/loading owner. |
| `data/minibar/st2.dat` only | 2 | Approved P-14 Unity overhead-health-bar exception. |
| Both `data/mode/ntsd.dat` and `data/resource.dat` | 2 | `sprite/UI/BG_RANDOM.png` and `sprite/UI/CHARMENU.png`; inspected playable selection consumers. |
| `data/frame/INKHUD2.dat` only | 2 | Alternate frame HUD child index 1, not selected by inspected battle path. |
| `data/minibar/stage.dat` only | 1 | P-14 overhead-bar exception. |
| `data/minibar/st1.dat` only | 1 | P-14 overhead-bar exception. |
| **Total** | **114** | Every absent `sprite/*` path has a DAT text owner; this is not proof that every file has an active consumer. |

The selected direct active-battle `resource.dat` paths verified separately are WORDS0..5 at indices 16–21 and SPARK at index 43: all **7/7** are staged, byte-identical to the formal VFS. The two absent Loading paths belong to pre-battle loading; `UI/PAUSE.png` index 22 has no selected image consumer proved in the inspected playable renderer, which draws a solid pause bar. The seven `UI/extra/*` paths remain outside `NativeResourceCatalog28::parse_text`'s `<bmp_begin>` scope. See `RESOURCE-INDEX-DIRECT-CONSUMERS-20260925.md`, `LOADING-PAUSE-CONSUMER-AUDIT-20260925.md`, and `RESOURCE-FRAME-EXTRA-CATALOG-SCOPE-20260925.md` for source anchors.

This matrix closes the **DAT text-reference owner census**, not Q07 content/pixel acceptance. In particular, `resource.dat`-only images outside the seven selected active-battle direct indices still require consumer-specific or approved-exception treatment; C++ literal/dynamic readers, Unity publication, natural skills, and GPU pixels are separate gates. Do not infer 114 absent battle images, 114 approved exceptions, or deletion/copy authorization. Whole-VFS totals remain formal 1,255, staged 1,031, absent 224 = background 110 + sprite 114; all 1,031 common PNGs matched by SHA in the prior byte audit. Formal background and native mode DAT exclusions remain intact; Q07/Q09/R17 and the full goal are open.
