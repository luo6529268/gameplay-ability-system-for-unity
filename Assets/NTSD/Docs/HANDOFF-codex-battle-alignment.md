# 接手文档 — NTSD C++ Release → Unity 战斗逻辑对齐（Codex 无缝接手版）

> **2026-08-20 接手定调（覆盖本文全部“C# 唯一权威”表述）**：唯一 gameplay authority 是 `J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime，而非 `ntsd_release_C#`。以 `src/entity/game_tick.cpp` 的 `game_tick(...)` 及其正式构建的 frame/physics/collision/weapon/cpoint/input/renderer 模块为准。C#、旧 self-check、旧 Architect 结论和原有 Play Mode 记录只作为历史移植/回归材料；与 C++ release live path 冲突时必须以 C++ 为准。

> **当前工作边界**：不废弃现有 Unity 架构或性能成果；按“C++ pass trace → Unity legacy/fallback 对照 → Unity fast path 对照 → 真实 Play Mode”逐模块收口。不得把 C++ 的 debug probe 当规则本身，但可用其观察 release live path。

## 2026-09-01：SIMULATION-WORLD-MODULE-EXTRACTION-001（用户批准、实施前）

当前实施进度以 `docs/ai/CHANGE-RECORDS/SIMULATION-WORLD-MODULE-EXTRACTION-001.md`
为准：M1～M9 已完成逐批 Unity 门禁，M10 代码清理和定向运行时已完成。最终
runtime/editor compile均0 error；AI回归158/158、worker/checksum/shutdown/architecture
合同35/35、三项陈旧owner-path测试3/3；两轮干净Play/Stop无目标cleanup warning，
Scene `isDirty=false`。full EditMode job `4d26dc2aaed44165807b5da87b4714cf`
完整执行1763项，但被position38、package version、Blood/Catch static guard和并行S0
WPoint既有基线阻塞；fresh完整SelfCheck停在任务外central-render P4断言。
`SimulationWorld.cs` 当前6040行，超过2500报警线的剩余根职责已在Change Record解释；
partial声明与历史partial文件均为0。按计划M10保持`IN_PROGRESS`，不得报告整个计划完成。

- 用户要求移除 `SimulationWorld` 以 partial 共享 private 状态的 God Object 实现，改成主 World 持有 Registry、AI、PassPipeline、Stage、Presentation 等普通 C# 子模块。
- 完整设计、模块所有权、Phase 0～5、验证矩阵和回滚合同已写入 `Assets/NTSD/Docs/simulation-world-module-extraction-plan.md`；Change/Task/Ledger/STATE 已在任何 C# 修改前建立。
- 当前基线：相关文件约22,727行，仍属于 World partial 的主体约20,130行，三个 AI partial 约13,412行。FrameInput、QueryAndLinks、StageWave、StageRender 已有 module 先例，继续沿用而非重建。
- 本 Change 只做架构所有权迁移，禁止改变 C++ pass 顺序、30Hz、input/OPoint/slot/RNG/checksum/worker/shutdown，也不处理当前 Naruto DDA SelfCheck 阻塞。
- 当前检查点已推进到 Phase2 Registry：架构 allowlist guard `2/2`；删除两个已空 FrameInput/QueryLink compatibility 文件；新增 Registry module 统一拥有 slots/rest/buckets；组合构造函数已移回主 World。compile0、slot5/5、query2/2、snapshot3/3、worker20/20、shutdown4/4。当前 partial allowlist仍为6，后续先完成 Registry 行为迁移，再进入 AI/Pass。
- 后续检查点：Registry claim/release/register/unregister/deferred核心已迁，partial约1672→706行；AI aggregate和Input/Sensing/Decision子模块已建立并接管首批状态。AI input相关46/46、sensing12/12、worker20/20、shutdown4/4通过；decision 67/68，唯一position38与2026-08-24已登记独立基线一致。AI算法主体、PassPipeline和partial=0仍未完成。
- 最新检查点：所有 `partial class SimulationWorld` 声明和
  `SimulationWorld.*.partial.cs` 历史文件已清零，独立 AI types 与 Stage modules
  已使用正常文件名；production `Assembly-CSharp` fresh build 为92 warning/0 error。
  尚未抽离的算法体暂时机械合并在约19,439行普通 World 中，因此只完成 partial
  硬边界，不代表 AI/Pass 模块抽离完成。fresh production/editor dotnet build
  均为0 error；Unity Test Runner桌面自动化未获批准，最终 focused 尚未运行。
  当前 Editor 消费完整 SelfCheck 请求后，在任务外
  `CheckUnityBattleCameraRemainsDisabled` stale-camera 注入断言失败，未包装为通过。
  Phase4a随后把 OID7/8↔51 的 timer/merge/split/HP gate 算法完整搬入独立
  `BattleOid5152RuntimeModule`，World只保留public转发和4类internal capability；
  Runtime/Editor build均0 error，OID focused因Test Runner UI不可控仍待运行。
- 用户再次 Refresh 后，当前 Unity 已完成 M1 fresh focused：architecture `4/4`、
  OID5152 `7/7`、Respawn `4/4`，合计 `15/15 PASS`，请求文件已消费。M1
  停止条件已满足，现严格进入 M2，仅提取 `BattleEarlyFrameAdvanceModule`；后续批次
  仍保持未完成。
- M2 已把 Early teleport、state500/state501 handle/special、scratch 和 diagnostics
  物理迁入独立模块，World只保留readonly引用、public façade与窄capability；隔离
  Runtime/Editor完整编译均0 error。当前Unity尚未导入新物理文件，M2 request已放置；
  用户再次Refresh后应自动运行architecture4+early6+flow1，`11/11`未绿不进入M3。

## 2026-09-01：BATTLE-RUNTIME-ORDERED-SHUTDOWN-001（代码与真实 teardown 通过，full SelfCheck 外部阻塞）

- 已按 `Assets/NTSD/Docs/battle-runtime-ordered-shutdown-contract.md` 实现固定 11 阶段关闭事务：先关 tick/input、停 worker，再关 spawn、unseal、清 publication、discard OPoint、归还 renderer、清 World、unbind、quiesce pool，最后由 Bootstrap 清 runtime map/boundary 后进入 `Stopped`。
- shutdown 只使用 preparation/seal 阶段捕获的 factory/pool owner，不在 Stopping 阶段解析或创建全局 singleton；Editor `ExitingPlayMode` 与 App Scene unload 复用同一 coordinator。
- 新鲜证据：compile 0；focused `4/4`、worker `20/20`、W05 OPoint `8/8`、central latest frame `13/13`、singleton teardown `2/2`；真实 `NTSD_Battle` 两轮 Play/Stop 均 error/warning 0、cleanup warning 0、Scene `isDirty=false`、无 factory/pool/boundary runtime carrier。
- 完整 SelfCheck 连续三次运行均在既有 Naruto DDA 240-247 throw-chain 断言失败；本 Change 不获准修改 Naruto 战斗规则，状态为 `BLOCKED` 而非 `VERIFIED`。详细文件、job ID、后置条件和恢复条件见 `docs/ai/CHANGE-RECORDS/BATTLE-RUNTIME-ORDERED-SHUTDOWN-001.md`。

## 2026-08-29：WORKER-UNITY-BOUNDARY-001（生产 Play 阻塞修复）

- 用户真实 Play 堆栈确认 Dedicated Worker 在实体 late destroy/free 时经 `LF2Sprite.ApplyEntityRendererVisibility` 触发 `EnsureRunningOnMainThread`，Driver 随后按 fail-closed 合同暂停；这与已还原的 2.5D 实验无关。
- 根因是初始生产实体在 worker 启动前已经绑定 Unity Renderer，而 `SetLogicOnlyEntityMaterialization(true)` 只影响后续实体，现有资格检查没有拒绝该状态。
- Change `WORKER-UNITY-BOUNDARY-001 / VERIFIED / SAFE-SYNC-FALLBACK` 已增加 Renderer-bound world 的 worker 资格拒绝并保留同步 tick。Unity worker整类job `881e133b32ae4d3f82043dc29ecec66d` 20/20 PASS；真实Play至tick2860保持unpaused/failure=null并已发生66次Free，原两类异常0条。该结论不声称Unity-bound worker或worker性能已恢复；后续重启必须单独处理主线程presentation detach/release所有权。
- 权威记录：`docs/ai/CHANGE-RECORDS/WORKER-UNITY-BOUNDARY-001.md`。

## 2026-08-08：1000 AI catch-up CPU 预算接手状态

- `ProductionEntityStressHarness` 已加入可关闭的 `catchUpCpuBudgetMs`：request 默认 `0` 保持历史吞吐口径，诊断 Window 默认 `33.33 ms`。首 tick 不会被预算阻止；后续 tick 依据本帧累计耗时和上一 tick 实测成本决定是否延后。报告/fingerprint 已包含预算与受限帧数，backlog/dropped tick 不隐藏。
- 预算 A/B 报告分别为 `Temp/NTSD_ProductionEntityStress.catchup-budget-1000-120ticks-20260808.json` 与 `Temp/NTSD_ProductionEntityStress.catchup-throughput-1000-120ticks-20260808.json`。预算模式最大 `1 tick/frame`，Unity frame Avg/P95=`75.388/103.443 ms`（约 `13.26 FPS`）；无预算模式最大 `4 ticks/frame`，为 `161.826/257.808 ms`（约 `6.18 FPS`）。可见卡顿下降，但预算模式 dropped tick=`113`，无预算=`20`，所以该模式只改善 Editor 交互性，不提高逻辑吞吐。
- 核心 lockstep 所有分层哈希和 overall 均一致（overall=`2348281130f1c432260ccb9f17a6f31affc06a08632724c3be77070542ce82e4`）。extended slots/overall 的差异由每 tick 与最终 catch-up tick 两种表现刷新频率造成，仅涉及 presentation-finalized hit-record 字段。
- 仍未达到目标：预算模式完整 tick Avg/P95=`45.573/61.142 ms`，无预算带表现 tick Avg=`34.597 ms`。两轮 logic tick allocation=`0 B/tick`，但 Editor Profiler frame GC 仍为数百 KB 级。后续不要继续把追帧调度当成容量优化；应回到单 tick 的 CharacterInput、CandidateCollect、RenderDispatch/表现构建和 Editor frame 分配归因。
- fresh 回归：Unity 编译完成；focused job `ded49f6e80d346eebee7f3229bdfc0e6`=`2/2 passed`；full EditMode job `15ba76f83027436db37474e58681c015`=`720/720 passed`；完整 `BattleRuntimeSelfCheck`=`2026-08-08 12:54:38 PASS`。两轮 teardown 均 restored、cleanup exception=`0`、active runtime 清零。1000 AI 稳定 30 Hz gate 仍开放；T8 与 Android 真机继续排除。

## 2026-08-03：BuildCommands / 高频表现命令接手状态

- 已完成两项有证据的等价优化：`BuildCommands` 每 tick 单次捕获 viewport 快照；可信生产资源命令只做一次身份校验并走 `ResolveTrusted`。外部/测试/身份失配命令继续完整签名校验并 fail-closed。
- 详细 A/B：`BuildCommands` Avg `5.295 -> 2.789 ms`（`-47.3%`）；`ResolveCommands` Avg `5.066 -> 4.190 ms`（`-17.3%`）；`PrepareFrame` Avg `10.418 -> 8.842 ms`。详细诊断有逐命令观察开销，只用于模块归因。
- 真实 no-detail 两轮 1000 AI 120 tick：Avg/P95=`32.846/40.521 ms` 与 `32.648/39.167 ms`；sampled GC=`0 B/tick`，两轮 parity hash 相同，teardown 后 active GameObject/world entity/claimed slot 均为 0。平均值跨过 30 Hz 预算，但 P95 仍为 `39–40 ms`，Unity frame Avg 仍为 `58–60 ms`，不得写成稳定 30 FPS。
- 下一瓶颈按 coarse 数据排序：CharacterInput Avg/P95=`7.742/8.924 ms`、RenderDispatch=`7.137/8.619 ms`、CandidateCollect=`4.024/9.580 ms`。优先调查 CandidateCollect 的高 P95 尖峰，再决定是否继续压缩输入和表现命令常态成本。
- fresh 验证：compile=`0 error`；focused command writer/mesh backend=`10/10`、resolver=`15/15`；完整 EditMode job `6e2addcb4bbb4d089e8f669ac802f595`=`714/714`；`BattleRuntimeSelfCheck`=`2026-08-03 00:37:00 PASS`。T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 2026-08-02：Slice 0/1 fresh 接手状态（当前口径）

以下状态覆盖下方旧日期的“当前结论”。接手者应以此处证据边界继续工作，不得将诊断相等、focused 测试或 self-check PASS 扩大为迁移完成。

### 已取得的 fresh 证据

- Authority400 的 W01/W02/W03/W04/W07 位于 `.omc/validation/authority400-witness-slice0-final-20260802/`，五项均为 `equal-diagnostic`，比较 tick 数 `8/2/2/1/3`；五项都 `certificateEligible=false`，仅是诊断证据。
- W06 focused EditMode：UnityMCP job `e1e1a3d8b6e74bb895fa0068333a4cf7`，`2/2 passed`。
- W06 导入前的既有全量 EditMode：UnityMCP job `e1b5a3057e7d4f7bbdd71bbb8cbe381e`，`472/472 passed`、`0 failed`、`0 skipped`。W06 导入后当时发现总数为 `474`，但该轮只执行了新增 W06 的 `2/2`；此后 full-order 运行暴露的 **W05 测试隔离失败**已完成代码修复，但 Unity LicenseRevoked modal 阻止正式 focused summary 与 full-suite 重跑。当前 full suite **不得写绿**。
- fresh `BattleRuntimeSelfCheck`：`2026-08-02 02:49:44 PASS`。

### Registry 日志与 W05 接手状态

- `SimulationWorld.Registry.partial.cs` 中成功 Register/Unregister 的 `Debug.Log` 已退出默认生产/测试路径：world-level `EnableRegistryLifecycleLoggingForDiagnostics` 明确默认 `false`，只有显式诊断启用时才构造插值字符串并写 Log；注册、slot/generation、structural event 与 pass 行为未改。fresh Runtime/Editor dotnet build 均为 `0 errors`。
- `.omc/validation/UnityMCP-http-restored-editor-20260802.log` 为 `3,309,135,435 bytes`（约 `3.309 GB` 十进制），只作为发现默认生命周期日志和 Unity 堆栈放大的证据。压力 harness 运行时固定 `Debug.unityLogger.filterLogType=LogType.Error`，Candidate final300 报告也记录 `runningFilterLogType=Error`；普通 Register/Unregister `Log` 已被过滤。接手者**不得把本次日志退出算作 Candidate long300 性能收益**，不得据此修改其 Avg `34.7836 ms` / P95 `50.888 ms` 或稳定 30 Hz 失败结论。
- W05 root cause 是 `GameConfig.LF2ObjectPrefab` 跨测试污染。隔离 fixture 已保存原值、测试期间清空并在 dispose 恢复；每 renderer 严格两个 mount、current generation 绑定、released generation 不复活与 no-ghost command 的断言合同全部保留。fresh Editor dotnet build 为 `0 errors`。
- focused Unity 运行已到 `RunFinished`，日志没有新的 assertion failure 信号；但 `License revoked: Your Unity Personal Version license has been revoked` 触发 `EditorWindow.ShowModal`，MCP job 未产生正式 summary，full suite 也未重跑。接手状态是**代码修复 / Unity 回归未验收**，不得写 focused PASS 或 full-suite PASS。

### 1000 AI 性能接手数据

| 顺序 / 模式 | Avg (ms) | P95 (ms) | 接手判断 |
|---|---:|---:|---|
| 正向 Legacy | `43.848` | `60.641` | 同轮基线 |
| 正向 Candidate | `31.327` | `38.780` | Avg/P95 相对改善 `28.56%/36.05%`，为相对门禁 positive evidence |
| 正向 Candidate + Remainder | `34.181` | `42.998` | 比 Candidate 回退 `9.11%/10.88%` |
| 反向 Candidate | `32.000` | `39.777` | 与正向 Candidate 接近 |
| 反向 Candidate + Remainder | `34.246` | `40.426` | 比 Candidate 回退 `7.02%/1.63%` |
| 反向 Legacy | `74.494` | `163.728` | **异常系统抖动/离群样本**；不得据此宣称反向相对门禁闭合 |
| Candidate final300 no-detail | `34.7836` | `50.888` | `0 B/tick`；可真实运行，但仍未稳定达到 30 Hz |

原始报告为 `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-*-20260802.json`。最新长样本 `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-final300-20260802.json` 为 `StoppedCleanly`、`60 warmup + 300 sample`、Avg `34.7836 ms`、P95 `50.888 ms`、GC average/maximum=`0 B/tick`，final parity overall hash=`68af82ba7cdf284d7f62e889e1cd1188e14e9c15ec48d15167cd6c8dcf210388`；teardown `restored=true`、活动对象/world/claimed slots 清零、cleanup exception=`0`。它证明 Candidate workload **可真实运行**，但 Avg 超过 `33.33 ms` 且 P95 为 `50.888 ms`，绝对稳定 30 Hz 仍失败。Remainder 在两个顺序都回退，继续关闭。

### Gate-B candidate list pool fresh 接手证据

- 修复前 Unified SoA configured 基线 `Temp/NTSD_ProductionEntityStress.dispersed1000.gateb-authority-100ticks-20260802.json` 为 Avg `31.367 ms`、P95 `40.225 ms`。Profiler 已把 CandidateCollect 的分配根因定位为每 tick 对约 1000 个 AI 各创建一个 `List`，合计约 `155.9 KB`。
- candidate list pool 修复的 focused Unity job `fe5de...` 为 `4/4`；fresh `BattleRuntimeSelfCheck` 为 `PASS`。
- fixed 报告 `Temp/NTSD_ProductionEntityStress.dispersed1000.gateb-listpool-100ticks-20260802.json` 有效、`StoppedCleanly`，overall hash=`0ce668469bce74a7945adc2981ff7bacc5596f6dcc374d3ee6478a694a70976d`，与旧 configured 基线一致；逻辑 tick Avg/P95=`34.552/45.569 ms`，CandidateCollect Avg/P95/max=`4.991/10.438/14.588 ms`，sampled GC average/maximum=`0/0 B`，Console=`0 error`。
- **接手边界：**Editor 抖动使这份 fixed 报告不能证明性能回退，也不能证明目标达到；1000 AI gate 仍开放。broadphase pair peak=`23,262`，约为均值的 10 倍；broadphase-only SoA shadow 正在推进但尚未切为 authority。T8 默认 `stage.dat` 与 Android/Adreno/Mali 真机验收继续排除。

### CandidateCollect 负实验接手记录

#### CandidateStore authority：不切 production default

| 报告 | authority | Tick Avg/P95 (ms) | CandidateCollect Avg/P95 (ms) |
|---|---|---:|---:|
| `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-neighbor-detail100-20260802.json` | off | `39.1249 / 56.0624` | `4.3780 / 9.5977` |
| `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-store-only-detail100-20260802.json` | on | `43.1031 / 65.4513` | `5.3683 / 16.2869` |

- on 相对 off 的 tick Avg/P95 回退 `10.17%/16.75%`，CandidateCollect Avg/P95 回退 `22.62%/69.70%`，未通过至少改善 10% 的门禁。
- 两份报告均为 `StoppedCleanly`、GC average/maximum=`0 B/tick`，所有 final parity 分层 hash 与 overall hash 相同（overall=`fa019a38aba6668b7222bf9b61b0400d2cba7b422799bbd0964506a9875450e9`）；on 为 `requested/configured/applied=true`、`appliedTickCount=160`、legacy fallback=`0`。teardown 均 `restored=true`，活动对象/world/claimed slots 清零，cleanup exception=`0`。
- 接手动作：只保留显式诊断/实验入口，生产默认继续关闭；不要将 CandidateStore 当作 CandidateCollect 性能赢家。

#### Stamped role-aware ordinal map：负实验已删除

