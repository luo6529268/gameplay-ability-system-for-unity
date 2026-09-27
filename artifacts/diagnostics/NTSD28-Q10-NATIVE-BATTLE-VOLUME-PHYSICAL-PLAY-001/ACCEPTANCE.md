# Q10 physical F11/F12/F2 battle-volume Play gate — 2026-09-27

Status: `VERIFIED` for the bounded Unity physical-key-to-battle-SFX-percent gate only. Parent O-03/Q10 and the overall goal remain open.

The original Unity Editor PID11944 was idle, not in Play, with the saved `NTSD_Menu.unity` active before the run. Its existing MCP bridge at `127.0.0.1:6402` was used for `refresh_unity`, `manage_editor play`, `execute_menu_item` for the single new Q10 probe, and `manage_editor stop`; no computer-use, second Editor, new Unity project or saved Scene edit was used. The MCP editor-state cache reports `is_changing=true` throughout Play because it maps `EditorApplication.isPlayingOrWillChangePlaymode` directly; actual `is_playing=true`, menu invocation and the completed probe established Play readiness.

The new Editor-only probe was imported/domain-reloaded in that Editor, appeared as an executable menu item, entered the existing additive Battle from Menu after formal content prewarm, and used `InputSystem.QueueStateEvent` plus `InputSystem.Update` for physical keyboard state changes. It observed the production `SimulationTickDriver` and `AppManager.SoundPlayer`:

| Gate | Actual result |
|---|---|
| Formal content root | `Assets/NTSD/Content/LoganRuntime` |
| Initial | tick 2, SFX percent 100 |
| Paused physical F11, no tick | tick unchanged, percent 100, host command `VolumeDown` |
| Physical F11+F2 | tick 2→3, percent 100→99, remained paused |
| Release F11/F2 | tick and percent unchanged |
| Paused physical F11+F12, no tick | percent unchanged, continuous command `VolumeUp` (F12 priority) |
| Physical F11+F12+F2 | tick 3→4, percent 99→100, remained paused |
| Final release | tick and percent unchanged |

Result file `play-result.json` reports `PASS`, all six flags true, `endTick=4`, and `battleUnloaded=true`; SHA-256 `6F0E281BAD41F97529DE9C860CBAC04F5D9AAB2987F00E2D1F3A488C85824DF0`. The probe released injected keys, restored its test bootstrap flag and unloaded the Battle it loaded. MCP stopped Play; the observed Editor then reported non-Play, no compilation and active Menu.

Saved Scene SHA-256 remained Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, Battle `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. Neither Scene appears in `git status --short`. `git diff --check` exited 0. `Tools/Validate-ChangeLedger.ps1` exited 0 with `Change ledger validation PASSED` (929 records, 37 governed code files in this pre-existing dirty tree; unrelated historical warnings remain).

This proves the physical keyboard → successful paused logic tick → production battle SFX percentage path for the selected content and match. It does not prove dedicated worker publication, distinct gain for cues from two ticks batched into one LateUpdate, BGM/WMA, audible device output or formal EXE audio comparison. Those remain Q10/O-03 work; Q07's bounded exit and Q08 independent gates are unchanged.
