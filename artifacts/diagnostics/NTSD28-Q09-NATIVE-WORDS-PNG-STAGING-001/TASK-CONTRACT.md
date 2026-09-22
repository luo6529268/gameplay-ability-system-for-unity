# NTSD28-Q09-NATIVE-WORDS-PNG-STAGING-001

Status before mutation: `PLANNED`. Scope: exact-byte staging in the original Unity project only. No Unity Editor is launched.

## Authority and reason

The formal NTSD 2.8-Logan `resources/runtime/decoded_dat/data/resource.dat` lines 18-23 list `sprite\\UI\\WORDS0.png` through `WORDS5.png` (resource indices 16-21). The staged decoded `resource.dat` has the same six paths. The formal playable `GameSession28::initialize_resources` resolves those six slots (`game_session.cpp:1217-1241`); `render_snapshot.cpp:778-816` assigns the chosen glyph resource to in-battle name/HUD commands; `d3d11_renderer.cpp:1739-1770` draws entity nameplates from the selected native glyph atlas when available. These are battle presentation dependencies, not character selection menu-face layers. Unity `BattleCommonVisualCatalog.WithWords` still describes the old `WORDS0.bmp`-`WORDS5.bmp` sheets; staging bytes does not switch its runtime reader or prove pixel parity.

## Exact authorized file list

Copy from formal `resources/runtime/vfs/sprite/UI/` to matching paths under original-project `Assets/NTSD/Content/LoganRuntime/vfs/sprite/UI/`. Every destination must be absent before copy. No other files may be copied, overwritten or deleted.

| File | Bytes | SHA-256 |
|---|---:|---|
| `WORDS0.png` | 8505 | `CE459CEC36289543D4A4D344B133E35E9D9A5BCD5DDE0B07AFE9E1C99AF463E5` |
| `WORDS1.png` | 8902 | `E959F2C6A992ED75DA9AFB335F379D35ECAB686D1E9F4B3B60E240D084B265DC` |
| `WORDS2.png` | 8733 | `B75FDC4B8413A1427BEB7482DF10621F91B9B5BCC91589CA6CC1E7AACB673E0A` |
| `WORDS3.png` | 8769 | `0A1BE25062D8043C9614BF0180FB13F7068B3BA62B77B6533AA873EA6826451A` |
| `WORDS4.png` | 8769 | `20879D07EE7DEFFBD1EFF3592E1FE24595B0D2E600EE4755F1704616311FBAA6` |
| `WORDS5.png` | 8418 | `6F704853CC86D70349C724411E74C89B93EF6BCD8DD637BA321FA1B4ECC73F90` |

## Invariants, validation, rollback

Preserve the original Unity/GAS project, all old BMPs, menu/UI code, Scene, Prefab, GameConfig, `.meta`, DAT, scripts and nonbattle behavior. No deletion is authorized. Verify formal source hashes and lengths before copy, destination hashes and lengths after copy, exact six-path Git status, `git diff --check`, and remaining formal PNG counts. Expected staged count is 1021 and expected formal missing count is 234 if no concurrent changes occur. Unity import, reader switch, actual nameplate pixels and formal-EXE parity remain separate Q09/R17 work; do not label this package runtime-verified. If a copy fails, stop; inspect exact paths before any proposed rollback, which remains subject to deletion authorization.
