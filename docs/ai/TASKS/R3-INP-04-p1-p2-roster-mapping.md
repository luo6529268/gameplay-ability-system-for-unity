# R3-INP-04 — P1/P2 authority fixture and Unity roster-extension boundary

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、static、Unity scripts compile 和 focused self-check 均已通过；
> physical binding / C++ trace 保持独立未关闭。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R3。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。  
> 执行方式：按 `D-009` 连续推进；脚本修改前已建立 test-only `R3-INP-004-001` Change Record。

## Goal

只关闭 `D-INP-004` 的 authority fixture 边界：验证 Unity roster player slot 0/1 严格把各自的
canonical frame input 送至 runtime slot 0/1 的 P1/P2 entity，且不串键、不会把 P1 的 input 写给 P2
或反之。

Unity 的 8-slot roster 是已批准扩展，不等同 C++ authority 的 P1/P2 上限；本包只证实 2-human authority
夹具，不把 3+ player 作为 C++ rule。

## Scope

允许仅：

1. 只读核对 C++ P1/P2 runtime identity和 Unity roster resolver；
2. 只在 `BattleRuntimeSelfCheck.cs` 添加 P1=runtime slot0、P2=runtime slot1 的 test-only packet routing
   fixture；
3. 运行 static、ledger、compile与 self-check后留下记录。

禁止改动：production capacity、`BattleRosterRuntimeState` array length、`SimulationFrameInputModule` routing、
pool slot allocation、AppManager spawn、InputAction asset、AI、worker、lockstep protocol、C++、scene/DAT/render。

## Authority / Evidence

- **C++ source — VERIFIED**：`src/core/main.cpp:2379-2380` 在 battle scene 获取
  `world.get_entity(0/1)` 为 P1/P2，`main.cpp:4607-4608` 只 poll active P1/P2；
- **Unity source — VERIFIED**：`BattleRuntimeState.cs:186-225` 保留 eight-slot roster extension；
  `SimulationFrameInputModule.cs:42-69` 以 FrameInput player slot 解析 bound human entity；
  `SimulationWorld.Passes.partial.cs:130-151` 以 runtime slot order poll bound human；
  `AppManager.cs:196-255` 将 match roster index 的 spawned entity runtime slot/stable id 回写 roster；
- **Adapter boundary — DECIDED**：D-007 保留 Unity capacity extension；本 task 的 P1/P2 fixture 是
  authority subset，不裁决 3+ player。

## Acceptance

| 层级 | 最小验收 | 初始状态 |
|---|---|---|
| S0 source | C++ P1/P2 runtime slots 0/1 和 Unity roster player slots 0/1 mapping 已定位。 | `PASS` |
| S1 routing fixture | P1 right 与 P2 jump 同 tick 各写入正确 entity，而非互串。 | `PASS` |
| S2 identity fixture | roster runtime slot/stable id 与 P1/P2 entity 保持各自绑定。 | `PASS` |
| S3 extension boundary | 只记录 3+ 为 Unity extension，且不改变 capacity。 | `PASS` |
| S4 compile/self-check | static、ledger、Unity compile、`error CS`、full self-check。 | `PASS` |
| S5 Play Mode | 两个物理 input map / asset。 | `OUT_OF_SCOPE / R3-PHY-01` |
| S6 C++ trace | P1/P2 same input trace。 | `BLOCKED / R1-WP02` |

## Stop conditions

停止并建立新 Record，若 fixture 需要改 production roster/capacity、pool allocation、input provider、
physical bindings、AppManager spawn或C++。3+ player 的行为只能作为 Unity extension diagnostic，不能被
解释为C++ authority mismatch。

## Out of scope

R3-INP-03A以外的 packet semantics、D-INP-005/R3-AI-TGT-01、D-INP-006/R3-PHY-01、所有 D-MOV、
R4～R8、R1-WP02 trace、T8 default `stage.dat`、服务器/Android。

## 实际验证结果（2026-08-22）

- test-only fixture 固定 P1/runtime slot0/roster player0 和 P2/runtime slot1/roster player1；同 tick
  `P1=Right`、`P2=Jump` 后，P1 只得到 right/cdRight/history6，P2 只得到 jump/cdAttack/history5；
- fixture 同时确认 roster slot0/1 的 `RuntimeSlotIndex` / `StableId` 与对应 entity 保持绑定，
  `ActiveSlotCount=2`；
- `Tools/Validate-ChangeLedger.ps1`：PASS（9 records、10 governed code files）；
- `git diff --check`：exit 0（仅现有 LF/CRLF warning）；
- UnityMCP force scripts refresh / compile 成功，filtered `error CS` 为 0；
- `NTSD/验证/运行战斗运行时自检`：`Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`，
  文件最后写入为 `2026-08-22 01:28:23 +08:00`。

该结果仅覆盖 C++ authority 的 two-human fixed-slot fixture；不验证实际 InputAction asset、P1/P2 physical
mapping、3+ roster gameplay、C++ executable trace或完整场景。
