# Q07 当前正式资源剩余项与背景/HUD 调用者复核

状态：`READ_ONLY / OWNER_SPLIT_VERIFIED / MIGRATION_NOT_AUTHORIZED_BY_THIS_REPORT`，2026-09-24。此报告承接 `NTSD28-Q07-RESUME-CONTENT-GATE-001`；不修改 DAT token、图片、Scene、菜单或战斗脚本，不授权旧资源删除。

## 当前磁盘事实

- 根目录正式 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。本报告起始基线：正式 `resources/runtime/decoded_dat` 有 405 个 DAT；项目 `Assets/NTSD/Content/LoganRuntime/decoded_dat` 有 338 个，逐路径 SHA-256 比较为 338 个相同、67 个缺失、0 个同名不同字节。67 项的起始清单沿用 `NTSD28-Q07-REMAINING-DAT-REACHABILITY-001/current-formal-dat-not-staged.csv`。**本轮两个独立Task分别暂存24个背景DAT与26个背景模式DAT；当前388/405个同名SHA全部相等，缺17，差异0。`current-formal-dat-not-staged-after-bg-mode.csv`是当前缺项逐文件SHA清单；旧67/43行CSV均为历史快照。**
- 正式 `resources/runtime/vfs` 有 1255 张 PNG，项目暂存 1031 张，按名称剩 224 张；其中正式角色目录 `vfs/c` 的 **587/587 张 PNG 全部暂存且同名 SHA-256 相同**，缺失 0、内容不一致 0。这证明当前角色目录字节覆盖，不证明所有角色画面帧/alpha/排序都已正确消费。其余缺名集中于 `b/` 110 和 `sprite/` 114，旧路径及现存引用仍按旧资源退场审计保护。

## 正式调用者与 Unity 边界

| 项目 | 当前正式 playable 调用 | Unity 当前调用/资源 | 结论与归属 |
|---|---|---|---|
| `data/bg_mode.dat` + `data/bg/*.dat` | `game_session.cpp` 初始化时加载模式组；阵容选择后的背景/模式选择进入 `selected.background_id` 与 `background_mode_index_4a9ff8`。`apply_native_background_mode_record`将命中、攻击、回血/回MP、受伤MP、边界、武器掉落、stage/revive、`recmp`/`caughtact`等字段投影到战斗配置；源码在 playable 构建闭包。 | 26个正式DAT现已精确暂存，但生产脚本未读；`SettingBattleBgController` 的 Background 选项当前仅 `break`，`selectedMapId` 没有更新写者；`CharacterSelectionController` 把确认值写到 `MatchConfig.backgroundId`，Driver 将其写入 Match 状态。 | 这26个DAT有战斗规则用途，已独立暂存；暂存不等于字段接线。菜单属于明确的非战斗保护边界；Q07/Q08须先决定可达默认记录/战斗侧输入，再建脚本Task/Change，不能只复制资源就宣称规则对齐。 |
| `b/*/b.dat` 与背景 PNG | 正式 `data/data.txt` 列出 24 个背景 ID；选中后 `GameSession28` 加载对应 background DAT，renderer 使用其 source path。ID 1 是 `b/San/b.dat`，其正式宽 1330、Z 边界 375..575，并引用多层 `b/San/*.png`。 | 24个DAT已按精确清单暂存，生产Unity尚未读取；`BattleMapCatalog.asset` 只含 `Sunagakure`，`NTSD_Battle.unity` 的 `BattleBootstrap.mapId` 固定为它，背景由单一 Sprite 给出；地图多边形另由项目定义。当前 Stage 快照采用项目边界（Play 曾见宽 2040、Z 237..760）。 | 暂存只证明正式数据在场。用户批准固定全景/项目可行走区域例外并排除原生多层/cycle；不能把 `b/San` 几何值直接覆盖当前地图。后续为确切允许的选择和战斗边界另建 Task/Change 与例外判定。 |
| `data/frame/INKHUD2.dat` 与 `sprite/frame/INKHUD2/*` | 正式 `data/frame.dat` 有 `INKHUD` 和 `INKHUDV2` 两条。`NativeFrameHudCatalog28::captured_active_index` 在 playable 为 0，`GameSession28` 只解析被选中的第 0 条 child；第 1 条 `INKHUD2` 当前并非已证的活动 HUD 子项。 | `data/frame.dat` 与第 0 条 `data/frame/INKHUD.dat` 已暂存；第 1 条 child 与两张 `INKHUD2` PNG 未暂存。Unity 没有发现生产 `INKHUD2` 文本引用。 | 不把第 1 条缺项报成当前活动 HUD 的已证首差。保留 Q09/R17 的可达性与正式视觉验证，若将来有第 1 条选择证据再迁移。 |
| `sprite/radar/*` 16 张 | 活动的 `data/frame/INKHUD.dat` 确实声明了这 16 张图，但同一 `<radar>` 段的 `bound: 0`。`RenderSnapshotBuilder28::build` 仅在 `bound == 1` 时把 `native_radar_bound` 设为真；playable renderer 未发现独立 radar 绘制路径。 | 暂存内容缺这 16 张；Unity 战斗脚本未发现 radar 资源读取。 | **已声明不等于已显示**。按当前正式 DAT token 与 live path，不能把这 16 张列为活动 HUD 缺图；保留 Q09/R17 的条件可达性检查，不能为了清零缺名数而复制。 |

