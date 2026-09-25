# Q07 鸣人 DDJ 正式发行／Unity Manual 同初态对照（2026-09-25）

## 结论与边界

原项目 Unity Editor 中的独立 Manual 诊断 `manual-ddj-20260925-3` PASS，连续输出完成 tick 1–26。以正式根 `NTSD2.8-Logan.exe`（SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`）回放 trace 的相同 tick、实体 slot 为键，25 个显式实体字段在 57 个实体行上 **1425/1425 相等**；13 个输入字段在 57 个实体行上，加 26 个全局输入相位，**767/767 相等**。这两个有限字段集合共 **2192/2192 相等**，零首差。tick 6 鸣人进入 action 495、MP 150；tick 16 OID 518 出现在 slot 50。不能由此推出全部世界状态、像素、音频或 Q07 聚合出口对齐。

正式发行依据是 `../NTSD28-Q07-FORMAL-DDJ-RELEASE-PLAYBACK-001/formal_release_playback_action110_trace.jsonl`（SHA-256 `769D3A734C72EE5FD165054383D6A237691F5401D286590CDF53472AC267F189`），其 LFR 报告明确 `nativeParityClaim:false`。Unity 依据是本目录 `naruto-ddj-formal-26-physical-lsk.json`（SHA-256 `240DE56FF5358E4AAF9B2E464F285A355E9B5D59AFEBE27AF02AE76FA3641622`）、`manual-ddj-20260925-3.raw.jsonl`（SHA-256 `E7731F99CE6B1F0AE7AA8E0829F6BF5D88D6E2B1C55378FC79F5AB38B1FCB9E0`）与 `manual-ddj-20260925-3.input-rng.jsonl`（SHA-256 `2649ED9C9E0AD21101B1A5197AA51DABDA9FA8755D4A4A03B91E66E005CA98D3`）。Unity 使用已暂存正式非排除 DAT、项目 `ProjectBattleModeConfig` Asset；未加载用户排除的原版背景及模式 DAT，未改 DAT 数值或保存 Scene。

对照按 native `tick` 与 Unity `completedTick` 连接，再按 `slot` 连接；每个 tick 的活跃 slot 集合完全相同。实体字段为 OID、类型、action、state、frameCounter、facing、整数及精确 XYZ、速度 XYZ、当前/基础最大/有效最大 HP、MP、weapon HP、复活次数、render phase、previous action、owner、team。输入字段为 bound state、last action、remap state/indices、proxy tail、run accumulator、global record state、defend reentry cooldown、combo state、key history、current/previous mask、七键 edge window；另比对每 tick 的 input phase。Unity 原始代理键序 `W,S,A,D,J,K,L` 与 native 语义位序不同：本次按已观察到的物理 `L/S/K` 路由将 Unity down 位 8 归一为 native down 位 2，并将 edge-window 字典按 native `up,down,left,right,attack,jump,defend` 顺序比较。未经这个显式映射的原始 mask 数值不应写成相等。上述计数只包括这些已列字段，不包含未经投影的状态。

## RNG 与诊断过程

初始 RNG **不相等**：Unity CRT state `2461722130`、累计 3000 次；发行 LFR CRT state `3374725112`、累计 3000 次。正式 `GameSessionLfrPlayback28::load` 先将 `config_={}`，`BattleConfig28::random_seed` 默认 0；`GameSession28::begin_match` 用该 seed 初始化 CRT，再单独恢复录像同步表。Unity 此夹具显式 seed `0x28A55A5A`。双方同步表哈希同为 `58181F48CC1F3BB5`，但同步计数/last-call-site 的记录口径不同。此 26 tick 内双方 CRT 和同步 RNG 都无新增调用，所以该初态差异没有进入本段已比字段。它仍阻止完整 RNG／全世界校验证书；若后续测试触发随机调用，须另建同口径初态与调用顺序对照，不能用本报告外推。

`manual-ddj-20260925-1` 在 allocation seal 前 FAIL，原因是新诊断只装入初始角色而漏发布同身份的 visual candidate；已保留失败结果并只修 Editor 诊断。`-2` runner PASS 但旧夹具 `K/S/J` 被实际解释为跳／下／攻击，未触发 DDJ；保留全部输出后另建 `L/S/K` 的 `-3` 夹具，未改共用输入映射。旧三 tick 场景的独立 `legacy-three-tick-regression-20260925-1` runner PASS，输出 header 加 tick 1–3。原 Battle Scene 自然物理键 Play 是另一份见证，起点不同且抽样漏 tick 1/3；本次同初态 Manual 结果不替代自然 Play、Renderer 借用数或像素验收。
