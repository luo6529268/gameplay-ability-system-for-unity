# NTSD C# 工程 vs Unity 工程 — 战斗逻辑差异与对齐清单

## 2026-07-26：1000 实体等价优化当前结论

**状态：表现构建、Late snapshot 与 AI 查询三条优化已落地并通过代码侧验证；30 Hz 性能门禁未通过。** 本批只改变 Unity 适配层的工作量，不改变 C# 权威定义的 battle pass、候选消费、输入、RNG 或生命周期语义。本节取代下方 2026-07-24 压力阶段中“尚无 per-pass timing”和“AI 仍全槽扫描”的历史描述。

- **catch-up 表现：**仅 `LocalFreeRun + CentralOnly` 的最终可见 catch-up tick 构建中央 presentation command；中间 tick 的全部战斗逻辑仍执行。`LegacyOnly`/`Shadow` 不抑制表现构建。
- **Late snapshot：**已移除 `StateSpecial`、`FrameExit`、`PrevFrameMirror`、`Recovery`、`FrameTickSuppressed`、`CleanupCompleted` 的冗余 refresh；保留 `FrameTick`、`DeathOpoint`、`TailAndQueuedFlush`。`Temp/NTSD_ProductionEntityStress.dispersed-full-ai-late-recovery-elided-detail-20260726.json` 的六个 removed `callCount` 均为 `0`，三个 retained `callCount` 均为 `334000`；retained 平均耗时为 `0.850/0.668/0.771 ms`。
- **AI 等价索引：**exact empty-air、ground team partition、phase1 Team5 list、融合索引与 first-10 top/second 保持既有 fail-closed 契约。新增 occupancy-epoch nearest resolver elision：`RuntimeSlotTable` 在成功 claim/allocate/release/reset/grow 后推进非零 epoch；AI snapshot 仅在前后 epoch 一致时发布，filter 用 epoch、generation 与 slot entity 证明复用安全，失败即 `Abort` 并进入现有 brute。实时 HP/team/state/Y/Vx、低 slot tie、same-Z、air、RNG 和 slot consumption 不变。

| 报告 | tick (ms) | `CharacterInput` (ms) | `FindNearestGround` (ms) | `RemainingAiDecision` (ms) | `Late` (ms) |
|---|---:|---:|---:|---:|---:|
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-air-fastpath-detail-20260726.json` | `110.846` | `37.318` | `15.698` | `24.944` | `23.444` |
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-team-partition-detail-20260726.json` | `85.898` | `29.171` | `9.237` | `16.779` | `14.373` |
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-index-fusion-detail-20260726.json` | `62.061` | `21.344` | `6.847` | `12.370` | `11.841` |
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-late-recovery-elided-detail-20260726.json`（334 ticks） | `82.712` | `28.148` | — | — | `14.164` |

- **occupancy-epoch 样本：**`Temp/NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json`：402 sampled ticks，tick `53.483 ms`，`CharacterInput=17.919`，ground/air/nearest=`5.712/2.036/7.748`，`RemainingAiDecision=10.086`，`Late=10.130 ms`，`bruteFallback=0`，visits/AI=`25.16`。
- **证据边界：**同机其他 Unity Editor 与系统负载持续变化；连续无 AI `72.343 ms/tick` 甚至慢于 full-AI `62.061 ms/tick`。新旧报告不能作为稳定 A/B，occupancy 优化的独立收益尚未隔离；最新 `53.483 ms/tick` 仍高于 `33.33 ms/tick`。
- **fresh 验证：**Unity 全量 compile `0 error`；EditMode job `49f6e6800c8a45db988de0b7b9f412ef` 为 **112 completed / 0 failed**（工具 global total=`216`，不得写成 112/112）；`BattleRuntimeSelfCheck` `2026-07-26 04:37:33 PASS`；Architect `PASS`、`P0-P2=0`，`P3` 仅为证据措辞。
- **未完成项：**nearest 暂停继续大改，主线转向 `RemainingAiDecision` 与全实体基础 pass；`FrameTick/Opoint`、`CandidateCollect` 继续观察。进一步删除 retained Late refresh 前仍须先建立 debug-only snapshot delta oracle。
- **teardown 与排除：**本批压力报告均 `restored=true`，active/world/claimed slots 清零；inactive pool capacity 增长仅为缓存信息。T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 2026-07-24 P8 v5 最终代码侧验收（覆盖下方 v3/v4 历史结论）

- P8-D v4 的全局 `Texture Memory` counter 在 Central/Legacy probe 中均为 `0`，因此 v4 是 `Incomplete` 历史证据。v5 改为 generation-owned `benchmarkOwnedTextureMemoryBytes`；无 generation、无 owned texture、非正值、非空 workload 的 0 draw calls 或任一适用必需指标样本不足都会阻止 PASS。
- `Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v5.json` 与对应 `-player-ab-v5.json` 共 8 份报告全部为 suite v5 `Pass`。每份 Central/Legacy 都是 120/120 正式样本、0 个必需指标缺失、owned texture 为正、600-frame leak 与 teardown 通过，且 teardown owned bytes/resources 为 0；A/B workload/input/final checksum 一致。
- Windows Player 采用真实窗口化 graphics device，不使用 `-batchmode`/`-nographics`。当前 16-retry/cleanup 源码生成的 Editor `100/300/500/1000` 报告完成于 `2026-07-24 03:00:12`、`03:06:39`、`03:12:02`、`14:10:19`：logic tick 平均/最大依次为 `13.227/45.537`、`42.752/198.637`、`78.149/221.383`、`36.488/201.219 ms`。Editor 300/500/1000 平均均超过 30 Hz 的 `33.33 ms` 预算；Windows Player 1000 为 `9.123012 / 42.3011 ms`。报告 PASS 只证明门禁和可比 workload 通过，不等于性能达标，也不表示 Central 必然快于 Legacy；数据非单调且受 Editor/当前机器影响。
- 最新 UnityMCP focused EditMode job `9869909f3c27446d8ca33cbaf0f436ab` 为 `44/44 passed`、`0 failed`、`0 skipped`，取代此前 `34/34` 的旧证据，并包含 request processor lifecycle 的 3 个 focused tests；完整 `BattleRuntimeSelfCheck` 为 `PASS`，Runtime/Editor dotnet build 为 0 errors。连续矩阵的 300 Player 首次曾 native exit `-805306369`；同 build 独立 300 单样本和完整重跑均退出码 0，最终 300/500/1000 报告有效通过。该偶发启动失败保留为已知运行记录。
- fresh Architect 最终只读复核为 `PASS`，`P0=0`、`P1=0`、`P2=0`、`P3=0`；覆盖 benchmark 生命周期、v5 policy、8 份报告、teardown、A/B identity、fresh `44/44`、self-check/build/Console 和当前文档边界。该复核不改变本节关于 Editor 性能预算未达标的结论。
- 本轮修复一个 P1 benchmark 生命周期问题：Play Mode 退出可能留下 hidden runner，令已经消费的请求永久显示 `RUNNING`。processor 现于 `ExitingPlayMode` fail-close、在非 Play 状态 reconcile 残留 runner，并在 EditMode 保留 request；新增 3 个 focused tests 通过。
- P8-A/B/C 维持既有验收范围；P8-E Android/Adreno/Mali 真机由用户负责，T8 默认 `stage.dat` 部署取消/排除。下方 v3、v4、presentation-only 或“没有 Standalone Player”的描述只作历史追溯。

### 2026-07-24 ProductionEntityStressHarness 全交互压力证据

- `ProductionEntityStressHarness` 使用真实 `GameObject`、正式 `SimulationWorld`、全 AI/正式输入、碰撞命中、opoint 和完整 lifecycle；运行配置为 `MobileExtended(1050)` 与 `LooseQuadtree`，不是 P8-D 的冻结 A/B workload。
- `Temp/NTSD_ProductionEntityStress.smoke-fresh-v3-20260724.json` 记录 50 个初始实体、46 个衍生实体、peak `96`，`SmokePassed`。teardown 后 active GameObject/world objects/world entities/claimed slots/objectPool active/referencePool active 均为 `0`；objectPool available 从 `10` 增长到 `96` 并作为 inactive 缓存保留，不是资源恢复到运行前基线。
- **cleanup remediation 回归：**teardown 现按阶段 best-effort 执行，使用 stress root 独立扫描 `activeGameObjectsAfter`；清理异常进入结构化记录并令 `restored=false`，retained inactive pool capacity 仅作信息，不参与 `restored` 判定。`Temp/NTSD_ProductionEntityStress.smoke-cleanup-remediation-20260724.json` 为 `SmokePassed`：50 initial、peak world entities `301`，`restored/activeState/driver/logging=true`，`cleanupExceptionCount=0`，active GameObject/world objects/world entities/claimed slots/objectPool active/referencePool active after 均为 `0`，retained inactive capacity 为 `10 -> 301`。这是追加的 remediation 回归，不替代上一条旧 smoke 的 50 initial + 46 衍生历史数据。
- **cleanup remediation fresh 验证：**Unity fresh compile `0 error`；focused EditMode job `1327ac9736cf4b03ad9a73d75dabd298` 为 `15/15`；`BattleRuntimeSelfCheck` 于 `22:02:29` 为 `PASS`。
- `Temp/NTSD_ProductionEntityStress.dispersed1000-cleanlog-20260724.json` 记录 1000 个真实 GameObject/world entities/slots、41 samples、平均/P95/最大 `3077.612/5943.039/6245.802 ms`、pair sum/peak `5706633/184181`、candidate peak `735`；`StoppedCleanly`。teardown 后上述活动对象/逻辑注册与 active pool 计数均为 `0`；objectPool available 为 `10 -> 1001` 的 inactive 缓存，不是资源恢复到运行前基线。
- `Temp/NTSD_ProductionEntityStress.concentrated1000-short-20260724.json` 记录 1000 个真实 GameObject/world entities/slots、25 samples、平均/P95/最大 `5148.808/8889.234/9848.765 ms`、pair sum/peak `11427523/499500`、candidate peak `198`；`StoppedCleanly`。teardown 后上述活动对象/逻辑注册与 active pool 计数均为 `0`；objectPool available 同样为 `10 -> 1001` 的 inactive 缓存，不是资源恢复到运行前基线。
- **边界与风险：**Editor 1000 全 AI、全交互实体约 `0.1-0.3 FPS`，远未达到 30 Hz。P8-D v5 real-runtime A/B 是受控/冻结工作负载，不能代表此场景；中央渲染正确性、资源和 teardown 验收不等于 1000 实体完整战斗性能达标。代码审查已确认：`BruteForceSceneQuery` 的 formal fallback participant 会与全部 participant 配对、排序去重后双向 `CollectCandidatesForPair`；分散场景 peak fallback=`154`，仅 fallback 理论约 `142,065` unique pairs，约为实测 peak `184,181` 的 `77%`；集中场景 peak=`499,500`（`1000 choose 2`），但 candidate peak 仅 `198`。次要热点是所有 1000 实体启用 AI 时，`SimulationWorld.AiInput.partial.cs` 每个 AI 仍扫描 slots `20..1049`，约 `103` 万 slot visits/tick，部分 phase 另有同队扫描。当前报告没有 per-pass timing，不能精确分摊毫秒。清理方面可结论为本次运行的活动对象与逻辑注册清理正确，inactive pool capacity 仍只是保留缓存信息。T8 `stage.dat` 继续取消/排除，Android 真机仍由用户负责，本项不改变其状态。

## 2026-07-23 P8 当前渲染验收（覆盖下方 P8-C/P8-D 的过时结论）

本节仅更新中央渲染 P8 的当前证据，保留下方历史审计记录。任何下方“P8-D 未运行真实 logic tick”或“没有 Standalone Player”的描述均已被 final v3 报告取代。

- **P8-B：**诊断数据现在有 `FrameId`、显式 `AtlasPageIndex`、strict central-binding validation、first unresolved/unsupported status，以及 generation/tick-coherent aggregate diagnostics。Runtime/Editor 的相关构建为 0 errors；focused/full checks 在当前证据范围内通过。
- **P8-C：**`Temp/P8-C-Resume-Live/P8-C-report.json` 在 `2026-07-23 17:28:29` **PASS**，覆盖正式 `LF2ObjectPointFactory.CreateObjectImmediate` / `FreeEntityLikeExe` 链。Pool 结果为 `availableBefore=7`、`totalCheckout=9`、`expandedAndPublished=2`、`availableAfter=9`、`uniqueRuntimeHandles=2`，且 cleanup PASS。`Entity(33,0)` type `0` 与 `Entity(100,0)` type `4` 均使用 `AtlasPageTexture2D`；前者 Legacy/Central alpha pixels 为 `4971/4971`，后者为 `2090/2090`，两者 maximum pixel diff 都为 `0`。范围仍不包含 skill-input opoint。
- **P8-D：**final v3 的 eight reports，即 `Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v3.json` 与对应 `-player-ab-v3.json`，均 **PASS**。它们不是 synthetic presentation-only test：每档使用 `MobileExtended(1050)` primary + mirror `SimulationWorld`、准确数量的真实 `LF2Entity` fixtures、`FrameInputSet.Empty`、完整 `NTSDBattleTickSystem`、30 warmup + 120 sample logic ticks、deterministic checksum、从真实 handle/generation/position 冻结的 presentation，以及 600-frame leak gate。A/B 运行相同 logic workload；不得将 PASS 写成 central 快于 legacy。

| report | logic tick avg ms | max ms | tick alloc avg/max B |
|---|---:|---:|---:|
| `100-editor` | `8.3087375` | `12.0803` | `0/0` |
| `300-editor` | `24.3566941666667` | `33.9412` | `0/0` |
| `500-editor` | `42.7971166666667` | `57.0061` | `0/0` |
| `1000-editor` | `100.006675` | `126.7602` | `0/0` |
| `100-player` | `0.537154166666667` | `1.285` | `0/0` |
| `300-player` | `2.59706583333333` | `29.4842` | `0/0` |
| `500-player` | `1.56702166666667` | `2.752` | `0/0` |
| `1000-player` | `2.980925` | `6.0687` | `0/0` |

Editor `1000` 约 `100 ms/tick`，所以不满足 30 Hz；Windows Standalone final v3 `1000` 的平均约 `2.98 ms/tick`。这些数值不替代 Android/Adreno/Mali 真机验证，后者仍为用户负责的 P8-E 排除项。T8 默认 `stage.dat` 部署同样继续排除。

最终顺序回归已关闭。held geometry 失败不是 benchmark 全局状态泄漏，而是 parentless/root renderer 的 `_visualTransform == rootTransform`：正确世界位置写入后，同一 Transform 又被 local-zero 重置。`LF2ObjectRenderer` 现只对独立 child visual 归零 local position；focused fixture 验证 `SetLogicObject` 保持 runtime X/Y/Z、`FirstPresentationTick`、`CentralShadowBuild` 模式与 legacy suppression，并要求 legacy root position 等于 immutable central command。fresh `Assembly-CSharp.dll` `18:05:55` 晚于相关源码 `17:59:02`；1000 实体 Central/Legacy A/B 于 `18:10:49` PASS，退出 Play 后完整 `BattleRuntimeSelfCheck` 于 `18:13:03` PASS。最终 Runtime/Editor dotnet 构建为 `0 errors / 42 warnings` 和 `0 errors / 48 warnings`。本节不提前声明新的 Architect PASS。

## 2026-07-23 P8 中央渲染验收更新（当前证据）

- **P8-C 已完成定义内的正确性/像素矩阵。** `Temp/P8-C-EditModeTest/P8-C-report.json` 为 PASS，覆盖 1000 次 generation reuse、超预热隔离扩容、Texture2DArray/OrderedPages、`A/B/A`、类别遮挡、4095/4096/4097 chunk、缺资源 fail-closed 与 frozen-frame Legacy/Central 像素对照；`Temp/P8-C-LivePool/P8-C-report.json` 为 PASS，真实 Play pool 从 `availableBefore=4` 获取 5 个对象，确认 5 个唯一 mount owner。旧 job `f278668e3a2445139c6a1a5ceb8815be` 的 11/11 是历史证据；P2 回归后的 fresh job `e455b7f70043438a938faa23e82e53f3` 为 12/12 passed（P8-C 2 + P8-D 10，0 failed/skipped）；fresh full `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 2026-07-23 12:07:26 PASS，P2 `BattleRenderingBenchmark.cs` 11:56:24 < Unity DLL 11:59:33 < result 12:07:26。过滤到的 2 条 Console error 是自检刻意构造的 registration rollback / mismatched rest binding release 拒绝路径（`BattleRuntimeSelfCheck:7046` / `:1133`），无编译错误栈或 benchmark 异常。
- **P8-D 已完成受控表现基准矩阵，不是战斗容量或完整性能宣言。** `Temp/P8-D-presentation-100-ab-rerun.json`、`300`、`500`、`1000` 四份报告均 PASS；每档严格验证 presentation entity/command 数、256x256、资源/owned heap 与 retained heap 增长阈值。P2 已关闭 EditMode 把 mesh segment 冒充 `Graphics.DrawMesh` submission 的问题：`presenterSubmissionDrawCalls` 显式为 unavailable，Play 仅在实际调用提交后计数。它们是冻结的 synthetic presentation workload，不创建 `SimulationWorld` active entities、也不执行 logic tick；不可用的 main/render/GPU/draw 指标保持 unavailable。本轮没有 Standalone Player 实测，不能据此宣称全面性能收益或真实 active-entity 上限。
- **额外 current-scene production 覆盖。** `Temp/P8-D-current-scene-ab-v2.json` PASS：退出 Play 前真实 `NTSD_Battle` 的 `SimulationWorld ObjectCount=12/tick=3847`，published frame 为 `6 entities/12 commands`。Central/Legacy 均实际为 `6/12`、同 fingerprint `f3aaf429518f46ec`、同 256x256；retained managed heap 为 Central `+28672 B`、Legacy `+49152 B`，graphics/owned bytes 为 `+0`、resource count 不变。presentation build/GPU 只作本次 Windows Editor 样本，main/render/draw 仍 unavailable；这是额外生产覆盖，不是独立 P8 gate 或全面性能结论。
- **范围。** P8-E Android/Adreno/Mali 真机验证由用户负责；T8 默认 `stage.dat` 部署继续排除。下方 P8-C/D “待实施”“未验收”仅为历史快照，若与本节冲突，以本节为准。

## 2026-07-22 对象池预热上限后 opoint 武器不可见（当前状态）

- **复现：**隔离 `PoolInitialSize=10`，经生产 `opoint`/factory 保留 12 个 `LightWeapon`。第 11/12 个实体的逻辑、声音、unique root/renderer、mount/runtime handle、sprite 与 12 条 Entity command 均存在，但中央像素缺失。
- **定性与根因：**这不是 C# 战斗逻辑差异，也不是 pool 扩容、runtime handle 或资源问题；它是 Unity 表现后端适配缺陷。根因位于 `BattleDynamicMeshBackend` 的动态 submesh descriptor 生命周期：旧布局/增长时默认 descriptor 曾临时重叠；Unity 2022.3 收缩 `subMeshCount` 会截断 index buffer。权威 C# 不定义此 Unity 渲染实现。
- **修复：**每个 chunk 维护 `activeSubMeshCount`；physical `subMeshCount` 作为只增不减的 high-water。增长后先将全部 descriptor 置 inert，再写 active；非增长时先清旧 active，再写 active；empty 不收缩。禁止 bulk `SetSubMeshes`，此前该路径触发 native crash。
- **回归矩阵：**隔离预热 10，经生产 `opoint`/factory 保留 12 个 `LightWeapon`；检查 unique root/renderer、mount/handle、sprite、12 条 Entity command；覆盖 `1 -> 32 -> 1 -> 33 -> 1`、inactive inert tail、`GraphicsBuffer.count=24576`、`4096/4097` 边界、recovery、0 GC 与 scoped warning 捕获。
- **fresh 证据：**source `20:24:58` / `20:26:45` < DLL `20:28:54` < result `20:29:44` **PASS**；Unity 编译 `0 error`。本轮 `Editor.log` offset `31277122` 后 descriptor overlap、bulk `SetSubMeshes` 与 native crash 均为 `0`；Editor PID 响应正常。
- **验收边界：**代码、编译、self-check 与生产 `opoint` 链已验证；用户真实 Play Mode 视觉复测仍待确认。T8 明确排除，默认 `stage.dat` 部署继续暂缓。

## 2026-07-22 Rendererless 武器显示回归修复（当前状态）

- **复现与根因（旧复现限定）：**4 个随机掉落武器已存在时，角色 `opoint` 再生成武器会使既有掉落武器及新武器不显示；后续 `opoint` 仍不显示，但落地声音继续。rendererless `LF2Sprite.Hide` 将 `EntityVisible=false`，而成功 `ShowPic(valid)` 没有像旧 `SpriteRenderer` 路径那样恢复可见性。因此 `CurrentEntry`、`pic`、战斗逻辑与声音均正常，中央渲染仍永久过滤 Entity command。此 `EntityVisible` 根因只解释该旧复现，不解释 `PoolInitialSize=10` 后第 11/12 个实体已有 command 但缺像素的问题；后者以本文件上方的动态 submesh descriptor 适配缺陷为准。
- **修复边界：**只在 catalog 或 legacy sprite 成功解析时恢复 `EntityVisible`；`pic=999` 和缺失资源仍保持不可见，不把失败语义改为显示。
- **验证证据：**Unity `Assembly-CSharp.dll` 于 `2026-07-22 18:56:11` fresh compile，Console 为 `0 error`；完整 `BattleRuntimeSelfCheck` 于 `18:58:50` **PASS**；`dotnet build Assembly-CSharp.csproj` 为 `0 errors / 42 warnings`。Play Mode 中先保留 4 个随机武器，再经 `LF2ObjectPointFactory` 的 `opoint oid121` 调用 `Hide -> ShowPic`，随机 slot `50` 和 opoint slot `54` 均仍有 Entity command；销毁并复用同一 renderer instance 后再次 `opoint`，slot `54` command 仍存在；central `IsStale=false`、`unresolved=0`。
- **范围：**此记录只关闭该 rendererless 显示回归；不宣称全部战斗系统、全部资源组合或设备表现已完成验收。T8 默认 `stage.dat` 部署继续暂缓。

## 2026-07-21 集中式渲染 Fresh Final Validation（当前状态）

本节记录与战斗可观察行为直接相关的中央渲染验收，覆盖本文件顶部及后续旧快照中“`CentralOnly` 不可用、Overlay blocker、P7 未完成、B2C 未经 Architect 验证”的过期措辞。

- **CentralOnly 实际运行**：诊断为 `requested/effective=CentralOnly`，`frame/ownership/ready/submitted=true`，`draws=12`；P7 Overlay、Shadow、Entity、HitRecord 已共同进入单帧 pixel owner，不再有旧后端与中央后端双重出像素。
- **伪影修复依据**：`BattleDynamicMeshBackend.ClearActive` 曾把 `subMeshCount=0`，触发 Unity 2022.3 释放 native index buffer，造成后续索引错误、黑块及三角形 UV 伪影。修复为保留零索引 inert submesh；不是对战斗 runtime、DAT、挂点或排序规则的改写。
- **同帧像素验收**：暂停同一帧的 Legacy/Central `1920x1080` 截图比较为 `changed=0`。该截图直接覆盖当前可见的角色、武器/球体与阴影；Overlay/HitRecord 的 ownership 与资源路径另由 self-check 和运行时 diagnostics 证明，不宣称它们在该截图中一定可见。它不能代替所有角色、资源组合和设备的逐帧生产证书。
- **运行时与空间查询**：`Temp/NTSD_BattleRuntimeSelfCheck.result` 为 **PASS**，Unity Console **0 error / 0 warning**。真实 Play 的 `LooseQuadtree` 为 `backend=LooseQuadtree, objects=12, tick=1436`，Console 同为 **0 error / 0 warning**。B2C 的 Architect final 结论为 **PASS / no P0-P2**。
- **Editor 性能记录，不作移动端结论**：Legacy `6.1884 ms CPU / 0.346112 ms GPU / 18 draws`；Central `6.5114 ms CPU / 0.70656 ms GPU / 20 draws`；Central 内存 `1391.17 MB allocated / 1005.19 MB graphics`。
- **外部边界**：尚未取得真实 Adreno/Mali 或 Android Player 的像素、兼容性及性能证据。T8 默认 `stage.dat` 部署仍按用户要求暂缓；该资源前置不构成当前代码差异。

> 下方出现的“CentralOnly 继续拒绝”“Overlay 未实现/阻塞”“P7 仍未完成”“B2C 未经 Architect 复核”以及“Play/pixel/Profiler 尚未验收”等表述均为历史快照。保留它们用于追溯，但当前状态以本节为准。

## P7 Batch6 per-entity Overlay 当前状态（2026-07-21，覆盖旧 Overlay blocker 结论）

- P7 Batch6 已完成代码侧 Overlay 收口，不再把 per-entity Overlay、`WORDS0..5` 缺失或“current Overlay blocker”列为当前代码差异。`WORDS0.bmp` 至 `WORDS5.bmp` 已加入 Unity Assets；其 SHA256 与权威 C# host 引用的运行时资源来源一致。此核验只确认资源依赖，不改变唯一战斗逻辑权威 `J:\QQFile\NTSD2.4\ntsd_release_C#`。
- `BattleSpriteCatalog.CommonWordGlyph(sheet, charCode)` 覆盖 `6 * 256` glyph，按 top-left authority rect 转 Unity bottom-left rect；WORDS prewarm 使用 exact-black transparency、Point/Clamp、atomic publication 与 retirement ownership。`BattleSlotLabelRuntimeState` 已提供 `char[10,12]` + `int[10]` 并接入 reset/`MatchConfig` bootstrap。
- `BattleEntityOverlayLayout` 已无分配地布局复活 counter、普通/括号标签、普通 `Com` 与特殊 `WORDS5 Com`；标签 clamp、counter 不 clamp，容量异常 fail-closed。presentation snapshot 保留原始 `ObjectId` 用于 shadow OID223/224 gate，并单列 current DAT identity 用于 Overlay；命令固定为 `Shadow -> Entity -> OverlayGlyph -> HitRecord`。
- legacy 后端已有 pooled `BattleEntityOverlayRenderer`，含 generation/stable-id guard；`LegacyOnly` 发布 immutable frame 但不构建 central mesh，`CentralShadowBuild` 仍仅诊断，`CentralOnly` 继续由 `ValidateAvailable` 显式拒绝。frame-level catalog lease、HitRecord cycle lease finalizer 和 empty-frame no-retain 均已覆盖；retirement 窗口、命令顺序与 zero-GC 进入 self-check。
- fresh 证据：latest relevant source `2026-07-21 16:01:49` < Unity DLL `16:03:35` < full self-check result `16:04:54` **PASS**；Unity Console **0 C# error**；最后一次主代理 `dotnet build` **0 errors / 18 existing warnings**；Architect final **PASS / no P0-P2**。`git diff --check` 待主任务最终统一执行。
- 这只关闭代码/编译/self-check/静态复核层的 Overlay 缺口，不构成 P7 全门槛或完整 Play 验收：Play/pixel/Profiler/Adreno/Mali 未验收，T8 默认 `stage.dat` 部署继续排除。本文后续相反的 Overlay 结论均为历史快照，除非明确另行重开。

