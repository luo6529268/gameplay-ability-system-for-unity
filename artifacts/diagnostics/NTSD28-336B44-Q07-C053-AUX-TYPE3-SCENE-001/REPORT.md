# C053 辅助 type3 原 Battle Scene 定向验证准备

## 2026-10-04 共用候选消费修复后限定通过

独立生产 Change `NTSD28-336B44-Q07-C053-LATCH-CANDIDATE-CONTINUE-001` 将锁存角色候选的处理改为仅跳过当前候选，不终止该攻击者的整个候选序列；没有 OID 或角色特判。原 Editor 已重新编译生产及本探针脚本，生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` 为 0 error、299 warning。第四次独立默认命中模式 [GREEN 原件](ank610-jira500-firz700-yminus60-green-04.json) 为 `SCOPED_PASS/DONE`，12 个完整生产 Driver tick；第7 tick OID251/slot2 action20、HP368，OID808/slot50 action156、HP440，与当前336B44正式源码预期一致。前三份 RED 原件未覆盖。

[逐 tick 配对](ank610-jira500-firz700-yminus60-green-04-paired.json)以正式源码同初态 CSV 为基准，12 tick 的 261 个可比数值零差；对正式源已空出的 slot50/51 仅比较 World 槽活跃状态，跳过 39 个失活后探针缓存对象或另一槽对象的字段。此前同正式根 EXE 的受控 World 字段 1136/1136 一致，根内部 RNG 算法不据此认定相同。Play 探针写出时 `sceneCleanAfter=true`，Battle 磁盘 SHA 前后同为 `402139F7…EC8694CB`。其后独立 MCP 查询发现 Editor 已退出 Play，但 Scene 再次 `isDirty=true`，磁盘 SHA 仍相同；写入者与意图未证，保留当前内存改动，不保存、切场景或再次 Play。

该证据仅支持受控辅助 type3 C053 案例的修复后限定通过。相邻纯角色锁存回归、Tobi 自然物理按键链、其它 C053 内部、Q07/Q12 与总目标仍开放；生产 Change 暂保持 `RUNTIME_PENDING`，不宣称整个战斗已对齐。

后续只读现场复核：当前 Battle 场景磁盘文件相对 Git 的差异为新增一处 `NTSDButtonRipplePreview` Prefab 实例及其 RectTransform 引用（119 行添加、0 行删除），不是本 C053 探针声明的脚本修改范围。MCP 仍报告同一场景在内存中 `isDirty=true`；磁盘差异不能证明是谁造成内存 dirty，也不能授权本任务替他人保存或回退。下一次原场景 Play 必须重新确认干净前置；这期间可继续当前336B44正式源码与Unity共享路径的只读审计。

## 2026-10-04 原 Scene 三次定向 Play：确认首差，保持开放

后续现场补注：第三次结果写出时报告 `sceneCleanAfter=true`、Battle磁盘SHA前后均为`CACF6664…E4DB8`；在后续治理校验后的独立MCP复核中，原Editor同一Battle Scene `isDirty=true`，磁盘SHA又变为`402139F7…EC8694CB`。变动发生于探针退出后，写入者及意图未证；保留当前磁盘与内存状态，不切场景、不再Play，不把第三次运行内保护扩称为当前仍干净。

原 Editor 已在同一项目编入当前探针，原 `NTSD_Battle` 保存场景分别运行独立 `ShadowCompare`、默认命中模式、候选计划三案，每案12完整 Driver tick，均退出非Play、唯一Scene clean，场景SHA在各自运行前后相同。三份原件依次为 [ShadowCompare](ank610-jira500-firz700-yminus60-01.json)、[默认模式](ank610-jira500-firz700-yminus60-default-02.json)、[候选计划](ank610-jira500-firz700-yminus60-plan-03.json)；没有覆盖旧结果。生成Editor工程二次增量均0错误，原Editor DLL均晚于源脚本。未运行全套EditMode。

[正式源码/Unity首轮所选字段配对](ank610-jira500-firz700-yminus60-01-paired.json)前6tick×23字段零差；第7tick正式源码与根EXE同有slot50/OID808的candidate0→slot0 rejected、candidate1→slot2/OID251 applied/90伤。正式源码slot2动作20/HP368；Unity两种命中模式均为动作10/HP458，CRT调用由正式源码3006分叉为Unity3004。OID808本体仍到动作156/HP440，不能据其目标HP相同宣称本案对齐。根EXE的World字段此前与正式源码1136/1136同，但根LFR不证明内部RNG同种；上述CRT差只作同seed正式源码/Unity观察。

第三案的Unity候选计划在tick7确有slot50候选序列`ordinal0→slot0`、`ordinal1→slot2`，两者均记录`PreprocessObserved=false/DispositionObserved=false`，slot2没有写者。`ShadowCompare`同时报`ObservationPreprocessMissing`，而默认命中模式（`shadowConfigCount=0`）也产生同一World差异；因此可将排查范围收窄到**候选建成之后、slot50→slot2的消费/预处理/伤害写入之前**。尚未证明是哪个共享守卫拒绝，不能凭这条诊断直接修改生产或添加OID特判。下一步只读追 `BattleHitCandidateSequenceRunner.TryConsumeCandidate` 与正式 `BattleWorld28` 的候选0拒绝后继续处理候选1的条件，必要时建立独立聚焦Change先证RED再改通用条件。

本包状态 `RUNTIME_PENDING / FIRST_DIFFERENCE_CONFIRMED`；Q07/C053、物理按键自然链、其它角色/实体、Q12和总目标仍开放。未修改DAT数值、图片、Scene、Prefab或生产战斗逻辑。Battle Scene在此前Q08 v1/v2两次运行之间出现过外部SHA变化，本包三次各自只核验运行内不变，不回退那处变化。

状态：`COMPILE_PASS / UNITY_RUNTIME_PENDING`。这是当前336B44正式源/根[同tick双命中阳性](../NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)的Unity原Scene出口准备；**尚无新案例Unity Play结果**。

只扩既有 `NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 的独立请求 `Temp/NTSD28_Q07_C053AuxType3.request.json`、runId `ank610-jira500-firz700-yminus60-01`，输出写入本目录同名JSON且拒绝已有结果。原双安科与Q10请求路径、runId和断言保留。新案由两名受控初始角色自然生成OID875/808，slot2由现有factory创建OID251/action0并以共用投影设置源X700/Y-60/Z400；仅两玩家离散空输入。逐tick记录辅助体、自然攻击体、目标动作/HP/源坐标、target命中计划和逐writer，tick7期望slot2及slot51依次写入目标HP465/440。测试仅使用已有生产Driver，不修改DAT、战斗生产、Scene或模式Asset。

