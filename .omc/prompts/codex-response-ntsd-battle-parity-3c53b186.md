---
provider: "codex"
agent_role: "architect"
model: "gpt-5.6-sol"
prompt_id: "3c53b186"
timestamp: "2026-07-17T05:35:07.580Z"
---

我会按代码审查处理，并遵循仓库的 delegation-first 规则：先委派两路只读审查，一路聚焦输入时序与结果态，一路聚焦 trace/certificate 边界；随后再做一次独立架构核验。当前任务只读，不会修改文件。
仓库搜索在尝试访问权威盘符时触发了沙箱 ACL 拒绝，我会把外部权威源码与仓库内检查拆开。Unity 侧仍可完整审查；权威侧若持续不可读，将严格以你提供的差异和已给出的调用顺序为证据边界，不会把推断写成已确认事实。
输入差异已经收敛到一个明确契约问题：权威在进入 `GameTick.Run` 前完成“上一状态滚动、完整 held 快照写入、冷却递减、边沿生成”，而 Unity 把同一组操作放在 `RunReleaseTick` 内部。这个位置差异会与首帧 `NeedClearInput` 早退叠加，正好解释 tick 2 的 `PrevLeft=0/CdLeft=5` 对比权威 `1/4`。我正在核对现有 self-check 是否覆盖了 driver 级别而不仅是模块级别。
目前有两个会直接影响结论的高优先级事实。第一，tick 2 的输入差异来自 authority trace runner 漏传生产 Host 的 `MarkHumanInputPolled` 回调，不能据此把 Unity 改成双轮询；第二，当前 comparator 只凭 `full/full` 就授予 certificate，完全忽略 Unity header 中的 `dataFixture`，因此 diagnostic trace 确实可能被误认成生产证书。接下来做最后一轮独立架构核验，重点检查这两个门禁结论是否自洽。
独立核验正在确认最后两个边界：`Phase/Winner` 不是 UI-only，因为 `ActivateSummary` 写入后会让下一 tick 跳过正常战斗；而 `CameraX` 虽主要是表现态，权威仍在 F8 掉武器分支读取它。因此相机 profile 可以用于当前无 F8 的战斗场景，但不能冒充无条件的完整模拟等价证书。
**Gate: FAIL**

The proposed iteration must not proceed against the current tick-2 trace. That trace contains a runner defect, and the certificate boundary can currently promote diagnostic data to a production certificate.

**Prioritized Findings**

1. **P0: tick 2 is not a valid production-authority difference.**

Authority production polls human input exactly once:

- `SimulationTickDriver.ApplyFrameInput` polls before `GameTick.Run` at `J:\...\SimulationTickDriver.cs:42-49`.
- `PollHumanInput` rolls previous state, writes held state, decrements cooldowns, then creates edges at `J:\...\InputRuntime.cs:611-624`.
- `GameTick` resets `HumanInputPolledExternally` at `GameTick.cs:42`; the production Host passes `MarkHumanInputPolled` at `BattleHostRuntime.cs:193-196`, preventing the fallback poll in `CharacterLogic.cs:17-20`.

The parity runner omits that callback at [AuthorityTraceCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/AuthorityTraceCommand.cs:90). It therefore polls twice:

- First poll: left edge produces `PrevLeft=0`, `CdLeft=5`.
- Fallback zero-input poll: rolls to `PrevLeft=1`, decrements to `CdLeft=4`, releases left, and suppresses movement.

That exactly explains the reported authority values. **Do not change Unity to reproduce `PrevLeft=1/CdLeft=4`.** Repair the authority trace runner, regenerate the authority trace, and establish a new first difference.

2. **P0: Unity still needs a production-grade exactly-once input boundary.**

Unity currently queues the frame packet before the tick at [SimulationTickDriver.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs:195), but drains it inside the post-cooldown phase at [LF2Character.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs:748). [NTSDInputStateModule.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs:73) currently combines polling, cooldowns, edges, and action resolution.

The correct contract is:

