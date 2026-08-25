# R4-HIT-02B — kind16 character raw-frame preflight

> 调查日期：2026-08-22  
> 状态：`VERIFIED source contract / implementation planned`  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> C++ 边界：仅只读源码；未运行、构建、修改、复制或向 authority 写入。

## 1. C++ contract

`src/entity/hit.cpp:664-793` 的 `apply_kind15_16(..., kind=16)`、target `obj_type==0` branch按如下顺序：

1. 依据 `fall_damage_div`计算 `adjusted_injury`；
2. 写 lethal attribution（适用时）、HP、HP max、victim combo、holder combo、damage stat；
3. 写 SFX_065；
4. **直接** `victim_core.frame = 200`；
5. **下一句显式** `victim_special.attacking = 0`；
6. 写 positive vrest；
7. 若 active held relation符合条件，写 vrest/link、random held frame与held Vy。

结论：frame=200是raw current-frame write；attacking清零是该case的独立显式写入。C++ case没有对
prev-frame或wait-counter的隐式重置。

## 2. Unity crosswalk

- exact `LF2CharacterHitResolver` 和 shared `LF2CharacterDatHitResolver` 的kind16都调用
  `world.DamageWriter.ApplyKind16(...)`；
- `BattleDamageWriter.ApplyKind16:137-179` 已保持伤害/stat → `QueueBattleSound` → frame →
  `AttackingCounter=0` → vrest → `ReleaseHeldTarget`顺序；
- 唯一确认差异为line172使用`ImmediateFrame(LF2StandardFrames.MpDrain)`，该helper提前写PN、清 attacking、
  以target frame重同步wait；而后面的显式 `victim.AttackingCounter=0` 仍必须保留；
- `BattleEcsHitExecutionPlan` 是可配置的diagnostic shadow/projection：`SimulationWorld`只在diagnostic capture
  API中调用其projection，注释与source都说明DataOriented仍复用canonical resolver writer。该projection的
  `TargetPrevFrame` / `TargetWaitCounter`为基线快照而非本包的runtime writer；不在本包改动。

## 3. 最小实施设计

1. 只将`ApplyKind16`的`ImmediateFrame(MpDrain)`替换为已有
   `DirectWriteRawFramePreserveWaitCounter(MpDrain)`；
2. 保留紧随其后的`victim.AttackingCounter = 0`，从而将C++显式副作用保留为显式Unity写入；
3. 扩展已有 exact/shared kind16 self-check：在调用前预置non-default `Frame.PN`和`Trans.WaitCounter`；
   断言frame/Data mirror正确、PN/wait不变、attacking仍变为0，并保留原伤害/stat/vrest/link/held random结果；
4. 不修改`BattleEcsHitExecutionPlan`、resolver入口、global helper或其他kind/frame writer。

## 4. 未关闭 / 不得扩大

- C++ runtime trace、真实 kind16 Play Mode和跨tick表现未验证；
- kind16内held release的完整行为已有现有fixture部分覆盖，但不是本包新增目标；
- 不以本包替代 `R4-HIT-02C`/`02D` weapon raw-frame或者`D-HIT-003` weapon vital/stat；
- 不得把C++ source证据和当前 Unity self-check扩大成完整R4或完整battle alignment。

## 5. 实施证据（2026-08-22）

- `BattleDamageWriter.ApplyKind16`已只将`ImmediateFrame(MpDrain)`替换为
  `DirectWriteRawFramePreserveWaitCounter(MpDrain)`；紧随其后的显式`AttackingCounter=0`未移动或删除；
- existing actual/shared kind16 fixture现在预置source frame10、`PN=71`、`WaitCounter=17`，并同时断言
  current/runtime/Data frame200、PN/wait保留、attacking=0，以及原lethal vital/stat/vrest/link/held结果；
- Unity 2022.3.62f3 / UnityMCP port6401 scripts refresh后filtered `error CS`=0；
  `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 05:58:02 +08:00写入`PASS`；
- post-check两条error-level console entry是既有self-check runtime-rest negative control，不是compiler或
  kind16 fixture failure。

本子包状态为`RUNTIME_PENDING`；C++ runtime trace、真实Play Mode和跨tick表现仍未关闭。