| A/B 顺序 | 报告 | stamped | Tick Avg/P95 (ms) |
|---|---|---|---:|
| 正向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-off-detail100-20260802.json` | off | `34.8805 / 46.3090` |
| 正向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-on-detail100-20260802.json` | on | `38.8578 / 68.2115` |
| 反向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-on-detail100-rev-20260802.json` | on | `34.0021 / 45.7062` |
| 反向 | `Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-stamped-off-detail100-rev-20260802.json` | off | `32.2586 / 40.3518` |

- stamped on 在正向顺序的 tick Avg/P95 回退 `11.40%/47.30%`，反向顺序仍回退 `5.40%/13.27%`；正反向都未过门。
- 四份均为 `StoppedCleanly`、GC average/maximum=`0 B/tick`、teardown `restored=true`、活动对象/world/claimed slots 清零、cleanup exception=`0`；on 两份实际应用 `160` ticks 并在 teardown 恢复。反向 A/B 的全部 final parity hash 相同（overall=`fa019a38aba6668b7222bf9b61b0400d2cba7b422799bbd0964506a9875450e9`）。正向 A/B 的 input/RNG/metadata/world/A-rest/V-rest/stats/events hash 相同，但首份 off 是冷池运行（inactive pool capacity `10 -> 1000`），所以 slots/overall hash 与随后暖池 on 不同，不能写作正向 overall hash 相同。
- 接手动作：实验实现、专用接线与测试均已删除，仅保留上述 JSON；生产不存在 stamped 开关或默认启用路径，不要恢复该方案，除非先提出新的可证伪假设和门禁。

### 接手后仍需关闭

1. 不切换 Candidate production switch/default，直到生产接线、失败回退与门禁证据完整。
2. CandidateStore authority 性能负实验保持默认关闭；stamped ordinal map 正反向负实验已删除，不是生产候选。
3. W05 根因与代码修复已关闭；先解除 LicenseRevoked modal，取得正式 focused summary，再重新执行 fresh full suite。在此之前 Unity 回归仍未验收、不得写绿，也不能沿用导入前 `472/472` 冒充当前全量结果。
4. 继续处理绝对稳定 30 Hz 与 slot identity parity；Authority400 equal-diagnostic 不替代这两个合同。candidate list pool 已修复该 Profiler 分配根因并保持 parity，但 fixed Editor 样本不构成性能改善/回退结论；pair peak 与 broadphase-only SoA shadow authority 切换仍待关闭。
5. 所需定向 Play Mode/真实战斗运行时证据仍须按风险补齐，当前不可写“完成”或“全面对齐”。
6. T8 默认 `stage.dat` 部署与 Android/Adreno/Mali 真机验收继续排除，不得顺手纳入。

## 2026-07-26：1000 实体优化接手状态

**当前结论：三项等价优化已实现并通过代码侧回归，但 1000 实体仍未达到 30 Hz。** 接手者不得把 `104/104`、self-check PASS 或单轮耗时下降扩大为性能验收完成。本节取代下方 2026-07-24 压力阶段中“尚无 per-pass timing”和“AI 仍全槽扫描”的历史接手描述。

1. **最终可见 tick：**`LocalFreeRun + CentralOnly` 的 catch-up 只在最后一个可见 tick 构建中央表现命令；中间 tick 的 battle pass 全部执行。`LegacyOnly`/`Shadow` 不抑制。
2. **Late snapshot：**removed=`StateSpecial/FrameExit/PrevFrameMirror/Recovery/FrameTickSuppressed/CleanupCompleted`；retained=`FrameTick/DeathOpoint/TailAndQueuedFlush`。最新 334-tick 报告中 removed `callCount=0`，retained 各 `334000`，平均 `0.850/0.668/0.771 ms`。
3. **AI：**exact empty-air、ground team partition、phase1 Team5 list、融合索引与 first-10 top/second 已接线。新增 occupancy-epoch resolver elision：`RuntimeSlotTable` 成功 claim/allocate/release/reset/grow 后推进非零 epoch；snapshot 前后 epoch 一致才发布，filter 以 epoch、generation、slot entity 证明，失败 `Abort -> existing brute`。实时 HP/team/state/Y/Vx、tie、same-Z、air、RNG 与 slot consumption 不变。

### 最新性能证据

| 报告 | tick (ms) | `CharacterInput` (ms) | `FindGround` (ms) | `Remaining` (ms) | `Late` (ms) |
|---|---:|---:|---:|---:|---:|
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-air-fastpath-detail-20260726.json` | `110.846 ms` | `37.318` | `15.698` | `24.944` | `23.444` |
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-team-partition-detail-20260726.json` | `85.898 ms` | `29.171` | `9.237` | `16.779` | `14.373` |
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-index-fusion-detail-20260726.json` | `62.061 ms` | `21.344` | `6.847` | `12.370` | `11.841` |
| `Temp/NTSD_ProductionEntityStress.dispersed-full-ai-late-recovery-elided-detail-20260726.json`（334 ticks） | `82.712 ms` | `28.148` | — | — | `14.164` |

- **occupancy 最新报告：**`Temp/NTSD_ProductionEntityStress.dispersed-full-ai-occupancy-epoch-detail-20260726.json`：402 sampled ticks，tick `53.483 ms`；`CharacterInput/Ground/Air/Nearest/Remaining/Late=17.919/5.712/2.036/7.748/10.086/10.130 ms`；`bruteFallback=0`、visits/AI=`25.16`。
- **禁止跨轮硬比较：**同机仍有其他 Unity Editor 与系统负载漂移；no-AI `72.343 ms/tick` 甚至慢于 full-AI `62.061 ms/tick`。occupancy 独立收益尚未稳定隔离，最新结果仍高于 `33.33 ms/tick`。
- **fresh gate：**Unity compile `0 error`；EditMode `49f6e6800c8a45db988de0b7b9f412ef` 为 **112 completed / 0 failed**（global total=`216`，不要写 112/112）；self-check `2026-07-26 04:37:33 PASS`；Architect `PASS`、`P0-P2=0`，`P3` 仅为证据措辞。
- **下一步：**nearest 暂停继续大改，转向 `RemainingAiDecision` 与全实体基础 pass；继续观察 `FrameTick/Opoint`、`CandidateCollect`。删除 retained Late refresh 前仍先做 debug-only snapshot delta oracle。
- **清理：**本批所有报告 `restored=true`，active GameObject/world entities/claimed slots 均为 `0`；inactive pool capacity 增长只表示缓存保留。
- **继续排除：**T8 默认 `stage.dat` 和 Android 真机验证不属于当前任务。

## 2026-07-24 P8 v5 当前交接结论（覆盖下方 v3/v4 历史快照）

- P8-A/B1/B2 已完成，P8-C 保持既有生产 factory/pool、像素与稳定性验收范围。P8-D v5 的 Editor/Windows Development Player `100/300/500/1000` 共 8 份 real-runtime A/B 报告全部 PASS。
- 报告路径为 `Temp/P8-D-runtime-{100,300,500,1000}-{editor,player}-ab-v5.json`。每个 backend 有 120/120 样本，适用必需指标完整，generation-owned texture memory 为正，600-frame leak 与 post-dispose teardown 通过，owned bytes/resources 归零；Central/Legacy 的 workload fingerprint、input fingerprint、final runtime checksum 相等。
- v4 的全局 `Texture Memory` counter 返回 0，属于 `Incomplete` 历史证据；v5 不再依赖它。Player 也不再使用 `-batchmode`/`-nographics`，从而保留真实 graphics device、GPU timing 和 draw-call 证据。0 draw calls 对非空 workload 无效，正式样本最多重试 16 次，耗尽为 `Incomplete`。
- 当前 16-retry/cleanup 源码重新生成的 Editor `100/300/500/1000` 报告完成于 `2026-07-24 03:00:12`、`03:06:39`、`03:12:02`、`14:10:19`：logic tick 平均/最大依次为 `13.227/45.537`、`42.752/198.637`、`78.149/221.383`、`36.488/201.219 ms`。其中 Editor 300/500/1000 平均均超过 30 Hz 的 `33.33 ms` 预算；Player 1000 为 `9.123012 / 42.3011 ms`。数据非单调且受 Editor/当前机器影响；PASS 只说明门禁和可比 workload 通过，**不代表性能预算达标，也不代表 Central 必然快于 Legacy**。
- fresh 验证：UnityMCP EditMode job `9869909f3c27446d8ca33cbaf0f436ab` 为 `44/44 passed`、`0 failed`、`0 skipped`，取代此前 `34/34` 的旧证据，并包含 request processor lifecycle 的 3 个 focused tests；完整 `BattleRuntimeSelfCheck` `PASS`，Runtime/Editor dotnet build 0 errors。矩阵连续启动 300 Player 时曾一次 native exit `-805306369`；同 build 的独立 300 单样本与完整重跑均退出码 0，最终四档 Player 报告均有效。
- fresh Architect 最终只读复核：`PASS`，`P0=0`、`P1=0`、`P2=0`、`P3=0`。复核范围包括 benchmark 生命周期、v5 policy、8 份报告、teardown、A/B identity、fresh `44/44`、self-check/build/Console 与当前文档边界；它不推翻 Editor 性能预算未达标的结论。
- 本轮修复 P1：Play Mode 退出可能遗留 hidden benchmark runner，让请求已被消费但永久显示 `RUNNING`。processor 已接入 `ExitingPlayMode` fail-close、非 Play 状态 runner reconcile 与 EditMode request 保留；新增 3 个 focused tests 通过。
- P8-E Android/Adreno/Mali 真机继续由用户负责；T8 默认 `stage.dat` 部署取消/排除。后续接手者不得用下方 v3/v4 或 presentation-only 结论覆盖本节。

### 2026-07-24 ProductionEntityStressHarness 全交互压力交接

- 新增 `ProductionEntityStressHarness`：真实 `GameObject`、正式 `SimulationWorld`、全 AI/正式输入、碰撞命中、opoint 与完整 lifecycle，配置为 `MobileExtended(1050)` 和 `LooseQuadtree`；它不等同于 P8-D v5 的受控/冻结 A/B workload。
- smoke 证据 `Temp/NTSD_ProductionEntityStress.smoke-fresh-v3-20260724.json`：50 个初始实体、46 个衍生实体、peak `96`，`SmokePassed`。teardown 后 active GameObject/world objects/world entities/claimed slots/objectPool active/referencePool active 均为 `0`；objectPool available 从 `10` 增长到 `96` 并作为 inactive 缓存保留，不是资源恢复到运行前基线。
- **cleanup remediation 回归：**teardown 现按阶段 best-effort 执行，使用 stress root 独立扫描 `activeGameObjectsAfter`；清理异常进入结构化记录并令 `restored=false`，retained inactive pool capacity 仅作信息，不参与 `restored` 判定。`Temp/NTSD_ProductionEntityStress.smoke-cleanup-remediation-20260724.json` 为 `SmokePassed`：50 initial、peak world entities `301`，`restored/activeState/driver/logging=true`，`cleanupExceptionCount=0`，active GameObject/world objects/world entities/claimed slots/objectPool active/referencePool active after 均为 `0`，retained inactive capacity 为 `10 -> 301`。这是追加的 remediation 回归，不替代上一条旧 smoke 的 50 initial + 46 衍生历史数据。
- **cleanup remediation fresh 验证：**Unity fresh compile `0 error`；focused EditMode job `1327ac9736cf4b03ad9a73d75dabd298` 为 `15/15`；`BattleRuntimeSelfCheck` 于 `22:02:29` 为 `PASS`。
- dispersed 1000 证据 `Temp/NTSD_ProductionEntityStress.dispersed1000-cleanlog-20260724.json`：1000 个真实 GameObject/world entities/slots、41 samples；平均/P95/最大 `3077.612/5943.039/6245.802 ms`，pair sum/peak `5706633/184181`，candidate peak `735`；`StoppedCleanly`。teardown 后上述活动对象/逻辑注册与 active pool 计数均为 `0`；objectPool available 为 `10 -> 1001` 的 inactive 缓存，不是资源恢复到运行前基线。
- concentrated 1000 证据 `Temp/NTSD_ProductionEntityStress.concentrated1000-short-20260724.json`：1000 个真实 GameObject/world entities/slots、25 samples；平均/P95/最大 `5148.808/8889.234/9848.765 ms`，pair sum/peak `11427523/499500`，candidate peak `198`；`StoppedCleanly`。teardown 后上述活动对象/逻辑注册与 active pool 计数均为 `0`；objectPool available 同样为 `10 -> 1001` 的 inactive 缓存，不是资源恢复到运行前基线。
- **接手结论：**Editor 1000 全 AI、全交互实体约 `0.1-0.3 FPS`，远未达到 30 Hz。不能以 P8-D v5、中央渲染正确性、资源或 teardown PASS 声称 1000 实体完整战斗性能达标。代码审查已确认：`BruteForceSceneQuery` 的 formal fallback participant 会与全部 participant 配对、排序去重后双向 `CollectCandidatesForPair`；分散场景 peak fallback=`154`，仅 fallback 理论约 `142,065` unique pairs，约为实测 peak `184,181` 的 `77%`；集中场景 peak=`499,500`（`1000 choose 2`），但 candidate peak 仅 `198`。次要热点是所有 1000 实体启用 AI 时，`SimulationWorld.AiInput.partial.cs` 每个 AI 仍扫描 slots `20..1049`，约 `103` 万 slot visits/tick，部分 phase 另有同队扫描。当前报告没有 per-pass timing，不能精确分摊毫秒。清理方面可结论为本次运行的活动对象与逻辑注册清理正确，inactive pool capacity 仍只是保留缓存信息。T8 `stage.dat` 仍取消/排除；Android 真机仍由用户负责，状态不变。

## 2026-07-23 P8 当前交接证据（优先于下方 P8-C/P8-D 历史快照）

- **P8-B 已具备可核验诊断契约：**`FrameId`、显式 `AtlasPageIndex`、strict binding validation、first unresolved/unsupported status 和 generation/tick-coherent aggregate diagnostics 均已进入当前实现与 focused/full checks。Runtime/Editor 相关构建为 0 errors。
- **P8-C 生产验收已更新：**`Temp/P8-C-Resume-Live/P8-C-report.json` 于 `2026-07-23 17:28:29` **PASS**。正式链是 `LF2ObjectPointFactory.CreateObjectImmediate` / `FreeEntityLikeExe`：`availableBefore=7`、`totalCheckout=9`、`expandedAndPublished=2`、`availableAfter=9`、`uniqueRuntimeHandles=2`，cleanup PASS。生产 `Entity(33,0)` type `0`、`AtlasPageTexture2D` 在 Legacy/Central 都有 `4971` alpha pixels；`Entity(100,0)` type `4`、`AtlasPageTexture2D` 都有 `2090` alpha pixels；两者 maximum pixel diff 均为 `0`。该 factory/pool/publication 证据不覆盖 skill-input opoint。
- **P8-D final v3 取代 synthetic-only 描述：**`Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v3.json` 及 matching `-player-ab-v3.json` 全部 **PASS**。每一项使用真实 `MobileExtended(1050)` primary + mirror `SimulationWorld`、准确数量的真实 `LF2Entity`、`FrameInputSet.Empty` 和完整 `NTSDBattleTickSystem`；执行 30 warmup + 120 sample logic ticks、deterministic checksum、真实 handles/generation/positions 的 frozen presentation、相同 A/B workload 与 600-frame leak gate。不能由此写成 central 快于 legacy。

| v3 report | logic tick avg/max ms | tick allocation avg/max B |
|---|---:|---:|
| `100-editor` | `8.3087375 / 12.0803` | `0 / 0` |
| `300-editor` | `24.3566941666667 / 33.9412` | `0 / 0` |
| `500-editor` | `42.7971166666667 / 57.0061` | `0 / 0` |
| `1000-editor` | `100.006675 / 126.7602` | `0 / 0` |
| `100-player` | `0.537154166666667 / 1.285` | `0 / 0` |
| `300-player` | `2.59706583333333 / 29.4842` | `0 / 0` |
| `500-player` | `1.56702166666667 / 2.752` | `0 / 0` |
| `1000-player` | `2.980925 / 6.0687` | `0 / 0` |

- **性能解释：**Editor 1000 的约 `100 ms/tick` 不满足 30 Hz；Windows Standalone final v3 1000 约 `2.98 ms/tick`。A/B PASS 证明同一工作负载和 gate 的正确执行，不是一般性的 performance-winner 结论。
- **最终顺序回归：**held geometry 失败已定性为 parentless/root renderer 的 `_visualTransform == rootTransform`，不是 benchmark 全局状态泄漏；正确世界位置曾被随后对同一 Transform 的 local-zero 重置。当前实现只对独立 child visual 归零，并有 focused fixture 验证 runtime X/Y/Z、`FirstPresentationTick`、`CentralShadowBuild`、legacy suppression 与 immutable central command 对照。fresh DLL `18:05:55` 晚于源码 `17:59:02`；1000 实体 A/B `18:10:49` PASS，退出 Play 后 full self-check `18:13:03` PASS；最终 Runtime/Editor dotnet 构建分别为 `0 errors / 42 warnings`、`0 errors / 48 warnings`。当前仍不预写 Architect PASS。P8-E Android/Adreno/Mali 与 T8 默认 `stage.dat` 继续排除。

## 2026-07-23 P8 中央渲染交接状态（当前）

- P8-C 正确性/像素矩阵已闭合到其定义范围：`Temp/P8-C-EditModeTest/P8-C-report.json` PASS；`Temp/P8-C-LivePool/P8-C-report.json` PASS（真实 Play pool `4 available -> acquire 5`，5 个唯一 mount owner）。旧 job `f278668e3a2445139c6a1a5ceb8815be` 的 11/11 仅为历史；P2 回归后的 fresh job `e455b7f70043438a938faa23e82e53f3` 为 12/12（P8-C 2 + P8-D 10，0 failed/skipped）；fresh full self-check `Temp/NTSD_BattleRuntimeSelfCheck.result` 为 2026-07-23 12:07:26 PASS（P2 `BattleRenderingBenchmark.cs` 11:56:24 < Unity DLL 11:59:33 < result 12:07:26）。过滤到的 2 条 Console error 是 self-check 刻意构造的 registration rollback / mismatched rest binding release 拒绝路径（`BattleRuntimeSelfCheck:7046` / `:1133`），无编译错误栈或 benchmark 异常。
- P8-D 四档受控表现 A/B 报告均通过：`Temp/P8-D-presentation-100-ab-rerun.json`、`300`、`500`、`1000`。它们严格验证 presentation count/commands、256x256、资源、owned memory 与 retained heap 阈值，但只是 deterministic synthetic presentation workload，不表示真实 SimulationWorld active capacity、logic tick 性能、生产 atlas 性能或全面性能收益。P2 已关闭 EditMode 将 mesh segment 冒充 `Graphics.DrawMesh` submission：`presenterSubmissionDrawCalls` 显式 unavailable，Play 仅在实际调用提交后计数；其他无法取得的 main/render/GPU/draw 指标也保持 unavailable。本轮没有 Standalone Player 实测。
- 额外 current-scene production 覆盖：`Temp/P8-D-current-scene-ab-v2.json` PASS。退出 Play 前真实 `NTSD_Battle` 是 `ObjectCount=12/tick=3847`，冻结 published frame 是 `6 entities/12 commands`；Central/Legacy 均实际 `6/12`、同 fingerprint `f3aaf429518f46ec`、同 256x256。retained managed heap 为 Central `+28672 B`、Legacy `+49152 B`，graphics/owned bytes `+0`、resource count 不变。presentation build/GPU 仅为一次 Windows Editor 样本，main/render/draw unavailable；该项只是额外生产覆盖，不是 P8 独立 gate 或整体性能结论，运行后已退出 Play。
- P8-E Android/Adreno/Mali 真机验证仍由用户负责，T8 默认 `stage.dat` 部署继续排除。下方关于 P8-C/D 待实施、Play/pixel/Profiler 待验收的相反表述均为历史记录，不能覆盖本节。

## 2026-07-22 对象池预热上限后 opoint 武器不可见（当前接手结论）

- **复现：**隔离 `PoolInitialSize=10`，经生产 `opoint`/factory 保留 12 个 `LightWeapon`。第 11/12 个实体逻辑、声音、unique root/renderer、mount/runtime handle、sprite、12 条 Entity command 均存在，中央像素却缺失。
- **定性与修复：**这不是 C# 战斗逻辑差异，也不是 pool 扩容、runtime handle 或资源问题，而是 Unity `BattleDynamicMeshBackend` 的动态 submesh descriptor 生命周期适配缺陷；权威 C# 不定义此 Unity 渲染实现。旧布局/增长时默认 descriptor 临时重叠，Unity 2022.3 收缩 `subMeshCount` 会截断 index buffer。每 chunk 现维护 `activeSubMeshCount`，physical `subMeshCount` 为只增不减的 high-water；增长后先置全部 descriptor inert 再写 active，非增长先清旧 active 再写 active，empty 不收缩；禁止 bulk `SetSubMeshes`，此前会触发 native crash。
- **回归覆盖：**隔离预热 10、生产 `opoint`/factory 12 个 `LightWeapon`，检查 unique root/renderer、mount/handle、sprite、12 条 Entity command；并覆盖 `1 -> 32 -> 1 -> 33 -> 1`、inactive inert tail、`GraphicsBuffer.count=24576`、`4096/4097`、recovery、0 GC、scoped warning 捕获。
- **fresh 证据：**source `20:24:58` / `20:26:45` < DLL `20:28:54` < result `20:29:44` **PASS**；Unity `0 compile errors`。本轮 `Editor.log` offset `31277122` 后 descriptor overlap、bulk `SetSubMeshes`、native crash 均为 `0`；Editor PID 响应正常。
- **诚实状态：**代码、编译、self-check、生产 `opoint` 链已验证；用户真实 Play Mode 视觉复测仍待确认。T8 明确排除，默认 `stage.dat` 部署继续暂缓。

## 2026-07-22 Rendererless 武器显示回归修复（当前接手结论）

- **问题与根因（旧复现限定）：**4 个随机掉落武器存在后，角色 `opoint` 武器会使掉落武器与新武器不显示；后续 `opoint` 仍不显示，但落地声音继续。rendererless `LF2Sprite.Hide` 把 `EntityVisible=false`，成功 `ShowPic(valid)` 未恢复它，导致 `CurrentEntry`、`pic`、逻辑和声音正常而中央 Entity command 被持续过滤。此 `EntityVisible` 根因只解释该旧复现，不解释 `PoolInitialSize=10` 后第 11/12 个实体已有 command 但缺像素；该问题以本文件上方的 Unity 动态 submesh descriptor 适配缺陷为准。
- **已修复的边界：**仅成功解析 catalog 或 legacy sprite 时恢复 `EntityVisible`。`pic=999` 与 missing sprite 不恢复显示，保留失败/隐藏语义。
- **fresh 证据：**Unity `Assembly-CSharp.dll` `2026-07-22 18:56:11` fresh compile，Console `0 error`；完整 `BattleRuntimeSelfCheck` `18:58:50` **PASS**；`dotnet build Assembly-CSharp.csproj` 为 `0 errors / 42 warnings`。Play Mode：4 个预存随机武器后，经 `LF2ObjectPointFactory` `opoint oid121` 的 `Hide -> ShowPic`，随机 slot `50` 和 opoint slot `54` 都有 Entity command；销毁复用同一 renderer instance 后再执行 `opoint`，slot `54` command 仍存在；central `IsStale=false`、`unresolved=0`。
- **验收边界：**本项仅证明该 rendererless 武器显示回归已完成编译、self-check 和定向 Play Mode 验证；不扩大为完整战斗系统或全设备/资源组合验收。T8 默认 `stage.dat` 部署继续暂缓。

