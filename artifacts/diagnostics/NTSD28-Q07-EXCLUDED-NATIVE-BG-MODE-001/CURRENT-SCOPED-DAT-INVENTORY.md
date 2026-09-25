# Q07 排除背景与模式 DAT 后的新鲜目录对照（2026-09-24）

正式 `resources/runtime/decoded_dat` 共405个 DAT。依用户确认排除 `b/*/b.dat` 24个、`data/bg_mode.dat` + `data/bg/*.dat` 26个、`data/mode.dat` + `data/mode/ntsd.dat` 2个，共52个；当前范围上限353个。项目 `Assets/NTSD/Content/LoganRuntime/decoded_dat` 现有336个 DAT，全部在这353个同路径集合内且逐文件 SHA-256 相同；没有额外或不同内容。缺17个不等同于17项 Q07 必迁移任务。

| 剩余缺项 | 已知边界/下一 owner |
|---|---|
| `data/frame/INKHUD2.dat` | 正式 `frame.dat` 当前活动索引0，对应 `INKHUD` 已暂存；index1 未证活动，Q09 需活动证据。 |
| `data/menu.dat` | 原生选择/菜单流程排除，不作为 Q07 战斗资源迁移目标。 |
| `data/bgm.dat` | BGM/选择路径属音频与原生选择边界；Q10 按活动使用证据处理。 |
| `data/minibar.dat`、`data/minibar/none.dat`、`st1.dat`、`st2.dat`、`stage.dat` | 原生 HUD/mini bar 表现排除；不得因目录缺失整组复制。 |
| `data/sound.dat` | 非排除的 DAT，但其声音资源/事件由 Q10 独立处理；不以目录缺失直接宣布实际静音。 |
| `data/stage.dat`、`s/0`～`s/5` 及 `s/S` 下的七个 stage DAT | 默认 stage.dat 部署已暂缓；战斗规则 Q08/T8 与实际内容部署分开，需按既有暂缓与活动 stage 条件回访。 |

本对照仅是目录与 SHA 证据；活动引用、parser、发布及真实 Play 需各 owner 单独验收。用户批准的项目背景/模式例外不是正式 EXE 的数据字节一致性结论。旧的“正式405/暂存388、缺17”属于移出52个文件前的历史快照，不应再被当作现状。

2026-09-25 current-disk rerun: `CURRENT-SCOPED-DAT-INVENTORY-20260925.json`重新逐SHA核对正式405、用户排除52、目标353、暂存336，336个同路径全部同SHA；无额外、无不同，缺失仍为上表17个。此轮未复制、修改或删除DAT；17项的活动使用/owner状态并未因磁盘对照而自动关闭。

2026-09-25 后续内容暂存：经 `NTSD28-Q07-SOUND-TABLE-DAT-STAGING-001` 独立范围核对，正式 playable 音频后端实际读取且未被用户排除的 `data/sound.dat` 已原字节加入暂存根（397字节、SHA `7DA4AAD9E3729C44FD6F1D9A4659BC1C925069A7FDD405DD30D2E5BEA11B1C12`），新增唯一 `.meta`。**当前**重新逐SHA统计变为正式405、排除52、目标353、暂存337、同SHA337、额外0、不同0、缺16；机器清单为 `CURRENT-SCOPED-DAT-INVENTORY-AFTER-SOUND-20260925.json`。上表“缺17”及旧 `CURRENT-SCOPED-DAT-INVENTORY-20260925.json` 均是本次加入前快照。WAV、音频规则、Unity导入/播放未处理，仍归Q10；其余16项按原owner和stage暂缓分别处理。验收边界见 `SOUND-TABLE-DAT-STAGING-ACCEPTANCE-20260925.md`。

余项 `data/bgm.dat` 的Q10 owner进一步核对：正式playable的 `GameSession28` 在构造时读取其8条name/WMA表，赛前选择可影响入战音乐；正式VFS有8/8 WMA、Unity暂存VFS有0/8。它未被用户排除，但涉及保留的Menu与音频部署，不能只为缩减“缺16”机械复制。精确边界见 `artifacts/diagnostics/NTSD28-Q10-FRAME-SOUND-ENTRY-READINESS-001/BGM-TABLE-OWNER-20260925.md`。

2026-09-25 `data/frame/INKHUD2.dat` 活动性补证：正式 `frame.dat` 的索引0为已暂存 `INKHUD.dat`，索引1才是尚未暂存的 `INKHUD2.dat`；参与 playable 构建的 `GameSession28` 当前通过 `captured_active_index=0` 只解析所选子文件。本条将上表“index1 未证活动”收紧为“所检正式 playable 选择路径未选中 index1”，不等于全域不可达或 HUD 像素已验。具体调用链和边界见 `INKHUD2-ACTIVE-INDEX-CLOSURE-20260925.md`。未复制、修改或删除该 DAT。
