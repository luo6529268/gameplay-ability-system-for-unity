# R7-AI-02A — authority dispatcher fixture contract

> 日期：2026-08-22  
> 状态：`VERIFIED / TEST-ONLY RED-WITNESS CONTRACT`

## Goal

建立C++ 39-position character decision chain的source-derived顺序、outer gate、RNG和early-return验收入口，
并以两个定向红测证明当前Unity missing/gate differences。production AI不得改。

## Scope

- 新建一个Editor test fixture；
- 固定39个position的顺序表；
- OID6 position7 missing witness；
- OID33 position28 outer-gate witness；
- 记录red result、compile、现有AI regression与fresh-domain full self-check。

## Authority / Evidence

- C++ `input_handler.cpp:340-367,824-851,2055-2204`；
- `RESEARCH/R7-AI-02-character-decision-chain-preflight-20260822.md`；
- `D-INP-007A/B`、`D-INP-009`。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/AiDecisionAuthorityChainContractEditorTests.cs`；
- 本Task、Change Record、ledger、STATE和handoff。

## Deliverables

1. 39-position source contract test；
2. 两个`Explicit` red witnesses；
3. 实际定向red evidence；
4. ordinary AI suite/full self-check保持通过；
5. 不修改production。

## Verification

- source-contract test PASS；
- 两个显式witness定向执行并按预期FAIL；
- existing `AiDecisionKernelEditorTests` + `AiDecisionSoAShadowEditorTests` baseline；
- Unity compile error=0；
- fresh-domain full self-check；
- ledger validator / scoped diff。

## Stop conditions

- 为让红测通过需要修改production；
- fixture需要02B HitJ；
- 发现39-position authority表有歧义；
- 需要改RNG/pass/profile/capacity。

## Out of scope

02B～02F实现、PlayMode、C++运行、broadphase、worker、capacity、R8/T8。

## Final result

- fresh Editor compile：`Assembly-CSharp-Editor.dll` 22:57:58，Console error/warning=0；
- class job `d2e5b878dab74cb3a04dd0e4920a1a17`：1 PASS + 2 Explicit skipped；
- position7 job `ae7bf5e441c845628067227aacd36c81`：expected DRJ=3 / actual=0，按预期FAIL；
- position28 job `8ba6322f7a984f669afd80610bad5c5e`：expected DUA=0 / actual=3，按预期FAIL；
- existing AI job `0417660e5b6440c98d93e2c0fb7c8ae1`：75/75 PASS；
- fresh-domain full self-check：2026-08-22 23:01:13 PASS；
- production AI无改动。02A只验证红测合同已正确建立，未关闭`D-INP-007A/B`。
