# Task Contract — NTSD28-B2-FUNCTION-KEY-ROUTE-CONTRACT-001

> 状态：`FOCUSED_TEST_PASS / PURE-ROUTE-READY / PRODUCTION-UNCONNECTED / NEXT-SESSION-STATE-CARRIER`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / FUNCTION KEYS`  
> 建立日期：2026-09-04

## 目标

以纯C#、无UnityEngine/InputSystem依赖的值类型与router，精确复刻Authority F1～F12映射、disposition、command、
repeat/context/maintenance优先级和result predicates，为后续Session carrier与production physical adapter提供唯一语义入口。

## Authority 与原状

- Authority：`native_function_keys.h`中的`NativeFunctionKey28`、disposition/reject/command enums、
  `native_function_key_from_virtual_key28`与`route_native_function_key28`；正式main/tests证明其在playable closure内。
- crosswalk：`docs/ai/MANIFESTS/NTSD28-B2-FUNCTION-KEY-ROUTING.md`。
- Unity当前没有统一route/result类型；B1 Host latch与旧F7/F8/F9 latch直接读physical keys，各自定义局部语义。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeyRouter.cs`（新建）。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyRouterEditorTests.cs`（新建）。
- 上述Unity生成的`.meta`，以及本Task/Record、Ledger、STATE、handoff、总表。

不得修改`SimulationTickDriver`、`BattleFunctionKeyInputLatch`、B1 Host policy、GameConfig/asset、Input Actions、
Runtime/Flow、snapshot/checksum、Scene/Prefab、DAT、Authority或任何下游F4/F6～F9/F11/F12效果。

## 实现合同

- enum数值顺序与Authority一致；virtual key仅接受`0x70..0x7B`。
- 提供不带context的Authority-default入口（battle/main/delay均true、lock false），避免C# default struct全false误语义。
- priority严格为non-F→F11/12→repeat→CtrlF9/F10→battle→main→F6-9 lock→F8/9 delay→dispatch。
- F10保持no-action；F11/F12不是one-shot并绕过repeat/context；router不复制B1 Host transition，不修改runtime state。
- 不分配集合、不读取wall clock/Transform/InputSystem；每次Route为纯函数。

## 验收

- 先创建引用缺失类型的Editor test并记录compile red，再实现production类型。
- tests覆盖12键表、virtual key边界、repeat、maintenance-before-context、F11/F12 bypass、context优先级、
  lock/delay选择性、F10与`IsOneShotCommand`。
- Unity scripts compile 0 error；新focused tests全通过；B1 Host相关tests无回归；full SelfCheck PASS；Console 0 error。
- Change Ledger validator通过。production physical行为保持不变，状态不得扩大成runtime aligned。

## 回滚

删除两个新脚本及`.meta`并回退本包治理行即可；现有Host/FunctionKey production路径未接入，行为可完整恢复。

## 完成证据

- test-first：修正当前NUnit不支持`Assert.Multiple`的测试壳后，fresh compile得到123条预期missing-type
  `CS0246/CS0103`；未由无关错误冒充red。
- 新`NTSD28NativeFunctionKeyRouter`实现Authority enum/value顺序、virtual-key投影、九级priority、全部result字段与
  one-shot predicate；不依赖Unity/InputSystem，不接production。
- Unity scripts compile 0 error；focused router `8/8`（job `ceb9036117174adeac78ed527543dfbb`）；router+旧FunctionKey+
  B1 Host policy `19/19`（job `0020ddd4225443b88f46c3f2dd44857f`）；4096 warm calls为0 allocation。
- full BattleRuntimeSelfCheck：2026-09-04 20:01:21 `PASS`；7条既有negative-path预期错误已审阅后清空，Console 0 error。
- validator：`PASSED / Records 154 / governed code files 106`；production physical/runtime行为未改变。
