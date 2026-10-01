# C053 正式双 producer 的原 Battle Scene 限定验收

状态：`VERIFIED_SCOPED_NATURAL_SCENE / C053_OPEN`（2026-10-02）。本包只证明安科与自来也**受控初始动作**后的正式 OPoint 生成、单次 Uj 命中及选定状态，在原 Unity Battle Scene 的生产 Driver 与当前 336B44 正式源码同态；没有证明玩家物理键自然选招、双 Uj、全 World 字段或 C053/Q07 整项完成。

权威：根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、其 playable `GameSession28::step` 和正式 `resources/runtime`。同一源码案例为OID65安科slot0/team1、action511、X610/Y0/Z400；OID702自来也slot1/team2、action553、X500/Y0/Z400；seed682973786、mode0/difficulty0、MP500、中性后续输入。源码自然 OPoint 于tick1生OID808/action150、tick4生OID875/action50、tick5进55，tick7记录一条 `51:0:2:156` 的 `applied/effect2/Uj156`、目标HP475。源码CSV和根LFR、40tick×12字段480/480的根回放证书见[producer报告](../NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/REPORT.md)。

只新增[请求式 Editor 探针](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalSceneProbeEditor.cs)及唯一meta。原项目唯一 Editor PID105896、Battle Scene clean/单场景时，经已有本机 Unity-MCP bridge 刷新导入；Editor DLL晚于探针源码且含类型。生成 Editor C# 工程以仅用于本探针、只在 `Assembly-CSharp-Editor` 生效的 Temp targets 明确纳入脚本，`dotnet build ... --no-restore -v:q -clp:ErrorsOnly` 最终0错误、249警告。首版targets误进引用项目产生70个非目标工程的命名空间错误，限定工程后解决；未改生成工程或启动第二Editor。

首轮[保留原件](ank610-jira500-natural-scene-01.json)是探针前置错误：误把根trace `facing=false` 映射为 Unity `SwitchDir("left")`，tick1主角与子体水平位移方向相反，八tick结果`FIRST_DIFFERENCE`、已退出Play/Scene clean。只改探针初始朝向和唯一RunId，不改生产/DAT/Scene。第二轮[原Scene结果](ank610-jira500-natural-scene-02.json)在生产 World 的完整Driver跑8tick，`SCOPED_PASS / DONE`、OID808 tick1与OID875 tick4自然出生，无人工建875；tick7目标action156/HP475、攻击者rest10，tick8 rest9。Play已退出且Scene clean。

[独立逐字段比对](ank610-scene-v2-source-rng-comparison.json)：8tick中正式源码CSV的15个实体字段每tick、根trace的两主角源X与OID808 HP共144项，另按当前 playable `NativeRandom28::reset_from_seed` 的 MSVCR80 递推和当前源初始seed核五个RNG标量每tick共40项；总 **184/184 零差、首差无**。Unity探针没有逐hit事件输出，不能由目标末态/rest反推内部逐hit日志与源码完全一致。

随机流边界：根LFR回放的 `GameSessionLfrPlayback28::load` 将 `config_ = {}`，`BattleConfig28.random_seed` 默认0；LFR恢复同步随机表，但不编码/恢复源码案例的随机种子682973786。因此根trace CRT tick1～6为3374725112，Unity/当前源码种子递推为1758127634；tick7分别2015516220与1334545526。两条均经历3000初始CRT调用和tick7新增4次，其他选定实体状态仍匹配。把根trace CRT与源码seed的Unity状态直接比较是**回放载体初态不等价**，不能据此判Unity RNG错误，也不能把根回放报告扩张为全随机流同态。这个边界只读定位，未修改根EXE、DAT或Unity生产。

退出复核：原Editor回非Play、Battle Scene clean；Battle、Menu、GameConfig、ProjectBattleModeConfig四项SHA分别为`93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与本包前一致。无 DAT、图片、Scene、Prefab 或非战斗逻辑修改；未重跑全案例，因为本包只新增测试探针。后续 C053 应找第二次 Uj 的正式自然同态或可比逐hit内部证据，再决定生产逻辑是否需要改。

交付检查：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` exit0/PASSED，1116 Records、62个当前差异代码文件，详[输出](ledger-validation-v1.txt)；本包已跟踪文档 `git diff --check` exit0。之后本机Bridge再读原Editor为非Play且 `NTSD_Battle` active/clean。旧Record的未在当前diff警告不影响本包覆盖结论。

对齐总表 C053 当前行补入自然单Uj出口后又运行一次[最终validator](ledger-validation-v2.txt)，exit0/PASSED、1116 Records/62代码文件；该文档 `git diff --check` 仍exit0。