## 2026-07-22 Rendererless Central Mount 收口（当前接手结论）

- **生产 prefab 已接线：**`EntityObject` 与 `Shadow` prefab 的对应节点均已挂载 `BattleCentralPresentationMount`；持久 `Entity`/`Shadow` `SpriteRenderer` 已从生产 prefab 移除。common shadow 使用 `BattleCommonShadowDescriptor`；`LF2Sprite` 维护 renderer-independent 的 `visible`、`pic` 与 offset。默认模式为 `CentralOnly`。
- **生命周期、销毁和失败语义：**mount 标记 `[ExecuteAlways]`，以 `gameObject.scene.IsValid()` 为 gate；prefab asset 本身不注册。Prefab Stage preview 可参加编辑态 lifecycle，但没有 runtime handle，因而不属于生产 battle/pool 验证。mount/renderer 在 `OnDestroy` 主动移除 owner binding，防止 pool expire 销毁后静态字典仍保留 destroyed wrapper。冷启动失败 fail closed；已有成功帧后出现失败时保留 last-good frame 并记录 stale。该表现路径不回写战斗 runtime。
- **fresh 自动、编译与 Console 证据：**mount source `15:41:46` < Unity `Assembly-CSharp.dll` `15:43:40` < 完整 `BattleRuntimeSelfCheck` result **PASS** `15:44:50`；full self-check 已加入真实 `DestroyImmediate(root)` focused 断言，覆盖销毁 owner binding 清理。主代理最后一次 `dotnet build` 为 **0 errors / 18 existing warnings**，此前 42 warnings 属于不同生成视图。最新清空 Console Play/Stop 为 **0 error / 0 warning**。第一轮截图工具自身的 RenderTarget errors 不作为项目错误或项目验证证据，第一轮截图不能用于写 Console 为零。
- **最新 Play Mode 状态：**`NTSD_Battle` 最新观测为 `objects=6`，requested/effective=`CentralOnly`，`frame`、`ownership`、`submission`、`submitted` 均为 true，`draws=6`，`sim/display tick=339`，`stale=false`；3 个生产 `LF2ObjectRenderer`、6 个 mount/handle 均有效，且 `persistent SpriteRenderer=0`。
- **前一轮视觉证据：**此前 `objects=12`、6 个生产 renderer、12 个 mount/handle、`draws=12` 的观测继续保留为前一轮视觉证据；`Temp/central-rendererless-game-20260722.png` 可见角色、武器与阴影。该截图不代表最新运行的对象数量。
- **当前 Prefab Stage 例外：**一个 `EntityObject` prefab-stage preview instance 仍带旧 `SpriteRenderer`，但 `logic=null`；它可参加编辑态 lifecycle，却没有 runtime handle，是当前打开 Prefab Stage 的内存态，不属于生产 battle/pool 对象，未计入生产验证。本轮没有修改或关闭用户的 Prefab Stage。当前 Scene View 位于该 Prefab Stage，故本轮没有 fresh Scene View 截图；此前 Scene View 证据仅保留为历史证据。
- **继续排除：**T8 默认 `stage.dat` 部署和 Android/真机验证仍不在本轮范围内。

## 2026-07-22 Central Presentation Mount v1（历史快照，已由上方 rendererless 收口取代）

- **已实施，范围严格受限：**新增 `BattleCentralPresentationMount` 和 `BattleCentralPresentationMountRegistry`，并在 `LF2ObjectRenderer` 中声明/注册。v1 只建立 generation-aware `RuntimeEntityHandle` 绑定；没有更改渲染、资源加载、`Update`、render command 或战斗 runtime。
- **World 生命周期：**`SimulationWorld` 已在 register、release 与 reset 路径接线，确保 handle generation 变化或 runtime slot 复用后，旧 mount 不会继续代表新实体。disable -> enable restore 与 rollback clear 均已关闭并纳入 `BattleRuntimeSelfCheck` 覆盖；本批新增并通过了 world `ResetRuntimeState` 与 registration rollback focused checks。自检同时覆盖 `LF2ObjectRenderer` 集成、注册、release、reset 和 generation-aware binding。
- **场景与迁移边界（历史）：**当时尚未编辑 prefab，`Legacy` 保留；当时的下一项是向 `EntityObject` 的 `EntityModel` 和 `Shadow` nodes 挂 mount component 并设置 `ownerRenderer`。此段不代表当前 rendererless `CentralOnly` 状态。
- **最终验证：**relevant source `2026-07-22 11:48:18` < Unity `Assembly-CSharp.dll` `11:49:08` < `Temp/NTSD_BattleRuntimeSelfCheck.result` **PASS** `11:50:11`。最终完整命令 `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 完成，结果为 **0 errors / 42 existing warnings**；Architect closure 为 **PASS / no P0-P2**。Console 清空后仍有两类预期 self-check-active Error：既有 mismatched release 和新的 registration rollback。因此本批不是“Console 0 errors”验证，任何旧的 0-error Console 叙述不得用于证明此批。

## 2026-07-22 Editor Scene View Preview Validation

- **Scope and guardrails:** `CentralOnly` submits the same immutable mesh from the base Scene View camera only under `#if UNITY_EDITOR` and `Application.isPlaying`. Only the exact world camera may update renderer readiness. Scene View preview does not alter combat state, Player builds, or the Game camera.
- **Freshness and automated evidence:** Unity `Assembly-CSharp.dll` timestamp `23:47:47` is newer than the relevant source timestamp `23:30`; the direct `BattleRuntimeSelfCheck` result is **PASS**.
- **Observed Play/Scene View evidence:** Play state reported `objects=12` and the central mesh reported `quads=12`. `Temp/screenshot-20260722-000938.png` shows all current entities in the Scene View. The screenshot round's tool-originated RenderTarget errors are not project evidence; this screenshot does not establish a Console-zero result.
- **Validation boundary:** This verifies the current observed Scene View preview state only. It does not establish coverage for all resource scenes. T8 default `stage.dat` deployment and Android/device validation are not part of this task.

## 2026-07-21 Fresh Final Validation（接手时的当前渲染状态）

- **CentralOnly 已在真实 Play 接管**：运行时为 `requested/effective=CentralOnly`，`frame/ownership/ready/submitted=true`，`draws=12`。P7 Overlay、Shadow、Entity 与 HitRecord 都使用同一帧 pixel owner；“CentralOnly 继续拒绝”“Overlay blocker”“P7 未完成”均已是历史状态。
- **已修复的像素根因**：`BattleDynamicMeshBackend.ClearActive` 置 `subMeshCount=0` 会让 Unity 2022.3 释放 native index buffer，下一次写入造成黑块/三角形 UV 伪影。现保留零索引 inert submesh，恢复稳定索引缓冲；该修复不改变战斗 tick 或实体逻辑。
- **像素与运行时证据**：暂停同一帧 Legacy/Central `1920x1080` 截图比较 `changed=0`，截图直接覆盖其中可见的角色、武器/球体与阴影。Overlay/HitRecord 的 ownership 与资源路径由 self-check 和运行时 diagnostics 证明，不宣称它们在该截图中一定可见。`Temp/NTSD_BattleRuntimeSelfCheck.result` **PASS**；Unity Console **0 error / 0 warning**。显式 `LooseQuadtree` 真实 Play 为 `backend=LooseQuadtree, objects=12, tick=1436`，亦 **0 error / 0 warning**。B2C Architect final：**PASS / no P0-P2**。
- **Editor 性能快照**：Legacy `6.1884 ms CPU / 0.346112 ms GPU / 18 draws`；Central `6.5114 ms CPU / 0.70656 ms GPU / 20 draws`；Central `1391.17 MB allocated / 1005.19 MB graphics`。这是当前 Editor 样本，不代表 Central 已取得性能优势。
- **仍需外部环境的验收**：没有真实 Adreno/Mali 或 Android Player 的目标设备数据，故不得声称移动真机通过。T8 默认 `stage.dat` 部署继续按用户决定暂缓，不能为了验收私自补资产。

> **历史注记**：下方关于 `CentralOnly` unavailable/拒绝、Overlay blocker/P7 未完成、B2C 无 Architect final，以及 Play/pixel/Profiler 未验收的文字，均为本次 fresh final validation 前的阶段快照；保留用于溯源，当前接手判断以本节为准。

## P7 Batch6 per-entity Overlay 接手状态（2026-07-21，覆盖旧 blocker 叙述）

- **已收口的代码侧缺口**：P7 Batch6 完成了 per-entity Overlay。Unity 已新增 `WORDS0.bmp` 至 `WORDS5.bmp`，SHA256 与权威 C# host 使用的运行时资源来源一致；这只是资源依赖核验，不引入 C# 之外的战斗逻辑权威。唯一战斗逻辑权威继续是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。
- **资源和 runtime 契约**：typed `CommonWordGlyph(sheet, charCode)` 覆盖 `6 * 256` glyph，authority top-left rect 转 Unity bottom-left；WORDS 预热采用 exact-black transparency、Point/Clamp、atomic publication/retirement。`BattleSlotLabelRuntimeState` 为 `char[10,12]` + `int[10]`，已接入 reset 和 `MatchConfig` bootstrap。
- **绘制行为**：`BattleEntityOverlayLayout` 无分配处理复活 counter、普通与括号标签、普通 `Com`、特殊 `WORDS5 Com`，标签 clamp、counter 不 clamp，容量错误 fail-closed。snapshot 分离原始 `ObjectId`（shadow 223/224 gate）与 current DAT identity（Overlay）；顺序是 `Shadow -> Entity -> OverlayGlyph -> HitRecord`。legacy pooled `BattleEntityOverlayRenderer` 有 generation/stable-id guard；默认 `LegacyOnly` 仍发布 immutable frame 但不构建 central mesh，`CentralShadowBuild` 保持诊断，`CentralOnly` 继续由 `ValidateAvailable` 拒绝。
- **资源生命周期与自检**：frame-level catalog lease、HitRecord cycle lease finalizer、empty-frame no-retain 已实现；self-check 覆盖 retirement 窗口，以及 HP2、slot/bracket/empty/Com、palette、特殊 OID/type/hitstop、clamp、fail-closed、命令序列和 zero-GC。
- **fresh 验证**：latest relevant source `2026-07-21 16:01:49` < Unity `Assembly-CSharp.dll` `16:03:35` < full self-check result `16:04:54` **PASS**；Unity Console **0 C# error**；最后一次主代理 `dotnet build` **0 errors / 18 existing warnings**；Architect final **PASS / no P0-P2**。`git diff --check` 留待主任务最终执行。
- **未完成门槛**：不得表述 P7 全门槛完成。Play/pixel/Profiler/Adreno/Mali 未验收；T8 默认 `stage.dat` 部署保持排除。下方所有“Overlay 未实现”“Assets 没有 WORDS0..5”“confirmed blocker”或“Overlay 阻塞”的文字均属 Batch6 前历史快照，已被本节覆盖。

## B2C Extended checksum 交接（2026-07-21 当前状态）