正式 `data/data.txt` 将 0..23 共24个ID映射到24个 `b/*/b.dat`。本轮逐文件审计形成 `formal-background-catalog.csv`（24行、DAT正式SHA/尺寸）与 `formal-background-image-references.csv`（每背景去重后合计133引用，合并为110张PNG；正式源均存在、起始暂存均缺失）。这些DAT在正式 `GameSession28` 的开战出生位置计算中提供width/Z范围，故24个DAT已按 `NTSD28-Q07-FORMAL-BACKGROUND-DAT-STAGING-001` 精确暂存且复核。110张引用图含层、缩略图和共用阴影等不同用途；总表P-18排除原生多层/cycle背景，D-023也不自动覆盖非角色图片，故没有整组复制。`data/bg_mode.dat`及25个child经正式`apply_native_background_mode_record`确认含战斗规则字段，已由`NTSD28-Q07-FORMAL-BACKGROUND-MODE-DAT-STAGING-001`精确暂存，但Unity尚未消费。其余17个DAT为bgm、menu、minibar、sound、INKHUD2、用户暂缓默认stage及7个story/stage DAT；各按Q08/Q09/Q10、非战斗保护和暂缓规则分流。`stage.dat`仍不部署，旧DAT/图片521项删除授权仍为0。

25个正式背景模式child各有3条`<mode>`，共75条；逐行剔除仅用于名称/缩略图的`name`、`small`后，同一记录索引下其余token在25个child之间完全相同，索引0/1/2各只有一种词法签名。索引0含`weapon_drop:2`、`regen_hp:1`、`regen_mp:1`、双受伤MP增益75；索引1无这些显式token；索引2保留`weapon_drop:2`和`regen_hp:1`。这是当前内容的词法事实，不授权在生产脚本里硬编码三套常量或跳过正式DAT解析；正式record结构及默认值以playable loader为准。Unity`MatchConfig`目前只有`backgroundId`/`difficulty`等字段，没有背景模式记录索引，故战斗端读取和可达输入仍是Q07首差。

## 下一可执行出口

1. Q07 背景映射读路径复核结果：`CharacterSelectionController` 把整数写入 `MatchConfig.backgroundId`，`SimulationTickDriver` 只写 `Match.BackgroundId`/`StageIdx`；`AppManager` 调用 `BattleBootstrap.TryPrepareMapConfiguration`，后者只读取 Scene 序列化的字符串 `mapId: Sunagakure` 与单项 `BattleMapCatalog`，没有读取 `MatchConfig.backgroundId`。正式 `data/data.txt` 的 Sunagakure ID 为 1，而 Unity `SettingBattleBgController.selectedMapId` 当前序列化/代码默认值是 0，也不能把二者默认为同义。因此菜单整数选择当前不能控制战斗背景。战斗DAT现已按正式24项暂存；下一精确包须先定义整数ID与项目地图的战斗端映射、其余ID处理范围，以及项目多边形/全景相机例外下的出生规则。不得仅因已有DAT就启用正式几何数值或改Scene/菜单。
2. 背景选择 UI 若确为正式模式整链必需，应准备最小变更和非战斗影响分析后单独取得范围扩展决定；本报告不授权修改 `SettingBattleBgController` 或普通菜单。
3. Q09 对第 0 条活动 HUD 做实际表现对照；第 1 条 `INKHUD2` 保持 `REACHABILITY_UNPROVEN`，16 张 radar 图保持 `DECLARED_BOUND_0`。不能以缺文件等同于当前运行时缺图。Q08/Q10 的 stage 与音频前置按各自专项继续。

证据性质：正式 EXE 身份哈希与当前磁盘枚举为新鲜读取；正式规则/调用者依据正式 playable 构建闭包中的源码；Unity 路径由当前脚本、资产和 Scene YAML 静态读取。50个DAT暂存的逐文件SHA与原Editor导入见两个独立Task；未执行新一轮背景选择Play、正式EXE操作或像素对照，故没有宣称完整背景/HUD对齐。
