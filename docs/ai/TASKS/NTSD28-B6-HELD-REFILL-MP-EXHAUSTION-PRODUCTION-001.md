# Task Contract — NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001

## 2026-09-09 Goal 2 限定恢复授权（优先于下方历史实施范围）

用户只恢复本包专项Play验收；production实现直接沿用已确认成果，禁止重写。
本轮精确写入路径为本Task/Change Record、新增
`Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldRefillMpExhaustionPlayModeProbeEditor.cs`及其`.meta`、
`docs/ai/CHANGE-LEDGER.md`、`docs/ai/STATE.md`、
`Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`和`Temp/`结果产物。
旧范围内的resolver、SelfCheck、其他既有测试、handoff此次不授权修改。

探针事前设计：在目标NTSD_Battle生产World中创建仅本探针拥有的临时角色/饮料逻辑实体，
使用正式catalog的角色与OID122/123 DAT，不修改wrapper或数据；通过现有kind2拾取入口建立关系，
进入真实state17饮用帧，逐次使用生产driver完整tick推进至exhaustion。
饮料测试子类只在`base.Act()`前后采样并断言，不替换resolver或physics；每条采样记录入场/出场
HP/PP、exact gate与KillCount反向sentinel、RNG state/call delta、motion、action/counter/relation/
weapon flight。覆盖奶连续refill、果汁连续refill、零/负HP入场、gate负/零/正与150边界。
RNG和motion断言在实际held消费边界检查，避免将tick其他pass的合法变化归因于exhaustion。
既有C09 placement证据继续用于确认共享writer的两次pass接线。

运行前固定instance `gameplay-ability-system-for-unity@b1b02287`并核验Unity2022.3.62f3与场景。
driver暂停后等待dedicated worker idle；探针只回收自身entity，退出Play走现有有序关闭。
探针不创建Mono runtime服务或常驻队列；临时Editor观察回调在完成/失败/Play退出时解绑。
生命周期归属：仅Play中的测试对象，停止新probe调用后先unregister自身实体，生产World/pool保持有效；
禁止在Stopping/Stopped后新建服务。清理需验证自身实体已解绑、slot回收、对象/slot计数归还。

验收：新鲜Play结果逐条通过，退出Play后Console无error，Scene SHA必须为
`D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`；
两套Assembly build 0 error与Ledger validator PASS。full SelfCheck若仍停在独立GT-06则如实
记录`SELFCHECK_BLOCKED_UNRELATED_GT06`，不修夹具。生产修改需求、无关Console新error、Scene
hash无法恢复或diff越界均立即停止。所有证据齐全才VERIFIED，随后停止等待用户复核。
回滚仅在用户批准后逆向本轮新增探针和治理增量，禁止使用下方历史production回滚方案。

### Play夹具入口更正

首轮ground-kind2预设在任何refill调用前失败：冻结OID122/123的ground动作分别state9997/3005，
不是ITR kind2可拾取地面帧。正式normalized projection证明Sakura OID1 action241的OPoint kind2
生成OID122/action31，Naruto OID2 action291生成OID123/action20。
本探针因此改为当前对应角色DAT与`AttachOpointHeldObject`（正式factory PostInitLiving使用的
OPoint kind2关系绑定入口），不伪造ground state、不修改DAT、不改生产入口。
测试观察子类仍由探针注册，只验证refill消费者与完整tick，不据此认证完整OPoint materializer。

### 完整tick的环境对象归属见证

v2五组消费通过后发现全场多1对象/slot；本轮不会清理未知对象或修改随机掉落。
新增测试内structural allocate观察器，只把调用栈明确包含
`BattleRandomWeaponDropModule.RunNormalDrop`的新对象登记为环境随机掉落；不改RNG或任何生产结果。
清理验收仍要求探针自身角色/饮料完全解绑、slot无自身occupant；全场计数只允许增加已记录且仍存活的
上述环境对象，未知增量仍失败。开始时已有structural sink则拒绝运行；finally解绑测试sink。
结果逐对象保存slot/OID/source，退出Play后仍须Console0与指定Scene SHA，不能用环境分类掩盖残留。

> 当前状态：`VERIFIED / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / NTSD28_CATEGORY_229_OF_229 / B5_NAME_GROUP_759_OF_759 / TARGETED_PLAY_6_OF_6 / REFILL_SAMPLES_26 / EXHAUSTION_EVENTS_6 / BUILDS_0_ERROR / CONSOLE_0_ERROR / SCENE_UNCHANGED / SELFCHECK_BLOCKED_UNRELATED_GT06 / HP_BASEMAX_DEFERRED / GOAL3_USER_HOLD`
> 依赖：`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001 / VERIFIED`

