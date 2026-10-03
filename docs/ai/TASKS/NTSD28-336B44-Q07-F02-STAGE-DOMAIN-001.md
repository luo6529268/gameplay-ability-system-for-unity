# NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001

状态：`RUNTIME_PENDING`。Q07/F02 的正式 kind10 正例以三实体 Z542 初始化，但 Unity 原 Battle Scene 使用自己的地图，首轮在 tick1 把这个位置限到源 Z481/482。必须先用双方均有效的 Z 重新建立正式对照，不能改 Unity 地图或把首轮差异当作生产首差。

范围只限 `Tools/NTSD28Q07Diagnostics/f02_pickup_throw_entry_probe.cpp` 的可选初始 Z 参数及本 Task/Change/Ledger/STATE/handoff/总表和新诊断结果；现有默认 Z542、near/far/kind10 调用保持兼容。先以 Z400、原 X530、mode0、seed `0x28A55A5A`、相同条件攻击输入跑正式 playable 源与 336B44 根 EXE 的 LFR，保留 source/root 两份结果及首差。再与原 Unity Battle Scene 同 Z400 完整 Driver 逐 tick 比较。若 Z400 不在 Unity 实际可行走区域或正式版未进入 F02，则报告真实限制并另选双方有效 Z，不修改内容数值或生产代码。

实测修订：正式背景23在 tick1 把三实体 Z400 限回约 Z542，因此该场景也不在双方共同纵深域；其根回放 PASS 仅证明同正式背景行为，不能与 Unity Z400 直接配对。正式背景1的 DAT `zboundary: 375 575` 包含 Z400，且这是正式诊断会话的背景选择，不部署或使用原版背景于 Unity。脚本同范围增加可选背景 ID，仅允许测试用 ID1/默认23；原默认路径保持不变。以背景1/Z400 重跑正式源和根，再核对原 Unity 地图的可行走结果。

接受标准：新工具编译 0 错，原默认 Z542 正例回归关键 tick 与旧报告一致，新 Z 的正式源/根回放 PASS 且报告相应触发是否发生；只在同条件且初始身份/动作/位置/输入可比时讨论 Unity 首差。脚本修改前记录状态；失败保留原件。回滚采用后续更正/supersede，不删除旧原始结果或用户文件。

2026-10-03 阶段结果：正式背景23/Z400会在tick1限回Z542；正式背景1/Z400的源码与根正例3096/3096选定字段同态、tick39 F02；旧默认回归CSV同SHA。原Unity Z400第二轮在Play启动阶段超时，尚无同条件tick样本；第三轮待完成，故此Task和F02仍开放。[证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/REPORT.md)。
