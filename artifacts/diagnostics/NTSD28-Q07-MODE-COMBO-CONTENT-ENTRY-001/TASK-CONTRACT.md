# NTSD28-Q07-MODE-COMBO-CONTENT-ENTRY-001

Status: `FILES_STAGED_HASH_VERIFIED / RUNTIME_PENDING` (2026-09-22). Parent: BATCH-04/Q07; downstream Q09 combo presentation. This package stages only the formal mode pointer, its selected child DAT and the child `<combo>` picture into the existing original Unity project's LoganRuntime root. It does not enable a new reader or alter menu, result-page, scene, project settings, battle logic, or Q06 combo production.

Formal playable chain: `GameSession28` reads `data/mode.dat`, selects `data/mode/ntsd.dat`, loads `NativeComboHud28` from its `<combo>` block, resolves `sprite/combo_hits.png`, and passes the config to the render snapshot (`game_session.cpp:1332-1374,3330-3332`; `native_combo_hud.cpp`). The same child contains `<menu_control>`, `<menu_small>` and native KO-feed data. Existing P-19/G-08 UI exceptions remain; merely staging the whole formal DAT does not authorize changing those flows. The root `mode.dat` alone is not a complete battle-combo dependency.

| Relative path | Formal bytes | Formal SHA-256 | Current staged target |
|---|---:|---|---|
| `decoded_dat/data/mode.dat` | 78 | `A1BEBF1E92A8438206EFD1FBDB7ECD6AE24AF9865858C217301937537C8E73A9` | absent |
| `decoded_dat/data/mode/ntsd.dat` | 5,097 | `556DF3014F575D25D0642E840AC70F27CEB52DADA7109FEA666458FA0F316F12` | absent |
| `vfs/sprite/combo_hits.png` | 2,697 | `1D398C0AE0AE01495B7FF5FEC78B7978690CFE53B98D10913E1BCEC92426AD48` | absent |

Copy only these three absent files to matching relative paths after failing if any target already exists. Verify each staged SHA-256/byte size, legacy files and Scene/ProjectSettings unchanged, and record actual Git status. No code edit or test run belongs to this content-only Task. A later Q09 Task/Change must parse only the relevant combat combo fields, prove it does not alter excluded menu/result flows, and perform same-hit command/pixel verification in the original project. If a rollback is needed, first obtain repository-required deletion approval for these exact new files; do not automatically remove or overwrite any file.

## Actual result (2026-09-22)

All three targets were absent before `Copy-Item -LiteralPath`. Each copied file matches its formal byte count and SHA-256 above. With the earlier global-spark content package, the staged root now contains 335 DAT, 1,012 PNG and 0 WAV; 70 formal DAT remain unstaged, including the explicitly deferred default `stage.dat`. Scoped `git status` lists only these three new content files and no script, Scene or ProjectSettings change. Legacy `SPARK.bmp` remains. No new reader, compile, Play or visual comparison was performed; Q07 and Q09 remain open.
