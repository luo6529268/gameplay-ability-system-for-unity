# NTSD28-B6-KIND2-PICKUP-PURE-TRANSACTION-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-KIND2-PICKUP-PURE-TRANSACTION-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_PICKUP_PURE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattlePickupTransactionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupPureTransactionProductionEditorTests.cs
authority: 用户第2步；playable battle_world.cpp:5306-5411；physical owner5364-5376
evidence: FOCUSED_PASS / B6_443_OF_443 / REFILL_9_OF_9 / FRESH_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_SCHEMA_UNCHANGED / PLAY_NOT_PERFORMED_RULES_PURE_LAYER
-->

# NTSD28-B6-KIND2-PICKUP-PURE-TRANSACTION-PRODUCTION-001 — Task Contract

IN_PROGRESS / TEST_FIRST；用户第2步授权，P1 23/23已通过，P3未授权。
Authority：当前playable battle_world.cpp:5306-5411（实际kind2块5310-5404）；+35C/physical owner精确写在5364-5376。owner_slot=static_cast<int>(attacker_slot)，不是attacker.owner_slot、HolderCopy或linked root。纯input只携带attacker physical slot，不能读取Unity legacy mirror。

## 范围
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattlePickupTransactionPlan.cs及自动.meta，新纯input/operation/plan/resolver类型。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupPureTransactionProductionEditorTests.cs及.meta，新focused。
- 本Task/Record、Ledger/STATE/对齐总表、Temp证据。
P1 locked规则只读复用，无新规则配置、runtime consumer、world调用或schema。无Apply方法，不连接TryApplyPickup，P3才消费。

## 完整事务合同
明确Outcome Unsupported（规则/输入不支持、零写）、AppliedWithoutRelation（不支持的target type仍公共tail）、RelationEstablished（有关系且公共tail）。Applied属性覆盖后两者，不把tail-only写成false。
顺序：dead type6 cache清0（适用时）→旧relation0 count+1（适用时）→holder/child relation→holder child-slot→child parent-slot→child owner physical slot→child group→initial holder action→holder frame_counter0→target current WPoint非零literal覆盖holder action。
type1 locked120/124=101/-1，其他1/-1；type2=2/-2/action116；type4=4/-4；type6HP>0=6/-6否则4/-4并清weapon_hp_31c；其余target type不建关系仍tail/applied。普通initial action115。
WPoint零不覆盖，正/负/>=1000原样写；target frame缺失忽略残留WPoint输入。无old-child参数、无Free/unlink operation，不做清理旧child，不写catch字段、rest、delay、HP/MP或RNG。
P1 unaudited/空/alternate规则在用户locked准入层Unsupported且零写；它与Authority default target type的tail-only不同，必须分别测试，避免术语混淆。

## test-first与验收
先写typed tests，再补仅可编译的类型合同骨架（Create统一Unsupported，未实现事务），运行真实RED后才实现Create。记录骨架RED与控制组，不能将编译错误算行为RED。
RED：四类型/OID组合、dead6 HP-1/0/1、count0/非0、physical owner、WP0/333/-888/1000、unsupported type相同尾部、missing frame；有序operation列表对照、枚举不能表示Free/unlink。无状态/side effect的pure层，warmed4096次zero allocation。
P2 focused后共享B6(388+P1/P2新增)、refill9、fullSelfCheck、双build、validator、Scene固定SHA。不跑Play，状态PLAY_NOT_PERFORMED_RULES_PURE_LAYER；不宣称P3原子生产已接线。
任一既有指定测试失败、需要schema/old-child Free/unlink、diff超界/Scene变化即停，禁止为绿改旧SelfCheck。回滚须用户批准且仅本包新增增量。
无manager/queue/worker，plan栈上值数据，不接runtime关闭阶段；既有十一阶段不变。
P2 typed tests先写；随后新增可编译input/operation/plan合同骨架，Create统一返回Unsupported，尚未实现类型事务。GetOperation合同预定义有限写集合，不含Free/unlink；新32项focused将据此取得真实未实现RED。无P3接线。

P2 RED job3499eae8fd6b4d9aa245d81796fd26dc实际completed32，status failed，返回20条失败且failures_capped=true，result null；不把截断明细推断成实测29失败。证据Temp/Goal15_P2_RED_Result.json。随后实现Create：locked准入与physical slot检查、type1/2/4/6与default outcome、dead6清cache；GetOperation输出有序写，未接consumer。当前CODE_WRITTEN待GREEN。

## 2026-09-10 最终验收（限定子集）
- B6 job62bf9144983b4dbfbd5cd424f98dffc2：443/443 PASS，即既有388+P1 23+P2 32；完整results再次核实两个focused分别23/32且均Passed。
- held-refill jobac230b9e0de74570bfb45bd725047478：9/9 PASS。
- full SelfCheck：20:07:23 UTC请求，20:08:03 UTC新结果PASS；Temp/Goal15_Final_SelfCheck.result。Console本轮error/assert/exception读取7条既有negative-fixture Error，未见MinMaxAABB；不声明Console0，不关闭Goal14独立mesh顺序缺陷候选。
- 实际执行 dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：0 error/47 warnings；Assembly-CSharp-Editor.csproj同参数：0 error/104 warnings。Unity现有b1b02287/2022.3.62f3刷新后真实EditMode执行全部通过，没有第二Editor。
- Scene NTSD_Battle：dirty=false、rootCount13；SHA256 D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11；34个Snapshot/Checksum及meta对基线SHA全部相同（Temp/Goal15_Final_Integrity.json）。
- git diff --check PASS；未暂存、未commit/push。最终validator另追加实际结果。
- PLAY_NOT_PERFORMED_RULES_PURE_LAYER；用户明确免本包自然Play，P3仍需原子actual/HitPlan/legacy集成与scoped Play。没有新的双端同tick trace，不把纯计划测试宣称为完整pickup对齐。保留旧child且没有Free/unlink/schema改动。

P2最终VERIFIED仅pure plan子集；GREEN jobb1aa7dfda4a442eb98e42043cc8f8174 32/32，warmed4096次构建并枚举操作测得0分配。RED已记录截断边界；未接生产consumer。
最终 Tools/Validate-ChangeLedger.ps1 实测PASS：445 records、4 governed code diff covered。15个Git改动路径均在用户授权清单；历史Record不在本次diff的提示仅为warning。