## B2C Extended checksum（2026-07-21 当前状态）

`Authority400` 的 `ntsd-battle-trace-v3` 与 direct parity guard 保持不变；`MobileExtended` / `DesktopExtended` 已使用独立 `ntsd-unity-extended-battle-checksum-v1`。容量感知 slot metadata、generation/stable ID、active/runtime raw state、稀疏 ARest/VRest、rest binding guard 与 non-materializing capture 已落地。B2C 也已接入 generation-aware AI Loose Quadtree 输入快照查询，以及显式 `LooseQuadtree` 后端下的即时 weapon/body current-world 查询；索引、几何或映射异常均回退 brute，生产默认仍是 `BruteForce`。最新 full `BattleRuntimeSelfCheck` `2026-07-21 00:48:06` **PASS**；`dotnet build` **0 errors / 42 existing warnings**；`git diff --check` 通过。先前复审的两个 blocker 已修复并进入 self-check，但 fresh 最终架构复审待补，因此当前状态不是 Architect PASS。

## 集中式渲染 P1-P6 与 P7 Batch1-3 当前状态（2026-07-21）

P1 compact legacy sorting 已完成代码与自动验证；具体排序、`8192` legacy guard、同层 renderer 检查和 Play-unverified 边界见集中式方案文档。P2 immutable `BattleSpriteCatalog` 也已完成代码层实施：唯一 key 为 `(LF2Entity.ResolveCurrentDataObjectId(entity), effectivePic)`，entry 保存 source sheet/shared texture、bottom-left rect、UV、metrics、pivot 与 legacy `Sprite`。

P2 prewarm 使用 invocation-local staging、generation/disposed gate 和原子 publish；configs、`MergedSprites`、catalog 只会整体替换，失败、stale result 与 teardown 均清理。renderer 引用计数把旧 catalog 的退役推迟到零引用。正式 partial BMP 按 declared row/col + `localPic` 建立稀疏 rect 并保留 holes，normal/swapped 仅在完整匹配时择优；weapon6、weapon3 等生产矩阵已进入 self-check。display、collision、anchor、SpecialAttack point-center 与 shadow metrics 不再以战斗期 `Sprite.rect` 为真值；`pic=999`、missing key、current identity 切换和 pool reuse 均清除旧表现引用。

P2 fresh 证据为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full `BattleRuntimeSelfCheck` `04:18:04` **PASS**；dotnet build **0 errors**。由于 Unity 自动生成 `.csproj` 的刷新视图不同，既有 warning 数分别出现 18 与 42，本节不冻结 warning 数。最终 architect review **PASS / no blocker**；最终 code review **no P0-P2 findings**。本轮未执行 Play Mode、真实异步 BMP stress 或性能验收，因此 P2 仅能标为“代码、编译、self-check、静态复核完成；Play/stress/performance-unverified”。

P3 已实现 value-only immutable presentation snapshot/commands、double buffering、几何容量增长和 atomic publish。模式边界是默认 `LegacyOnly`、诊断 `CentralShadowBuild` 和明确拒绝的 `CentralOnly`。命令按 `(ZInt, runtime slot)` 排序，并为每实体依次产生 `Shadow -> Entity -> Overlay -> HitRecord`。早期 `AuthorityExpectedButLegacyMissing` 标记来自不完整权威盘点，现已废止；权威两个 host 实际都绘制 per-entity Overlay，Unity 尚未实现，所以 P3 不能宣称 overlay 等价。

P3 actual legacy probe 直接采样真实 renderer 的 sprite、texture、material instance、rect、pivot、position、flip 和 sorting；HitRecord 在 legacy advance 前采样。catch-up 帧的中间逻辑 tick 因没有对应实际 renderer 状态而明确记录为 `Incomplete`，包含 count/first/last，只有最后可观测 tick 能做完整 probe。persistent scratch 已由 steady `RenderDispatch` zero-allocation self-check 覆盖；zero-hit 经 `SparkRenderer.RenderAll` finalize，production pool 路径覆盖 nonzero spark atlas cells、每 tick age once 和 `OnDisable`/`OnDestroy` 归池。P3 诊断与战斗 checksum 隔离。

P3 fresh 证据为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full `BattleRuntimeSelfCheck` `05:40:16` **PASS**；dotnet build **0 errors / 18 existing warnings**；最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。未执行 Play Mode、真实 SPARK BMP/设备或性能验收；未来异步 consumer 仍必须持有 catalog lease 并验证 generation。不能把 catch-up `Incomplete` 中间 tick 扩大为逐 tick actual legacy parity 已验证。

P4 已完成代码层实现：中央 Mesh 后端持久复用，并以 `4096` quad/`UInt16` index 的固定 chunk 契约切分。`OrderedChunks` 只合并相邻兼容命令并保持 `A,A,B,A` 原序；`StrictOrderedDraw` 提供更细的正确性回退。unresolved command 是提交 barrier，stale chunk/submesh 会清空。模式边界继续是 `LegacyOnly` 不 build、`CentralShadowBuild` 不提交、`CentralOnly` 在全类别 ownership 完成前拒绝。

P4 URP pass 过滤为 world camera 的 `Base` camera，注入点为 `AfterRenderingTransparents`。`BattleRenderFeature` 已验证为 active renderer asset 的唯一 subasset。初审发现 feature B 覆盖 A 后注销 B 不恢复 A，现已改为 registration stack，并以 `A -> B -> unregister B -> restore A` 覆盖 fallback material、array material 与 draw mode 恢复。

P4 fresh 证据为 source `2026-07-21 06:32:00.287` < Unity DLL `06:32:56.970` < full `BattleRuntimeSelfCheck` result `06:33:43.796` **PASS**；dotnet build **0 errors / 42 existing warnings**；最终 architect review **PASS / no P0-P2 findings**。没有执行 Play Mode、桌面像素 baseline、Profiler GC 或 Android/Adreno/Mali 验证，故只能标为 P4 代码/self-check/静态复核完成，不能宣称全部验收门槛完成。

P5 已完成代码层实现：确定性 planner 以 whole-sheet 为单位生成 `2048 x 2048` 多页布局，按 normalized path ordinal 去重；同 path/同尺寸的 pixels 冲突会拒绝。sheet 使用 `1px` extrusion。满足能力 gate 时建立 `RGBA32 Texture2DArray`，否则使用保持相同 page 顺序的 2D fallback。catalog entry 保留 legacy source 并增加 immutable central binding；manager 以事务方式发布，明确持有 Unity Object ownership，renderer 与 central lease 一起控制旧资源退役。

P5 array shader 使用 per-vertex slice，允许相邻跨 slice 命令在相同 array material 下保持顺序合批；2D fallback 的 `A/B/A` 保持三个连续段，禁止重排。array/fallback 双 shader、material、pass 和 installer 均已接线。复核关闭两个 P2：同 path、同尺寸、不同 pixels 对两种输入排列都拒绝，equal-content duplicate 成功；显式两页 fallback 在 page0 成功、page1 失败时，两页均销毁且不产生 partial publication。

P5 fresh 证据为 source `2026-07-21 07:06:28` < Unity DLL `07:07:12` < full `BattleRuntimeSelfCheck` log `07:08:13` **PASS**；dotnet build **0 errors / 42 existing warnings**；architect final review **PASS / no P0-P2 findings**，code review **no P0-P2 findings**。未执行生产 BMP Play、桌面 overlap pixel baseline、Profiler/allocation stress、Android/Adreno/Mali array/fallback 与内存性能验收，因此 P5 仅为代码/self-check/静态复核完成，不能宣称全部验收完成。

P6 已完成设备策略与诊断代码：`BattleRenderingDevicePolicy` 是 immutable capabilities，`FromSystem` 是唯一系统能力采集边界。resolver 严格按 CLI > `GameConfig` > Auto 解析 `-ntsdBattleAtlasMode` / `-ntsdBattleDrawMode`，在 `TextureArray` 与 `OrderedPages` 间安全 fallback 并报告原因；draw mode 只在 Auto、`OrderedChunks`、`StrictOrderedDraw` 中选择，`SingleMesh` 不进入生产。确定性 JSON report 显式记录请求、capabilities、effective mode 与 fallback reason。

P6 manager 每次 publication 只解析一次，central 使用缓存的 effective draw mode；每 tick 不再查询 `SystemInfo` 或 CLI。该策略不改变 profile、capacity、tick、collision、checksum 或 `CentralOnly` guard。P6 尚未完成 Adreno/Mali、Play、pixel baseline 或 Profiler 验收，因此只能宣称代码策略/诊断完成。

P7 Batch1 完成 held-object 子批。权威链为 `InteractionRuntimePasses -> WeaponPointRuntime/WeaponRuntime -> SdlBattleRenderer/BattleHostForm`；legacy 与 snapshot 共用 pure held-offset helper，在 capture 时将 offset 固化为 immutable 值并追加到 Entity command。right/left、target mismatch、release、missing holder/wpoints、slot generation reuse、dormant holder 与 legacy/central equality 均已覆盖。

P6/P7-held 统一 fresh 证据为 self-check source UTC `23:42:44` < Unity DLL `23:44:03` < `Unity-P6-P7-Final2-SelfCheck.log` `23:45:00` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS**，code review **approve / no P0-P2 findings**。

P7 Batch2 已完成 render-state semantic parity：snapshot/command 持有 value-only `Color32`、flipX/flipY、mask/material semantic 与 logical resource key，instance ID 仅用于诊断。catalog 提供 immutable `Sprite -> key[]` 反查和 preferred entity key。legacy probe/Compare 检测 RGB、alpha、flipY、unsupported state 与 logical key；central resolver 转发 color，对无法解析的语义 fail closed。

Mesh 把 color 写入 quad 四顶点，flipY 通过交换 V 坐标实现；color 变化不切 segment，material semantic variant 必须断段。pool entity/shadow/spark checkout 重置为 white、flipXY false、mask none；首次干净 checkout 借用 `Sprites/Default.sharedMaterial`，不触发 `.material` 实例。

两个中央 shader 依据 Unity 官方 `2022.3.4f1` builtin shaders ZIP changeset `35713cd46cd7` 改为 `Blend One OneMinusSrcAlpha`，最终 `rgb *= a`，并声明 `NTSDAlphaContract` tag；installer 验证 white/tag。fresh 链为 source `08:27:50` < DLL `08:28:48` < self-check log `08:29:48` **PASS**；installer validation **PASS**；dotnet **0 errors**；architect/code review **PASS / no P0-P2 findings**。

P7 Batch3 已完成 Shadow。实现依据 authority `BattleHostForm` / `SdlBattleRenderer.DrawShadow` gates；资源采用 typed `EntitySprite`/`CommonShadow` key。`GameConfig.ShadowPrefab` 被捕获为 immutable borrowed binding，包含真实 sprite、texture、UV、size、pivot、color 和 material；manager 在 main thread atomic common publication，borrowed Unity Object 不进入 owned retirement。

snapshot 保存 actual ObjectId 与 `HasCurrentFrame`；Shadow command 携带 real descriptor/`CommonShadow` 并位于 Entity 前，legacy probe 对比 exact sprite。central resolver 校验 sprite、texture、rect、pivot、material ID，并使用 source2D + fallback material；missing config/resource 一律 fail closed。actual OID223/224、state3005/9997、`Link < 0`、HitStop 与 missing frame 已对齐。

review 关闭 P1 missing-frame legacy/central，以及 P2 material ID、真实 `GameConfig` asset、real commit -> replace retirement tests。fresh 链为 source `09:29:03` < Unity DLL `09:31:10` < self-check log `09:32:07` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect/review **PASS / no P0-P2 findings**。

Batch3 结束时 P7 仍未完成，Play、实际 pixel baseline 与设备未验收，HitRecord/Overlay 当时均未收口。后续 Batch4/5 已关闭 HitRecord resource/lifecycle 代码缺口；当前仍由 Overlay 阻塞 `CentralOnly`。T8 继续排除。

P7 Batch4 已完成 **SPARK / Common HitRecord resource ownership** 的代码层收口：typed `CommonSpark(pic)` 的 20 帧资源在 prewarm 中只 decode/process 一次，再于 main thread atomic publish；legacy `SparkRenderer` 不再在 `Awake` decode 或创建资源。central resolver 验证 logical key、`Sprite`、`Texture`、rect、pivot、size 和 material；publication lease/retirement 已接入。

Batch4 失败契约：缺失/无效 SPARK 释放 stale lease 且不修改 `HitRecord` age/count；partial `Texture`/`Sprite` 构造失败事务式清理，不能发布半成品资源。fresh 链为 source `11:13:05` < Unity DLL `11:15:20` < result `11:17:38` **PASS**；architect re-review **PASS / no P0-P2 findings**。code-review provider 为 `429`，不得表述为 code-review 通过。

此项不构成 P7 整体或运行时验收：Play、pixel、Profiler、真机和真实 SPARK 资源路径未验证；Batch4 当时未包含的 HitRecord lifecycle mutation 已由下方 Batch5 收口。T8 继续排除。

P7 Batch5 已完成 backend-neutral immutable double-buffer HitRecord presentation cycle。`RenderDispatch` 捕获 owner handle/generation、count、age、x/z 与 frozen common publication；`SparkRenderer` 只 materialize/probe，不写 live HitRecord。`LateUpdate` 顺序固定为 legacy materialize -> central `PrepareFrame` -> one finalizer；catch-up 只 finalize 最后 cycle。

Batch5 mutation 契约：missing SPARK zero-write；valid age 每 cycle 恰好 `+1`；invalid sampled tail 每 cycle 最多删除 1 项，`4/14/28/38` 进入 gap 的同 cycle 不删除。slot reuse、count/age guard 已覆盖，pool/camera/backend 不影响结果。后续 P2 修复将 binding 改为 direct ownership transfer，无 per-tick lease GC；no-hit 不持 binding。coordinator reset 接入 world reset、driver unbind、world replacement、destroy；ordered owner cursor 为 O(N)，`1000` owners 精确为 `1000` comparisons。

Batch5 fresh 链为 source `12:39:24` < Unity DLL `12:40:40` < result `12:41:20` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS / no P0-P2 findings**；code review **APPROVE / no P0-P2 findings**。Play、pixel 与 device 仍未验收。

Overlay authority re-audit 将其确认为当前 blocker，而不是空占位：权威 `BattleHostForm` 与 `SdlBattleRenderer` 都按 `Shadow -> Entity -> EntityOverlays -> HitRecords` 绘制。per-entity Overlay 内容为 `Hp2Orig > 1` 的复活次数和 entity label；资源 `WORDS0..5.bmp` 的 glyph 为 `8x16`、步距 `9`、black colorkey。Unity `Assets` 当前没有 `WORDS0..5`，也缺 `BattleSlotLabels[10,12]` / state 镜像和 snapshot 字段契约，因此 Overlay 未实现，`CentralOnly` 继续拒绝。global function/pause overlay 是独立后置 UI，且 GDI/SDL 不一致，不塞入 per-entity P7，本批不处理；T8 继续排除。

以下旧阶段中“Extended checksum 跳过/为空、schema 未实施”或“即时 weapon/body、AI 查询未迁移”的陈述是历史快照，已由本节覆盖。Loose Quadtree 默认启用证据、P1-P6 的运行时表现/真机/真实资源/性能验收，以及 P7 Overlay 实现仍未完成；Batch5 已关闭 HitRecord lifecycle mutation 代码层缺口。T8 已排除，不计入完成条件。

## BATTLE-RENDER-PLAN1 集中式战斗渲染系统方案（更新于 2026-07-20）

移动端集中式战斗渲染与 runtime 容量/空间索引决策已记录在 [central-battle-render-system-plan.md](central-battle-render-system-plan.md)。当前状态是 **R1-R2C-4、B0、B1-B1.3、B2A 与 B2B generation-aware incremental Loose Quadtree 已完成代码层实施和既定验证**。

`Authority400` 已接入 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused`，保留 C# 权威 400 槽、特殊槽区与最低空闲槽语义；`SimulationWorld` 仍显式 pin `Authority400`。fresh 证据为源码 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` `12:05:07` **PASS**；100,000 次随机分配操作与朴素扫描模型对照 **PASS**；架构复核 **PASS**。

R2A 已建立固定 `PageSize = 256`、按需物化的 `RuntimeSlotTable`，验证 `Authority400` 的 400 逻辑地址、`MobileExtended` 设计所需的 1050 逻辑地址及最后一页尾部 guard、每槽独立 raw runtime/rest 存储、`ClaimedCount`，以及 `(slot, generation)` 句柄在 release、同槽 reuse、reset 后使旧引用失效。fresh 证据为源码 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` `12:36:53` **PASS**；架构复核 **PASS**。

R2B 已将生产 `Authority400` registry 迁移到单一 `RuntimeSlotTable`，替换旧的 used/raw runtime/raw rest 并行数组；slot 到当前 occupant 为 O(1) 查询。live ascending slot scan 保留游标以上新生实体同 pass 可见、游标以下低槽复用实体延至下一 pass 的时序；release 以 `expectedEntity`/当前 occupant 防止旧实体释放复用槽。stage spawn 的 raw rest 恢复/消费、ordinary spawn 重置语义，以及 `ObjectCount`、buckets、`SceneQueryHit` 的 slot-address 契约均保持不变。fresh 证据为生产源码 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` `12:57:02` **PASS**；`dotnet build` **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索 **0**。

R2C 已为 `RuntimeSlotAllocator` 与 `RuntimeSlotTable` 实现单调 `GrowTo`：增长保持 min-heap、`nextUnused`、claims、既有 pages、occupants、generation handles、raw runtime/rest；等容量调用为成功 no-op，缩容拒绝且不改变状态。移动端契约同时修正为 **1000 active admission + 1050 logical slot addresses**：保留 `0..49` 后，1000 个动态槽为 `50..1049`；256 槽分页会建立 5 个物理页地址区间，但 `1050..1279` 必须不可访问。fresh 证据为源码 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` `13:25:34` **PASS**；`dotnet build` **0 errors**；架构复核 **PASS**。

R2C-3A 已让 `SimulationWorld.RuntimeSlotCapacity` 读取当前槽表逻辑容量，并将 registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量边界改为当前实例容量。默认 `SimulationWorld()` 仍是 `Authority400/400`；新增 internal `DesktopExtended/512` focused contract 仅验证 slot `511` 注册/查询/AI 可见、slot `512` 拒绝和 reset 清理，不是生产 Profile 接线。`BattleParitySnapshot` 继续固定使用明确的 400-slot authority schema。fresh 证据为相关源码约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` `13:54:22` **PASS**；fresh `dotnet build` **0 errors / 42 warnings**。

R2C-3B 已关闭外部固定容量边界：`LF2SpecialAttack` 的高槽 holder 验证和 Karasu oid209 扫描使用当前 world capacity；`LF2Entity` transition effect 的可用槽计数使用当前 dynamic range。历史 parity capture 现在必须同时满足 `Authority400` Profile 与 400 逻辑容量，明确拒绝 `DesktopExtended/512` 和 `DesktopExtended/400`，避免同容量非 authority world 伪装成旧 certificate。fresh 证据为相关源码 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors**，warnings 为既有告警。

R2C-4 已激活生产 Profile：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用解析/创建路径，直接 `BattleTestBootstrap` 在实体注册前协调晚到的 `GameConfig`。默认容量为 `Authority400=400`、`MobileExtended=1050 logical / TOTAL active admission 1000`（跨全部槽区计数）、`DesktopExtended=512 initial`（按 256-slot 页规范化并自动增长）；Desktop 增长保持最低空洞优先并同步 AI snapshot。Extended Driver checksum 当前跳过/为空，direct parity 仍严格拒绝非 `Authority400/400`。fresh 证据为相关源码 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors / 42 existing warnings**；architect final review **PASS**。

B0 已落地纯数据 X/Z half-open Loose Quadtree shadow：`looseness=1.5`、`leafCapacity=16`、`maxDepth=8`，每次 collision collect 全量重建，诊断默认关闭。诊断比较 brute AABB pair、tree pair 与正式 accepted subset；正式 `i/j`、VRest、RNG、candidate 收集/截断/消费流程保持不变，shadow 结果不写回战斗真值。fresh 证据为相关源码不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` `16:15:43` **PASS**；fresh `dotnet build` **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。该结果不代表性能提升或正式 broadphase 已切换。

B1 已建立纯数据 `RuntimeRestStore`：分页/惰性 ARest；定向稀疏 `VRest[victim, attacker]` 只存正值、写零移除；`ResetSlot` 同时清 ARest、victim row 与 attacker column；支持 `GrowTo`、全局 reset、排序 diagnostics/snapshot 与 restore。2,000 次随机操作已与 dense reference model 逐步 differential。fresh 证据为相关源码 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` `16:37:13` **PASS**；fresh `dotnet build` **0 errors**；architect final review **PASS**。B1 尚未接入生产。

B1.1 已实现 optional `LF2ItrRestTracker` facade 与 exclusive victim-row lease：绑定 store 的 facade 独占一个 victim row，释放后其他 owner 才能接管；未绑定时保留既有 tracker 路径。architect 首轮发现 `ReplaceVictimState` 对 mixed-invalid attacker 输入可能部分写入，现已改为完整预验证后原子替换；direct `ReplaceVictimState` 与 facade `Bind` 均新增 failed-import 原状态不变测试。B1.1 阶段 production world 尚未绑定 facade，后续由 B1.2 接入。复跑 `dotnet build` **0 errors / 18 existing warnings**；相关源码 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` `17:39:07` **PASS**；architect final review **PASS / no blocker**。invalid bound `RestoreState` 可后续补独立断言，但复用已验证 atomic 入口，不构成 blocker。

B1.2 production lifecycle 已完成代码层实施与验证：`SimulationWorld` 持有 store，ordinary claim `ResetSlot + Bind(false)`，release 保留 store 并解绑，`StageSpawnAt` post-Initialize retention，world reset/grow 同步；`RuntimeSlotTable.RawRest` 已删除，parity fallback 直读 store。B1.2 初轮审查发现 Stage pool 回收不完整与错槽 release 未拒绝，次轮发现 release 拒绝未传播，末轮复核 PASS/no blocker；partial import 属于 B1.1，不计入 B1.2 三轮审查。`18:13:00` 与 `18:22:59` 保留为非完成历史证据。最终 `dotnet build` **0 errors**，源码 `18:31:25` < DLL `18:33:58` < self-check `18:34:54` **PASS**。

B1.3 已实现 collision pair VRest tick 解耦：正式顺序为 `CaptureSnapshots -> sparse Tick -> Collect`；eligible `active + CharData` row 递减，inactive row 冻结；`BruteForceSceneQuery` 删除 pair 内 tick。初版 `19:11:13` PASS 后 architect 发现 eligibility 仍按 `RuntimeSlotCapacity` 全扫，该证据保留为非完成记录。最终改为直接遍历 registered bucket items，无 capacity scan/eligibility snapshot 分配；Desktop sparse high-slot 测试 `visited=2`。最终 `dotnet build` **0 errors**，源码 `19:19:14` < DLL `19:19:47` < self-check `19:22:50` **PASS**；architect final review **PASS / no blocker**。

B2A 已实现独立 `CollisionBroadphaseBackend.BruteForce/LooseQuadtree` 正式后端，选择优先级为命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`。它只替换 fixed-tick candidate collect；即时 weapon/body query 不变。formal participant 保留 brute authority ordinal，tree 与 invalid-AABB fallback-all pair 统一转换为 canonical slot pair、排序去重，再按 authority ordinal 双向派发。slot/mapping/index/entry count 非法、rebuild/query 异常或 diagnostics 缺少 brute coverage 时，整 tick 丢弃 formal 输出，原子恢复 RNG/candidate state 并重跑 brute-force；extra pair 交 narrow phase。fresh 证据为源码 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` `22:19:28` **PASS**，`dotnet build` **0 errors**；architect final review **PASS / no blocker**。

B2B 已把 formal backend 从每 tick full rebuild 改为 collision collect 边界的 batch synchronize。索引身份使用 `(runtime slot, generation)` handle：未移动实体保持原记录，AABB 在当前 loose 范围内变化时原位更新，跨 loose 范围时迁移；spawn/remove、valid/invalid AABB 转换与同槽复用都在下一 collect 收口，root escape 才执行 full rebuild。query handle 必须通过当前槽表 generation 解析并核对 entity/ordinal；sync、query、invariant 或 mapping 失败会 reset 索引并走 B2A 的整 tick brute/RNG/candidate rollback。world reset 也显式清空 formal index。fresh 证据为源码 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` `22:47:04` **PASS**，`dotnet build` **0 errors**；architect final review **PASS / no blocker**。

本批未执行 Play Mode。生产默认仍为 `BruteForce`；即时 weapon/body query、AI 查询、Extended parity/replay/checksum schema 与集中式渲染仍未迁移或实施。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT14 DAT movement 显式值读取回归（2026-07-19）

本段覆盖下方 BATTLE-AUDIT13 关于“可玩 Naruto `oid2 running_speed=8`”和“`BattleVisualScale` 临时为 `1`”的旧结论。生产 Naruto DAT 的显式值是 `running_speed=15`，Unity 实体表现缩放已恢复为项目要求的 `BattleVisualScale=1.5`。

| 项目 | 根因 / 契约 | 修复与验证 |
|---|---|---|
| DATA-01A movement regression | DATA-01A 先前把 `LF2CharacterData` 的兜底值从 Unity 旧值 `15` 改为 C# 权威默认值 `8`；但 Unity parser 同时遗漏了 `<bmp_begin>` 内无冒号的 movement `key value`，导致生产 Naruto 的显式 `15` 错误回退为 `8`。这是 Unity loader bug 和本轮对齐回归；先前仅归因于 `1.5` 缩放并不完整 | `Lf2DatParserV2` 只对白名单中的 BMP 顶层 18 个 movement 键支持无冒号 `key value`，不扩大通用语法；`ExtractMovementParameters` 读取 `Bmp.Properties`，浮点数和 `frame_rate` 均使用 `InvariantCulture` 正确解析；DAT 真正缺字段时仍保留 C# 默认 `8` |
| 生产与合成覆盖 | 显式 DAT 值必须覆盖默认值，且 BMP 顶层 movement 不能泄漏到 frame、weapon、stage 或 data 语法 | 生产夹具断言 Naruto `15`、Kakashi `18`、Sakura `17`、Sasuke `23.9`、clone `15`；weapon4 冒号语法 guard；synthetic 覆盖全部 18 键、last-wins、frame 隔离和缺省 `8` |
| 同类遗漏审计 | 审计当前 101 份 DAT；除 5 份角色 DAT 的 18 个 movement 字段外，没有第二组当前生产数据会触发同类无冒号属性遗漏 | weapon/frame/stage/data 现有生产语法安全。多词 `name` 属潜在 parser 表示风险但不进入战斗逻辑；`catchingact/caughtact` 双值是未来风险，当前 218 处两值均相等，现有消费者无可观察差异 |