首次生成Editor工程编译[失败原件](editor-generated-project-build.txt)：新增`ObjectPoint`缺少`NTSD.Animation` using，1个CS0246。补齐后同命令 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` [第二轮](editor-generated-project-build-v2.txt) exit0、0 error、299 warning。`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` [通过](change-ledger-validation.txt)；相关diff `git diff --check`无空白错误。四保护文件Battle、Menu、GameConfig、ProjectBattleModeConfig SHA仍为前次记录值。

原Editor通过Unity-MCP只读状态持续报告 `is_compiling=true/last_compile_finished=null`，同时测试未运行、Battle Scene非Play且clean。生成工程编译不能代替原Editor程序集重载；本次没有创建运行请求，也没有执行Unity Play。待原Editor安全恢复后，先核对当前程序集时间和Scene SHA，再只运行这个新请求；若factory初态或writer有首差，保留结果并从第一个不同tick调查，不通过测试专用生产逻辑凑PASS。C053/Q07及总目标保持开放。

只读静态复核：正式playable `BattleWorld28::spawn_at`允许在空闲槽2发布受控OID251且用请求给定的action、HP/MP、owner/group；Unity `SimulationRegistryModule.AllocateRuntimeSlot`对显式`RequiredRuntimeSlot`先走该槽认领，不以动态槽起点替换该请求。因此探针的slot2不是因目录或名称猜测。具体`BattleLogicEntityFactory`初始化后的owner/关系/HP衰减和第7tickwriter仍需原Scene实测，此复核不提升运行状态。[Editor编译现场](../NTSD28-336B44-ORIGINAL-EDITOR-COMPILE-RECOVERY-20261004/REPORT.md)记录主进程与程序集时间。