- `Authority400` 继续使用冻结的 `ntsd-battle-trace-v3`，direct parity capture 仍只接受 `Authority400/400`；Extended 通过通用 API 生成独立 `ntsd-unity-extended-battle-checksum-v1`，`LastFrameSnapshot` 保持 Authority-only。
- Extended checksum 已覆盖 profile/capacity/count/tick、slot claimed/generation/stable ID/current DAT、active runtime、已物化未 claimed raw runtime，以及确定性稀疏 ARest/VRest；未物化分页不会因 capture 创建，错误 rest-store/victim-slot binding 会使 capture 失败。
- focused matrix 覆盖 Mobile `1050` / high slot `1049`、Desktop `512 -> 768` / slot `700`、高槽 rest、raw runtime、纯 generation reuse、profile separation、`65536` 稀疏容量和 repeat/non-mutating capture。
- B2C 还已接入 generation-aware AI Loose Quadtree 输入快照查询，以及显式 `LooseQuadtree` 后端下的即时 weapon/body current-world 查询；索引、几何或映射异常均回退 brute，生产默认仍为 `BruteForce`。
- freshness：source `2026-07-21 03:00:45` < Unity DLL `03:05:59` < latest full self-check `03:07:05` **PASS**；dotnet **0 errors / 42 existing warnings**；最终 architect review **PASS / no blocker**。
- P1 compact legacy sorting 已完成代码和自动验证：`(ZInt, runtime slot)` dense rank，`Shadow/Entity/Overlay/HitRecord=0/1/2/3`。权威 host 确实在 Entity 后绘制 per-entity Overlay；Unity 只有子序、没有对应消费者，这是 confirmed blocker。相关 `SpriteRenderer`/`SortingGroup` 全部使用 `Object` sorting layer。legacy 表现后端 guard 为 `8192` materialized active entities，移动端 `1000` 安全，DesktopExtended 在中央后端完成前有该临时表现上限。真实双实体 `ForceRefresh` 验证 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5`。未执行 Play Mode，故 P1 仍为 **代码/self-check/architect 通过，Play-unverified**，不可写作 P1 全部验收或 P1-P7 已完成。
- P2 immutable `BattleSpriteCatalog` 已完成代码层实施：key 为 `(ResolveCurrentDataObjectId, effectivePic)`；entry 固化 shared sheet texture、bottom-left pixel rect、UV、metrics、pivot 与 legacy `Sprite`。prewarm 以 invocation-local staging + generation/disposed gate 原子发布 configs、`MergedSprites` 和 catalog；失败、stale 与 teardown 均清理，renderer 引用计数保证旧 catalog 零引用后才退役。
- P2 已覆盖正式 partial BMP 的 declared row/col + sparse `localPic` holes、normal/swapped 完整匹配择优，以及 weapon6/weapon3 等矩阵。display、collision、anchor、SpecialAttack point-center、shadow metrics 已脱离战斗期 `Sprite.rect`；`pic=999`、missing key、identity switch 与 pool reuse 会清除旧表现引用。`MergedSprites` 只作兼容/预览。
- P2 fresh 证据为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full self-check `04:18:04` **PASS**；dotnet **0 errors**（自动生成 `.csproj` 的不同刷新视图显示 18 或 42 条既有 warnings，不冻结 warning 数）；最终 architect **PASS / no blocker**，最终 code review **no P0-P2**。未执行 Play Mode、真实异步 BMP stress 或性能验收，因此 P2 状态是 **代码+编译+self-check+静态复核完成，Play/perf/stress-unverified**。
- P3 已实现默认 `LegacyOnly` 与诊断 `CentralShadowBuild`；`CentralOnly` 明确拒绝。value-only immutable snapshot/commands 按 `(ZInt, slot)` 展开每实体 `Shadow -> Entity -> Overlay -> HitRecord`，使用 double buffer、几何容量增长和 atomic publish；persistent scratch 的 steady `RenderDispatch` self-check 为 zero allocation。早期 `AuthorityExpectedButLegacyMissing` 标记已由权威复核废止；权威 Overlay 可绘制而 Unity 未实现，P3 不宣称 overlay 等价。
- P3 actual legacy probe 读取真实 renderer 的 sprite/texture/material instance、rect/pivot/position/flip/sorting，HitRecord 在 advance 前采样。catch-up 中间 tick 明确发布 `Incomplete` count/first/last，只对最后可观测 tick 做完整 probe，不能写成全部逻辑 tick 已实际 parity verified。zero-hit 经 `SparkRenderer.RenderAll` finalize；production pool path 覆盖 nonzero spark atlas cells、age once、`OnDisable`/`OnDestroy` pool return。
- P3 与 battle checksum 隔离。fresh 证据为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full self-check `05:40:16` **PASS**；dotnet **0 errors / 18 existing warnings**；最终 architect **PASS / no blocker**，最终 code review **no P0-P2**。未执行 Play Mode、真实 SPARK BMP/设备或性能验收；未来异步 consumer 仍需 catalog lease/generation。P1-P3 代码/self-check/静态复核完成。
- P4 代码层已完成：持久 `4096`-quad/`UInt16` chunks；`OrderedChunks` 保持 `A,A,B,A` 原顺序，`StrictOrderedDraw` 为正确性回退；unresolved barrier、stale clear、跨 chunk 顺序均有 self-check。`LegacyOnly` 不 build，`CentralShadowBuild` 不提交 draw，`CentralOnly` 在全类别 ownership 前拒绝。URP 只接收 world-camera `Base` camera，并在 `AfterRenderingTransparents` 注入。
- `BattleRenderFeature` 已作为 active renderer asset 唯一 subasset 安装并验证。初审发现 feature B 覆盖 A 后注销 B 不恢复 A，现以 registration stack 修复；`A -> B -> unregister B -> restore A` 已验证 fallback material、array material 与 draw mode。
- P4 fresh 证据为 source `2026-07-21 06:32:00.287` < Unity DLL `06:32:56.970` < full self-check `06:33:43.796` **PASS**；dotnet **0 errors / 42 existing warnings**；最终 architect **PASS / no P0-P2 findings**。未执行 Play Mode、桌面像素 baseline、Profiler GC、Android/Adreno/Mali，因此 P4 为 **代码/self-check/静态复核完成，全部验收门槛未完成**。
- P5 代码/self-check/静态复核已完成：确定性 `2048` whole-sheet 多页 planner、normalized path ordinal 去重与像素冲突拒绝、`1px` extrusion、`RGBA32 Texture2DArray` 能力 gate 和有序 2D fallback 已落地。catalog 保留 legacy source 并增加 immutable central binding；manager 事务发布并持有 Unity Object ownership，renderer/central lease 共同保护退役资源。
- P5 array 路径使用 per-vertex slice，使相邻跨 slice 命令在同一 material 下保持原序合批；fallback `A/B/A` 保持三段不重排。双 shader/material/pass 与 installer 已接线。复核关闭两项 P2：同 path/同尺寸/不同 pixels 的双排列均拒绝、equal content 成功；显式两页 fallback 在 page1 失败后 page0/page1 全销毁且无 partial publication。
- P5 fresh 证据为 source `2026-07-21 07:06:28` < Unity DLL `07:07:12` < full self-check log `07:08:13` **PASS**；dotnet **0 errors / 42 existing warnings**；architect final **PASS / no P0-P2**，code review **no P0-P2**。未执行生产 BMP Play、桌面 overlap pixel baseline、Profiler/allocation stress、Android/Adreno/Mali array/fallback 或内存性能，因此不得宣称 P5 全部验收完成。
- P6 设备策略/诊断代码已完成：immutable `BattleRenderingDevicePolicy` + `FromSystem` 边界；CLI > `GameConfig` > Auto strict resolver（`-ntsdBattleAtlasMode` / `-ntsdBattleDrawMode`）；`TextureArray`/`OrderedPages` fallback 及原因；Auto/`OrderedChunks`/`StrictOrderedDraw`，且 `SingleMesh` 不入生产。resolver 输出确定性 JSON report，manager publication 一次解析，central 缓存 effective draw，tick 热路径无 `SystemInfo`/CLI。
- P6 不改变 profile/capacity/tick/collision/checksum/`CentralOnly` guard。它只是代码策略与诊断完成，Adreno/Mali、Play、pixel baseline 与 Profiler 仍未验收。
- P7 held-object 子批已完成：按 `InteractionRuntimePasses -> WeaponPointRuntime/WeaponRuntime -> SdlBattleRenderer/BattleHostForm` 权威链核对；legacy/snapshot 共用 pure held-offset helper，capture-time 固化 immutable offset 并追加到 Entity command。矩阵覆盖 right/left、target mismatch、release、missing holder/wpoints、slot generation reuse、dormant 与 legacy/central equality。
- P6/P7-held 的分项 fresh 证据为 source UTC `23:42:44` < Unity DLL `23:44:03` < `Unity-P6-P7-Final2-SelfCheck.log` `23:45:00` **PASS**；dotnet **0 errors / 18 existing warnings**；architect **PASS**，code review **approve / no P0-P2**。
- P7 Batch2 render-state semantic parity 已完成：value-only `Color32`、flipXY、mask/material semantic 和 logical resource key 已进入 snapshot/command，instance ID 仅诊断；catalog 支持 immutable `Sprite -> key[]` 与 preferred entity key。legacy probe/Compare 覆盖 RGB/alpha/flipY/unsupported/key，central resolver 转发 color 并 fail closed。
- Mesh 四顶点写 color，flipY 交换 V；color 不切 segment，semantic variant 断段。pool entity/shadow/spark checkout 归一为 white、flipXY false、mask none，首次干净 checkout 使用 `Sprites/Default.sharedMaterial`，不调用 `.material`。
- 两个中央 shader 依据 Unity `2022.3.4f1` builtin shaders ZIP changeset `35713cd46cd7` 使用 `Blend One OneMinusSrcAlpha` + final `rgb *= a`，并带 `NTSDAlphaContract` tag；installer 验证 white/tag。fresh source `08:27:50` < DLL `08:28:48` < self-check log `08:29:48` **PASS**；installer **PASS**；dotnet **0 errors**；architect/code review **PASS / no P0-P2**。
- P7 Batch3 Shadow 已完成：authority `BattleHostForm` / `SdlBattleRenderer.DrawShadow` gates；typed `EntitySprite/CommonShadow` key；immutable borrowed `GameConfig.ShadowPrefab` binding（真实 sprite/texture/UV/size/pivot/color/material）。manager main-thread atomic common publication，borrowed object 不进入 owned retirement。
- snapshot 保存 actual ObjectId/`HasCurrentFrame`；Shadow command 使用 real descriptor/`CommonShadow` 并位于 Entity 前；legacy probe exact sprite；central resolver 校验 sprite/texture/rect/pivot/material ID，并提供 source2D + fallback material。missing config/resource fail closed。矩阵覆盖 actual OID223/224、state3005/9997、`Link<0`、HitStop、missing frame。
- review 关闭 P1 missing-frame legacy/central，以及 P2 material ID、真实 `GameConfig` asset、real commit -> replace retirement tests。fresh source `09:29:03` < DLL `09:31:10` < log `09:32:07` **PASS**；dotnet **0 errors / 18 existing warnings**；architect/review **PASS / no P0-P2**。
- P7 Batch4 已完成 SPARK / Common HitRecord resource ownership 代码层收口：typed `CommonSpark(pic)` 覆盖 20 帧，prewarm 仅 decode/process 一次并在 main thread atomic publish；legacy `SparkRenderer` 不再在 `Awake` decode 或创建资源。central resolver 验证 logical key、`Sprite`、`Texture`、rect、pivot、size 和 material，publication lease/retirement 已接入。
- Batch4 错误边界已覆盖：缺失/无效 SPARK 释放 stale lease 且不改变 `HitRecord` age/count；partial `Texture`/`Sprite` 构造失败事务式 cleanup，禁止 partial publication。fresh 证据为 source `11:13:05` < DLL `11:15:20` < result `11:17:38` **PASS**，architect re-review **PASS / no P0-P2 findings**。code-review provider 返回 `429`，未取得 code-review 通过结论。
- P7 Batch5 已完成 backend-neutral immutable double-buffer HitRecord presentation cycle：`RenderDispatch` 冻结 owner handle/generation、count、age、x/z 和 common publication；`SparkRenderer` 仅 materialize/probe。`LateUpdate` 为 legacy materialize -> central `PrepareFrame` -> one finalizer，catch-up 仅 finalize 最后 cycle。
- mutation 矩阵覆盖 missing SPARK zero-write、valid age 每 cycle `+1`、invalid sampled tail 每 cycle最多删 1、`4/14/28/38` 入 gap 同 cycle 不删，以及 slot reuse/count/age guards；pool/camera/backend 不影响结果。binding direct ownership transfer 无 per-tick lease GC，no-hit 不持 binding；coordinator reset 接 world reset/driver unbind/replacement/destroy；ordered owner cursor O(N)，`1000` owners=`1000` comparisons。
- Batch5 fresh source `12:39:24` < DLL `12:40:40` < result `12:41:20` **PASS**；dotnet **0 errors / 18 existing warnings**；architect **PASS / no P0-P2**；code review **APPROVE / no P0-P2**。Play/pixel/device 仍未验收。
- Overlay authority re-audit 确认为 blocker：`BattleHostForm` 与 `SdlBattleRenderer` 都按 `Shadow -> Entity -> EntityOverlays -> HitRecords`；per-entity 内容为 `Hp2Orig > 1` 复活次数和 entity label，`WORDS0..5.bmp` glyph 为 `8x16`、步距 `9`、black colorkey。Unity `Assets` 没有 `WORDS0..5`，也缺 `BattleSlotLabels[10,12]` / state 镜像和 snapshot 字段契约。Overlay 未实现，`CentralOnly` 继续拒绝。global function/pause overlay 是独立后置 UI 且 GDI/SDL 不一致，不纳入 per-entity P7。
- P7 仍未完成：Play/pixel/device 未验收；Overlay confirmed blocker，`CentralOnly` 继续拒绝。下方旧状态只表示历史阶段；T8 已排除。

## BATTLE-RENDER-PLAN1 集中式战斗渲染系统方案交接（更新于 2026-07-20）

方案入口：[central-battle-render-system-plan.md](central-battle-render-system-plan.md)。当前状态为 **R1-R2C-4、B0、B1-B1.3、B2A 与 B2B generation-aware incremental Loose Quadtree 已完成代码层实施和既定验证**。

- **已落地**：`BattleRuntimeProfile` / `BattleRuntimeProfileResolver`；生产解析顺序为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认。平台宏只负责默认值：Editor/其他平台为 `Authority400`、Android Player 为 `MobileExtended`、Standalone Player 为 `DesktopExtended`。Unity 条件编译符号不进入战斗 pass；后续设备能力检测只允许选择或降级渲染后端。
- **已接线**：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析/创建路径；直接 `BattleTestBootstrap` 在实体注册前协调晚到的 GameConfig。`Authority400` 使用 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused`；Mobile total active admission 与 Desktop 自动分页增长已接入，Desktop 增长保留最低空洞并同步 AI snapshot。
- **fresh 验证**：相关源码 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` `12:05:07` **PASS**；100,000 次随机 claim/release/allocate 与朴素扫描模型逐步对照 **PASS**；架构复核 **PASS**。
- **R2A 已落地 / 已验证**：独立 `RuntimeSlotTable` 固定 256 槽/页并按需物化；`Authority400` 的 400 逻辑地址、`MobileExtended` 设计所需的 1050 逻辑地址及尾页 guard、每槽独立 raw runtime/rest、`ClaimedCount` 与 `(slot, generation)` 句柄契约均有 focused self-check。release、同槽 reuse 与 reset 后旧句柄均失效。
- **R2A fresh 验证**：相关源码 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` `12:36:53` **PASS**；架构复核 **PASS**。
- **R2B 已落地 / 已验证**：生产 `Authority400` registry 已由单一 `RuntimeSlotTable` 替换 used/raw runtime/raw rest 并行数组；slot 当前 occupant 为 O(1) 查询。live ascending scan 保留 high-newborn / low-reuse 时序；release 以 `expectedEntity`/当前 occupant 防止旧实体释放复用槽；stage/ordinary raw rest 语义、`ObjectCount`、buckets 与 `SceneQueryHit` slot-address 契约保持不变。
- **R2B fresh 验证**：生产源码 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` `12:57:02` **PASS**；fresh `dotnet build` **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索 **0**。
- **R2C 已落地 / 已验证**：`RuntimeSlotAllocator.GrowTo` 与 `RuntimeSlotTable.GrowTo` 只允许单调增长；增长保留 dynamic min-heap、`nextUnused`、claims、既有 pages、occupants、generation handles、raw runtime/rest，并优先复用旧低槽空洞。等容量调用为成功 no-op；缩容拒绝且原状态不变。
- **移动端地址契约修正**：`1000 active` 是 admission 预算，不是最大 slot address。保留 `0..49` 后，1000 个动态槽为 `50..1049`，故逻辑地址容量为 `1050`；`PageSize=256` 时物理需要 5 页，但 `1050..1279` 尾部地址必须不可访问、不可 claim、不可创建 raw runtime。
- **R2C fresh 验证**：相关源码 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` `13:25:34` **PASS**；fresh `dotnet build` **0 errors**；架构复核 **PASS**。
- **R2C-3A 已落地 / 已验证**：`SimulationWorld.RuntimeSlotCapacity` 读取当前 `_runtimeSlots.LogicalCapacity`；registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量循环已改为实例容量。默认 `SimulationWorld()` 仍创建 `Authority400/400`。
- **R2C-3A focused 契约**：internal `DesktopExtended/512` world 仅用于代码层验证；slot `511` 可注册、查询并进入 AI 目标扫描，slot `512` 被拒绝，reset 后高槽被清理。`BattleParitySnapshot` 继续固定 400-slot authority schema。
- **R2C-3A fresh 验证**：相关源码约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` `13:54:22` **PASS**；fresh `dotnet build` **0 errors / 42 warnings**。
- **R2C-3B 已落地 / 已验证**：`LF2SpecialAttack` 的高槽 holder 验证和 Karasu oid209 扫描读取当前 world capacity；`LF2Entity` transition effect 统计当前 dynamic range，不再固定 `50..399`。
- **parity capture guard**：历史 capture 必须同时满足 `Authority400` Profile 与 400 逻辑容量；`DesktopExtended/512`、`DesktopExtended/400` 都被拒绝，现有 400-slot schema 不能用于非 authority Profile。
- **R2C-3B fresh 验证**：相关源码 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors**，warnings 为既有告警。
- **R2C-4 Profile 优先级**：命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认。默认容量为 `Authority400=400`、`MobileExtended=1050 logical / TOTAL active admission 1000`（跨全部槽区）、`DesktopExtended=512 initial`（按 256-slot 页规范化并自动增长）。
- **R2C-4 生产接线**：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析/创建路径；直接 `BattleTestBootstrap` 在实体注册前协调晚到的 GameConfig。Desktop 增长保留最低空洞优先并同步 AI snapshot。
- **R2C-4 checksum 边界**：Extended Driver checksum 跳过/为空；direct parity capture 继续严格拒绝非 `Authority400/400`，Extended replay/checksum schema 尚未实施。
- **R2C-4 fresh 验证**：相关源码 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors / 42 existing warnings**；architect final review **PASS**。
- **B0 shadow Loose Quadtree 已落地 / 已验证**：纯数据 X/Z half-open tree，`looseness=1.5`、`leafCapacity=16`、`maxDepth=8`；每次 collision collect 全量重建，诊断默认关闭。比较 brute AABB pair、tree pair 与 accepted subset，正式 `i/j`、VRest、RNG、candidate flow 不变。
- **B0 fresh 验证**：相关源码不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` `16:15:43` **PASS**；fresh `dotnet build` **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。
- **B1 `RuntimeRestStore` 已落地 / 已验证**：分页/惰性 ARest；定向稀疏 `VRest[victim, attacker]` 只存正值、写零移除；`ResetSlot` 清 ARest + victim row + attacker column；支持 `GrowTo`、全局 reset、排序 diagnostics/snapshot 与 restore。
- **B1 differential / fresh 验证**：2,000 次随机操作与 dense reference model 逐步对照 PASS；源码 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` `16:37:13` **PASS**；fresh `dotnet build` **0 errors**；architect final review **PASS**。
- **B1.1 optional facade 已落地 / 已验证**：`LF2ItrRestTracker` 可选绑定 `RuntimeRestStore`；exclusive victim-row lease 保证同一 victim row 同时只有一个 facade owner，释放后才允许接管。B1.1 阶段未 production-bound，后续已由 B1.2 接入。
- **B1.1 architect 首轮修正**：`ReplaceVictimState` 对 mixed-invalid attacker 输入原可能部分写入；现已先完整预验证再原子替换。direct `ReplaceVictimState` 与 facade `Bind` 均补 failed-import 原状态不变测试。
- **B1.1 修正后 fresh 验证**：复跑 `dotnet build` **0 errors / 18 existing warnings**；源码 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` `17:39:07` **PASS**；architect final review **PASS / no blocker**。
- **B1.1 非阻塞补强**：invalid bound `RestoreState` 可后续补独立断言；该路径复用已验证 atomic replace 入口，不影响当前结论。
- **B1.2 lifecycle binding**：`SimulationWorld` owns store；ordinary claim、release、`StageSpawnAt`、world reset/grow 与 parity fallback 已接入，`RuntimeSlotTable.RawRest` 已删除。
- **B1.2 三轮审查归属**：初轮发现 Stage pool 回收不完整与错槽 release 未拒绝，次轮发现 release 拒绝未传播，末轮 PASS/no blocker；partial import 属于 B1.1，不计入 B1.2。
- **B1.2 三个 architect blockers 已修 / self-check verified**：Stage rejected bind 走共享完整 pool 回收；错槽 release 被拒绝；`ReleaseRuntimeSlot` bool 事务传播到全部注销/待销毁调用链，拒绝时不再半注销。
- **B1.2 旧 fresh 证据**：`dotnet build` **0 errors**；源码 `18:11:41` < DLL `18:12:23` < self-check `18:13:00` **PASS**。该证据早于 blocker 修复，只证明初版可编译/旧断言通过。
- **B1.2 第一轮修复证据**：源码 `18:21:20` < DLL `18:21:58` < self-check `18:22:59` **PASS**；第二轮审查随后发现半注销 blocker，因此仍是非完成证据。
- **B1.2 最新 fresh 证据**：`dotnet build` **0 errors**；源码 `18:31:25` < DLL `18:33:58` < self-check `18:34:54` **PASS**。公开 `Unregister` 故障测试验证 bucket/slot/lease/store/entity 完整注册上下文不变；architect final review **PASS / no blocker**。
- **B1.3 tick 解耦已实现 / self-check verified**：`CaptureSnapshots -> sparse Tick -> Collect`；eligible active+CharData row 递减，inactive row 冻结；`BruteForceSceneQuery` 已删除 pair 内 tick。
- **B1.3 初版非完成证据**：源码 `19:09:44` < DLL `19:10:34` < self-check `19:11:13` **PASS**；architect 随后发现 eligibility 仍按 capacity 全扫。
- **B1.3 sparse 修复**：eligibility 直接遍历 registered bucket items，无 capacity scan/snapshot 分配；Desktop sparse high-slot `visited=2`。active-positive-row/stamp + scratch 预扩继续保持。
- **B1.3 最终 fresh 证据**：`dotnet build` **0 errors**；源码 `19:19:14` < DLL `19:19:47` < self-check `19:22:50` **PASS**；architect final review **PASS / no blocker**。
- **B2A 后端选择**：新增独立 `BruteForce` / `LooseQuadtree` formal backend；命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`，不按平台分叉战斗规则。
- **B2A 固定帧边界（历史）**：B2A 当时仅接管 fixed-tick candidate collect，保持 `CaptureCollisionFrameSnapshots -> TickCollisionPairVRest -> CollectCollisionCandidates`；即时 weapon/body current-world query 已由后续 B2C 在显式 `LooseQuadtree` 后端下接入，并保留 brute fallback。
- **B2A pair/回退契约**：eligible participant 保留 authority ordinal；tree pair 与 invalid-AABB fallback-all pair 使用 canonical slot key 合并、排序、去重，再按原 ordinal 双向派发。slot/mapping/index/count 异常、rebuild/query exception 或 diagnostics 缺 brute coverage 时整 tick brute fallback；formal 失败会恢复 RNG 并清除 candidate 中间态，保证 candidate 20 上限、tie、RNG 与消费顺序不被部分执行污染。
- **B2A fresh 证据**：源码 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` `22:19:28` **PASS**；`dotnet build` **0 errors**；architect final review **PASS / no blocker**。
- **B2B 同步与身份**：formal backend 在 collision collect 边界 batch synchronize 当帧 participant；索引键改为 `(runtime slot, generation)` handle。同槽复用不会继承旧 occupant 的空间身份，query 结果必须经当前槽表 generation 解析并核对 entity/ordinal。
- **B2B 增量策略**：未移动实体保持原记录；AABB 改变但仍在当前节点 loose 范围内时原位更新，越出 loose 范围时迁移。spawn/remove、invalid AABB 与同槽 reuse 均在下一 collect 收口；root escape 才 full rebuild。
- **B2B 回退/lifecycle**：sync/query/invariant/mapping 异常会 reset 索引并整 tick brute fallback，继续使用 B2A 的 RNG/candidate rollback；world reset 显式清空 formal index。
- **B2B fresh 证据**：源码 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` `22:47:04` **PASS**；`dotnet build` **0 errors**；architect final review **PASS / no blocker**。
- **后续边界（B2B 历史状态，已由 B2C 部分替代）**：生产默认仍为 `BruteForce`；即时 weapon/body 与 AI 查询、Extended checksum 已由 B2C 接入。Extended replay、Loose Quadtree 默认启用证据与完整渲染仍未完成。B2C 未执行 Play Mode 或性能验收；T8 已排除，默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT14 DAT movement 显式值读取回归交接（2026-07-19）

- **最新覆盖结论**：下方 BATTLE-AUDIT13 的“可玩 Naruto `oid2 running_speed=8`”与“`BattleVisualScale=1`”已经失效。生产 Naruto DAT 显式配置为 `running_speed=15`；Unity 实体表现缩放已恢复为项目要求的 `1.5`。
- **回归根因**：DATA-01A 把 `LF2CharacterData` 的兜底值从旧 `15` 改为 C# 权威默认 `8` 本身正确，但 Unity parser 未读取 `<bmp_begin>` 内无冒号的 movement `key value`，使生产显式 `15` 回退到 `8`。这是 Unity loader bug 和对齐回归；此前将慢速主要归因于缩放并不完整。
- **生产修复**：`Lf2DatParserV2` 仅对白名单中的 BMP 顶层 18 个 movement 键接受无冒号 `key value`；`ExtractMovementParameters` 现读取 `Bmp.Properties`；浮点字段和 `frame_rate` 均以 `InvariantCulture` 解析。DAT 缺字段时仍保留 C# 默认 `8`，没有恢复错误的 Unity 默认 `15`。
- **测试矩阵**：生产 DAT 覆盖 Naruto `15`、Kakashi `18`、Sakura `17`、Sasuke `23.9`、clone `15`，并保留 weapon4 冒号语法 guard；synthetic 覆盖全部 18 键、last-wins、frame 隔离与缺省 `8`。
- **同类风险审计**：已审计当前 101 份 DAT。除上述 5 份角色 DAT 的 18 个 movement 字段外，没有第二组当前生产数据触发同类遗漏；weapon/frame/stage/data 当前安全。多词 `name` 是非战斗的潜在表示风险；`catchingact/caughtact` 双值为未来风险，但当前 218 处两值均相等，当前无可观察战斗差异。
- **fresh 验证**：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 14:39:43.992`，晚于相关源码，Console C# error 为 **0**。一次请求因 Editor 误留 Play Mode 未计入结果；退出后 fresh full `BattleRuntimeSelfCheck` 于 `14:44:58.748` 返回 **PASS**。
- **未验收边界**：真实双击 D Play trace 因 UnityMCP 临时注入卡住而未完成，本轮不宣称 Play Mode 通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT13 Naruto 防下攻与跑速缩放交接（2026-07-19）

- 常规战斗逻辑的唯一权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。本项是用户明确指定的例外：Naruto 防下攻以用户已验证表现正确的 `J:\QQFile\NTSD2.4\ntsd_release` C++ 版本作定向参考；该例外不扩展到其他战斗逻辑。
- C++ 行为依据：`oid2 frame286` 的 `centery=79`，opoint 为 `y=80 action=240 dvy=0 oid=33`，child 初始为 `Y=+1, Vy=0`。角色物理落地要求 `new_y > 0.0001 && pre-move Vy > 0.0001`，所以不会立即进入 frame219；后续链为 `240 -> 241 -> 242 -> 243 -> 235 -> 236(dvy=-7) -> 244..247`，真实下降落地后才进入 `219 / AI`。
- Unity 根因与修复：`CharacterMechanics` 的 `landed` 判定缺少 `Vy` 门槛；旧 `LateOpoint + state15` 专项 gate 过宽，并且仍会把 `Y` 钳为 0。现已改为通用 `landed` 条件并移除专项 gate；`CheckLateOpointState15LandingControls` 与 `PH-02` 三向速度矩阵已同步更新。
- 跑速测试状态：按用户要求，`BattleVisualScale` 临时由 `1.5` 改为 `1`，供用户复测奔跑速度体感。可玩 Naruto `oid2` 的逻辑 `running_speed` 仍为 `8`，固定逻辑频率仍为 30 Hz，本轮没有修改逻辑跑速。

fresh 验证链：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 03:21:41.985`，晚于测试时间 `03:20:06.169`；Console C# error 为 **0**；fresh full `BattleRuntimeSelfCheck` result 时间 `03:22:49.668`，结果 **PASS**。本轮没有可复用的真实 Play 自动 trace 入口，因此没有重新运行真实 Play trace；防下攻与 scale 1 奔跑仍需用户手测，当前不宣称 Play Mode 验收通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT12 代码差异修复与 fresh 验证（2026-07-18）

本段是当前交接状态，并覆盖下方 BATTLE-AUDIT9/10/11 的历史冻结措辞。用户负责 4 组 Naruto/武器 Play Mode 场景，本轮不运行也不代替其验收。最新 freshness：相关源码最晚 `BattleRuntimeSelfCheck.cs` `16:44:31.210` < Unity `Assembly-CSharp.dll` `16:45:52.868` < self-check result `16:46:29.080` **PASS**；fresh `dotnet build` 为 **0 errors / 18 warnings**。

- `FW-FLOW-01`：已恢复普通 tick 的 cooldown-before-human-input 顺序，focused check 与 full self-check 通过。
- `LP-03`：typed/generic formal throw 已移除 `Zz=1` 额外层级，release 矩阵通过。
- `LP-05`：formal release、consume、force-clear 的 `TargetIdx/HolderIdx/HeldWeaponSlot/HolderCopy` 写入边界已按 authority 分开，typed/generic 矩阵通过。
- `FW-RESULT-01`：固定 roster slot、inactive/dormant 与 alive/team bucket 矩阵已补齐并通过。
- `UNRES.04`、`DATA-01A-D`：生产修复和对应断言均进入本次 fresh full PASS；此前 transformed landing 阻塞已由 authored-frame gate 修复消除。
- `FW-FLOW-02`：Unity 生产无 writer，authority 仅 Host debug/step 控制，归 dormant/scope-excluded。
- `FW-BOOT-01/02`：旧表误把 rematch-only 写入及普通 reset 后偶合等价字段记成正式差异；普通非-rematch 路径关闭为 equivalent，result rematch 保持 scope-excluded。
- `FW-RESET-01/DEP.RNG.01`：保留 per-world lockstep RNG adapter；算法等价，不迁移为进程静态 owner。

当前 code-only 清单没有未修复的 confirmed item；但这只关闭脚本差异与 self-check 层，不是 Play Mode 结论，也不是完整逐帧 production certificate。T8 默认 `stage.dat` 部署继续暂缓，raw DAT 表示差异继续排除。

## BATTLE-AUDIT11 代码层 12 项待确认项已全部定性（2026-07-18）

本轮只核验脚本/代码层，不进行 Play Mode、资源部署、DAT 文件表示或场景/表现确认；核验后的 Unity 代码修复已落地，但最新 fresh Unity full self-check 仍为 **FAIL**。2026-07-18 最新 fresh run 的 `CheckStateTransformLandingMatrix` transformed landing fixture 断言失败，实际为 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`；这是既有代码契约回归，不是 Play Mode 结论。最终依据为：

