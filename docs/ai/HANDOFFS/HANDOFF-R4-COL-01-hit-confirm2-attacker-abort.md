# HANDOFF — R4-COL-01 C++ `hit_confirm2` attacker abort

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-COL-001`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、修改、构建或写入 C++ authority。

## 已完成

- 在 C++ `collision.cpp` 复核 C07-A：valid frozen candidate先过 vrest，再由
  `attacker.hit_confirm2 != 0 && target.char_data->obj_type == 0` 跳出该 attacker 的剩余 pair；
  label `next_attacker` 位于 pair loop 外，Loop1/Loop2共用。
- 在 Unity `BattleHitCandidateSequenceRunner` 中，仅在既有 target resolve和
  `CanConsumeRecordedCandidate` 成功后加入 current-DAT character target gate；返回既有 sequence-break
  值，不改 scheduler、candidate collect或每个 consumer。
- 新增 `CheckHitConfirm2AttackerAbortContracts`：
  - exact `LF2Character` / shared character-DAT fallback；
  - `HitConfirm2=1` 整 sequence abort；
  - `HitConfirm2=0` 两 candidate正常连续命中。
- 发现并修复 R3-FRAME-02A shared-DAT test fixture的两个独立 CS1061：
  `SelfCheckCharacterDatShell` 不具 `CurrentFrameId`，已最小改为 `Frame.N`。该修复在
  `R3-FRAME-002A-001` 留痕，现已重新为 `RUNTIME_PENDING`。
- 首次 full self-check暴露既有 held-kind5 fixture的未隔离前置：holder同 tick 会先合法命中 held light
  weapon并写 `HitConfirm2`，从而触发刚对齐的 C07-A。该 fixture现仅在 collect前设 test-only
  `holder.AttackExempt=1`，并断言 object consume前 weapon carrier为0；生产 gate没有豁免。

## 验证证据

| 检查 | 结果 |
|---|---|
| Unity scripts refresh/compile | existing Unity Editor / MCP port 6401，03:44:14 +08:00 ready；后续 `error CS` 为0。 |
| full `BattleRuntimeSelfCheck` | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 03:44:57 +08:00。 |
| Change Ledger | 待本 handoff/状态更新后的最后一次 validator重跑。 |
| diff hygiene | 待本 handoff/状态更新后的最后一次 `git diff --check`。 |

## 未关闭 / 不得夸大

- 未做真实 weapon/special attacker 多目标 Play Mode；
- R1-WP02 C++ runtime trace仍 `BLOCKED`，不得运行 C++ executable；
- 本包不是完整 R4或完整 battle alignment，只是 `D-COL-001` 的代码级闭环。

## 连续下一步（D-009）

不等待逐包确认，进入 `D-COL-002` 的只读 source preflight：只闭合 C++ caught-cpoint/hurtable gate
在 candidate consume 的位置、Unity shared runner已有/缺失的统一入口和最小 multi-candidate fixture。先建
Task Contract / Change Record，之后才改脚本；不得和 `D-COL-003` effect21、D-COL-005 kind1 target type、
R5 held/link合并。
