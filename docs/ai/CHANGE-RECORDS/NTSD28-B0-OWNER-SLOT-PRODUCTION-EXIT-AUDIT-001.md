# NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_AND_EDITOR_TEST
code-path: Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1
code-path: Tools/NTSD28AuthorityTrace/owner_slot_exit_capture_main.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0OwnerSlotProductionExitEditorTests.cs
authority: NTSD 2.8-Logan playable direct/story self initializer, ObjectSpawnPlanner28 owner propagation, GameSession28 F8 owner99 tail, BattleWorld28 state9996/type3/despawn-spawn paths; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority runner built against current source subclosure and produced a byte-stable 15-record trace twice. Unity focused and targeted Play each produced the matching 15-record/135-field owner trace with no first difference; route1-4 regression is 32/32, builds are 0 error, target Play console is clean and Scene SHA unchanged. Full SelfCheck was run but remains blocked earlier by the unrelated existing CPoint raw throw mode0 Vz assertion. No production runtime behavior changed; B0 owner producer exit is ready and B2 runtime may resume.
-->

> 状态：`VERIFIED / B0_OWNER_PRODUCER_ROUTE_5 / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS / DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / B0_OWNER_PRODUCER_EXIT_READY / B2_RUNTIME_RESUMABLE / PRODUCTION_UNCHANGED`

完整范围、Authority、不变量、验收与回滚见
`docs/ai/TASKS/NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001.md`。

## 当前实施

- 已确认既有通用raw capture只支持固定slots 0/1与三tick，无法覆盖动态OPoint、state9996、F8及slot reuse。
- 已冻结专项trace只比较owner域；F8内容/数量/位置、state9996运动与全战斗parity均不借本包扩大结论。
- 启动时未修改生产runtime、content或Scene；随后只新增诊断runner/focused+Play test并扩展build helper可选参数，
  战斗生产runtime仍未改。

## 最终证据

- helper可选runner build与默认runner regression build均成功；正式EXE identity guard通过。
- Authority trace：15 records，两次字节相等，SHA
  `577313C83290597E9C293194EE2A9CCE714465B46B6C31A70F8916E7AD2E1DCC`。
- Unity trace：15 records/135 fields，两次字节相等，SHA
  `C2DCF420395A10DDAAE83AA128C4CF7E896138245A109CBFA0E153880BC3492B`；comparison为
  `equal-owner-trace`、first difference空。
- 覆盖direct self 0/1、ordinary root/两跳child、owner7/target0去混淆、F8 first spawn owner99、
  state9996 slots50..54 owner-1、type3 mutation与slot50 generation1→2/owner7→13。
- Unity focused `1/1`；route1～4回归`32/32`；进程外build runtime/editor均0 error；01:06:52目标
  Play通过、Console0、Scene SHA/dirty/root不变。
- full SelfCheck 00:55:48仍提前失败于既有CPoint throw-Vz；已如实保留，不用该阻塞否定专项运行时证据，
  也不把专项trace扩大为全战斗parity。