- `.omc/research/final-verify-unres-02-05-code-parity-20260718.md`
- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

分类汇总：

- **equivalent / Unity-adapter**：`UNRES.01/02/03/05`、`DEP.INT.01-04`、`DEP.WORLD.01`。
- **confirmed code difference**：`UNRES.04`、`DATA-01A`（`running_speed` 默认值）、`DATA-01B`（frame index 容量）、`DATA-01C`（合法缺帧语义）、`DATA-01D`（cpoint front/back action alias）。首轮修复已落地，但 fresh full self-check 仍被 transformed landing fixture 回归阻塞。
- **Unity-adapter / policy-open**：`DEP.RNG.01`（LCG 算法等价；owner/reset 边界保留为 Unity lockstep 策略待定）。
- **关联确认代码差异**：`FW-RESULT-01`（非正常 roster/lifecycle 下 dormant/inactive 选择与 relation identity alias）。
- `DATA-01E` 为当前 consumer 已屏蔽的 adapter/masked，`DATA-01F` 为 schema-only omission，`DATA-01G` 已在源码闭合，不计为正式 runtime 差异。

在本轮 **code-only scope** 下，原先剩余的 4 个 `authority-unresolved`（`UNRES.02`-`UNRES.05`）现已全部定性，数量为 **0**。这不是修复完成声明：`FW-RESULT-01` 仍是确认差异，且 `UNRES.04`/`DATA-01A-D` 的 fresh full self-check 被 transformed landing fixture 回归阻塞；4 组 Play Mode 场景仍由用户自行验证，本轮不改变 LP 或 Play 验证状态。

## BATTLE-AUDIT10 代码核验结果（2026-07-18）

本轮只处理代码层面的待确认项，不进行 Play Mode、资源部署或场景/表现验证，也未修改任何生产代码。核验报告：

- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

结论与交接状态：

- 已闭合为 **equivalent / Unity-adapter**：`UNRES.01`、`UNRES.02`、`UNRES.03`、`UNRES.05`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01`。
- 已升级为 **confirmed code difference**：`UNRES.04`、`DATA-01A`/`DATA-01B`/`DATA-01C`/`DATA-01D`（DAT parser/runtime contract）；首轮修复已落地，但最新 full self-check 仍被 `CheckStateTransformLandingMatrix` transformed landing fixture 回归阻塞（实际 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`）。
- `DEP.RNG.01` 为 **Unity-adapter / policy-open**（算法等价，owner/reset 边界待策略决定）；`FW-RESULT-01` 仍为未修复的确认差异（非正常 roster/lifecycle 的结果 slot/relation identity）。
- `DATA-01E` 为 **Unity-adapter / masked**，`DATA-01F` 为 **schema-only omission**，`DATA-01G` 为 **closed in source**，不作为正式 runtime 差异计数。
- **BATTLE-AUDIT10 历史中间快照**曾保持 `UNRES.02`-`UNRES.05` 为 authority-unresolved；该状态已被 BATTLE-AUDIT11 取代，当前 code-only scope 下 02/03/05 为 equivalent，04 为 confirmed difference。

**本段为 BATTLE-AUDIT10 历史中间快照，已被 BATTLE-AUDIT11 取代。** 当时统计为剩余 authority-unresolved 4 项（`UNRES.02`-`UNRES.05`）；当前 code-only scope 下已全部定性为 0。BATTLE-AUDIT9 的 LP 项状态和计数保持不变，4 组 Play Mode 场景仍由用户自行验证，本轮不对其下结论。以上仅是代码核验，不是完整战斗逻辑对齐声明。

## BATTLE-AUDIT9 差异盘点冻结（2026-07-18）

当前执行口径已切换为“先完成差异盘点，再按文档集中修复”。本轮只读合并以下报告，**没有按冻结清单修改生产代码**：

- `.omc/research/full-diff-inventory-framework-20260718.md`
- `.omc/research/full-diff-inventory-input-interaction-20260718.md`
- `.omc/research/full-diff-inventory-lifecycle-presentation-20260718.md`
- `.omc/research/reaudit-open-differences-20260718.md`

冻结计数（**BATTLE-AUDIT9 历史快照，已由 BATTLE-AUDIT11 取代**）：9 个正式 runtime 差异、1 个工具/trace 差异、12 个 authority-unresolved 待确认项、4 个 Play Mode 未验证场景。原 12 项现已在 code-only scope 下全部定性；正式差异表保留作历史追踪。

| ID | 权威 C# | Unity 对应 | 触发与预期/实际 | 证据/分类 |
|---|---|---|---|---|
| `FW-FLOW-01` | `BattleCore/Simulation/GameTick.cs:53-67` cooldown/step gate 在 input 前 | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:32-43`、`RunFrameAdvancePhase` | ARest/AttackExempt 与输入边沿同 tick 到期；应先递减再读，Unity 先读 | 静态 confirmed-difference，未修复 |
| `FW-FLOW-02` | `GameTick.cs:56-67` `BattleStepGate44905C` mode=2 转换与抑制 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:272-281`、`NTSDBattleTickSystem.RunReleaseTick` | 单步/慢速模式；应 gate，Unity 无转换/抑制 | 静态 confirmed-difference，生产可达性待确认 |
| `FW-BOOT-01` | `DirectBattleBootstrap.cs:138-140` 写 `Unk344`/`HolderCopy` | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 未显式写两字段 | 初始玩家统计/holder 分支；应 team/slot，实际可能 `0/99` | 静态 confirmed-difference，未修复 |
| `FW-BOOT-02` | `DirectBattleBootstrap.InitializeBattleStats:224-244` 完整 difficulty/HP/PP/respawn/Cd/edge | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 依赖隐式初始化 | 非默认 difficulty、DAT Hp3、复用；应完整字段契约，实际缺显式写入 | 静态 confirmed-difference，未修复 |
| `FW-RESET-01` | `SimulationWorld.Passes.cs:13-70` reset 不播 RNG | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:138-151` reset 播 `0x4E545344` 后再播 config seed | 连续重开/重赛随机序列；应遵循权威播种边界，实际 Unity 增加边界 | 静态 confirmed-difference，播种归属待确认 |
| `LP-01` | `BattleCore/Interaction/WeaponRuntime.cs:169-212,287-303` generic held throw/kind3 写 `ReleaseTick` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` generic throw/kind3 通过 `ClearLinks(..., stampReleaseTick: true)` 写当前 tick | generic DAT held 正式释放；应清 link并写当前 tick | confirmed-difference；**代码已写 / `CheckAudit9GenericHeldReleaseTickContracts` self-check verified / Play-unverified** |
| `LP-02` | `src/Host/SdlBattleRenderer.cs:476-497` 同 Z 按 slot 稳定排序 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` compact presentation sort、`Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` `ForceRefresh`；`LF2Sprite.cs` 表现刷新 | 同 Z 实体；按 `(ZInt, runtime slot)` dense rank，四槽中实际使用 `Shadow/Entity/HitRecord=0/1/3`；同层统一 `Object` sorting layer。真实双实体 renderer 检查为 `0/1/4/5` | confirmed-difference；**代码/self-check/architect verified / Play-unverified** |
| `LP-03` | `BattleCore/Interaction/WeaponRuntime.cs:169-212` 释放不写额外 Zz | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:77-98,391-402` 写 `Zz=1` | 正式投掷；应由 Z/slot 决定，实际额外抬层 | 静态 confirmed-difference，未 Play |
| `LP-04` | `src/Host/SdlBattleRenderer.cs:519-548` 实体/阴影按负 HitStop 阈值与四拍相位隐藏 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:416-448`、`LF2ObjectRenderer.cs:206-243` 已接入 gate | 负 HitStop 闪烁区间；应按实体/阴影各自阈值与四拍相位隐藏 | confirmed-difference；**代码已写 / `CheckHitStopPresentationGates` self-check verified / Play-unverified** |

### BATTLE-AUDIT9 修复进度（LP-01 / LP-04）

Fresh verification: `Assembly-CSharp.dll` `2026-07-18 14:01:27.540`; full `BattleRuntimeSelfCheck` `2026-07-18 14:01:51.078` returned **PASS**.

冻结后仅 `LP-01`、`LP-04` 更新为“**代码已写 / self-check verified / Play-unverified**”，其余冻结状态和计数不变，整个差异清单仍保持开放。`LP-01` 已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` 的 `ThrowHeldObject`、`DropRandomly`、`ClearLinks(..., stampReleaseTick: true)` 补齐 generic held `ReleaseTick`，由 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs:3062` `CheckAudit9GenericHeldReleaseTickContracts` 覆盖；`LP-04` 已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs:206-243` 的 `UpdateSprite`、`ShouldDrawEntityForHitStop`、`ShouldDrawShadowForHitStop` 接入表现门控，由 `BattleRuntimeSelfCheck.cs:1394` `CheckHitStopPresentationGates` 覆盖。

验证证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 warnings**；`Assembly-CSharp.dll` `13:43:59.791` 晚于本轮最新相关源码，fresh Unity full `BattleRuntimeSelfCheck` 于 `2026-07-18 13:44:26.093` 返回 **PASS**。两项仍需 Play Mode 定向验证：generic held 的实际投掷/掉落，以及负 `HitStop` 下实体与阴影的阈值隐藏和四拍闪烁。

`LP-05`（新增 reviewer 候选，只记录、不修复）：权威 `BattleCore/Interaction/WeaponRuntime.cs:289-295` `ReleaseHeldWeaponRuntime` 不清 `holder.TargetIdx`/`held.HolderIdx`；Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs:23-28,39-59` 正式 release 当前清 holder `TargetSlotIndex` 与 held `HolderStableId=-1`，generic `ClearLinks` 也有同类清理。分类保持 **confirmed-candidate / 未修复 / 需 authority 调用链与 Play Mode 复核**，不纳入 `LP-01` 已写结论，也不改变冻结计数。

`RT.CHECK.01` 是 `CharacterSync.cs:796-877,173-317` 内部 snapshot 与 `BattleParitySnapshot.cs:385-542` trace projection 的 schema/alias 差异，分类为 **validator adapter**，不是 runtime 语义差异（见 `reaudit-open-differences-20260718.md:44-56`）。12 个 unresolved 只保持待确认，不得猜测为等价。四个 Play 未验证场景的详细输入与预期见主文档 BATTLE-AUDIT9 详细冻结表：Naruto 防下跳六分身、防前跳螺旋丸、奔跑防跳后续招、投掷武器首击/持续命中。

F1-F7 仅 static/focused self-check 闭合，不能替代 Play Mode；DAT 表示差异不处理，T8 默认 `stage.dat` 部署继续暂缓，fixed-world camera 为批准的 Unity adapter。修复阶段必须从本冻结表开始，逐项取得编译、self-check 和必要 Play Mode 证据后再更新状态。

## BATTLE-AUDIT8 当前交接（2026-07-18，继续开放）

- fresh Unity full `BattleRuntimeSelfCheck` 已于 `2026-07-18 12:46:40.638` 返回 **PASS**；freshness 为 test source `12:45:10.110` < `Assembly-CSharp.dll` `12:46:15.927` < result `12:46:40.638`。
- F6/R1 的生产修复位于 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`：`UpdateLocalInputStateFromControllerBuffer` 先 `SyncFromRuntime`，再轮询 controller buffer；权威对照为 `BattleCore/Input/InputRuntime.cs` 的 human poll/cooldown runtime 真值及 `BattleCore/Simulation/GameTick.cs` 的 results early return。
- `BattleRuntimeSelfCheck.cs` 的 `TemporaryAppManagerRuntimeScope` 只修复 EditMode 下测试 fixture 的 AppManager singleton/Awake 生命周期；生产 `Assets/NTSD/Scripts/App/AppManager.cs` **未修改**。
- Frame/Input 已完成 **237/237** 分类：39 equivalent、181 Unity-adapter、4 confirmed-difference、1 missing、12 authority-unresolved。完整账见 `.omc/research/unity-frame-input-mapping-complete-20260718.md`；`FLOW.05`、`FLOW.09`、`IN.CD.02`、`RT.CHECK.01`、`RT.LINKS.01 / ReleaseTick` 及 12 个 unresolved 在按最新生产代码重新核销前仍开放。
- 该 PASS 只覆盖当前 self-check，不等于全战斗逐帧最终完成；后续仍需静态重审、必要 Play Mode/双端 trace 和最终独立复核。DAT 表示差异不处理；T8 默认 `stage.dat` 部署继续暂缓。

> 生成：2026-07-13 ｜ 供 Codex 或任何接手者直接开工，无需追溯历史会话。
>
> **当前状态（BATTLE-AUDIT7，2026-07-18）**：旧的“完整对齐/无剩余差异”推断已撤销。重新按唯一权威 C# 做完整框架正向映射和 Unity-only 反向审计后，确认 **13 个去重开放根因**：**12 个战斗 runtime/语义差异 + 1 个 trace 投影工具差异**，均为“已确认 / 未修复 / 未运行时验证”。Audit5 的 74/74 与原 15/15 仅表示历史批次已关闭；旧 `01:07:52.834 PASS` 和 Architect `P0/P1/P2=0` 不覆盖本轮新发现。

## 0. 你要做什么

把 **NTSD C# 战斗核心** 里 Unity 尚未对齐的战斗逻辑，逐条补齐 / 修正到 Unity 工程。
T0-T9、Audit2、Audit3、Audit4、Audit5 和 Audit6 只保留为历史实现/定向回归基线。Audit5 的 **74/74** 与原 trace 风险 **15/15** 仍是对应历史批次已关闭的记录，但不能覆盖 BATTLE-AUDIT7 的 13 个新开放根因，也不能作为当前完整对齐证明。C# 与 Unity 的 raw DAT/manifest 差异属于 Unity 适配预期，不是待处理项；T8 默认 `stage.dat` 继续暂缓。

