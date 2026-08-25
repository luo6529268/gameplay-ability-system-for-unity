# R7-AI-02C — OID6/7/8/11 helper module

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / UNWIRED MODULE`

## Goal

以 C++ release source 为唯一行为权威，实现 39-position chain 的 positions 7–16（OID6/7/8/11）为一个
非 partial、持久复用、纯数据实例模块，并建立 source-derived fixtures。模块本包不得接入默认 dispatcher。

## Scope

- positions 7–16 的条件、strict comparisons、RNG短路顺序、input/combo写入和early-return；
- position15 OID11 `HitJ==290` void side-effect，依赖已完成的02B row；
- 提取现有 `AiDecisionKernel.RngClone` 为共享值类型，保持LCG、call count和trace hash逐位不变；
- 新模块只读`AiSensingSnapshot`，只写`AiDecisionInputState`与共享RNG值类型；
- fixed-seed focused tests覆盖每个position、side-effect continuation、early return和warmed 0 B。

## Authority / Evidence

- `J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp:340-603`；
- dispatcher顺序：同文件`2080-2116`；
- release build/live-path参与性已由R7-AI-02 inventory闭合；C++目录保持只读。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionRandomStream.cs`（new）
- `Assets/NTSD/Scripts/Simulation/Ai/AiCharacterDecisionModule.cs`（new）
- `Assets/NTSD/Scripts/Simulation/Ai/AiDecisionKernel.cs`（RNG type mechanical extraction only）
- `Assets/NTSD/Scripts/Test/Editor/AiCharacterDecisionModuleEditorTests.cs`（new）

## Unknowns

- 本包不验证完整positions1–39相互遮蔽；只由02F联合验收；
- C++ runtime trace仍BLOCKED；
- 真实角色DAT/Play Mode由02F/R8负责。

## Deliverables

1. shared allocation-free RNG stream；
2. instance `AiCharacterDecisionModule.TryEvaluatePositions7Through16`；
3. source-derived fixed-seed helper/order/0B tests；
4. compile、existing AI regression、fresh self-check、ledger证据与handoff。

## Verification

- generated C# build与fresh Unity compile 0 error；
- exact focused fixtures全部PASS；
- existing 39-position contract/red witnesses保持原状态；
- existing AI 212+ regression PASS；
- kernel RNG state/calls/order-hash regression不变；
- warmed module path 0 B；
- fresh-domain `BattleRuntimeSelfCheck` PASS；
- `Tools/Validate-ChangeLedger.ps1` PASS。

## Stop conditions

- 需要改变outer gate、positions1–6或17–39；
- 需要默认接线或改变Legacy/DataOriented输出；
- 需要新数据字段（02B HitJ之外）；
- 需要修改C++、pass、profile、capacity、render或input binding。

## Out of scope

- default dispatcher integration（02F）；
- OID10/1及后续helpers（02D/02E）；
- input edges/cooldown调用点移动；
- Play Mode/C++ runtime trace/R8。
