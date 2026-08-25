# HANDOFF — R3-FRAME-02A remove Unity-only ThrowFrameGuard readers

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R3-FRAME-002A-001`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改或写入 C++ authority。

## 已完成

- R3-FRAME-02只读 preflight确认：
  - C++ release field inventory有9个 `throw_frame_guard` occurrence，0 conditional reader、0 nonnegative writer；
  - C++ F03 / F07没有该 gate；
  - Unity有 F03、exact F07、fallback/shared F07三个 extra reader；normal production writer不存在。
- 先建立 Task Contract、Change Record、ledger和preflight研究文档，再删除三处 reader。
- 保留 field、默认值与所有 `-1` held-release / reset cleanup writer；仅将属性说明改为上述 C++ source事实。
- 新 self-check覆盖 matching test-only value：
  - exact/shared F03不再跳过 physics，`XInt=13`；
  - exact/shared F07不再跳过 counter，`AttackingCounter=1`；
  - field仍为0，证明本包没有抢占它的cleanup ownership。

## D-MOV-005 的明确处置

不改代码。C++ state2000 facing行为与 Unity fallback都存在；当前 assets内所有 literal `state:2000` 都是
type2/type4 weapon，exact ECS只接收 type0 character-DAT，所以 current exact route不可达。这是
`INFERRED` asset/reachability结论，不是永久规则：未来新增 type0 state2000 DAT或改变 exact eligibility时必须重开合同。

## 验证

| 检查 | 结果 |
|---|---|
| source/static | C++ guard reader/nonnegative writer=0；Unity production reader=0，production nonnegative writer=0。 |
| focused fixture | exact/shared F03 `XInt=13`；exact/shared F07 `AttackingCounter=1`。 |
| existing Unity Editor refresh/compile | UnityMCP port 6401，03:14:53 +08:00 idle/ready。 |
| full `BattleRuntimeSelfCheck` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，2026-08-22 03:15:33 +08:00。 |
| Change Ledger | `Tools/Validate-ChangeLedger.ps1` PASS（14 records / 12 governed code files）。 |
| diff hygiene | `git diff --check` exit 0；仅有既有 LF/CRLF warning。 |

## 未关闭 / 不得夸大

- 未执行 real held-throw / frame flow Play Mode；
- R1-WP02 C++ runtime trace仍 `BLOCKED`，不运行C++ executable；
- R3全部既有包仍为各自的 `RUNTIME_PENDING`，这不是完整 R3 或完整 battle alignment。

## 下一步（按 D-009 连续推进）

进入 R4 的只读 candidate-consume / hit source preflight。优先从现有静态差异中选择一个能以单一
attacker→multi-candidate fixture验收的分支，先闭合 C++ caller、field reader和Unity unified runner，再建立新的
Task Contract / Change Record；在此之前不修改 R4脚本。
