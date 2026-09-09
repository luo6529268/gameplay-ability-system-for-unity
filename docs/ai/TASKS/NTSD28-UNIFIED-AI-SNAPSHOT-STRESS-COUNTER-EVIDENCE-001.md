# NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001 — Task Contract

> Goal5 Part B，2026-09-09，事前建立，VERIFIED / EVIDENCE_CAPTURE_ONLY / SINGLE_RUN_FAILED_REFRESH_1_VS_2 / ASSERTIONS_UNCHANGED / GOAL6_USER_HOLD。

## 授权与精确改动

仅在ProductionEntityStressEditorTests.UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport
的报告映射完成后、原计数断言之前增加一次File.WriteAllText，输出
Temp/Goal5_UnifiedAiSnapshot_Counters.json。
只读取report与world的BuildCount/InitialCaptureCount/CommittedPassCount/RefreshCount/ReadCount/
SlotVisitCount及mode、InputPhase、force-rebuild/post-refresh、failure/roll-forward等诊断值。
使用现有Newtonsoft.Json引用序列化局部匿名对象；不声明新全局状态，不改任何计数、输入、
World、RNG、配置、执行顺序、期望、assert或finally cleanup。

精确文件：Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
（只在该测试内插桩）、本Task/Record、CHANGE-LEDGER、STATE、对齐总表和Temp结果。
不改production、其他测试、既有期望、Unity MCP工具或配置。

## 一次性验收

按指定instance核验2022.3.62f3/NTSD_Battle、EditMode idle。只通过testNames运行该具名测试一次，
不重跑、不清Console、不进Play。保留插桩与结果，记录首次失败断言行及期望/实际。
MCP若仍result=null，使用该次dump与未改的断言顺序定位首失败条件，明确这是源码顺序判读；
不可伪造异常栈或未取得的运行值。
若通过则如实记录，不刷绿。若dump改变测试行为或出现新无关Console error，立即停止。
根据实际证据三选一分类；任何期望/production修正留后续独立Goal。

插桩不代表原测试已通过，状态必须区分取证成功与测试FAIL；未取到JSON不写VERIFIED。
与PartA共享两套Assembly build/validator/Scene检查，但代码所有权完全独立。
回滚须用户批准，只移除本包dump和治理增量，原测试所有代码原样保留。


## 最终结果

Only pre-assert dump added; removing it restores every original byte. Single job8ee44f549ed046dca3e2277c40ac97ad failed1/2; report/world counters1/1/1/2/1/1050. First unchanged failing assertion is RefreshCount==1 at line3760, determined from same-run JSON plus source order. Stale fixture expectation; evidence-only VERIFIED, original test still FAIL.
完整证据与实际命令见同ID Record；停止等待Goal6。
