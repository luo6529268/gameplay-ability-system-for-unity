# U9 单机 1000 AI 最终验收（2026-08-15）

## 1. 结论

U9 的 Windows Mono Player 正式矩阵已通过，U6 的生产所有权退出门与 U8 的 worker/synchronous 等价门也随本轮证据关闭。

本轮只完成单机阶段，不实现服务器业务。S0～S5、Socket、ACK、Jitter Buffer、房间、登录与重连均未开始。T8 默认 `stage.dat` 和 Android 真机仍按用户要求排除。Windows IL2CPP Player gate 因本机 Unity 安装缺少 Windows IL2CPP Player variation 而未运行，不能把 Windows Mono 结果冒充 IL2CPP 结果。

## 2. 最新构建与正确性门

- Unity 脚本编译：0 C# error；Windows Mono Player 的 `Assembly-CSharp.dll` 更新时间为 `2026-08-15 21:30:03`，晚于最终代码修复；
- U6 聚焦：CharacterInput 37/37 PASS，生产所有权 6/6 PASS；
- U7 聚焦：snapshot、restore、ring、session、checksum 合计 29/29 PASS；
- U8 聚焦：worker boundary、单机 runtime validation、lockstep checksum 合计 25/25 PASS；
- 完整 EditMode job：1265 项中 1264 PASS；唯一失败来自 UnityMCP `NetworkStream disposed` Error 日志注入，对应 `BattleRenderingBenchmarkEditorTests.LeakCheck...` 独立重跑 1/1 PASS，不记录为代码断言失败，也不把受污染 job 写成干净全量 PASS；
- `BattleRuntimeSelfCheck`：`2026-08-15 21:29:11 PASS`；
- Authority400 fresh full/full diagnostic：权威 C# `Temp/NTSDParity/u9-final-authority-20260815.jsonl` 与 Unity `u9-final-unity-authority-dat-diagnostic-20260815.jsonl` 比较为 6/6 `equal-diagnostic`、`firstDifference=null`、manifest 相同，见 `u9-final-compare-authority-dat-diagnostic-20260815.json`。该夹具因 Unity DAT 适配边界而明确不是 production certificate。

## 3. U7 Snapshot/Restore 边界

最新 Windows Mono Player 报告：`Temp/U7-Windows-IL2CPP/Mono/u7-runtime-report-final.json`。

- 状态：`Passed`；
- pure value transfer/factory：通过；
- snapshot -> mutate topology -> restore -> journal replay：通过；
- source/restored checksum：`2f92a339254225de11790c2d4eb8fc51f36e7cdd6245a891d25f041ef17ac093`；
- replay checksum：`3DEB30C4D190E5FB`；
- warm exact restore：0 B。

Windows IL2CPP 没有可执行报告。检查到 `D:\Unity\HubEditor` 下的 2022.3.4f1c1、2022.3.34f1、2022.3.40f1、2022.3.59f1c1 等安装只有 Windows Mono player variations，没有 Windows IL2CPP player variation。这是外部工具链待验证项。

## 4. U8 Worker 与同步等价门

报告：

- `Temp/U9-Windows-Player/Reports/u8-worker-combat1000-30x300-final.json`；
- `Temp/U9-Windows-Player/Reports/u8-sync-combat1000-30x300-final.json`。

| 模式 | 正式 tick | Average | P95 | P99 | Max | Central draw | Tick alloc | Gen0/1/2 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Dedicated worker | 300 | 4.3539 ms | 5.7763 ms | 6.5023 ms | 7.9603 ms | 1 | 0 B | 0/0/0 |
| Synchronous | 300 | 4.1795 ms | 5.4246 ms | 6.4738 ms | 7.4542 ms | 1 | 0 B | 0/0/0 |

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
| Idle1000 | 2.8514 ms | 3.9232 ms | 5.6346 ms | 7.3894 ms | 17.9221 ms | 55.80 | 7 | 1 |
| Move1000 | 2.8540 ms | 3.7321 ms | 4.2448 ms | 5.6814 ms | 17.0388 ms | 58.69 | 7 | 1 |
| Dispersed1000 | 3.6808 ms | 4.4386 ms | 5.5952 ms | 6.6915 ms | 16.9167 ms | 59.11 | 7 | 1 |
| Combat1000 | 3.6604 ms | 4.5215 ms | 5.5222 ms | 7.4254 ms | 16.9778 ms | 58.90 | 7 | 1 |
| Concentrated1000 | 4.3253 ms | 5.8677 ms | 8.5765 ms | 10.4058 ms | 17.8285 ms | 56.09 | 7 | 1 |

完整报告位于 `Temp/U9-Windows-Player/Reports/`：

- `idle1000-rendered-60s.json`；
- `move1000-rendered-60s.json`；
- `dispersed1000-rendered-60s.json`；
- `combat1000-rendered-60s.json`；
- `concentrated1000-rendered-60s.json`。

五份报告共同满足：

- status 为 `StoppedCleanly`，1800/1800 正式 tick；
- logic tick、driver update、presentation、PlayerLoop managed-memory 边界均为 0 B；
- Gen0/1/2 collection 均为 0；
- capacity pressure gate 通过，正式窗口 rejected/dropped delta 为 0；
- collision broadphase abort、candidate store failure、worker failure 为 0；
- U6 configuration/runtime ownership evidence 通过，canonical mismatch 为 0；
- 中央 Renderer Feature 有实际像素提交证据，draw 为 1；
- teardown 后 active GameObject、world entity、claimed slot、active object pool、active reference pool 均恢复为 0，cleanup exception 为 0。

各独立 Player 进程在资源加载前后累计记录了 backlog clamp/drop；该计数包含正式采样前的 BMP、图集与音频预加载长帧，不属于 1800 tick 正式窗口。正式窗口的 capacity pressure/rejection gate 为 0 violation，且 `maxCatchUpTicksPerFrame=1`，没有在单机采样中执行每帧四个完整 tick 的追帧行为。

## 6. 可见窗口与隐藏窗口诊断

最新 Player 初次复验使用 `Start-Process -WindowStyle Hidden` 时，战斗逻辑仍完成 300 tick，但 Windows/URP 没有推进可见的 `BattleRenderFeature`，中央 draw 为 0，验收脚本按设计拒绝该报告。随后使用 640×360 的正常可见窗口重跑：同一 Player、同一 URP Asset 与 Renderer Data 下，中央 draw 恢复为 1，像素证据有效。

因此隐藏窗口报告不纳入 U8/U9 正式证据；最终矩阵全部来自正常可见的 Windows Player，不是脚本放宽门禁，也不是回退到 Legacy SpriteRenderer。

## 7. 阶段边界

- U6：按生产 canonical owner、Unity shell/fail-closed compatibility、A/B 否决 Legacy 三类路径完成退出审计；
- U7：单机 snapshot/restore/replay 功能与 Windows Mono correctness 完成，Windows IL2CPP 外部待验证；
- U8：dedicated worker 生产接线与同步等价门完成；
- U9：Windows Mono Player 1000 AI / 30 FPS 容量目标完成；
- S0～S5：未开始。必须由用户再次明确确认后才能进入 S0；
- T8 默认 `stage.dat`、Android 真机、Windows IL2CPP 不被本报告伪装为已验证。
