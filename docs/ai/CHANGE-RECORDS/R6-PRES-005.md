# R6-PRES-005 — hit-record RenderDispatch writeback

<!-- CHANGE-RECORD
id: R6-PRES-005
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:/QQFile/NTSD2.4/ntsd_release/Makefile:11-35;src/entity/game_tick.cpp:2061-2083;src/render/renderer.cpp:687-758;src/entity/collision.cpp:457-490
evidence: SOURCE-RENDER-WRITEBACK-VERIFIED / UNITY-LATE-OR-ACK-TIMING-DIFFERENCE / FRESH-UNITY-COMPILE-AND-FULL-SELF-CHECK-PASS
-->

> 创建日期：2026-08-22  
> 最后更新：2026-08-22  
> 状态：`RUNTIME_PENDING`

## 1. Authority / requirement

C++每tick在render callback内推进valid age或移除一个invalid tail；next tick collision的10槽gate可
影响两次全局RNG消费。Unity当前依赖LateUpdate/worker acknowledgement，no-publication replay不会建立
cycle。本Record只恢复RenderDispatch writeback时点。

## 2. Unity before

- BeginFrame冻结cycle但不立刻写live entity；
- main thread LateUpdate和worker ack才finalize；
- CentralOnly buildPresentation=false不capture、不advance；
- explicit/manual/replay可在finalize前进入后续tick。

## 3. Planned changes

| 文件 | 符号 | before | after |
|---|---|---|---|
| `BattlePresentationShadowBuild.cs` | coordinator no-publication lifecycle | 无 | sealed runtime lifecycle、no-allocation active scan、C++ age/tail规则 |
| `NTSDBattleTickSystem.cs` | `RenderDispatch` | 只capture/queue | capture后立即finalize；no-publication直接advance |
| `BattleRuntimeSelfCheck.cs` | R6 spark timing matrix | 无 | `[0,5,38,39]` two-tick + unavailable + idempotence |

## 4. Protected boundaries

不改pass order、collision/RNG、snapshot schema、command/GPU、checksum、worker protocol、scene/DAT/C++或
任何已批准adapter。

## 5. Acceptance

- build=false tick结果count3 `[1,5,39]`；
- worker build=true cycle snapshot是advance前值，live结果count2 `[2,5]`；
- post-render finalizer不二次advance；
- unavailable lifecycle no change；
- compile/full self-check/validator/diff PASS；
- PlayMode/C++ trace未取得时最高`RUNTIME_PENDING`。

## 6. Actual changes / verification

- `BattlePresentationCoordinator.AdvanceHitRecordsWithoutPublication`：使用sealed runtime
  lifecycle catalog与复用`entityScratch`，按capture相同active/pending/generation/first-tick gate直接推进
  valid age、保留invalid non-tail并每tick最多移除一个invalid tail；unavailable时零写入；
- `NTSDBattleTickSystem.RenderDispatch`：publication capture/dispatch后立即finalize frozen cycle；
  CentralOnly no-publication与worker no-publication改走direct lifecycle；existing Late/ack finalizer保持幂等；
- `CheckHitRecordRenderDispatchWritebackContracts`：加入`[0,5,38,39]` no-publication、worker
  pre-advance snapshot、live writeback、重复finalize及unavailable control矩阵；
- `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`：exit 0，
  0 error / 43 warning；这只是生成C#工程的窄编译，不替代fresh Unity compile；
- `Tools/Validate-ChangeLedger.ps1`：42 records / 30 governed code files，PASS；scoped
  `git diff --check`：PASS（仅LF→CRLF提示）；
- 首次Unity 2022.3.62f3 batch尝试未进入编译：`Unity.Licensing.Client` IPC 60.01s timeout，Editor
  return code 199；当时旧DLL/result均作废且未计入本包证据；
- 用户交互身份随后通过Hub打开同一项目。三份source最后写入时间为19:23:43～19:24:38，fresh
  `Assembly-CSharp.dll`为19:41:38；Editor.log记录Tundra build success 26.11s，当前log内
  `error CS`计数为0；
- request驱动full `BattleRuntimeSelfCheck`于19:49:12写入fresh `PASS`，包含本Record新增
  `CheckHitRecordRenderDispatchWritebackContracts`矩阵；因此状态提升为`RUNTIME_PENDING`，但仍不是
  C++ runtime/PlayMode完整对齐。

## 7. Risks / pending

- published frame必须继续独立持有渲染资源；
- existing direct-world selfchecks仍可手动finalize，scheduler新行为不得破坏诊断API；
- `B-R6-PRES-005-01`已解决：交互用户Editor成功完成fresh compile与request self-check；保留首次
  batch licensing 199作为历史失败证据，不再阻塞后续R6/R7工作；
- PlayMode/C++ trace待验。

## 8. Rollback

只回滚本Record三份脚本内diff与关联文档，不触碰其它用户修改。
