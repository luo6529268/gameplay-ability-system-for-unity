# NTSD28-336B44-Q09-GLOBAL-TICK14-GAMEVIEW-001

状态：`FOCUSED_TEST_PASS / TEST_ONLY`。归属当前 336B44 总表 Q09 的**同一例**鸣人 OID2／鼬 OID9 静置表现出口；替换前次 Unity 全局 tick19 的弱配对，不增加角色或场景矩阵。

正式根 EXE（SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`）已有真实 D3D11 客户区、无玩家输入、全局 tick14 的角色画面。原 Unity 探针等直接 Battle 自动运行到 tick5 才重置角色/RNG，然后采相对14 tick 至全局19；不能把那张图称为同全局 tick。现有 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-IDLE-TICK14-GAMEVIEW-001/REPORT.md)保留为历史限定证据。

仅修改 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs`：新增独立、短时有效且结果拒绝覆盖的请求路径/运行 ID；在 Play clone 的 `BattleTestBootstrap.Start` 前临时令 `autoResume=false`，等待正式内容和 OID2/OID9 roster 就绪且 Driver 全局 tick0；按原生产顺序调用 `BeginBattleAllocationSeal`、`BattleBootstrap.EnablePresentation` 和 `SetPaused(false)`，同一 Editor 回调立即暂停，再按已声明的源坐标/HP/MP/seed/正式 BGM RNG 初态提交 14 个零输入生产 `StepOneTick`。仅当截图、中央发布、像素计划、Driver 均为全局 tick14 且原 Scene 退出后 clean/SHA 稳定时，记为可比图像样本。旧 P-08、旧静置请求和结果路径保持不变；不改生产、DAT、图片、Scene、Prefab、ProjectSettings、非战斗。

风险：异步 Start 尚未完成、封印/表现顺序不完整、手动启用后意外自动 tick、MCP 指向别的项目、并行编辑使 Scene 变脏。运行前后检查原项目实例、编译状态、唯一干净 Battle Scene 与文件 SHA；任一条件不满足则不进 Play 或报告 FAIL。报告显式记录全局初末 tick、每 tick 角色字段与 RNG、发布/截图 tick、退出状态。若 source 初态或逻辑行不匹配正式根，不用截图修渲染。

验收：生成 Editor 工程编译 0 error；Change Ledger validator 通过；原 Editor MCP 刷新/导入后仅一次定向 Play；结果全局 `0→14`、14 行无输入、目标动作/HP/MP 与正式根标题一致、双方配置初态 X/Z 可核对、同 tick 合成 Game View、Scene clean/SHA 未变。正式 GUI 标题不显示 X/Z；完整逐 tick 坐标须另用正式 trace 裁决。实际两图只比较用户未排除的角色本体、阴影、挂点及战斗效果；不同背景、固定视野、普通 HUD/名字不作逐像素相等门。若已有可复现的非例外首差，另建共用 owner 最小修复；本 Task 不授权生产改动。回滚只对本次新增分支作精确前向编辑，保留旧请求/报告与并行工作；不运行 Git restore/reset/删除。

实际出口：[唯一有效重试及首轮异步预热失败原件](../../../artifacts/diagnostics/NTSD28-336B44-Q09-GLOBAL-TICK14-GAMEVIEW-001/REPORT.md)已留存。有效样本为全局0→14、14行零输入，末帧动作/HP/MP 与正式根暂停标题一致，发布/计划/截图 tick14；原 Battle Scene 退出后 clean/SHA 稳定。[正式根逐 tick 回放](../../../artifacts/diagnostics/NTSD28-336B44-Q09-GLOBAL-TICK14-ROOT-TRACE-001/REPORT.md)补证12字段×14tick中140/168同值、28项差异全部为两人受不同地图边界影响的Z；本测试出口不能称完整同位置。两图有双方本体与足下阴影，但不同背景/视口/HUD 及正式 World 明细缺口阻止整图逐像素或全 World 同态结论。只闭这一例测试出口，Q09/Q12/总目标维持条件触发开放。
