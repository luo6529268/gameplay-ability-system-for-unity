# Task Contract — NTSD28-B5-NATIVE-COMBO-EXPIRY-001

> 状态：`VERIFIED / CORE_COMBO_EXPIRE_ROUTED / FORMAL_TUPLE_INACTIVE`
> 依赖：`NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001 / VERIFIED`

## 目标

实现Authority `CoreComboExpire`：C25 live-slot尾完成后、session post-core之前按当前native frame sequence
扫描active slots，并以inclusive `elapsed >= respond`只清零positive native combo count。

## 允许路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleNativeComboExpiryModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5NativeComboExpiryEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅需要broad guard时）
- 本Task、Change Record、Ledger、STATE、handoff与总表

## Authority合同

- `simulation_tick_driver.cpp:181-209,1068-1075`：完整slot tail后执行一次world scalar expiry。
- 缺mode record直接no-op；`respond < 0`直接no-op；不以`bound`或`caughtact`作为expiry gate。
- 只处理active entity且`count > 0`；`lastTick > battleTick`保留；否则当
  `battleTick - lastTick >= (ulong)respond`时仅清零count，不清lastTick。
- Unity当前process tick在C24后由`NativeFrameSequence`表示；接点必须在`LateEntityUpdate`返回后，
  不进入其ascending live-slot循环，也不使用输入combo timeout。

## 验收

- test-first缺类型RED；focused覆盖record/negative gates、inclusive边界、future tick、zero respond、
  nonpositive count、lastTick保留、active-only、精确placement和warm zero allocation。
- focused、B5、NTSD28、fresh SelfCheck、Console/Scene与Ledger通过。
- 不实现caughtact、HUD或正式tuple激活；不改Config、Scene、Prefab、资源或Authority。

## 回滚

移除expiry module、tick-system单一调用点和focused fixture；保留carrier与ordinary producer。

## 完成证据

缺expiry RED已取得；focused `5/5`、B5 `831/831`、NTSD28 `1178/1178`、fresh SelfCheck、
Console 0均通过。Scene `isDirty=false/rootCount=13`且SHA/mtime不变；B6/B10/H仍未接。
