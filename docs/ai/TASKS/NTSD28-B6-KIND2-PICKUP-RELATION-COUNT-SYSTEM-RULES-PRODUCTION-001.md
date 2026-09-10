# NTSD28-B6-KIND2-PICKUP-RELATION-COUNT-SYSTEM-RULES-PRODUCTION-001 — Task Contract

IN_PROGRESS / TEST_FIRST，脚本修改前建立。用户本Goal第1步授权；D-022 locked规则政策。

## Authority与绑定证明（事前闭合）
当前正式B1E13AE1 EXE对应playable battle_world.cpp:5310-5404；build.ps1:74包含此live源码。
1. battle_world.h:216声明int interaction_relation_count_35c=0；live world.cpp:5364-5368明确+35C helper仅旧interaction_state==0递增。该成员全core production仅此writer，render_snapshot.cpp:1248将其投影为row.armed；实体新建默认0，不因普通输入reset清零。
2. Unity NTSDEntityRuntime.cs:125 int PickupCount，full reset1062为0、canonical copy824保留、input reset不清；BattleInteractionWriter:229在同一kind2关系事务递增，LF2SpecialAttack:545为复制writer，HitPlan投影AttackerPickupCount对应同一操作；没有第二个独立gameplay计数用途。
3. 现ECS fingerprint1105、checksum541、parity1057及entity snapshot canonical-copy均已携带该int。它们仅是既有数据流证据，本包不修改这些文件。
4. Authority测试battle_world_tests.cpp:1899-1919给出旧relation4/count5成功拾取后count仍5；普通0→related只加1。当前Unity无条件++是writer错误，而不是需要新建字段的证据。旧kind7额外writer也不改变该槽的计数身份，kind7退休留P3。
结论：PickupCount复用为+35C逻辑槽位。此为类型/default/reset/完整读写域/同事务数据流映射证明，不声称C#内存偏移等于原生+35C，也不声称已运行新的C++ trace。

## 精确写入范围
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleInteractionWriter.cs：TryApplyPickup仅kind2 type1 relation选择和旧relation条件count；具名immutable BattleLockedPickupWeaponThrowRules同文件，无新增配置/持久化字段。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupRelationCountSystemRulesProductionEditorTests.cs及.meta，新focused。
- 本Task/Record、Ledger、STATE、对齐总表；第0步DECISIONS文档；Temp证据。
规则公开Locked值，不暴露可变数组；测试用准入工厂只承认audited且精确{120,124}。空表、unaudited及其它集合unsupported，属于用户locked准入限制。生产3参数入口固定Locked；受限overload用于无副作用准入测试，不能接alternate配置。

## 不变量与阶段边界
type1 OID120/124 holder101而child仍-1；type4 OID124仍4/-4；count仅旧relation0递增，递增发生在关系改写前。kind7旧行为不在本包退休，已由现有旧relation0门保持。
不写target OwnerSlot、不加WPoint公共尾部、不改变old-child cleanup、consumer前门或HitPlan；这些属于P2纯计划/P3集成。无Free/unlink；无schema、Scene、DAT、RNG、input timing、cache新owner；纯rules无独立关闭阶段。

## RED/验收/停止/回滚
test-first：OID120/124/其他（含current DAT与CLR OID冲突）、type4不提升；old relation0/1/2/4/6/101/负sentinel计数不误增；audited空/unaudited/alternate零写，正常恰+1；snapshot/copy现有合同只读证明。
P1 focused后P2纯事务；最后共享B6(388+新增)、两focused、refill9、fullSelfCheck、双build0error、validator、SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。指定b1b02287/2022.3.62f3/NTSD_Battle，不第二实例。
任何绑定证据冲突、需要schema或Free/unlink旧child、任何既有指定测试失败、清单外diff/Scene变化立即停。不得为了绿修改P3期望或旧SelfCheck。PLAY_NOT_PERFORMED_RULES_PURE_LAYER，P3再做原子与Play。
回滚仅在用户明确批准后反向本包差异，不回退已提交Goal1-14。相关reserved/schema退出按D-022后续窗口，不在此Goal实施。

验收结果：VERIFIED（限定P1规则/计数或P2纯计划子集）；证据见同ID Record。P3_NOT_CONNECTED / PLAY_NOT_PERFORMED_RULES_PURE_LAYER。
