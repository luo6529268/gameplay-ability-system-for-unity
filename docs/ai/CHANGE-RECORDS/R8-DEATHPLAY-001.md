# R8-DEATHPLAY-001 — live death / respawn / AI / integer S4 probe

<!-- CHANGE-RECORD
id: R8-DEATHPLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleDeathRespawnAiIntegerPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\core\main.cpp;src\input\input_handler.cpp;src\entity\frame_advance.cpp;src\entity\game_tick.cpp;Makefile
evidence: R3-AI-LIFE-001 and R3-SYNC-RESP-001 source/compile/self-check evidence; WP01C-04 lethal-damage S4; live joint lifecycle pending
-->

> 创建日期：2026-08-23  
> 状态：`VERIFIED`  
> 所属：`R8-WP01C-05`

## 1. 修改前状态

- C++ AI-before-cleanup、state14 hit-stop、respawn gate与三分支字段合同已只读闭合；
- Unity现有production实现和self-check静态映射未发现新首差；
- `R3-AI-LIFE-001`、`R3-SYNC-RESP-001`仍缺同一live world的联合S4；
- WP01C-04已证明production lethal damage，但本包尚无完整death checkpoint→respawn报告。

## 2. 允许改动

- 仅新增 `Assets/NTSD/Scripts/Test/Editor/BattleDeathRespawnAiIntegerPlayModeProbeEditor.cs` 及meta；
- 更新本包Task、Ledger、STATE、handoff与证据文档；
- 禁止修改production gameplay、AI policy、scheduler、DAT/scene、render/URP、C++ authority。

## 3. 预期副作用与保护

- probe在driver paused且worker idle时运行，只注册自有逻辑fixtures；
- RNG、pending sounds、对象/slot/pool与pause先保存后恢复；
- OID998生产效果若生成，必须纳入owned cleanup；
- fixture不依赖具体角色/技能特判；
- production first-difference只能让本Record进入BLOCKED，不能扩权修行为。

## 4. 验收

- Task required matrix全部PASS；
- fresh compile、focused、clean Play、full self-check、validator与diff check；
- 输出逐tick/pass状态与cleanup证据；
- full C++ trace继续BLOCKED，不扩大结论。

## 5. 回滚

- 删除新增probe/meta并把本Record标为`ROLLED_BACK`；
- 不触碰其他dirty-worktree内容。

## 6. 实际改动（待编译）

- 新增Editor-only显式Play probe及meta；
- 在paused/worker-idle live world运行HP=0 AI input、state14 0→30→4、no-count/stored/free三分支；
- no-count用两名same-relation ally的stale integer坐标和live double坐标故意分离，预测并校验两次RNG；
- stored-count使用production OID998 factory并校验action6、位置、relation与spawner；
- 明确验证no-count无额外relation/link/holder/target writer，stored-count只重写relation；
- cleanup恢复RNG、pending sounds、fixtures/effect、object/slot/pool和pause；
- production gameplay、AI、scheduler、DAT/scene、render与C++均0改动。

当前状态`CODE_WRITTEN`；尚未取得compile、focused、Play或self-check证据。

首次Play入口未执行到probe：Windows stdio bridge把中文MenuItem路径传成乱码，Unity返回menu item invalid。
已仅把Editor probe菜单路径改为ASCII；production与行为矩阵未变，需重新compile/Play。

## 7. Final verification

| 层级 | 结果 | 状态 |
|---|---|---|
| compile | all-scope refresh；Editor DLL 13:50:36；Console C# error 0 | `PASS` |
| AI focused | job `bfe625591b59498faf28aa29a7a65a86` 85/85 | `PASS` |
| W05 focused | exact `f7323ae3265c40f89786ad26f73580ae` 1/1；isolated `b87c9db826b244f1965aee87147cf29e` 8/8 | `PASS` |
| clean Play | HP=0 AI、state14 30→4、no-count/stored/free、OID998、字段边界 | `PASS` |
| cleanup | objects/claimed/pools 4/2/2/2恢复，RNG/sounds/pause恢复 | `PASS` |
| Play Console | error 0 | `PASS` |
| full self-check | 2026-08-23 13:52:04 | `PASS` |
| ledger validator | 70 records / 69 governed code files | `PASS` |
| production changes | 无 | `NOT APPLICABLE` |

组合运行AI+W05时W05B曾记录一次mount generation断言失败；随后exact与isolated class均通过，证明是
跨group静态状态污染，不是本探针或production first-difference。失败事实保留，未为它修改production。

final报告：`Temp/NTSD_R8_WP01C_05_DeathRespawnAiInteger.result.json`。本Record状态为`VERIFIED`，只裁决
WP01C-05 Unity production Play S4；worker inactive、C++ full trace BLOCKED，不得扩大为完整战斗对齐。
