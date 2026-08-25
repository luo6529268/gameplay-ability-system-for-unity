# R7-PRES-WORK-01 — frozen presentation / worker publication-ack recertification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-PRODUCTION-CODE CONDITIONAL CERTIFICATION`

## Scope

本包只读复核以下性能适配，不修改 Unity gameplay、C++ authority 或表现实现：

- C++ `game_tick(...)` 的 render observation point；
- Unity `RenderDispatch` / simulation-worker frozen frame capture；
- CentralOnly 的 latest-publication queue、延迟命令物化、world/generation gate；
- dedicated simulation worker 的 publication、host consume、presentation acknowledgement 和下一 tick gate；
- 当前 production scene/config 是否真正启用上述路径。

## Authority

- `J:\QQFile\NTSD2.4\ntsd_release\Makefile:32,34`：`game_tick.cpp` 与 `renderer.cpp` 都参与 release build；
- `src/entity/game_tick.cpp:945-948,2023-2087`：PreFrame/stage 后调用
  `pre_postprocess_render`，然后才执行 FramePostProcess、late entity 和 post-frame tail；
- `src/render/renderer.cpp:1300-1438`：同 tick active slot capture、stable signed-Z insertion sort、
  shadow/entity/overlay/hit-record 顺序；render offset 和 hit-record render-tail side effect 仍由 R6 合同裁决；
- `src/input/input_handler.cpp:1554-1608`：human poll 先把旧 key 写入 prev，再写本 tick key，
  key 不在同一 human poll 尾部清零。

## Unity mapping

### Frozen observation point

- `NTSDBattleTickSystem.RunPresentationAndCleanupPhase` 在 PreFrame/Stage 后调用 `RenderDispatch`，随后才进入
  FramePostProcess/Late/随机武器/post-frame tail；
- non-worker 通过 `SimulationWorldStageRenderModule.RenderDispatchAll` 调用 `BattlePresentation.BeginFrame`；
- worker 通过 `CaptureSimulationWorkerPresentationFrame` / `BeginSimulationWorkerFrame` 在同一逻辑 pass 冻结
  logical presentation frame；
- CentralOnly frozen frame只包含逻辑快照。表现主线程随后排序/建命令，不把 Transform、SpriteRenderer 或材质状态
  写回战斗真值。

### Latest publication / central materialization

- `BattleCentralRenderSystem.QueueLatestPublishedFrame` 以 world/frame/tick/mode/version 去重；
- `MaterializeLatestPublishedFrame` 同 Unity frame 最多物化一次，只消费最新 publication；
- world 切换会 retirement/clear 旧 plan；camera lease 还会校验 plan generation 与 tick；
- frozen frame的实体顺序和命令可以在主线程延迟物化，但原始逻辑 publication 不被修改。

### Worker publication / acknowledgement

- `BattleSimulationPublicationBuffer` 使用 sequence + two cells 发布不可撕裂的 tick/hash/checksum/has-frame；
- driver 只接受与 submitted tick 相同的 publication；
- `CanAdvanceTick` 在 tick in-flight 或 presentation awaiting acknowledgement 时拒绝下一 tick；
- worker 发布后等待 acknowledgement，host 在 LateUpdate 完成表现消费后确认；worker 才执行 consumption tail并释放
  single-flight gate；
- worker failure会使 driver fail-closed pause，不会静默回退到另一条 gameplay 路径。

## Production deployment fact

- `Assets/NTSD/Config/GameConfig/GameConfig.asset`：`BattlePresentationBackendName: CentralOnly`；
- `Assets/NTSD/Scene/NTSD_Battle.unity`：`useDedicatedSimulationWorker: 1`、LocalFreeRun、
  `requireInputFrameReady: 0`、`captureFullFrameSnapshotForDiagnostics: 0`；
- 同场景 `maxCatchUpTicksPerFrame: 1`。因此当前正式单机路径本来就只允许每 Unity frame 尝试一个 tick；
  worker single-flight没有额外把该已部署上限从多 tick降成一 tick，但它仍是未来改变catch-up策略时必须重审的边界。

