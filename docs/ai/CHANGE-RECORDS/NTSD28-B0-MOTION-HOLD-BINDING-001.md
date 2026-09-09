# NTSD28-B0-MOTION-HOLD-BINDING-001 — motion hold / FrameDelay diagnostic binding

<!-- CHANGE-RECORD
id: NTSD28-B0-MOTION-HOLD-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28::motion_hold_timer signed countdown, physics/frame gates, hit/cpoint/holder/respawn writers; Unity NTSDEntityRuntime::FrameDelay and corresponding production chain.
evidence: AUTHORITY-AND-UNITY-SOURCE-CHAIN-CLOSED / SIGNED-COUNTDOWN-AND-WRITER-SET-MATCH / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / POSITIVE-3-AND-NEGATIVE-5-PROJECTION / REAL-MOTIONHOLD-DIFFERENCE-CLOSED / MATURITY-28-10-9 / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

## 改前事实

- B0 contract 将 `combat.motionHoldTimer` 标为 MISSING，Unity raw 输出 `null`。
- 权威字段在 physics 前每 tick 向 0 推进并早退，frame/rest/armor/finalize 同样受其 gate；hit/cpoint 写入
  3、-3、-5、2，holder relation 会镜像该值，revive 会写 10。
- Unity `FrameDelay` 在 `TryEnterReleaseFrameAdvanceAfterDelay` 中以相同正负规则向 0 推进；物理、frame、
  postprocess、opoint、cpoint 与 holder gate 使用同一字段，hit/cpoint/respawn writers 使用对应值。
- `hitReactionTimer` 是另一套受击反应计时，不能复用本字段。

## 计划改动

- contract 将 `combat.motionHoldTimer` 绑定 `NTSDEntityRuntime::FrameDelay` 并晋级 VERIFIED。
- Unity raw projection 输出 live `runtime.FrameDelay`。
- focused tests 覆盖 +3 与 -5；neutral raw 关闭默认 0 的 missing 差异。
- 不改任何生产 writer、运行时 reset、DAT、资源或 Scene。

## 验收与回滚

- 验收标准见对应 Task Contract。
- 回滚只恢复 contract/projection/tests 与 27/10/10 maturity。

## 实际结果

- `combat.motionHoldTimer` 现为 VERIFIED，读取 live `runtime.FrameDelay`；maturity 28/10/9。
- 工具 build 0/0、21/21、5/5、format PASS；contract SHA
  `52A0284F9F125B47DEA86D634F0900602A578412681683367F33B5A94C28AEF2`。
- Unity fresh compile 0 error；job `0f6ee3e497964378b1d2b8d9b7c141b8` 6/6 PASS，正值3与负值-5
  均被真实投影测试覆盖。
- 新 raw SHA `362F1811010FABB0DE83731DED20F3A716B87B6B3476E22E193B57A697C09D95`；
  comparison 为3 ticks / 6 pairs / 282 fields、11 unique differences、36 equal、66 difference occurrences。
- 生产逻辑、DAT、资源、Scene 与 authority 均未改。