- **唯一 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release_C#`；核心战斗入口位于 `src\BattleCore`。旧工程、反汇编及历史对齐结论不得作为当前实现或验收依据。
- **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
- **完整差异清单（配套读）**：`Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
  - 本文是「行动版」，那份是「全量核实版」。当前状态优先回查其顶部 BATTLE-AUDIT7 章节。

## BATTLE-AUDIT7 全量权威映射交接（2026-07-18，开放）

### 覆盖与结论

- Framework：权威 **172/172 ID** 已映射；独立复核为 13 difference ID，去重为 **7 个 framework 根因**。反向 Unity-only 扫描没有发现这 7 项之外的新 framework 根因。
- Frame/input/physics/runtime：**[历史 BA7 快照，已由 BATTLE-AUDIT8 237/237 分类取代；当前开放项见顶部]**：旧记录曾记权威 **237/237 ID** 集合定位、4 difference ID + 1 missing ID，并注明其余 219 ID 尚未逐项拆分；该旧静态边界不再作为当前状态依据。
- Interaction：权威 **105/105 ID** 集合相等，但独立复核确认 **2 个正式可达差异**；原 0 difference 结论失效。
- 总账：framework 7 + frame/input 新增 3（Results 去重）+ interaction 2 = **12 个战斗 runtime/语义根因**；另有 **1 个 trace 工具根因**，合计 **13**。
- Frame/input 权威 ledger 有两处账本校正：字段组机械相加为 138，而 footer 写 137；`IN.JUMP.03` 曾误写权威成功 jump 清 8 Cd，实际权威与 Unity 都只清 7 个普通 Cd并保留 `CdDefendLock`，因此该 ID 为 equivalent。两者都不是 Unity 差异；`IN.CD.02` 的 AI 递减 ownership 根因仍成立，所以 13 个去重根因不变。

### 13 项开放根因

| 组 | 根因 | 关联 ID | 状态 |
|---|---|---|---|
| Framework | bootstrap 把 `WaveIdx -1 -> 0` | `FW-BS-008`,`FW-LC-004` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 8-slot roster 压缩且 independent team 未规范化 | `FW-BS-008-B1` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 初始出生 X/Z 与 RNG 消耗改用 scene transform | `FW-BS-008-B2` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 初始 `HitStop=75,Vx=Vz=0.1` prime 缺失 | `FW-BS-009` | 已确认 / 未修复 / 未运行时验证 |
| Framework | stage spawn 经通用 Register 误清复用槽 ARest/VRest | `FW-WR-005`,`FW-TK-028`,`FW-H-050`,`FW-H-059`,`FW-LC-004` | 已确认 / 未修复 / 未运行时验证 |
| Framework | Results active 后仍执行普通战斗 pass | `FW-TK-002`,`FW-END-002`,`FLOW.05` | 已确认 / 未修复 / 未运行时验证 |
| Framework | `HitConfirm2` 等 candidate carrier 到下一次 collect 才清 | `FW-TK-034`,`FW-H-042` | 已确认 / 未修复 / 未运行时验证 |
| Frame/input | `CdDefendLock` 错对 AI 递减；成功 jump 双方均清7个普通Cd并保留lock，不是差异 | `IN.CD.02`；`IN.JUMP.03` 已移出差异账 | ownership 已确认 / 未修复 / 未运行时验证 |
| Frame/input | late holder 改帧后再次写 held Frame/位置 | `FLOW.09` | 已确认 / 未修复 / 未运行时验证 |
| Frame/input | `ReleaseTick` storage/writer/hash 缺失 | `RT.LINKS.01` | 已确认 / 未修复 / 未运行时验证 |
| Interaction | IronBall type2 的 dvx/dvy 预处理 gate 错落到 type6 | `INT-HIT-005` | 已确认 / 未修复 / 未运行时验证 |
| Interaction | late opoint child X/Y 使用浮点 `PS`，未按 spawner `XInt/YInt` | `INT-OP-001`,`INT-OP-002` | 已确认 / 未修复 / 未运行时验证 |
| Trace 工具 | `BattleParitySnapshot` 对空槽/category、release、block、transform/weapon/owner 等字段硬编码或错映射 | `RT.CHECK.01` | 已确认 / 未修复 / 未运行时验证 |

每项的权威方法、Unity 对应、可复现前置、预期/实际和依赖见完整差异清单的 BATTLE-AUDIT7 总表。DAT 文件适配不处理；T8 默认 `stage.dat` 部署暂缓，stage runtime 用内存 fixture；fixed-world camera 和不改变逻辑结果的 Unity-native 适配保持。

### 行动顺序

1. 先修 tick/runtime 契约：Results early return、`CdDefendLock`、late held、`ReleaseTick`、candidate carrier；同步补 focused self-check。
2. 修 interaction：IronBall type gate、late opoint 整数 X/Y；覆盖 real/shared、正负和跨零坐标。
3. 修 bootstrap/stage：WaveIdx、8-slot/team、spawn RNG、HitStop/velocity prime、stage rest policy；全部使用内存 fixture，不部署默认 `stage.dat`。
4. **历史行动项（已由 BATTLE-AUDIT8 取代）**：修 trace snapshot 投影，并完成剩余 219 个 frame/input ID 的逐项 equivalent/adapter 分类和反向 Unity-only 零未分类核销；237/237 分类现已完成，但 trace snapshot 等开放差异仍需按最新生产代码重新核销。
5. 最后跑 fresh Unity 编译、full `BattleRuntimeSelfCheck`、normal + hole/independent roster Play Mode、held/opoint/结果态定向场景，再做独立 Architect 复核。证据齐全前不得宣称完整战斗逻辑对齐。

---

## Audit5 全量逐帧审计交接（2026-07-18，风险账已收口）

### 权威与历史结论

- 唯一战斗逻辑权威是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。所有差异定性、修复方向和预期 trace 都必须从该工程的正式 C# 调用链闭合。
- 旧章节中依赖其他来源得出的“已对齐”“已关闭”或“仅作映射参考”结论，在 Audit5 当前状态下全部废止为权威证据。它们只能保留为历史回归基线；未经 C# 重审、fresh 验证和双端 trace，不得恢复完成状态。
- T8 默认 `stage.dat` 部署继续暂缓；默认 trace 使用 `stageFixture=false`，不读取或生成默认资产。

### 当前总账

| 分区 | 报告结果 | 当前实现与 fresh 证据 | 风险账状态 |
|---|---|---|---|
| GameTick / Physics | 21 确认 + 3 风险；正式主干和 Physics 全分支 100% 审计 | `GT-01..15`、`PH-01..06` 共 **21/21 逻辑已写并进入 fresh full PASS** | `R-GP-01..03` **3/3 关闭** |
| HitResolve / CollisionCollect | 33 确认 + 6 风险；两个权威入口全分支审计 | `C-01..33` 共 **33/33 逻辑已写并进入 fresh full PASS** | `R-HC-01..06` **6/6 关闭** |
| Frame / lifecycle | 20 确认 + 6 风险；25/25 方法及 reset/registry/rest 依赖审计 | `FL-01..06`、`FT-01..04`、`OP-01..05`、`LC-01..05` 共 **20/20 生产实现与 focused/full self-check 通过** | `R-FL-01..03`、`R-LC-01..02`、`R-FT-01` **6/6 关闭** |

跨分区原始确认项现为 **74/74 逻辑实现 + focused/full self-check**，原 15 项风险为 **15/15 已关闭**。`BATTLE-AUDIT6-01/02` 是原总账后新增且已关闭；CP-NV1/2/3 与 STEP10 是既有项重开后重新关闭。原 3 个受控 P2 已补强并关闭；最终 freshness 链为 source `2026-07-18 01:06:21.499` < Unity DLL `01:07:21.125` < result `01:07:52.834` **PASS**，Architect `P0/P1/P2=0`。这仍不是任意对局、全输入 production trace certificate。

原 3 个受控 P2 的关闭证据：HC-04 已覆盖真实 step6 `collect -> wrong loop 不消费 -> post consumer 消费` 整链及 current type3 负例；missing-definition 已覆盖 Character/Weapon 的完整错误循环、正确循环与 tail；`LF2CharacterInteractionResolver` 的本地类型 helper 仅单行委托中央 `LF2Entity.ResolveCurrentDataObjectType`，不再存在第二份类型判定维护漂移。

### 原 15 项 trace 风险关闭状态

| 分区 | 风险 | 状态 |
|---|---|---|
| GameTick / Physics | `R-GP-01` | ✅ fresh 2 tick trace 关闭；tick1 slot0=`frame0/wait37/FWC11/HitStop75`，tick2=`frame5/wait37/FWC0/HitStop74`，双方一致 |
| GameTick / Physics | `R-GP-02` | ✅ production `mass > 0` 扫描，static close |
| GameTick / Physics | `R-GP-03` | ✅ central active filter 关闭 |
| Hit / Collision | `R-HC-01` | ✅ 确认差异并修复 zero-width strict overlap；90 项已知非正宽 geometry 纳入权威等价覆盖 |
| Hit / Collision | `R-HC-02` | ✅ oid999 `next` 闭包 14 帧均为零有效 geometry |
| Hit / Collision | `R-HC-03` | ✅ current OID/type 统一与 gate A/B 覆盖 |
| Hit / Collision | `R-HC-04` | ✅ current-DAT pickup 去除 CLR cast；真实 step6 collect、错误循环不消费、post consumer 消费及 current type3 负例均已覆盖 |
| Hit / Collision | `R-HC-05` | ✅ fixed slot/reuse 关闭 |
| Hit / Collision | `R-HC-06` | ✅ 整数路径关闭 |
| Frame / lifecycle | `R-FL-01` | ✅ 四 weapon 矩阵关闭 |
| Frame / lifecycle | `R-FL-02` | ✅ current-DAT boomerang 关闭 |
| Frame / lifecycle | `R-FL-03` | ✅ raw empty slot `CatchTimer`、占槽清理与 reset 关闭 |
| Frame / lifecycle | `R-LC-01` | ✅ pooled snapshot/cache reset 关闭 |
| Frame / lifecycle | `R-LC-02` | ✅ StableId alias/reuse 关闭 |
| Frame / lifecycle | `R-FT-01` | ✅ 已关闭 FT-01 的 trace 验证债；不是重复风险 |

R-GP-01 freshness：authority source `00:11:23` < DLL `00:11:49` < trace `00:12:07`；Unity source `00:11:23` < Editor DLL `00:12:22` < trace `00:13:44`；compare `00:14:02` 为 `equal-diagnostic`、2 ticks、`firstDifference=null`。它关闭 R-GP-01，但不构成任意对局证书。

最终 PASS 前的失败和收口不可省略：`C-05`、`BATTLE-AUDIT3-12`、state8000/type6 fixture、`C-12`、`GT-04/GT-07/PH-02` 与 Weapon C-26/C-27 均已按权威收口；此前 `18:16:36.721 PASS` 与 `21:57:40.670 PASS` 只保留为过期历史证据，当前统一以 `01:07:52.834 PASS` 和 combined Architect `P0/P1/P2=0` 为准。该结论仍不能替代逐 tick 或目标 Play Mode。

### 原始总账后新增确认差异（BATTLE-AUDIT6）

- **BATTLE-AUDIT6-01 / GameTick-Input pass order，已关闭**：Unity 已拆分 human poll 和 unified character input，正式顺序为 poll → cooldown/M-1 → `NeedClearInput`/tick gate → character input。矩阵覆盖 tick1、清输入、oid51 frame85 gate 外延迟 split、AI 顺序；另补 transformed-human P1：CLR character 即使 current DAT 转为 non-character，仍按 roster human 轮询输入，但不会错误执行 character action。
- **BATTLE-AUDIT6-02 / DJA locals persistence，已关闭**：四类 early-return 保留进入的 private/runtime combo locals，只有正常尾路径 commit；缺/有效 target、oid6 guard、`Unk328` 与正常尾路径均有正负覆盖。
- **旧检查已按权威重写**：synthetic fixture 已补 frame85，same-tick 假阳性改为 gate 外延迟 split；不是删除断言求绿。
- **LC-02 最终契约**：plain free 清 pending、注销 slot/bucket 并归池，不触发虚拟 destroy/event/effect/额外 sound；显式 renderer/manual destroy 路径仍保留各自销毁事件。Frame / Lifecycle 20 项已由 combined fresh full PASS 覆盖。

### CP-NV1/2/3 与 STEP10 C# 重审（重开后已关闭）

这批是对原历史 backlog 的重审，不修改原始 74 项分母。旧历史 PASS 不作为当前证据；生产与检查已按 C# 调用链重写，并进入 `21:57:40.670` combined fresh full PASS。

- **CP-NV1 / immediate frame**：real/shared 双壳均清 Runtime FWC，保留 Trans wait/Prev2；最终负向矩阵覆盖 aaction/taction/jaction、负 action、方向、attacking 和双方 carrier。
- **CP-NV2 / throw snapshot/raw**：throw 已使用 source `atkFrame`；transform fixture 为 attacker frame112、victim `(76,-36)`；none/up/down/both 的 victim `Vz` 为 `0/-3/+3/0`，raw carrier 同步覆盖。
- **CP-NV3 / held sync**：`-131/0/131` 分别验证 frame131+翻面+FWC0、保留进入 frame/facing/FWC、frame131+不翻面+FWC0；位置 center/cpoint 均读最终 resolved current frame。
- **STEP10 P0**：state9 首次 sync、mismatch/escape immediate + early return、escape 同 tick `Vx/Vy`、FWC 清零与 entity stats-only 契约均已落地。
- **最终检查**：旧反权威断言已按唯一 C# 权威重写并扩展 real/shared-DAT、负 action、early-return、速度和 world stats 不变覆盖；combined Architect `P0/P1/P2=0`。

### DAT 诊断统计与 trace 证据

- `Temp/NTSDParity/data-audit-v3-required.json`：137 个权威 OID = 34 equal / 66 different / 37 missing Unity / 0 parse error；差异类别计数为 frame 126、geometry 31、sound cue 155。该统计只描述两套 raw DAT 在各自读取/适配前后的结构差异，保留作诊断信息；它不是战斗逻辑阻塞、backlog 或资源缺失清单，不要求把 DAT 文件改成相同。
- raw production battle-logic manifest 当前为 C# `41c088d2...0375`、Unity `6b34e118...332a`。旧 `compare-v3-full-final.json` 因工具按 raw manifest 做 header gate，返回 `different`、`certificateEligible=false`、`ticksCompared=0`。这只说明该次工具运行没有签发 certificate，不代表生产战斗逻辑失败；未来 certificate 应基于双方正式读取/Unity 适配后的语义 runtime 输入与 trace，raw DAT/manifest 相等不得作为前置条件。
- `Tools/NTSDParity` 构建 0 warning / 0 error。最新 `trace-compare-self-test-iter7.json` 为 **20/20 PASS**，覆盖连续 tick、空 trace、body/hash/slot commitment 防篡改、dense human input、diagnostic 显式 opt-in、diagnostic 永不签发 certificate 与 strict/fixed-world camera profile。
- iter7 authority/Unity full-detail diagnostic trace 均已生成。`compare-v3-diagnostic-full-iter7.json` 返回 `status=equal-diagnostic`、`ticksCompared=6`、`firstDifference=null`、`comparisonProfile=fixed-world-camera`、`diagnosticComparison=true`、`certificateEligible=false`、`certificateClass=none`。
- iter7 的 Unity 端使用 `authority-dat-diagnostic` 夹具；该结果只证明这 6 tick 样例的已观察域一致。原 15 项风险由各自证据逐项关闭，不是由 iter7 一次性关闭；iter7 与 R-GP-01 的 2 tick trace 都不能扩大为完整战斗逐帧等价或 production certificate。

### 状态纪律与下一步

必须按“逻辑已写 → isolated/目标编译 → Unity fresh 编译 → full self-check → 逐风险 trace → 必要 Play Mode”逐级报告，任何一级都不能替代后一级。production certificate 可以继续作为聚合对拍证据建设，但当前数量仍为 0，不能冒充已完成，也不能以 raw DAT/manifest 相等作为签发前置。

1. 原 15 项风险账已 15/15 关闭，不再把“关闭 15 风险”列为下一步。
2. 若继续建设 production certificate，扩展双方正式读取/适配后的语义 runtime、真实输入与长时间 full/full trace；保持 source < DLL < trace/result freshness。
3. 不处理 raw DAT 文件或 manifest 差异；T8 默认 `stage.dat` 部署继续暂缓，不读取、生成或私自部署默认资产。

**Audit5/Audit6 历史交接结论（已被顶部 BATTLE-AUDIT7 当前状态取代）：原始确认项曾达到 74/74 逻辑实现 + focused/full self-check，原 15 项 trace 风险曾达到 15/15 已关闭；Audit6 与重开的 CP-NV1/2/3、STEP10 也保持关闭，原 3 个受控 P2 亦已补强关闭。该批 full self-check 为 source `01:06:21.499` < DLL `01:07:21.125` < result `01:07:52.834` PASS，Architect 当时为 `P0/P1/P2=0`。R-GP-01 fresh compare 为 `equal-diagnostic`、2 ticks、无差异，但不能扩大为任意对局、全输入 production certificate，更不能覆盖 BATTLE-AUDIT7 新发现。34 equal / 66 different / 37 missing Unity 只保留为 raw DAT 适配诊断，不是阻塞或 backlog；raw DAT/manifest 相等不是 certificate 前置。T8 默认 `stage.dat` 部署继续独立暂缓。**

完整报告：

- `.omc/research/game-tick-physics-audit-20260717.md`
- `.omc/research/hit-collision-audit-20260717.md`
- `.omc/research/frame-lifecycle-audit-20260717.md`
- `Temp/NTSDParity/authority-v3-full-iter7.jsonl`
- `Temp/NTSDParity/unity-trace-v3-diagnostic-full-iter7.jsonl`
- `Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json`
- `Temp/NTSDParity/trace-compare-self-test-iter7.json`

## 1. 铁律（不可违反）

1. **权威锁死**：任何正式战斗改动必须能在 `ntsd_release_C#` 的真实调用链中找到对应行为；无法确认时标“待确认”，不得以旧工程或历史资料补写规则。
2. **表现效果一致优先**：能逐行对齐就对齐；Unity 框架限制无法同构时，**运行时最终表现必须逐帧等价**（位置/帧号/速度/伤害/时序）。
3. **只新增不误删**：本文的 ❌ 项都是「C# 有 Unity 无」，是**新增**任务，**不是删除**。
4. **架构等价严禁删**：见 §5 清单——Unity 用 resolver/组合/hook 换方式实现的，不算冗余。
5. **排除范围不碰**：bg.dat 可活动范围、相机——不对齐，不改。

## 第三次实战/静态审计交接（2026-07-16，历史记录；已被 Audit4 取代）

旧版“当前没有已确认但未实现的正式战斗逻辑差异”结论已失效。完整编号和双方证据见 `csharp-vs-unity-battle-alignment.md` 的 BATTLE-AUDIT3-01..17。17 项生产修复现已全部落地；10 已完成通用 hit_Fa 重构并补齐 3/4/10/14 直接覆盖，12 已补齐 generic holder、damaged 后继续 dvx/kind3、IronBall `FrameDelay=1` 及 world-level 真实武器覆盖。最新 fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `2026-07-16 18:24:04` < Unity `Assembly-CSharp.dll` `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。该结果包含 M-1/T4 最新矩阵；此前生产 diff 的 Architect 复核结论保留。当前仍不代表 17 项 Play Mode 全完成：真实 `NTSD_Battle` 的 Naruto 防前跳螺旋丸、奔跑防跳命中及防下跳六分身仍待本轮回归，也不能宣称全部战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署仍按用户要求暂缓。

### 分组进度

- **既有候选收集 7 项（03/04/13/14/15/16/17）**：生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；真实 Play Mode 尚未运行，不能标记场景验收完成。
- **本批已落地 8 项（01/02/05/06/07/08/09/11）**：生产修复与针对性 self-check 已通过。01 已补 `RelationTeam`，仍等待真实 bootstrap/螺旋丸 Play Mode；02/05 等 held 表现和攻击链也须在同一场景回归。09 的权威契约是 invalid positive link 只清 holder 的 `LinkState/TargetIdx/HeldWeaponSlot`，不清 inactive/mismatch target 的反向字段。
- **本批新增落地 2 项（10/12）**：10 已将 `hit_Fa1..14` 唯一实现下沉 `LF2Entity`，由 Special/Other/current-DAT shell 共用，并删除旧 TU/重复副本；新增 self-check 覆盖 3/4/10/14，其中 3/14 对 Other、current-DAT Character、Special 三种壳连续两 tick 验证副作用仅执行一次，4 覆盖 catch frame/速度/`CatchTimer`，10 覆盖原路径与落地摩擦防重复。12 的 generic holder、damaged 后继续 dvx/kind3 与 IronBall `FrameDelay=1` 已落地；`CheckWorldLevelRealWeaponStep12Contracts` 经 `SimulationWorld.HeldObjectProcessAll`、generic `LF2Entity` holder 和真实 `LF2Weapon` 覆盖 damaged→dvx、damaged→kind3、IronBall `FrameDelay=1`。新增矩阵 fresh PASS；两项仍未完成真实场景 Play Mode 验收。
- **T8**：默认 `stage.dat` 资产部署继续暂缓，不进入本轮推进。

### 执行顺序

1. **编译与自检已清**：fresh `/m:1` build 为 0 errors / 42 existing warnings；source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full self-check fresh PASS。编译和针对性自检仍不能替代真实场景行为。
2. **关系与 held 前置**：01、09、12 的生产修复和现有 self-check 已通过；09 只清 holder 三字段；12 的 world-level generic holder/真实 weapon 覆盖已补齐。01 仍待 bootstrap Play Mode。
3. **held/坐标相位**：02、05、06、08、11 的生产修复和针对性 self-check 已通过，等待真实 Play Mode。
4. **候选收集**：03、04、07、13、14、15、16、17 的生产修复和对应矩阵已通过，等待真实 Play Mode。
5. **frame logic 分派**：10 的生产重构和 fresh self-check 已通过；`hit_Fa1..14` 唯一实现已下沉 `LF2Entity`，直接覆盖已扩展到 3/4/10/14 及三壳两 tick 单次副作用矩阵。
6. **运行验收**：当前版本防下跳六分身已通过；继续回归 Naruto 防前跳螺旋丸的层级、位置、跟手和攻击路径、奔跑防跳完整后续招，以及投掷武器单次命中/Arest。
7. **Audit3 历史回写状态**：当时可写“生产修复已落地、针对性 self-check 已通过”；该阶段后来被 Audit4 的实现与 Play 验收取代，最终状态以本文后部 Audit4-01..16 为准。

### 验收门槛

- 编译错误必须为 0；“隔离 Roslyn 本轮 0 诊断”不能代替 Unity 编译成功。
- `BattleRuntimeSelfCheck` 已 fresh PASS；该结果只证明现有断言通过，不自动补齐未覆盖分支或真实场景。
- 17 个差异簇的现有针对性矩阵已通过；10 的 3/4/10/14 与三壳两 tick 矩阵、12 的 world-level generic holder/真实 weapon Step12 矩阵均已 fresh PASS。
- `NTSD_Battle` 当前版本的防下跳六分身已通过；仍需回归 Naruto 防前跳螺旋丸、奔跑防跳完整后续招和投掷武器单次命中/Arest。
- T8 只记录逻辑/生产接线状态；默认 `stage.dat` 资产部署继续暂缓。

### Audit3 历史对外措辞（已失效）

**“已发现并记录 17 个战斗逻辑差异簇，生产修复现已全部落地；fresh `/m:1` build 为 0 errors / 42 existing warnings，source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full `BattleRuntimeSelfCheck` PASS；M-1/T4 与 Audit3-10/12 的新增矩阵均已覆盖。但本轮真实 `NTSD_Battle` Naruto 螺旋丸、奔跑防跳和六分身仍待 Play Mode 验收，因此不能把 17 项标成 Play Mode 全完成，也不能宣称 C# 与 Unity 战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。”**

## 第四次战斗命中/技能链审计交接（BATTLE-AUDIT4，2026-07-17 最终状态）

完整双方坐标、影响和逐项状态见 `csharp-vs-unity-battle-alignment.md` 的 BATTLE-AUDIT4-01..16。本批 16 项**生产修复已落地**，Audit4 针对性断言已进入最终 fresh full self-check 并通过，3 项目标 Play Mode 也已全部通过。该结论只关闭本批确认差异，不能关闭完整对局逐帧对拍和 RISK-4。

| 执行组 | 编号 | 内容 | 当前状态 |
|---|---|---|---|
| 核心命中链 | 01、02、03、04、05 | AttackExempt 清理、统一标准命中、weapon candidate 消费、额外 Arest、post-collect 重筛 | 生产修复已落地，针对性矩阵 fresh PASS；投掷武器 Play 09:45:21 PASS |
| 独立实现轨 | 07、08、09、15 | SpecialAttack type3 tail、状态转换 RNG、held raw frame/facing、late frame 后 held pose/presentation | 生产修复已落地，已有断言 fresh PASS；螺旋丸 Play 01:10:34 PASS |
| 命中尾收口 | 06、10、12、14、16 | Naruto kind3、受击方向、命中声音、effect6/23 spark、catching state-exit/full reset | 生产修复已落地，针对性矩阵 fresh PASS；奔跑防跳 Play 09:34:36 PASS |
| opoint/声音生命周期 | 11、13 | first-op/OID5/52 与 frame sound/pic999 生命周期 | 生产修复已落地，针对性矩阵 fresh PASS |

最终 fresh 证据链：Unity Editor PID `11540` fresh script compile 为 **0 C# error**；`BattleRuntimeSelfCheck.cs` source/test `2026-07-17 01:39:46` < `Assembly-CSharp.dll` `09:26:23` < result `09:26:55`，full self-check **PASS**。Architect 最终复核为 **PASS**。Architect 复核后补入的矩阵明确覆盖：SpecialAttack consume 删除 live `Team` gate；collect 后将 attacker `Team=0` 仍按冻结候选连续消费两个目标；显式 oid300 abort 仍停止后续候选；SpecialAttack `PendingSounds` 严格断言单条 Cue/WorldX/Tick，并覆盖下一 tick 与 reset 清理。

`BATTLE-AUDIT4-15` 是 Play 抓出的 held late frame pose/presentation 差异：`HeldObjectProcess` 早于 late `SimFrameTick`，holder 首 tick 切帧后 held 仍读旧挂点。现已在 late frame 变化后执行纯 `SyncHeldPose`，不重复 step12，并按 holder→held 刷新 renderer。focused freshness 链 source `01:05:07` < DLL `01:06:22` < result `01:07:01` **PASS**；Rasengan Play `01:10:34` **PASS**：frame240 / oid434 / link 成立，change runtime/holderVisual/heldVisual=`5/5/5/5`，move=`9/9/9/9`，sorting `526 -> 527`，攻击链 `20 -> 257 -> 258 -> 259`，oid434 `396 -> 397`。

`BATTLE-AUDIT4-16` 是 Play 抓出的 catching state-exit/full reset 差异：Unity 普通 state transition 提前清 catch link，导致 `276 -> 277` 后下一 tick 按 `PrevFrame2=276` cpoint 强制 frame0。现已取消普通 state transition 清 link，完整实体 Reset 仍清。最终 full self-check `09:26:55` **PASS**；Running Play `09:34:36` **PASS**，完整链为 `9 -> 102 -> 295(prev2)/297(pn) -> 298 -> 299 -> 275 -> 276 -> 277 -> 278 -> 279 -> 86 -> 87 -> 88`，victim 保持 frame130/catch；oid33 `current311/pn310` 为 wait0 的正确口径。

Naruto 防下跳六分身的当前版本定向 Play Mode 已通过：真实生产输入链 `L -> L+S -> L+S+K`，tick1 frame271，tick12 frame272 且 PP `500 -> 295`/生成 oid205，tick15 frame273/oid204 展开，tick29-32 出现 6 个 unique oid33/action307，tick38 共有 6 个 renderer 可见；峰值 `max204=11`、`max205=3`、`uniqueClones=6`、`action307=6`、`maxVisible=6`。

投掷武器 Play `09:45:21` **PASS**：使用生产 oid120 / hold / double-D / D+J；HP 只在 tick17 从 `500 -> 489` 下降一次；weapon state1002/frame41 后同 tick 切到 frame7/state1000，`AttackExempt=4`；跨 35 tick 冷却归零并落地，HP 无二次下降。至此三项目标 Play Mode 已全部完成。T8 默认 `stage.dat` 资产部署继续暂缓。

当前 Unity 自动生成的 dotnet `.csproj` 仍包含 35 个已删除历史源文件，最终 `dotnet build` 被 `CS2001` 阻塞。不得把此前的 dotnet 0 error 冒充为 Audit4-16 后的最终编译证据；有效证据是 Unity fresh script compile 0 C# error。

当前对外措辞更新为：**“Audit4-01..16 的生产修复已落地并经 Architect 最终复核 PASS；Unity fresh script compile 为 0 C# error，fresh full `BattleRuntimeSelfCheck` PASS；Naruto 防下跳六分身、螺旋丸、奔跑防跳后续招和投掷武器目标 Play 均通过。本批确认差异已关闭，但完整对局逐帧对拍和 RISK-4 仍在，因此不能宣称 C# 与 Unity 全部战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。”**

非行为性清理债：`WeaponSpawner` 仍有历史非 C# 注释，F9 debug 说明也存在与当前 C# 唯一权威措辞冲突的历史文字。F7-F9/debug 按 `AGENTS.md` 排除正式战斗 backlog，不计为生产逻辑差异。

## 2. 任务清单（按建议顺序，坐标精确到行）

### T0 — 修真 bug：exemptVal 用错变量（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:268` → `int itrArest = itr.Arest < 4 && itr.Vrest == 0 ? 4 : itr.Arest;`
- **Unity 落点**：`LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入已改用 arest/vrest 权威公式。
- **验收**：`CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 Unity 运行时自检。

