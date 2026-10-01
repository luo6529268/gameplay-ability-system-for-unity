# 336B44 / Q01 未暂存 DAT 与 sprite PNG 的当前消费者复核

状态：`READ_ONLY_OWNER_RECHECK / Q01_RUNTIME_COMPATIBILITY_PENDING`（2026-10-01）。只读当前正式 `resources/runtime`、对应 playable live source、Unity 暂存内容和已批准的 P-14/P-17、stage 默认部署例外；没有复制、编辑或删除资源。

在上一份[索引内容身份报告](../NTSD28-336B44-Q01-INDEXED-CONTENT-IDENTITY-20261001/REPORT.md)中，正式版比 Unity 暂存根多 67 DAT 和 224 PNG。67 DAT 中的 52 份背景/模式 DAT 属用户排除范围。余下 15 份的当前归属为：

| DAT | 数量 | 当前归属及边界 |
| --- | ---: | --- |
| `data/stage.dat` | 1 | 默认 stage 资产部署按用户要求暂缓。正式 `GameSession28::initialize` 可选加载它，缺失时记录 story mission catalog 不可用；不因此私自部署。 |
| `s/0/stage0.dat`～`s/5/stage5.dat`、`s/S/su.dat` | 7 | 都由当前正式 `data/stage.dat` 的 `file:` 精确指向，是上述父表的剧情子表。只有恢复该入口时再验战斗可达性。 |
| `data/minibar.dat` 与四个 `data/minibar/*.dat` | 5 | 正式 playable 在初始化时可选读取，并把选中样式传入战斗 render snapshot；这是**正式可达的原生头顶条**。既有 P-14 用户例外保留 Unity 自有头顶血条，故不为补文件数迁移这五项。 |
| `data/frame/INKHUD2.dat` | 1 | 当前正式 `frame.dat` 的备用索引 1；所检 `GameSession28` 固定取活动索引 0 的 `INKHUD.dat`。原生角色 HUD 另属 P-17 用户例外。 |
| `data/menu.dat` | 1 | 菜单定义，处于本战斗目标以外；未把文件名负搜索当成全运行时不可达证明。 |

剩余 114 张非背景 `sprite/` PNG 在**当前正式 DAT 文本**中均找到至少一条精确引用；330 个对象 DAT 没有引用其中任何一张。按 DAT 所有者去重后为：

| 所有者 | 不同 PNG 数 | 当前处理 |
| --- | ---: | --- |
| `data/resource.dat` 独有 | 46 | 全局 UI/resource 入口；实际战斗消费者和 P-19 等表现例外逐项裁决。 |
| 排除的 `data/mode/ntsd.dat` 独有 | 21 | 模式 DAT 排除项；不自动迁移。 |
| `resource.dat` 与排除的 mode 子表共有 | 2 | 保留共享 owner；不能只按 mode 排除。 |
| 活动 `INKHUD.dat` 与备用 `INKHUD2.dat` 共有 | 16 | 原生雷达图。当前正式 `render_snapshot.cpp` 仅在雷达 `bound:1` 时启用，所检活动 DAT 为 `bound:0`；原生 HUD 还属 P-17 例外。 |
| 仅备用 `INKHUD2.dat` | 2 | 活动索引 0 不选；P-17 例外。 |
| `data/menu.dat` | 14 | 菜单定义，战斗范围外。 |
| `data/system.dat` | 9 | 系统菜单/等待图引用；保留具体消费者检查。 |
| `data/minibar` 子表 | 4 | 正式可达头顶条的图，P-14 例外。 |

当前正式 `GameSession28` 从 `resource.dat` 解析战斗 WORDS0～WORDS5 与 SPARK 的明确索引；这七张图都在 Unity 暂存根，并已包含在上一份 1031/1031 SHA 对照中。缺失的 resource 独有 46 张不属于这七张已选战斗图。正式源码还在结果和选择流程解析其它 resource 索引，不能因为这七张已在位就把整份全局图表判为兼容。

这些分类说明当前没有**对象 DAT 直接引用的缺失 PNG**，也识别出正式可达但由用户例外覆盖的头顶条。它们不证明 `resource.dat`、`system.dat` 的全部 57 个不同路径都不可见，也不证明 Q09 的非例外画面已通过。逐路径 owner、当前 DAT 行号和动作边界见 `missing-sprite-owner-matrix.csv`；15 份 DAT 逐项见 `remaining-dat-consumer-classification.csv`。

正式源码核对点：`GameSession28` 在 `game_session.cpp` 约 1287～1304 行固定选活动 frame HUD index 0，约 1309～1327 行加载 minibar catalog，约 1448～1465 行可选加载 stage 父表，约 3324～3329 行把选中 minibar 交给 render snapshot；`render_snapshot.cpp` 约 1311～1319 行用雷达 `bound` 控制启用。以上只说明所检 playable 路径，不以旧版 B1E13 报告替代新版证据。

## 2026-10-01 全局 resource/system 的直接索引消费者补查

当前正式 `data/resource.dat` 的 `<bmp_begin>` 有 48 个顺序索引，生产暂存根其中 7 张存在、41 张缺失；另 `<frame>` 声明 7 张额外图片，暂存根均缺失。因此上述由 resource 拥有的正式独有路径合计为 48（resource 独有46、与排除 mode 子表共享2）。在所检 playable `GameSession28` 调用链中：

- 活跃战斗明确选用索引 16～21 的 WORDS0～5 和 43 的 SPARK（`game_session.cpp` 1217～1245、3316～3321）；这 7 张均在暂存根。现有 Editor 隔离发布也对暂存根 330 对象、906 选定图片及 SPARK 通过 1/1，见 [发布报告](../NTSD28-336B44-Q01-PRODUCTION-CANDIDATE-EDITOR-20261001/REPORT.md)。
- `game_session.cpp` 3354～3384 将索引 24～26、28～31 绑定到 `battle_flow_step_.phase == result_visible` 的原生 scoreboard snapshot，并在特定 story-result 条件下解析索引 3；这些图片在暂存根缺失。它们有正式可达的结果阶段消费者，不能把“不是活跃战斗帧”误写成“正式不可达”；用户当前只要求战斗场景逻辑，结果页/故事阶段的范围需按已批准边界裁决，不因此自动复制资源。
- 选择画面直接解析索引 0、32～38、41、42（`game_session.cpp` 3508～3525）；加载流程解析索引 47（约 367～424）。它们均不属于本轮活跃战斗图像验收。其余 resource 索引没有在上述直接读取点得到可达/不可达的完整证明，继续留待 Q09 非例外表现核验。
- `data/system.dat` 的 9 张缺失图在当前文件文本中分别属于 `<menu_back_1/2>` 的 `sprite/menu/1..8.png` 和 `<menu_wait>` 的 `sprite/loading/menu_wait.png`；这些是菜单/等待图 owner，不从文件名推断全运行时不可达。其字段型战斗配置仍由已暂存的 `system.dat` 读取，不能把图像例外扩大到该 DAT 的逻辑字段。

此补查把“缺失 57 张 resource/system 图”的活跃战斗直接消费者缩到上述已在位的 7 张，但只覆盖这些明确索引调用，不是全程序静态可达性证明。原生 scoreboard/故事结果、未列明的动态资源及用户表现例外仍按 Q08/Q09 边界核对；没有复制、删除或编辑 DAT/PNG。
