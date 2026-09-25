# Q07 current full VFS PNG byte audit (2026-09-25)

2026-09-25 mode-child owner refinement: the user-excluded formal `data/mode/ntsd.dat` references 27 distinct PNGs. Its three battle knockout-icon images and one battle combo atlas are all staged at identical SHA-256 bytes. The other 23 are menu/selection text references; inspected playable post-roster consumers account for `menu_small`, `BG_RANDOM` and team flags, while four exact active callers remain unproven. Do not bulk-copy those 23 for Q07 count parity or call them confirmed battle omissions. This does not certify the battle image publication/draw path; see `MODE-CHILD-IMAGE-OWNER-AUDIT-20260925.md`. Whole-VFS counts remain unchanged.

Status: `READ_ONLY_CURRENT_DISK_BYTES / RUNTIME_AND_PIXELS_PENDING`. The formal root EXE SHA-256 was freshly checked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. This audit recursively enumerated `.png` files under that release's `resources/runtime/vfs` and the current Unity `Assets/NTSD/Content/LoganRuntime/vfs`, then compared every common relative path by SHA-256. No source or target file was edited.

| Disk scope | Formal | Unity staged | Result |
| --- | ---: | ---: | --- |
| Whole VFS PNG tree | 1,255 | 1,031 | All 1,031 staged paths are formal paths and **1,031/1,031 bytes match**; zero extra or differing staged PNG; 224 formal paths absent |
| `c/*` and `custom/*` character-associated directories | 600 | 600 | Zero absent, zero byte differences |
| Absent `b/*` background images | 110 | 0 | User excludes the original background; do not stage for count parity |
| Absent `sprite/*` images | 114 | 0 | Requires owner/exception classification below; not an automatic Q07 character-image gap |

The 114 absent `sprite/*` files group by the next path component as: `frame` 2, `Loading` 2, `menu` 8, `minibar` 4, `radar` 16, `small` 1, `UI` 81. The two `frame/INKHUD2` sprites are tied to the unselected alternate frame-HUD child in the inspected playable battle path; the four `minibar` sprites accompany the P-14 approved Unity overhead-health-bar exception. The `Loading`, `menu`, `small`, `radar`, and broad `UI` groups must be classified by their actual consumer and the existing HUD/menu exceptions before any migration. The active `INKHUD.dat` bound-0 radar reference is already documented in `NTSD28-Q07-BACKGROUND-HUD-CALLER-AUDIT-001`; the 16 absent radar images must not be called an observed pixel defect from the path count alone.

The existing indexed-image audit separately froze 1,010 distinct object-declared paths (`bmp:file`, `head`, `small`, and HUD-only `smallb`) and rechecked them as present and byte-identical; the present full-tree check covers them but does not replace its reference graph. The 906 published character-associated candidates and the 104 `smallb` files excluded from native HUD consumption remain separate contracts. **This audit proves disk identity only**: importer settings, dynamic `layer` or other references, candidate publication, playable visual choice, natural skills, rendered pixels and legacy-image retirement still need their own evidence. Q07 and R17 remain open; no background, nonbattle UI, approved overhead bar or native HUD exception is changed by this inventory.

## Exact DAT-reference backtrace for the 114 absent `sprite/*` paths

Each of the 114 absent relative PNG paths was normalized to forward slashes and case-folded, then searched as an exact substring in every formal decoded `.dat` file. **114/114 have at least one exact DAT text reference; zero lacked one.** The nine referring DAT files have 132 path-to-file incidences in total because some PNGs are referenced by more than one DAT:

| Formal DAT file | Number of absent `sprite/*` paths referenced | Contract owner |
| --- | ---: | --- |
| `data/frame/INKHUD.dat` | 16 | Native participant HUD/radar; radar has `bound:0` in the selected definition and native HUD is a user-excluded presentation surface |
| `data/frame/INKHUD2.dat` | 18 | Alternate index-1 frame HUD child, not selected by the inspected playable battle path |
| `data/menu.dat` | 14 | Protected nonbattle menu |
| `data/minibar/st1.dat`, `st2.dat`, `stage.dat` | 1, 2, 1 | P-14 approved Unity overhead-health-bar exception |
| `data/mode/ntsd.dat` | 23 | Original mode DAT is user-excluded; its project-Asset replacement and distinct battle event/image consumers require their own checks |
| `data/resource.dat` | 48 | Global UI/resource index; contains menu, selection, pause, scoreboard/result and other images, while the battle `SPARK.png` is already staged |
| `data/system.dat` | 9 | Eight `menu_back` layers and one `menu_wait` image in the inspected DAT text |

The missing `sprite/UI` group consists of 81 paths; its directory names alone cannot decide each runtime consumer. This exact-DAT backtrace identifies where to audit a path, and finds no *unreferenced* file among the 114; it does **not** prove that all other C++ literal, dynamic or external consumers are absent, nor that every image in `resource.dat` is an approved exception. The current result gives no evidence to bulk-copy these 114 as required character/skill images. Any newly evidenced non-exception battle visual consumer must be handled in its own Q09/R17 package, keeping menu/result/HUD and P-14 boundaries intact.
