# NTSD28-336B44-Q07-C056-FUSION-SELFCHECK-ORACLE-001

状态：`VERIFIED`（仅SelfCheck合成口径；C056父出口开放）；父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；新版 BATCH-04/Q07/C056。

原 Editor 第五轮完整 SelfCheck 越过当前336B44 effect22 断言后，停在 `CheckOid5152MergeSuccessAndDormantIsolation` 的旧合体锁存/计数断言；原件 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-05.txt`。该断言要求合体将 `Trans.WaitCounter`/`Frame.Prev2` 写成290且动作计数清0。当前正式 `BattleWorld28::advance_native_fusions` 调用 `set_action(290)` 后明确恢复合体前 `action_latch`、`tick_action_snapshot`、`frame_counter`，并在同 tick 后继 pass 依原计数决定OPoint出生。C056正式源码受控两条件与Unity原Battle Scene正反限定样本已证，但完整父出口未关。Unity `BattleOid5152RuntimeModule.TryMerge` 通过 `DirectWriteNativeRawFramePreserveWaitCounter(290)` 保留对应锁存/计数，重置声音锁存。

仅改 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckOid5152MergeSuccessAndDormantIsolation` 的合体前置和一项旧断言：明确设主角动作计数7、声音锁存10，保存合体前 `Trans.WaitCounter`、`Frame.Prev2/Prev2D`；合体后要求动作290而上述三个锁存/计数保留、声音锁存-1。保留HP/PP/位置/伙伴休眠/引用等其它断言。若后继首差不是这组，先记录实际值，不扩大改动。生产/DAT/Scene/非战斗不改，旧结果不删不覆盖。

脚本修改前建同名 Change Record、Ledger、STATE/handoff；修改后生成工程编译、原Editor刷新并运行完整SelfCheck，保留下一结果；验证Change Ledger、差异与四保护SHA。此包只证明合成SelfCheck口径，不自动关闭C056根自然入口、Q07或总目标。回滚仅审阅本测试精确差量，不用整体Git恢复。

2026-10-02 同规则范围增补（再次编辑脚本前）：第六轮完整SelfCheck已越过上述OID7主角测试，下一首差在 `CheckOid5152MirrorIdentityAndPresentation` 的OID8主角镜像合体断言，仍要求锁存290/计数0。只把同文件该方法的合体前置与这条旧断言纳入本ID：设非零计数5、保存原WaitCounter/Prev2/Prev2D，合体后要求保留；后续解融合和Renderer断言不改。失败原件 `selfcheck-result-06.txt` 已另存。