### T1 — ApplyAlternateDamage（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:629-827` `ShouldUseAlternateHurt` / `ApplyAlternateDamage` 完整方法。
- **Unity 落点**：共享 `LF2AlternateDamageResolver`，由真实角色与 shared-DAT 两入口复用；runtime/stat/运动尾契约已补齐。
- **验收**：alternate trigger/core/motion/character/shared-DAT/heavy/object-pass 针对性检查均通过。

### T2 — 武器命中 spark（M-9，已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1150` `RecordKind0Hit`（timer：`Fall>60 ? sparkPhase*20 : sparkPhase*20+10`），312/320/**506** 三处调用，**武器命中路径（506）也调**。
- **Unity 落点**：`LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径已接入。
- **验收**：`CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 Unity 运行时自检。

### T3 — frame 110/114 → CdDefendLock=3（M-14，已完成，Unity 运行时已验证）
- **C# 权威**：`FrameTick.cs:208-209` → `if (frame==110 || frame==114) CdDefendLock=3;`
- **Unity 落点**：`LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载。
- **验收**：`CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 Unity 运行时自检。

### T4 — oid 7/8 → 51 合体拆分（Audit6 重审已关闭）
- **C# 权威**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:1093-1263` `RunOid5152RuntimeMaintenance/TryMergeOid7Or8Into51/SplitOid51BackToPair`。旧来源结论只保留为历史记录，不能覆盖 C#。
- **历史错误顺序（已被 Audit6 推翻）**：旧实现曾按 `TickCooldowns -> human input -> AI input/combo -> M-1` 提前消费 DJA，并据此要求同 tick 拆分。唯一权威 C# 的正式输入消费在 M-1 与 `NeedClearInput` gate 之后，详见 `BATTLE-AUDIT6-01`。
- **Unity 落点**：`Oid5152RuntimeMaintenanceAll`、merge/split helper 与 runtime 身份/表现维护链已落地；split partner 在 `Reset()` 后恢复正式默认值 `FrameDelay=0`、三轴 knockback=`0.1`、`HolderCopy=99`、prev carriers 清零、`Effect`/`DeadBlink` reset，同时保留 Entity 外部 `ItrRest`。
- **当前验收**：旧 same-tick 期待已按权威改为 frame85 gate 外延迟 split，synthetic fixture 已补 frame85；poll/M-1/apply、tick1/clear gate、human/AI 与 transformed-human 均进入 combined fresh PASS。
- **freshness**：旧 `18:33:00` PASS 已过期；当前统一使用 source `21:55:28` < DLL `21:56:56` < result `21:57:40` PASS。

### T5 — 复活 pass（已完成，Unity 运行时已验证）
- **C# 权威**：`GameTick.cs:839-934` `RunRespawnPass`（tick step10）
  - 门控：state==14 + Hp<=0 + (KillCount>=0 OR Unk364==5 OR slot>=20) + HitStop∈(0,5)
  - 分支A（RespawnCount<=0）：Hp2Orig<2→FreeEntity；否则 Hp2Overlay-1、队友 X/Z 平均+随机、Pp=500、HpMax=Hp3、Hp=HpMax、HitStop=20、Frame=212、YInt=-300
  - 分支B（RespawnCount>0）：Pp=0、HpMax=RespawnCount、Hp3=HpMax、Hp=HpMax、RespawnCount=0、Unk364=1、oid∈[0x1E,0x24]→Unk318=0x8C、Frame=0xDB、FrameDelay=0xA、生成 oid998 复活特效
- **Unity 落点**：`PostFrameAdvanceDeathCleanupAll` 已实现两分支、free gate、队友平均落点、血量/PP/帧字段与 oid998 特效。
- **验收**：无 stored-count、free gate、stored-count + effect 三项检查均通过。

### T6 — kind 15/16 副作用补齐（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1628` `ApplyKind15Or16` + `1737` `ApplyKind15Movement`
  - kind16 完整：Hp-、KillStat++、ComboCountAtk、`RecordSound("SFX_065")`、Frame=200、vrest 写入、LinkState==2 断开
  - kind15 位移：`KnockbackVx = Vx + (±1)`、真实 Vx=KnockbackVx、`KnockbackVz = Vz + (±0.5)`、`YInt=-2`；按对象类型分 vyStep（角色3.0 / 飞行道具3.0 / IronBall2.3）