## Fresh focused evidence

| Job | Result | Coverage |
|---|---:|---|
| `ab2811b35d8e42f9b0ce8ed4733ed0ed` | 13/13 PASS | latest publication、same-frame gate、world switch、stale camera submission、frozen capture |
| `26be6db261c54e45ae8c15f5cf1a5a11` | 11/11 PASS | frozen order、signed Z、slot/generation reuse、scratch cleanup |
| `7e64d65b61924459b9419fd1d5d4bc34` | 6/6 PASS | delayed command materialization、reference writer、catalog epoch、warmed 0 B |
| `3789a22c55504027b33b0204c6e5f96e` | 16/16 PASS | worker queue/publication/ack/thread ownership/lifecycle/logic-only materialization |

总计 46/46 focused positive checks PASS。它们证明当前 Unity 合同范围，不是 C++ runtime trace或Play Mode像素证书。

focused suites之后再次执行脚本域重载；重载完成时Unity Console为0条error/warning。完整
`BattleRuntimeSelfCheck`于2026-08-22 22:32:29写入`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`。
运行中的两条rest-binding Error是self-check主动触发的negative-control日志，不是编译错误或新回归。

## New findings

### D-TEST-002 — stale worker human-input expectation

完整 worker suite 与 fresh-domain exact rerun均在
`DedicatedWorkerFullTickConsumesCanonicalHumanInput` 的首个 `KeyLeft == 0` 断言失败（actual=1）。
生产行为与 C++ `InputHandler::poll` 一致：首次 Left+Attack 后应为 current key=1、prev=0；错误的是旧测试注释和
`KeyLeft/KeyAttack` 清零断言。当前只登记，不在本盘点包修改测试。

### D-TEST-003 — production worker/presentation joint coverage gap

现有 tests分别覆盖 worker publication/ack和central materialization，但没有一条正式 driver 夹具同时执行：

1. `buildPresentation=true` worker tick；
2. host consume publication；
3. CentralOnly materialize exact published tick；
4. acknowledgement/finalization；
5. 下一 tick unblock且不复用旧 frame/generation。

所以 joint handoff目前是 source-closed、分段测试通过，但仍缺一个端到端自动验收。

### D-PERF-003 — single-flight throughput boundary

worker与driver目前刻意保持一个 outstanding tick，并在表现确认前禁止下一 tick。当前production scene同时配置
`maxCatchUpTicksPerFrame=1`，因此这是已知、相容的部署边界，不是当前C++行为差异。若未来恢复同帧多tick追帧或
希望worker pipeline，必须先设计不可覆盖的多版本frozen frame/ack合同，不能直接放宽gate。

## Verdict

- 没有发现新的 production frozen/central/worker 行为差异；
- `D-TEST-002` 是source-confirmed stale test expectation；
- `D-TEST-003` 是joint acceptance gap；
- `D-PERF-003` 是已部署的性能/并发边界；
- fresh-domain full self-check已通过；C++ runtime trace、真实 URP Play Mode、GPU像素和长期worker压力仍未关闭，因此最高状态保持
  `RUNTIME_PENDING / NO-PRODUCTION-CODE CONDITIONAL CERTIFICATION`。

## Stop / next

本包不授权修测试、改worker pipeline、改central renderer、改pass顺序或修改C++。下一项按R7计划进入
pool / slot allocator / dynamic capacity只读盘点；之后统一编排测试治理与repair WPs。

## 2026-08-23 follow-up correction

`D-TEST-003` 已由独立 `R7-TEST-003` test-only包关闭自动覆盖缺口：formal driver双tickfixture实际执行
worker frozen publication → CentralOnly exact-tick materialization → acknowledgement/finalization → next-tick
unblock/new frame+generation。exact 1/1、worker+central 31/31、compile 0 error、02:27:37 fresh full self-check PASS；
production worker/render未修改，真实URP Play Mode、C++ runtime trace和R8仍不因此关闭。
