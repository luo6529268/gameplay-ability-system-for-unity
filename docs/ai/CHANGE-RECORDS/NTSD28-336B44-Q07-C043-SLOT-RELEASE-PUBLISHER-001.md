<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C043-SLOT-RELEASE-PUBLISHER-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs
authority: selected 336B44 natural OID420 end-of-life plus Unity unified-row occupancy contract
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C043-SLOT-RELEASE-PUBLISHER-001.md
-->

# C043 子体退场时统一 AI 行失效顺序

脚本前记录：原Scene tick42 同正式输入触发 OID420 自然退场；`ReleaseRuntimeSlot` 在成功释放槽位后，先用 `BattleEntityLinkLifecycleWriter.ClearReferencesToReleasedSlot` 改其它活实体关系字段，然后才调用 `AiUnifiedRowPublisher.InvalidateAfterOccupancyChange`。这段时间统一AI发布器仍持有旧行/代数，关系 setter 经 `BattleRelationLinkStore.CaptureChangedField` 发布时触发硬异常。已见正式源码/根 tick42继续正常；Unity异常及退出残留不得掩盖。

预期最小修改、路径、风险、聚焦与真实Play出口见 Task。只移动失效调用到关系清理之前，并加一项可复现链接实体退场的聚焦测试；不改业务规则、DAT、Scene、资源或非战斗。已有未提交内容和失败证据全保留；本记录在实际结果前保持 `IN_PROGRESS`。

首版聚焦测试将注销放在 AI producer hook 内，原 Editor精确job `48a62e3ad42741efa0edbafa9646ad15` 1/1 FAIL，但首先触发的是同tick AI snapshot epoch 硬门，不是Play tick42的关系发布错误；不能将该FAIL计作目标RED。现改成共用槽释放的精确载体：已提交AI行排除休眠持有者、被释放slot50仍被其Target引用；注销时关系清理必须在失效旧AI行后写。保留首轮job与旧代码差异记录。

目标 RED：原 Editor job `bc0280ce78204eb596b9cd4b160cdf5a` 精确1例FAIL，异常为 `Unified AI row publisher observed a stale slot generation after commit`，栈与自然Scene tick42同在关系 setter→旧行发布；生成 Editor 工程此前0错。生产脚本仅将现有 `InvalidateAfterOccupancyChange()` 从关系清理后移到前，保留成功释放槽位后的其余顺序。下一跑同一聚焦GREEN、相邻测试及自然Scene。

聚焦 GREEN：原 Editor精确job `6ba8ac2c8f5b47e39a7356a4a2abc59a` 1/1 succeeded；相邻占用epoch重建和解融合旧行两例job `66b82234cffb454aabd1a7ff455d304e` 2/2 succeeded。生成Editor工程0错误/281警告。只证明共用释放顺序与这三例；原Scene v3、退出借用、当前正式根60tick配对尚待。

最终限定验收：v3原Battle Scene60/60生产tick完成，tick42自然子体退场不再抛旧行代数异常；正式源17字段原值1015/1020，5差仅无链接空槽哨兵0/-1，限定归一后1020/1020；退出池借用0、Scene clean、四保护SHA逐一不变。生成Editor工程最终0错/250警告；原Editor重载后生产/Editor程序集均新于两脚本。没有改变其它文件或非战斗逻辑。完整SelfCheck、其它关系路径、全World与真实物理键不在此限定证书；C043/Q07/总目标仍开。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/REPORT.md)。

交付核验：Change Ledger validator exit0/PASSED，生产/聚焦测试两个路径由本ID覆盖；`git diff --check` exit0，仅行尾警告。未删除文件、未覆盖历史结果。
