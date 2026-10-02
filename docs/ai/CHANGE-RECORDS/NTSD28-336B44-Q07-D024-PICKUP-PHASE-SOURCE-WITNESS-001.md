<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor.cs
authority: 336B44 formal pickup input phase and user D-024 full-view ratio requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001.md
-->

# Q07/D-024 原场景拾取前置门诊断

脚本修改前建立。原状、改动边界、风险、验收与回滚见同 ID Task。初态相位错位仅为可检验假设；源位置未初始化是当前探针的已观察事实。仅给原 Battle Scene 请求式探针新增显式诊断参数，默认旧请求不受影响。完成后填写实际脚本/符号、编译、聚焦 Play、正式对照、保护 SHA、未验项及状态；不得以探针成功宣称 Q07 全部对齐。

实际修改只在 `NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor.Request/Report` 加两个显式选项及相位观测字段，`NEUTRAL` 出生前可选将世界输入相位配为0，`CreateGroundWeapon` 可选初始化源规则位置；默认两选项false，旧请求路径不变。改前无法在非像素分支建立双域武器出生，首tick相位错拍；改后只调整受控诊断初态，不写生产规则。风险是探针可选初态与普通场景自然初态不同，因此报告限定同初态。

生成Editor工程编译退出0/0 error；原Editor刷新与原Battle Scene19完整tick Play完成，物理键链`PASS_SCOPED_PHYSICAL_CHAIN`、退出clean，六保护SHA相同。正式当前源码共同前7tick原值75/77，首tick两格仅空关系哨兵0/-1；18持有tick比例最大误差X0.536384/Z约0像素。根EXE同条件、canonical非武器、全画面和其它挂点未验；WPOINT父包与Q07仍开放。[完整证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001/REPORT.md)。

交付检查：`Tools/Validate-ChangeLedger.ps1`退出0并报告`Change ledger validation PASSED`（1146 Records，当前工作区23个代码diff均有Record）；`git diff --check`退出0，未报空白错误。工作区其他历史未提交修改和删除项未处理，本包未删除文件。
