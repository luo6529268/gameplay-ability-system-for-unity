# R7-PERF-01 — PreInteraction cross-pass proof 重新认证预检

> 日期：2026-08-22  
> 状态：`SOURCE_CONFIRMED_DIFFERENCE / READY_FOR_IMPLEMENTATION`  
> 差异 ID：`D-PERF-001`  
> Change ID：`R7-PERF-001`  
> 前置阻塞：已解除；`R6-PRES-005` 于2026-08-22 19:49:12取得fresh full self-check PASS。

## 1. 结论

Unity 的同点 `TryProveWholePreInteractionPassNoOp` 是保守 proof：它在 C++ T14 等价位置读取当前
frame、Prev2 collision frame、link/target/held reference 与 runtime snapshot；证明失败就进入正式三轮
CPoint / mismatch / weapon-sync 扫描。

但 `TryUsePreInteractionCrossPassProof` 不是等价 fast path。它在
`PostFrameAdvanceDeathCleanupAll` 后缓存 no-op 结论，随后仍会经过 first held pass、candidate collect、
character/object collision consume、random weapon drop 和 candidate cleanup。当前消费端只复核：

- logical capacity / claimed count；
- occupancy epoch / generation；
- pending destroy epoch / pending unregister count。

这些结构量无法证明中间 pass 没有修改既有实体的 frame、CPoint、link、target 或 held state。因此该缓存
可在内容已经变成 C++ T14 participant 后继续命中并跳过整个 T14。该项登记为 confirmed static
optimized/fallback behavior difference，不得继续以旧 C# A/B 或性能数据认证。

## 2. C++ Release authority

- `Makefile:11-35` 将 `game_tick.cpp`、`cpoint.cpp`、`weapon.cpp` 纳入 `ntsd_new.exe` release build；
- `game_tick.cpp:1818-1825`：object collision consume 完成后才调用
  `run_cpoint_and_weapon_sync_passes`；
- `game_tick.cpp:659-664`：T14 固定先 `run_cpoint_runtime_pass`，再
  `weapon_sync_runtime_pass`；
- `cpoint.cpp:23-190`：每次 T14 都从当时的 active entity、`prev_frame2` 和 current frame重新读取
  kind1/kind2 relation与动作；
- `weapon.cpp:13-107`：同一 T14 再从当时 current frame读取 state9/kind1和 held link。

C++ 没有在 death cleanup 后缓存 T14 no-op 结论。若 collision consume 改写 frame/relation，T14必须看到
改写后的状态。

## 3. Unity current route

- `SimulationWorld.Passes.partial.cs:657-735` 在
  `PostFrameAdvanceDeathCleanupAll` 发布 `preInteractionCrossPassProof*`；
- `NTSDBattleTickSystem.cs:337-376` 的当前正确顺序是 death cleanup之后仍运行held#1、collision
  collect/consume和random weapon，最后才进入`ResolveCpointAndWeaponSync`；
- `SimulationWorld.Passes.partial.cs:2237-2256` 优先消费cross-pass proof，命中后直接return；
- `SimulationWorld.Passes.partial.cs:2354-2376` 只比较结构epoch/计数，不比较frame/link/content epoch；
- 全仓搜索确认 `preInteractionCrossPassProofValid` 仅由publisher写入，没有中间 gameplay writer
  失效通知；`SimulationWorldMutationTracker`也只跟踪`PendingFlushDestroyEpoch`；
- 现有 `PreInteractionNoOpProofEditorTests` 覆盖neutral、初始non-neutral、slot occupancy变化、
  generation reuse和0 GC，但没有“proof发布后、同slot内容被collision writer改成kind1/kind2”的矩阵。

## 4. 最短 first-difference fixture

建立 cached 与 oracle 两个相同 world：

1. exact `LF2Character` 在 death-cleanup checkpoint 为neutral，两个world都发布cross-pass proof；
2. 不增删实体、不改变slot/generation/occupancy，只把同一角色切到current kind2 CPoint帧并同步其当前
   runtime snapshot，模拟合法intermediate hit writer结果；
3. cached world保持production cross-pass proof；oracle world强制跳过cross-pass proof；
4. 同 tick调用`PreInteractionTickAll`；
5. C++/oracle应执行kind2 invalid-relation tail，写frame=212、Vy=-3、Y<=-2；current cached route会
   `LastPreInteractionCrossPassProofUsed=true`并零执行，形成first difference。

还需一个neutral control证明移除cross-pass cache后，T14当点whole-pass proof仍可成功且保持0 writer。

## 5. 推荐最小实施边界

后续 `R7-PERF-001` 只允许：

- production不再消费 `TryUsePreInteractionCrossPassProof`；
- 保留并继续使用 T14 当点 `TryProveWholePreInteractionPassNoOp`；
- 保留participant filtering、forced legacy/full-scan oracle和现有诊断；
- 加入上述 stale-content difference fixture与neutral control；
- 不引入“全 runtime content epoch”。当前大量C++等价字段仍允许直接写，无法用一个未覆盖所有writer的
  epoch诚实证明缓存有效；伪epoch会把同一问题隐藏得更深。

## 6. 验收与边界

需要：source review、focused `PreInteractionNoOpProofEditorTests`、full self-check、fresh Unity compile、
fallback/optimized extended checksum、0 B warmed test。Play Mode/C++ full trace不可用时最高仍为
`RUNTIME_PENDING`。

不得修改C++、pass order、CPoint/weapon writer、R6 presentation、CentralOnly、capacity、30Hz、worker、
SoA/ECS、pool、T8或场景/DAT。`R6-PRES-005` fresh自动验收已通过；本修复现由独立Task/Change
Record约束实施。

## 7. 实施边界修正

除移除production consumer外，同时移除death-cleanup checkpoint内不再有消费者的proof计算/保存，避免
保留一套每实体无效工作。`ForceLegacyPreInteractionCrossPassProofForDiagnostics`与
`LastPreInteractionCrossPassProofUsedForDiagnostics`暂保留为stress/report schema兼容接口；production
不再读取前者，后者每次T14保持false。不得在本包修改stress report schema。
