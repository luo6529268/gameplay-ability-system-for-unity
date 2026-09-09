# Task Contract — NTSD28-B2-NATIVE-DIRECT-HOLD-DIRECTION-ROUTING-001

> 状态：`FOCUSED_TEST_PASS / DIRECT_HOLD_DIRECTION_READY / PRODUCTION_CONNECTED / BUILTINS_JOINT_TRACE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-04

## 目标

在已通过focused的native combo/generic action transaction之后，按NTSD 2.8 `step_sampled`严格顺序
接入three-button `hit_a/d/j`、held `hold_a/d/j`、facing-aware `hit_f/b`、depth
`hit_uz/dz`及held direction `hold_f/b/uz/dz`。所有非零字段都复用generic native action
transaction；调用者按权威的不对称规则消费exact edge/previous字节，并保持direct field不清frame counter。

## Authority 与当前事实

- 正式playable build闭包中的`input_routing.cpp:431-443`：`try_field`每次重新读取当前action frame；
  field为0不attempt，非零时即使`apply_action`失败也返回attempted并由调用者消费winning byte。
- `input_routing.cpp:524-562`：three-button先缓存attack/jump/defend edge age，严格大于另外两键才尝试；
  顺序为hit_a→hit_d→hit_j，再以previous held与缓存edge比较，顺序hold_a→hold_d→hold_j。
- `input_routing.cpp:564-620`：horizontal按当时facing选择forward/back；hit_uz清previous W、hit_dz
  清edge S；held horizontal在前三键/方向action可能改变facing后重新判向；hold_uz清edge W、hold_dz
  清previous S。
- `input_routing_tests.cpp`第3、5、6、10、22、26项覆盖strict priority、exact byte cleanup、非type0、
  reject仍consume与counter/facing；fresh authority全46项已通过。
- Unity carrier/parser已具备全部字段；DataOriented second pass当前只执行native combo，然后legacy
  resolver仍执行`hit_a/d/j`，hold/direction完全缺失。本包必须移交direct所有权但保留release/built-ins。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleCharacterInputActionResolver.cs`
- 新增`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeDirectHoldDirectionRoutingEditorTests.cs`及`.meta`
- `Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs`（仅把DataOriented旧direct-owner断言改为native-owner合同）
- 必要的既有B2 focused test加固
- 本Task、Change Record、Ledger、STATE、handoff与总表

禁止修改authority、Config/DAT、Scene/Prefab、ProjectSettings、Packages、carrier/parser字段、type-0
built-ins、同步RNG `0x82/0x83/0x84`、B3/B5/B7/B8 producer或timer。不得删除legacy compatibility
resolver；LegacyCanonical必须保持原行为。

## 不变量

- production顺序固定为combo→three-button direct/hold→direction direct/hold→legacy projection→
  release/built-ins；每个field读取动作发生时的当前frame，不能在pass开头缓存全部字段。
- three-button edge比较使用进入helper时缓存的三个age；ties不尝试field，后续built-ins仍可消费。
- hold_a/d/j比较previous bool与原缓存edge age；三个独立if按a→d→j执行，同tick允许连续读取新action
  frame并尝试多个hold字段。
- horizontal hit按进入direction helper时facing；held horizontal在前序action后重新读取facing。
- cleanup精确：hit_f/b清winning horizontal edge；hit_uz清previous W；hit_dz清edge S；hold_f/b
  清winning previous horizontal；hold_uz清edge W；hold_dz清previous S。
- field为0或当前frame不存在时不attempt、不消费；field非零则无论lock/cost成功均消费调用者字节。
- 所有field action复用`ApplyNativeInputAction`，不得引入direct locomotion/resource policy；direct
  hit/hold字段不清`AttackingCounter`。
- DataOriented共享legacy resolver跳过combo和direct field owner，但继续release/built-ins；Legacy不变。
- hot route不得分配managed memory；不修改InputHistory、combo state、与当前field无关的exact字节。

## 验收

- test-first红灯覆盖strict edge priority/tie、three held exact cleanup/连续action、horizontal facing、
  四条depth不对称cleanup、zero field、rejected action consumption、counter/negative facing、非type0；
- 覆盖combo后same-tick读取新frame、direction在three-button后读取新frame、held horizontal重新判向、
  DataOriented单一owner、Legacy不变及warm 0 B；
- Unity compile0、focused、B2 broad、snapshot/checksum相关回归、full SelfCheck、Console0、diff与Ledger PASS；
- 本包不以单元测试代替type-0 built-ins或B2 joint trace。

## Test-first 证据

- 2026-09-04：新增12项focused test并收缩既有DataOriented direct-owner断言后，Unity编译得到
  31条唯一C#错误；全部仅来自新测试引用尚未实现的`NTSD28NativeFieldRoutingResult`、
  `RouteNativeThreeButtonFields`与`RouteNativeDirectionFields`，其他项目脚本错误为0。
- 该红态只证明计划中的生产seam尚不存在；production实现与绿灯验收仍待执行。
- production已按authority顺序写入合同列出的3个生产文件；尚未取得Unity绿编译或测试证据。
- Unity Tundra编译0 error；新focused 12/12、既有input-owner 37/37、B2相关241/241、
  snapshot/checksum/restore/ring 28/28+4/4、worker/AI shadow 101/101全部通过。
- `BattleRuntimeSelfCheck`于2026-09-04 14:55:49写出`PASS`；7条已知负向rest-binding
  夹具日志清除后，Console error复读0。
- 本Task到此只达到focused-ready；type-0 built-ins、sync82/83/84和C++/Unity joint trace仍待后续包。

## 回滚

移除native direct/hold/direction methods及production caller，恢复DataOriented shared resolver只跳过combo、
继续legacy `ApplyDirectFrameInput`；保留已验证combo/action transaction和全部carrier/parser。
