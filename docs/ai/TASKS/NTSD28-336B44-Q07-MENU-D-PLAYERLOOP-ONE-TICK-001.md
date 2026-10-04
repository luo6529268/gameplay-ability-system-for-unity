# NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001

状态：`VERIFIED_DIAGNOSTIC_ONLY`（仅单 tick 合成输入断点；Q07 父项开放）。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07`，并为 Q09 姓名牌自然位移验收提供输入前置。

当前 336B44 原 Editor 的 Menu→Battle 合成 D 单 tick 两次阴性只证 Editor 回调中立即更新设备状态的测试链无效，不能判玩家键盘或生产输入故障。仓库既有 Hidan 原 Battle Scene 探针曾以 `QueueStateEvent`→`QueuePlayerLoopUpdate`→等待 `InputSystem.onAfterUpdate` 的 Dynamic 更新→完整 `StepOneTick` 成功取得离散按键。此包仅在现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs` 增加独立 opt-in 的 PlayerLoop 单 tick Menu→Battle D 诊断，沿用同一现成注入相位；不得修改生产输入、场景、Input Actions、DAT、图片或非战斗功能。

脚本编辑前需确认原 Editor 只有一个干净已保存的 Menu Scene、非 Play/非编译，记录四保护资产哈希与目标脚本哈希。新增入口必须写唯一结果文件，最多执行一个完整 Driver tick，并在 tick 前记录键盘 D、MoveAction 值/控制与 Dynamic 更新次数，tick 后记录 canonical 按钮和位置；阴性也要退出并保留原件，不重试长跑。释放键盘、还原暂停状态、经现有 Menu→Battle 清理链有序退出，并核对 Menu/Battle/两配置哈希及无残留。不能把合成输入成功冒称真实人手键，或把失败直接认作生产故障。

验收：生成 Editor 编译与原 Editor 导入 0 error；单次 Play 的事件相位及一 tick 报告可判，若 D→Action→canonical Right 有效再与当前 336B44 正式移动规则对照；否则记录首个断点并停止此注入路线。`Tools/Validate-ChangeLedger.ps1` 与 scoped diff 检查通过。回滚只回滚新增 opt-in 诊断行；保护已有脚本脏工作和全部失败证据。

2026-10-04：独立PlayerLoop诊断已写入现有Editor探针，生成Editor项目编译0 error/297 warning；原Editor刷新/单次Play尚待。此前`PLANNED`行是脚本前快照。

2026-10-04 出口：原Editor一次 Play 的 PlayerLoop Dynamic 序号1978→1979，键盘设备D=true、绑定同 device ID1，但启用的 MoveAction 与 canonical Right 均0；生产 Driver 恰走一 tick，源/视图X不动。已退出回干净Menu，四保护SHA稳。[原始报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-MENU-D-PLAYERLOOP-ONE-TICK-001/REPORT.md)。按本Task阴性出口停止这条合成输入路线，不判生产故障。
