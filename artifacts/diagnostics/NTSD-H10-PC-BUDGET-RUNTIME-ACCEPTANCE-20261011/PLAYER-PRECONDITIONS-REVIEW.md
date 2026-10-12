# 后续 Windows Player 验收的精确前置方案（未执行）

本轮只读了获准四个诊断来源，未修改或运行它们，未build、未启动Player、未清旧Temp构建。下述是下一阶段可独立审查的方案，不是当前批准的额外写域。

## 已确认的现有入口与限制

| 文件/入口 | 已确认事实 | 不能直接承担的门 |
|---|---|---|
| `Test/Editor/ProductionEntityStressPlayerBuild.cs:46` | 旧R8 builder写PlayerSettings backend/frameTiming/runInBackground、改Burst配置，finally覆盖Burst文件并SaveAssets；只构建Battle Scene。旧复制器只复制Config dat/txt、Sprite bmp与SPARK.bmp。 | 不能在当前零Settings写域调用；也没有完整正式Logan音频sidecar部署保证。 |
| `Animation/Rendering/ProductionEntityStressPlayerBootstrap.cs:12` | `--ntsd-production-stress-request <absolute-json>`读取请求，使用既有config/runner，强制autoStop并在一轮结束后Quit；运行前抑制默认bootstrap造实体，由真实stress runner接管。 | 不是同一进程三轮session循环；该文件本身没有配置音频抑制默认值。 |
| `Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs:57` | `-ntsd-q07-serialized-root-probe`可保留序列化root；普通probe会注入root。`-ntsd-q10-formal-audio-probe`核正式020/067路径、samples、两次播放计数，并走App公开关闭/卸载；一次后Quit。 | 没有进程采样、三轮重入、1000真实实体或实际isPlaying/输出证明；不能拿两次播放计数当听感证书。 |
| `Test/NTSD28Q07MenuFirstWindowsPlayerProbe.cs:45` | 真正走Menu按钮、VS、Naruto OID2、0电脑、Fight、Additive Battle和返回Menu；一次后Quit，并固定旧fingerprint。 | 不能直接承接当前版本三轮预算或千人最坏场景；硬编码fingerprint须在独立诊断调整中绑定当前获批输入，不能降低一致性门。 |

已保存的真实千人请求可作为配置来源：`NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008/windows-02/01-combat1000-on/report.json.request.json`，`combat1000`、`ai`、1000实体、seed1314149188、120warm+180sample、`simulationOnly:false`、`soundPresentationMode:"dispatch"`。这是明确请求值，不是未读config/runner的默认行为证明。历史候选/细分计时开关不得盲目整份照搬。正式性能阶段仍为120warm+180冒烟、120warm+1800正式；本轮音频短测不能替代。

## 构建设置的明确决定点

当前磁盘 `ProjectSettings/ProjectSettings.asset` 的 `scriptingBackend.Standalone: 1`（现有IL2CPP设置）、`runInBackground:1`、`enableFrameTimingStats:0`；Windows Burst配置为 `EnableBurstCompilation:true`。本轮只读这些值，没有修改。

- 零Settings写路线：新的独立builder只调用 `BuildPipeline.BuildPlayer` 并保持所有当前设置，独立output，显式Scene列表，不调用SaveAssets、不改Burst。这会继承当前Standalone backend，不能把产物称Mono。当前IL2CPP被后置，因此此路线不能悄悄执行为“先测Mono”。
- 如下一阶段选Windows Mono诊断，必要最小暂态改动候选只有Standalone backend 1→0，并在finally恢复原API值及核对真实文件字节；是否需要此Settings写域须在独立审阅后按用户规则准确确认。不要照搬旧R8的frameTiming、runInBackground和Burst改写。Burst构建兼容性未验证，不能预先关掉来规避未知失败。
- 不启动第二个占用同Library的Editor，不直接运行旧R8 builder，不以更换工具绕过Settings边界。

## 下一实施包的具体方案

1. 独立新根建议 `artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/`，子目录 `build-01/`、`ordinary-three-sessions/`、`combat1000-three-sessions/`；存在即拒绝覆盖。构建含原Menu与Battle Scene，不保存改写Scene。
2. 逐文件manifest部署正式runtime的 `catalog.csv`、`decoded_dat`、`vfs` 到产物根 `Assets/NTSD/Content/LoganRuntime`；按实际解析公式部署generic音频到 `<exe>_Data/NTSD/Sound`，其他必要SPARK/UI原素材按现有请求路径复制。实际复制清单、排除.meta等规则、SHA与源root必须在新Operation冻结；不依赖旧R8只复制dat/bmp的做法，不修改源资产。源目录无需“恢复”，只检查哈希；新输出保留，任何清理另行审计。
3. 在独立诊断runner/probe中复用上述真实普通入口与stress入口，保持音频dispatch，记录解析后的音频状态、prepared cue数、World实际实体数、seed/输入profile。现有四个文件不足以证明未读stress config默认音频状态；下一实现应在绑定请求时显式核该状态，失败即停，不能用同名模式推断。首次普通三轮后再运行真实千人三轮，不将1000音频请求等同千人实体。
4. 新probe需要同一Player进程完成三轮公开关闭/卸载/重入，跨轮采集旧owned instance IDs、borrowed cache身份、stage5/IO drain/下一帧Destroy/下一局首次load边界；现有单轮后Quit的probe不能冒充。最终自然退出，不把每轮新进程当无积累证据。
5. 外部Windows采样器用确切PID+创建时间，100ms间隔记录时间戳、Private Bytes/private commit与WorkingSet64；记录漏采/延迟、进程退出和阶段标记。Private与WorkingSet分列不相加。阶段标记包含冷启动、准备、稳态、关闭返回、drain、Destroy可见、下一代准备；峰值只称采样峰。不得将Unity内本轮返回0的Process计数视为有效数据。
6. 320MiB只评价去重AudioClip对象域；另列decode/staging/temp/voice可观测值及不可观测项。稳态Private≤2GiB、过渡采样峰≤3GiB、三轮owned归零且不积累，首次超门或不可解释差值停止。不改阈值、不缩catalog、不关声音、不重跑挑低峰。
7. 保存CPU/GPU/内存/电源模式、后台负载、实际分辨率和显示目标；当前i5-13600KF开发机不是8GB基础配置认证，CPU持续功耗/性能未知仍需目标机。Android、GPU认证、AB与IL2CPP后期门不提前关闭。

本轮没有形成可运行的新builder/probe，也没有批准资源复制或Settings写入。此方案供同H10后续独立审阅；当前先修本轮测量夹具并完成尚缺的Play门，不恢复其他优化点。