fresh 证据：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 14:39:43.992`，晚于相关源码，Console C# error 为 **0**。一次请求因 Editor 误留在 Play Mode 未作为结果；退出 Play 后 fresh full `BattleRuntimeSelfCheck` 于 `14:44:58.748` 返回 **PASS**。真实双击 D 的 Play trace 因 UnityMCP 临时注入卡住而未完成，因此本轮不宣称该 Play 场景已验收；T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT13 Naruto 防下攻与跑速缩放复核（2026-07-19）

常规战斗逻辑的唯一权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。本项是用户明确指定的例外：Naruto 防下攻以用户已验证表现正确的 `J:\QQFile\NTSD2.4\ntsd_release` C++ 版本作定向参考；该例外不改变其他战斗逻辑的 C# 唯一权威规则。

| 项目 | 参考行为 / 根因 | Unity 修复与当前状态 |
|---|---|---|
| 防下攻（DDA） | C++ 中 `oid2 frame286` 的 `centery=79`，opoint 为 `y=80 action=240 dvy=0 oid=33`，因此 child 初始 `Y=+1, Vy=0`。角色物理落地要求 `new_y > 0.0001 && pre-move Vy > 0.0001`；初生分身不会立即进入 frame219，而是按 `240 -> 241 -> 242 -> 243 -> 235 -> 236(dvy=-7) -> 244..247` 推进，真实下降落地后才进入 `219 / AI` | Unity 根因是 `CharacterMechanics` 的 `landed` 判定缺少 `Vy` 门槛；旧 `LateOpoint + state15` 专项 gate 范围过宽，并且仍会把 `Y` 钳为 0。现已改为与参考行为一致的通用 `landed` 条件，并移除专项 gate；`CheckLateOpointState15LandingControls` 与 `PH-02` 三向速度矩阵已同步更新 |
| 奔跑与缩放 | 可玩 Naruto `oid2` 的逻辑 `running_speed` 仍为 `8`，固定逻辑频率仍为 30 Hz，本轮未修改跑速规则 | 按用户要求，`BattleVisualScale` 临时由 `1.5` 改为 `1`，仅供用户复测奔跑速度体感；此项是 Unity 表现缩放测试，不代表逻辑跑速发生变化 |

fresh 证据：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 03:21:41.985`，晚于测试时间 `03:20:06.169`；Console C# error 为 **0**；fresh full `BattleRuntimeSelfCheck` result 时间 `03:22:49.668`，结果 **PASS**。本轮没有可复用的真实 Play 自动 trace 入口，因此没有重新运行真实 Play trace；防下攻与 scale 1 奔跑仍需用户手测，当前不宣称 Play Mode 验收通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT12 代码差异清单收口（2026-07-18，Play Mode 除外）

本段覆盖下方 BATTLE-AUDIT9/10/11 的历史冻结状态。按用户限定，本轮只验收脚本定义的战斗逻辑与战斗可观察契约；4 组 Naruto/武器 Play Mode 场景仍由用户自行验证，不在本轮结论内。最新证据链为：相关源码最晚 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` `2026-07-18 16:44:31.210` < Unity `Library/ScriptAssemblies/Assembly-CSharp.dll` `16:45:52.868` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `16:46:29.080` **PASS**。fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 18 warnings**。

| 项目 | 当前代码状态 | fresh 验证 |
|---|---|---|
| `FW-FLOW-01` | 已按 `GameTick.Run` 恢复普通 tick 的 `cooldown -> human input` 顺序 | `CheckFrameworkCooldownBeforeHumanInputOrder` + full self-check PASS |
| `LP-03` | typed/generic 正式投掷均不再写 Unity-only `Zz=1`，release 后保持 `Zz=0` | weapon/generic release 矩阵 + full self-check PASS |
| `LP-05` | formal release 保留 authority `TargetIdx/HolderIdx`，只清 active link 与 held slot；consume 单独写 `0/0`；force-clear 仍执行完整清理 | `CheckAudit7WeaponReleaseTickContracts`、`CheckAudit9GenericHeldReleaseTickContracts` + full self-check PASS |
| `FW-RESULT-01` | 固定 roster slot、dormant/inactive gate、`RelationTeam`（Unity 的 `Unk364` alias）和 alive bucket 已按 authority 收口 | `CheckBattleResultsSlotAndRelationContracts` + full self-check PASS |
| `UNRES.04` | N30 `triggerCode==100` 同 `Unk364` 存活角色坐标广播已落地 | `CheckAudit11N30Code100Broadcast` + full self-check PASS |
| `DATA-01A-D` | `running_speed=8`、frame cache 600、合法缺帧 `EmptyFrame`/authored gate、cpoint action alias 已落地 | `CheckDataDefaultsFrameCacheAndCpointAliases` 及 authored-frame 回归矩阵 + full self-check PASS |

重新核销后，`FW-FLOW-02` 在 Unity 生产代码中没有 writer，权威 writer 仅位于 Host 单步/调试控制，归为 **dormant / scope-excluded**。`FW-BOOT-01` 对 `Unk344/HolderCopy` 的写入只存在于 authority `resultStartRematch` 分支；`FW-BOOT-02` 的普通非-rematch 路径在 reset 后 `HpMax/Hp3=500`、difficulty bonus clamp、PP/respawn/input/Cd/速度字段上与 Unity 现状等价，唯一 `PP=200` 同属 rematch；两项均从正式普通战斗差异中移除。`FW-RESET-01/DEP.RNG.01` 保留为批准的 per-`SimulationWorld` lockstep RNG adapter，不要求改成 authority 的进程静态 owner。

本段只证明上述代码差异已写入且进入 fresh full self-check。它不证明 4 组 Play Mode 场景，也不构成任意角色、任意 DAT、长时间对局的完整逐帧 production certificate。

## BATTLE-AUDIT11 12 项代码核验全部定性（2026-07-18，仅代码层）

本轮完成原 `authority-unresolved` 清单的代码层核验，并已落地对应的 Unity 代码修复。范围严格限定为 C# 权威源码与 Unity 脚本调用链、字段契约、默认值、重置时机和可达分支；不包含 Play Mode、资源部署、DAT 文件表示差异或非脚本表现确认。当前 fresh Unity full self-check 仍为 **FAIL**：2026-07-18 最新 fresh run 的 `CheckStateTransformLandingMatrix` transform fixture 断言失败，实际为 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`；这是既有 transformed landing fixture/代码契约回归，不是 Play Mode 结论。因此生产修复状态为“已落地 / 编译通过 / self-check 阻塞”，不能宣称已对齐。依据报告：

- `.omc/research/final-verify-unres-02-05-code-parity-20260718.md`
- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

代码层结论：

| 分类 | 项目 |
|---|---|
| **equivalent / Unity-adapter** | `UNRES.01`、`UNRES.02`、`UNRES.03`、`UNRES.05`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01` |
| **confirmed code difference** | `UNRES.04`、`DATA-01A`、`DATA-01B`、`DATA-01C`、`DATA-01D` |
| **Unity-adapter / policy-open** | `DEP.RNG.01`（算法等价；RNG owner/reset 边界保留为 Unity lockstep 适配策略待定） |
| **关联确认代码差异** | `FW-RESULT-01` |
| **不计入正式 runtime 差异** | `DATA-01E`（当前 consumer 已屏蔽的 adapter/masked）、`DATA-01F`（schema-only omission）、`DATA-01G`（closed in source） |

`UNRES.04` 的具体差异是：权威 `GameTick` 在 `triggerCode == 100` 的 N30 历史触发中，对同 `Unk364` 的存活角色写入 `Unk3FC/Unk400` 随机坐标；Unity 已补齐该生产广播路径，并加入对应 self-check，但当前 full self-check 被独立的 transformed landing 断言阻塞。

因此，在 **code-only scope** 下，原清单中的 `authority-unresolved` 已由 4 项（`UNRES.02`-`UNRES.05`）降为 **0 项**。这只表示代码层项目已完成定性，不表示生产差异已经全部修复，也不改变用户自行负责的 Play Mode 场景验证状态。`UNRES.04`、`DATA-01A-D` 的首轮修复已落地，但 fresh full self-check 仍被上述 transformed landing fixture 回归阻塞；`FW-RESULT-01` 仍是确认差异，`DEP.RNG.01` 保留为 Unity-adapter/policy-open。本段不构成“完整战斗逻辑已对齐”声明。

## BATTLE-AUDIT10 代码核验（2026-07-18，仅代码层）

本段为 BATTLE-AUDIT10 历史核验快照：当时按用户限定只核验脚本/代码层面的 authority-unresolved 项，未进行 Play Mode、资源部署或场景/表现验证，也未修改生产代码；后续生产修复与当前阻塞状态以 BATTLE-AUDIT11 为准。核验依据为以下三份只读报告：

- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

核验结论：

- `UNRES.01`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01`：代码层已闭合为 **equivalent / Unity-adapter**，从 authority-unresolved 清单移出。
- `DEP.RNG.01`：LCG 算法和单次取值算术与权威一致；owner/lifetime 与 reset/seed 边界属于 Unity lockstep adapter 的策略选择，当前标为 **Unity-adapter / policy-open**，不作为待修复生产差异。
- `DEP.DATA.01`：拆分为 `DATA-01A`/`DATA-01B`/`DATA-01C`/`DATA-01D` 四项 **confirmed code difference**；`DATA-01E` 为当前 consumer 已屏蔽的 **Unity-adapter / masked**；`DATA-01F` 为 **schema-only omission**；`DATA-01G` **closed in source**。
- `FW-RESULT-01`：确认代码差异，非正常 roster/lifecycle 状态下 dormant/inactive 选择及 relation identity alias 与 authority 不同；当前仍未修复、未完成运行时验证。
- `UNRES.02`-`UNRES.05`：**BATTLE-AUDIT10 的历史中间快照**曾暂列为 authority-unresolved；该状态已由 BATTLE-AUDIT11 的代码核验取代，当前 code-only scope 下已分别定性为 equivalent（02/03/05）或 confirmed difference（04）。

本段为 **BATTLE-AUDIT10 历史中间快照，已被 BATTLE-AUDIT11 取代**：当时统计为剩余 authority-unresolved 4 项（`UNRES.02`-`UNRES.05`）。BATTLE-AUDIT9 中既有 LP 项的状态与计数、4 组 Play Mode 未验证场景的计数均不因本轮代码核验改变；这些场景由用户自行验证。本段不构成“完整战斗逻辑已对齐”结论。

## BATTLE-AUDIT9 当前冻结（2026-07-18，先盘点后修复）

本段为 **BATTLE-AUDIT9 历史冻结快照，已被 BATTLE-AUDIT11 代码核验取代**。当时计数为 9 个正式 runtime 差异、1 个 parity trace 工具差异、12 个 authority-unresolved 待确认项和 4 个 Play Mode 未验证场景；其中原 12 项现已在 code-only scope 下全部定性为 equivalent/adapter 或 confirmed difference。逐项权威/Unity 方法、触发条件、预期/实际、分类和证据仍保留在本历史表中。

F1-F7 仅达到 source/static + focused self-check 闭合，尚未全部 Play Mode 复核；DAT 表示差异排除，T8 默认 `stage.dat` 部署继续暂缓，fixed-world camera 为用户批准的 Unity adapter。

### BATTLE-AUDIT9 修复进度（LP-01 / LP-02 / LP-04）

冻结清单建立后，`LP-01`、`LP-02` 与 `LP-04` 已进入“**代码已写 / self-check verified / Play-unverified**”，但仍保留在 BATTLE-AUDIT9 差异清单中，不能据此关闭整个清单。`LP-01` 的 generic held 正式 throw/kind3 释放写回已落在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs` 的 `ThrowHeldObject`、`DropRandomly` 与 `ClearLinks(..., stampReleaseTick: true)`，并由 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 的 `CheckAudit9GenericHeldReleaseTickContracts` 覆盖；`LP-02` 现为 `(ZInt, runtime slot)` dense presentation rank，落在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` 的 `GetPresentationRenderSortingOrder` 与 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` 的 `ForceRefreshPresentation`，并由 `CheckCompactPresentationRenderSorting`、`CheckLegacySpriteRendererPresentationSorting` 覆盖；`LP-04` 的实体/阴影 `HitStop` 阈值与四拍显示门控已落在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` 的 `UpdateSprite`、`ShouldDrawEntityForHitStop`、`ShouldDrawShadowForHitStop`，并由 `CheckHitStopPresentationGates` 覆盖。

本批验证证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 warnings**；fresh Unity full `BattleRuntimeSelfCheck` 于 `2026-07-18 14:01:51.078` 返回 **PASS**，`Assembly-CSharp.dll` 时间 `14:01:27.540` 晚于本轮最新相关源码。该证据关闭三项的编译与 self-check 层级；generic held 实际投掷/掉落、同 Z slot 排序的画面顺序和负 `HitStop` 实体/阴影闪烁仍需 Play Mode 定向验证。

`LP-05`（新增 reviewer 候选，只记录、不修复）：权威 `BattleCore/Interaction/WeaponRuntime.cs:289-295` `ReleaseHeldWeaponRuntime` 只清双方 `LinkState`、写 `ReleaseTick` 并清 held slot，不写 `holder.TargetIdx` 或 `held.HolderIdx`；Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs:23-28,39-59` 当前正式 release 会清 holder `TargetSlotIndex` 并将 held `HolderStableId` 置 `-1`，generic `ClearLinks` 也有同类清理。当前分类为 **confirmed-candidate / 未修复 / 需 authority 调用链与 Play Mode 复核**；它不纳入 `LP-01` 的已写/self-check 结论，也不改动上述冻结计数。

## BATTLE-AUDIT8 当前进度（历史交接，已由上方冻结覆盖）

本轮 BATTLE-AUDIT7 生产修复及新增断言已经进入一次 **fresh Unity full `BattleRuntimeSelfCheck` PASS**：`BattleRuntimeSelfCheck.cs` source `2026-07-18 12:45:10.110` < `Assembly-CSharp.dll` `12:46:15.927` < result `12:46:40.638` **PASS**。其中 F6/R1 的正式 Unity 输入路径已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` 的 `UpdateLocalInputStateFromControllerBuffer` 中先执行 `InputState.SyncFromRuntime(Runtime)`，再轮询 controller buffer；它对应权威 `BattleCore/Input/InputRuntime.cs` 的 human poll/cooldown 单一 runtime 真值，并避免 results-active human observation 使用滞后的 `CdDefendLock`。results early-return 的 pass 边界仍以权威 `BattleCore/Simulation/GameTick.cs` 对照 Unity `NTSDBattleTickSystem.RunReleaseTick`。

