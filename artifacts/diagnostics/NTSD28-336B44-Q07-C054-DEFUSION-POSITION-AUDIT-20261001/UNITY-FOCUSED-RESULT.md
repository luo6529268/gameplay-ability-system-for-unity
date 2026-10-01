# C054 原项目聚焦验证（2026-10-01）

- 正式权威根 `NTSD2.8-Logan.exe` SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。
- 原项目 Unity Editor PID 11944 经既有 MCPForUnity 本地桥接刷新后完成脚本编译；`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 更新至 2026-09-30 23:29:51 UTC。
- 仅运行 `NTSD.Test.NTSD28Q06FusionRecordTransactionEditorTests.ControlledDefusion_RebuildsPartnerPrecisePositionFromEachIntegerDomain`。首轮 Test Runner 默认 15 秒初始化超时，测试未开始；第二轮 `initTimeout=120000`，job `e19472cf691540ac8ca88552e2872fd6`，结果 `Passed`，总数 1、通过 1、失败 0、跳过 0。
- Test Runner 临时清空活动场景后，已通过同一原 Editor 的 `manage_scene load` 重新载入 `Assets/NTSD/Scene/NTSD_Battle.unity`；磁盘 SHA-256 仍为 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`。
- 以正式 336B44 的 31 个 source-capture C++ 文件重新编译旧融合诊断，编译/运行均返回 0，4 行输出保存于 `Temp/NTSD28C054FormalWitness336B44-20261001/first.jsonl`。此诊断四组的主角色精确坐标与整数坐标原本相等，因此只证明当前版基础融合链可运行，**不证明小数解融合分支**。旧 Source4 初态也不得当作正式根小数样本。

当前状态 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。待正式版小数初态同态、原 Battle Scene 后继 tick 与退出验收；C054、Q07 和总目标保持开放。
