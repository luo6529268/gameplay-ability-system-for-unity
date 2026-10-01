# Q07/C052 effect21 正间隔共用门限定验收

状态：`SCOPED_SCENE_PASS / ROOT_AND_NATURAL_PENDING`。按当前 336B44 playable 的 `battle_world.cpp:5007-5015`，共享 `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 仅在 effect21、目标当前 state18/19 且其对攻击者的 relation victim rest **等于零**时结束攻击者其余候选。改动只有这道共用条件与合同注释；正式 DAT、角色图、Scene/Prefab、配置 Asset 和非战斗脚本未改。

修复前正式源完整 `GameSession28::step` 的 OID211/action161/X500 与两名 OID2/X500、X530 在第1 tick HP均500→420、rest44；原 Battle Scene 生产 Driver 同条件第1 tick 仅第一目标HP420/rest44，第二目标HP500/rest0。远距X650对照第二目标HP500。[修复前首差](REPORT.md)、[正式源码原件](../NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/REPORT.md)。

修复后原 Editor 编译完成，`Assembly-CSharp.dll` 与 Editor DLL 更新时间晚于改动脚本，最近导入日志无新 `error CS`。近距和远距各独立 Play 3个完整 Driver tick，在动作、HP、relation rest、攻击者动作共 **每例3×7=21字段，两例42/42** 与正式源码 TSV 相同，零差。近距首tick双目标HP420/action203/rest44；远距第二目标3tick始终HP500/rest0。[近距v2](x530-scene-v2.json)、[远距v2](x650-scene-v2.json)。

相邻零rest门槛：正式 OID2 第一目标预置 action203/state18、rest0 时，原Scene第1 tick 第一目标HP500/rest0，第二目标对 OID211 的 relation rest0，说明 OID211 未越过零rest终止门。[零rest v2](zero-rest-scene-v2.json)。首轮零rest v1将第二目标HP不变误作断言，实际该目标HP470；正式 DAT action203 自带effect20 ITR，因此该HP变化**可能**来自第一目标自身的攻击，当前未逐hit确认。v1 `FIRST_DIFFERENCE` 是测试断言过强，保留原件，不归因生产门；v2仅核对能识别OID211命中的relation rest和第一目标HP，不把第二目标HP470算为正式EXE同态。

三次修复后 Play 均`DONE/SCOPED_PASS`、退出Play、Scene clean，Menu/Battle/GameConfig/ModeAsset四SHA每次前后全等；Battle Scene SHA始终`93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`。`git status --short -- Assets/NTSD/Content/LoganRuntime` 无变动，Battle Scene原有M未清理或覆盖。请求文件最终`requested:false`，历史请求payload已复制到独立原件；未删除文件。用户指定无需为通用逻辑重跑所有案例，本包只测有分辨力的正、远、零rest三例。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` PASS（1107 Records、50 governed code files），`git diff --check` 对本包脚本与Record无差异警告。

随后根正式 EXE 用同三实体 LFR 分别重放 X530/X650，进程均退出0、报告 `passed=true/failureCode=0`，根 trace tick1～3的攻击者动作、两目标动作和HP对源码CSV每例15、合计30/30字段零差。近距根tick1双方HP420，远距仅首目标HP420；[根原件](../NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/REPORT.md)。根报告自身 `nativeParityClaim=false`，此处同态来自另行逐字段比较。

限定：自然角色→OPoint出生、物理键以及完整World回归仍待，C052/Q07/总目标保持开放。零rest对照没有当前正式源码同初态逐hit原件，只作原规则未退化的Unity聚焦验证。源/Unity两组从不同 slot 顺序映射相同目标角色，42/42仅选定可观察字段，不宣称逐 slot 全状态一致。
