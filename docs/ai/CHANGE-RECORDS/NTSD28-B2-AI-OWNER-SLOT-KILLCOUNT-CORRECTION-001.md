# NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiSensingSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiSensingKernel.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleAiUnifiedRowPublisher.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingKernelEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionKernelEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionAuthorityChainContractEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiRenderPhaseConsumerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B2AiOwnerSlotRuntimeAcceptanceEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/ai_owner_guard_capture_main.cpp
authority: NTSD 2.8-Logan native_ai.cpp owner_slot >= 0 OID122/OID123 avoid guard and EntityState28 owner_slot; Unity AI SoA/unified/legacy KillCount binding; EXE B1E13AE1, closure 39DDDA15.
evidence: AI snapshot/SoA/unified/legacy paths sample OwnerSlotIndex and publisher no longer projects KillCount mutations. Current source-model and Unity traces compare equal for 8 records/48 field occurrences with no first difference; both producers are byte-stable across two runs. Unity focused 4/4 and targeted NTSD_Battle Play pass; B0 direct/OPoint/F8 owner values 0/7/99 are consumed. Both project builds have 0 errors; full SelfCheck remains blocked later by the unrelated pre-existing CPoint throw Vz assertion.
-->

> 状态：`VERIFIED / B0_OWNER_PRODUCER_PREREQUISITE_CLOSED / OWNER_GUARD_TRACE_EQUAL_8_RECORDS_48_FIELDS / UNITY_FOCUSED_4_OF_4 / TARGETED_PLAY_PASS / DOUBLE_RUN_BYTE_STABLE / BUILDS_0_ERROR / EXACT_OWNER_SLOT_BINDING / LEGACY_KILLCOUNT_DETACHED_FROM_AI / SELFCHECK_BLOCKED_UNRELATED`

Authority AI guard读取`owner_slot>=0`，Unity原先误读`KillCount>-1`。AI snapshot/SoA/unified/legacy binding已
改到`OwnerSlotIndex`并退休publisher KillCount lane；candidate、RNG、顺序及legacy field其他用途保持不变。

B0 producer prerequisite已由`NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001`关闭。当前Authority source-model
runner以source manifest `07CD47A...D778F`、runner `094EDE9...EDCD`、binary `08821924...8349`生成8条
OID122/123 owner guard记录；Authority输出双跑SHA均为`FCFEA3DB...497A`。Unity production snapshot/SoA输出在
focused与Play后SHA均为`63ED4B64...149B`，comparison为8 records / 48 fields / first difference空。

Unity focused于2026-09-09 01:39:06 +08通过`4/4`，并验证direct/OPoint/F8 producer owner=`0/7/99`；
01:37:20 +08目标`NTSD_Battle` Play通过同4用例，Console error=0、Scene dirty=false/root=13且SHA保持
`50FD4D8F...FF3C`。两个project build均0 error。01:40:28 +08 full SelfCheck仍在独立既有CPoint mode0
victim-Vz断言停止，故不声称full SelfCheck或完整AI/B2/full parity通过。

严格后继为`NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001`，随后才是11xx/12xx
child propagation与B4 revival绑定。
