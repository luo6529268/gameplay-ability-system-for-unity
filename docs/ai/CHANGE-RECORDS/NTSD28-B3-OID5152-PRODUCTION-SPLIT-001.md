# NTSD28-B3-OID5152-PRODUCTION-SPLIT-001 — OID 51/52 production owner拆分

<!-- CHANGE-RECORD
id: NTSD28-B3-OID5152-PRODUCTION-SPLIT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Oid5152ProductionSplitEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOid5152MergeSplitPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan C12 advance_native_fusions and C25h advance_reaction_timers_slot, EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-4-CS1061 / C12-PREDECREMENT-FUSION / C25H-POST-FRAME-TIMER / DIRECT-COMPAT-PRESERVED / UNITY-COMPILE-0 / FOCUSED-10-OF-10-JOB-0A998F14 / RELATED-182-OF-182-JOB-1EC53FDC / SELFCHECK-2026-09-05T00-14-06-PASS / REAL-PLAY-OID7-8-51-4503-TICKS-PASS / COOLDOWN-4499-TO-0-THEN-NEXT-C12-SPLIT / MERGE-TIMER-4499 / SPLIT-TIMER-899 / SPLIT-PP5 / RESULT-SHA-D003E910 / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-29-PARTIAL-4 / NEXT-FIRST-DIFF-FRAME-MOTION-VS-EARLY-FRAME-ADVANCE / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C12-FUSION / C25H-TIMER / REAL-PLAY-PASS`

## 改前事实

Unity production 在 C03 后、FrameMotion 前调用 combined maintenance；它对每个 slot 先递减
`Unk338`，再执行 merge/split，并在 input-clear partial 中也运行。修复版权威在 C12 先用未递减 timer
执行 fusion，随后仅在 C25h 的每 slot frame 后递减 timer。因此 Unity timer=1 会提前一 tick触发，
并把新写入的 4500/900 少递减一次。

## 计划

先建立失败测试，再把 production 分成 C12 fusion scan 与 C25h timer writer；保留旧 combined direct
入口，不改 merge/split 内部行为。

## 验证

- test-first：新测试引用尚不存在的C12/C25h seam，Unity产生4条预期`CS1061`。
- 实现后Unity compile 0 error；新production/actual-phase focused `10/10` PASS，job
  `0a998f140008418db74127c475066d84`。
- OID、late lifecycle、frame snapshot、C01/C02/C03、combo、OPoint、architecture与worker相关回归
  `182/182` PASS，job `1ec53fdc04b8442ca571bfe735f0336b`。
- SelfCheck纠正三类旧combined顺序断言：NeedClear partial不减timer；F1 wait在C25h前返回时冻结timer；F2
  完整单步才递减。最终`2026-09-05 00:14:06 +08:00`写出`PASS`。
- 真实Play先暴露并纠正两项旧探针预期：C12位于physics后，merge midpoint为post-physics值；split后同tick
  C25 resource tail把双方PP从0恢复到5。最终OID7/8/51生产探针`4503` tick PASS：merge timer4499，
  4499次drain后timer0 tick不split，下一C12 split，tick末self timer899、双方frame113/PP5；dormant、
  slot generation、central suppression/restore、RNG restore与cleanup全部通过。结果SHA-256
  `D003E910D42D562A60B18914C5E40365CBFCECDA6DD245484D7060FA316A2218`。
- Play已退出；`NTSD_Battle.unity` SHA-256
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、mtime不变；Console 0 error。
- actual full occurrence保持29，input-clear partial由5降为4；下一首差为
  `expected CoreFrameMotion / actual EarlyFrameAdvance`。

## 未关闭边界

- FrameMotion、physics 与 C25 其余 nested steps 仍属于后续 B3 包。
- OID 51/52 的内容/资源权威策略不在本包内。