为使 `CheckAudit7AppManagerSpawnContract` 能在 EditMode 生命周期下运行，`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 的 `TemporaryAppManagerRuntimeScope` 补齐了测试 fixture 的 singleton/Awake 初始化与清理。这一项**仅是 self-check helper 修复**，没有修改生产 `Assets/NTSD/Scripts/App/AppManager.cs`，不能记作新的生产行为差异或 AppManager 生产修复。

Frame/Input 权威账的 **39 equivalent、181 Unity-adapter、4 confirmed-difference、1 missing、12 authority-unresolved** 计数属于 **BATTLE-AUDIT8 历史静态快照**；完整证据见 `.omc/research/unity-frame-input-mapping-complete-20260718.md`。BATTLE-AUDIT11 已完成原 authority-unresolved 项的代码层定性，当前 code-only scope 为 0；Play Mode 与非脚本表现仍按用户范围处理。

**验收边界**：本次 PASS 只证明当前 fresh DLL 中已被 `BattleRuntimeSelfCheck` 覆盖的断言全部通过，不等于全部战斗逻辑已完成逐帧最终对齐，也不替代必要的目标 Play Mode/双端 trace。DAT 文件表示差异继续不处理；T8 默认 `stage.dat` 部署继续按用户要求暂缓，stage runtime 只使用内存 fixture 验证。

> **当前结论（BATTLE-AUDIT7，2026-07-18）**：撤销此前任何“完整战斗逻辑已对齐”“无剩余差异”的推断。重新以唯一权威 C# 的完整框架、字段和正式调用链做正向映射及 Unity-only 反向审计后，现有静态证据确认 **13 个去重开放根因**：其中 **12 个战斗 runtime/语义根因**、**1 个 parity trace 投影工具根因**。它们均为“已确认、未修复、未运行时验证”。Audit5 的 **74/74** 与原 trace 风险 **15/15** 仍是对应历史批次的真实关闭记录，但不覆盖本轮新发现；`2026-07-18 01:07:52.834 PASS` 与当时 Architect `P0/P1/P2=0` 同样只覆盖当时源码和断言，不能证明 BATTLE-AUDIT7 的开放项已通过。

> 创建日期：2026-07-12
>
> **唯一 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release_C#`。战斗规则、pass 顺序、字段副作用和可观察行为只能以该 C# 工程为准。
>
> **核心入口**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`。旧工程、反汇编和旧对齐结论只保留为历史记录，不得作为当前实现或验收依据。
>
> **历史表说明**：下文历史表中仍可能出现旧来源坐标；这些坐标只说明当时的追踪过程。若与唯一权威 C# 冲突，必须重新按 C# 审计并更新结论。
>
> **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
>
> 说明：
> - 本文只覆盖**战斗相关逻辑**：固定 tick/pass 顺序、输入与 AI、帧推进/状态、实体位移与逻辑 X 边界、碰撞/命中、武器/cpoint/opoint、死亡复活、波次和实体生命周期。菜单、选人、加载、HUD/结算、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施不在本清单内。
> - bg.dat 的 Z 可活动范围与相机/背景表现不对齐，Unity 保留自己的 BoundaryWall + ProCamera2D；但 `ApplyPreframeBounds` 中会改变实体存亡或 X 坐标的逻辑分支仍属于战斗逻辑，不能随表现层一起排除。
> - "冗余脚本可删除"的判定必须严格：**只有在 C# 无对应分支、且 Unity 自身也不引用时才可删**；若只是 Unity 换了一种架构实现同一件事（组合/resolver/partial），**不算冗余，不得删除**。
> - **最终表现效果一致原则（重要）**：对于因 Unity 框架/架构限制而**无法做到逻辑层完全对齐**的项，退而求其次的底线是——**运行时最终表现效果必须与 C# 工程完全一致**（位置、帧号、速度、判定结果、伤害数值、时序等对外可观测行为逐帧等价）。即"实现方式可不同，但结果必须等价"。凡标 🔷 的项，验收标准就是这条：不比对代码是否同构，而比对运行结果是否逐帧一致。
> - 标记含义：✅ 已对齐 / ⚠️ 部分对齐或存疑 / ❌ 缺失或明显偏差 / 🔷 架构不同但结果需等价 / 🗑️ 疑似可删（需二次确认）
> - **历史批次口径（Audit5 + Audit6）**：Audit4 只保留为已完成的定向回归基线。Audit5 原始总账保持 74 个确认差异簇 + 15 个 trace 风险；对应批次曾达到 **74/74 逻辑实现 + focused/full self-check**，原 15 项风险为 **15/15 已关闭**。`BATTLE-AUDIT6-01/02`、CP-NV1/2/3、STEP10 与原 3 个受控 P2 也曾完成生产修复和验证。该历史 freshness 为 source `2026-07-18 01:06:21.499` < Unity DLL `01:07:21.125` < result `01:07:52.834`，结果 **PASS**，Architect 当时复核 `P0/P1/P2=0`。这些结论不覆盖 BATTLE-AUDIT7 新账，不能再据此宣称当前完整对齐。C# 与 Unity 的 raw DAT/manifest 差异仍属于读取方式和 Unity 适配的预期表示差异，不是阻塞或 backlog；T8 默认 `stage.dat` 部署继续暂缓。

---

## 第六次权威框架全量映射审计（BATTLE-AUDIT7，2026-07-18，开放）

### 审计方法、集合覆盖与结论边界

本轮不要求把 Unity 改写成与权威工程同形的纯 C#。`MonoBehaviour`、`GameObject`、`Transform`、对象池、渲染帧和 Unity 生命周期可以保留为适配层；判差异的标准是它们是否改变权威 C# 的 pass 顺序、字段真值、RNG 消耗、对象生命周期或逐 tick 可观察结果。

| 分区 | 正向权威账 | 当前核销结论 | 反向 Unity-only 结论 |
|---|---:|---|---|
| Framework / bootstrap / world / tick shell | **172/172 ID** | 独立复核后为 64 equivalent、57 Unity-adapter、13 confirmed-difference ID、8 authority-unresolved、30 scope-excluded；13 个 difference ID 去重为 7 根因 | 已扫描生产 framework 路径；未发现 F1-F7 之外的新 framework 根因。scene walkability 当前无 mechanics reader，属 dormant adapter；fixed camera 是用户指定适配 |
| Frame / input / physics / runtime | **237/237 ID** | **[历史 BA7 快照，已由 BATTLE-AUDIT8 237/237 分类取代；当前开放项见顶部]**：当时校正后为 4 个 difference ID + 1 个 missing ID，`IN.JUMP.03` 为 equivalent，另有 219 个 ID 尚未逐项拆分；该旧静态边界不再作为当前状态依据 | 正式可达新增是 results 普通 pass 与 late held 逻辑重同步；`SimEntityCollision`、6 个 `Suppress*UntilTick` 无生产效果；input event queue 是 adapter；duplicate player slot 仅 public contract 可构造、正式 provider 不可达 |
| Interaction / collision / hit / cpoint / weapon / opoint / stage | **105/105 ID** | 集合相等只证明 ID 存在；独立复核确认 2 个正式可达差异。原“0 difference / 212 semantic identities 完整闭合”不可作为 parity 结论 | 直接音频播放为表现 adapter；F8、step-wait、Mode2 debug 为 scope-excluded；未发现下列 I1-I2 之外的新普通战斗状态根因 |

Frame/input 权威 ledger 另有两处已校正的账本问题。第一，按分组显式字段机械相加为 138，而 footer 写 137；这是计数口径不一致，不是 Unity 战斗逻辑差异。第二，ledger 曾将 `IN.JUMP.03` 记为成功 `DoFrameJump` 清 8 Cd；当前权威 `InputRuntime.cs:926-927` 与 Unity `NTSDInputStateModule.ClearActionAndDirectionCooldowns` 实际都只清 Right/Left/Up/Down/Attack/Jump/Defend 7 个普通 Cd，并都保留 `CdDefendLock`，所以 `IN.JUMP.03` 应为 equivalent，不进入差异账。`IN.CD.02` 的 AI 递减归属差异仍独立成立，故去重根因总数不变。**“后续逐项拆分 219 个 ID”是 BATTLE-AUDIT7 历史门槛，已由 BATTLE-AUDIT8 的 237/237 完整分类取代。**

权威总账与复核证据：

- `.omc/research/csharp-authority-framework-ledger-20260718.md`
- `.omc/research/verify-unity-framework-mapping-20260718.md`
- `.omc/research/csharp-authority-frame-input-ledger-20260718.md`
- `.omc/research/unity-frame-input-mapping-ledger-20260718.md`
- `.omc/research/csharp-authority-interaction-ledger-20260718.md`
- `.omc/research/verify-unity-interaction-mapping-20260718.md`

### BATTLE-AUDIT7 去重开放总账

状态统一为 **已确认 / 未修复 / 未运行时验证**。表中的“运行时验证”指修复后 fresh Unity 编译、focused/full `BattleRuntimeSelfCheck` 和必要 Play Mode；旧 PASS 不计入。

| 编号 | 根因与关联 ID | 权威 C# 文件/方法 | Unity 对应 | 前置/输入、预期与实际 | 依赖与状态 |
|---|---|---|---|---|---|
| BA7-F1 | 首波 `WaveIdx` 被 Unity 提前推进；`FW-BS-008`，关联 `FW-LC-004` | `DirectBattleBootstrap.InitializeFromConfig` 写 `WaveIdx=-1`；`GameTick.ApplyCurrentWavePhaseAdvance` 在 `<0` 时返回 | `SimulationTickDriver.ApplyMatchConfig` 调 `StartInitialStageWave`，后者 `-1 -> 0` | 前置：加载任意内存 stage campaign。预期：bootstrap 后仍为 -1；实际：Unity 进入 wave 0 | 不部署默认 `stage.dat`，用内存 fixture；**已确认 / 未修复 / 未运行时验证** |
| BA7-F2 | 8-slot roster 被压缩且 independent team 未规范化；`FW-BS-008-B1` | `DirectBattleBootstrap.InitializeFromConfig` 保留原 0..7 index，team 0 规范为 `10+index` | `BattleRuntimeState.ApplyMatchConfig` 跳过 inactive 并按 `writeIndex` 连续写；`AppManager` 实体 team 与 roster raw `-1` 不一致 | 前置：8-slot 中间有洞并含 independent team。预期：原 slot、规范 team、实体 binding/human poll 一致；实际：slot 压缩且正常 unbound roster match 失败 | 依赖 bootstrap fixture 与 input binding；**已确认 / 未修复 / 未运行时验证** |
| BA7-F3 | 初始出生位置与 RNG 消耗不同；`FW-BS-008-B2` | `DirectBattleBootstrap.InitializeFromConfig` 每个有效角色按 bounds 消耗两次 `Rand` 写 X/Z | `AppManager` 使用 scene spawn transform，不消耗 battle RNG，并写入 `PS.x/z` 逻辑真值 | 前置：同 seed、同 bounds、至少一名有效角色。预期：权威 X/Z 和精确 RNG call count；实际：scene 坐标且下游 RNG 序列偏移 | fixed camera 不影响此结论；需先统一 bootstrap RNG；**已确认 / 未修复 / 未运行时验证** |
| BA7-F4 | 初始 `HitStop`/velocity prime 缺失；`FW-BS-009` | `DirectBattleBootstrap.InitializeBattleStats` 写 `HitStop=75,Vx=Vz=0.1,Vy=0` | `NTSDEntityRuntime.Reset` 为 0，`LF2Character.Initialize` 未补 prime | 前置：正常角色出生。预期：首 tick 前为 75/0.1/0/0.1；实际：0/0/0/0，普通命中 gate 和首段运动可变 | 与 F3 同属 bootstrap fixture，但独立根因；**已确认 / 未修复 / 未运行时验证** |
| BA7-F5 | stage spawn 误清复用槽 ARest/VRest；`FW-WR-005`,`FW-TK-028`,`FW-H-050`,`FW-H-059`,`FW-LC-004` | `SimulationWorld.Registry.SpawnAt` 与 stage spawn 不调用 `ResetCooldowns` | `SimulationWorld.Registry.Register` 对所有注册实体无条件清 rest；stage spawn 走该通路 | 前置：later stage spawn 复用带非零 ARest/VRest 的槽。预期：rest 行列保留；实际：被清零 | 需按 spawn semantic 拆 rest policy；默认 `stage.dat` 不需要；**已确认 / 未修复 / 未运行时验证** |
| BA7-F6 | Results active 后缺少权威 early return；`FW-TK-002`,`FW-END-002`,`FLOW.05` | `GameTick.Run` tick header/瞬态清理后，`Results.IsActive` 时只 `RunResultsTick` 并 return | `NTSDBattleTickSystem.RunReleaseTick` 无 results gate，继续 cooldown/input/frame/collision/stage/late/tail | 前置：`BattleResultsFlowAll` 激活 summary 后再跑 1 tick。预期：只推进 header/results；实际：普通战斗状态继续变化 | framework/frame 重复发现只计一次；不要求实现菜单；**已确认 / 未修复 / 未运行时验证** |
| BA7-F7 | hit candidate 瞬态载体晚一 tick 清理；`FW-TK-034`,`FW-H-042` | `GameTick.RunEntityPostframeTail -> ClearHitCandidateCarriers` 当 tick 清 `HitConfirm2` 等 scratch | `EntityPostFrameTailAll` 不清；`BruteForceSceneQuery` 下次 collect 才清 `HitConfirm2` | 前置：weapon/special hit 设置 `HitConfirm2`。预期：interaction 内可见、post-tail/checksum 前归零；实际：跨 tick 残留 | 当前证明 runtime/checksum 差异，未证明招式结果变化；**已确认 / 未修复 / 未运行时验证** |
| BA7-R1 | `CdDefendLock` 递减归属不一致；仅 `IN.CD.02` | `InputRuntime.PollHumanInput -> NtsdEntityInputRuntime.TickCooldowns` 只为 human 递减 8 Cd；AI `PrepareBasic` 不走该 cooldown tick | `VrestTickAll -> TickDefendLockCooldown` 对所有 active entity 递减 | 前置：human/AI 各 `lock=3`。预期：human poll 递减，AI 不递减；实际：Unity human/AI 都在 Vrest pass 递减 | `IN.JUMP.03` 双方均清 7 个普通 Cd、保留 lock，已从差异账删除；ownership 根因 **已确认 / 未修复 / 未运行时验证** |
| BA7-R2 | late holder 切帧后额外重同步 held 逻辑真值；`FLOW.09` | `GameTick.Run` 早期 `SyncHeldWeapons` 后，late `RunLatePerEntityUpdatePass` 不再执行 held sync | `SimulationWorld.LateEntityUpdateAll -> SyncHeldPoseAfterLateHolderFrameChange` 再写 held Frame/Facing/FrameDelay/X/Y/Z/Zz | 前置：holder 在 late `SimFrameTick` 改帧且持有对象。预期：held 本 tick 保持早期同步值，下一 tick 再同步；实际：Unity 当 tick 二次改逻辑帧/位置 | 表现 renderer 可同 tick 刷新，但不得反写逻辑；**已确认 / 未修复 / 未运行时验证** |
| BA7-R3 | `ReleaseTick` runtime 字段和写回缺失；`RT.LINKS.01` | `NtsdEntityRuntime.Links.ReleaseTick` reset/copy/hash；`WeaponRuntime` 两条释放路径写 current tick | Unity 无对应 storage/writer，`BattleParitySnapshot` 固定投影 `-1` | 前置：普通 drop 与 consume release。预期：两路径写当前 tick并进入 hash；实际：字段不存在且永远 -1 | 需 storage、reset、writer、snapshot/hash 一体落地；**已确认 / 未修复 / 未运行时验证** |
| BA7-I1 | IronBall 预处理类型 gate 错把 type6 当 type2；`INT-HIT-005` | `HitResolve.PreprocessCandidate` 仅在 `ObjType == IronBall(2)` 时将 itr `Dvx/Dvy` 各减半 | Unity shared hit preprocess 检查 `Drink/FlyingB(6)` | 前置：相同 itr 分别命中 DAT type2 与 type6 victim。预期：仅 type2 缩放；实际：type2 不缩放、type6 错缩放 | 需 real/shared 生产路径矩阵；**已确认 / 未修复 / 未运行时验证** |
| BA7-I2 | late opoint 使用 spawner 浮点 X/Y，而非权威整数坐标；`INT-OP-001`,`INT-OP-002` | `FrameTick.SpawnFromOpoint` 从 spawner `XInt/YInt` 计算 child，并同时写 child 浮点/整数 X/Y；Z 保留浮点 `spawner.Z+1` | `LF2ObjectPointFactory.ProcessOpointSpawnAlignedToCpp` 从 `PS.x/PS.y` 构造 task，未启用 direct runtime integer position；weapon/special 复制浮点 | 前置：正、负及跨零的小数 spawner 坐标。预期：child X/Y 与 XInt/YInt 均按整数源，Z 保持浮点；实际：child 继承小数并影响 next-tick physics | 需覆盖 weapon/special、slot、velocity 与下一 tick snapshot；**已确认 / 未修复 / 未运行时验证** |
| BA7-T1 | parity snapshot 错投影已有 runtime 字段；`RT.CHECK.01` | `NtsdEntityRuntime` 默认值与 `CharacterSync` hash 投影真实 `ReleaseTick`、Block、`Unk318/31C/324/33C`、owner/关系字段 | `BattleParitySnapshot.ProjectEntityRuntime` 硬编码/错映射空槽 category、release、block、transform/weapon 字段、grabbed/owner/`Unk364` | 前置：default 400 slots、bounds block、transform、weapon、release 场景。预期：snapshot 投影实际 runtime；实际：hash 可假相等或假不同 | 这是验证工具根因，不计入 12 个战斗 runtime 根因；依赖 R3 的 `ReleaseTick`；**已确认 / 未修复 / 未运行时验证** |

### 保留适配、排除项与验收门槛

- DAT 文件及 raw manifest 不要求相同；两端读取方式和 Unity DAT 适配差异不处理，也不作为 BATTLE-AUDIT7 backlog。
- T8 默认 `stage.dat` 资产部署继续暂缓；BA7-F1/F5 使用内存 stage fixture 验证 runtime，不读取、生成或私自部署默认资产。
- fixed-world camera 是用户指定 Unity 适配；Unity-native `Transform`、GameObject/CLR 壳、对象池、渲染刷新和生命周期接入继续保留，前提是不改变逻辑真值。
- F1/F8、step-wait、Mode2 debug、results 菜单交互、普通 HUD、完整 rollback/host rematch 继续排除；results active 后停止普通战斗 pass 仍属于战斗 runtime。
- **BATTLE-AUDIT7/8 历史验收门槛**曾记录 4 个 confirmed-difference、1 个 missing 和 12 个 authority-unresolved；这些是历史中间状态，已由 BATTLE-AUDIT11 的 code-only 定性取代。当前仍不得恢复“完整战斗逻辑已对齐”的结论，原因是确认的代码差异尚未全部修复，且 Play Mode 场景由用户自行验证。

---

## 0. 权威工程 BattleCore 结构 → Unity 映射总表

| C# BattleCore 文件 | 职责 | Unity 对应 | 映射类型 |
|--------------------|------|-----------|---------|
| `Simulation/GameTick.cs` | 单 tick 总调度（顺序主干） | `Simulation/NTSDBattleTickSystem.cs` + `SimulationWorld.Passes.partial.cs` | 🔷 pass 拆分 |
| `Simulation/NtsdBattleTickSystem.cs` | tick 外层入口 | `Simulation/NTSDBattleTickSystem.cs` | ✅ |
| `Simulation/SimulationWorld.cs` | 世界容器/对象池 | `Simulation/SimulationWorld*.cs` | 🔷 固定槽 vs 动态槽 |
| `Frame/FrameTick.cs` | frame_tick 帧推进 | `Character/FrameTransistor.cs` + `LF2Entity.RunCommonFrameTick` | 🔷 |
| `Frame/FrameAdvance.cs` / `Physics.cs` | 帧推进物理 | `Character/CharacterMechanics.cs` + `PhysicsState` | 🔷 |
| `Interaction/HitResolve.cs` | 命中结算（kind 0~16） | `LF2CharacterHitResolver.cs` + `LF2Weapon.ApplyHitEffects` + `LF2CharacterDatHitResolver` | 🔷 分散到多类 |
| `Interaction/CollisionCollect.cs` | 候选收集 | `Character/BruteForceSceneQuery.cs` | 🔷 |
| `Interaction/CPointRuntime.cs` | 抓取 cpoint | `LF2CharacterCatchResolver.cs` + `PreInteractionTickAll` | 🔷 |
| `Interaction/WeaponRuntime.cs` | 持武器同步/投掷/掉落 | `LF2WeaponHeldStateResolver.cs` + `LF2WeaponReleaseFlowResolver.cs` | 🔷 |
| `Interaction/ObjectPointFactory.cs` (`FrameTick.SpawnFromOpoint`) | opoint 生成 | `Character/LF2ObjectPointFactory.cs` | ✅ Naruto DDJ 生命周期差异修复后已验证 |
| `Input/InputRuntime.cs` | 输入消费 + AI | `Input/CharacterInputModule.cs` + `LF2Entity` shared-DAT 桥 | 🔷 |
| `Entity/Entity.cs` (大字段实体) | 实体真值 | `NTSDEntityRuntime.cs` + `LF2Entity` | 🔷 字段化 |
| `Entity/NtsdCharacter/NtsdWeapon/...` | 实体类别 | `LF2Character/LF2Weapon/LF2SpecialAttack/LF2OtherObject` | 🔷 |

---

## 1. Tick 主循环顺序（C# authority vs Unity pass）

C# `GameTick.Run` 是唯一正式顺序。Unity 拆成 `NTSDBattleTickSystem` 调度多个 `SimulationWorld` pass，两侧顺序必须逐段等价。

| # | C# 正式顺序 | Unity pass | 状态 |
|---|------------------------|-----------|------|
| 1 | `GameTick++` / `InputPhase` / `FrameMod12` / `FrameToggle` | `NTSDBattleTickSystem` + `BattleRuntimeState.Flow` | ✅ `AdvanceBattleFlowTick` 在 tick 头统一推进四项；state 400/401 读取持久化 `FrameToggle` |
| 2 | 清瞬时状态 `PendingSounds.Clear()` 等 | 战斗候选载体在 `EntityPostFrameTailAll` 清理 | 🔷 音频/overlay 瞬时状态排除；战斗候选清理已存在，仍随碰撞快照专项验收 |
| 3 | `RunCooldownsTick`（arest-- + attack_exempt 清理） | `VrestTickAll` + `ClearAttackExemptIfCurrentFrameCannotHit` | 🔷 |
| 4 | `GameTick.Run:61-62` `postCooldownInput` callback | `PostCooldownHumanInputAll` → `AiInputAndComboAll` | ⚠️ 历史自检曾通过；当前必须以 C# callback 契约重新核验 |
| 5 | `GameTick.Run:63-64` `RunOid5152RuntimeMaintenance`（实现见 `:1093-1263`） | `Oid5152RuntimeMaintenanceAll` + `TryMergeOid7Or8Into51` / `TrySplitOid51BackToPair` | ✅ Audit6 已按唯一 C# 权威重审并经 fresh full self-check |
| 6 | `GameTick.Run:75-78` `ApplyCharacterInputPass` | Unity 输入 pass 拆分 | 🔷 以 C# 正式调用顺序判定等价性 |
| 7 | `RunEarlyStatePasses`（400/401/500/501） | `EarlyFrameAdvanceSpecialsAll` | ✅ 含 BMD-023 修复 |
| 8 | `FrameRuntimePasses.RunFrameLogic`（hit_fa>0 非角色） | `FrameLogicBeforeAdvanceAll` | ✅ |
| 9 | `RunFrameAdvance`（所有 active，清方向键 + 帧推进） | `SerialTickAll`（SimTransit+SimTU） | 🔷 |
| 10 | `RunPostFrameAdvanceStatePasses`（9998 清理 + 复活） | `CleanupState9998Entities` + `PostFrameAdvanceDeathCleanupAll` + `RunReleaseEntityCleanupTail` | ✅ 复活由 T5 完成并通过运行时自检 |
| 11 | `ClampCharactersToStageZ` | (Z 边界，属可活动范围) | 🚫 不对齐 |
| 12 | `RunCPoint` | `PreInteractionTickAll`→`RunCpointCheckStep10` | ✅ |
| 13 | `SyncHeldWeapons` | `RunWeaponSyncHeldStep10` | ✅ |
| 14 | `ValidatePositiveLinks` | `ValidateHeldLinksAll` | ✅ 全局扫描 active slot `0..399`；invalid 只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`，不清 target 反向字段 |
| 15 | `RunHeldWeaponStep12` | `PreInteractionTickAll` 内 | ✅ |
| 16 | `SnapshotPrevFrame2` | `CaptureCollisionFrameSnapshotsAll` | ✅ |
| 17 | `CollectCandidates` | `CollectCollisionCandidatesAll` | ✅ |
| 18 | `ResolveCharacterHits` | `PostInteractionTickAll`（角色候选消费） | 🔷 |
| 19 | `RunNaturalRandomWeaponDrop` | `RandomWeaponDropTickAll` | ✅ |
| 20 | `RunF8WeaponDrop` | **未找到 F8 路径** | 🗑️? 调试功能，见 §7 |
| 21 | `ResolveObjectHits` | `ObjectInteractionTickAll` | 🔷 |
| 22 | `ApplyPreframeBounds`（含相机/bg） | `ApplyPreFrameBoundsAll`（只做逻辑边界） | 🔷 相机部分不对齐 |
| 23 | `ApplyCurrentWavePhaseAdvance` / `StageSpawns` | `CurrentWaveStageTickAll`（`SimulationWorld.StageWave.partial.cs`） | ✅ 已完成并通过 fresh Unity 运行时验收 |
| 24 | `ApplyFramePostProcess`（HitCount→Vx 平均） | `FramePostProcessAll` | ✅ |
| 25 | `RunLatePerEntityUpdatePass` | `LateEntityUpdateAll` | ✅ 主对齐点 |
| 26 | `RunMode2RandomWeaponDrop` | `Mode2RandomWeaponDropTailAll` | 🚫 C# baseline 的 F7-F9/debug 控制路径，不作为正式战斗对齐项 |
| 27 | `RunEntityPostframeTail`（heal/catch timer） | `EntityPostFrameTailAll` | ✅ heal/catch timer 与战斗候选载体清理已落地；`InitStats`/mode2 debug 分支排除 |
| 28 | `UpdateBattleResultsFlow` | (结算流程) | 🚫 非战斗运行时范围 |

**关键差异**：
- C# 是**固定 400 槽 `Objects[]` 线性遍历**；Unity 是**动态 runtime slot + SortedDictionary bucket**。这是 🔷 架构差异，结果需等价，遍历顺序必须仍是 slot 升序。
- C# `RunLateEntityUpdate` 单函数内顺序：`RunStateSpecialPreCollision → RegeneratePreCollisionStats → FrameTickRuntime.Tick → 帧组1100/1200 → 死亡掉武器/弹地 → ProcessOpointSpawn → 破武器回收 → RunN30InputTrigger → SpawnStateTransitionEffects → PrevFrame 镜像`。Unity `LateEntityUpdateAll` 已按同序拆分（✅），但 **`RegeneratePreCollisionStats`（HP/PP 自然恢复）** 的位置需核对（见 §5）。

---

## 2. 受击/命中结算（`HitResolve.cs` vs `LF2CharacterHitResolver` + `LF2Weapon`）

C# 把**所有对象**的命中都集中在 `HitResolve.ApplyCandidate`（一个 switch(kind)）。Unity 拆成三条独立路径：
- 角色被击 → `LF2CharacterHitResolver.ResolveHit`
- 武器被击 → `LF2Weapon.Hit` / `ApplyHitEffects`
- 非角色 DAT 实体 → `LF2CharacterDatHitResolver`

这是 🔷 架构差异（合法）。以下逐 kind 核对行为是否等价。

| kind | C# `HitResolve` 分支 | Unity 分支 | 状态 |
|------|---------------------|-----------|------|
| 0/4，以及预处理后的 9→0 → 伤害 | `ApplyDamageCandidate` | `ResolveHit` 普通伤害入口；raw kind9 先由 `BruteForceSceneQuery` 转为 kind0 | ✅ alternate 路径已补齐并运行验证，见下方逐点 |
| 6 | `victim.HitConfirm=3` | `HitConfirmEa=3` return | ✅ |
| 8 | `ApplyKind8`（heal_timer/传送） | `ResolveHit` kind 8 | ✅ |
| 10/11 | `ApplyKind10Or11`（笛子）：kind==11 && weaponCount>=0 return false；WeaponCount=FluteForce 值；Falling 双倍伤害 | `LF2CharacterHitResolver.cs:357-369`（✅）+ `LF2Weapon.cs:481-501`（✅） | ✅ |
| 14 | `ApplyKind14`（方向阻挡） | `ResolveHit` kind 14 + `ApplyKind14DirectionalBlockFrom` | ✅ |
| 15 | `ApplyKind15Movement`（KnockbackVx/Vx/Vz/YInt=-2，按对象类型分 vyStep=3.0/2.3） | `LF2CharacterHitResolver.cs:373-380` 简化实现；武器侧 `LF2Weapon.cs:503-506` `WhirlwindForce` | ⚠️ 形式不同（C# 走 KnockbackVx+真实 Vx/Vz+设 YInt=-2 三段；Unity 走 PS.vx/vz 增量；C# 按对象类型分 3.0/2.3 vyStep，Unity 未区分） |
| 16 | `ApplyKind15Or16` kind=16 路径：Hp-、KillStat++、ComboCountAtk、SFX_065、frame=200、vrest 写入、LinkState 断开 | `LF2CharacterHitResolver.cs:383-390`：`ImmediateFrame(MpDrain=200)` ✅ + MaxMP 缩放伤害 ✅；**缺** KillStat++、ComboCountAtk、SFX_065 音效、vrest 写入、LinkState 断开处理 | ⚠️ |
| 1/3 | `ApplyKind1Grab`/`ApplyKind3Grab` | 走 pre-interaction（`LF2CharacterInteractionResolver`） | 🔷 时序不同，见 §4 |
| 2/7 | `ApplyPickupCandidate` | pre-interaction | 🔷 见 §4 |
| kind 4+WeaponCount>0→0 + dvx 翻转 | `PreprocessCandidate` 154-172 | `BruteForceSceneQuery.cs:602-615` 完整实现（kind 翻转 + dvx 翻转按 PS.dir） | ✅ |
| kind 5 委托攻击 | `PreprocessCandidate`（holder wpoint 替换） | `ResolveHit` kind 5（TrackerParent） | ✅ |
| oid 300 特判 | `ApplyOid300SpecialHit` | `ResolveHit` `ObjectId==300` 分支（`LF2CharacterHitResolver.cs:279`） | ✅ |

### 2.1 kind 0/4/9 伤害主流程逐点核对

C# `ApplyDamageCandidate`（character victim）关键顺序：

1. `itrArest = (itr.Arest < 4 && itr.Vrest == 0) ? 4 : itr.Arest`（`HitResolve.cs:268`） — ✅ **C# 用 Arest 判定 + 取值**
   Unity 已由 `LF2Entity.ResolveArestCooldown` 统一实现同一公式，并供普通角色命中路径复用；`CheckArestCooldownRule` 已在 Unity batchmode 中通过。
2. IronBall victim → dvx/dvy 减半（`PreprocessCandidate`）— Unity 在 `LF2Weapon` 侧，角色路径无此（正确，角色不是 IronBall）
3. alternate 受击路径 — ✅ **已完整落地并通过 Unity 运行时自检**：
   - C# `ShouldUseAlternateHurt`（629-680）→ `ApplyAlternateDamage`（实际逻辑延续到约 line 827）。Unity 以共享 `LF2AlternateDamageResolver` 承载，真实 `LF2Character.Hit` 由 `LF2CharacterHitResolver` 接入，当前 DAT 为角色但 CLR shell 非角色的对象由 `LF2CharacterDatHitResolver.TryResolveHit` 接入；两条入口调用同一 `ShouldUseAlternateHurt` / `ApplyAlternateDamage`，并各自只记录一次 `RecordKind0Hit`。
   - `ShouldUseAlternateHurt` 已覆盖 oid 37/6/52 的 `HitStateCount`/frame 窗口、heavy effect、attacker oid 214/208，以及 `PrevFrame2` state 7 的 HP、`bdefend`、朝向、负 `dvx` 和特殊攻击者判定。
   - 伤害契约为 `FallDamageDiv` 整数换算后 `reducedInjury = injury / 10`；扣 `HP`，`HPBound -= reducedInjury / 3`（整数除法），不累计 `HPLost`。致死与统计副作用使用 holder-copy 的 `KillStat`/`ComboCountAtk`、victim `ComboCountVic`，并以 `Unk344` 索引稳定 3 槽 `KillStats`/`DamageStats`；世界 reset 保持数组 identity 并清零内容。
   - 其余已覆盖 `Fall=80`、hit/attacking 计数、attacker/victim/negative-link holder 的 FrameDelay、attacker-only AttackExempt、vrest clamp、frame 111/112 保留 wait counter、ground/air knockback、state 1002/2000/3000 尾分支。state1002 随机切帧只改 frame/速度，不额外写 `Runtime.WeaponState`；状态判断继续以当前 `Frame.D.state` 为准。
   - heavy weapon 普通伤害的减半发生在 alternate 判断之后，因此 alternate 始终消费原始 itr，不会错误变成 `injury/20`。`ApplyAlternateDamage` 本身也保留 character DAT/type guard，不能被非角色 victim 直接调用。
   - **raw kind9 不直接触发 alternate**：真实角色与 shared-character-DAT 两个 caller 都以 `itr.kind != 9` 为门；raw kind9 必须先由 `BruteForceSceneQuery.ResolveRuntimeItrForPair` 转换为 kind0，才会在非 kind9 普通伤害入口判断 alternate。`LF2SpecialAttack` 也统一在 object interaction pass 使用这条预处理，覆盖 kind4 的 `WeaponCount`/反向 `dvx`（读取逻辑真值 `Dirh()`/`Runtime.Vx`）和 kind9 的 kind0 转换/攻击者 HP 清零。
   - alternate 已写入的 clamp 后 vrest 不会再被角色 DAT、武器或技能对象外层 generic rest 更新覆盖。type3（`Consumable3`/Unity `SpecialAttack`）lead sound 条件已按权威修正；该声音分支属于代码权威对齐，headless 自检无法直接观测音频播放。
   - 针对性自检：`CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess`；均包含在 2026-07-14 02:54:22 的 fresh Unity batchmode PASS 中。
4. fall 累积档位（Light/Medium/Heavy/Fall 阈值 → frame 220/222/224/226/180/186）— Unity `HitFall`/`HitFallDown` ✅ 已对齐（注意 5f/7f→0.714 修复已在）
5. `victim.HitStateCount = 45` → Unity `SetHitStateCount(45)` ✅
6. `attacker.FrameDelay=3 / victim.FrameDelay=-3` 普通路径 — Unity 多处 `-3` ✅；alternate 路径独立写 `victim.FrameDelay=-5`，并传播 negative-link holder delay ✅。
7. 攻击方攻击豁免写入 — Unity `attackerLiving?.HitCounters?.SetAttackExempt(exemptVal)` ✅（公式按点 1 修正）

### 2.2 武器被击（`LF2Weapon.ApplyHitEffects` vs `HitResolve.ApplyObjectHurtTail`）

Unity `LF2Weapon.ApplyHitEffects` 已注明"C# baseline: ApplyObjectHurtTail + ApplyStandardDamageKnockbackX"，逐段抄写。核对：
- `FallCounter += fall!=0?fall:20` ✅
- `lightThrow||heavyLike||specialLike → FallCounter=80` ✅
- ApplyStandardDamageKnockbackX 五分支（固定5 / state2000+dvx / FlyingA/B scaled / effect22/23 / 常规）✅
- knockback 帧 180/186 + KnockbackVy ✅
- 攻击者 state 1002 反弹 / state 2000 减速 / state 3000 归 frame 10 — Unity `ApplyAttackerResponse` ✅

**✅ `RecordKind0Hit` 已统一**：`LF2Entity.RecordKind0Hit` 承载 C# timer、owner、随机坐标和 10 槽上限语义，角色与 `LF2Weapon.ApplyHitEffects` 的 kind0 路径均接入；`CheckKind0HitRecords` 已在 Unity batchmode 中通过。

---

## 3. 帧推进（`FrameTick.cs` vs `FrameTransistor` + `RunCommonFrameTick`）

C# `FrameTick.Tick` 是单函数，Unity 拆成 `FrameTransistor.Trans()`（wait/next 推进）+ `LF2Entity.RunCommonFrameTick`（前置门控 + 倒计时）+ hook（`OnFrameTickBeforeWaitAdvance` / `OnFrameTickAfterWaitAdvance`）。

| C# `FrameTick.Tick` 步骤 | Unity | 状态 |
|--------------------------|-------|------|
| `ThrowFrameGuard==Frame` early return | `RunCommonFrameTick` 门控 | ⚠️ 需确认 |
| `FrameDelay!=0 && !Consumable3` return | ✅ | ✅ |
| `AttackExempt--` | ✅ | ✅ |
| `LinkState<0` return | ✅ | ✅ |
| cpoint kind==2 return | ✅ | ✅ |
| Consumable3 + hitA>0 → HP-=hitA, HP<=0 跳 hitD | `LF2Entity.RunCommonFrameTick` type3 分支 | ✅ |
| HitStop/Fall/HitStateCount/HitConfirm 倒计时 | `RunCommonFrameTick` | ✅ |
| frame!=waitCounter → 音效+attacking=0 | `FrameTransistor.Trans` frame 变化清 attacking | ✅ |
| `attacking++` | `Trans.AttackingCounter++` | ✅ |
| state 0 + YInt<0 → frame 212 + SuppressJumpInit | `OnFrameTickBeforeWaitAdvance` | ✅ BMD-023 相关 |
| IronBall state 2000 静止 return | `LF2Weapon.ApplyObjectSpecificFrameTickBeforeWaitAdvance` | ✅ |
| state 14 HP<=0 → HitStop=30 | `RunCommonFrameTick` | ✅ |
| state 2000 facing=vx | ✅ | ✅ |
| `attacking>wait` → next 换帧 | `Trans` attacking>wait | ✅ |
| next=999 → 212/0（空中角色） | `ResolveFrameTickNext999Target` | ✅ |
| next<0 翻面 | `Trans` switchDir | ✅ |
| 上一帧 state14→非13 的 HitStop=15 逻辑 | `OnFrameTickAfterWaitAdvance` | ✅ 含 oid/5==3 skip + difficulty 分支 |
| frame 212 + JumpInitPending → 跳跃初速 | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame mp<0 PP 扣费 + hitD turn | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame 110/114 → CdDefendLock=3 | `RunCommonFrameTick` 尾 | ✅，`CheckFrameTickDefendLockTail` 运行通过 |
| frame 202 → HitStop=20 | ✅ | ✅ |

