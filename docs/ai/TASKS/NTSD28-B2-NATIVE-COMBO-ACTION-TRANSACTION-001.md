# Task Contract — NTSD28-B2-NATIVE-COMBO-ACTION-TRANSACTION-001

> 状态：`FOCUSED_TEST_PASS / COMBO_ACTION_TRANSACTION_READY / PRODUCTION_CONNECTED / JOINT_TRACE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03  
> 最近验证：2026-09-04

## 目标

把已完成的exact combo10和动作carrier接入2.8生产第二遍：先对current应用原子remap并计算bound
jump suppression，再推进edge/history/combo，随后按native优先级处理一个completed combo，并以完整
`FUN_0040D900`动作事务写入action、resource、last-action、facing与combo attempt清理。

## Authority 与当前事实

- authority `source/ntsd28_core/src/simulation/input_routing.cpp:287-428`：`apply_action`严格执行
  lock→负数/999→source frame存在→state编码重定向→source frame费用→fallback→last-action/action。
- 同文件`430-522`：combo field priority、horizontal facing、`hit_ja`特殊族、成功才清frame counter、
  非零字段一旦attempt即清combo输入窗口。
- 同文件`1504-1574`：current remap必须在edge前原子应用；bound只屏蔽K rising；路由顺序为
  combo→three-button→direction→built-ins。
- authority `input_routing_tests.cpp` 46项均由`main`调用；本包承担combo/action transaction域，
  direct/hold/direction与built-ins仍由后续包承担，最终全部进入B2 joint trace。
- 2026-09-04 fresh authority evidence：46个`test_*`定义、46个`main()`调用、两侧unique均46，
  missing/extra均0；从当前authority source编译到工作区`Temp`的测试EXE以只读正式夹具运行PASS。
- Unity exact combo selector、two-pass producer和carrier已通过focused，但selector仍无production caller；
  现有compatibility frame jump的费用、fallback、last-action与counter语义不等于2.8事务。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`
- 新增 `Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeInputPreprocessor.cs`及`.meta`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs`（仅DataOriented跳过legacy combo owner）
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`（仅新增native action raw write边界）
- 新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboActionTransactionEditorTests.cs`及`.meta`
- 必要的既有B2 focused test加固
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改authority、Config/DAT、Scene/Prefab、ProjectSettings、Packages、frame/definition内容值、
three-button direct/hold/direction consumer、type-0 built-ins、同步RNG `82/83/84`、timer或B3/B5/B7/B8
producer。不得把compatibility resolver删除或把本包状态扩大为完整action routing已对齐。

## 不变量

- remap只改current，7个destination全部在`0..6`时才提交；非法索引保持原sample不变。
- bound条件为`BoundState198>0 && globalState!=1 && globalState!=3`，只屏蔽K rising，不改current/
  previous，也不屏蔽其他键。
- completed combo严格只选择第一个native priority field；horizontal facing只在field非零时写。
- 普通非零combo field无论action成功或被lock/cost拒绝都属于attempt并清authority attempt边界；零field
  不清。普通成功（含reapply）才清`AttackingCounter`。
- `hit_ja` special family为`ObjectId==6 || characterData.use_ai==6`；300/HP>177/feature false走
  no-action success；linked/gate328尾严格复用`InputSpecialGate194/Unk328/Unk338`三态。
- generic action target先取绝对值、999→0；source frame存在后才允许state编码重定向，费用始终取
  source frame，重定向target不得二次取费。
- 资源事务仅在source `mp!=0 && InputLocalResourceEnabled49D034`时发生；waiver→definition recmp或
  mode multiplier→double cost；HP=`(mp/1000)*10+hp`，HPBound只减`hp/3`。
- 不可负担fallback严格`InputSpecialGate194→definition caughtact→InputModeFallbackActionB8`；
  fallback不写`InputLastAction144`、不扣资源，负requested仍翻转facing。
- normal success写`InputLastAction144`与action；本包不让three-button/direct/hold路径提前改成native。
- production顺序保持proxy→remap/bound→edge/history/combo→combo action→legacy projection→既有后续。

## 验收

- test-first覆盖remap原子性、bound only-K、lock/min/999/missing、两类state redirect、source-cost、
  recmp/mode/double/waiver/F6、HP/HPBound/totals、fallback priority、negative facing、reapply counter、
  horizontal facing、`hit_ja`特殊族与attempt clear；
- 后续coverage hardening钉住duplicate remap的source-order覆盖、undefined encoded redirect raw write、
  `mp==0`整段资源事务旁路、HP严格大于费用、reject保留counter与`hit_ja==0` gate328尾；
- 覆盖production two-pass caller、LegacyCanonical不变、warm preprocessor/transaction/route零分配；
- compile0、focused、B2 broad、action/lockstep相关回归、full SelfCheck、Console0、diff与Ledger PASS；
- authority `input_routing_tests` 46/46进入`main`且fresh binary PASS；
- 本包不以单元测试冒充B2 joint trace或真实全角色action parity。

## 回滚

移除native preprocessor、combo production caller与action transaction/raw action write边界，恢复
`ProcessNativeSampledState`只推进exact state并投影legacy。保留已验证carrier、selector、two-pass和RNG。

## 最终验证

- gameplay Unity PID 51752、Unity 2022.3.62f3；`Editor.log`确认project path为本仓库，Tundra
  build success，`Assembly-CSharp.dll`与`Assembly-CSharp-Editor.dll`均生成，0 C# error。
- focused job `1ad4b0b855d348cc8626a89378b4675e`：18/18 PASS，0 failed/skipped。
- B2 broad精确六组namespace/class过滤job `21fd6d75fdc74ac585ba076411d61661`：261/261 PASS；
  早先`991fc6d7b04e4e4b8bb9df04fd13f3b9`只覆盖180项，未冒充broad；加入B0 raw-capture
  namespace的超集`ba267403a2db4c2da7226853dcbcba20`为267/267 PASS。
- snapshot/checksum/lockstep job `adb87cbf590f4deb9481fc1372a0679e`：31/31 PASS。
- `Temp/NTSD_BattleRuntimeSelfCheck.request`由Editor消费后，2026-09-04 14:31:34 fresh result为
  `PASS`；Console先观测到7条既有负向rest-binding防御日志，清除这些预期项后重新读取
  error/exception/assert为0。
- authority `input_routing_tests` fresh 46/46 main closure与binary PASS；`git diff --check`及
  Change Ledger最终验证见Change Record。未执行B2 joint trace或真实全角色action parity，后三类
  direct/hold/direction和type-0 built-ins仍必须由后续包实现。
