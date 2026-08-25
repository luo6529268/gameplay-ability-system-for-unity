# R7-PERF-001 — retire stale PreInteraction cross-pass proof

<!-- CHANGE-RECORD
id: R7-PERF-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Test/Editor/PreInteractionNoOpProofEditorTests.cs
authority: J:/QQFile/NTSD2.4/ntsd_release/Makefile:11-35;src/entity/game_tick.cpp:659-664,1818-1825;src/entity/cpoint.cpp:23-190;src/entity/weapon.cpp:13-107
evidence: SOURCE-CONFIRMED-STALE-CONTENT-SKIP / FRESH-UNITY-COMPILE / FOCUSED-15-OF-15 / WARMED-0B / FULL-SELFCHECK-PASS
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING`

## 1. Authority / requirement

C++ T14在object consume之后读取当时current/Prev2 CPoint与held/link；Unity death-cleanup checkpoint的
cross-pass cache无法证明中间writer未改变同slot内容。必须退役该cache，不能用不完整content epoch掩盖。

## 2. Unity before

- death-cleanup循环对每个participant计算neutral proof并保存结构epoch；
- T14优先以这些结构量消费cache并直接return；
- 中间frame/CPoint/link writer没有invalidator；
- stale same-slot内容可绕过C++应执行的kind2 mismatch fallback。

## 3. Planned changes

| 文件 | before | after |
|---|---|---|
| `SimulationWorld.Passes.partial.cs` | checkpoint发布+T14消费cross-pass proof | 不发布、不消费；同点proof仍是唯一whole-pass fast path |
| `PreInteractionNoOpProofEditorTests.cs` | neutral test要求cross-pass used；无content-stale矩阵 | cross-pass始终false；kind2 stale-content与legacy oracle等价 |

兼容边界：stress/report public toggle与last-used属性保留，避免本包扩大schema；前者不再影响production，
后者每次T14为false。

## 4. Acceptance

- neutral checkpoint之后`CrossPassUsed=false`、`WholePassProof=true`、writer count 0；
- same-slot current kind2变化后执行fallback，写frame212、Vy=-3、Y=-2并与forced legacy checksum/RNG一致；
- occupancy/non-neutral/generation/derived/fail-closed tests继续通过；
- warmed same-point neutral proof为0 B；
- fresh compile/focused/full self-check/validator/diff通过；
- PlayMode/C++ trace缺失时最高`RUNTIME_PENDING`。

## 5. Protected boundaries

不改pass order、CPoint/weapon writer、R6 presentation、CentralOnly、capacity、30Hz/FrameInputSet、SoA/ECS、
pool/worker/0-GC、scene/DAT、T8或C++。

## 6. Rollback

只回滚本Record两份脚本内diff与关联文档；不触碰其它用户修改。

## 7. Actual changes / verification

- `PostFrameAdvanceDeathCleanupAll`删除checkpoint neutral proof计算与private proof发布；respawn/death cleanup
  loop与runtime refresh顺序不变；
- `PreInteractionTickAll`删除cross-pass consumer，仍先尝试T14同点
  `TryProveWholePreInteractionPassNoOp`，证明失败后进入原three-pass writer；
- 删除private cross-pass storage与helper；public stress/report兼容toggle及last-used属性保留，last-used每次
  T14重置为false且production不再读取toggle；
- focused Editor test把neutral checkpoint改为same-point proof vs forced legacy，并新增checkpoint后不改变
  slot/generation、只把current frame改成kind2的stale-content oracle矩阵；
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:quiet`：exit 0，0 error / 18
  existing dependency warnings；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo --verbosity:quiet`：exit 0，0 error /
  18 existing dependency warnings；
- `Tools/Validate-ChangeLedger.ps1`：43 records / 31 governed code files，PASS；scoped
  `git diff --check`：PASS（仅LF→CRLF提示）；
- 用户恢复现有UnityMCP session后，`~/.unity-mcp/unity-mcp-status-b1b02287.json`持续心跳并指向
  本项目、Unity 2022.3.62f3与port 6401；Codex通过legacy stdio bridge实际双向读取Console成功；
- fresh Unity compile已确认：source为20:02:43/45，`Assembly-CSharp.dll`为20:16:44、
  `Assembly-CSharp-Editor.dll`为20:16:45，均晚于source；Editor log无`error CS`；
- focused EditMode job `09948d3e3e314d84ab80791d0d2b2070`：
  `PreInteractionNoOpProofEditorTests` 15/15 PASS，0 failed/0 skipped；同批实际覆盖
  `PostFrameCheckpoint_SameSlotKind2ContentMatchesFullScanOracle`、
  `WarmedKind1Writer_AllocatesNoManagedMemory`与
  `WarmedNeutralWholePassProof_AllocatesNoManagedMemory`；
- 菜单`NTSD/验证/运行战斗运行时自检`实际执行，
  `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 20:22:37 +08:00写入`PASS`；
- Console中的两条Error是full self-check故意触发的rest-binding negative control，不是编译错误或本包失败。

## 8. Resolved blocker / remaining evidence

- `B-R7-PERF-001-01`已解决：用户执行Refresh并恢复UnityMCP session后，fresh DLL、focused 15/15、
  warmed 0 B与full self-check均取得本包新证据；旧19:49:12 PASS没有被复用；
- 本包仍缺真实战斗Play Mode与可用C++ runtime trace，所以最高只能是`RUNTIME_PENDING`，不能写成
  C++ runtime完整`VERIFIED`；
- 后续不得恢复stale cross-pass cache；若重新引入content epoch或调整pass order，必须建立新Change ID。
