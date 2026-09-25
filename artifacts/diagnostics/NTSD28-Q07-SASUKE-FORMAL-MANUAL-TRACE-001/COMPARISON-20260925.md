# Q07 佐助千鸟千本同初态逐 tick 对照（2026-09-25）

正式发行端是根目录 `NTSD2.8-Logan.exe`（SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`）的受控 LFR 回放。原项目 Unity Editor 在已暂存正式非排除内容与项目 `ProjectBattleModeConfig` Asset 下，使用独立 `ntsd28-q07-sasuke-needle/1.0` 夹具连续执行 Manual tick 1–26。双方初态为 OID11/action110 `(500,0,350)` 对 OID7 `(1200,0,350)`；输入物理 L/D/J 对应防御/向右/攻击，按两 tick 保持，在完成 tick2/4/6 生效。来源 LFR 的 `nativeParityClaim:false` 保持，不把受控回放当成独立原生录制。

## RED 与修复

原 Editor 首次请求 `manual-sasuke-20260925-1` runner PASS、26 完成 tick、每 tick 活跃 slot 集合与正式 trace 相同。按 native `tick`／Unity `completedTick` 和实体 slot 联接，25 个已列实体字段 ×100 行＝2500 次比较，**12 处首差**：tick24–26 的 slot50–53（均 OID440/type3/state15）的 X 速度正式为 0、Unity 为 550。tick24 为 action12，tick25/26 为13/14；位置、其他两轴速度与所检动作保持相等。对应正式 `c/sasu/a/chi.dat` 原文各帧 `dvx:550` 未改。正式 `FrameMotion28::apply` 对 `dvx>500` 减 550，Unity `BattleNativeFrameMotionKernel.Apply` 也已经换算；Unity 随后的 `LF2SpecialAttack.RunPostNativePhysicsSerialForWorldPass` 又调用旧 `ProcessState15TU`，把原始550写回。这是通用 state15 原生 World 路径的后物理重复写入，不是 OID440 专用数值问题。

独立 Change `NTSD28-Q07-STATE15-POSTPHYSICS-VELOCITY-001` 仅删去该后物理调用，保留旧 `TUEvent` 和帧运动核。原 Editor 重新编译后，唯一请求 `manual-sasuke-20260925-2` runner PASS、26 tick 连续、每 tick 活跃 slot 一致；**100 实体行 ×25 字段＝2500/2500 相等**。列明字段为 OID、类型、action、state、frameCounter、facing、整数及精确 XYZ、速度 XYZ、当前/基础最大/有效最大 HP、MP、weapon HP、复活次数、render phase、previous action、owner、team。四个 OID440 在 tick15 按 slot50–53 出生；其后帧、位置、速度及持续占槽至 tick26 都在上述已列字段范围相同。

输入对照为 **100 实体行 ×13 字段＋26 个全局相位＝1326/1326 相等**。字段是 bound state、last action、remap state/indices、proxy tail、run accumulator、global record state、defend reentry cooldown、combo state、key history、current/previous mask、七键 edge window。对 raw mask 显式做物理语义映射：Unity 代理右键位1→正式位8、下键位8→正式位2；edge window 字典按 `up,down,left,right,attack,jump,defend` 排序。未映射前有四个右键 mask 数值差，不是生产按键差异。实体与输入合计 **3826/3826 已列字段相等**，并不覆盖全部世界状态。

正式 LFR 初始 CRT state `3374725112`，Unity 显式 seed 夹具为 `2461722130`，与 DDJ 报告中的回放入口差异相同。同步表哈希同为 `58181F48CC1F3BB5`；这 26 tick 内双方 CRT／同步随机新增调用均为零。完整 RNG／checksum 证书仍缺。旧鸣人 DDJ 夹具在本次生产修复后以新唯一请求 `manual-ddj-20260925-4` PASS；它与修前 `-3` 的 raw/domain/input-RNG 的 26 个 tick JSON 行逐行相同（header 可因程序集身份不同），说明此次改动没有改变该相邻技能的已观测链。保存的 Battle/Menu Scene 与 GameConfig asset 磁盘 SHA 均保持原值。Renderer 借用数、自然同世界物理键 Play、碰撞后段、像素、音频及 Q07 聚合出口仍需另验。

| 输入／结果文件 | SHA-256 |
| --- | --- |
| 正式 `../NTSD28-Q07-SASUKE-NEEDLE-RELEASE-PLAYBACK-001/formal_release_explicit_vfs_trace.jsonl` | `2450850DC5D2548E8904DD2C815146B550D6708EF5172807B545D44E9DAFD957` |
| `sasuke-needle-formal-26-physical-ldj.json` | `582E930C489CE32BE319512BA9DE1AF4B48F123AB675AB14E7D9E4029F854B18` |
| RED `manual-sasuke-20260925-1.raw.jsonl` | `2F4406232405A57EC9928C462F08D666391199DC819EAC8F86913198E64EDAF2` |
| 修后 `manual-sasuke-20260925-2.raw.jsonl` | `6271D9D08DC1F3BF659DE5622BB3880C04E6CFD2D267D5836AA6AB653E535A9E` |
| 修后 `manual-sasuke-20260925-2.input-rng.jsonl` | `26B5B8490E110F5A32F2FEF53A25B74F242F8FBFC0794CDD7F23ADF6E7C4E05E` |