**结论**：帧推进主干及上述 state14、frame mp、110/114、202 尾部特判均已核实对齐（🔷 hook 拆分合法）。

**逐点核实结果（§3 全部）：**
- §3-1 state14 入口 HitStun=30 + AttackingCounter=0（KillCount>=0 OR Unk364==5 OR slot>=20）— Unity `LF2Character.cs:2205-2211` ✅ **完整对齐**
- §3-2 state14→非13 复活 HitStun=15 分支（aiControlled 检查 + Difficulty!=2 + oid/5==3 + GameMode==1/4 + Oid!=38）— Unity `LF2Character.cs:2134-2163` `ApplyCommonCaughtExitHitStop` ✅ **完整对齐**
- §3-3 frame mp turn-around（C# `HitResolve.cs:178-203`）— Unity `LF2Entity.cs:3284-3321` `ApplyCommonFrameTickPpDisplayPostAdvance` ✅ **完整对齐**（含 PP 扣费、frame.hitD turn、Dual KeyLeft/Right + Facing + YInt==0 条件）
- §3-4 frame==202 → HitStun=20 — Unity `LF2Entity.cs:3634-3635` ✅
- §3-5 frame==110 || frame==114 → `CdDefendLock=3` — Unity `LF2Entity.RunCommonFrameTick` 尾部已实现，runtime Reset/cooldown 衰减已承载；`CheckFrameTickDefendLockTail` 已运行通过 ✅

---

## 4. 交互（pre/post-interaction, cpoint 抓取, opoint）

### 4.1 命中候选消费时序差异（重要）

C# 在 `HitResolve.ApplyCandidate` 里**同一个 switch** 同时处理攻击(0/4/9)、抓取(1/3)、拾取(2/7)。Unity 分成两个阶段：
- `PostInteractionTickAll` → 角色候选消费（攻击 + pre-interaction 混合，`LF2CharacterInteractionResolver.TryConsumeUnifiedStep7CandidateSequence`）
- `ObjectInteractionTickAll` → 武器/技能候选消费

🔷 这是合法架构差异，但 **候选序列消费顺序必须与 C# 一致**（按 step6 收集顺序）。Unity 已用 `TryGetCollisionCandidateSequence` 保序 ✅。

### 4.2 抓取 cpoint

| C# | Unity | 状态 |
|----|-------|------|
| `ApplyKind1Grab`/`ApplyKind3Grab`（命中即建立） | `HandlePreInteractionKind`（pre-interaction 建立） | 🔷 时序不同 |
| `AlignGrabPair`（对位公式 centerx/wact/lerp） | `ApplyImmediateCatchPairState`（同公式） | ✅ 公式一致 |
| `CPointRuntime.Run`（step10 维护） | `RunCpointCheckStep10` + `RunCpointMismatchTailStep10` | ✅ |
| cpoint kind==2 受击 fronthurtact/backhurtact | `ApplyCaughtVictimHurtFrame` / `TryCaughtA` | ✅ |
| throwvx/vy/vz 投掷 + throwinjury | `LF2CharacterCatchResolver`（自检覆盖） | ✅ 有 BattleRuntimeSelfCheck |

### 4.3 opoint 生成 — ✅ 已在 `skill_release_flow_comparison.md` 验证一致

`FrameTick.ProcessOpointSpawn` / `SpawnFromOpoint` vs `LF2ObjectPointFactory.ProcessOpointSpawn` / `ProcessOneLateOpoint`：条件（kind>0 && oid>0 && attacking==0 && (角色→FrameDelay==0)）、facing 展开（>10 → count/facing）、多发 AttackExempt+VRest 扩散、state 3003 linked slot vrest — 均已对齐。

**2026-07-16 Naruto DDJ 完整链专项回归：** 既有 combo wrapper 测试只能证明输入可跳到技能起始帧，不能证明递归 opoint 和对象池生命周期正确。本次按真实 DAT/authority 链新增端到端断言：

- 同 tick held chord 内部输入 `att + down + def` 命中 Naruto frame `271`；随后 frame `272` 生成 oid205/action98，辅助链继续经过 99/325/341；frame `273` 生成 oid204/action130，并展开六个分支，最终各自到 frame `147` 生成 `6 x oid33/action307`。clone 从 307 后落地进入 frame `219` 是 authority 行为，不应把 219 误判为生成失败。
- 新确认差异 1：`LF2ReferencePool.Release` 无条件接收外部 synthetic 实例，造成逻辑池类型污染。
- 新确认差异 2：factory 角色 opoint 在 `ModuleBind` 注册前用 `slot < 0` 过早拒绝，合法生成会被提前丢弃。
- 新确认差异 3：tick 中 pending-unregister 对象同 tick 归池复用时，旧生命周期仍留在 registry bucket；后续 `Register` 被旧 bucket 的 `Contains` 拒绝，递归六分支只生成 3 个 clone。
- 新确认差异 4：池化 `LF2Character.Init` 没有重新分配 `StableId`，复用角色无法保持独立生命周期身份。
- 新确认差异 5：`SpawnFromOpoint` 缺少 `RelationTeam`、`Unk364` 与 holder-copy 继承，生成角色的关系字段与 authority 不完整。
- 已修复契约：`Release` 只把 active 实例归池；`Register` 先 finalize 旧 pending lifecycle；`slot < 0` guard 移到 `ModuleBind + Initialize` 之后；character `Init` 重新 `AllocateStableId`；`PostInitLiving` 继承 `Team`、`RelationTeam` 与 holder-copy（含 `Unk364`）。
- 回归结果：PP `500 -> 295`，所有生成对象使用 dynamic slot，6 个 clone 拥有 6 个唯一 `StableId`，均实际到达 action/frame `307`，且 6 个 renderer 均可见。

**真实 Unity Play Mode 生产输入链验收：** 在 `NTSD_Battle` Play Mode 中等待 slot0 的 `CharacterInputModule`/`ActionMap` 就绪，再通过 UnityMCP 临时 `InputSystem.Keyboard` 事件按默认物理绑定依次注入 `L (Defend) -> S (Down) -> K (Jump)`。事件真实经过 `InputActionMap -> CharacterInputModule -> SimInputBuffer`，没有直接调用技能、写帧或调用 opoint。观测日志为 `INPUT focused=True buffered=1, attackAction=0, jumpAction=1, defendAction=1, moveY=-1`；这里的 crossed internal mapping 是项目/C# baseline 的预期映射，不是错误。运行结果：

- `frame271=True`，`max204=11`，`max205=3`，`maxClones=6`，`maxSpriteReady=6`，`maxVisible=6`。
- clone 数量时间线：`t=0.446: 3`、`t=0.473: 4`、`t=0.509: 5`、`t=0.541: 6`；测试窗口无异常。
- 峰值截图：`Temp/naruto-ddj-unitymcp-peak.png`。
- 验收限制：Win32 `keybd_event` 不被 Unity RawInput 接收，因此本次不是物理硬件键盘证明；它证明的是 UnityMCP `InputSystem.Keyboard` 事件通过完整生产输入链可以稳定释放真实六分身技能。

---

## 5. HP/PP 自然恢复 + heal/catch timer

**✅ HP/PP 自然恢复语义对齐**（逐字段核实）：
- C# `RegeneratePreCollisionStats`（`GameTick.cs:1474-1519`） vs Unity `LF2Character.cs:2534-2584`：
  - HP `Hp < HpMax`（HP < HPBound）每12tick+1 ✅
  - `hpForRate = Hp; >500 → 500; oid 51/52 /=2; PP += (500-hpForRate)/100+1` ✅
  - `WeaponCount<0` 每12tick 扣血（injury=900/FallDamageDiv）✅，HP -= injury、HPBound -= injury/3、ComboCountVic += 9 ✅
- 字段映射：`HpMax`↔`HPBound`、`Pp`↔`PP`，通过 `Runtime.HpMax` / `Health.HPBound` / `Runtime.Pp` / `Health.PP` 字段映射。
- 调用入口：Unity `RunPreCollisionRecoveryPhase` 虚函数（`LF2Entity.cs:972` + `LF2Character.cs:2619-2622`），由 `SimulationWorld.Passes.partial.cs:264` 调用。✅

**heal/catch timer（C# `RunEntityPostframeTail`）**：Unity `EntityPostFrameTailAll` 覆盖 HealTimer/CatchTimer/state1700 ✅（之前已确认）。

---

## 6. 输入 + AI

### 6.1 玩家输入消费（`InputRuntime.ApplyCharacterInput` vs `CharacterInputModule` + `LF2CharacterActionResolver`）

C# `ApplyCharacterInput` 单函数：combo wrapper → hitA/hitD/hitJ frame jump → frame110 facing → state 301/19 lane → LinkState2 heavy → frame215 landing → frame182/188 recovery → state 0/1/2/4/5 分发 → ApplyFrameVelocityTail。

Unity 有两套：
- `LF2Character` → `LF2CharacterActionResolver`（完整角色输入）
- `LF2Entity` shared-DAT 桥（`RunSharedCharacterDatStandingActionInputPhase` 等，用于"当前 DAT 是角色但 CLR 实例不是 LF2Character"的 transform 后对象）

🔷 合法架构分层。**注意**：shared-DAT 桥自称"最小实现"，只覆盖 standing/walking/running/dash/jump 基础，**不覆盖 combo/catching/held-weapon 全动作**。这不是冗余 —— 它服务 transform（state 501/4000/8000）后仍挂在 wrong shell 的角色。

关键值对齐（已修复）：
- walk 斜向 `Vx *= 5.0/7.0` = 0.7142857142857143 ✅（两侧都是）
- heavy run 斜向 `Vx *= 5f/6f` / `0.8333...` ✅

**✅ combo wrapper（DJA 等 9 组方向+攻击/跳连招）已落地并补 fresh 运行时验证**：Unity 现已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6（Sasuke）DjaGuard 特判，真实输入消费路径是 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。本轮新增 `BattleRuntimeSelfCheck` 覆盖 9 组连招帧跳与 oid6 guard hold/release，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。

### 6.2 AI（`InputRuntime.PrepareAiInputBasic`）

**✅ AI 输入生成器已完整落地并通过 fresh Unity 运行时验证**：
- C# `InputRuntime.cs:16` `PrepareAiInputBasic`（~600 行巨型函数，oid 专属 combo 决策、C8 威胁扫描、7A/7B 守卫、队友守卫、held weapon 决策、历史闸门、oid1/4/5/33/52 多种 oid 专属 combo）。
- 实际包含 14 个辅助函数（已 grep 确认）：
  - `AiBetweenX`、`AiPostCacheCoordinateAllowsSpecial`、`AiPreUpdateTarget3000SideEffect`
  - `AiUpdateOid33_19_16PredictedDuaDecision`、`AiUpdateOid52_1_2_21PreLabel591Decision`
  - `AiUpdateLabel591Oid51_2_18_7Decision`、`AiUpdateFirstDecision`、`AiUpdateTeammateGuardDecision`
  - `AiUpdateOid1ComboDecision`、`AiUpdateCloseOid1Decision`、`AiUpdateOid4ComboDecision`、`AiUpdateOid5ComboDecision`
  - `AiProcessSubOidGroup`、`AiSpecialOidForSubGate`、`AiProcessHelper`
- Unity `SimulationWorld.AiInput.partial.cs` 已覆盖主入口及文档原先漏列的 target/team/move-mode/no-target/三个 `AiProcessSub*` 等完整直接/间接 helper 闭包。
- 输入 pass、runtime 字段、deterministic RNG、runtime-slot 顺序、shared-DAT shell 与 roster/opoint bootstrap 均已接通；fresh build 0 errors，fresh Unity batch 自检通过。

---

## 7. C# 有、Unity 未确认/缺失的战斗逻辑（重点排查项）

| 编号 | C# 逻辑 | 位置 | Unity 状态 | 判定 |
|------|---------|------|-----------|------|
| M-1 | **oid 7/8 → 51 合体 / 51 拆分**；唯一权威为 C# `GameTick.cs:1093-1263` | `GameTick.Run:61-64` 的 input poll 后、正式 character input 前 | `NTSDBattleTickSystem` / `SimulationWorld.Passes` / `NTSDEntityRuntime` / `BattleRuntimeSelfCheck` 已按 poll → M-1 → input apply 分相 | **✅ Audit6 生产修复、延迟 split/输入 gate 矩阵和 fresh full self-check 已通过** |
| M-2 | **复活 pass**（`RunRespawnPass` `GameTick.cs:839-934`：state14+HP<=0 + HitStop 窗口 + 两分支[Hp2Overlay/RespawnCount] + 队友位置平均 + Pp=500/HpMax=Hp3 + Frame=212/YInt=-300 + 生成 oid998 复活特效） | GameTick step10 | ✅ `SimulationWorld.Passes` / `BattleRuntimeSelfCheck` 主逻辑与样例已落地；已补 no-renderer 销毁注销链与 reference-pool 惰性初始化 | **✅ 已完成 / Unity 运行时已验证（T5）** |
| M-3 | **N30 输入触发**（`RunN30InputTrigger`：input history 9/0/9/0→触发码 100/102/104 生成 998 + history gate 广播） | LateEntityUpdate | ✅ `RunLateCharacterDatInputTrigger`（LF2Entity） | ✅ 已移植 |
| M-4 | **状态转换特效**（`SpawnStateTransitionEffects`：state13/frame200 退出 + state18/19 燃烧特效） | LateEntityUpdate | ✅ `SpawnLateTransitionEffects` | ✅ |
| M-5 | **死亡弹地帧**（`ApplyDeathBounceFrame`：frame186 + Vy=-3） | LateEntityUpdate | ✅ `RunLateDeathOpointPreCleanupPhase` 已对齐并由 `CheckLateDeathBounceFrame` 覆盖 | **✅ 已完成 / Unity 运行时已验证（提交 `995c860b`）** |
| M-6 | **F8 强制掉武器**（`RunF8WeaponDrop`） | GameTick | ❌ grep `F8/force drop` 0 命中 | 🗑️ **确认是调试功能，可不移植** |
| M-7 | **kind 4 + WeaponCount>0 → kind 0 + dvx 翻转**（`PreprocessCandidate` 154-172） | HitResolve | ✅ `BruteForceSceneQuery.cs:602-615` 完整实现 | ✅ 已对齐 |
| M-8 | **ShouldUseAlternateHurt / ApplyAlternateDamage**（injury/10 减伤 + KnockbackVx 特殊累积 + FrameDelay=-5） | HitResolve 629-约827 | ✅ 共享 `LF2AlternateDamageResolver`；`LF2Character.Hit` 与 shared-character-DAT resolver 两入口均接入；runtime/stat/运动尾契约均有自检 | **✅ 已完成 / Unity 运行时已验证（T1）** |
| M-9 | **RecordKind0Hit**（命中记录锚点 + spark，武器命中也调用） | HitResolve 1150 | ✅ `LF2Entity.RecordKind0Hit` 统一角色/武器 kind0 记录 | **✅ 已完成 / Unity 运行时已验证（T2）** |
| M-10 | **oid300 特殊命中**（bdy.x>1000→帧号） | HitResolve | ✅ `ResolveHit` ObjectId==300（`LF2CharacterHitResolver.cs:279`） | ✅ |
| M-11 | **state 400/401 传送**（最近敌/最远友） | GameTick early | ✅ `RunEarlyTeleportSpecialsPhase` | ✅ |
| M-12 | **state 500/501 变身 transform** | GameTick early | ✅ `RunEarlyState500/501Specials`（BMD-023） | ✅ |
| M-13 | **stage 波次生成**（`ApplyCurrentWavePhaseAdvance` `GameTick.cs:2317` + `ApplyCurrentWaveImmediateStageSpawns` :2350 + `RefillCurrentWavePositiveStageSpawns` :2226，StageProgression/StageSpawnRuntime 一整套） | GameTick step 23 | ✅ `BattleStageCampaignLoader` / `ApplyMatchConfig` 生产接线 + progression + spawn/refill/advance/bound + identity/dynamic-slot 契约已落地 | **✅ 逻辑与接线已完成 / Unity 运行时已验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前 backlog（T8）** |
| M-14 | **frame 110/114 → CdDefendLock=3**（`FrameTick.cs:208-209`） | FrameTick 尾 | ✅ `LF2Entity.RunCommonFrameTick` 尾部 + runtime Reset/cooldown | **✅ 已完成 / Unity 运行时已验证（T3）** |
| M-15 | **kind 16 完整结算**（`ApplyKind15Or16` kind=16：KillStat++/ComboCountAtk/SFX_065/vrest/LinkState 断开） | HitResolve 1640-1704 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已补齐 FallDamageDiv 缩放、KillStat/ComboCount、frame200、vrest、2/-2 持有断开与 SFX_065 | **✅ 已完成 / Unity 运行时已验证（T6）** |
| M-16 | **kind 15 完整位移**（`ApplyKind15Movement`：KnockbackVx+真实 Vx/Vz+YInt=-2，按对象类型分 vyStep 3.0/2.3） | HitResolve 1737 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已改为 authority 的 KnockbackVx/Vz + YInt/Vy 语义；武器/铁球侧原 `WhirlwindForce` 保持 3.0/2.3 分支 | **✅ 已完成 / Unity 运行时已验证（T6）** |

> **判定原则提醒**：当前仍标 ❌/⚠️ 的项目都**不能直接删对应 Unity 脚本**；它们是"C# 有 Unity 缺/结果仍需验证"。M-1/M-2/M-7/M-8/M-9/M-10/M-11/M-12/M-13/M-14/M-15/M-16 已确认对齐或完成并运行验证。只有 M-6（F8 调试）确认是调试功能后可不移植。

---

## 8. 判定为"架构不同但等价"的项（🔷 — 不得当冗余删除）

以下 Unity 代码看似"多出来"，实为 Unity 框架下实现 C# 同一逻辑的必要产物，**严禁因为 C# 没有同名文件就删除**：

| Unity 脚本/机制 | 对应 C# 逻辑 | 说明 |
|-----------------|-------------|------|
| `LF2Character*Resolver.cs`（Hit/Catch/DamageState/Action/Interaction/State/WeaponLink） | `NtsdCharacter` + `HitResolve`/`CPointRuntime`/`InputRuntime` 各段 | 组合模式拆分，逻辑等价 |
| `LF2AlternateDamageResolver` + `LF2CharacterDatHitResolver` | `HitResolve.ShouldUseAlternateHurt` / `ApplyAlternateDamage` | alternate 真值集中一次实现，由真实 `LF2Character.Hit` 与 shared-character-DAT 两入口复用 |
| `LF2Weapon*Resolver.cs`（Interaction/HeldState/ReleaseFlow/FrameLogic） | `WeaponRuntime` 各段 | 同上 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 中"当前 DAT 是角色"的分发 | 服务 transform 后 wrong-shell 角色，C# 因为是纯数据 Entity 不需要 shell 概念 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | Unity 运行时化，字段一一对应 |
| `FrameTransistor` hook（OnFrameTickBeforeWaitAdvance 等） | `FrameTick.Tick` 内联步骤 | 拆成 hook 供子类覆写 |
| `SimulationWorld` 动态 runtime slot | `Objects[400]` 固定槽 | Unity 用对象池，遍历顺序需保持 slot 升序 |
| `RefreshRuntimeSnapshot` 调用 | `CharacterSync.SyncRuntimeFromLegacy` | Unity 每 pass 后刷快照 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023：区别于 `ImmediateFrame`（会清 attacking） |

---

## 9. 不需要对齐的部分（明确排除）

| 项 | C# 位置 | 原因 |
|----|---------|------|
| 可活动范围 / Z 边界钳制 | `ApplyPreframeBounds` Z 段、`ClampCharactersToStageZ`、`Bg.ZBoundary*` | 用户明确：bg.dat 可活动范围不对齐，Unity 用 BoundaryWall |
| 相机 | `UpdateCameraAndBgAnimation`、`CameraX`/`CameraVel` | 用户明确：相机不对齐，Unity 用 ProCamera2D |
| bg 层动画 | `layer.AnimCounter` | 背景表现 |
| 结算界面 | `RunResultsTick`、`UpdateBattleResultsFlow` | 非战斗运行时（菜单/结算） |
| SDL/Host/音频桥 | `src/Host/*` | C# EXE 适配层 |
| 数据加载 | `src/Data/*` | Unity 用自己的 DatParser |

---

## 10. 对齐优先级清单（已全部逐行核实，✅=已核实定性）

### P0 — 已修复并完成 Unity 运行时验证
- [x] **§2.1-1 / T0** `exemptVal` 公式 — **已修复并通过 Unity 运行时自检**：`LF2Entity.ResolveArestCooldown` 与 `LF2CharacterHitResolver` 已按 arest/vrest 权威公式处理
- [x] **§2.1-3 / M-8 / T1** ApplyAlternateDamage — **已完成并通过 Unity 运行时自检**：共享 `LF2AlternateDamageResolver` 覆盖约 line 827 的完整权威契约；真实 `LF2Character.Hit` 与 shared-character-DAT resolver 两入口、`Unk344`/统计数组/`HPBound`、heavy/rest/preprocess/state tail 均有针对性检查

### P1 — 已补齐并完成 fresh Unity 运行时验证
- [x] **M-1 / T4** oid 7/8→51 合体拆分 — 已按唯一 C# 权威 `GameTick.cs:1093-1263` 重审；生产顺序为 human poll → M-1 → `NeedClearInput`/tick gate → unified character input，矩阵覆盖 frame85 gate 外延迟 split、oid8 镜像、identity/presentation、human+AI、split reset 与外部 `ItrRest`，并进入 `21:57:40` fresh full PASS
- [x] **M-2 / T5** 复活 pass（`RunRespawnPass` 完整逻辑）— **已完成并通过 fresh Unity 运行时自检**
- [x] **M-13 / T8** stage 波次生成（`ApplyCurrentWaveXxx` 整套）— **逻辑与生产接线已完成并通过 fresh Unity 运行时自检；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进**
- [x] **P1 / BOUNDS-X** PreFrame 实体 X clamp/free — **已完成并通过 physical worktree fresh Unity 运行时自检**：base `bg.width` 与 phase override 分离、current-DAT 分派、`RelationTeam`/`HitStop`/`Unk344`/`YInt`/严格边界与 `XInt` 契约均有矩阵覆盖

### P1 — 已确认缺失战斗逻辑（需新增）
- [x] **§6.2 AI / T9** `PrepareAiInputBasic` 完整调用闭包 — **已完成并通过 fresh Unity 运行时自检**

### P1 — 已确认对齐（无需动作）
- [x] **M-7** kind4+WeaponCount>0→0 dvx 翻转 — ✅ `BruteForceSceneQuery.cs:602-615`
- [x] **M-9 / T2** 武器命中 spark（`RecordKind0Hit`）— **已完成并通过 Unity 运行时自检**（角色与武器 kind0 路径统一记录）
- [x] **§5** HP/PP 自然恢复 + HpMax/HPBound — ✅ 逐字段对齐
- [x] **kind 10/11 笛子** ✅、**kind 14 方向阻挡** ✅、**oid300** ✅、**kind 5 委托** ✅

### P2 — 帧推进尾部特判（已核实）
- [x] **§3-1/§3-2** state14 复活 HitStop（oid/5==3 + difficulty 分支）— ✅ 完整对齐（`LF2Character.cs:2134-2163 / 2205-2211`）
- [x] **§3-3** frame mp turn-around — ✅ 完整对齐（`LF2Entity.cs:3284-3321`）
- [x] **§3-4** frame 202 HitStun=20 — ✅（`LF2Entity.cs:3634`）
- [x] **M-14 / T3** frame 110/114 CdDefendLock=3 — **已完成并通过 Unity 运行时自检**

### P2 — 已补齐并完成 Unity 运行时验证
- [x] **M-15 / M-16 / T6** kind 15/16 完整位移与副作用 — **已完成并通过 Unity 运行时自检**

### P3 — 确认可不移植
- [x] **M-6** F8 强制掉武器 — ✅ 确认是调试功能，Unity 不需实现（非冗余，是未移植的调试项）

### 二次审计战斗差异收口（2026-07-15）

> 本表只列会改变战斗模拟结果的项目。UI/HUD、camera/background/render、audio playback、network、replay，以及 F7-F9/debug 路径均不进入 backlog。`stage.dat` 默认资产部署由用户明确暂缓，也不进入本轮推进。
>
> 计数单位是“差异簇”而不是原子代码点；例如 INPUT-8 同时包含 shared-DAT running 的提前返回和缺 defend 分支。下表是 **Audit2 历史记录**；其中旧来源坐标不得用于当前实现，任何后续复查都必须回到 `ntsd_release_C#`。当前执行状态以 Audit4 为准。

#### 已确认差异簇（14/14 已修复并通过新增自检）

