<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C056-FUSION-SELFCHECK-ORACLE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal 336B44 playable BattleWorld28::advance_native_fusions preserves entry action latch, tick snapshot and frame counter
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C056-FUSION-SELFCHECK-ORACLE-001.md
-->

# Q07/C056 SelfCheck 合体锁存与计数版本修正

脚本前建立。原Editor完整SelfCheck第五轮在 `CheckOid5152MergeSuccessAndDormantIsolation` 的旧“锁存290/计数0”断言失败，原始结果已另存 `selfcheck-result-05.txt`。当前336B44正式 `BattleWorld28::advance_native_fusions` 对OID7/8→51先写动作290，再恢复合体前action_latch/tick_action_snapshot/frame_counter；Unity当前共用 `TryMerge` 已采用保留WaitCounter的帧写入口。原测试仍按旧B1E13合体后提交290的预期，不能作为新版裁决。

声明代码路径仅 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckOid5152MergeSuccessAndDormantIsolation`。计划设置非零动作计数7和声音锁存10作为有辨识度的前置，保存合体前锁存值；合体后检查动作290、动作计数7、原有WaitCounter/Prev2/Prev2D保持，声音锁存-1。其余HP、PP、位置、伙伴休眠与引用测试不变。预期副作用仅测试口径，C056正式根自然入口与完整关闭仍需独立验。四保护文件、DAT、Scene和非战斗功能保持。

验收：生成工程0 error；原Editor完整SelfCheck实际越过此方法或记录下一首差；Ledger validator、diff check与四保护SHA。结果Temp若被菜单覆盖，旧结果先另存到具名原件；无删除。回滚只审阅本测试精确差量。

2026-10-02 代码已写：在既有合成融合正例中明确设置主角动作计数7、声音锁存10，并保存合体前 WaitCounter/Prev2/Prev2D；旧“锁存290/计数0”断言改为保留三个原值及计数7、声音锁存-1，失败消息包含观测值。其余测试与生产/DAT/Scene/非战斗未改。当前`CODE_WRITTEN`，编译和Editor复验待执行。

2026-10-02 首轮复验与精确扩域：生成Editor工程0 error/281 warning，原Editor刷新后第六轮完整SelfCheck已越过上述OID7主角方法，下一首差是同规则OID8镜像主角 `CheckOid5152MirrorIdentityAndPresentation` 的旧“锁存290/计数0”断言，原件另存`selfcheck-result-06.txt`。再改脚本前本ID扩域到同文件该方法的合体前置与这一条断言：设计数5、保存WaitCounter/Prev2/Prev2D并检查保留，保持合体Renderer可见性和后继解融合断言原样。Temp第五轮原件已先另存，菜单覆盖的是本轮生成物；MCP30秒调用超时不等于测试终态，磁盘结果明确FAIL。

镜像方法代码已写：合体前主角计数5、保存WaitCounter/Prev2/Prev2D；合体后要求同三值保留，原OID/Renderer/解融合断言不动。无生产、DAT、Scene、非战斗改动；下一编译与原Editor复验。

2026-10-02 限定验收：第七轮完整SelfCheck已越过OID7及OID8两条合体锁存/计数断言，随后停在独立C054解融合小数旧断言，原件`selfcheck-result-07.txt`保留；C054独立口径更正后第八轮完整SelfCheck磁盘结果`PASS`（SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`）。生成Editor工程0 error/281 warning，原Editor idle/nonPlay/noncompiling，四保护SHA稳；本测试口径`VERIFIED`，C056正式根自然条件及父项/Q07仍开放。
