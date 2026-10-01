# Q07/C052 Hayate → OID417 → OID211 自然 OPoint 限定证据

状态：`VERIFIED_SCOPED_SOURCE_ROOT_OPPOINT / UNITY_AND_INPUT_SELECTION_PENDING`。正式336B44 DAT/catalog：OID73 Hayate 的 `c/hay/hay.dat` action162生成OID417/action49；OID417的 `c/hay/a/sla.dat` 49→50→51→52→10→11，action11生成OID211/action160。本包以受控**初始 action160**进入162，再由完整 `GameSession28::step` 自然生成两层 OPoint；尚未证明玩家物理按键如何选到action160，也未证明原Unity Scene同链。

唯一新增[源码诊断](../../../Tools/NTSD28Q07Diagnostics/hayate_fir_natural_reachability_probe.cpp)只读正式 `resources/runtime`、mode0、seed682973786、Hayate/X500/Z400与两名OID2/Z400、中性输入。当前正式28 Core+playable编译参数见[argv](compile-argv-v2.txt)，`g++` exit0/stderr0，候选EXE SHA256 `9C5A0A35889FF50709AC7126ACA736B98AF8D216D34E5A6EB7A1C59611C51031`，不等于正式根 EXE。

首轮直接从action162起、目标请求X1700/X1730，120tick双跑三个输出各自同SHA，但目标实际被正式边界钳到X1330且OID417始终未出生。这个样本只证明“直接初始162 + 边界外目标”阴性，不证明静态链不可达。[首轮原件](far-x1700-x1730-v1/source-ticks.tsv)。改用正式前驱action160、边界内目标X1200/X1230后，OID417 tick4出生、OID211 tick20以X589/Z402出生；远距两目标无命中。两次120tick的tick/hit/LFR各自同SHA。[远距原件](a160-x1200-x1230-v1/source-ticks.tsv)。

目标收窄到X589/X619后，两次60tick的tick/hit/LFR各自同SHA；OID417 tick4出生、与第一目标接触后于tick22 action11生成OID211/action160，子体tick23进入161。**tick24自然双目标阳性**：OID211/slot51对目标1/slot1首条 effect21 applied，第二条对同目标因relation rest拒绝且`terminates=0`，第三条对目标2/slot2 applied；两目标HP均500→420、action203。[近距tick](a160-x589-x619-v1/source-ticks.tsv)、[逐hit](a160-x589-x619-v1/source-hits.tsv)、[LFR](a160-x589-x619-v1/source-packets.lfr)。

根正式 EXE SHA-256 本轮复核为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，直接用上述近距三实体LFR回放两次，进程均exit0，报告`passed=true/failureCode=0/declaredTicks=60`。首次根调用未覆盖slot1初始朝向，根trace该目标向右而源码初态向左，逐tick 19选定字段×60＝1140项中193项不同，首差tick6；保留[根v1报告](root-near-v1-report.json)与trace，不能将`passed=true`写成同态。第二次明确`--lfr-slot1-facing 1`（slot0面对右），修正首目标朝向后，根trace的角色动作、目标HP/X、OID417与OID211的数量/槽位/动作/XYZ共**1140/1140选定字段零差**，tick24根事件顺序同为applied/rejected/applied且两目标HP420。根LFR未覆盖slot2初始朝向；源码该目标设为左，根默认右，因此完整初态和全状态同态未证。[根v2报告](root-near-v2-report.json)、[根v2 trace](root-near-v2-trace.jsonl)、[逐字段比较](comparison-near-v1.json)。根报告自身`nativeParityClaim=false`，1140/1140来自独立字段比对。

本包确认正式角色**帧链进入后的两层OPoint自然出生**及其双目标effect21候选，可作为原Unity Battle Scene同条件验收的入口。它不证明从物理按键选择action160、逐hit内部所有字段、全World或画面/音效。Unity生产/DAT/图片/Scene/配置均未修改；父C052、Q07、总目标仍开放。下一独立Scene包应在原项目用OID73/2/2三人同源坐标、action160、seed/mode/difficulty相同的完整Driver逐tick比较，先检查tick4、22、24，再核退出与四保护SHA；若Unity首差出现，按共享所有权另立修复任务。
