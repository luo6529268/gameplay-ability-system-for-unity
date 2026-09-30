# Q07/F02 OID600 可控完整 tick 对照

状态：`UNITY_FOCUSED_PASS / ROOT_EXE_AND_NATURAL_PLAY_PENDING`。此报告只针对新版正式资源中的 OID600、frame0/state1000、X 速度阈值；F02、Q07 和总目标均未关闭。

正式根 `NTSD2.8-Logan.exe` 的 SHA-256 是 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 `resources/runtime/decoded_dat/w/6.dat` 与 Unity 暂存同路径 DAT 的 SHA-256 都是 `2641D21E09CD222B50036559496D266B73037BB7F3CF23F7B5A6590FCE5C118A`。本包不改 DAT 或正式源码。

诊断 C++ 探针在所选 playable 源码闭包上编译成功；它在 `GameSession28` 的 Naruto/Lee 对局中，将 OID600 置于 slot50、X500/Y-20/Z650、frame0，设置各组初始 Vx 后调用一次 `GameSession28::step()`。结果见 [原始 CSV](source-full-ticks.csv) 和 [编译参数](compile-argv.txt)。源码产生的局部对象结果如下；这个自行编译的探针不是正式根 EXE 的直接运行证据。

| 初始 Vx | 帧事件 from→to | 最终帧 | 整数 X | 精确 X | Y | 最终 Vx/Vy |
| ---: | :---: | ---: | ---: | ---: | ---: | :---: |
| -20 | 40→41 | 41 | 476 | 476 | -20 | -20 / 0.85 |
| +20 | 40→41 | 41 | 524 | 524 | -20 | +20 / 0.85 |
| -9 | 0→1 | 1 | 489 | 489.2 | -20 | -9 / 0.85 |
| +9 | 0→1 | 1 | 510 | 510.8 | -20 | +9 / 0.85 |

Unity 原 Editor PID11944 经刷新编译后，在同一暂存 DAT、相同 OID600 局部初态下运行生产 `NTSDBattleTickSystem.RunReleaseTick`；[首次完整 tick job](unity-full-tick-job.json) 4/4 PASS，逐例核对最终动作、源规则整数/精确 X、Y 与 Vx/Vy。随后把完整背景参考宽设为 2048、按同一投影放置对象，原 Editor [最终完整 tick + 投影 job](unity-full-tick-and-projection-job.json) 5/5 PASS：原版规则 X 在 +20 案例仍为 524，Unity 物理 X 等于统一投影 `524 × (2048/1333)`。这是完整生产 Driver 的局部行为和该投影入口的一个聚焦正例；早先 50/50 直接物理聚焦测试和全量 `BattleRuntimeSelfCheck` 新鲜 PASS 另见 [F02 报告](../NTSD28-336B44-Q07-F02-FAST-WEAPON-ACTION-001/REPORT.md)。

正式探针含远处 Naruto/Lee，Unity 夹具只注册 OID600，故两者不是整场同态，也没有正式根 EXE 对同初态的直接 trace。正式 LFR 初始槽不保存 Vx，不能用编辑回放覆盖项伪造上述 ±20 初态。正式根自然生成/投掷 OID600 和原 Battle Scene Play 仍未验；单个 2048 宽投影案例也不能代替其他实体、武器和边界的全域比例验收。
