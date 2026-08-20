# U9 单机 1000 AI 最终验收（2026-08-15）

## 1. 结论

U9 的 Windows Mono Player 正式矩阵已通过，U6 的生产所有权退出门与 U8 的 worker/synchronous 等价门也随本轮证据关闭。`2026-08-20` 针对当前工作树、渲染资源迁移和相机修改又重新构建 Player，并重跑 U7、U8 和完整 U9 五场景矩阵；下方表格与路径已更新为这次 fresh 证据。

本轮只完成单机阶段，不实现服务器业务。S0～S5、Socket、ACK、Jitter Buffer、房间、登录与重连均未开始。T8 默认 `stage.dat` 和 Android 真机仍按用户要求排除。`2026-08-16` 项目迁移到国际版 `Unity 2022.3.62f3 (96770f904ca7)` 后，匹配 revision 的 Windows Mono 与 IL2CPP correctness gate 已全部真实运行并通过，旧的中国版 Editor/国际版 Player variation 混装阻塞已经关闭。

## 2. 最新构建与正确性门

- Unity 脚本编译与 Windows Mono Development Player 构建：`2026-08-20` 当前代码真实构建通过；
- U6 聚焦：CharacterInput 37/37 PASS，生产所有权 6/6 PASS；
- U7 fresh 扩大聚焦：snapshot、restore、ring、session、checksum、runtime validation 合计 39/39 PASS，job `f0e76a50c9064e239ea1c2f438be465b`；
- U8 聚焦：worker boundary、单机 runtime validation、lockstep checksum 合计 25/25 PASS；
- 完整 EditMode job：`126790e9345043bd83c1e5a81b1f38a5`，`1265/1265 PASS`、0 failed、0 skipped，耗时 `168.0734563 s`；此前受 UnityMCP `NetworkStream disposed` 日志污染的 job 不纳入本结论；
- `BattleRuntimeSelfCheck`：`2026-08-20 12:14:11 PASS`；
- Authority400 fresh full/full diagnostic：权威 C# `Temp/NTSDParity/u9-final-authority-20260815.jsonl` 与 Unity `u9-final-unity-authority-dat-diagnostic-20260815.jsonl` 比较为 6/6 `equal-diagnostic`、`firstDifference=null`、manifest 相同，见 `u9-final-compare-authority-dat-diagnostic-20260815.json`。该夹具因 Unity DAT 适配边界而明确不是 production certificate。

## 3. U7 Snapshot/Restore 边界

最新 Windows Player 报告：`Temp/U7-Windows-IL2CPP/Mono/u7-runtime-report.json` 与 `Temp/U7-Windows-IL2CPP/IL2CPP/u7-runtime-report.json`。

- 状态：`Passed`；
- pure value transfer/factory：通过；
- snapshot -> mutate topology -> restore -> journal replay：通过；
- source/restored checksum：`2f92a339254225de11790c2d4eb8fc51f36e7cdd6245a891d25f041ef17ac093`；
- replay checksum：`3DEB30C4D190E5FB`；
- warm exact restore：0 B；
- Unity 版本：两者均为 `2022.3.62f3`；
- 平台：两者均为 `WindowsPlayer`；
- 恢复 `(slot, stableId, generation)`：两者均为 `(3, 100, 1)`；
- Mono/IL2CPP 上述字段及 source/restored/replay checksum 逐项相同。

`2026-08-20 12:08～12:11`，国际版 `Unity 2022.3.62f3 (96770f904ca7)` 使用同 revision 的 Windows Mono 与 IL2CPP Player variation 再次完成真实 CleanBuildCache 构建、启动和 runtime bootstrap。两份报告状态均为 `Passed`，交叉运行时比较全部通过。为隔离本机 Burst Windows hash cache，正确性门禁对两个后端一致地临时关闭 Burst AOT；门禁结束后 Standalone 后端恢复为 IL2CPP，Frame Timing Stats 恢复为关闭，Burst 恢复为开启。

`2026-08-15` 的旧版本混装故障记录保留如下，但不再代表当前状态：当时中国版 `2022.3.40f1c1 (0bae6c114c78)` Editor 与国际版 `2022.3.40f1 (cbdda657d2f0)` Player variation 不一致，导致 Player 在 runtime bootstrap 前退出。该问题最终通过完整迁移到同 revision 的国际版 `2022.3.62f3` Editor 与模块解决，没有替换 DLL、伪造版本字符串或手改 `ProjectVersion.txt`。

`2026-08-16` 用户明确只使用国际版，不需要中国版；项目随后以完整国际版 `D:\Unity\HubEditor\2022.3.62f3\Editor\Unity.exe` 打开并完成上述跨运行时门禁。旧 Editor 不再作为项目验证环境。

## 4. U8 Worker 与同步等价门

报告：

- `Temp/U9-Windows-Player/Reports-2022.3.62f3/u8-worker-combat1000-30x300.json`；
- `Temp/U9-Windows-Player/Reports-2022.3.62f3/u8-sync-combat1000-30x300.json`。

| 模式 | 正式 tick | Average | P95 | P99 | Max | Central draw | Tick alloc | Gen0/1/2 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Dedicated worker | 300 | 4.2995 ms | 5.7216 ms | 6.3513 ms | 9.0103 ms | 1 | 0 B | 0/0/0 |
| Synchronous | 300 | 4.0190 ms | 5.2066 ms | 5.8594 ms | 8.4089 ms | 1 | 0 B | 0/0/0 |

