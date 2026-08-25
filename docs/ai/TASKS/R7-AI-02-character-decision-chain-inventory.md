# R7-AI-02 — character-specific decision-chain inventory

> 日期：2026-08-22  
> 状态：`SOURCE-CONFIRMED DIFFERENCE / PLANNING COMPLETE / NO GAMEPLAY CHANGE`

## Goal

确认并登记 C++ post-special 39-position character decision chain与Unity Legacy/DataOriented现状的
first difference，拆出可实施的小Work Package；不在本Task修改production。

## Scope

- C++ `input_handler.cpp:68-1180,2055-2353`；
- Unity `AiDecisionKernel.cs:545-672`；
- Unity `SimulationWorld.AiInput.partial.cs:1946-2028,5207-5251`；
- `AiSensingSnapshot` data contract；
-现有decision Editor tests的authority覆盖能力。

## Result

- `D-INP-007A`：30个C++ call positions在两条Unity路径均缺失；
- `D-INP-007B`：现有3 helper被错误放到outer random gate外；
- `D-INP-008`：optimized snapshot缺current frame `hit_j`；
- `D-INP-009`：现有75/75只证明两条Unity路径共享一致，不能对C++裁决；
- common tail与coordinate branch本轮未发现新source difference。

## Authority / Evidence

- C++ release source / Makefile；
- `RESEARCH/R7-AI-02-character-decision-chain-preflight-20260822.md`；
- UnityMCP job `3eaff2c1bb474565b2dd4c66d02c49db` 75/75 PASS（coverage baseline only）。

## Deliverables

- 完整差异表；
- 02A～02F后续包；
- STATE / main plan / all-diff register / handoff同步。

## Verification

静态调用顺序、函数存在性与字段合同核对；不运行C++，不修改Unity gameplay。

## Stop conditions

任何production修复、长期架构变化、profile/pass/RNG/capacity边界变化都必须进入后续独立Task/Change Record。

## Out of scope

实现02A～02F、Play Mode、C++ trace、R8、T8、服务器、渲染和1000 AI性能。

