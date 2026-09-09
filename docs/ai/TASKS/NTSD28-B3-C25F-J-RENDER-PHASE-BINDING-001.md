# Task Contract — NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001

> 状态：`VERIFIED / RENDER-PHASE-HITSTOP-BINDING / JOINT-RAW-EQUAL / NO-RUNTIME-BEHAVIOR-CHANGE`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25h prerequisite`
> 依赖：`NTSD28-B3-C25F-J-FIELD-OWNER-AUDIT-001 / VERIFIED`

## 目标

用Y与phase互异的非零source-model/Unity completed-tick raw trace，裁决Authority `render_phase_008`在Unity中是否唯一绑定`NTSDEntityRuntime.HitStop`，并纠正旧AI临时Y投影冲突；本包只改诊断合同/投影，不改runtime writer或表现行为。

## 允许路径

- `Tools/NTSD28Parity/EntityFieldContract.cs`及必要self-test/README。
- `Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp`及新增workspace scenario。
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs`及raw focused tests。
- 本Task/Record、Ledger、STATE、handoff、总表、C25f-j manifest，以及对旧B2文档的append-only correction。

## 不做

- 不新增第二个render phase runtime字段，不改变`HitStop`写入/递减/renderer/AI production consumer。
- 不实现C25h timer算法，不增加C25f-j其余carrier，不改content/Scene/Prefab/Authority。
- source-model raw仍是diagnostic only，不冒充formal EXE runtime certificate。

## 验收

- test-first先让48字段/39 verified/renderPhase=HitStop断言在旧47字段投影上失败。
- Authority source runner和Unity exporter对同一3tick、Y与phase互异scenario都输出`combat.renderPhase`。
- comparator逐tick/slot证明该字段equal，且Unity测试证明它不读取Y/Fall。
- parity tool build/self-tests、Unity compile/focused/NTSD28 broad/SelfCheck通过；Scene unchanged、非Play、Console0。

## 回滚

删除renderPhase字段、scenario和测试，恢复47/38合同；不得改回已纠正的current_mp或已验证C25 carriers。

## 结果与证据

- test-first job `54f5e3f642c24fa9a0aa57392400344c`：旧47/38投影使1/3按预期失败；失败JSON不含renderPhase。
- raw contract现为48字段/39 verified；Authority source runner直接读`render_phase_008`，Unity exporter直接读`Runtime.HitStop`。
- 共同scenario以slot0 Y0/phase5、slot1 Y-11/phase-5起步；3tick两端phase均为`4/-4, 3/-3, 2/-2`，证明不是Y或Fall代理。
- comparator：3 ticks、6 pairs、288 field occurrences；`combat.renderPhase`在31个unique equal fields中。其余17个既有差异照常报告，first difference仍为`frame.action`。
- source capture validator valid；Authority raw SHA`C1661264...00C8`、Unity raw SHA`31031CF9...5012`、report SHA`092A307C...ED29`。
- 同包发现并纠正Unity diagnostic header残留旧正式EXE SHA，现为`B1E13AE1...9033`；scenario legacy reference身份仍单独保留，没有混称formal certificate。
- tool build0/0、trace self-test21/21、raw self-test5/5；Unity focused final `1e6c394774864be9a2179633398e0ccf` 15/15，NTSD28 broad `3316c77333374a5ca4ef8d5619b99948` 406/406。
- 2026-09-05 08:53:55 SelfCheck PASS；Scene SHA`0D74E174...D77`不变、未进Play、post-clear Console0。
- 本包不修改runtime writer/AI consumer/renderer；仍读取Y的native render-phase AI分支与C25h timer owner留后续行为包。
