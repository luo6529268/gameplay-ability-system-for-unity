# R7-AI-02D — OID10/1、9/2、32/19/33 helper module

> 日期：2026-08-23
> 状态：`RUNTIME_PENDING / UNWIRED MODULE`

## Result

positions 17–28 已写入现有非 partial 实例 module，但默认 Legacy/DataOriented dispatcher 均未调用。
source-derived focused 26/26、AI regression 238/238、warmed position21 400-slot scan 0 B、fresh Unity
compile、generated Editor build 和 fresh-domain full self-check 均通过。两个 02A red witnesses仍按预期失败，
因此 production difference 没有被误写为已关闭；完整接线与联合遮蔽只由 02F 处理。

## Goal

以 C++ release source 为唯一行为权威，实现 39-position chain 的 positions 17–28（OID10/1、
OID9/2、OID32/19、OID33/19/16）为现有非 partial、持久复用、纯数据实例模块的第二段，并建立
source-derived fixtures。模块本包不得接入默认 dispatcher。

## Scope

- positions 17–28 的固定顺序、strict comparisons、RNG 短路顺序、combo 写入和 early-return；
- position 21 OID10/1 full/team scan 的 C++ 400-slot authority 域、slot 升序、严格 `dist > bestDist`
  最远候选与同距离低槽位保留合同；
- position 21/22 void side-effect 必须继续执行后续 positions；
- position 24/26 的 dynamic modulus `target.hp / 4 + 40` 与 path-A/path-B RNG 顺序；
- position 28 只在未接线 module 内实现；不得移动现有 production helper，直到 02F；
- 模块只读 `AiSensingSnapshot`，只写 `AiDecisionInputState` 与共享 `AiDecisionRandomStream`；
- full/team scan 的 scan-slot count 显式传入；C++ authority fixture 使用 `min(400, capacity)`，
  Unity >399 adapter 语义不在本包裁决。

## Authority / Evidence

- `J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp:606-841`；
- dispatcher 顺序：同文件 `2118-2159`；
- `J:\QQFile\NTSD2.4\ntsd_release\include\game_world.h:13`：`MAX_OBJECTS = 400`；
- release build/live-path 参与性已由 R7-AI-02 inventory 闭合；C++ 目录保持只读。

## Position contract

| Position | helper | 关键合同 |
|---:|---|---|
| 17 | OID10/1 first | `Rand10` 在 range/PP 后；左朝向命中可 true 但 C++ 不写 DDA |
| 18 | OID10/1 frame271 | frame271 + target Y<0 + state12，无 RNG，写 DUA |
| 19 | OID10/1 predicted DUA | OID 命中后总先 `Rand10`；state16/8 只绕过失败门 |
| 20 | OID10/1 midrange | range 预筛；`Rand15` 命中则不取 `Rand4`，否则先取 `Rand4` 再查 target state |
| 21 | OID10/1 HP team scan | 条件短路后 `Rand20`；0..399 升序扫描，选严格最远；void side-effect 继续 |
| 22 | OID10/1 HP advantage | `self.hp > target.hp` 后先 `Rand70`，再检查 PP；void side-effect 继续 |
| 23 | OID9/2 predicted DDA | OID 命中后总先 `Rand10`；state16/8 只绕过失败门 |
| 24 | OID9/2 midfar | path-A `Rand13`；未命中后总取 dynamic modulus，再检查 X range |
| 25 | OID9/2 nearest DUA | `bestDist < 10000` 后先 `Rand30`，再检查 PP |
| 26 | OID32/19 midfar | path-A `Rand60`；未命中后总取 dynamic modulus，再检查 X range |
| 27 | OID32/19 close | X/Z strict range 后 `Rand15`，写 DRA/DLA |
| 28 | OID33/19/16 predicted DUA | OID 命中后总先 `Rand5`；state16/8 只绕过失败门；本包不接 production |

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ai/AiCharacterDecisionModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiCharacterDecisionPositions17To28EditorTests.cs`（new）

## Unknowns

- 本包不验证 positions 1–39 的联合遮蔽，只由 02F 联合验收；
- C++ runtime trace 仍 BLOCKED；
- >399 slot 没有 C++ authority 对应物，其 adapter 行为必须在 02F/R7 capacity 合同下单列验收；
- 真实角色 DAT / Play Mode 由 02F/R8 负责。

## Deliverables

1. `AiCharacterDecisionModule.TryEvaluatePositions17Through28`；
2. source-derived fixed-seed order/short-circuit/scan/side-effect/0 B tests；
3. compile、existing AI regression、fresh self-check、ledger 证据与 handoff；
4. 默认 dispatcher 与 02A red witnesses 保持原状。

## Verification

- generated C# build 与 fresh Unity compile 0 error；
- exact focused fixtures 全部 PASS；
- position 21 覆盖 400-slot 边界、strict farthest/tie、inactive/type/team/HP 过滤和 continuation；
- positions 21/22 side-effect 与 positions 23–28 early return 顺序可观测；
- existing 39-position contract/red witnesses 保持原状态；
- existing AI regression PASS；
- warmed module path 0 B；
- fresh-domain `BattleRuntimeSelfCheck` PASS；
- `Tools/Validate-ChangeLedger.ps1` PASS。

## Stop conditions

- 需要改变 outer gate、positions 1–16 或 29–39；
- 需要默认接线、移动现有 position 28 production helper 或改变 Legacy/DataOriented 输出；
- 需要新增 snapshot/runtime 数据字段；
- 需要替 positions 21/22 引入改变顺序的索引或缓存；
- 需要修改 C++、pass、profile、capacity、render 或 input binding。

## Out of scope

- default dispatcher integration（02F）；
- positions 29–37（02E）；
- position 28 现有 production helper 的移动/删除；
- input edges/cooldown 调用点移动；
- Play Mode/C++ runtime trace/R8。
