# NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_RUNTIME_AND_TEST
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleMatchConfigRuntimeAdapter.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/StageSpawnTaskConfigurator.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B0DirectEntitySelfOwnerProductionEditorTests.cs
authority: NTSD 2.8-Logan game_session.cpp direct combatant owner_slot=self physical slot and spawn_native_story_row owner_slot=selected physical slot; battle_world.cpp spawn_at exact owner materialization; EXE B1E13AE1, closure 39DDDA15.
evidence: direct App/bootstrap registration now declares required/self slot before ModuleBind, accepts only Authority physical slots 0..19, and resets/recycles a rejected exact-slot registration before skipping roster publication; stage tasks carry owner=required slot; character/weapon/special/other consume explicit task owner before first registration snapshot; stage/results-reserve retain self owner after post-init. Runtime compiled 0 errors/47 warnings and Editor compiled 0 errors/104 warnings out of process. Restarted Unity 2022.3.62f3 refreshed both assemblies at 2026-09-08 22:51:33 local with no C# errors; corrected focused v1 then passed 15/15 at 22:51:46. The first Unity run was an informative 7 pass/8 fail because the test incorrectly expected independent raw-slot backing to mirror claimed entity runtime; all eight active entity owners were correct, and the oracle now checks claimed entity runtime plus raw backing sentinel -1. Full SelfCheck ran at 22:52:37 and stopped at the pre-existing unrelated CPoint throw-Vz assertion before this package's checks. Play and joint trace remain pending.
-->

> 状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / UNITY_FOCUSED_15_OF_15 / SELFCHECK_BLOCKED_BY_UNRELATED_CPOINT / RUNTIME_PENDING / B0_OWNER_PRODUCER_ROUTE_2 / DIRECT_AND_STAGE_SELF_OWNER_ONLY`

完整范围、权威、验收与回滚见
`docs/ai/TASKS/NTSD28-B0-DIRECT-ENTITY-SELF-OWNER-PRODUCTION-001.md`。

## 实际修改与验证

- `BattleMatchConfigRuntimeAdapter.PrepareDirectParticipantRegistration` 将 direct participant 的
  config index同时声明为required physical slot与self owner，并按Authority的20槽上限只接受`0..19`；
  `AppManager`和`BattleTestBootstrap`均在`ModuleBind/Register`前调用。
- `AppManager`与`BattleTestBootstrap`在`ModuleBind/Register`后要求actual slot等于requested slot；冲突或
  其他注册失败时调用既有`ReleaseRejectedSpawn`完成unregister/reset/recycle，并跳过Initialize与roster写入。
- `StageSpawnTaskConfigurator` 将有效`requiredRuntimeSlot`同步写入`ownerEntityIndex`；默认`-1`保持未绑定。
- `LF2Character`、`LF2WeaponBase`、`LF2SpecialAttack`与`LF2OtherObjectLifecycleModule`在各自OPoint
  初始化流程、首次`Register`之前读取task explicit owner。character的写入位于其旧`-1`重置处；weapon/other
  同样替换初始化期`-1`，pool `Reset()`仍严格写回`-1`；special在parent初始化时显式写入。
- 两个factory保留既有注册后owner校验写；没有加入依赖具体派生类型的外围时序分支。
- `SimulationStageWaveModule` ordinary stage与results-reserve成功路径最终owner改为
  `requiredRuntimeSlot`，不再尾部覆盖为`-1`。
- SelfCheck新增direct slot 0/9/10/19 initial claimed active-slot entity runtime snapshot、`-1/20/int.MaxValue` invalid边界、reset sentinel、
  occupied-slot拒绝后caller cleanup与stage task owner断言；
  既有stage slot21、reuse slot20与results-reserve slot20断言同步要求claimed entity runtime owner=self，
  同时保护独立raw-slot backing owner保持`-1`。
- 新focused Editor测试覆盖direct slot矩阵、`-1/20/int.MaxValue` invalid边界、reset、occupied-slot拒绝后
  caller cleanup、stage task 20/399、logic factory character首次claimed active-slot snapshot、
  non-character explicit non-self owner及results-reserve post-init，并附v1 request runner。
- 中间runtime compile曾因同名赋值误命中OtherObject `Reset()`而报`task`未定义；已恢复reset `-1`并只
  修改`InitializeParent(task)`。最终验证：
  - direct slot审查发现adapter会接受Authority范围外`20+`，且App/bootstrap未在注册失败后阻止
    Initialize/roster写入；现已限制为`0..19`、要求actual=requested并走统一cleanup，补
    `-1/20/int.MaxValue`与occupied-slot focused/SelfCheck断言。
  - 边界修正后`dotnet build Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`：
    `0 error / 47 warnings`。
  - 临时把新test source纳入生成Editor csproj后`dotnet build Assembly-CSharp-Editor.csproj
    --no-restore -v:quiet -clp:ErrorsOnly`：边界修正后`0 error / 104 warnings`；临时行已移除。
  - `Tools/Validate-ChangeLedger.ps1`：`PASS / 397 records / 324 governed code files`。
- 用户重启后的Unity 2022.3.62f3 PID 49020已通过本地stdio bridge重新连通；`ping`返回`pong`，随后
  `refresh_unity(force/scripts/compile)`成功请求编译。`Assembly-CSharp.dll`与
  `Assembly-CSharp-Editor.dll`于2026-09-08 22:51:33本地时间刷新，Editor日志无C#编译错误。
- 首次v1实际执行为`7 passed / 8 failed`。八个失败均来自测试错误地要求独立raw-slot backing复制
  active entity owner；失败中的active entity `Runtime.OwnerSlotIndex`均已正确，raw backing按既有合同保持
  `-1`。测试与SelfCheck已纠正为读取claimed slot view中的entity runtime，并额外断言raw backing不被污染。
- 纠正后v1于22:51:46实际通过`15 passed / 0 failed / 0 skipped / 0 inconclusive`。
- full `BattleRuntimeSelfCheck`于22:52:37实际运行并在既有、无关的
  `CheckCpointThrowRawAndTransformMatrix` throw-Vz断言处失败；调用顺序尚未到达本包新增检查。
  Play和joint trace仍未运行，故保持`RUNTIME_PENDING`。
