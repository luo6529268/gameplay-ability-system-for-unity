# Task Contract — NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001

> 状态：`VERIFIED / FOCUSED_8_OF_8 / RELATED_17_OF_17 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_CURRENT_BASELINE_UNCHANGED / FULL_RESOURCE_DEFERRED / ILLEGAL_CATEGORY_RETIRED`
> 依赖：`NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001 / VERIFIED`
> 取代：`NTSD28-B6-CPOINT-THROW-ATOMIC-PRODUCTION-001 / SUPERSEDED`

## 目标

精确实现当前无阻塞的 CPoint throw tail：resource-attacker存在时写 authority display lead，正
`throwinjury` 写 caught environment injury/self source且不污染 `WeaponCount`，并只在 depth
up/down恰一项为真时覆盖 Vz。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs`
- 本 Change 治理文档。

## 强制边界

- 移除 superseded 包加入的 `ResolveNativeHitResourceInjury` /
  `ApplyNativeHitResourceTransaction` production调用；不得以 `MPMax` 代替 Authority baseMax。
- MP reward/drain/gain明确保持 `FULL_RESOURCE_DEFERRED_B7_B8_B11_H`，测试不得断言临时近似值。
- display lead只在resource attacker解析成功时写；environment写入不依赖该解析结果。
- 不改 HitPlan、完整 CPoint schema、held injury、caughtact combo、Scene或内容。

## 验收

- 新 focused test 编译；Unity运行受Licensing IPC阻塞时如实保持 `RUNTIME_PENDING`。
- Runtime与Editor项目0 error；source contract/旧错误断言同步。
- 可用时补跑focused、B6/B5/NTSD28、SelfCheck和grab Play probe。
- Scene hash/dirty不变，Ledger validator通过。

## 回滚

反向移除本 Change 的display/environment/Vz写入并恢复同 Change 测试；不得恢复已判定错误的
WeaponCount/clear-Vz断言，也不得恢复superseded MP transaction。

## 当前结果

- invalid MP transaction 已移除；production 只保留可精确闭合的 display lead、environment/self
  source、WeaponCount exclusion 与 Vz XOR。
- Runtime `Assembly-CSharp.csproj`：47 warnings / 0 errors；临时把新测试纳入生成的 Editor
  project后为104 warnings / 0 errors，随后已还原生成项目，不留下该临时行。
- source contract 7项全部为 true；`git diff --check`无错误；Scene SHA仍为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- Unity batch两次均在 Test Runner 启动前因 `LicenseClient-Logan` IPC拒绝退出199，未生成XML；
  因此focused/SelfCheck/Play均未通过，状态只能是`RUNTIME_PENDING`。
- Ledger validator：355 records / 307 governed code files，PASS。

## Current corpus correction（2026-09-08）

Direction-B current已确认58条throwvx、48条positive throwinjury；本包实现的environment/self-source、
WeaponCount exclusion与exclusive-depth Vz规则不需回退，但focused/SelfCheck/Play及corpus guard必须扩展，
仍保持RUNTIME_PENDING。详见multiline总correction。

## Runtime acceptance（2026-09-09）

- 许可通道已由当前已运行的Unity 2022.3.62f3 Editor恢复；首次真实focused job
  `0e530ff8294141a9a7432f9e574539a4`执行到fixture后，10个test node均因既有非法NUnit category
  `NTSD28-B6`在OneTimeSetUp失败。该失败发生在任何生产断言之前，不构成behavioral RED。
- 将category改为项目既有规范`NTSD28_B6`后，focused job
  `b78fa2a696504d87bb9944afbf7910b4`实际`8/8`通过；同category复跑job
  `36f73c6b48da43698cb7404a93e4609c`亦为`8/8`。
- CPoint/formal/HitPlan/pre-interaction/B5 credit相关7-class job
  `b93bfe236b5b4401a3d82144c962bcf5`实际`17/17`通过。
- full SelfCheck修正漏掉的raw-throw旧clear-Vz断言后已跨过throw检查，并在较后的held-CPoint injury
  accounting断言`BattleRuntimeSelfCheck.cs:11399`停止；该断言要求legacy ComboVic/ComboAtk/global
  DamageStats，归held accounting后继包，不归本throw子包。
- 真实`NTSD_Battle` Play的`BattleGrabCpointLinkPlayModeProbeEditor`返回`PASS`：mismatch throw尾部
  position/action/Vx/Vy/Vz/environment/self-source/WeaponCount exclusion通过，link cleanup恢复到baseline；
  退出Play后Console error为0。
- 当前测试窗口的Scene baseline前后SHA-256均为
  `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`，长度`216762`，mtime
  `2026-09-09T01:58:58.6144489Z`，Editor `isDirty=false`。该baseline包含本包测试开始前已经出现的并行
  Scene修改；本包没有回退、删除或认领这些内容。
- fresh runtime/Editor `dotnet build --no-restore`均为0 error（既有warning分别47/104）。
- 完整MP resource transaction仍依赖B7/B8/B11/H；本包只关闭display/environment/self-source/
  WeaponCount exclusion/Vz exact子集。
