<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053DoubleUjBattlePlayProbeEditor.cs
authority: selected 336B44 playable four-combatant OID702 2 875 875 GameSession tick7 type3 latched Uj source witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001.md
-->

# C053 原 Battle Scene 双命中分读探针

脚本前登记。当前Unity C053共用解析/Editor聚焦2/2已存在，源完整Driver受控四人于tick7有分读鉴别阳性，但原Battle Scene和根四人回放均未证。本包只增加Editor请求式测试探针，不改战斗生产、DAT、资源、Scene、Prefab、GameConfig或非战斗。

原状/预期：原Battle Scene可用的BattleTestBootstrap与Production Driver/World负责自然OID702 OPoint出生；`LogicEntityFactory.Create`用于预tick准备正式OID875两实体，只有在确实获得正式内容、对应slot/source坐标、两条受害者rest和预期动作时才报告通过。复用已在自然Scene探针证实的暂停稳定门、同seed重置、显式StepOneTick和退出清理。生成工程早于新脚本，因此仅用 Temp targets 把该脚本纳入离线 Editor 编译，不能将此当成 Unity 导入。若工厂/容量/编译拒绝，保留首个结果不升父状态。风险为原Editor中的运行时临时实体与场景dirty；退出后四SHA、借用、非Play核验。完整验收和回滚见Task；受治理脚本以 metadata 的 `.cs` 路径为准，`.meta` 与 Temp targets 是本Record正文及Task声明的附属文件，修补范围变化必须先更新本Record。

2026-10-01 实际写入：新增请求式Editor探针及唯一GUID `.meta`、单文件Temp编译targets。探针仅在原项目、Edit、clean且唯一Battle Scene时消费请求；别的项目或正在运行的场景不会消费请求。受控角色OID702/2和两个正式OID875从生产工厂进入同一World，八次完整Driver tick记录OID808帧、锁存、位置、两攻击者动作、各自victim-rest和RNG；任何前置不符保存失败，不改生产。`rg`确认GUID只出现一次。生成Editor工程时间早于脚本且未列入新脚本，所以直接生成工程的旧成功不可作本探针证据。运行 `dotnet msbuild Assembly-CSharp-Editor.csproj -getItem:Compile -p:CustomAfterMicrosoftCommonTargets=<Temp targets>` 确认纳入新文件；随后同targets的 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功，277警告、0错误。此为**离线编译**；原Editor导入、聚焦Play、源逐tick比对、退出后四SHA与借用仍待。原Editor PID11944存在但标题 `Hold on (busy...)`，其MCP状态虽显示项目路径但心跳/重载状态不可作idle凭证；当前连接返回另一 `I:/UnityPreject/test` 项目的Play状态。未切场景、未发请求、未启动第二Editor。状态保持 `CODE_WRITTEN`。

2026-10-01 运行续证（覆盖上一段的待验快照）：对原项目实例6401完整Asset刷新后，Unity导入新脚本GUID，Tundra脚本构建成功、Editor DLL包含新类型；生成 `.csproj` 也列出该文件。原Scene唯一请求在非Play、clean下消费并完成8个生产Driver tick，结果`SCOPED_PASS / DONE`、已退出Play/Scene clean。源CSV/RNG与Unity JSON **13字段×8tick＝104/104相同**；tick7源两次命中响应156，Unity两攻击者各自victim-rest从0→10、子体动作/锁存156、RNG同态。四保护SHA稳定，近期Editor日志本探针与C#异常标记0；`& ./Tools/Validate-ChangeLedger.ps1`在本包脚本纳入后通过1087 Records/31 governed code files，历史非diff记录有大量WARN但本ID无ERROR。详[场景报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001/REPORT.md)。未导出对象池借用数，根EXE四人回放载体与物理键自然入口仍未证；父C053、Q07、总目标不关闭。本Record标`RUNTIME_PENDING`，表明受控Scene已通过但本Task的完整退出合同未满足。