| 编号 | 差异 | Unity 证据 | Authority 证据 |
|---|---|---|---|
| INPUT-1 | state 7 `Defending` 被加入正式输入 state switch；authority switch 只分发 0/1/2/4/5 | `LF2CharacterActionResolver.cs:54-81` | C# `InputRuntime.ApplyCharacterInput:718-735` |
| INPUT-2 | jump 输入门槛读取 `PS.y`/浮点 Y，authority 使用 `YInt`；real character 与 shared-DAT 路径均需统一 | `LF2CharacterActionResolver.cs:61-68`；`LF2Entity.cs:1529` | C# `InputRuntime.ApplyCharacterInput:728-730` |
| INPUT-3 | state 301/19 的纵向移动门槛读取 `PS.y`，authority 使用整数 Y 门槛 | `LF2CharacterActionResolver.cs:503-516` | C# `InputRuntime.ApplyCharacterInput:680-685` |
| INPUT-4 | 正式 battle input pass 调用 `RunPostCooldownInputPhase` 后没有执行当前帧 `dvx/dvy/dvz` tail；唯一 tail 留在当前无生产调用者的 `RunCharacterInputPhase` | `SimulationWorld.Passes.partial.cs:54-63`；`LF2Character.cs:750-779` | C# `InputRuntime.ApplyCharacterInput:737`；`InputRuntime.ApplyFrameVelocityTail:1463-1510` |
| INPUT-5 | `CdDefendLock` 同时由 Runtime 与 `NTSDInputStateModule` 持有/衰减/回写，存在双状态源不同步 | `SimulationWorld.Passes.partial.cs:920-928`；`NTSDInputStateModule.cs:75-111,165-174,408-436`；`LF2Entity.cs:1188-1196` | authority 仅有实体 input runtime 单一字段 |
| INPUT-6 | Super Punch 分支提前清零 `HitConfirmEa`；authority 在这里只读取命中确认并切帧 | `LF2CharacterActionResolver.cs:92-104`；shared-DAT `LF2Entity.cs:1269-1281` | C# `InputRuntime.ApplyStandingActions:942-953` |
| INPUT-7 | `ImmediateFrame` 统一清零 `AttackingCounter`，把 authority 的 raw frame write 和计数副作用合并，影响多个输入动作跳帧 | `LF2LivingObject.cs:480-497` | C# `InputRuntime.ApplyJumping:1210-1247`；`ApplyDash:1250-1315`；`ApplyFrame215Landing:1402-1441`；这些分支直接写 `Frame`，只在对应分支明确清 `Attacking` |
| INPUT-8 | transformed/shared-DAT running 路径存在提前返回，并缺少 authority 的 running defend 分支（一个关联差异簇） | `LF2Entity.cs:1578-1636` | C# `InputRuntime.ApplyRunning:1131-1205` |
| INPUT-9 | transformed/shared-DAT frame 215 额外接受 attack 分支，authority 只处理其正式输入条件 | `LF2Entity.cs:1774-1810` | C# `InputRuntime.ApplyFrame215Landing:1405-1438` |
| INTERACT-1 | `LF2SpecialAttack` 没有声明使用 dynamic runtime slot，opoint 技能实体不能稳定遵循 `50..399` 槽区契约 | `LF2SpecialAttack.cs:68`；`LF2Entity.cs:1014` | C# `FrameTick.SpawnFromOpoint:333-350`；`NtsdConstants.MaxObjects:9`；`SimulationWorld.Objects:28` |
| INTERACT-2 | dynamic slot `50..399` 满后 Unity 回退分配 `0..49`；authority 应直接生成失败 | `SimulationWorld.Registry.partial.cs:359-369` | C# `FrameTick.SpawnFromOpoint:333-350`；只扫描 `50..399`，无槽时直接返回 `null` |
| INTERACT-3 | vrest key 混用 `StableId` 与 runtime slot，可能导致互斥命中对象身份与固定槽 authority 不一致 | `LF2WeaponBase.cs:672,718`；`LF2ObjectPointFactory.cs:260-261`；`LF2SpecialAttack.cs:1001`；对照 `LF2SpecialAttack.cs:995-996` | production collision/vrest 路径以 `Runtime.SlotIndex` 为对象身份 |
| INTERACT-4 | state 3003 opoint 的双向 vrest 参与对象/身份写入与 authority 不一致 | `LF2ObjectPointFactory.cs:213-216,533-537` | C# `FrameTick.ProcessOpointSpawn:280-287` |
| INTERACT-5 | 非角色 parent 的 kind 2 链接把 `StableId` 写入 `TargetSlotIndex`/`HeldWeaponStableId`/`HolderStableId` 等 slot 字段 | `LF2ObjectPointFactory.cs:540-555`；消费端 `SimulationWorld.QueryAndLinks.partial.cs:119-133` | C# `FrameTick.SpawnFromOpoint:422-430`；kind 2 的 `TargetIdx/HeldWeaponSlot/HolderIdx` 均写 runtime slot |

当前收口状态：

- **INPUT-1~9：全部已修复并运行时验证。** `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖 state switch、`YInt` 门、frame velocity tail、单一 defend-lock 真值、Super Punch、raw frame write、running 顺序/defend/反向停跑和 frame215。
- **INTERACT-1~5：全部已修复并运行时验证。** `CheckInteractionRuntimeSlotContracts` 覆盖 dynamic slot `50..399`、满槽直接拒绝、runtime-slot vrest、state3003 双向 vrest 和 non-character kind2 链接；满槽拒绝同时断言不遗留空 registry bucket、renderer pool 或 reference/logic pool 生命周期残留。
- **NARUTO-DDJ / OPOINT-LIFECYCLE：已修复并运行时验证。** 真实 frame271→272/273→oid205/204→六分支→`6 x oid33/action307` 回归覆盖 reference pool 类型安全、pending lifecycle finalize、factory 注册时机、池化角色 `StableId` 重分配和 opoint 关系字段继承；详细链路见 §4.3。

#### 历史 Audit2 风险收口（当前均已关闭）

| 编号 | 状态 | 审计结论 / 验证 |
|---|---|---|
| RISK-1 | ✅ 已修复 / Unity 运行时已验证 | late frame rollover 不再通过 `FrameEvent` 二次推进 walking/running locomotion；新增矩阵验证同 tick `AnimCounter` 只推进一次并保留 state-entry 副作用 |
| RISK-2 | ✅ 已修复 / Unity 运行时已验证 | input/move raw frame write 均保持 `PrevFrame/PN`、wait counter 和非显式清零的 attacking；新增 raw move write 矩阵通过 |
| RISK-3 | ✅ 已修复 / Unity 运行时已验证 | held/`TrackerParent` 行为引用改由 runtime slot 和反向关系校验；注销、同槽复用、异槽复用均清理失效缓存，`CheckHeldReferenceSlotReuseContracts` 通过 |
| RISK-4 | ✅ 已由 Audit5 `R-HC-05` 关闭 | fixed slot、注销与同槽复用矩阵已补齐；本行仅保留旧风险来源，不再是开放项 |
| RISK-5 | ✅ 已修复 / Unity 运行时已验证 | step7/step9 capability 与入口按 current DAT `obj_type` 中央分派；character shell→non-character 和 special/non-character shell→character 双向矩阵验证不会漏跑或重复跑 interaction pass |

#### 历史 backlog / 验收矩阵（CP-NV1/2/3 与 STEP10 已重新关闭）

下表保留既有工作的**历史来源坐标与 self-check 证据**；旧工程/EXE 坐标不具当前权威性，不得用于当前实现或验收，也不能覆盖或冲销 Audit4。

| 优先级 / 编号 | 状态 | Authority | Unity 现状 | 明确缺口 | 验收标准 |
|---|---|---|---|---|---|
| P0 / CP-NV1 action selection | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.RunKind1Pass:109-130`；`ApplyCpointAction:189-203`；`FrameRuntime.SetFrameImmediate:12-16` | real/shared 两路 immediate helper 已清 `Runtime.FrameWaitCounter`，并保留 `Trans.WaitCounter` 与 `Prev2` | 已关闭 | `CheckCpointNegativeActionMatrix` 现覆盖 real/shared 双壳、aaction/taction/jaction 三类负 action、双方 FWC 清零、方向/attacking/wait/Prev2；combined fresh PASS |
| P0 / CP-NV2 throw snapshot/raw | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.ApplyThrow:306-343`；`SwapAttackerCharData:345-362` | throw 使用进入 `ApplyThrow` 的 source `atkFrame`；victim `Vz` 先清 0，再按方向覆盖；raw frame 顺序保持 | 已关闭 | `CheckCpointThrowRawAndTransformMatrix` 覆盖 none/up/down/both=`0/-3/+3/0`、raw carrier 与 transform source snapshot 的 frame112、victim `(76,-36)`；combined fresh PASS |
| P0 / CP-NV3 held sync | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.SyncHeldCpoint:22-48`；`SyncCaughtByCpoint:206-304`；`FrameRuntime.SetFrameImmediate:12-16` | `vaction=0` 保留进入 frame/facing/FWC；非零 immediate 切帧并清 FWC；负值 flip/abs；center/cpoint 均取最终 resolved current frame | 已关闭 | `CheckCpointHeldSyncVactionMatrix` 的 real/shared `-131/0/131` 完整矩阵 fresh PASS |
| P1 / FLOW-1 FrameToggle | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:32-36`；`GameTick.RunState400401Pass:962-1030` | Flow 新增 `FrameMod12`/`FrameToggle` 并由 `AdvanceBattleFlowTick` 与 CurrentTick/InputPhase 同步推进；early teleport 读取 toggle，source 无 Character gate、401 可选 self、target 保留 Character 过滤 | 已关闭 | `CheckBattleFlowToggleAndTeleportMatrix` 覆盖 tick 1-4/11-13、reset、401 self、non-character source、target 选择/no-target，Unity self-check PASS |
| P1 / LINK-1 positive link validation | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:113`；`GameTick.ValidatePositiveLinks:2009-2034` | `ValidateHeldLinksAll` 按 runtime slot `0..399` 覆盖所有 active `LF2Entity`；valid 仅 target range/active/反向 holder；invalid 只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`，不清 inactive/mismatch target 的反向字段 | 已关闭 | `CheckValidatePositiveLinksMatrix` 覆盖 valid character/non-character、slot0/399、target -1/400、inactive/mismatch、link<=0、target link 状态和多 holder slot 顺序，Unity self-check PASS |
| P1 / BOUNDS-X | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:128`；`GameTick.ApplyPreframeBounds:1301-1398` | `LF2Entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride)` 按 current DAT type/OID 中央分派；实体 pass 显式使用 `BaseStageWidthPx`，不改变 stage spawn/AI/camera 的既有 `StageWidthPx` 消费 | 已关闭；oid122/123 条件使用独立 `Unk344>0`，不再误用 `WeaponFlightCounter` | `CheckPreFrameXBoundsMatrix` 覆盖 slot/team/hit-stop/override、strict edges、type3/free、oid122/123、`YInt`、current-DAT/CLR 交叉、base/active width 分离、`XInt` 与 world lifecycle；physical worktree fresh Unity 日志双 PASS |
| P1 / TRANSFORM-SHELL | ⚠️ 历史 focused PASS，当前 fresh full self-check FAIL | C# `GameTick.RunStateSpecialPreCollision:1615-1662`；`GameTick.InitRuntimeIdentity:1664-1671`；`EntityCategoryResolver.Get` | frame/physics/landing 及 step7/step9 interaction capability/entry 已按 current DAT 中央分派；transform 后以目标 `weapon_hp` 刷新 `WeaponFlightCounter`，不改 `WeaponCount`；state8000 再设 hit-stop 140 | 既有修复保留；最新 transformed landing fixture 仍有代码契约回归 | `CheckStateTransformLandingMatrix` + `CheckStateTransformInteractionPhaseRouting`；2026-07-15 focused PASS 为历史证据，2026-07-18 最新 full run 失败，实际 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4` |
| P0 / STEP10 call-chain/tail/stats | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.RunKind1Pass:50-149`；`SyncCaughtByCpoint:206-304`；`ApplyThrow:306-343` | state9 首次 sync、mismatch/escape immediate + early return、escape 同 tick `Vx/Vy`、FWC 清零及实体 stats-only 契约均已落地 | 已关闭 | `CheckCpointEscapeAndMismatchEarlyReturn`、`CheckCpointDecreaseEscape`、`CheckSharedDatCpointStep10StatsAndInputOrder` 已按权威重写；覆盖不跑 throw tail、即时速度和 world stats 不变，combined fresh PASS |
| P1 / OPOINT-VIS | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:81-148`；`RunNaturalRandomWeaponDrop:636-697`；`RunLatePerEntityUpdatePass:1533-1537`；`RunLateEntityUpdate:1539-1612`；`FrameTick.ProcessOpointSpawn:233-331` | 已恢复 pre-advance frame_logic、natural drop、逐实体 late producer 三个发布边界；late pass 保持动态 slot 扫描 | 验收过程修复 pending destroy 实体被 active-only 采集过滤，确保 fragment/transition 发布后只回收一次 | `CheckQueuedObjectPointPassBoundaries`、`CheckSimulationWorldLateMutation` 覆盖 real factory queue、三边界、父回收、高/low slot 可见性；2026-07-15 fresh PASS |
| P2 / FRAME-ADV | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:92-104`；`EntityCategoryResolver.Get`；`EntityDispatch.DispatchFrameAdvance:36-57`；`FrameAdvance.Advance:13-47`；`Physics.Update:16-31` | `SerialTickAll` 按 runtime slot 交错执行 Transit/TU；per-class 路由已收到 current DAT；SpecialAttack 不再提前运行 wait/next | 验收过程修复 character/weapon 壳的 `PS.BindRuntime`，防止物理仍读写脱离 runtime 的状态 | `CheckSerialTickInterleaveAndFrameEdgeMatrix` 覆盖逐 slot 顺序、SpecialAttack 单次 physics、weapon shell 的 type3/other DAT、negative next；落地矩阵同时通过 |
| P2 / FRAME-TICK | ⚠️ 历史 focused PASS，当前 fresh full self-check FAIL | C# `FrameTick.Tick:13-216` | current-DAT 公共主干已集中到 `RunCommonFrameTick`；type3 `hit_a`、state14、iron-ball frame20、wait/next/999/negative、frame mp/`PpDisplay` 与 tail 统一执行 | C# `FrameTick.Tick` 无 oid9 专属 drain 分支；SpecialAttack 的旧重复 drain/counter 已移除；transformed landing fixture 仍需修复 | `CheckSpecialAttackStep4AndLateFrameTick`、`CheckFrameTickPpDisplayAndCurrentDatMatrix`、`CheckStateTransformLandingMatrix`；2026-07-15 focused PASS 为历史证据，2026-07-18 最新 full run 失败 |
| P3 / COLLISION-SNAPSHOT | 🔷 权威审计未发现生产差异 / 保留回归风险 | C# `GameTick.SnapshotPrevFrame2:1521-1531`；`CollisionCollect.CollectCandidates:14-40`；`CollectPair:50-136`；`RecordCandidate:162-302`；`NtsdConstants.HitCandidateMax:11` | `CaptureCollisionFrameSnapshotsAll` + `BruteForceSceneQuery` 的普通生产路径与权威一致，当前没有实锤修复项 | Unity carrier 缓存对象引用；权威 C# `NtsdEntityRuntime.HitCandidateSlots:219-220` 缓存 runtime slot。若未来在 snapshot 消费期间引入同 slot 即时复用 producer，语义可能分叉 | 保留多候选/20+、同距、Prev2、cache 隔离回归；未来新增 pass 内 slot reuse 时必须补专项测试 |

#### CP-NV / STEP10 C# 重审矩阵（原始总账既有项，重开后重新关闭）

本段是对原历史 backlog 的重审，不修改 Audit5 原始 74 项分母。旧历史 PASS 不作为新证据；生产和检查已按下列权威矩阵重写，并统一进入 `21:57:40` combined fresh full PASS。

1. **CP-NV1 immediate frame 字段边界**：real character 与 shared-DAT 两路 attacker/victim immediate frame 均清 `Runtime.FrameWaitCounter`，同时保留 `Trans.WaitCounter`、`Prev2` 和 C# 未写的其他 carrier。最终负向覆盖包含 aaction/taction/jaction、负 action、方向翻转、双方 attacking 清理及双壳字段边界。
2. **CP-NV2 source snapshot 与 raw 顺序**：throw 保留调用前 `atkFrame`，先据 source snapshot 算位置，再 raw 写 attacker `Frame/Prev2`、清 attacker `Attacking`，之后写 victim `Vx/Vy`、先将 `Vz=0`、按方向覆盖，最后 raw 写 victim `Frame/Prev2`。定向矩阵已覆盖 none/up/down/both 的 victim `Vz=0/-3/+3/0`，以及 transform fixture 的 attacker frame112 与 victim `(76,-36)`。
3. **CP-NV3 held `vaction` 矩阵**：`SyncCaughtByCpointStep10` 和 held position sync 已统一读取最终 resolved current frame；权威矩阵如下：

| `vaction` | 权威 frame/facing | `Runtime.FrameWaitCounter` | held 位置数据源 |
|---:|---|---|---|
| `-131` | immediate 写 `-131` 后 flip/abs 为 frame131、朝向翻转 | 清 0 | frame131 的 center 与 cpoint |
| `0` | 不切帧、不翻面，保留进入时 current frame | 保留进入值 | 保留下来的 current frame 的 center 与 cpoint |
| `131` | immediate 切 frame131，不翻面 | 清 0 | frame131 的 center 与 cpoint |

4. **STEP10 P0 调用链**：state9 已先执行第一次 `SyncCaughtByCpoint` 再做 decrease/action；invalid victim/mismatch 与 caught-duration escape 均在 immediate frame 后 early return，不再继续 throw/dir。escape 同 tick 写 victim `Vx/Vy`；held injury 保留 holder/victim entity stats，不写 world `KillStats/DamageStats`。

最终证据：上述共享 helper、throw snapshot 和 Step10 顺序均已收口；相关检查不是删除失败断言求绿，而是按 C# 权威改写并扩展 real/shared-DAT、负 action、early-return、速度与 stats 负向覆盖。combined Architect 最终结论为 `P0/P1/P2=0`。

---

## 附：核对方法

1. 本文所有 ⚠️/❓ 项都需**打开对应 C# 源码段 + Unity 源码段逐行比对**后才能定性。
2. 定性为"Unity 用别的方式实现了" → 标 🔷 并记录对应关系，**不删**。
3. 定性为"C# 有 Unity 真没有，且是正式战斗逻辑" → 标 ❌ 进 P1 待补。
4. 定性为"C# 是调试/表现/菜单，非战斗运行时" → 标 🚫 排除。
5. 每完成一项核对，更新对应行状态并在 §10 勾选。

---

## 附二：核实总账（更新至 2026-07-16）

**✅ 二次审计确认差异已收口（14/14）：**

输入/动作 9 项（INPUT-1~9）与交互/opoint/vrest 5 项（INTERACT-1~5）均已修复并通过新增自检。RISK-1/2/3/5 经审计实锤后也已修复并运行时验证；只剩 RISK-4 一项未找到正式主循环可达触发边界的待审计风险，不计入确认差异。

**✅ Naruto DDJ 新确认差异已收口（1 个关联差异簇 / 5 个根因）：**

真实 Naruto 防下跳链暴露的 reference pool 污染、factory 注册时机、pending lifecycle 同槽复用、池化角色 StableId 和 opoint 关系字段继承问题均已修复；完整链回归确认 6 个 clone 到达 action307 且 renderer 可见。

**✅ 已修复真 bug（共 1 项）：**

| 项 | 内容 |
|----|------|
| §2.1-1 / T0 | `exemptVal` 已改用权威 arest/vrest 公式，并通过 Unity 运行时自检 |

**✅ 原缺失项已完成并通过 Unity 运行时自检（主要项）：**

| 项 | 内容 |
|----|------|
| M-1 / T4 | oid 7/8→51 合体拆分；C# `GameTick.RunOid5152RuntimeMaintenance:1093-1121`、`TryMergeOid7Or8Into51:1123-1212`、`SplitOid51BackToPair:1214-1263` 的 gate/oid8 镜像/身份表现/DJA human+AI full-tick/split reset 与 `ItrRest` 契约均已覆盖 |
| M-2 / T5 | 复活 pass（含 free-entity gate、队友平均落点、stored-count 分支与 oid998 特效） |
| M-8 / T1 | 共享 ApplyAlternateDamage 完整契约、真实角色/shared-DAT 两入口及 object-pass 预处理 |
| M-9 / T2 | 角色/武器统一 `RecordKind0Hit` |
| M-14 / T3 | frame 110/114 写 `CdDefendLock=3` 及 cooldown 生命周期 |
| M-15 / M-16 / T6 | kind15 authority 位移 + kind16 完整结算、副作用与持有断开 |
| combo / T7 | RunComboWrappers 9 组连招 + oid6 DjaGuard |
| Naruto DDJ / OPOINT-LIFECYCLE | frame271 起始、oid205/204 递归链、6 x oid33/action307、对象池/slot/StableId/关系字段完整契约 |
| M-13 / T8 | stage immediate spawn、positive refill、清场推进与 phase bound |

**历史快照（Audit4 前）：** 当时只保留 RISK-4 与完整对局逐帧对拍缺口；该结论已被 Audit4-01..16 取代，不代表当前无待实现差异。

**✅ 已确认对齐或已完成并验证（主要项）：**
tick 主循环主干、kind 0/4/9 主流程（含 raw kind9→kind0 预处理与 alternate）、kind 6/8/10/11/14 命中、oid300、kind5 委托、kind4+WeaponCount 翻转（M-7）、HP/PP 自然恢复（§5）、heal/catch timer、帧推进主干 + state14 复活 HitStop（§3-1~§3-5）、frame mp turn-around、opoint 生成、cpoint 抓取、state 400/401/500/501、N30 触发、状态转换特效。

**🔷 架构不同但等价（严禁删，见 §8）：** resolver / shared-DAT 桥 / 字段化 runtime / hook 拆分 / 动态槽 / DirectWriteFramePreserveWaitCounter 等。

**🚫 不需对齐（见 §9）：** UI/HUD、camera/background/render、audio playback、network/replay、Host 和 F7-F9/debug 控制路径。**🗑️ 确认可不移植：** M-6 F8 调试掉武器。

**⏸️ 用户明确暂缓：** T8 默认 `stage.dat` 资产部署。T8 逻辑/接线和 self-check 状态不变，但该资产工作不进入当前推进。

---

### Audit4 前历史总结（已失效）

**本段只记录 Audit4 前的历史验收快照，不是当前执行口径。** BATTLE-AUDIT4-01..14 的生产修复和已有断言现已通过 fresh full self-check，但 3 项定向 Play Mode 尚未完成；T8 默认 `stage.dat` 资产部署仍由用户明确暂缓并排除在当前 backlog 之外。

## 第三次实战/静态审计（2026-07-16，最高优先级）

旧版“当前无确认差异”结论已失效。以下 BATTLE-AUDIT3-01..17 均为已静态确认的战斗逻辑差异，17 项生产修复现已全部落地。最新 fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` 源码时间 `2026-07-16 18:24:04`，Unity `Assembly-CSharp.dll` 时间 `18:31:52`，`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `18:33:00` fresh 返回 **PASS**，满足 source < DLL < result。该结果包含本轮 M-1/T4 完整矩阵。此前生产 diff 的 Architect 复核结论保留；新增自检覆盖由本次 fresh build/PASS 证明。上述证据只关闭编译、静态复核和针对性 self-check 门槛；本轮变更后的真实 `NTSD_Battle` Naruto 防前跳螺旋丸、奔跑防跳命中及防下跳六分身仍待 Play Mode 验收，因此不得把 17 项标成 Play Mode 全完成，也不得宣称战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。

| 编号 | 双方证据 | 影响 | 状态 |
|---|---|---|---|
| BATTLE-AUDIT3-01 | Unity `BattleTestBootstrap.cs:203` 只写 Team；C#/正式入口 `AppManager.cs:206-207` 写 Team+RelationTeam；`LF2WeaponInteractionResolver.cs:20-23` 对 RelationTeam=0 退出 | oid434 action396 kind3 消费被阻断，Naruto frame256 链不成立 | 生产修复和针对性 self-check 已通过；`RelationTeam` 已补，仍待真实 bootstrap 与 Naruto 螺旋丸 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-02 | C# `WeaponRuntime.cs:140-149` cover=0 为 z+1/y-1；Unity `LF2CharacterWeaponLinkResolver.cs:265-277` 相反；renderer `LF2ObjectRenderer.cs:219-220` 另加 zz | held 武器 Y/Z 与排序偏移，renderer 仅部分抵消 | 生产修复和针对性 self-check 已通过；held 层级、位置与跟手仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-03 | Unity `BruteForceSceneQuery.cs:1630-1643` coarse union 排除 kind5，消费侧 `:529-660` 才替换；C# `CollisionCollect.cs:431-451` union 纳入全部 itr | kind5-only 命中在粗筛阶段消失 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-04 | Unity `BruteForceSceneQuery.cs:1614-1625,1658-1668` 过滤大坐标；C# `CollisionCollect.cs:431-478` 保留原始几何；DAT 有 Naruto y=80000 kind3 | 高层碰撞候选无法进入 Unity | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-05 | Unity `LF2WeaponHeldStateResolver.cs:75-78`、`LF2Weapon.cs:675-730` 有 ordinary weapon_strength held 旁路；C# `WeaponRuntime.cs:71-213` 无此旁路 | 普通武器 held 动作/伤害路径偏离 | 生产修复和针对性 self-check 已通过；螺旋丸按攻击键的真实 weapon 路径仍待 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-06 | Unity `NTSDBattleTickSystem.cs:37,50,56` 每 tick 三次 HeldObjectProcessAll；C# `GameTick.cs:99-103` 一次 Step12、一次 SyncHeld | 重复同步/释放/消耗 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-07 | Unity `NTSDBattleTickSystem.cs:38-50` candidate/hit 后才 PreInteraction；C# `GameTick.cs:99-106` 先 cpoint/link 再 collect | 本 tick cpoint/held 状态不能影响候选 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-08 | C# `GameTick.cs:95-106` candidate 前 clamp Z；Unity `NTSDBattleTickSystem.cs:37-39,55-56` clamp 在交互后 | 候选读取未 clamp 的角色 Z | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-09 | Unity 原实现对 invalid positive link 只清 `LinkState`；C# `ValidatePositiveLinks` 对无效链接只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot` | holder 残留 target/held slot 污染后续 held；inactive/mismatch target 的反向字段不在此处清理 | 已按 C# 契约只清 holder 三字段，针对性 self-check 已通过；不清 target 反向字段，仍待真实 Play Mode |
| BATTLE-AUDIT3-10 | Unity `SimulationWorld.Passes.partial.cs:635-648` 依赖 Supports；基类 `LF2Entity.cs:1034-1036` 默认 false，Special/Other 无 override；C# `GameTick.cs:83-90,165-170` 统一分派非角色 DAT hit_Fa | Special/Other hit_Fa 时机/执行路径错误 | 生产重构和 fresh self-check 已通过：`hit_Fa1..14` 唯一下沉 `LF2Entity`，Special/Other/current-DAT shell 共用；新增覆盖 3/4/10/14，3/14 对 Other、current-DAT Character、Special 三壳连续两 tick 验证副作用仅一次，4 覆盖 catch frame/速度/`CatchTimer`，10 覆盖原路径与落地摩擦防重复；仍待真实 Play Mode 场景验收 |
| BATTLE-AUDIT3-11 | Unity `LF2ObjectPointFactory.cs:221-229` logicalY+PS.z；C# `FrameTick.cs:381-394` spawnY 不加 Z；Character/Weapon/Other 初始化直接用 task.pos.y，renderer `LF2ObjectRenderer.cs:278-280` 再加 displayZ | non-special opoint 出生高度可能双加 Z；SpecialAttack `LF2SpecialAttack.cs:1383-1387` 会减回 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-12 | Unity `SimulationWorld.QueryAndLinks.partial.cs:77-83` 强制 LF2Character holder；C# `WeaponRuntime.cs:86-94` 接受任意带 CharData Entity | shared-DAT/非 Character holder 断链 | generic holder、damaged 后继续 dvx/kind3 与 IronBall `FrameDelay=1` 已落地；新增 `CheckWorldLevelRealWeaponStep12Contracts` 经 `SimulationWorld.HeldObjectProcessAll`、generic `LF2Entity` holder、真实 `LF2Weapon` 覆盖 damaged→dvx、damaged→kind3、IronBall `FrameDelay=1` 并 fresh PASS；仍待真实 Play Mode 场景验收 |
| BATTLE-AUDIT3-13 | Unity `BruteForceSceneQuery.cs:1603-1627,1646-1677` 过滤 body kind、x/y、w/h/zwidth；C# `CollisionCollect.cs:431-478` 不过滤；full-height 识别两边均有 | 正式大范围技能/特殊几何被 Unity 粗筛排除 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-14 | Unity `BruteForceSceneQuery.cs:446-526,1277-1304` nearest/bodyX gate 依赖 modeArg==1；C# `CollisionCollect.cs:181-240` 无 mode gate | 默认模式目标选择/候选数不同 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-15 | C# `CollisionCollect.cs:144-158` 有 oid205→oid9 frame301、hit_a/d/j=999、同非零 Unk364 pair gate；Unity 仅有 oid→209 kind9 gate `BruteForceSceneQuery.cs:1064-1075` | Naruto 相关同关系对象错误进入候选 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待 Naruto 真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-16 | C# same-team 例外 `CollisionCollect.cs:304-355` 读 attacker Prev2/collision；Unity `BruteForceSceneQuery.cs:988-1007,1034-1037` 读 current | 帧边界放行/拒绝相反 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-17 | C# kind8/state3005 lead-in `CollisionCollect.cs:99-101` 读 current；Unity `BruteForceSceneQuery.cs:990-1002` 传 Prev2 collision | kind8 延迟命中时机偏移 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |

