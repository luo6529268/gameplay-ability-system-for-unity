# NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001 — AI RNG owner assertion rebase

<!-- CHANGE-RECORD
id: NTSD28-B2-SELFCHECK-AI-NATIVE-RNG-SEAM-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan dual RNG ownership plus accepted-only IndexedCanonical synchronized cursor commit.
evidence: RED-FULL-SELFCHECK-2026-09-03T11:29:43 / TARGETS-EQUAL-7 / DUAL-SEED-FIXTURE / LEGACY-OWNER-ADVANCES / INDEXED-NATIVE-OWNER-ADVANCES / INDEXED-LEGACY-UNCHANGED / FULL-SELFCHECK-2026-09-03T11:36:54-PASS / CONSOLE-0 / PRODUCTION-UNCHANGED
-->

> 状态：`VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`

## 改前事实

- `AssertAiTargetInputProfilePair`要求legacy与IndexedCanonical结束后的`world.Rng` state/calls相等。
- IndexedCanonical现在按权威提交`world.NativeRandom`，并有意保持`world.Rng`不变；旧断言因此失败。
- fixture只seed旧RNG，没有同步seed NativeRandom，无法表达双流可重复性。

## 预期改后职责

- legacy与indexed各自验证正确RNG owner，target/input仍检查可观察结果。
- indexed fixture同时seed NativeRandom；shadow/oracle不提交合同不变。
- 不修改任何production脚本。

## 验证记录

- red：2026-09-03 11:29:43，target均7，旧RNG equality断言失败。
- test-only实现：fixture同时seed NativeRandom；用两个实际算法寻找共同首个cache remainder seed；
  target/input继续跨profile比较，另分别断言正确owner推进与indexed legacy owner不变。
- Unity compile 0；2026-09-03 11:36:54 full SelfCheck PASS；Console 0 error；production零改动。
