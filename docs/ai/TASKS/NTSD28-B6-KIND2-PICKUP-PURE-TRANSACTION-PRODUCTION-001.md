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

验收结果：VERIFIED（限定P1规则/计数或P2纯计划子集）；证据见同ID Record。P3_NOT_CONNECTED / PLAY_NOT_PERFORMED_RULES_PURE_LAYER。