两份报告使用相同 seed、负载与采样口径；workload fingerprint 相同。overall、world、slots、aRest、vRest、RNG、input、stats、events、metadata 十域最终 hash 全部一致。implementation config fingerprint 因“是否启用 dedicated worker”配置位而不同，这是对照变量，不是状态分叉。

## 5. U9 五场景 Windows Player 矩阵

所有场景均使用：

- 1000 个真实生产实体；
- 300 tick 预热；
- 1800 tick（60 个逻辑秒）正式采样；
- `DataOrientedCanonical` AI；
- dedicated simulation worker；
- 中央渲染开启，不是 simulation-only；
- U6 production ownership audit 与 zero-GC hard gate 开启。

| 场景 | Logic Avg | Logic P95 | Logic P99 | Logic Max | 完整帧 Avg | 平均 FPS | SetPass | Central draw |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Idle1000 | 3.4161 ms | 6.5989 ms | 12.1378 ms | 16.7198 ms | 16.6725 ms | 59.98 | 6 | 1 |
| Move1000 | 2.7957 ms | 3.9235 ms | 4.9547 ms | 6.7520 ms | 16.7091 ms | 59.85 | 6 | 1 |
| Dispersed1000 | 3.9128 ms | 5.0592 ms | 5.9115 ms | 9.9178 ms | 16.6725 ms | 59.98 | 6 | 1 |
| Combat1000 | 3.8269 ms | 5.1214 ms | 6.3258 ms | 9.6252 ms | 16.6725 ms | 59.98 | 6 | 1 |
| Concentrated1000 | 4.2915 ms | 5.5420 ms | 7.4755 ms | 15.4023 ms | 16.6726 ms | 59.98 | 6 | 1 |

`2026-08-20` 当前工作树使用新构建的可见 640×360 Windows Mono Development Player 重跑完整五场景矩阵。所有场景均 `StoppedCleanly`、1800/1800 正式 tick、worker `2100/2100`、SetPass=6、中央 draw=1；完整帧平均约 `59.85～59.98 FPS`。构建工具为规避本机 Burst hash cache 临时关闭 Burst AOT，并在构建后恢复开启；因此本表如实记录该 Development Player 配置，不把它描述为单独的 Burst 性能基准。

完整报告位于 `Temp/U9-Windows-Player/Reports-2022.3.62f3/`：

- `u9-idle1000-60s.json`；
- `u9-move1000-60s.json`；
- `u9-dispersed1000-60s.json`；
- `u9-combat1000-60s.json`；
- `u9-concentrated1000-60s.json`。

五份报告共同满足：

- status 为 `StoppedCleanly`，1800/1800 正式 tick；
- logic tick、driver update、presentation、PlayerLoop managed-memory 边界均为 0 B；
- Gen0/1/2 collection 均为 0；
- capacity pressure gate 通过，正式窗口 rejected/dropped delta 为 0；
- collision broadphase abort、candidate store failure、worker failure 为 0；
- U6 configuration/runtime ownership evidence 通过，canonical mismatch 为 0；
- 中央 Renderer Feature 有实际像素提交证据，draw 为 1；
- teardown 后 active GameObject、world entity、claimed slot、active object pool、active reference pool 均恢复为 0，cleanup exception 为 0。

各独立 Player 进程在资源加载前后累计记录了 backlog clamp/drop；本轮仅 Move1000 在正式窗口外累计 `droppedBacklogTicks=4`，其余为 0。该计数属于资源加载/预热长帧，不属于 1800 tick 正式窗口；五场景正式 `capacityPressure` 均为 1800 tick、0 violation、`totalRejectedOrDroppedDelta=0`，且 `maxCatchUpTicksPerFrame=1`，没有在单机采样中执行每帧四个完整 tick 的追帧行为。

## 6. 可见窗口与隐藏窗口诊断

最新 Player 初次复验使用 `Start-Process -WindowStyle Hidden` 时，战斗逻辑仍完成 300 tick，但 Windows/URP 没有推进可见的 `BattleRenderFeature`，中央 draw 为 0，验收脚本按设计拒绝该报告。随后使用 640×360 的正常可见窗口重跑：同一 Player、同一 URP Asset 与 Renderer Data 下，中央 draw 恢复为 1，像素证据有效。

因此隐藏窗口报告不纳入 U8/U9 正式证据；最终矩阵全部来自正常可见的 Windows Player，不是脚本放宽门禁，也不是回退到 Legacy SpriteRenderer。

## 7. 阶段边界

- U6：按生产 canonical owner、Unity shell/fail-closed compatibility、A/B 否决 Legacy 三类路径完成退出审计；
- U7：单机 snapshot/restore/replay、Windows Mono 与 Windows IL2CPP 跨运行时 correctness 全部完成；
- U8：dedicated worker 生产接线与同步等价门完成；
- U9：Windows Mono Player 1000 AI / 30 FPS 容量目标完成；
- S0～S5：未开始。必须由用户再次明确确认后才能进入 S0；
- T8 默认 `stage.dat`、Android 真机继续排除；Windows IL2CPP 已有真实通过报告，不再属于排除项。
