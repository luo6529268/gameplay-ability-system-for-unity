# NTSD28-336B44-Q07-C042-SELFCHECK-ORACLE-001

状态：`VERIFIED`（仅SelfCheck合成口径；C042父出口开放）。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；BATCH-04/Q07/C042；当前总表为 `Assets/NTSD/Docs/ntsd28-logan-336b44-vs-unity-battle-alignment.md`。

## 证据与范围

2026-10-02 原 Unity Editor 运行完整 `BattleRuntimeSelfCheck.RunAllChecksStatic()`，于 `CheckCpointThrowRawAndTransformMatrix` 的首个 `character raw throw mode=0` 停止。失败原件另存 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-01.txt`。旧断言认为投掷同时清抓取者与被投者动作计数。当前正式 336B44 `source/ntsd28_core/src/simulation/battle_world.cpp::BattleWorld28::advance_catch_relations` 的投掷分支只清抓取者；此前 C042 当前源码392例受控见证中，无选招196例被投者8→8、先选招196例8→0。Unity `BattleCpointWriter.ApplyThrow` 已移除被投者额外清零。此处测试设置计数5/6且没有先选招，正确结果应为0/6。

仅修改 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckCpointThrowRawAndTransformMatrix` 的该项断言和失败消息。不得改生产 writer、DAT、角色图、Scene、菜单或其它测试预期。脚本修改前先建同名 Change Record，并登记 Ledger、STATE、handoff；修改后生成工程编译、运行原 Editor 完整 SelfCheck，再运行 Change Ledger 校验及差异检查。完整 SelfCheck 若出现下一项失败，记录真实首差，按当前权威独立归因，不为使其通过而批量改测试。

预期副作用仅为旧版本断言按 336B44 重基线；投掷真实行为保持不变。回滚仅审阅并反向调整本任务的唯一断言差量，不使用整体 Git 恢复。Q07/C042/C043 的自然物理键、全 World 等父出口不会因本任务自动关闭。
