# R5-LINK-02 — negative-link invalidation contract

> 建立日期：2026-08-22  
> 状态：`PLANNED`  
> 对应差异：`D-LINK-002`  
> Change ID：`R5-LINK-002`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1441-1457, 1860-1872`。

## Goal

使Unity的invalid negative-held relation与C++ release一致：child仅清`LinkState`，保持`HolderStableId`
（C++ `holder_idx`映射）的既有值。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；
- `Assets/NTSD/Scripts/Test/Editor/SimulationQueryAndLinkModuleEditorTests.cs`（新建focused test）。

## Required behavior

1. active、negative-link child的holder slot越界、holder inactive或holder target mismatch时，只写child `LinkState=0`；
2. child `HolderStableId`保持原值；holder `TargetSlotIndex`和其它reverse/held字段不被该invalid branch改写；
3. shared `HeldObjectProcessAll`被first/second held pass调用时，第二次不重新处理已经为0的child；
4. valid negative link仍进入既有`BattleHeldObjectWriter.RunStep12`；任何release、throw、CPoint、slot或frame写入不变；
5. 不改变slot升序、capacity/generation guard或allocation行为。

## Authority / Evidence

- C++ source contract：`game_tick.cpp:1441-1457`与`1860-1872`；`Makefile:32`。
- Unity static mapping：`SimulationQueryAndLinkModule.cs:39-61`、`NTSDBattleTickSystem.cs:450-457`。
- preflight：`RESEARCH/R5-LINK-02-negative-link-invalidation-preflight-20260822.md`。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | C++ source / Makefile、Unity two-pass caller与exact invalid branch重读；不运行/写C++ authority。 |
| S1 | self-check和focused EditMode覆盖out-of-range holder、active-holder mismatch与第二shared pass不重清。 |
| S2 | ledger validator、R5范围`git diff --check`、当前打开Unity Editor scripts refresh、`error CS=0`、full self-check PASS、focused EditMode PASS。 |
| S3 | 只可标`RUNTIME_PENDING`；C++ trace与真实Play Mode继续待验。 |

## Stop conditions

如发现正确处理需要修改双pass调度、CPoint/WeaponSync、valid held release/throw、target/holder其它字段、
generation/slot allocator、C++ authority或任一其它R5链路，停止并新建合同。

## Out of scope

`D-LINK-001`、`D-HOLD-001/002`、`D-CPT-*`、`D-OP-*`、T8、性能、服务器、render、C++ trace和Play Mode。
