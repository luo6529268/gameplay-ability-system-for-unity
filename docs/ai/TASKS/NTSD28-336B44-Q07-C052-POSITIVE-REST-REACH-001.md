# Q07/C052 effect21 正间隔后继候选可达性

> 2026-10-01 最新：`VERIFIED / SCOPED_SOURCE_ROOT_POSITIVE`。完整 `GameSession28::step` 的 OID211/action161 及160→161 受控初态均出现“首目标 applied → 同目标正 rest 拒绝但不终止 → 第二目标 applied”；每案双跑 CSV/hits/LFR 逐 SHA 相同，远距第二目标无命中。根正式 EXE 三实体 LFR 近远报告均通过，选定动作/HP与源码30/30零差；原 Unity Scene 已取得修复前首差和修复后近远42/42同态，见独立包。自然 OPoint 链仍待，父 C052/Q07 开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/REPORT.md)。

> 以下 `PLANNED` 为脚本前历史快照。

状态：`PLANNED / SOURCE_FIRST`。父 C052/Q07 与总目标开放。权威：根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与当前对应 playable live source；只读取正式 `resources/runtime` 内容。用户要求按新版总表 G1 顺序继续战斗首差，不改 DAT 数值、Unity 框架或非战斗功能。

已知静态前置：正式 `battle_world.cpp` 的 `classify_ordinary_hit_eligibility` 仅在 effect21、目标当前 state18/19 且 `victim_rest==0` 时终止本攻击者；Unity `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 的相同提前终止未检查 rest。正式 OID211 `a/fir/fir.dat` 的 action161～166、232 各含左右两条 effect21 ITR，`c/nar/nar.dat` action203 为 state18。候选收集先于消费，`vrest:300` 使用多候选缓冲；以上仅给出可达性假设，不得当运行首差。[前次只读核查](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-STATIC-REBASELINE-20261001/REPORT.md)。

唯一脚本范围：新建 `Tools/NTSD28Q07Diagnostics/effect21_rest_reachability_probe.cpp`。用正式 28 Core+playable 当前源码重新链接有界诊断，不改权威源码、Unity生产/测试、DAT、图片、Scene 或资源。先用同一 OID211/action160→161 或161 与两名 OID2、mode0、seed682973786 的完整 `GameSession28::step` 测近距/远距几何；记录逐候选攻击者、目标、ITR index、status、当前动作/state、rest、是否终止本攻击者及后继目标HP。各输入双跑，保留命令、编译参数、输出 SHA。若无正间隔后继阳性，仅报告本批有界阴性并筛下一正式入口；不改 Unity。若阳性，再独立对照正式根可载入条件和原 Unity 生产路径，确认 first difference 后另建生产修复 Task/Change。

验收：诊断编译0错、正式DAT与脚本未修改、案例输入与输出可重放且双跑同SHA；精确区分“初态为正式数据动作的受控完整 tick”和“角色技能自然生成子体”，不得把前者写成自然物理键证书。不会为不满足条件的测试合成 DAT 或强造生产分支。风险仅诊断输出占用新路径；输出用唯一新目录，不覆盖旧证据。回滚仅诊断新文件，任何删除/覆盖前按文件操作合同记录并取得适用授权。

2026-10-01 根正式 EXE 增补：原三实体LFR近X530、远X650两例均进程退出0、报告passed/failureCode0、trace tick1～3的攻击者动作与两目标动作/HP各15字段、总计30/30同源码。此前root pending段为旧快照；自然角色→OPoint待。详原报告。
