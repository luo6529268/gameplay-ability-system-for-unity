# NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001

<!-- CHANGE-RECORD
id: NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001
status: VERIFIED
change-kind: TEST_ONLY_DIAGNOSTIC_EVIDENCE
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
authority: 用户2026-09-09 Goal5 PartB明确授权仅在具名stress测试断言前增加一次性六计数/phase dump并单跑一次；不改期望或production。当前NTSD2.8-Logan battle authority不变。
evidence: Only pre-assert dump added; removing it restores every original byte. Single job8ee44f549ed046dca3e2277c40ac97ad failed1/2; report/world counters1/1/1/2/1/1050. First unchanged failing assertion is RefreshCount==1 at line3760, determined from same-run JSON plus source order. Stale fixture expectation; evidence-only VERIFIED, original test still FAIL.
-->

完整边界与一次性运行限制见[Task](../TASKS/NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001.md)。
本Record仅覆盖具名测试内dump，不接管整个stress文件或既有AI运行时实现。
无全局状态、World/RNG/计数写入，也不创建runtime服务或修改生命周期。

## 最终取证

一次testNames具名EditMode运行，job8ee44f549ed046dca3e2277c40ac97ad，FAIL，未重跑。
Temp/Goal5_UnifiedAiSnapshot_Counters.json于2026-09-09T12:42:31.4494903Z写出。
BuildCount=1，InitialCaptureCount=1，CommittedPassCount=1，RefreshCount=2，ReadCount=1，SlotVisitCount=1050；WorldCounters与report完全一致。
Phase：InputCallTick2/WorldTick0/InputPhase0，UnifiedAuthority/IndexedCanonical，ForceFullRebuild=false，ForceFullPostRefresh=false，RollForwardCount0，PreCommitFailureCount0，PostCommitHardBreachCount0，FirstFailureStage=None，ObjectCount1/ClaimedSlotCount1/CharacterSlot0。
MCP仍返回Expected1/Actual2且result=null，保存于Temp/Goal5_UnifiedAiSnapshot_Result.json。
同次dump与未改断言顺序证明首失败为ProductionEntityStressEditorTests.cs:3760 RefreshCount==1（实际2）。不是MCP异常栈返回的行号。
此前Build1、SlotVisit1050（BattleRuntimeProfile.cs:137常量）、Initial1、Committed1均匹配。Read1来自dump，其原断言因前一失败未执行。
三选一结论：夹具期望需修正，仅此单AI双阶段夹具的RefreshCount应1→2，不改其他==1。
依据SimulationWorld.cs:2869/2875/2974–2979在AI producer后刷新，2932–2936在routing后再次刷新；统一writer位于SimulationAiDecisionModule.cs:1642起，committed gate满足，slot0不走1693–1700高slot skip，正常refresh在1813累计。
没有双build/capture证据，Build/Initial/Committed均1，failure/roll-forward均0。
Console前后相同5条既有日志，不清理，无新error。原所有断言及cleanup字节保留。
源码SHA：E40B5799A78BEACF975D8ED37040C1D8EFCF5EA67DC89B25334B09EDDD117F00。
后续独立test-only包可修该RefreshCount期望并验证两阶段语义，本轮未实施。

## 公共验证与边界

最终执行 `Tools/Validate-ChangeLedger.ps1`：PASS，434 Records / 374 governed code files / 531 warnings。warnings未在本包扩大处理。

dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，47 warnings/0 errors，00:00:06.92。
dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，104 warnings/0 errors，00:00:06.26。
Unity按指定instance核验2022.3.62f3/NTSD_Battle，未进入Play。Scene dirty=false/root13，SHA保持D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
VERIFIED分别仅关闭期望修正/取证，不代表完整kind10、full SelfCheck或原stress测试通过。production不变，完成后停止等待Goal6。

当前状态：VERIFIED / EVIDENCE_CAPTURE_ONLY / SINGLE_RUN_FAILED_REFRESH_1_VS_2 / ASSERTIONS_UNCHANGED / GOAL6_USER_HOLD
