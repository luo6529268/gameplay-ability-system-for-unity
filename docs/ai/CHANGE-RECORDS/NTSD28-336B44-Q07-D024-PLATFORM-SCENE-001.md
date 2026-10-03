<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07D024PlatformBattlePlayProbeEditor.cs
authority: 336B44 playable full tick plus current root controlled OID56 frame182 LFR and user D-024 ratio
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001.md
-->

# NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001

脚本前记录：完整范围、权威、已证受控初态、现有原 Scene 单 tick 原状、测试写入路径、预期副作用、不能自动晋升的边界、验收与回滚均在 [Task](../TASKS/NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001.md)。只新增 Editor-only opt-in 测试载体；现有生产、资源、场景、菜单及用户脏文件不可回退或覆盖。首轮真实 Play 结果未取得，状态 `IN_PROGRESS`。

2026-10-03 首轮原Scene `platform-182-20261003-01.json` 已保留为 `FAIL_TEST_ASSERTION`：实际11行源规则位置、链接与正式受控源一致，tick1～10记录 `targetPlatformSlot=1`（已把诊断槽50规范化到正式槽1），目标X205→178，画面比例残差最大约2.27e−13px；清理对象/槽/池借用回基线、Battle Scene SHA稳定。测试末尾断言错误地将规范化值1与原诊断槽50比较，故报“link did not persist”。这不是已证生产首差。下次仅把该断言与既有规范化格式一致，再以新runId复验；不改变运行时、DAT、Scene或失败原件。
2026-10-03 `VERIFIED / CONTROLLED_SCENE_ONLY`：仅把 test-only 断言从原测试槽50改为已声明的规范化槽1。修后生成Editor编译0错/276警告，原Editor程序集晚于脚本且真实Play `platform-182-20261003-02.json` 为 PASS；11样本对正式源码九字段99/99、首差无，目标源X205→178，物理X最大投影残差2.2737367544323206e−13像素。对象4→4、槽2→2、池借用2→2、fixture解除注册，Battle SHA不变；随后恢复原Menu clean、非Play，两Scene磁盘SHA稳定。首轮FAIL原件保留。`Tools/Validate-ChangeLedger.ps1` PASS（1203 Records，既有缺diff路径产生WARNING）；生成工程与原Editor编译均0错。只新增本Editor探针及meta，未改生产/DAT/Scene/非战斗。未跑全量案例，因本包仅验该十tick出口；自然输入、Game View/正式GPU及Q07父项未验。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001/REPORT.md)。