## 目标

对齐 C09/C20 共享 held-refill writer 中不依赖 authoritative baseMax 的精确子集：

- OID123 对入场 HP<=0 仍执行 `HP-=2`、holder MP+3、child MP cap 和exhaustion。
- OID123 仅以 child `OrdinaryCreditGate2F4 >= 0 && child MP > 150` 将 child MP 截到150。
- OID122/123 exhaustion 共享 tail 仅消耗一次 `[0,7)` battle RNG 写 Vx，写 Vy=0
  并保留入场 Vz，重置现有 action/counter/relation/weapon-HP 载体。

## 允许修改路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldRefillMpExhaustionProductionEditorTests.cs` 及 `.meta`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementPlayModeProbeEditor.cs`（仅在可运行Play验收时补充精确见证）
- 本 Task/Change、Ledger、STATE、handoff 和 NTSD28 总表。

## 不变量

- 不改 C09/C20 placement 和扫描次数。
- 不改 OID122 HP/HPBound 对 baseMax 的 clamp；该依赖继续归 B11/H。
- 不改 WPoint pose、kind-3 release、DVX release、cover、terminal action、invalid reciprocal或
  system-table/content authority。
- 不读 `KillCount`作为 +0x2F4，不把 cap 写到 holder。
- exhaustion 不清 Vz，不写 -8 垂直速度，不多消耗 RNG。
- 不修改 Config、Scene、Prefab、资源、ProjectSettings 或 Authority 目录。

## 验收

1. Focused test 覆盖 OID122 HP=1、OID123 HP=2/0/负值、gate=-1/0、child/holder MP
   独立 sentinel、exhaustion/non-exhaustion RNG delta、Vy/Vz、action/counter/relation/weaponHP。
2. 修正 production actual，更新现有 SelfCheck 的 consume 断言。
3. 运行 isolated Runtime/Editor compile、source contract、scoped diff check、Ledger validator。
4. Unity 许可恢复后运行 focused、held/C09 related、B6/B5/NTSD28 broad、fresh SelfCheck、
   C09/C20 Play probe、Console 与 Scene dirty/hash。没有这些证据不得写 `VERIFIED`。

## 回滚

恢复 resolver 原有 OID123 早退/cap 和exhaustion Vy/Vz，移除本包新测试及治理记录；
不回退已有 B0–B6 其他用户工作。

## 当前证据（2026-09-08）

- 新增7-case focused Editor test，覆盖本合同要求的OID122/123、nonpositive HP、gate、child/holder
  MP、RNG、motion、action/counter/relation/weaponHP矩阵；在production修改前先加入隔离Editor编译。
- production与现有SelfCheck consume断言已同步；Runtime isolated compile为47 warnings/0 errors，
  Editor isolated compile为104 warnings/0 errors。
- 10项源码合同检查和scoped `git diff --check`通过；Change Ledger validator通过
  （361 records / 309 governed code files）；临时csproj测试Include已移除。
- Unity Test Runner、fresh SelfCheck和Play probe均未执行；既有Hub/AppData许可阻塞没有外部变化，
  因此本包只能保持`RUNTIME_PENDING`，不得标记`FOCUSED_TEST_PASS`或`VERIFIED`。
- 2026-09-09 Unity已恢复：focused `7/7`、C09 related `2/2`实际通过，build 0 error；
  但专门refill/exhaustion Play仍未取得。用户要求暂停总体目标，本包转为`USER_HOLD`并继续
  保持`RUNTIME_PENDING`。

## 2026-09-09 Goal 2 最终结果

仅新增test-only探针，生产实现沿用。正式NTSD_Battle的Sakura/action242与Naruto/action291 DAT，
通过正式OPoint kind2关系绑定入口后由生产driver完整tick5→20推进，6/6序列、26个消费样本和
6次exhaustion通过。nonpositive HP、child/holder MP cap与KillCount分离、单次RNG、Vy0、Vz保留、
PS.zz0和既有reset均有逐样本结果。既有HP/baseMax权威clamp与完整OPoint materializer不在本包。
环境新增OID150/slot52已由normal random-drop的allocate调用栈确认，探针自有实体全部解绑回收。
退出Play后Console0、Scene dirtyfalse/root13、指定SHA不变，无需恢复Scene。
focused+C09 9/9、NTSD28 category229/229、B5 name-group759/759、两套build0 error；
fresh SelfCheck11:36:59Z仍停在独立GT-06，状态如实保留。详细job、命令、输出与产物见本包Record。
本包推进VERIFIED；完成本轮治理同步与validator后停止等待用户复核，禁止自动启动Goal3。
