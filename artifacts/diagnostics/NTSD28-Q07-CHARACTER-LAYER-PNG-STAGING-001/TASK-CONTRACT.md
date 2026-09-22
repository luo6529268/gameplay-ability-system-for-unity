# NTSD28-Q07-CHARACTER-LAYER-PNG-STAGING-001

Status before mutation: `PLANNED` (2026-09-22). Parent: BATCH-04/Q07, R17. Scope is **three exact formal character/skill layer PNG bytes**, no script or loader connection. Formal authority is the D-023 `NTSD 2.8-Logan/resources/runtime/vfs` under the user-confirmed release; the current staged object catalog and DAT files are read-only inputs.

After exact copy and hash/count verification: `VERIFIED_STAGING_ONLY`; see `ACCEPTANCE.md`. The original pre-mutation contract above is retained.

## Why these three files

The current staged root has 1,012 PNG, while formal runtime has 1,255. A path-by-path missing-file scan found one `c/*` and two `custom/*` PNG among the 243 missing. Unlike the 110 `b/*` background and 130 `sprite/*` mixed HUD/menu resources, these three are explicitly referenced by already staged catalog-object DAT `<layer>` records:

| Exact relative path under `vfs` | Formal bytes | Formal SHA-256 | Formal dimensions | Confirmed staged DAT reader input |
|---|---:|---|---|---|
| `c/0/M.png` | 197436 | `3DF00A8D69C0F8654F820085D946932C8DE256978C9ECB6B34B1DA3E0572E6D8` | 2020x2020 | `m/hug/hug.dat` line 30 and other indexed character DAT layer records |
| `custom/1genma/Mclone.png` | 4338 | `6106F2C193F3BCB2658F98C5FDDDD705EAC5C9BAC7C0DF6FDC9BCE82629A51D5` | 606x100 | `custom/1genma/genmaclone.dat` line 35, catalog OID 903 |
| `custom/1genma/Mgenma.png` | 2049 | `E8BEB851D603FFDA7E279F05152046C3EBB5329A6399B5B63065095226D9C8F5` | 404x100 | `custom/1genma/genma.dat` line 35, catalog OID 901 |

The three representative staged DAT hashes match formal source: `m/hug/hug.dat` `BE15C3D7A4E1A4B607F3B77ECAEC83C3143C9F1B9D98E83C38F0A0F8D14372D1`; `genmaclone.dat` `62ADBC5F619D8DBA3C28AB733F7FE403AE4EB35F257E3070E11B59C4EC73A950`; `genma.dat` `AB8F579FB2729750F76D43867164D635F7CF9542EE45B762B70B3D4AA8F7E5E3`. Their `layer: pic:` references are present in the staged DAT bytes. This proves a formal file dependency, not that Unity currently draws the layers.

## Exact mutation and safety boundary

Copy only these three PNG files from formal `resources/runtime/vfs` into matching paths under `Assets/NTSD/Content/LoganRuntime/vfs`, creating only their parent directories. Before each copy require destination absent. Do not overwrite an existing file, add unrelated `sprite/*` or `b/*`, edit DAT, source, `.meta`, Scene, Prefab, GameConfig, old BMP/PNG, ProjectSettings, or nonbattle flow. No deletion is authorized. The existing Q07 content identity may not yet include these layer files; this staging package must not claim publication freshness or visual behavior.

## Acceptance and rollback boundary

After copying, compare each destination SHA-256 and byte length against the table, recount formal/staged PNG and exact remaining paths, and check `git diff --check` and Git status for the three new paths. Record any Unity-generated `.meta` separately; do not remove or reset it. The expected count is 1,015 staged PNG and 240 formal PNG missing, provided no concurrent asset change occurs. This task is `VERIFIED_STAGING_ONLY` if bytes match; Unity candidate/parser/layer publication/actual pixels remain separate Q07/Q09/R17 exits. If a copy fails, stop and inspect the exact destination; proposed rollback is review of only newly created files and requires the existing deletion authorization rules before execution.

## Post-copy authority correction (2026-09-22)

The pre-copy wording above called these "character/skill layer" inputs and treated their `<layer>` references as potentially battle-visible. A deeper formal-source check **supersedes that classification**: all three `layer:` references are inside `<menu_face>`, parsed only in `MajorContext::menu_face` (`source/ntsd28_core/src/data/dat_parser.cpp:422-429`), and projected/drawn by the selection slot menu-face path (`source/ntsd28_playable/src/game_session.cpp:3745-3771`, `d3d11_renderer.cpp:1240-1267`). They are **character selection portrait layers**, not a confirmed battle draw dependency. This content-only copy falls within the user's formal character-image content decision, but it does not change protected menu behavior or advance Q09 battle presentation. No menu-face parser/presentation change is authorized by this Task. Exact-byte acceptance remains valid; the earlier battle-dependency inference does not.
