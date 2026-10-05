# Q01 正式根独有 DAT/PNG 的消费者归口（2026-10-05）

状态：`READ_ONLY_SCOPE_CLASSIFICATION / STAGE_HOLD_RETAINED`。此项复用[冻结差异清单](../NTSD28-336B44-Q01-INDEXED-CONTENT-IDENTITY-20261001/REPORT.md)中正式根多出的 14 个 `consumer_review_pending` DAT 和 114 个 `consumer_or_presentation_exception_review_pending` PNG；没有重新定义 338 个已暂存 DAT、1013 张对象 DAT 索引图片或用户允许删除的范围，也没有修改资源、Scene 或脚本。正式版规则身份仍为根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

| 冻结清单中的项目 | 数量 | 正式消费者与本目标归口 |
| --- | ---: | --- |
| `data/menu.dat` | 1 DAT | 文件内是 `menu1` 等带 `pic: sprite\\UI\\menu1\\...` 的菜单层；主菜单不属战斗模拟。 |
| `data/frame/inkhud2.dat`、`data/minibar.dat`、`data/minibar/{none,st1,st2,stage}.dat` | 6 DAT | `inkhud2.dat` 给原生框/条纹图层指定 `FRAME.png`、`BARS.png`；`GameSession28` 加载 minibar catalog 并把选中样式交给 `render_snapshot`/D3D11 的 minibar 命令。这些是普通 HUD/叠加显示，不写战斗实体规则；项目自有 HUD 属用户例外。 |
| `s/{0,1,2,3,4,5}/stage{0,1,2,3,4,5}.dat`、`s/s/su.dat` | 7 DAT | 当前正式 `data/stage.dat` 的七个 `<stage_information>` 各以 `file:` 引用其中一项；`GameSession28::load_native_story_missions28` 只在父 `stage.dat` 存在时解析子体。Unity 当前没有父文件，默认 stage 内容部署按用户要求暂缓。子 DAT 含剧情 phase 和 spawn，**不是普通菜单资源**；未来用户恢复 stage 部署时必须连同父文件及项目地图边界重新审查，不能因本次停排视作永久排除。 |
| `sprite/{loading,menu,small/random,ui}/...` | 92 PNG | 冻结清单分组为 loading 2、menu 8、small/random 1、ui 81。`GameSession28::selection_render_snapshot` 实际取 `sprite\\small\\random.png`、`sprite\\UI\\menu_small\\...` 等选人 UI；其余菜单/加载/暂停/结算图仍按菜单、普通 HUD 或结果页例外处理，不进入对象图片迁移。 |
| `sprite/{frame/inkhud2,minibar,radar}/...` | 22 PNG | 冻结清单分组为 frame 2、minibar 4、radar 16；`inkhud2.dat`、minibar 样式和正式 frame/radar HUD 定义可达这些显示层。它们不是角色帧或战斗对象贴图，归项目自有普通 HUD 表现边界。 |

14 个 DAT 和 114 张 PNG 的逐文件路径仍以原冻结的 [`formal-only-dat.csv`](../NTSD28-336B44-Q01-INDEXED-CONTENT-IDENTITY-20261001/formal-only-dat.csv) 与 [`formal-only-png.csv`](../NTSD28-336B44-Q01-INDEXED-CONTENT-IDENTITY-20261001/formal-only-png.csv) 为准；上述组数按这两份 CSV 的 `classification` 和路径前缀重新计数。`data/stage.dat` 本身是原清单另一条 `stage_hold`，未计入这 14 项；原版背景与两类模式 DAT 同样不在此组。分组说明是**消费者和任务范围分类**，不是证明每个 UI PNG 都已在 Unity 运行时加载，也不是批准删除或禁止未来其它功能使用。

可达依据：正式 playable 构建脚本 `source/ntsd28_playable/scripts/build.ps1` 包含 `game_session.cpp`、Core `render_snapshot.cpp` 与 `d3d11_renderer.cpp`；`GameSession28` 的 story loader 位于 `game_session.cpp:854–973`，父 `stage.dat` 条件入口位于 `game_session.cpp:1448–1464`，minibar 条件入口位于 `game_session.cpp:1309–1330`，选人小图入口位于 `game_session.cpp:3487–3520`，D3D11 minibar 消费位于 `d3d11_renderer.cpp:1662–1705`。正式 `data/stage.dat` 的七条 `file:` 行及 `data/frame/inkhud2.dat`、`data/menu.dat` 的实际文本也已只读核查。Unity 当前暂存根中父 `data/stage.dat` 及上述抽样子 DAT 均缺失，符合已知 hold，而非本轮新增删除。

结论仅限此冻结清单：14 个正式独有 DAT 中 **7 个是明确暂缓的剧情 stage 子内容，7 个是菜单/HUD 显示配置**；114 张 PNG **按实际路径和已定位消费者归入选人/加载/菜单/普通 HUD/结果图**。当前无需为这 128 个文件启动战斗生产修复或批量资源复制；Q01 对 338 个 DAT 的动态解析、对象引用、Unity importer 和未覆盖角色表现仍保持原有限证据边界，若正式可达战斗对象出现具体缺图/错引用，按当前总表由共享消费者开最小回访。
