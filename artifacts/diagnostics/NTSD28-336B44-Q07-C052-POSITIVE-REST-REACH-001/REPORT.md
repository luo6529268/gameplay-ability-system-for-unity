# Q07/C052：effect21 正间隔后继候选正式源码受控阳性

状态：`VERIFIED_SCOPED_SOURCE_ROOT / NATURAL_ENTRY_PENDING`。父 C052、Q07 与总目标开放。当前根正式 EXE 已在同三实体 LFR 受控初态通过近远两例；重建诊断 EXE 仍只作为源码证据，受控初态仍不是自然玩家按键或正式 OPoint 出生。

当前正式 336B44 playable 的 `BattleWorld28::classify_ordinary_hit_eligibility` 在 `battle_world.cpp:5007-5015` 只于 effect21、目标当前 state18/19 且 `victim_rest(target,attacker)==0` 时终止本攻击者；同文件 `:6574` 的普通命中间隔门可将候选拒绝而不终止。正式 `a/fir/fir.dat` OID211 action161～166、232 各有两条 effect21 ITR、`vrest:300`；正式 `c/nar/nar.dat` OID2 action203 为 state18。Unity 当前 `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 的 effect21 提前终止门未直接读取 rest。以上是权威源码与当前 Unity 的静态条件差，是否表现为真实首差仍需 Unity 生产运行。

新建唯一[源码诊断](../../../Tools/NTSD28Q07Diagnostics/effect21_rest_reachability_probe.cpp)，以正式 `resources/runtime`、mode0、seed682973786 在完整 `GameSession28::step` 中设置 OID211/action161 或160/X500/Z400 与两个 OID2/action0/X500、X530。当前 28 Core + playable 源码重新链接编译 exit0、0诊断；[编译参数](compile-argv-v1.txt)、[编译输出](compile-output-v1.txt)和候选 EXE SHA 见[比较原件](comparison-v1.json)。每案三 tick、中性输入，两次独立运行；`source-ticks.tsv`、`source-hits.tsv`、LFR 三文件各自 v1/v2 SHA 全部相同。

| 受控初态 | 阳性 tick | 正式源码消费顺序 | tick 后结果 |
| --- | ---: | --- | --- |
| OID211/action161 | 1 | slot1 effect21 applied；slot1 第二条 effect21 因 relation rest 拒绝、`terminates=0`；slot2 effect21 applied | 两目标 HP 各 500→420；目标1 action203、rest44 |
| OID211/action160→161 | 3 | 同样为 slot1、slot1、slot2；status `applied/rejected/applied`，终止位均0 | 两目标 HP 各 420；目标1 action203、rest44 |
| action161、第二目标 X650 | 1 | 首目标仍命中且第二候选受 rest 阻挡；第二目标无重叠命中 | 第二目标 HP500 |

具体原件：[action161 首跑 tick](a161-x500-x530-v1/source-ticks.tsv)、[逐候选](a161-x500-x530-v1/source-hits.tsv)、[action160 前驱帧 tick](a160-x500-x530-v1/source-ticks.tsv)、[逐候选](a160-x500-x530-v1/source-hits.tsv)、[比较与 SHA](comparison-v1.json)。`battle_world.cpp` 的分支顺序连同第二候选消息“target relation rest currently blocks this attacker”支持：第二候选进入消费时目标已是受击后的正 rest，故正式源码不终止后继目标。诊断输出只在 tick 尾直接导出 rest44；第二候选当刻的数值由源码分支、先后顺序及结果推定，未作为单独逐候选字段采样。

2026-10-01 根正式 EXE 增补：已重新核其 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。根 LFR 回放实际接受本诊断的三实体录包；近 X530 和远 X650 均进程退出0、报告 `passed=true/failureCode=0/declaredTicks=3`。以根 trace tick1～3 对源码 CSV 的攻击者动作、两目标动作与HP共每例15、两例**30/30字段零差**：近距tick1双方HP420，远距第二目标HP500。[近距根报告](root-a161-x530-v1-report.json)、[近距根trace](root-a161-x530-v1-trace.jsonl)、[远距根报告](root-a161-x650-v1-report.json)、[远距根trace](root-a161-x650-v1-trace.jsonl)、两个 `root-...-argv.json`。根报告自身注明 `nativeParityClaim=false`；30/30是独立逐字段比较，不从 `passed=true` 自动推出完整同态。先前“根只配置两名、三实体能力待核”为旧快照，已由实际重放覆盖。

当前限制：OID211 在本诊断中作为受控初始 slot0，`action160→161` 是正式 DAT 自然帧推进；这还不是正式玩家角色通过 OPoint 自然生成 OID211 的链。正式 DAT 的 `c/hay/a/sla.dat` 确有 `opoint oid:211 action:160`，但此父对象的自然入场与双目标条件未运行。原 Unity Scene 已另取修复前首差、共用门修复后近远各3tick与源码选定42/42字段同态；见[首差与验收](../NTSD28-336B44-Q07-C052-UNITY-SCENE-001/ACCEPTANCE.md)。根仍未导出每个候选的内部状态或 relation rest 数值，因此根的30/30不等于逐hit内部同态。

下一出口：若要将 C052 从受控条件提升为自然战斗验收，需跑正式角色→父对象→OID211 OPoint出生→双目标的可达链及原Scene相同输入；完整 World、物理键和逐hit内部字段仍未证。本源码诊断包本身未改 DAT 数值、图片、场景、非战斗或 Unity生产；生产单点修复记录于独立消费者 Change Record。