**本轮验收状态：**fresh `/m:1` build 已为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full self-check 返回 **PASS**。除 Audit3-10 的 3/4/10/14 扩展矩阵和 Audit3-12 的 world-level generic holder/真实 weapon Step12 矩阵外，本结果也覆盖 M-1/T4 的完整运行时矩阵。下一步仍须在真实 `NTSD_Battle` 回归 Naruto 防前跳螺旋丸的层级/位置/跟手/攻击路径、奔跑防跳命中，以及防下跳六分身。因此 17 项只能称为“生产修复已落地、针对性 self-check 已通过、Play Mode 未全部验收”。T8 默认 `stage.dat` 部署继续暂缓。

## 实施进度（2026-07-16）

> §10 的 `[x]` 仅表示“已核实定性”，不表示已经实现；实际完成状态以本表为准。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState`。type3 lead sound 已按代码权威对齐，headless 未直接观测音频 | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **历史实现/self-check 已通过；待 C# 重审** | 唯一权威为 C# `GameTick.cs:1093-1263`；旧实现的 pass 顺序、merge/split 与身份链需据此重新核验 | 既有 7 项检查仅保留为回归基线，不能代替 C# 权威重审 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓，不进入当前 backlog |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；human/AI 输入 pass 分段；runtime 字段与 roster/opoint bootstrap；shared-DAT shell | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |
| 二次审计 INPUT-1~9 | **全部已修复 / Unity 运行时已验证** | real/shared-DAT input state、raw frame、velocity tail、running/frame215 等契约已按 authority 收口 | `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵通过 |
| 二次审计 INTERACT-1~5 | **全部已修复 / Unity 运行时已验证** | dynamic slot、满槽拒绝、runtime-slot vrest、state3003、non-character kind2 已收口；拒绝路径清理空 bucket/pool/reference 生命周期 | `CheckInteractionRuntimeSlotContracts` 通过 |
| Naruto DDJ / OPOINT-LIFECYCLE | **已修复 / 当前版本真实 Play Mode 已通过** | active-only reference release；register finalize pending old lifecycle；factory slot guard 后移；pooled character 重分配 StableId；`PostInitLiving` 补 Team/RelationTeam/HolderCopy 继承 | 真实生产输入链 `L -> L+S -> L+S+K` 通过；6 个 unique clone 均到 action307，6 个 renderer 同时可见 |
| 二次审计 RISK | **历史 RISK-1..5 均已关闭** | locomotion、raw move frame、held/Tracker slot、current-DAT interaction 与 fixed-slot reuse 已收口 | Audit5 对应 `R-GP/R-HC/R-FL/R-LC/R-FT` 总账 15/15 关闭 |

Audit3 历史验证（2026-07-16）：fresh `/m:1` build 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。M-1/T4 的 gate、oid8 镜像、identity/presentation、human+AI DJA full-tick、split formal reset 与 `ItrRest` 保留矩阵，以及 Audit3-10/12 的扩展矩阵均包含在该结果中。该结果是针对性断言证据，不是完整 Play Mode 或逐帧等价证明；M-1 已完成 runtime self-check，不能据此扩大为全部战斗逻辑完全对齐。RISK-4 与完整对局逐帧对拍仍是验证缺口；T8 默认 `stage.dat` 部署继续由用户明确暂缓。

当前版本已在真实 `NTSD_Battle` Play Mode 重新验证 Naruto 防下跳六分身：生产输入按同一逻辑帧渐进注入 `L -> L+S -> L+S+K`，经 `InputActionMap -> CharacterInputModule -> SimInputBuffer`；tick1 到 frame271，tick12 到 frame272 且 PP `500 -> 295`、生成 oid205，tick15 到 frame273 并开始展开 oid204，tick29-32 出现 6 个 unique oid33/action307，tick38 共有 6 个 renderer 同时可见。峰值为 `max204=11`、`max205=3`、`uniqueClones=6`、`action307=6`、`maxVisible=6`，因此该项定向 Play Mode **PASS**。Audit4 后续三项定向 Play 也已全部通过，证据见本节末。T8 默认 `stage.dat` 资产部署继续暂缓。

## 第四次战斗命中/技能链审计实施进度（BATTLE-AUDIT4，2026-07-17 最终状态）

> 唯一权威为 `J:\QQFile\NTSD2.4\ntsd_release_C#`；表内所有 C# 坐标均指向该工程。以下 16 项的生产修复已经落地，fresh full `BattleRuntimeSelfCheck` 与 3 项真实角色/输入/对象链 Play Mode 均已通过，Architect 最终复核为 **PASS**。本段是 Audit4 历史快照；当时保留的 RISK-4 已由 Audit5 `R-HC-05` 关闭。完整对局逐帧 production certificate 仍未取得。T8 默认 `stage.dat` 资产部署继续按用户要求暂缓，不进入本批任务。

| 编号 | C# 权威（文件 / 方法 / 行） | Unity 差异（文件 / 方法 / 行） | 影响 | 当前状态 |
|---|---|---|---|---|
| BATTLE-AUDIT4-01 | `Simulation/GameTick.cs:1265-1297` `RunCooldownsTick`：以当前 frame 的 `Itrs` 判定是否清 `AttackExempt`，并处理 state1001 holder/wpoint/attacking 分支 | `SimulationWorld.Passes.partial.cs:943-958` `ClearAttackExemptIfCurrentFrameCannotHit` 错查 `opoints/opoint`，且没有 holder 分支 | 攻击豁免可能在仍有 itr 时被清除，或在无 itr 时残留，导致技能/武器重复命中或错误漏命中 | **生产修复已落地；Audit4 针对性矩阵 fresh PASS** |
| BATTLE-AUDIT4-02 | `Interaction/HitResolve.cs:262-510` `ApplyDamageCandidate` 是真实角色、shared-DAT 和对象的统一标准命中结算；其中 `:447-485` 统一写 `FrameDelay/AttackExempt`，state1002 随机帧与 `Vx/Vy=-4`，并处理 FlyingA 对撞 | 真实角色 `LF2CharacterHitResolver.cs:360-420` 只向 `LF2LivingObject.HitCounters` 写豁免、普通受击写 `FrameDelay=-5`，state1002 不换帧且读取 victim `PS.vx`/写 `Vy=-3.5`；shared-DAT `LF2CharacterDatHitResolver.cs:681-744` 又采用另一套行为并额外写 `WeaponState`/ProjectileFlying frame10 | 同一 C# 命中规则在两条 Unity 路径漂移；投掷物反弹、首击结束时机、飞行物互撞和不同实体壳表现不一致 | **生产修复已落地；标准命中矩阵 fresh PASS；投掷武器 Play 09:45:21 PASS** |
| BATTLE-AUDIT4-03 | `Interaction/HitResolve.cs:26-65` `ResolveCandidates` 除显式 `AbortRemainingHitPairs` 外继续消费同 tick 后续候选 | `LF2WeaponInteractionResolver.cs:38-100` 成功命中后无条件在 `:99` `break` | 武器同 tick 多目标/多候选只处理首个成功对象，与 C# 候选消费数量和顺序不一致 | **生产修复已落地；连续候选/显式 abort 矩阵 fresh PASS** |
| BATTLE-AUDIT4-04 | `Interaction/HitResolve.cs:26-65,447-485` 使用 world `ARest/VRest` 契约；`Interaction/WeaponRuntime.cs:99-215` 的 held/throw/drop 路径没有额外清零双方 arest | `LF2WeaponInteractionResolver.cs:91-99` 额外调用 `ItrArestUpdate`；`LF2WeaponHeldStateResolver.cs:92-95,108-111` 在投掷/受伤掉落时清零 weapon 与 holder 的 `ItrRest.Arest` | Unity 的第二套 arest 状态会暂时挡住命中，冷却结束后又重新命中；投掷/掉落还会改变下一次可命中时机 | **生产修复已落地；held/Arest 断言 fresh PASS；投掷武器 Play 09:45:21 PASS** |
| BATTLE-AUDIT4-05 | `Interaction/CollisionCollect.cs:14-240` 在 collect 阶段完成 pair/geometry/team 等筛选；`HitResolve.cs:26-65` consume 只校验 slot、itr index、active/CharData 与 VRest | `LF2CharacterInteractionResolver.cs:45-139` 和 `LF2WeaponInteractionResolver.cs:43-99` consume 时再次计算 allow gate、runtime itr、target/team/type/geometry/arest 等条件 | collect 后到 consume 前状态变化会让已收集候选被 Unity 二次拒绝，技能命中窗口和候选顺序偏离 C# | **生产修复已落地；SpecialAttack 已删除 live Team gate；collect 后 attacker `Team=0` 仍消费两个冻结候选并 fresh PASS** |
| BATTLE-AUDIT4-06 | `Interaction/HitResolve.cs:563-617` `ApplyKind3Grab/AlignGrabPair`：raw 写双方 frame、按整数坐标快照对位、建立 slot 关系，不附带丢武器 | `LF2CharacterInteractionResolver.cs:265-350,419-450`：限制目标必须是真实 `LF2Character`，使用 `ImmediateFrame`，坐标/计数副作用不同，并在 `:446-447` 额外 `DropWeapon` | Naruto 奔跑 `L -> K` 的 `102 -> 295/296 -> kind3 -> 297 -> 298 -> 299 -> 275...` 后续链可能在抓取帧、对位或目标壳 gate 中断，导致命中后缺少下一招 | **生产修复已落地；kind3 real/shared-DAT 矩阵 fresh PASS；Naruto 奔跑防跳 Play 09:34:36 PASS** |
| BATTLE-AUDIT4-07 | `Interaction/HitResolve.cs:1318-1529` `ApplyKind0Type3Tail` 完整覆盖 state3000/3005/3006 的关系继承、双方速度/帧/延迟、effect 尾和声音 | `LF2SpecialAttack.cs:456-519` `Hit/ApplyPostHitSelfDestruct` 只覆盖部分 3000/3006 分支，且 oid201/214 的 `DieEvent`/HP 清零后处理按 Unity CLR attacker 类型分流 | 技能对象互撞、扩张/飞行态转换、关系字段及 oid201/214 自毁方向/时机与 C# 不一致 | **生产修复已落地；type3/oid201/214 针对性矩阵 fresh PASS** |
| BATTLE-AUDIT4-08 | `Simulation/GameTick.cs:1773-1870` `SpawnStateTransitionEffects` 规定 branch 判定及每个碎片的 RNG 调用顺序（Y、X、Vy、Vx 等） | `LF2Entity.cs:3501-3564` `SpawnLateTransitionEffects/SpawnTransitionEffectBranch1/2` 的随机取值顺序和次数不同 | 即使单个特效范围相同，也会推进不同的全局 RNG 状态，继而改变后续战斗随机结果 | **生产修复已落地；现有 transition/RNG 断言随 full self-check fresh PASS** |
| BATTLE-AUDIT4-09 | `Interaction/WeaponRuntime.cs:99-155` `RunHeldObjectStep12ForPair` 每 tick raw 写 `held.Frame/Facing/FrameDelay`，朝向直接跟 holder | `LF2CharacterWeaponLinkResolver.cs:251-292` 与 `LF2WeaponHeldStateResolver.cs:32-41,139-175` 每 tick `ImmediateFrame`，并按 cover 十位再执行额外 flip | held 对象的 attacking/wait 等计数被重复重置，朝向和挂点帧可能抖动或滞后，影响螺旋丸跟手、层级与按攻击键后的动作 | **生产修复已落地；raw frame/wait/facing 矩阵 fresh PASS；Naruto 螺旋丸 Play 01:10:34 PASS** |
| BATTLE-AUDIT4-10 | `Interaction/HitResolve.cs:382-406,889-906` 受击帧按 attacker/victim 的 `Facing` 关系选择 | `LF2CharacterHitResolver.cs:581-596,673-680` 与 `LF2CharacterDatHitResolver.cs:954-968,1011-1016` 通过 attacker 相对 X 推断方向 | 交叉、瞬移、同 X 或攻击者背向出招时会进入错误的正面/背面受击帧 | **生产修复已落地；real/shared-DAT facing 矩阵 fresh PASS** |
| BATTLE-AUDIT4-11 | `Frame/FrameTick.cs:242-252` 要求 first op 同时满足 `Kind>0 && Oid>0`；`:414-419` 为 oid5/52 初始化 `Hp/HpMax/Hp3/Pp=10/10/10/5` | `LF2ObjectPointFactory.cs:139-145` first-op 总闸门漏 `oid>0`；`:536-547` 的 oid5/52 初始化字段不完整 | 无效 first-op 可能错误放行后续生成；oid5/52 技能实体初始生命/PP 契约错误 | **生产修复已落地；first-op 与 oid5/52 初始化矩阵 fresh PASS** |
| BATTLE-AUDIT4-12 | `Interaction/HitResolve.cs:1084-1147` `RecordDamageEffectSound/RecordStandardHurtSounds/RecordAlternateHurtLeadSound` 覆盖 effect cue、effect1 附加声、attacker/victim 武器声音及 oid 条件 | `LF2CharacterHitResolver.cs:439-446` 与 `LF2CharacterDatHitResolver.cs:762-767` 主要只播通用 `SFX_001/006`；shared 路径部分判断还使用 `type_sub` 代替 oid（`:276-282`） | 命中确认的声音组合、声源位置和特定技能反馈与 C# 不一致 | **生产修复已落地；声音记录随 Audit4 full self-check fresh PASS** |
| BATTLE-AUDIT4-13 | `Frame/FrameTick.cs:13-216,218-230` 在规定 frame_tick 边界统一 `QueueFrameSound`；`SpawnFromOpoint` 仍按正常实体生命周期生成对象 | `LF2SpecialAttack.cs:96-98,230-231` 存在类内独立 frame sound；`LF2ObjectPointFactory.cs:331-340,467-477` 对 `pic=999,wait=0,next=1000` 直接播放并立即回收 | 同一声音可能在不同 pass 播放、重复或丢失；pic999 对象不再经历 C# 的注册、frame tick 和回收边界 | **生产修复已落地；living/weapon/SpecialAttack `PendingSounds` 单次精确断言与 tick/reset 清理 fresh PASS** |
| BATTLE-AUDIT4-14 | `Interaction/HitResolve.cs:503-507,1150-1195` 对成功 kind0 统一 `RecordKind0Hit`，不以 effect6/23 排除 spark 记录 | shared-DAT `LF2CharacterDatHitResolver.cs:770-773` 显式跳过 effect6/23 的 `SpawnSpark`，真实角色路径又在 `LF2CharacterHitResolver.cs:449-450` 无该排除 | 同一命中在真实角色与 shared-DAT 壳的 spark 记录数量/随机数消费不同 | **生产修复已落地；effect6/23 统一 spark 断言 fresh PASS** |
| BATTLE-AUDIT4-15 | `Simulation/GameTick.cs:142-147` 在交互后的 late update 推进 holder frame；`Interaction/WeaponRuntime.cs:99-155` 定义 held frame/挂点/整数位置契约。Unity 必须在 late holder 切帧后刷新该契约的表现结果 | `HeldObjectProcessAll` 早于 late `SimFrameTick`，holder 首 tick 切帧后 held 仍使用旧挂点；renderer 刷新也没有保证 holder 后于 held 的同 tick 可见顺序 | 螺旋丸已生成但首 tick 位置滞后、移动不跟手或层级/攻击表现落后一拍 | **生产修复已落地：late frame 变化后只调用纯 `SyncHeldPose`，不重复 step12，并按 holder→held 刷新 renderer；focused self-check 01:07:01 PASS；Rasengan Play 01:10:34 PASS** |
| BATTLE-AUDIT4-16 | `Interaction/CPointRuntime.cs:58-85` 按 `PrevFrame2` 与持久 `CaughtIdx/CatcherIdx` 维持抓取链；`Runtime/NtsdEntityRuntime.cs:178-190` 只在完整实体 reset 时清关系字段 | `LF2CharacterCatchResolver` 的普通 `state_exit` 与 `LF2Character.ResetStateRuntime` 提前清 `CaughtSlotIndex/CatcherSlotIndex`；`276 -> 277` 后下一 tick 的 cpoint 仍读 `PrevFrame2=276`，却因关系已清而强制 frame0 | Naruto 奔跑防跳抓取链在 276 后中断，缺失 277/278/279 与 86/87/88 后续招 | **生产修复已落地：普通 state transition 保留 catch link，完整实体 Reset 仍清；fresh full self-check 09:26:55 PASS；Running Play 09:34:36 PASS** |

### Audit4 fresh 验证证据（2026-07-17）

- 当前 Unity Editor PID `11540` 完成 fresh script compile，Console 为 **0 C# error**。
- 最终 freshness 链：`BattleRuntimeSelfCheck.cs` source/test `01:39:46` < `Library/ScriptAssemblies/Assembly-CSharp.dll` `09:26:23` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `09:26:55`；fresh full self-check **PASS**。
- 早一轮 held late pose focused freshness 链为 source `01:05:07` < DLL `01:06:22` < result `01:07:01`，结果 **PASS**；最终 full PASS 已再次覆盖该回归。
- Architect 复核后新增的 SpecialAttack 候选矩阵已进入本次 PASS：生产 consume 删除 live `Team` gate；候选在 collect 后把 attacker `Team` 改为 `0`，仍按冻结的 geometry/team 连续消费两个目标；显式 oid300 abort 仍会停止后续候选。
- SpecialAttack frame sound 断言精确要求 `PendingSounds.Count == 1`，且 Cue、WorldX、Tick 均匹配；living/weapon 分支、下一逻辑 tick 清空及 `ResetRuntimeState` 清空也在同次 PASS 中。
- Naruto 防前跳螺旋丸 Play `01:10:34` **PASS**：frame240 / oid434 / link 均成立；change runtime/holderVisual/heldVisual=`5/5/5/5`，move=`9/9/9/9`，sorting `526 -> 527`；攻击链 `20 -> 257 -> 258 -> 259`，oid434 `396 -> 397`。
- Naruto 奔跑防跳 Play `09:34:36` **PASS**：完整链为 `9 -> 102 -> 295(prev2)/297(pn) -> 298 -> 299 -> 275 -> 276 -> 277 -> 278 -> 279 -> 86 -> 87 -> 88`，victim 保持 frame130/catch；oid33 `current311/pn310` 是 wait0 的正确观测口径。
- 投掷武器 Play `09:45:21` **PASS**：使用生产 oid120 / hold / double-D / D+J；HP 只在 tick17 从 `500 -> 489` 下降一次；weapon state1002/frame41 后同 tick 切到 frame7/state1000，`AttackExempt=4`；跨 35 tick 冷却归零并落地，HP 无二次下降。
- 当前 Unity 自动生成的 dotnet `.csproj` 仍包含 35 个已删除历史源文件，最终 `dotnet build` 被 `CS2001` 阻塞。不得把此前的 dotnet 0 error 冒充为 Audit4-16 后的最终证据；最终有效编译证据是上述 Unity fresh script compile 0 C# error。

### Audit4 实施顺序与剩余边界

- **已完成的串行核心链**：`01 -> 02 -> 03/04 -> 05` 已按依赖顺序收口，cooldown、标准命中和 candidate 消费矩阵已进入 fresh PASS。
- **已完成的独立轨**：`07`（SpecialAttack type3 tail）、`08`（状态转换 RNG）、`09`（held 同步）生产修复已合并并通过已有断言。
- **已完成的第二阶段**：`06/10/12/14` 的命中尾与 `11/13` 的 opoint/声音生命周期生产修复已落地并通过已有断言。
- **Play 抓出的后续修复**：`15` 收口 late holder 切帧后的 held pose/renderer 同 tick 刷新；`16` 收口普通 state transition 错清 catch link。两项均已进入最终 full self-check，并由对应 Play 场景验证。
- **目标 Play Mode**：Naruto 奔跑 `L -> K` 后续招、Naruto 防前跳螺旋丸 held/层级/跟手/攻击链、投掷武器首击后的单次命中/Arest 时间线均已 **PASS**。
- **仍保留的审计/验证边界**：完整对局逐帧对拍尚未完成，RISK-4 仍是待审计风险，因此不能将 Audit4 本批验收扩大成“全部战斗逻辑完全对齐”。
- **非行为性清理债**：`WeaponSpawner` 仍有历史非 C# 注释，F9 debug 说明也存在与当前 C# 唯一权威措辞冲突的历史文字；F7-F9/debug 已按 `AGENTS.md` 排除正式战斗 backlog，不计为生产逻辑差异。

## 第五次全量逐帧审计（BATTLE-AUDIT5，2026-07-18 风险账收口）

### Audit5 权威、废止声明与验收口径

- 唯一战斗逻辑权威是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。Audit5 的差异定性、修复方向和对拍预期只来自该工程的正式 C# 调用链。
- 本文此前所有依赖其他旧来源的“已对齐”“已关闭”或“仅作映射参考”结论，在 Audit5 中一律废止为当前权威证据；相关实现只能作为 Unity 现状或历史回归基线，必须按 C# 重新核验后才能恢复完成状态。
- T8 默认 `stage.dat` 部署继续按用户要求暂缓；对拍场景默认 `stageFixture=false`，不会把默认资产缺失混入当前战斗逻辑差异。
- Audit5 的最终目标是在双方各自正式读取与 Unity 适配后，基于语义等价的 runtime 输入、同场景、同 seed、同 `FrameInputSet`，逐逻辑 tick 比较 400 个固定 runtime slot、world、RNG、arest/vrest、stats、sound events；这里的 400-slot parity 口径仅适用于 `Authority400` 兼容模式和该历史 schema。`Extended` 模式不能复用旧 certificate，必须新增分页 slot、generation handle 和稀疏 rest projection/schema。若继续签发 production parity certificate，也应以该语义 runtime 与 full/full trace 为准，不要求 raw DAT 文件或 manifest 相等。

状态词严格区分如下：

| 状态 | 只证明什么 | 不能据此声称什么 |
|---|---|---|
| 逻辑已写 | 生产代码中已落地目标修改 | 不能证明可编译或行为正确 |
| isolated/目标编译 0 error | 当前隔离编译范围没有诊断 | 不能代替 Unity fresh script compile |
| fresh 编译 | 最新 Unity 脚本程序集晚于目标源码且 0 error | 不能代替 self-check 或真实行为 |
| full self-check PASS | 最新程序集上的现有自动断言通过 | 不能覆盖未写断言、Play Mode 或双端逐帧等价 |
| diagnostic trace 一致 | 使用诊断夹具隔离后的已比较 tick/domain 一致 | 不能自动关闭未覆盖风险，也不能代替必要 Play Mode |
| production certificate | 适配后的语义 runtime 输入声明成立，full/full 全 tick、全 domain 相等 | 可作为聚合对拍证据；目前尚未取得，且 raw DAT/manifest 相等不是前置条件 |

### 静态审计总账与当前修复层级

三份报告按不同调用链分区，原始总账为 **74 个确认差异簇 + 15 个 trace 风险**。确认项现为 **74/74 逻辑实现 + focused/full `BattleRuntimeSelfCheck`**，原 15 项风险现为 **15/15 已关闭**。该计数是差异簇而非代码行；`BATTLE-AUDIT6-01/02` 仍作为原总账后新增且已关闭的项目单列，CP-NV1/2/3 与 STEP10 是原总账既有项重开后重新关闭，不另改分母。风险账关闭不等于取得任意对局、全输入、长时间 production certificate。

| 分区报告 | 静态覆盖与发现 | 当前实现与 fresh 证据 | 风险账状态 |
|---|---|---|---|
| GameTick / Physics | `GameTick.cs` 正式对局主干与 `Physics.cs` 全分支 100%；21 确认 + 3 风险 | `GT-01..15`、`PH-01..06` 共 **21/21 逻辑已写并进入 fresh full PASS** | `R-GP-01..03` **3/3 关闭** |
| HitResolve / CollisionCollect | 两个权威入口全分支；33 确认 + 6 风险 | `C-01..33` 共 **33/33 逻辑已写并进入 fresh full PASS** | `R-HC-01..06` **6/6 关闭** |
| Frame / lifecycle | 25/25 权威方法及 reset/registry/cooldown 依赖；20 确认 + 6 风险 | `FL-01..06`、`FT-01..04`、`OP-01..05`、`LC-01..05` 共 **20/20 生产实现与 focused/full self-check 通过** | `R-FL-01..03`、`R-LC-01..02`、`R-FT-01` **6/6 关闭** |

最终 combined freshness 链为：`BattleRuntimeSelfCheck.cs` source `2026-07-18 01:06:21.499` < Unity `Assembly-CSharp.dll` `01:07:21.125` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `01:07:52.834`，结果为 **PASS**。原 3 个受控 P2 已补强并关闭，Architect 最终复核为 `P0/P1/P2=0`。这仍不是任意对局、全输入、完整逐 tick 的 production certificate。

原 3 个受控 P2 的关闭证据：

