# Task Contract — NTSD28-B3-C23-C24-WORLD-CLOCK-001

> 状态：`VERIFIED / C23-C24-SINGLE-OWNER / SNAPSHOT-CHECKSUM-CLOSED / TARGETED-PLAY-PASS / C25-PENDING`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C23-C24`
> 依赖：`NTSD28-B3-C21-C22-PLACEMENT-001 / VERIFIED`

## 目标

建立Authority C23/C24的Unity生产single owner：C23将native resource phase12/phase3各推进一次，C24将native frame sequence推进一次；两者在C22后、临时C25 serial proxy前提交。

## 架构边界

- 共享`BattleFlowRuntimeState`来自Server受治理package，当前Server队列仍有独立authority/phase约束，本包不跨路线修改它。
- 新增Unity battle-runtime自有`NTSD28NativeWorldClockState`，纳入reset、core snapshot/restore、checksum和parity snapshot，不能只增加空phase。
- 不提前实现C25资源、display、frame、opoint或lifecycle消费者；现有`FrameMod12`/`CurrentTickIndex`语义不改。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Runtime/NTSD28NativeWorldClockState.cs`（新增及meta）
- `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- C06～C22 placement tests（仅phase index/count）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockPlayModeProbeEditor.cs`（新增及meta）
- snapshot/checksum/parity focused tests及`BattleRuntimeSelfCheck.cs`（仅新字段闭包/phase契约）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

## 验收

- 初值/Reset为phase12=0、phase3=0、sequence=0。
- 每个完整生产tick在C23/C24各推进一次：首tick1/1/1；12 ticks后0/0/12。
- input-clear partial tick和被拒绝tick不提交C23/C24；F1完整core路径提交时钟，但C25 late仍可被step-wait冻结。
- phase顺序C21→C22→C23→C24→serial proxy；full/partial为34/4。
- snapshot/restore/checksum/parity均覆盖新状态；compile、focused、SelfCheck和Play通过。

## 回滚

移除新carrier、snapshot/checksum字段和C23/C24 caller/phase；不得回退C01～C22。

## 完成证据

- test-first compile red为20个缺失成员/phase诊断，证明Unity原先没有C23/C24 carrier与owner。
- 新增`NTSD28NativeWorldClockState`：phase12、phase3、frame sequence；生产C23/C24位于C22后、临时C25 proxy前。
- 新状态已纳入Runtime Reset、core snapshot 6、full snapshot 8、checksum 11、restore与parity snapshot。
- focused job `3ba28aa75dcb4f3e81da495ae657846c`为14/14；snapshot/restore/checksum/lockstep job `18c0280d17a14490ad9603f26a5a57a5`为78/78；C04～C24/actual job `8d4d747fa977495c8b838832fdc1142d`为51/51。
- SelfCheck 2026-09-05 06:01:19 +08 PASS。
- 真实Play tick6：phase12 5→6、phase3 2→0、sequence 5→6；phase24～27=C22/C23/C24/serial proxy；artifact SHA-256 `186F7F0243538008F1D7B74960A736DC650239E632C7FB5B161E2E4C818D05F3`。
- Play退出、Console 0、Scene SHA/mtime不变；full/partial为34/4。下一结构首差仅剩C25 nested live-slot tail（C26及后续session/post仍在其后）。
