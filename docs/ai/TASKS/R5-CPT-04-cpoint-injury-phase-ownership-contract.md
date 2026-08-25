# R5-CPT-04 — CPoint injury phase ownership contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — owner transfer、joint fixture、Unity compile与full self-check已完成；C++ trace / Play Mode待验。  
> 对应差异：`D-CPT-004`  
> Change ID：`R5-CPT-004`  
> Authority：`J:/QQFile/NTSD2.4/ntsd_release` 的 `src/entity/game_tick.cpp:659-664`、
> `src/entity/cpoint.cpp:23-190`、`src/entity/weapon.cpp:13-107`。

## Goal

使 Unity prev-frame CPoint pass只执行 C++ `cpoint.cpp` 的 relation/decrease/action/throw/dircontrol，
使 current-frame held vaction、injury、position只在之后对应 C++ `weapon.cpp` 的 weapon-sync pass执行一次。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

唯一 production change：从 `RunKind1` 移除其对 `SyncCaughtByCpoint` 的 early call；保留
`SyncHeldCpoint` 作为 current-frame state9 CPoint 唯一 held vaction/injury/position owner。

## Required behavior

1. `RunKind1` 仍处理 relation frame0、decrease、aaction/taction/jaction、throw和dircontrol；
2. `RunKind1` 不得提前写 victim vaction、HP/HPBound、ComboCount、AttackingCounter、FrameDelay或held position；
3. `SyncHeldCpoint` 在 current attacker frame为state9/kind1、victim current frame为kind2且relation有效时，
   只执行一次 vaction/injury/position；
4. no-action state9 CPoint 通过 `PreInteractionTickAll` 后只伤害一次；
5. action→state9 CPoint 在 action 后只伤害一次，不得产生早期/第二次伤害；
6. action→non-state9 不得伤害，即使 entering prev-frame是state9/kind1；
7. `D-CPT-002` global stats 在本包仍不得写入；`D-CPT-003` reciprocal mismatch control flow、
   pass order、kind2 validation、throw、held/link、opoint、input、collision、render、DAT/scene、C++ authority
   均不改。

## Evidence

| 证据 | 等级 | 内容 |
|---|---|---|
| C++ Makefile:20-21 | VERIFIED | cpoint / weapon source参与 release build。 |
| C++ game_tick:659-664 | VERIFIED | CPoint pass后才是 weapon sync。 |
| C++ cpoint.cpp:23-190 | VERIFIED | 没有 held injury/vaction/position writer。 |
| C++ weapon.cpp:22-107 | VERIFIED | current state9 CPoint唯一 vaction/injury/position writer。 |
| Unity writer / preinteraction loops | VERIFIED | RunKind1与SyncHeldCpoint均可调用SyncCaughtByCpoint，且前者先执行。 |
| C++ runtime trace / Play Mode | BLOCKED / PENDING | 不因本包关闭。 |

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | 重读 source owner、Unity call graph、all callers。 |
| S1 focused joint fixture | 使用实际 `PreInteractionTickAll` 验证 no-action state9 once、action→state9 once、action→non-state9 zero；锁定frame/vaction/HP/HPBound/combo/attacking/frame-delay/link/position和无world stats写入。 |
| S2 governance | 更新 Change Record、ledger、STATE、full diff、主计划、handoff；运行 validator 与 scoped diff check。 |
| S3 Unity | scripts refresh 后 `error CS=0`，full `BattleRuntimeSelfCheck` PASS。 |
| S4 honesty | 最高 `RUNTIME_PENDING`；C++ trace / real Play Mode 继续待验。 |

## Stop conditions

- 需要改 `SimulationWorld.Passes` pass order、`D-CPT-002` stats、`D-CPT-003` flow、kind2 validation、throw、
  held/link、opoint、input、collision、render或其他未列出文件；
- source 或 fixture显示同期还有另一个 injury writer；
- Unity compile / self-check失败且无法在限定两文件修复；
- 需要修改、运行、构建或向 C++ authority 写入。