- **HC-04 完整 step6 整链**：真实 `collect -> wrong loop 不消费 -> post consumer 消费` 链已进入 self-check，并覆盖 current type3 非武器负例，确认负例不产生 pickup/link/计数等副作用。
- **missing-definition 完整分派链**：Character 与 Weapon 两类 missing-definition shell 均覆盖候选收集、错误循环不消费、正确循环消费及 tail 结算，不再停留在 helper 级断言。
- **Interaction resolver helper 去漂移**：`LF2CharacterInteractionResolver` 的本地类型 helper 仅单行委托中央 `LF2Entity.ResolveCurrentDataObjectType`，不再维护第二份类型判定逻辑。

### 原 15 项 trace 风险关闭总账

| 分区 | 风险 | 状态与关闭证据 |
|---|---|---|
| GameTick / Physics | `R-GP-01` | ✅ 已关闭：fresh 双端 2 tick frame/wait trace；tick1 slot0 `frame=0, wait=37, FWC=11, HitStop=75`，tick2 `frame=5, wait=37, FWC=0, HitStop=74`，双方一致 |
| GameTick / Physics | `R-GP-02` | ✅ 已关闭：production 扫描确认可部署对象 `mass > 0`，static close |
| GameTick / Physics | `R-GP-03` | ✅ 已关闭：中央 active filter 覆盖 pass 输入集合与失活实体边界 |
| HitResolve / CollisionCollect | `R-HC-01` | ✅ 已关闭：确认差异后修复 zero-width strict overlap；Unity 适配后扫描锁定 90 项非正宽几何，并按权威严格交叠语义收口 |
| HitResolve / CollisionCollect | `R-HC-02` | ✅ 已关闭：oid999 `next` 闭包 14 帧均为零有效 geometry，`IsPureTransitionSmoke` gate 不吞有效碰撞 |
| HitResolve / CollisionCollect | `R-HC-03` | ✅ 已关闭：current OID/type 统一，gate A/B 正负路径均有覆盖 |
| HitResolve / CollisionCollect | `R-HC-04` | ✅ 已关闭：pickup 使用 current DAT type/OID，移除 CLR `LF2WeaponBase` cast 前置；真实 step6 collect、错误循环不消费、post consumer 消费及 current type3 负例均已覆盖 |
| HitResolve / CollisionCollect | `R-HC-05` | ✅ 已关闭：fixed slot 与 slot reuse 边界已验证 |
| HitResolve / CollisionCollect | `R-HC-06` | ✅ 已关闭：碰撞/命中整数坐标路径已验证 |
| Frame / lifecycle | `R-FL-01` | ✅ 已关闭：四类 weapon 矩阵覆盖 current DAT 分派与 frame lifecycle |
| Frame / lifecycle | `R-FL-02` | ✅ 已关闭：current-DAT boomerang 路径已验证 |
| Frame / lifecycle | `R-FL-03` | ✅ 已关闭：raw empty fixed slot 的 `CatchTimer=100` side effect、后续占槽清理与 world reset 已验证 |
| Frame / lifecycle | `R-LC-01` | ✅ 已关闭：pooled instance 的 snapshot/cache reset 已验证，旧 DAT 不泄漏到复用实例 |
| Frame / lifecycle | `R-LC-02` | ✅ 已关闭：StableId alias、注销与复用边界已验证 |
| Frame / lifecycle | `R-FT-01` | ✅ 已关闭：这是已关闭 `FT-01` 的 trace 验证债，不是重复生产风险；现有 fresh trace/self-check 已补证 |

R-GP-01 freshness：authority source `2026-07-18 00:11:23` < authority DLL `00:11:49` < trace `00:12:07`；Unity source `00:11:23` < Editor DLL `00:12:22` < trace `00:13:44`；compare `00:14:02` 返回 `status=equal-diagnostic`、`ticksCompared=2`、`firstDifference=null`。该证据关闭 R-GP-01，但只覆盖这 2 tick 的已观察域。

最终 PASS 前的失败均保留为诊断证据，不以最终结果淡化：

1. `C-05` 首先暴露 oid300 no-redirect 未继续 frozen pairs；根因是 CLR `LF2SpecialAttack` 覆盖了 current Character-DAT 分派，修正为 current-DAT 优先。
2. `BATTLE-AUDIT3-12` real `LF2Weapon` damaged release 未进入 dvx；根因是 GT current-DAT 新增的 `wrapper.type_sub` fallback 过宽，将未填 `type_sub` 的 real weapon 误判为 Character。已撤销该 fallback，并让 fixture 完整注册 `GameDataManager` 类型。
3. state8000/current type6 检查曾期待 landing 后 `Unk31C=-1`；权威与 production 实际在同一个 late pass 先 landing 写 `-1`，随后 weapon cleanup 归 0 并释放 slot。旧 fixture 停在中间态，现按最终 `0 + slot released` 断言。
4. `C-12` 的 `YInt<0/Vy>=0` fixture 朝向预期错误；权威先补 `KnockbackX=+5`，再为朝右 victim 选择 frame186 / `FallingBack`。修正 fixture 后 actual/shared 两路径通过。
5. 后续 Architect 对 GameTick / Physics 抓出的 `GT-04/GT-07/PH-02` 以及对 Weapon 抓出的 C-26/C-27 P1 均已按权威 C# 收口；原 3 个受控 P2 补强后最终复核为 `P0/P1/P2=0`。这些复核和 self-check 不能替代生产逐 tick 或目标 Play Mode。

### 原始总账后新增确认差异（BATTLE-AUDIT6，2026-07-17）

以下两项是在 Audit5 原始 **74 个确认差异簇 + 15 个风险** 建账后，由唯一权威 C# 调用链重新核实出的新增差异。它们不并入 74 的分母；生产、focused/full self-check 与最终 Architect 复核现均已收口。

| 编号 | C# 权威 | Unity 现状 / 修复 | 影响与当前证据 |
|---|---|---|---|
| BATTLE-AUDIT6-01 | `SimulationTickDriver.cs:42-47,93-116` 只把本 tick `SimulationFrameInput` 交给 `InputRuntime.PollHumanInput`；`InputRuntime.cs:611-624` 仅 roll 当前输入、写键值、tick cooldown、apply edges。正式 combo/direct/action/velocity 消费位于 `GameTick.cs:52-77`：`RunCooldownsTick -> marker/M-1 -> NeedClearInput gate -> GameTick>1 时 ApplyCharacterInputPass` | Unity 已拆分 human poll 与 unified character input，正式顺序改为 poll → cooldown/M-1 → clear/tick gate → character input；AI 同走 gate 后统一入口 | **已关闭**；矩阵覆盖 tick1、`NeedClearInput`、oid51 frame85 gate 外延迟 split、AI 顺序，以及 CLR character 在 current DAT 转为 non-character 后仍轮询 roster human 输入但不错误执行 character action。该 transformed-human P1 已补齐，combined fresh PASS |
| BATTLE-AUDIT6-02 | `InputRuntime.cs:826-893` 将九组 combo 复制为 locals；在 `frame null / comboDja != 3`、oid6 DjaGuard、成功/失败目标 frame jump、`Unk328==1` 四类 early-return 中均不执行 `:885-893` 的 locals 回写，只有正常尾路径才统一 commit | Unity 已按 C# 让 early-return 保留进入的 private/runtime combo locals，正常尾路径才 commit | **已关闭**；缺 target、有效 target、oid6 guard、`Unk328` 与正常尾 commit 的负向/正向覆盖进入 combined fresh PASS |

#### oid51 DJA 旧检查的反权威点

- 旧 `CheckOid5152DjaReleaseTriggersSameTickSplit` 假设 PostCooldown input 会在 M-1 前消费 DJA 并同 tick split；这正是 Unity 当前错误 pass-order 的产物，不能继续作为权威验收标准。
- C# 正式顺序中，M-1 先把 split cooldown `30 -> 29`；之后角色输入消费将 combo 清零，并在 state2 进入 frame85。frame85 落在 M-1 的 `[9,260]` gate 内，因此同 tick或下一 tick都不能自然 split。
- 旧 synthetic fixture 缺少 frame85，导致 Unity 跳转失败并停在 frame9，进而制造了“同 tick split”的假阳性。修复测试时必须补齐 frame85，并在离开 `[9,260]` gate 后验证延迟 split；不得通过删除 gate 或继续提前消费输入让旧断言变绿。
- 旧 same-tick split 断言与缺 frame85 fixture 已按权威重写；Audit6 两项和 Frame / Lifecycle 20 项均由 `21:57:40` combined fresh full PASS 覆盖。

### DAT 诊断统计与 production trace 口径

`Temp/NTSDParity/data-audit-v3-required.json` 对 137 个权威 OID 的结构化结果是：

- 34 个 OID 相同，66 个 OID 不同，37 个 OID 在 Unity 侧 raw 结构审计中未匹配，解析错误为 0。
- 差异类别计数包括 frame 126、碰撞 geometry 31、sound cue 155；这些是字段/类别差异计数，不是额外 OID 数量，也不互斥。
- 权威 production battle-logic manifest 为 `41c088d2...0375`，Unity production manifest 为 `6b34e118...332a`，当前不相等。
- 上述 34 / 66 / 37 与 manifest 只描述两套 raw DAT 在不同读取方式和 Unity 适配下的表示差异，保留作诊断统计；它们不是战斗逻辑阻塞、backlog 或资源部署清单，不需要把文件或 manifest 改成相同。
- `Temp/NTSDParity/compare-v3-full-final.json` 因旧工具按 raw manifest 做 header gate，在 tick 比较前返回 `status=different`、`certificateEligible=false`、`ticksCompared=0`。这说明该次工具运行没有 production parity certificate，不代表生产战斗逻辑失败。未来 certificate 必须改以适配后的语义 runtime 输入和 trace 为准，raw DAT/manifest 相等不得作为前置条件。

“37 missing Unity”不能解释为 Unity 缺少 37 个必须补部署的生产 DAT；它只是当前 raw 结构审计在 Unity 适配表示中未找到一一对应项。diagnostic runner 仍可使用权威 DAT headless 夹具隔离特定代码路径，但夹具结果只能证明明确覆盖的行为，不能外推到任意对局。

### v3 trace 工具与当前诊断结果

- `Tools/NTSDParity/NTSDParity.csproj` 已提供 `data-audit`、`trace-authority`、`compare` 和 trace self-test；Audit5 工具构建为 0 warning / 0 error。
- schema v3 逐 tick 校验 input、RNG、world、400 slot commitments、arest/vrest、stats 与 sound events；该 400-slot schema 仅是 `Authority400`/历史 parity schema。`Extended` 模式需新的分页 slot、generation handle 和稀疏 rest projection，不能伪装成旧 authority certificate。certificate 应比较双方正式读取/适配后的语义 runtime 与 full/full trace。最新 `Temp/NTSDParity/trace-compare-self-test-iter7.json` 为 **20/20 PASS**，覆盖连续 tick、空 trace、body/hash/slot commitment 防篡改、dense human input、diagnostic 显式 opt-in、diagnostic 永不签发 certificate、strict/fixed-world camera profile及非 camera world 字段严格比较。
- iter7 authority/Unity full-detail diagnostic trace 已生成。`Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json` 返回 `status=equal-diagnostic`、`ticksCompared=6`、`firstDifference=null`、`comparisonProfile=fixed-world-camera`、`diagnosticComparison=true`、`certificateEligible=false`、`certificateClass=none`。
- iter7 的 authority 端使用 production authority DAT，Unity 端明确使用 `authority-dat-diagnostic` 夹具；该 6 tick 结果只证明对应样例的已观察域一致。原 15 项风险已由各自证据逐项关闭，不是由 iter7 一次性关闭；iter7 和 R-GP-01 的 2 tick diagnostic 均不能被扩大为全战斗逐帧等价或 production certificate。

### 报告与证据索引

- `.omc/research/game-tick-physics-audit-20260717.md`：GameTick / Physics，21 确认 + 3 风险。
- `.omc/research/hit-collision-audit-20260717.md`：HitResolve / CollisionCollect，33 确认 + 6 风险。
- `.omc/research/frame-lifecycle-audit-20260717.md`：Frame / lifecycle，20 确认 + 6 风险。
- `Temp/NTSDParity/data-audit-v3-required.json`：137 OID 审计与 production manifest。
- `Temp/NTSDParity/compare-v3-full-final.json`：旧 raw-manifest header gate 的诊断结果，不是战斗逻辑失败证据。
- `Temp/NTSDParity/authority-v3-full-iter7.jsonl` 与 `Temp/NTSDParity/unity-trace-v3-diagnostic-full-iter7.jsonl`：iter7 双端 6 tick full-detail trace。
- `Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json`：`equal-diagnostic`、6 tick、无首差但不具 certificate 资格。
- `Temp/NTSDParity/trace-compare-self-test-iter7.json`：20/20 防护、输入与 profile 用例 PASS。

### 下一执行顺序

1. 原 15 项风险账已 15/15 关闭，不再把“逐项关闭 15 风险”列为下一步。
2. 若继续建设 production certificate，扩展到双方正式读取/适配后的语义 runtime、更多真实输入和长时间 full/full trace；2 tick diagnostic 只作为 R-GP-01 定向证据。
3. 保持 source < DLL < result/trace freshness；不处理 raw DAT 文件或 manifest 差异，也不得把 diagnostic 写成 production certificate。
4. T8 默认 `stage.dat` 部署继续独立暂缓，不得为 trace、certificate 或测试私自部署默认资产。

**Audit5/Audit6 历史结论（已被顶部 BATTLE-AUDIT7 当前结论取代）：原始确认项曾达到 74/74 逻辑实现 + focused/full self-check，原 15 项 trace 风险曾达到 15/15 已关闭；Audit6、CP-NV1/2/3 与 STEP10 也保持关闭，原 3 个受控 P2 亦已补强关闭。该批 fresh full self-check 为 source `01:06:21.499` < DLL `01:07:21.125` < result `01:07:52.834` PASS，Architect 当时为 `P0/P1/P2=0`。R-GP-01 fresh 2 tick compare 为 `equal-diagnostic`、无差异；它只能证明这 2 tick 的已观察域，不能扩大为任意对局、全输入 production certificate，更不能覆盖 BATTLE-AUDIT7 新发现。34 equal / 66 different / 37 missing Unity 仍只是 raw DAT 适配诊断，不是阻塞或 backlog；raw DAT/manifest 相等不是 certificate 前置。T8 默认 `stage.dat` 部署继续独立暂缓。**

## BATTLE-AUDIT9 详细差异冻结表（2026-07-18）

本节是“先盘点、后修复”的冻结边界。本轮只合并并去重四份只读报告中的未收口项，不修改生产代码，也不把历史 PASS、静态等价或单个 self-check 扩大成完整对齐结论。后续修复必须严格按本清单逐项进行；在清单冻结后，按清单实施修复的阶段尚未开始。

### 冻结计数

| 类别 | 数量 | 口径 |
|---|---:|---|
| 正式战斗 runtime 差异 | **9** | 5 个 Framework/pass/bootstrap/reset + 4 个 lifecycle/presentation；均为当前源码可确认且未修复 |
| 工具/trace 差异 | **1** | `RT.CHECK.01` parity snapshot projection；不等同于 runtime 语义差异 |
| authority-unresolved 待确认（BATTLE-AUDIT9 历史冻结） | **12** | `UNRES.01-05`、`DEP.INT.01-04`、`DEP.WORLD.01`、`DEP.RNG.01`、`DEP.DATA.01`；当前 code-only 定性数量为 0，不得作为现状计数 |
| Play Mode 未验证场景 | **4** | Naruto 防下跳、Naruto 防前跳螺旋丸、Naruto 奔跑防跳、投掷武器首击/持续命中；是验收缺口，不额外重复计数 |

报告依据：`.omc/research/full-diff-inventory-framework-20260718.md`、`.omc/research/full-diff-inventory-input-interaction-20260718.md`、`.omc/research/full-diff-inventory-lifecycle-presentation-20260718.md`、`.omc/research/reaudit-open-differences-20260718.md`。

### 正式战斗差异（冻结，未修复）

| ID | 权威 C# 调用链 | Unity 对应链 | 触发条件 / 预期与实际 | 分类 / 证据 |
|---|---|---|---|---|
| `FW-FLOW-01` | `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:53-67`：先 cooldown/step gate，再 `postCooldownInput` | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:32-43`、`RunFrameAdvancePhase`：input 观察早于 `VrestTickAll` | 普通非-results tick 且输入边沿与 ARest/AttackExempt 同 tick 到期；预期先递减再读输入，实际 Unity 先读输入 | confirmed-difference；静态调用链，未修复、未运行时验证。报告：framework inventory |
| `FW-FLOW-02` | `GameTick.cs:56-67`：清/设置 `BattleStepGate44905C`，mode=2 转 step-wait 并抑制 input | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:272-281`、`NTSDBattleTickSystem.RunReleaseTick` 无对应转换/抑制 | 单步/慢速 `BattleStepMode=1/2`；预期 gate 控制顺序，实际 Unity 无条件继续 input | confirmed-difference；静态调用链，是否可达仍需 production fixture。报告：framework inventory |
| `FW-BOOT-01` | `DirectBattleBootstrap.cs:138-140` 写 `Unk344=battleTeam`、`HolderCopy=slot` | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 未显式写 `Unk344`/`HolderCopySlot` | 初始玩家参与统计、holder-copy 或相关 AI/技能分支；预期 identity 字段完整，实际可能为默认 `0/99` | confirmed-difference；静态字段契约，未修复、未运行时验证。报告：framework inventory |
| `FW-BOOT-02` | `DirectBattleBootstrap.InitializeBattleStats:224-244`：difficulty HP bonus/cap、PPBound、respawn、HitStop、速度、输入边沿、Cd 全集 | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 主要依赖 `Initialize` 默认值，仅显式写部分 team/位置/速度/HitStun | 非默认 difficulty、DAT `Hp3` 非默认、pool/rebootstrap 或初始边沿非零；预期完整字段集合，实际依赖隐式 reset/default | confirmed-difference；字段契约缺失，未修复、未运行时验证。报告：framework inventory |
| `FW-RESET-01` | `BattleCore/Simulation/SimulationWorld.Passes.cs:13-70` reset 不调用 `NtsdRng.Srand`，进程级 RNG 延续 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:138-151` 每次 reset `Rng.Seed(0x4E545344u)`，之后 config seed 再播种 | 同进程重开/重赛后发生随机掉落或 stage spawn；预期按权威入口延续/显式播种，实际 Unity 增加 reset 播种边界 | confirmed-difference；静态调用链，播种归属仍有 authority-unresolved 依赖，未修复。报告：framework inventory |
| `LP-01` | `BattleCore/Interaction/WeaponRuntime.cs:169-212,287-303` generic held 正式 throw/kind3 都写 `ReleaseTick=currentTick` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` generic throw/kind3 通过 `ClearLinks(..., stampReleaseTick: true)` 写当前 tick | 非 `LF2WeaponBase` CLR 壳但按 DAT held 参加 step12，且 `Dvx != 0` 或 kind3；预期清 link 同时写 tick | confirmed-difference；**代码已写 / `CheckAudit9GenericHeldReleaseTickContracts` self-check verified / Play-unverified**。报告：lifecycle inventory |
| `LP-02` | `src/Host/SdlBattleRenderer.cs:476-497` 先 `ZInt`，同 Z 按 runtime slot 升序稳定绘制；随后按 `Shadow -> Entity -> EntityOverlays -> HitRecords` 展开 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` compact presentation sort、`Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` `ForceRefresh`；`LF2Sprite.cs` 表现刷新 | 两个实体同 `ZInt` 且无额外 cover；按 `(ZInt, runtime slot)` dense rank。四槽为 `Shadow/Entity/Overlay/HitRecord=0/1/2/3`；Unity Overlay 子序存在但对应 per-entity consumer 未实现。真实双实体 renderer 检查为 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5` | confirmed-difference；排序代码/self-check/architect verified，Overlay 仍为 confirmed blocker，Play-unverified。legacy 后端 guard 为 `8192` materialized active entities；移动端 `1000` 安全，DesktopExtended 在中央后端完成前受此临时表现上限约束。报告：lifecycle inventory |
| `LP-03` | `BattleCore/Interaction/WeaponRuntime.cs:169-212` 释放只写逻辑位置/速度/owner/link/ReleaseTick，层级由 `ZInt/slot` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:77-98,391-402` 额外写 `Runtime.Zz=1`，由 `LF2Entity.GetRenderSortingOrder` 加入排序 | 正式投掷起始帧；预期由权威 Z/slot 决定，实际 Unity 额外上抬一个 sorting order | confirmed-difference；静态表现契约，未修复、未 Play。报告：lifecycle inventory |
| `LP-04` | `src/Host/SdlBattleRenderer.cs:519-548`：实体/阴影分别按负 `HitStop` 阈值和四拍相位隐藏 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:416-448`、`LF2ObjectRenderer.cs:206-243` 已接入实体/阴影各自的 `HitStop` gate | 实体进入负 HitStop 闪烁/隐藏区间；预期按实体/阴影不同阈值隐藏 | confirmed-difference；**代码已写 / `CheckHitStopPresentationGates` self-check verified / Play-unverified**。报告：lifecycle inventory |

### 工具差异

`RT.CHECK.01`：权威 `BattleCore/Entity/CharacterSync.cs:796-877,173-317` 是内部 runtime snapshot；Unity `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:385-542` 输出带 alias/default/reset-slot 的 trace projection。两者 schema 不同，但当前 runtime 语义未证明不同；分类为 **trace/validator adapter difference**，不是正式战斗 runtime 差异。`reaudit-open-differences-20260718.md:44-56` 的 focused projection 已通过，但不得要求 JSON 形状相等来替代 runtime 对齐。

### Play Mode 未验证场景

以下四项保留为“本轮未验证”，不把旧日志自动视为本轮 freshness，也不把它们重复计入上面的 9 个正式差异：

1. **Naruto 防下跳六分身**：权威 `InputRuntime.RunCombo -> FrameTick` opoint 递归 oid205/204/oid33；Unity `CharacterInputModule -> SimInputBuffer -> SimulationWorld`。预期六个 clone、关系字段、renderer 可见且生命周期不提前结束。
2. **Naruto 防前跳螺旋丸**：权威 combo frame240 -> oid434/action396/397 -> held wpoint/step12 -> 257/258/259；Unity `LF2ObjectPointFactory`、`LF2WeaponHeldStateResolver`、`LF2CharacterWeaponLinkResolver`。预期层级、整数挂点、跟手和攻击键驱动 held DAT。
3. **Naruto 奔跑防跳后续招**：权威 running frame102 -> kind3/cpoint 295-299 -> 275-279 -> 86-88；Unity `LF2CharacterInteractionResolver`/catch/link pass。预期命中后下一招和 caught/catcher link 均持续。
4. **投掷武器首击与持续命中**：权威 `WeaponRuntime` release -> `HitResolve` -> ARest/VRest/AttackExempt；Unity `LF2WeaponHeldStateResolver`/`LF2WeaponReleaseFlowResolver`/hit resolver。预期首击只结算一次、ReleaseTick 与 rest 窗口一致。

报告依据：`full-diff-inventory-input-interaction-20260718.md:77-86`、`full-diff-inventory-lifecycle-presentation-20260718.md:97-118`。

### 明确排除与当前阶段边界

- F1-F7 已达到 **source/static + focused self-check 闭合**，但不等于全部 Play Mode 已验证；本冻结不把它们重新计入开放正式差异。
- 12 个 authority-unresolved 是历史冻结时的原始计数；BATTLE-AUDIT11 已将其全部定性，当前 code-only scope 下为 0。未修复的 confirmed code differences 仍不得视为已对齐。
- raw DAT/manifest 表示差异不属于当前差异清单；T8 默认 `stage.dat` 部署按用户要求继续暂缓。
- fixed-world camera 是用户批准的 Unity adapter；不得恢复 C# camera_x 表现链，也不得将 camera offset 写回 runtime 真值。
## 2026-07-22 — C++ 跳跃水平动量例外核验

- **问题**：移动中起跳后，Unity 未稳定保留起跳前的水平移动速度；按住方向进入普通跳跃时也可能读不到该方向。
- **本项行为依据（用户明确指定的例外）**：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\frame_advance.cpp` 的 frame 212 初始化。C++ 在进入 212 时始终写 `vy = jump_height`；只有右/左或上/下为互斥按住态时才以 DAT 的 `jump_distance/jump_distancez` 覆盖对应轴，否则保留起跳前 `vx/vz`。空中不执行地面摩擦。
- **共同根因**：C# `src/BattleCore/Simulation/GameTick.cs` 与 Unity `SimulationWorld.SerialTickAll` 都曾在 frame advance 前清除当前 action/directional keys。这样 late `frame_tick` 的 211 -> 212 初始化看不到本 tick 的按住态，属于 C# 移植与 Unity 共有、但 C++ 表现正确的差异。
- **Unity 修正**：`SimulationWorld.SerialTickAll` 不再在 `SimTransit` 前清当前键。输入 poll/AI preparation 继续负责下一 tick 的 previous/current 滚动与 release，`NeedClearInput` 的战斗入口全量清理保持不变。没有修改 DAT 数值、1.5 表现缩放或空中物理倍率。
- **回归契约**：`CheckGameTickInputClearBoundaries` 的 GT-02 改为断言 current/previous keys 在 frame advance 可见；新增 frame 211 -> 212 回归，覆盖“按住右/上使用 DAT jump distance”“无方向覆盖时继承原 Vx/Vz”“不制造 cooldown/history edge”。
- **当前证据**：`git diff --check` 无 whitespace error；`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` 为 **0 errors / 42 existing warnings**。目标源码最晚时间 `23:15:11` < Unity `Assembly-CSharp.dll` `23:15:37` < fresh result `23:16:33`，`Temp/NTSD_BattleRuntimeSelfCheck.result` 为 **PASS**。本项状态是 **逻辑已修正 / Unity 自动运行时已验证**；真实键盘 Play Mode 的移动起跳体感仍待用户或后续定向验证。
- **同时关闭的表现阻塞**：fresh 自检先定位出动态扩容池实例的 EntityModel mount 保持 `Invalid` handle。`BattleCentralPresentationMountRegistry.BindOwnerRuntime` 现会直接更新 renderer 本体 mount，并继续保留 slot+generation 校验；P4 pool-overflow 回归随后通过。

### Texture2DArray 现状澄清

- 中央渲染的角色图集主路径已经使用 `BattleSpriteCentralBindingMode.AtlasTextureArray`；设备不支持数组或策略选择 `OrderedPages` 时才回退到多 `Texture2D`。
- 公共阴影当前由 `BattleCommonVisualCatalog` 发布为 `SourceTexture2D`，没有进入角色的 `Texture2DArray`，所以阴影与角色仍会形成不同 resource segment/draw。该事实是批次边界，不代表角色数组路径未实现。
