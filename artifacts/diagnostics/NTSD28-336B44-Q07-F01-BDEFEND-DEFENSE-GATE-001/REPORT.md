# Q07/F01 普通防御门槛字段

状态：`F01_BDEFEND_FIELD_GATE_VERIFIED_SCOPED`；Q07 整组未关闭。

正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的 playable 闭包含 `defense_resolution.cpp`。`DefenseResolver28::match_ordinary` 在 kind0 分支检查 `interaction.bdefend >= 61`，再检查 state7/70/75、HP、朝向、spark、dbdefend、dvx 和双向防御 OID。`BattleWorld28::classify_confirmed_defense` 从实际 ITR 传入该规则。正式 `resources/runtime/decoded_dat/c/gur/a/cag.dat` 与 Unity 暂存副本 SHA-256 都是 `4D1E6DB5D1B963B685AB4A9A244911E75BAE390514B1DF5CCFC5EBF6512CE3C6`；其中 kind0 ITR 有 `effect=1、bdefend=61`，说明两字段不同的内容实际存在。本包未改 DAT。

Unity 原先 `BattleOrdinaryDefenseResolver.Resolve` 用 `interactionEffect>=61` 当防御门，两个生产调用者都传 `itr.effect`。原 Editor 两个聚焦反例按新版预期运行时均失败：[RED job](red-focused-job.json) 的 `failures_so_far` 记录 `effect61/bdefend0` 错误禁用防御、带护甲时错误选择 `ReducedType1Armor` 而非 `ReducedDefense`。首次测试 job [启动超时](pre-red-init-timeout-job.json) 未执行任何案例，不计为规则失败。

修复只把共用 resolver 第二参数的意义改为 bdefend，并让两个生产调用者传 `itr.bdefend`；原 Editor 新运行时/测试程序集编译后，两个精确案例 [2/2](green-exact-job.json)、相邻纯 resolver/减伤/护甲路由 [18/18](green-defense-adjacent-job.json) PASS。归档 job 是域重载后再查询的终态，仍含 `status=succeeded`、`progress.completed=2/18`；先前即时查询还返回 `result.summary.passed=2/18`。`effect` 仍由其他效果分支按原职责读取，未改护甲/伤害公式。

完整 `BattleRuntimeSelfCheck` 随后运行到 `CheckAlternateHurtTriggerMatrix` 的旧 `effect61 must be outside ordinary defense` 断言并 [返回 FAIL](pre-selfcheck-old-effect-gate-failure.txt)。这是自检的旧字段契约，与新版正式门槛和已通过的生产聚焦结果矛盾。按同一 Change 范围，只将该断言改为 effect61/bdefend0 保留防御，并加 bdefend61 关闭防御的反例；改后的原 Editor [新鲜自检返回 PASS](post-selfcheck-pass.txt)，文件时间 2026-09-30 15:51:30 +08:00。改前已有的 PASS 另存 [对照](before-f01-self-check-pass.txt)，未被当作改后证明。

后续[正式根自然命中见证](../NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001/REPORT.md)已在未改 DAT 的 Guren action150→OID619/action304 链上观察到 tick12 `bdefend61` 接触，冻结根 EXE 同 LFR 的可观察 hit 为 `applied/hpDamage50`，源/根声明20 tick指定字段120/120一致。根 trace 不直接打印 bdefend，内建 `nativeParityClaim=false`。原 Unity Battle Scene 同内容自然接触 Play 未验，不能把 F01 报为完全对齐。

随后[原 Battle Scene 限定 Play](../NTSD28-336B44-Q07-F01-GUREN-CAG-SCENE-PLAY-001/REPORT.md)已完成：相对 tick11 CAG 出生、tick12 Lee HP500→450/action186，20 tick 六字段与正式源 120/120 无首差，Editor 退出 Play 且 Scene clean。此后 F01 所声明的 `bdefend` 字段门首差可以记为限定 `VERIFIED`。以上前一段“Play未验”是该 Play 子包执行前的历史状态；不代表全防御分支、物理按键或 Q07 整组完成。

最终账本校验 `Tools/Validate-ChangeLedger.ps1` PASS（1049 records / 当前 diff 9 个受管脚本）；`git -c core.safecrlf=false diff --check` PASS。正式根 EXE、Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 的 SHA-256 均与本包前受保护基线相同；原 Editor 已退出 Play 且 ready。
