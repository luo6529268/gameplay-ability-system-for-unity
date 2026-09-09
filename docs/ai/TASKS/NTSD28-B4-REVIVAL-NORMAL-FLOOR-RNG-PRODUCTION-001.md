# Task Contract — NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001

> 状态：`VERIFIED / RED_0_OF_7 / FOCUSED_7_OF_7 / RELATED_59_OF_59 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXIT_AUDIT_NEXT`
> 依赖：`NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001 / VERIFIED`

## 目标

只闭合`lives>=2`的normal revival：复用C06相同本slot effective floor，按physical slot升序扫描同group type0
peers，保持Authority的`sumX!=0`分支、整数平均、同步RNG callsite/range/offset、precise-vs-integer坐标时点，
并精确写MP/HP/render phase/action/Y/Vy。

## Authority合同

- C07 caller只向active、当tick执行过physics的slot传递`floor_y`。当前playable flat floor为0；negative
  `collision_y_reference`替代flat floor。Unity C06已使用同一规则：`reference<0 ? reference : 0`。
- normal branch先`--revive_lives_30c`，再扫描所有active slots：排除self，只接收definition type0且
  battle group完全相同；累计integer X/Z和peer count。
- 原生分支判断`sumX!=0`，不是`peers>0`。成立时以C++整数除法算平均，随后同步RNG
  `0x90/mod 0x33`、`0x91/mod 0x1f`，偏移分别`-25/-15`。
- 该函数只写precise X/Z，不立即同步integer X/Z；后续C08 stage tail负责整数同步。
- 尾部只写current MP=500、effective max HP=base max HP、current HP=base max HP、render phase20、
  action212、integer/precise Y=floor、Vy=0。它不写base max MP或Unity legacy PPBound。

## Unity原状与允许修改

- `BattleRespawnModule.ApplyRespawnWithoutStoredCount()`按`count>0`触发旧`DeterministicRng`，偏移
  `-26/-16`，立即写XInt/ZInt，并固定Y=-300、额外写PPBound。
- 允许修改：
  - `Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs`
  - `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
  - `Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalNormalFloorRngProductionEditorTests.cs`及`.meta`
  - 仅为新合同纠正的既有normal-revival tests/probes；本Task/Change及治理恢复文档。

## 不变量与排除

- 不改route2 gate/branch、route3 queued continuation、terminal结果、C07 placement或OID998。
- 不改C06 physics、CollisionYReference producer、C08 stage sync、NativeRandom primitive或其他RNG callsite。
- 不在C07提前写XInt/ZInt；不把Transform/renderer/PS当逻辑真值。PS只保持precise mirror。
- 不改content、Scene、Prefab、ProjectSettings、Authority、对象容量或公开schema。

## Test-first验收

1. RED覆盖negative/nonnegative floor、same-group type0 filter、sumX nonzero与zero、no-peer、整数平均及
   precise X/Z更新但integer X/Z保留。
2. 同步RNG必须恰为callsite `0x90/0x91`、modulus51/31、两次调用；legacy `world.Rng`不得消费。
3. 尾部验证lives、HP/MP、render20、action212、Y/YInt/Vy，以及PPBound和queued字段保持。
4. 运行focused、B4 route2/3/C07/C25/physics/RNG相关回归、build、SelfCheck、目标Play、Console/Scene、
   Change Ledger validator；之后才进入B4 exit audit。

## 回滚

只恢复normal helper中的旧peer/RNG/position/floor/vitals写法并删除新增tests/SelfCheck；不得回退route2/3。

## RED证据（2026-09-09）

- focused v1实际`passed=0 / failed=7 / total=7`。
- nonzero/negative average均暴露legacy RNG与旧offset；`sumX==0`错误消费并移动。
- no-peer与三组floor case先暴露额外PPBound=500写入；旧实现还固定Y=-300并提前同步XInt/ZInt。

## 实施与验证（2026-09-09）

- normal helper保留physical-slot顺序与type0/group过滤，触发条件改为`sumX!=0 && peers>0`；平均仍使用
  C# int除法（与C++ signed truncation toward zero一致）。
- RNG从legacy `world.Rng`迁移到`world.NativeRandom.SynchronizedNext(0x90, 0x33)`和
  `(0x91, 0x1f)`，偏移改为`-25/-15`；sumX0/no-peer不消费任何流。
- X/Z只写Runtime precise与PS precise mirror，不在C07写XInt/ZInt；移除额外PPBound写。Y/YInt取
  `CollisionYReference<0 ? reference : 0`，PS Y与Vy mirror同步，Runtime Vy清0。
- 04:11:33 +08 focused v1为`7/7`；normal、queued、gate、C07、C25、type0 physics、collision-Y carrier、
  native RNG联合回归为`59/59`。
- fresh `Assembly-CSharp.csproj`为0 error/47 warnings；`Assembly-CSharp-Editor.csproj`为
  0 error/104 warnings。
- 04:15:53 +08真实`NTSD_Battle` Play通过7个逻辑case，覆盖sync callsite、三类floor、sumX zero/nonzero和
  integer defer。Console error=0；前后Scene dirty=false/root=13，SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`。
- 04:16:39 +08 full SelfCheck仍提前停在既有CPoint mode0 victim-Vz断言，未到更新后的B4 SelfCheck；
  focused/Play直接覆盖本包。
- 旧R8 death/respawn综合probe仍含NTSD2.4 synthetic countdown/旧整数时点，未作为本包证据；B4 exit
  audit必须以新2.8 joint trace取代它。下一严格route为`NTSD28-B4-REVIVAL-EXIT-AUDIT-001`。
