# NTSD28-B2-AI-HELD-RNG-001 — held synchronized RNG sites 0x28..0x35

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-HELD-RNG-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiHeldRandomEditorTests.cs
authority: NTSD 2.8-Logan NativeAi28::step_held_object sites 0x28..0x35 in playable step_main closure.
evidence: PRE-CODE-AUTHORITY-STEP-HELD-CLOSED / TEST-FIRST-1-EXPECTED-CS0117 / FOCUSED-19-OF-19-JOB-FEAA950AF1A446D4B9A4008141EEB18A / ALL-AI-361-OF-361-JOB-323E390959514D0DBE07153E64109F87 / FULL-CANDIDATE-14-3C-1C-28-HELD-DECISION / ALL-14-HELD-EXPRESSIONS / ALL-40-LIVE-IDS-CANDIDATE-READY / LINE-BLOCKER-SUBJECT-GROUP / THRESHOLDS-115-300 / STATE17-RENDER-PHASE / COMBO-INDEX4-5 / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T10-56-19 / CONSOLE-0 / LEDGER-126-RECORDS-75-FILES / LEGACY-CRT-PRESERVED / PRODUCTION-CURSOR-UNCONNECTED
-->

> 状态：`FOCUSED_TEST_PASS / HELD_SITES_28_35_READY / ALL_40_LIVE_IDS_READY / PRODUCTION_UNCONNECTED`

## 改前事实

- synchronized non-held已到达Complete，但`LinkState>0`仍进入 legacy `ProcessHeld`并在首个无site
  `Rand(int)` fail closed。
- legacy helper在验证linked slot前消费首个RNG；line blocker错误要求selected target与subject同组，
  还用10000/5000范围、HitStop和MoveMode替代authority的115/300、render phase和全局默认锁0。
- 最终非direct分支错误写 `ComboDrj/Dlj`，authority实际写combo index4/5，即`ComboDda/Ddj`。

## 预期改后职责

- synchronized cursor专用helper逐分支实现14个site与native返回语义；legacy helper原样保留。
- 仅valid linked entity进入0x28；line cover按subject battle group扫描前20 slots。
- weapon-run branch按StageZ/width、Y、behavior-range和combo index执行；不接production cursor。

## 验证记录

- test-first refresh：唯一错误为缺少 `ProcessNativeHeld`，无其他编译错误。
- `ProcessHeld`只在 snapshot 带 synchronized cursor时分派到新 helper；legacy body保持原样。
- valid linked entity检查位于`0x28`前；line cover按subject RelationTeam扫描前20 slots，enemy selected
  target + ally blocker夹具通过。
- ordinary weapon使用strict `<115/<6`，150/151使用strict `<300/<6`；2B、2C和所有weapon-run
  sites按原生短路消费。
- state17门改读`rows.Y`对应render phase；最终右/左分支分别写`ComboDda/Ddj`对应native
  combo index4/5，不再写旧`Drj/Dlj`。
- focused `feaa950af1a446d4b9a4008141eeb18a` 19/19；全AI
  `323e390959514d0dbe07153e64109f87` 361/361；full candidate `14→3C→1C→28`
  到达HeldDecision；4096 zero-allocation。
- SelfCheck 10:56:19 PASS；Console清理后0 error。
- Change Ledger validator PASS：126 records / 75 governed code files。

## 未关闭项

- production AI snapshot尚未捕获 NativeRandom synchronized cursor，witness也尚未由唯一accepted
  canonical commit；当前仅测试显式启用。
- native input producer/combo/action仍有迁移包，joint trace未执行。
- stage bounds异常缺失语义与G-04完整mode矩阵仍由后续B8/B12治理。

## 2026-09-05 correction

`NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001`用Y/phase互异source-model/Unity raw证据将Authority `render_phase_008`唯一绑定到`Runtime.HitStop`。本记录中“Y对应render phase”的旧判断被新证据取代；当时测试只证明旧实现自洽，不能继续裁决current Authority consumer。后续必须单独迁移仍读取Y的AI render-phase分支，不能篡改本包历史结果。
