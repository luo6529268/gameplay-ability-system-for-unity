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
