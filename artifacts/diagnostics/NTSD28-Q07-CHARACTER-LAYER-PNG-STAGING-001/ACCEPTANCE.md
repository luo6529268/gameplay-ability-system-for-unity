# NTSD28-Q07-CHARACTER-LAYER-PNG-STAGING-001 acceptance

Status: `VERIFIED_STAGING_ONLY` (2026-09-22). The three formal PNG destinations were confirmed absent before copy. Each formal source was checked against the exact Task Contract SHA-256; each copied destination then matched the same SHA and byte length:

| Staged path under `Assets/NTSD/Content/LoganRuntime/vfs` | Bytes | SHA-256 |
|---|---:|---|
| `c/0/M.png` | 197436 | `3DF00A8D69C0F8654F820085D946932C8DE256978C9ECB6B34B1DA3E0572E6D8` |
| `custom/1genma/Mclone.png` | 4338 | `6106F2C193F3BCB2658F98C5FDDDD705EAC5C9BAC7C0DF6FDC9BCE82629A51D5` |
| `custom/1genma/Mgenma.png` | 2049 | `E8BEB851D603FFDA7E279F05152046C3EBB5329A6399B5B63065095226D9C8F5` |

The formal runtime still has 1,255 PNG. Staged PNG increased from 1,012 to **1,015**; formal PNG absent from the staged root fell from 243 to **240**, now split into `b/*` 110 and `sprite/*` 130. There are no remaining formal `c/*` or `custom/*` PNG paths absent from the staged root. This is a path/count and exact-byte result, not a claim that all remaining `sprite/*` PNG are nonbattle or covered by a presentation exception.

The staged catalog-object DAT files already refer to these three images through `layer: pic:`. Current `LoganVisualContentCandidate.Capture` enumerates `characterData.files`, `head` and `small` only; it does not add `<layer>` images to its visual fingerprint or publish them for drawing. Therefore copied bytes **do not** close layer parsing, publication freshness, ordering, atlas binding, or battle pixels. A separate Q07/Q09/R17 Task/Change must trace the formal layer render path before connecting these resources. The current native/common visual work and Q06 logic remain unchanged.

`git status --short --` showed only the three intended new PNG under these paths; `git diff --check` returned exit 0. No old file, `.meta`, Scene, Prefab, DAT, script, audio, background or menu image was edited or removed by this package. Original Unity import/Play was not run, and Q07 remains in progress. The original Editor still has stale assemblies; no second Editor or computer-use was used.

## Authority correction after acceptance

The earlier paragraph's proposed Q07/Q09 battle-layer follow-up is **superseded**. Formal `DatParser` accepts these `layer:` rows in `<menu_face>` context only, and `GameSession28`/D3D11 use them on selection slots, not in the confirmed battle entity render path. The three exact bytes remain staged as formally referenced character portrait resources, with no menu logic or visuals activated by this copy. Do not treat them as evidence of battle presentation progress or open a Q09 package for them. The 1,015/240 PNG counts and SHA verification remain true; scope classification was corrected before any further implementation.
