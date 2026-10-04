# C053 辅助 type3 原 Battle Scene 定向验证准备

状态：`COMPILE_PASS / UNITY_RUNTIME_PENDING`。这是当前336B44正式源/根[同tick双命中阳性](../NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)的Unity原Scene出口准备；**尚无新案例Unity Play结果**。

只扩既有 `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 的独立请求 `Temp/NTSD28_Q07_C053AuxType3.request.json`、runId `ank610-jira500-firz700-yminus60-01`，输出写入本目录同名JSON且拒绝已有结果。原双安科与Q10请求路径、runId和断言保留。新案由两名受控初始角色自然生成OID875/808，slot2由现有factory创建OID251/action0并以共用投影设置源X700/Y-60/Z400；仅两玩家离散空输入。逐tick记录辅助体、自然攻击体、目标动作/HP/源坐标、target命中计划和逐writer，tick7期望slot2及slot51依次写入目标HP465/440。测试仅使用已有生产Driver，不修改DAT、战斗生产、Scene或模式Asset。

首次生成Editor工程编译[失败原件](editor-generated-project-build.txt)：新增`ObjectPoint`缺少`NTSD.Animation` using，1个CS0246。补齐后同命令 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` [第二轮](editor-generated-project-build-v2.txt) exit0、0 error、299 warning。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` [通过](change-ledger-validation.txt)；相关diff `git diff --check`无空白错误。四保护文件Battle、Menu、GameConfig、ProjectBattleModeConfig SHA仍为前次记录值。

原Editor通过Unity-MCP只读状态持续报告 `is_compiling=true/last_compile_finished=null`，同时测试未运行、Battle Scene非Play且clean。生成工程编译不能代替原Editor程序集重载；本次没有创建运行请求，也没有执行Unity Play。待原Editor安全恢复后，先核对当前程序集时间和Scene SHA，再只运行这个新请求；若factory初态或writer有首差，保留结果并从第一个不同tick调查，不通过测试专用生产逻辑凑PASS。C053/Q07及总目标保持开放。

只读静态复核：正式playable `BattleWorld28::spawn_at`允许在空闲槽2发布受控OID251且用请求给定的action、HP/MP、owner/group；Unity `SimulationRegistryModule.AllocateRuntimeSlot`对显式`RequiredRuntimeSlot`先走该槽认领，不以动态槽起点替换该请求。因此探针的slot2不是因目录或名称猜测。具体`BattleLogicEntityFactory`初始化后的owner/关系/HP衰减和第7tickwriter仍需原Scene实测，此复核不提升运行状态。[Editor编译现场](../NTSD28-336B44-ORIGINAL-EDITOR-COMPILE-RECOVERY-20261004/REPORT.md)记录主进程与程序集时间。
