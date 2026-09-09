# Task Contract — NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001

> 状态：`VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 validation support`  
> 建立日期：2026-09-03

## 目标

修正 `R3-AI-TGT-01` 在 AI production 已切换到 NTSD 2.8 synchronized RNG 后仍比较
legacy `world.Rng` 的旧验收合同。保留 target/input 可观察行为检查，同时分别验证 legacy profile
推进 legacy RNG、IndexedCanonical 只提交 `NativeRandom` 且不改 legacy RNG。

## 证据、范围与验收

- red：2026-09-03 11:29:43 full SelfCheck 在首个 cache-retain 场景失败；两边 target 都为7，
  唯一失败量为 `legacyRng=(1976090332,7)` 与 `indexedRng=(7,0)`。
- 原因：`NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001` 已按2.8双流权威让
  IndexedCanonical accepted witness提交 `world.NativeRandom`，不再恢复占位 legacy RNG。
- 唯一脚本：`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；production零改动。
- fixture必须同时按seed初始化两个RNG owner；不得用 legacy `DeterministicRng` 的 remainder
  推断 synchronized stream 的值。
- 验收：focused/full SelfCheck通过、Console0、原生产包focused回归不退化、Ledger通过。
- 若出现下一处旧断言或真实生产差异，停止并独立登记，不在本包顺手扩大范围。

## 回滚

只恢复 `R3-AI-TGT-01` 旧测试比较；不回退 production synchronized RNG 接线。

## 结果

- cache retain/refresh fixture现在寻找legacy与synchronized首个`0x13` remainder一致的seed，且同时
  seed两个world RNG owner。
- target/input仍做跨profile可观察比较；legacy必须推进旧RNG，IndexedCanonical必须保持旧RNG不变并
  推进NativeRandom。
- 2026-09-03 11:36:54 full SelfCheck PASS；清理后Console 0 error；production零改动。
