<!-- CHANGE-RECORD
id: NTSD28-Q06-STANDARD-HIT-PENDING-Y-PROJECTION-001
status: VERIFIED
change-kind: STANDARD_HIT_PENDING_Y_PREDICTION_GUARD
code-path: Tools/NTSD28AuthorityTrace/standard_hit_fall80_preservation_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06StandardHitFall80PreservationEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
authority: Current playable hit_response.cpp accumulate_unarmored_vertical and Unity actual ApplyStandardVerticalKnockback nonzero-dvy gate.
evidence: Source3 pendingY10 and shadow mask1<<46 predicted12; existing fall80 task owns independent timer correction.
-->

# NTSD28-Q06-STANDARD-HIT-PENDING-Y-PROJECTION-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。独立于Fall80保持：真实candidate/ShadowCompare RED mask70368744177664=1<<46(TargetKnockbackVy)，source3/actual lethal/nonlethal pending17 + default(-7)=10，projection额外钳12。当前hit_response.cpp::accumulate_unarmored_vertical与writer.ApplyStandardVerticalKnockback仅dvy!=0时才检查int(YInt+pendingY)>0钳12；dvy0只减7。ProjectStandardCharacterDamageWriterEffect约4803无条件钳制是独立缺口，brokenArmor延后调用同一helper不得被删除。

第一写域：已有Tools/NTSD28AuthorityTrace/standard_hit_fall80_preservation_witness.cpp，新增--vertical代表输出模式，不改变默认3例任何输出bytes。新增两例nonzero dvy2：pending17/Y0→12；pending-1.5/Y0→0.5(int截断0不钳)。默认source3原SHA与新build输出须核对保持。复用真实source完整hit入口，不改权威源码。

源后精准声明Unity test/唯一projection条件：仅if增加resolvedItr.dvy!=0，保留累加/knockback/选帧/held-rest/brokenArmor与其他type3预测。source3复用默认分支证据，新2补非零/截断控制；有效Shadow RED与实际field对照，不改expected掩盖。实际writer不需要本包修改。回滚仅本ID条件与测试扩展，保留Fall80两clear和effect4访问点。

闭包出口相关代表、Shadow/DataOriented、SelfCheck与代表Play联合一次，不全量重复。sourcederived不当正式EXE录制，不改Scene/资源/schema/Kernel/非战斗。Q06未完/Q07未迁移。

准确测试扩域：在既有Fall80单fixture中抽取RunCapturedHitPlan(mode,sourceFile)，保留原source3两个mode入口，新增--vertical source2两个mode入口；比较actual HP/Fall/pendingY与source、Shadow差异mask/Failure0。DataOriented本来不提供ObservedWriterEffectCount，断言只在Shadow开启，原夹具误断言已保留修正。最新Fall80单纯source3直接矩阵job5d758a8fc9934e1aabc3395b95b73951 PASS，未关闭parent；vertical源buildsession59549中，尚不改projection条件。

源vertical2双跑一致SHA0df0dc5e1d3172ef5212e4a3f8b87645a735d2b6a076feb69bcd6e1f575e165f，非零结果12/0.5；新build默认3输出SHA仍cc6686…527b3，字节不变证据已存。生产前扩准确范围：仅ProjectStandardCharacterDamageWriterEffect的knockback分支钳制条件增加resolvedItr.dvy!=0；其余三元加法、int截断、大于0条件、brokenArmor、其他投影分支不动。既有default dvy0 Shadow RED mask1<<46与source10/预测12为依据；不再次运行旧全域矩阵，只联合3默认+2控制/父effect与相关旧回归。


2026-09-21 联合出口：job d41e706b23d048448436ee57d9f2f0c9 30/30 PASS，实际1.777秒；effect16+smoke6 before/即时/following零差异，标准source3及vertical2真实candidate Shadow/DataOriented通过。完整SelfCheck 01:55:20 UTC PASS已存joint-pass。Ledger603/28 PASS，diffcheck通过。仍IN_PROGRESS，尚待代表Play/回放。
测试扩域预声明（生产不再修改）：既有Fall80 fixture RunCapturedHitPlan改internal并增加可选renderer参数；CreateWorld/Shutdown使用该参数，并检查对象池borrower恢复。供Effect同文件Play probe复用default3+vertical2，在ShadowCompare/renderer和DataOriented/logic两条代表路径验证，共10例；保留原EditMode调用不变。无新逻辑/资源/Scene修改，不据此证明跨World恢复。

测试扩域已实现：Effect fixture新增Authority6 replay与EffectPlay22 probe；Fall80 RunCapturedHitPlan可选renderer并逐world断言borrowers恢复。当前仅刷新编译，新增replay/Play尚未通过；不重跑既已通过完整SelfCheck。

真实Play初次12个effect代表通过；captured helper在NUnit TestContext.WriteLine处因Play没有测试context抛NullReference（两条路径）。Scene checksum/borrowers2→2保持，关闭01:58:24Z PASS。失败已存effect/representative-play-initial-test-context-failure。精确修复同测试helper日志：只在非Play时调用TestContext.WriteLine，所有assert/生产逻辑保持。修后仅重跑此Play，不重做完整SelfCheck/旧矩阵。


2026-09-21 限定VERIFIED：Only standard projected pendingY clamp gains nonzero-dvy guard. Defaultdvy0 pending17→10; vertical2 dvy2 pending17→12 and pending-1.5→0.5, preserving truncation. Sourcevertical SHA0df0dc5e1d3172ef5212e4a3f8b87645a735d2b6a076feb69bcd6e1f575e165f,default3 output bytes unchanged. Actual writer/brokenArmor unchanged.
联合30/30、SelfCheck01:55:20Z、代表回放6/12ticks及Play22/关闭02:00:04Z PASS；Scene hash/dirtyfalse/root14保持。完整证据与限制见artifacts/diagnostics/NTSD28-Q06-STANDARD-HIT-PENDING-Y-PROJECTION-001/ACCEPTANCE.md。上文未运行/生产未改是历史检查点，由本条覆盖；不关闭整体Q06或Q07。