- **Unity 落点**：真实角色与 shared-DAT resolver 已补 kind15 authority 位移、kind16 统计/vrest/link/SFX 副作用。
- **验收**：`CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过。

### T7 — combo 连招 wrapper（大）
- **C# 权威**：`InputRuntime.cs:740` `RunComboWrappers`（9 组：Dra/Dla/Dld/Dlu/Drd/Dru/Djd/Dja/Daa/Dab + DjaGuard，含 oid6 Sasuke DjaGuard 特判），入口 `InputRuntime.cs:647`。
- **Unity 现状**：已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6 DjaGuard，真实消费路径为 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。
- **本轮新增验证**：`BattleRuntimeSelfCheck` 已补 `CheckComboWrappersCharacterFrameJumps` 与 `CheckOid6DjaGuardComboHold`，覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release。
- **验收现状**：已通过当前打开 Unity 的 request 自检机制，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。**T7 已完成。**
- **Naruto DDJ 完整链补充验收（2026-07-16）**：同 tick held chord 的内部输入 `att + down + def` 先命中 frame271；272 生成 oid205/action98，辅助链经过 99/325/341；273 生成 oid204/action130，展开六分支并各自到 147 生成 `6 x oid33/action307`。clone 在 307 后落地到 frame219 是 authority 行为。
- **本次确认的 5 个根因**：`LF2ReferencePool.Release` 无条件接收外部 synthetic，污染逻辑池类型；factory 角色 opoint 在 `ModuleBind` 注册前过早用 `slot < 0` 拒绝；pending-unregister 对象同 tick 归池复用时，旧 registry bucket 的 `Contains` 拒绝后续递归分支，六 clone 只出 3；旧实现依赖构造、`Init` 或 renderer 提前消耗 `StableId`，导致池冷热状态与表现创建顺序影响逻辑身份；`SpawnFromOpoint` 缺 `RelationTeam`、`Unk364` 与 holder-copy 继承。
- **修复契约**：`Release` 只归池 active 实例；`Register` 先 finalize pending old lifecycle；slot guard 移至 `ModuleBind + Initialize` 后；每个新逻辑生命周期只在 `SimulationWorld.Register` 成功完成 slot/rest admission 后获得 `StableId`，构造、`Init` 与 renderer 不再消耗逻辑 ID；`PostInitLiving` 继承 `Team`、`RelationTeam` 与 holder-copy。专项回归既有结果保持 PP 500→295、dynamic slot、6 unique StableId、6 x action307 和 6 visible renderer。
- **真实 Play Mode 生产输入链验收**：在 `NTSD_Battle` Play 中等待 slot0 `CharacterInputModule`/`ActionMap` 就绪，通过 UnityMCP 临时 `InputSystem.Keyboard` 按物理绑定注入 `L (Defend) -> S (Down) -> K (Jump)`。事件完整经过 `InputActionMap -> CharacterInputModule -> SimInputBuffer`，未直接调用技能、帧或 opoint。日志为 `INPUT focused=True buffered=1, attackAction=0, jumpAction=1, defendAction=1, moveY=-1`，crossed internal mapping 符合预期；结果 `frame271=True, max204=11, max205=3, maxClones=6, maxSpriteReady=6, maxVisible=6`。
- **Play Mode 时间线/证据/限制**：clone 数在 `t=0.446/0.473/0.509/0.541` 依次为 `3/4/5/6`，测试窗口无异常，截图 `Temp/naruto-ddj-unitymcp-peak.png`。Win32 `keybd_event` 不被 Unity RawInput 接收，所以这不是物理硬件键盘证明；成功证据是 UnityMCP Input System Keyboard 事件经过完整生产输入链。

### T8 — stage 波次刷敌（M-13，大）
- **C# 权威**：`GameTick.cs:2317` `ApplyCurrentWavePhaseAdvance` + `2350` `ApplyCurrentWaveImmediateStageSpawns` + `2226` `RefillCurrentWavePositiveStageSpawns`（配套 `StageProgression` + `StageSpawnRuntime*` 一整套，见 `SimulationWorld.cs:68-80`），tick step23。
- **Unity 落点**：`BattleRuntimeState` 已补齐 `StageProgression` / `StageSpawnRuntime*`；`SimulationWorld.StageWave.partial.cs` 已实现立即刷敌、正 ratio 并发槽/总量补充、清场推进和 phase bound 写回；`NTSDBattleTickSystem` 在 `PreFrameBounds` 后、`RenderDispatch` 前执行该 pass，匹配权威 step23 顺序。spawn 契约已补 `Unk344=2`、DAT type 0/5 的 character-init `RelationTeam=2/HitStun=20`、其他类型 `RelationTeam=0/HitStun=0`、dynamic slot 50+ 和 action 0 保留。
- **生产接线**：`AppManager.InitializeBattle -> SimulationTickDriver.ApplyMatchConfig -> BattleStageCampaignLoader -> ConfigureStageCampaigns(-1) -> StartInitialStageWave()` 已接通；默认读取 `Application.streamingAssetsPath/NTSD/data/stage.dat`，也可由 `MatchConfig.stageCampaignFilePath` 显式覆盖。仓库当前未纳入二进制 `stage.dat`，缺失时会明确 warning 并保持 `StageProgressionValid=false`。
- **本轮新增验证**：`CheckStageWaveBootstrapAndSpawnContract` 覆盖 stage 文本解析、pre-wave -1→0、bound、type0/type5/非角色身份契约和 action 0；`CheckStageWaveImmediateSpawnAndAdvance` 覆盖真实 direct spawn、dynamic slot 50+、20-49 非 stage 槽隔离、清场推进；`CheckStageWavePositiveSpawnRefill` 覆盖并发槽补位与总量上限。
- **验收现状**：fresh Unity batch self-check 返回 `PASS`。**T8 逻辑与生产接线代码已完成并通过运行时验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。**

### T9 — AI 输入生成器（已完成，Unity 运行时已验证）
- **C# 权威**：`InputRuntime.cs:16` `PrepareAiInputBasic` + 14 辅助函数：
  `AiBetweenX / AiPostCacheCoordinateAllowsSpecial / AiPreUpdateTarget3000SideEffect / AiUpdateOid33_19_16PredictedDuaDecision / AiUpdateOid52_1_2_21PreLabel591Decision / AiUpdateLabel591Oid51_2_18_7Decision / AiUpdateFirstDecision / AiUpdateTeammateGuardDecision / AiUpdateOid1ComboDecision / AiUpdateCloseOid1Decision / AiUpdateOid4ComboDecision / AiUpdateOid5ComboDecision / AiProcessSubOidGroup / AiSpecialOidForSubGate / AiProcessHelper`（行号见差异清单 §6.2）
- **Unity 落点**：`SimulationWorld.AiInput.partial.cs` 已完整承载主入口及直接/间接 helper 闭包，包含 runtime-slot target/cache、coordinate、team/history/held gate、C8/D3/D4/7A/7B 扫描、oid 决策组、move-mode/no-target 和三个 `AiProcessSub*` 尾部分支。
- **历史输入接线（已由 Audit6 修正）**：Unity 曾让 human 与 AI input/combo 在 oid51/52 maintenance 前执行；`BATTLE-AUDIT6-01` 已按唯一权威 C# 拆分 poll 与 apply，并经 tick1/clear gate、human/AI、oid51 延迟 split 和 transformed-human 矩阵 fresh 验证。
- **验收**：fresh dotnet build 为 0 errors / 42 existing warnings；fresh Unity full self-check 返回 `PASS`。自检覆盖 target/cache、coordinate、同 seed 确定性、human 隔离，并由 M-1 full-tick 矩阵覆盖 AI DJA 在 maintenance 前同 tick 拆分。**T9 已完成。**

## 3. 已确认对齐（不要重复处理）

tick 主循环主干（含 `InputPhase`/`FrameMod12`/`FrameToggle` 统一推进）、全局 `ValidatePositiveLinks`、kind 0/4/9 主流程、kind 6/8/10/11/14、oid300、kind5 委托、M-5 死亡弹地、M-7 kind4+WeaponCount 翻转、HP/PP 自然恢复、heal/catch timer、state14 复活与 respawn pass、frame mp turn-around、frame202 HitStun=20、opoint 生成、cpoint 正值主流程、state 400/401/500/501、N30 触发、状态转换特效。

## 4. 确认可不移植

- **M-6 F8 强制掉武器**（`RunF8WeaponDrop`）：调试功能，Unity 不需实现（非冗余）。
- `RunMode2RandomWeaponDrop`、`InitStats`/mode2 postframe 分支：属于 C# 权威工程的 F7-F9/debug 控制路径，不作为正式战斗对齐项。

## 5. 架构等价（🔷 严禁当冗余删除）

| Unity 机制 | 对应 C# | 说明 |
|-----------|---------|------|
| `LF2Character*Resolver` / `LF2Weapon*Resolver` | `NtsdCharacter`/`HitResolve`/`CPointRuntime`/`WeaponRuntime` 各段 | 组合模式拆分 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 角色分发 | 服务 transform 后 wrong-shell 角色 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | 运行时化，字段一一对应 |
| `FrameTransistor` hook | `FrameTick.Tick` 内联步骤 | 拆 hook 供覆写 |
| `SimulationWorld` 动态槽 | `Objects[400]` 固定槽 | 遍历顺序须保持 slot 升序 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023，区别于会清 attacking 的 ImmediateFrame |

## 6. 排除范围（不对齐、不改）

菜单/选人/加载、HUD/结算、bg.dat 的 Z 可活动范围、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施、`src/Host/*`。注意：PreFrame 中改变实体存亡或 X 坐标的逻辑边界仍在战斗范围内。

## 7. 工作流（每个任务照做）

1. **溯源**：打开 C# 权威行号，读懂完整逻辑（含分支/常量/字段读取顺序）。
2. **索要原型**：向 Codex 要 unified diff patch（`sandbox=read-only`，严禁真实改码），作为逻辑参考。
3. **重写**：以原型为参考，写成符合 Unity 架构的生产级代码（用现有 resolver/hook/runtime 字段）。
4. **改码**：用 executor-high（多文件）或 executor（单文件）落地。
5. **Review**：改完立即用 Codex review 或 `code-reviewer-low`。
6. **验收**：按每项的「验收」标准，优先跑 `BattleRuntimeSelfCheck`；无法运行时说明原因，不谎报。
7. **更新清单**：完成一项，去 `csharp-vs-unity-battle-alignment.md` §10 勾选对应行。

## 8. 关键文件速查

| 用途 | 路径 |
|------|------|
| 全量差异清单 | `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md` |
| C# tick 主干 | `ntsd_release_C#/src/BattleCore/Simulation/GameTick.cs` |
| C# 命中结算 | `ntsd_release_C#/src/BattleCore/Interaction/HitResolve.cs` |
| C# 帧推进 | `ntsd_release_C#/src/BattleCore/Frame/FrameTick.cs` |
| C# 输入+AI | `ntsd_release_C#/src/BattleCore/Input/InputRuntime.cs` |
| Unity 角色命中 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs` |
| Unity 武器 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` |
| Unity 帧推进钩子 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` / `LF2Character.cs` |
| Unity pass 调度 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` |
| Unity 候选收集 | `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs` |

## 9. 优先级建议

T0-T9、Audit2/Audit3/Audit4、P1 BOUNDS-X 以及 OPOINT-VIS、STEP10、TRANSFORM-SHELL、FRAME-ADV/FRAME-TICK 的既有 self-check 继续作为回归基线。**当前 Audit5 原始确认总账为 74/74 逻辑实现 + focused/full self-check，Audit6 与重开的 CP-NV/STEP10 也已关闭；freshness 为 source `21:55:28` < DLL `21:56:56` < result `21:57:40` PASS，combined Architect `P0/P1/P2=0`**。这不替代本批目标 Play Mode、完整逐 tick 或 production certificate。

| 优先级 | 当前推进 |
|---|---|
| P0 | ✅ CP-NV1/2/3 与 STEP10 已按唯一权威 C# 重审、修复并重新关闭；immediate FWC、source throw snapshot/Vz、held resolved frame、early-return/即时速度和 entity stats-only 均进入 `21:57:40` combined fresh PASS |
| P1 | ✅ INPUT-1~9 与 INTERACT-1~5 全部修复并通过新增运行时矩阵；既有 OPOINT-VIS、Step10 等 runtime matrix 继续作为回归基线 |
| P2 | ✅ RISK-1/2/3/5 与 NARUTO-DDJ/OPOINT-LIFECYCLE 已修复并运行时验证；后者覆盖 pending 注销、同 tick 归池复用、递归 opoint、StableId 和关系字段继承 |
| P3 | ⚠️ Audit4-01..16 与 3 项目标 Play 已清；继续保留 RISK-4 与完整对局逐帧对拍缺口，不扩大为全战斗完成声明 |

T8 默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。

16 个 Audit4 差异的发现证据与逐项收口状态不在本行动版重复维护，统一见完整差异清单的 Audit4 章节。INPUT-1~9 由 `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖；INTERACT-1~5 由 `CheckInteractionRuntimeSlotContracts` 覆盖；NARUTO-DDJ/OPOINT-LIFECYCLE 由真实 frame271→oid205/204→6 x oid33/action307 完整链覆盖。

P0 的旧 CP-NV 检查曾含覆盖不足或反权威期待，历史 PASS 已废止。当前 `CheckCpointNegativeActionMatrix`、`CheckCpointHeldSyncVactionMatrix`、`CheckCpointThrowRawAndTransformMatrix` 已按 C# 重写并覆盖 real/shared 双壳、负 action、FWC、source snapshot、Vz 和 `-131/0/131`；STEP10 的 mismatch/escape、即时速度和 world stats 不变也已纳入 combined fresh PASS。

本批已验收项：

- OPOINT-VIS：`CheckQueuedObjectPointPassBoundaries` 与 late-mutation 矩阵已验证 pre-advance、natural drop、逐实体 late 发布边界、real factory queue、父回收与高/low slot 可见性；过程修复 pending-destroy active-filter。
- STEP10：state9 首次 sync、mismatch/escape early return、即时速度、real/shared-DAT cpoint 与 entity stats-only/world stats 不变矩阵已通过。
- TRANSFORM-SHELL / FRAME-ADV / FRAME-TICK / LC-02：已验证 character/weapon `PS.BindRuntime`、逐 slot Transit/TU、SpecialAttack 单次 physics/frame_tick/type3 drain、`PpDisplay`、state14、negative next、state4000/8000 WFC/hit-stop 顺序、type1/2/4/6/oid999 current-DAT landing，以及 cross-SimOrder pending plain free 只注销一次且不触发虚拟 destroy/event/effect/额外 sound。
- INPUT-1~9：real character 与 shared-DAT 输入路径均已修复；`CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖 state switch、`YInt` 门、velocity tail、单一 defend-lock 真值、Super Punch、raw frame write、running 和 frame215。
- INTERACT-1~5：dynamic slot、runtime-slot vrest、state3003 双向 vrest 与 non-character kind2 链接均已修复；动态槽 `50..399` 耗尽时直接拒绝生成，并由 `CheckInteractionRuntimeSlotContracts` 断言不遗留 registry 空桶、renderer pool 对象或 reference/logic pool 生命周期残留。
- NARUTO-DDJ / OPOINT-LIFECYCLE：reference pool active-only release、pending lifecycle finalize、factory 注册时机、成功注册后按新逻辑生命周期分配 StableId，以及 opoint Team/RelationTeam/HolderCopy 继承均已修复；真实链验证 6 个 clone 使用 dynamic slot、拥有 unique StableId、到达 action307 且 renderer 可见。
- RISK-1/2/3/5：locomotion 单次推进、raw move frame、held/`TrackerParent` runtime-slot 生命周期和 current-DAT step7/step9 路由均已修复并运行时验证；`CheckHeldReferenceSlotReuseContracts`、`CheckStateTransformInteractionPhaseRouting` 等新增矩阵通过。
- RISK-4 / COLLISION-SNAPSHOT：这是 Audit2 历史风险名，现已由 Audit5 `R-HC-05` 的 fixed-slot/reuse 覆盖关闭，不再是开放项。

## 10. 实施进度（2026-07-16）

> 下表是 Audit4 前的历史实施快照，不代表当前验收已经结束。Audit4 当前状态以本文前部交接段和完整差异清单为准；旧来源记录不得用于当前实现。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState` | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **历史实现/self-check 已通过；待 C# 重审** | 唯一权威为 C# `GameTick.cs:1093-1263`；merge/split 与 pass 顺序需据此重新核验 | 既有 7 项检查只保留为回归基线，不能代替 C# 权威重审 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓 |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；输入 pass 分段；runtime 字段与 roster/opoint bootstrap | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |
| 二次审计 INPUT-1~9 | **全部已修复 / Unity 运行时已验证** | real/shared-DAT input state、raw frame、velocity tail、running/frame215 等契约已按 authority 收口 | `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵通过 |
| 二次审计 INTERACT-1~5 | **全部已修复 / Unity 运行时已验证** | dynamic slot、满槽拒绝、runtime-slot vrest、state3003、non-character kind2 已收口；拒绝路径不遗留 registry 空桶、renderer pool 或 reference/logic pool 生命周期残留 | `CheckInteractionRuntimeSlotContracts` 通过 |
| Naruto DDJ / OPOINT-LIFECYCLE | **已修复 / 当前版本真实 Play Mode 已通过** | active-only reference release；register finalize pending lifecycle；slot guard 后移；成功 slot/rest admission 后按新逻辑生命周期分配 StableId；`PostInitLiving` 补 Team/RelationTeam/HolderCopy | 当前回归通过 PP 500→295、dynamic slot、6 unique StableId、6 x oid33/action307、6 visible renderer |
| 二次审计 RISK | **历史 RISK-1..5 均已关闭** | locomotion、raw move frame、held/Tracker slot、current-DAT interaction 与 fixed-slot reuse 已收口 | Audit5 原 15 项风险总账 15/15 关闭 |

Audit3 历史验证（2026-07-16）：fresh `/m:1` build 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。M-1/T4 的 gate、oid8 镜像、identity/presentation、human+AI DJA full-tick、split formal reset 与 `ItrRest` 保留矩阵，以及 Audit3-10/12 的扩展矩阵均包含在该结果中。M-1 runtime self-check 已完成，但仍不能扩大为全部战斗逻辑完全对齐。T8 默认 `stage.dat` 部署继续由用户明确暂缓。

当前版本 `NTSD_Battle` Play Mode 已通过 Input System 的 `L -> L+S -> L+S+K` 完整生产输入链并观测 `frame271=True`、`max204/max205=11/3`、`uniqueClones/action307/maxVisible=6/6/6`。螺旋丸、奔跑防跳和投掷武器三项 Play 也已分别于 `01:10:34`、`09:34:36`、`09:45:21` 通过。上述证据完成本批定向场景验收；历史 RISK-4 已由 Audit5 `R-HC-05` 关闭，但这些定向证据仍不能替代任意对局、全输入 production certificate。T8 默认 `stage.dat` 部署仍暂缓。
## 2026-08-01 — Slice 0 handoff status

### Authority boundary

- `J:\QQFile\NTSD2.4\ntsd_release_C#` is the sole general battle-logic authority. Do not promote C++, disassembly, historical implementations, or old diagnostics to general authority.
- Preserve only the two user-specified narrow historical directed exceptions: Naruto 防下攻 and jump horizontal momentum. They do not establish rules beyond their documented cases.
- T8 default `stage.dat` deployment and Android/mobile rendering remain excluded.

### Evidence, with limits

- Full `BattleRuntimeSelfCheck` **PASS**: `.omc/validation/BattleRuntimeSelfCheck-combotxn2-20260801.log`.
- W01 6/6 and W02 2/2 are `equal-diagnostic` only: `.omc/validation/authority400-witness-root-run-W01-aftercombo2` and `.omc/validation/authority400-witness-root-run-W02-final`. Neither is a certificate.
- 100/300/500/1000 simulation-only AI short smokes all cleanup **PASS**: `.omc/validation/ProductionEntityStress.dispersed{N}.ai-sim-smoke-20260801.json`.
- 1000 baseline: `81.888 ms` average tick (~`12.21 Hz`) and `394.413 ms` average visible frame. It fails the 30 Hz target. Because dynamic opoint/capacity makes the size results non-monotonic, do not describe this as performance acceptance.

### Next open work

1. Correct C07: C# `RunLateEntityUpdate` has no independent collision, while Unity `LateEntityUpdateAll` invokes extra `SimEntityCollision`; treat it as confirmed difference / pending contract correction.
2. Resolve W08/C12 P0: `LF2ObjectRenderer` presentation release/consume of forced integer position writes `XInt/YInt/ZInt`; relocate that mutation to the deterministic TU-success physics tail.
3. W03/W04 v4 structural witnesses remain under implementation and must not be marked complete.

## 2026-07-22 交接补充：C++ 跳跃动量与 Texture2DArray 边界

- 用户明确要求本项以 `J:\QQFile\NTSD2.4\ntsd_release` 的 C++ 表现纠正 C# 共有缺陷。C++ `src/entity/frame_advance.cpp` 的 frame 212 只在存在互斥方向输入时用 DAT `jump_distance/jump_distancez` 覆盖 `vx/vz`，否则保留起跳前水平速度；空中不走地面摩擦。
- 根因位于 Unity `SimulationWorld.SerialTickAll`：它与 C# `GameTick.cs` 一样，在 frame advance 前清 current keys，使同 tick 后段 211 -> 212 初始化看不到按住方向。已移除这两次清键；战斗入口 `NeedClearInput` 全量 reset 不变，human/AI 下一 tick 的输入滚动仍由各自输入阶段负责。
- `BattleRuntimeSelfCheck` 已把 GT-02 改为 current/previous keys through-frame-advance 保留契约，并新增真实 211 -> 212 定向/无方向 Vx/Vz 回归及 cooldown/history 不变断言。
- 新鲜证据：`git diff --check` 无错误；dotnet build 为 **0 errors / 42 existing warnings**；源码 `23:15:11` < Unity DLL `23:15:37` < result `23:16:33`，fresh full `BattleRuntimeSelfCheck` 返回 **PASS**。因此代码与自动运行时契约已关闭；真实键盘 Play Mode 移动起跳仍未执行。
- 同轮解除 P4 阻塞：pool overflow 的首个动态扩容武器 mount 实际为 `Invalid`，而 world handle 为 `62:1`。`BindOwnerRuntime` 已直接同步 renderer 本体的 EntityModel mount，同时保留 generation-aware handle；P4 回归通过后全套才继续执行到跳跃矩阵并最终 PASS。
- 渲染澄清：角色中央资源已经有 `Texture2DArray` 主路径；公共阴影仍为独立 `SourceTexture2D`，因此角色/阴影会拆 resource segment。不要再把当前系统描述成“未使用 Texture2DArray”。

## 2026-08-02 — 交接增量：StableId / AI decision 热路径（验收待恢复 Editor）

### 已写入代码的合同

- StableId 的唯一逻辑分配点已收敛到 `SimulationWorld.Register` 成功 slot/rest admission 之后。失败注册不前进 allocator；renderer 使用 `GetInstanceID()` 作为 presentation-only identity，不再影响逻辑身份序列。
- 显式 StableId 与活动 identity 冲突时 fail-closed；合法显式 ID 会推进 auto allocator 下界，防止后续自动分配冲突。
- `StableIdDeterminismEditorTests` 已覆盖冷热 pool、`ResetWorld` checksum、失败注册零消耗，以及同 slot reuse 的 generation/new identity。
- `AiDecisionRowContext` 已实现且默认关闭。每个 AI decision 一次 bind；正常路径两次 gateway/六 identity rows；不进行全槽预扫。RNG 前失败可整实体 legacy fallback；RNG 后失败必须 hard-fail，禁止 replay legacy。
- AI focused/`0 B/tick` 契约和 stress gate 已写入。上述均为“逻辑已写/编译通过”状态，不是 Unity 运行时验收结论。
- Architect P1 denominator 已关闭：stress 改为精确统计 eligible invocation，不再用 tick-end `AiControlled` 推导分母；门禁要求 `applied + fallback + hard == eligible`，并覆盖 dead、coordinate、DAT-change 与 non-character。dotnet 为 0 error，Unity 仍待主代理实跑。
- StableId P2 tests 已补 zero-ID rest-bind rollback 不消耗 ID，以及 active renderer opposite activation order 下逻辑 checksum/RNG 不变；当前只记录测试已写，未宣称 Unity PASS。

### 新鲜证据与阻塞

- 根目录最终顺序 `dotnet build`：Runtime `0 error / 18 warnings`、Editor `0 error / 48 warnings`；`git diff --check` 通过。
- UnityMCP HTTP 已恢复，但目标 Editor 被 `License revoked: Your Unity Personal Version license has been revoked` modal 阻塞。一次 StableId focused job 得到 `0/unknown` 后已清理；严禁误报 focused tests PASS。fresh self-check 与 1000 AI 尚未重跑。
- 当前最近真实 1000 全 AI Candidate long300：`Temp/NTSD_ProductionEntityStress.dispersed1000.slice1-candidate-final300-20260802.json`，Avg `34.7836 ms`（约 `28.75 Hz`）、P95 `50.888 ms`、max `75.992 ms`、`0 B/tick`；仍未稳定达到 30 Hz。
- T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 2026-08-09 — 1000 AI 表现句柄缓存 A/B

- `BattlePresentationShadowBuild.cs` 增加按当前排序索引复用的 `RuntimeEntityHandle` 帧内缓存。缓存只在 `BeginFrame` 内有效，每帧先重置，实体排序数量变化时扩容；因此不会跨帧、跨 generation 或跨排序顺序复用旧句柄。`holderSlot < 0` 也直接使用空 holder，避免无意义的查询；有实际 holder slot 的路径保持不变。
- 聚焦 Editor 测试 `6fb939e7777e4009880824ea29be05c0`：`2/2 PASS`（多 hit record 的复用排序、休眠/待销毁/未来实体过滤）。`dotnet build Assembly-CSharp.csproj --no-restore --nologo`：`0 errors / 43 existing warnings`；`git diff --check` 通过。
- p118 基线（同种子、1000 分散 AI、warmup 30/sample 60）：logic Avg/P95/Max=`27.867830/33.002170/36.143700 ms`；Editor frame=`44.291700/62.196185/88.395100 ms`；frame GC Avg/P95=`124504/625116 B`；lockstep hash=`0728b2662f9b91853c50240ba5af54433c574ed073009f4a7345fe2f35bfbdb7`。
- p123（句柄缓存）：logic=`27.208958/33.038755/38.103000 ms`；Editor frame=`45.243109/73.757290/125.266105 ms`；frame GC=`108910.9/645535.1 B`；commands=`3000`，hash 与 p118 一致，cleanup 正常。
- p124（同配置重复样本）：logic=`27.963562/33.743300/36.447400 ms`；Editor frame=`44.805037/60.036521/80.776200 ms`；frame GC=`133287.7/677109.45 B`；commands=`3000`，hash 与 p118 一致，cleanup 正常。
- **结论：**句柄缓存保持行为/锁步一致，减少了重复查找，但 p123/p124 相对 p118 的差异处于样本噪声范围，不能宣称它解决了 30 FPS 问题。当前 steady tick 仍为 `0 B/tick`；主要阶段继续是 CharacterInput、Central materialization/BeginFrame、CandidateCollect 和 LateEntityUpdate。p122 的全局 OID 字典缓存因模拟-only Avg 从 `21.168987 ms` 变为 `21.837535 ms` 且无 hash 收益，已不保留。
- 当前仍不能宣称完整 Editor 帧稳定 30 FPS；`BattleRuntimeSelfCheck` 仍受既有 `BATTLE-AUDIT7-F1` 阻塞，T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 2026-08-09 — 1000 实体无输入归因对照

- p125 使用与 p124 相同的 1000 分散实体、种子、中央表现和详细计时配置，但 `inputMode=none`，用于隔离 AI 输入/状态变化的成本。logic tick Avg/P95/Max=`21.572652/25.269870/27.440300 ms`；Editor frame=`35.693512/41.237081/67.448601 ms`；logic allocation=`0 B/tick`；frame GC Avg/P95=`91044.56/505359.80 B`。
- 与 p124（1000 AI）对照：logic tick=`27.963562/33.743300/36.447400 ms`，Editor frame=`44.805037/60.036521/80.776200 ms`，logic allocation=`0 B/tick`。CharacterInput/EntityInputPass 从 `5.225420` 降到 `1.854843 ms`；CharacterInput 总阶段从 `6.046033` 降到 `2.692430 ms`；CandidateCollect 从 `3.917705` 降到 `1.558985 ms`。AI 运行时 detail 中 IndexedCanonical kernel/capture/commit 只在 AI 样本出现，说明差值来自 AI 决策和随后激活的候选/状态链，而不是单独的 Renderer `Update`。
- p125 的 `collisionCandidateCountSum`、`broadphasePairCountSum` 均为 `0`；p124 分别为 `9087`、`266263`。这证明 AI 输入会改变实体状态，从而开启碰撞候选工作；不能把两者的全部差值归于单一函数。
- **验收边界：**`inputMode=none` 不满足压力工具“AI authority roster”门，因此 p125 标记为 `StoppedWithResidue/harnessValidity=false`，只作归因实验，不作正式压力通过证据。teardown 结构化证据为 `restored=true`、`cleanupExceptionCount=0`、active GameObject/world entity/claimed slot 均为 `0`。
- **结论：**无 AI 时完整 Editor P95 仍为约 `41 ms`，所以 Renderer/Editor 壳层仍有独立成本；AI 使逻辑和候选阶段再增加约 `6.4 ms` 与 `2.36 ms`。后续优化应拆成两条可测路径：AI kernel/输入状态更新，以及候选碰撞/状态变化；不再以“整体迁移 ECS”或“某个 Update 独占”作为未经验证的结论。

## 2026-08-02 — 最新交接：Unity 全回归恢复，DecisionRowContext 不晋升默认

### Fresh 验收

- Unity fresh scripts refresh：Console `0 error`。
- focused tests：`202/202 PASS`，`44.908 s`。
- full EditMode：`483/483 PASS`，`147.809 s`。
- fresh `BattleRuntimeSelfCheck`：**PASS**。
- 这些新鲜结果已解除此前 license modal 对本轮 Unity 验收的阻塞。

### 1000 AI A/B 结论

同 seed、`60 warmup + 300 sample`：

| 顺序 / 模式 | Avg / P95 / P99 / Max（ms） |
|---|---|
| forward baseline | `32.878289 / 39.47168 / 52.064546 / 224.4373` |
| forward DecisionRowContext enabled | `34.418292 / 43.71326 / 56.953651 / 63.1937` |
| reverse baseline | `32.084814 / 39.6633 / 46.135898 / 63.9063` |

- 三份 overall/RNG/slots hashes 相同；`0 B/tick`；teardown `restored=true`、cleanup exception=`0`。
- enabled：eligible/applied=`359000/359000`，bind=`359000`，gateway=`718000`，identity rows=`2154000`，fallback/hard=`0/0`。行为门与 denominator 合同通过。
- enabled 的 Avg/P95 在双向基线对照中均回退，所以默认保持关闭，不计性能收益。baseline Avg 已略高于 30 Hz，但 P95 约 `39.5–39.7 ms`，当前仍不能称稳定 30 Hz。
- 下一步推进 Late snapshot `3 -> 1`；T8 默认 `stage.dat` 与 Android 真机验证继续排除。

## 2026-08-09 — 1000 AI 性能对照与中央 Mesh 尾部清理

- CharacterInput 后的 AI 运行时镜像已采用增量刷新；UnifiedAuthority 提交校验复用已发布的 slot/entity/generation 数组，完整刷新保留为诊断 oracle。聚焦 parity job `11c2127339394f6b9524eceaa7407ebf`：`1/1 PASS`；Unity refresh/compile 成功，未发现新的 CS 编译错误。
- p113（模拟-only、1000 生产 AI、同种子、warmup 30/sample 100、强制 sweep）：logic tick Avg/P95/Max=`21.168987/26.236925/28.5895 ms`，Editor frame Avg/P95/Max=`28.988946/46.160299/217.8702 ms`，核心 tick allocation=`0 B`，hash=`d39a52af...`，cleanup `restored=true`。
- p114（完整表现、Mesh 尾部优化前）：logic tick=`34.132236/55.206165/79.7751 ms`，Editor frame=`57.423442/91.889846/438.9619 ms`，commands=`3006`，hash 与 p113 一致。
- p115 详细计时（启用诊断开销）显示主要热段为 CharacterInput `7.183 ms`、RenderDispatch `6.203 ms`、CandidateCollect `5.869 ms`、LateEntityUpdate `4.251 ms`；RenderPrepareFrame/LegacyCapacityGuard `5.442 ms`、BuildCommands `2.783 ms`、ResolveCommands `2.610 ms`、WriteQuads `1.656 ms`。该样本 hash 差异仅由采样期间 tick 数差异造成。
- `BattleDynamicMeshBackend.Upload` 现只清理失效尾部 submesh，活动前缀直接覆盖；不改变高水位、descriptor/index 范围或尾部契约。聚焦 Mesh 测试 job `5f94e269f3e9450699b11da46bdf10a0`：`1/1 PASS`。
- p116/p117（完整表现、Mesh 尾部优化后）：logic tick 分别为 `25.766937/31.019265/32.5922 ms`、`26.479747/32.42429/35.7732 ms`；Editor frame 分别为 `42.618139/67.091432/256.229997 ms`、`43.683248/63.464321/262.176394 ms`。两次均 3006 commands、hash=`d39a52af...`、cleanup 完整恢复。
- 当前状态：模拟-only P95 已低于 30 Hz 门槛，但完整 Editor frame P95 仍在 `63–67 ms`，不能宣称稳定 30 FPS；下一步继续针对表现快照、命令构建、Renderer backend 和测试壳层做有测量依据的优化，不据此直接迁移整体 ECS。
- fresh `BattleRuntimeSelfCheck` 当前失败于既有 `BATTLE-AUDIT7-F1`（`BattleRuntimeSelfCheck.cs:26643`，ApplyMatchConfig 预波次契约），与本轮改动无直接调用关系；完整 self-check 仍未通过。T8 默认 `stage.dat` 和 Android 真机验证继续排除。

## 2026-08-02 — Late snapshot `3 -> 1` 已 promotion

- **语义边界：**权威 C# `GameTick` 的单实体 Late 连续链无中间 snapshot。Unity 仅合并 Runtime mirror，不改变 pass、flush、opoint、RNG、slot 或 lifecycle。普通 active entity 在 `Consolidated` 下只保留 Tail final refresh；11xx/12xx FrameExit 即时 refresh 保留；inactive/free/cleanup 不再无效发布；transition 内部冗余发布跳过。`LegacyThree` 显式 oracle 保留。
- **报告入口：**六份 A/B 均为 `Temp/NTSD_ProductionEntityStress.dispersed1000.late-snapshot-ab-*-20260802.json`。
- **detailed100：**LegacyThree Avg/P95=`31.898607/39.055705 ms`，FrameTick/Death/Tail=`100000/100000/100000`；Consolidated=`31.038641/38.034015 ms`，仅 Tail=`100000`。delta Avg/P95=`-0.859966/-1.02169 ms`；overall=`fa019a38aba6668b7222bf9b61b0400d2cba7b422799bbd0964506a9875450e9`，RNG/slots/events 相同，`0 B/tick` 且 clean。
- **long300 forward：**LegacyThree=`31.619902/39.4185 ms`，Consolidated=`31.063709/37.637035 ms`。**reverse：**Consolidated=`32.93091/45.88688 ms`，LegacyThree=`34.39729/49.865545 ms`。两组 overall=`68af82ba7cdf284d7f62e889e1cd1188e14e9c15ec48d15167cd6c8dcf210388`，RNG/slots/events 相同，`0 B/tick`、`restored=true`、cleanup exception=`0`。
- **晋升与回归：**Architect **PASS**；ordinary world、request empty/default、Window default、AI smoke 已使用 `Consolidated`，显式 legacy 仍可选。fresh runtime/editor build `0 error`；Unity clean refresh `0 error`；focused `135/135 PASS`，job `d63ca30b2e7049a88d718807a6d9820d`；full `495/495 PASS`，job `7936ba59d6a44553b3d41429821810d1`；fresh self-check **PASS**。
- **默认模式 smoke：**`Temp/NTSD_ProductionEntityStress.smoke50.late-snapshot-default-promotion-20260802.json` 为 `SmokePassed`，requested/effective consolidated，`0 B/tick`、`restored=true`、cleanup exception=`0`；最后退出 Play Mode、清 Console 后为 `0 error`。
- **剩余性能状态：**优化已 promotion，但仍不能称 1000 AI 稳定 30 Hz。best forward Consolidated Avg=`31.063709 ms`、P95=`37.637035 ms`，reverse P95=`45.88688 ms`。下一热点为 CharacterInput `9.188 ms`（Remaining `5.223 ms`）、CandidateCollect Avg/P95=`3.878/9.405 ms`、Late FrameTick=`2.377 ms`、FrameAdvance=`2.05 ms`，等待下一 SoA slice 设计。
- T8 默认 `stage.dat` 与 Android 真机验证继续排除。
## 2026-08-10 — 战斗零 GC、static 与 partial 结构收敛

- 专项计划与验收边界见 `Assets/NTSD/Docs/battle-zero-gc-and-structure-plan.md`。
- formal tick、driver Update/LateUpdate 与 PlayerLoop envelope 已分别建立计数；Editor 的完整帧数据只作观察，Player 才执行完整帧硬门禁并在战斗窗口禁用 GC。
- 已修复资源加载器空队列每帧异步/快照分配、暂停节点死循环、pool mount 数组式 GetComponents、Editor Play 边界每帧 FindObjectsOfType、Camera.allCameras 数组和 Loose Quadtree 节点 List 扩容风险。
- 当前状态是“代码已写、聚焦测试逐批通过”；fresh self-check、1000 AI 完整复测和 Desktop Player 完整帧门禁尚未完成，不能扩大宣称为全战斗零 GC 完成。
