# Q01 `resource.dat` 主表在当前 playable 的战斗消费者闭合审计

2026-10-03，状态：`READ_ONLY_CURRENT_PLAYABLE_BATTLE_CALLSITE_CLOSED / ROOT_RUNTIME_VISUAL_PENDING`。根正式 `NTSD2.8-Logan.exe` 的本轮 SHA-256 仍为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本报告只读对应 playable 构建闭包中的 `native_resource_catalog.cpp`、`game_session.cpp`、`d3d11_renderer.cpp` 和既有 [55 项 v3 矩阵](../NTSD28-336B44-Q01-RESOURCE-SECONDARY-CONSUMER-20261002/resource-55-path-matrix-v3.csv)；未改、复制或删除 DAT、图片、Unity 脚本、Scene 或配置。

`NativeResourceCatalog28::parse_text` 只收 `data/resource.dat` 的前 48 个 `<bmp_begin>` 图条目；`virtual_path`/`resolved_path` 是这个表的索引出口。当前 playable 的 `NativeResourceCatalog28` 生产实例仅在 `GameSession28` 持有。源码范围内逐一核对所有 `native_resources_` 用法后，未见生产代码调用该实例的 `entries()` 或 `virtual_path()` 来遍历、按其它字段动态选取；所有 `resolved_path()` 消费点均在 `game_session.cpp`，且有可枚举的固定索引或单一 `33+digit` 范围。`build.ps1 -Target playable` 的输入清单包含上述 catalog、session 和 renderer 源文件。

| 当前 playable 的出口 | 主表索引 | 归属 |
| --- | --- | --- |
| 战斗 WORDS0～5、SPARK | 16～21、43 | 当前战斗直接资源；Unity 已暂存 7/7 且逐 SHA 同版，画面验收另计。 |
| 普通结果板 | 24～26、28～31 | 用户排除的结果图文。 |
| 剧情结果 | 3 | 依赖暂缓的剧情入口，且结果图文排除。 |
| 选人 | 0、32～38、41～42 | 非战斗选人流程；索引 7/8 另有虚拟路径直读选人预览。 |
| 加载 | 47 | 非战斗加载流程。 |
| 当前未找到直接 playable 索引消费者 | 1、2、4～6、9～15、22～23、39～40、44～46 | 共 19 项；见下方边界。 |
| 范围排除但其它消费者未证 | 27 | `SCORE_BOARD4.png`，原生结果图文。 |

对这 19 项还检索了当前 playable/Core 生产源码中的文件名常量；未发现直接路径读取。索引 22 `PAUSE.png` 在已检战斗暂停绘制分支没有被加载，该分支使用 `draw_solid`；索引 40 `CMC.png` 的源码注释指向 alternate gate/mode-4，但当前选人快照明确选索引 41，不能由注释推断索引 40 在本轮战斗消费。索引 7/8 的选人预览直读以及独立 `<frame>` 七图已在既有报告分开处理，不能被本次 catalog 调用点闭合漏掉。

**实施判定：** 19 项目前没有已证的当前 playable **战斗**消费者，不能因为它们存在于原版 `resource.dat` 就批量搬入 Unity 战斗资源。本结论比只查单个文件名更窄地封闭了当前 catalog 的生产读取路径，但它不是正式根 EXE 在所有隐藏条件下永不读取这些图片的证明，也不是 Unity Game View 或正式 GPU 画面对齐证书。若后续根 EXE 可观察战斗行为或新的当前 playable 调用链显示真实消费者，再按具体资源重开。Q01 仍须对非例外的活跃战斗七图做可见使用/画面边界判定；Q09/Q12 与总目标继续开放。