1. Check readiness; a stalled lockstep tick must mutate nothing.
2. Fetch the frame packet once and overlay its seven held states after local callbacks.
3. Pre-poll every eligible human character-DAT entity exactly once.
4. Mark the tick as externally polled.
5. In the existing post-cooldown phase, fallback-poll only for direct/legacy callers, then resolve combo/direct/release actions once.
6. Clear the marker after the tick.

This supports `LocalFreeRun`, `LockstepBuffered`, `Manual/replay`, and direct tick-system tests without duplicate edges. Packet precedence is already deterministic in [SimulationWorld.FrameInput.partial.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs:19).

Implementation hazards:

- Pre-poll must sync `LF2Character.InputState` to `Runtime` without running combos.
- The transformed character-DAT shell path at [LF2Entity.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:2247) never calls `Runtime.ApplyInputEdges()`.
- Battle-entry clearing resets only `Runtime` at [SimulationWorld.Passes.partial.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs:66), leaving duplicate local input state potentially stale.

3. **P0: diagnostic traces can currently become production certificates.**

Unity emits `dataFixture` at [BattleParityTraceEditor.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs:334), but [TraceCompareCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/TraceCompareCommand.cs:169) never validates or compares it. Certificate eligibility is only `full/full` at line 164.

Worse, diagnostic mode substitutes the authority manifest hash at [BattleParityTraceEditor.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs:448). A matching diagnostic trace can therefore report generic `equal` with `certificateEligible=true`.

Production certification must bind three values: authority manifest, runtime-loaded manifest, and deployed Unity manifest. Diagnostic execution may compare behavior, but must remain certificate-ineligible. Also, any status beginning with `equal` exits zero at [TraceCompareCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/TraceCompareCommand.cs:57); CI needs an explicit required certificate class rather than exit zero alone.

The existing production manifest mismatch remains correctly hard-rejected before tick comparison. Current evidence shows `ticksCompared=0`, `header/manifest`, and unequal hashes in [compare-v3-full-final.json](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Temp/NTSDParity/compare-v3-full-final.json:7).

4. **P1: a named camera profile is acceptable, but not as exact parity.**

A `fixed-world-camera-v1` profile may normalize exactly these four duplicated fields:

- `world.cameraX`
- `world.cameraVel`
- `world.runtime.stage.cameraX`
- `world.runtime.stage.cameraVel`

It must validate each raw trace and its reported hashes before normalization, require all four fields, and retain `cameraMaxOverride`, bounds, stage state, every entity/slot field, `renderOffsetX`, results, RNG, and events.

The result must use a distinct profiled certificate class. Authority reads `CameraX` in the F8 weapon-spawn branch at `GameTick.cs:724-744`, so this is not unconditional full-simulation equivalence. It is defensible for current v3 scenarios where F8 is unavailable and false.

5. **P1: real battle-results runtime is required.**

Unity currently emits constants at [BattleParitySnapshot.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:668).

For tick-2 parity, the immediate fields are `HadBoth`, `TeamCount`, `TeamIds[2]`, `BattleEndPhase`, and `PendingWinner`. Production combat lifecycle also requires `Phase`, `Winner`, and `Timer`: authority `ActivateSummary` sets `Phase=200`, after which normal combat stops. `PendingHostAction` may remain inert because it belongs to results-menu behavior.

`UpdateBattleResultsFlow` must be copied exactly from authority `GameTick.cs:529-605` and run after `EntityPostFrameTailAll`. Results-menu navigation can remain excluded. The alignment document is stale where it excludes this combat-affecting pass at [csharp-vs-unity-battle-alignment.md](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md:76).

**Required Tests**

Add full-driver coverage for all drive modes, lockstep-not-ready immutability, held/release edges, packet precedence, one-time combo resolution, transformed shells, battle-entry reset, and provider call count. Full trace currently calls `GetFrameInput` a second time at [BattleParityTraceEditor.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs:194).

Comparator tests need full production and diagnostic traces, certificate-class assertions, camera-only and non-camera changes, stale raw hashes, unknown profiles, manifest mismatch under every profile, and CLI exit behavior. Current self-tests only construct compact traces at [TraceCompareSelfTestCommand.cs](I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Tools/NTSDParity/TraceCompareSelfTestCommand.cs:55).

This was a read-only review; no files were modified and no Unity runtime validation was run.