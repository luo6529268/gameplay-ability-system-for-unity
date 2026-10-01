# NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001

状态：VERIFIED（仅正式源/根的受控正反完整tick）。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C050；原Unity和自然物理键仍待。

权威与前提：用户选定 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根 EXE 与对应 playable 源。`battle_world.cpp` 的 `native_effect_action_override_is_suppressed` 读取目标锁存帧首个 BDY kind50/52；`resolve_confirmed_unarmored_standard_hit` 据此跳过普通垂直反应。正式 catalog 的 OID56 `c\hid\rea.dat` action259 首 BDY kind50，OID24 `c\hid\hid.dat` action38 的 ITR kind0/effect1/dvy-5。当前 Unity `BattleNativeOrdinaryHitPrelude.IsSpecialLinkRestGate` 已用于 rest，而 `BattleDamageWriter.ApplyStandardFall` 未显式接同门；这仍是静态疑点。

范围：新增 `Tools/NTSD28Q07Diagnostics/hidan_bdy50_vertical_lfr_probe.cpp` 一项诊断脚本和新诊断输出。正式 GameSession28、mode0、seed682973786、OID24/action37/X500 攻击 OID56/action259/X近距与远距，中性输入，最多40完整 tick。逐 tick 记录锁存首 BDY、标准命中/垂直响应、目标速度/动作及 LFR，判断是否同 tick 真实触发 C050。正式源、DAT、Unity 生产/测试、Scene、非战斗均不改。

验收：编译0错；同输入双跑诊断输出逐字节相同；近距必须有正式完整 tick 的 `applied` 普通命中、锁存首 BDY kind50 且 ITR 非零 dvy 才能晋升根 EXE/Unity 对照；若阴性只标有界排除，不据静态线索改代码。远距作无碰撞控制。首差之后另建生产 Task/Change，不能在本 Task 内扩修。既有未提交资产全部保护，回滚本新增脚本及输出需遵守仓库删除批准规则。

出口：近X520/X550第2tick均满足 applied kind0/dvy-5/首BDY50，正式源码垂直累计0、目标HP465/Vy0；远X1200无命中。各双跑CSV/LFR同SHA，根X520与X1200同LFR各40tick 200/200选定字段相同。仅限定受控初态与选定字段；原Unity后继独立处理。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-HIDAN-BDY50-REACH-001/REPORT.md)。
