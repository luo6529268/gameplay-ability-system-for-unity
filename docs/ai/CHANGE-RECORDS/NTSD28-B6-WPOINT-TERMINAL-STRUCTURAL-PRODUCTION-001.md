# NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_TERMINAL_STRUCTURAL_PRODUCTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointTerminalStructuralProductionEditorTests.cs
authority: 用户Goal12 Part A明确授权；BattleWorld28::settle_held_refill_objects battle_world.cpp:7967-7970；terminal owner audit；B1E13AE1 EXE / playable closure39DDDA15。
evidence: RED completed80 failed capped25; finalfocused80/80 B6=275/275 kind3+refill=101/101 structural+lifecycle=35/35; fullSelfCheckPASS; bothbuilds0error; RockLee254 scopedPlayPASS tick6 C09 terminal/free each1 unregister/generationRelease each1; pools2to2 world4to4 Console0 SceneSHAunchanged; validator440/379; Goal13USER_HOLD.
-->

[事前Task](../TASKS/NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001.md)定义路径、scope、验收、关闭及回滚。
原状：缺terminal gate，real current-frame-null guard过早；Goal11 kind3 continuation已验收。
预期：refill/exhaustion优先、terminal transient outcome、world held consumer唯一Free；不新增第二cleanup owner。
风险：renderer Reset后旧引用不可再读、holder可能alias held、C09/C20及same-slot reuse需区分generation。Free后按预存holder handle resolve，不访问旧held。
不可越界：不改cleanup合同/Destroy/全局RNG/内容/Scene/后置族。任何旧测试失败立即停。
本Record只记录Part A；Part B零文件改动，结论只写最终报告。

新增focused落盘：real/generic×1000/1777×current-frame-null×renderer/logic-only×slot0/399×DVXkind3 overlap；另type2 prefix、8种refill、第三方plain/encoded清理/ABA、self-link alias、同pass下一child/C20trace、logic shell真实复用、fulltick。仅新test改动，production仍未改。renderer fixture复用现有pool/ResetState并finally回收/关闭临时host、恢复singleton；free前截取motion/frame快照，free后只访问独立audit与generation handle。

新test import后首次编译发现2个fixture类型/调用签名错误（Catching需LF2LivingObject，清sink需tick/pass），仅修新test；非行为RED，production仍未改。

首次新focused job0b78e4e96e994177b867b7bab615e565 completed80 failed/capped25，generic fixture把可空Health/Trans当作real武器组件导致NullReference，不能计作该矩阵的有效behavioral RED；保留FixturePreflight结果，修新test的可空fixture读取与HP初始化后重新跑。生产未改。

有效RED job0d17452e1ccb4fe49bf612c0ff4d7096 completed80 failed，返回25条失败且capped，均为expected Free1 actual0，保存Temp/Goal12_Terminal_RED_Result.json；不推算总失败数。后静态检查修新trace buffer slotCapacity128→400以覆盖399（不是事件条数）。
production四文件已写：TerminalDespawnRequested、real仅terminal绕过current-frame guard且refill后terminal、generic pre-pose terminal、Query模块唯一Free与预存holder generation handle重新resolve；Free后无held读取。sink-only tick occurrence labels C09/C20及terminal事件，不新增常规路径事件，不修改StructuralWriter/cleanup合同。GREEN/回归/Play待验。

GREEN attempt1 job0e36ccd183194f6ab23c4eb5c6817b57 completed80 failed/capped25，返回renderer-backed NullReferenceException，logic-only已越过原Free0失败。暂停旧回归/Play，只在新test包裹异常以获取完整stack，确认是否触发free后访问硬停止；不改production。

Renderer异常已定位：single diagnostic job1411e9902cff474a82992b66df53fb35 stack为LF2ObjectPool.Release:320（未初始化_activeObjects），发生在Free尚未完成时。MMSingleton.InitializeSingleton仅Play设置_instance；EditMode手动Awake只初始化fixture pool，未绑定static，所以生产.Instance找到了另一未Awake的场景pool。只修新RendererScope在初始化后显式绑定自己的pool，并finally恢复原static；无production/cleanup修改。不是Free返回后访问。Scene仍dirtyfalse/root13/SHA不变。

GREEN attempt2 job4991d3d91dcd445e854e6e0ee1c2edef completed80，仅返回3个新fixture失败（uncapped）：self-link未Free、两项第三方InvalidCast。已确认LF2WeaponBase直接继承LF2Entity，不是LF2LivingObject；第三方catch改用合法exact plain/encoded slot字段，不伪造managed Catching武器引用。generic无Trans，ImmediateFrame为no-op；BindHeld和self-link fixture改用existing raw held frame setter明确设置20/0。因此此前generic missing=false初始帧前置不完整，RED仍证明Free owner缺失，但不把该初始前置误报为已覆盖。production不改；其余返回无失败不据summary-null推导精确通过数。新增sound队列零增长及整数pose/Zz哨兵。

实际GREEN job13beb496f7c44c34a6cddc9b1e81e28f：80/80；B6 jobbea88e752ef34183ae438951cfa1f6f8：275/275；kind3+refill jobc5e220f2ff604fc48828fa96be50ef01：101/101（92+9）；lifecycle/structural/shutdown job4860573e5049488ca2255b1d2fd3c9d4：35/35。既有断言全绿，没有更改旧测试。fullSelfCheck已提交。
同新test文件将加入scoped Play probe：current Lee254 WPoint1000/DVX100与current OID123，经AttachOpointHeldObject生产relation入口建立holder/child，明确以ImmediateFrame254选择动作夹具，然后真实driver完整tick；child从当前World logic pool获取并绑定当前renderer pool borrower。observer在terminal请求时记录child状态，在free时只读结构计数/旧handle失效；Free后不再读旧child字段。finally仅按仍有效handle清理，解除observer、归还owned renderer与logic shell，普通ExitPlay；不改Scene/DAT/input规则。

fullSelfCheck实际PASS，mtime2026-09-09 16:50:43UTC，保存Goal12_Terminal_SelfCheck.result。同文件Play probe已写，编译与Play待运行。

Play attempt1实际PASS：tick6/C09 slot51 generation1，terminal请求/Free各1；Free窗口unregister/genRelease各1、Destroy/RNG/sound0；Frame20未写1000，实际两个pool归还，World4→4，logic2→2/render2→2。整tick旧RNG1、Native0，与terminal窗口零分开记录。发现JsonUtility没有导出非Serializable BattleParityStructuralEvent列表，保留counts产物，在新probe改Serializable TraceRow复制实际事件/结构ordinal计数并复跑取得完整trace；不修改production。


## 最终验收（2026-09-10）
状态：VERIFIED，仅terminal structural精确子集。Part B所有结论仅写最终回复，本Record不存其甄别结果。

| 检查 | 实际结果 | 证据 |
|---|---|---|
| 有效behavioral RED | job0d17452e1ccb4fe49bf612c0ff4d7096 completed80 / failed；25条明细capped，expected Free1 actual0；不能推出80全红 | Temp/Goal12_Terminal_RED_Result.json |
| focused GREEN | job13beb496f7c44c34a6cddc9b1e81e28f 80/80 | Temp/Goal12_Terminal_GREEN_Result.json |
| 最后probe导出修改后的focused | job2aa7c68f4418454582dad4e0f1c53979 80/80 | Temp/Goal12_Terminal_FinalFocused_Result.json |
| B6分类 | jobbea88e752ef34183ae438951cfa1f6f8 275/275（195+80） | Temp/Goal12_Terminal_B6_Result.json |
| Goal11+held-refill | jobc5e220f2ff604fc48828fa96be50ef01 101/101（92+7+2） | Temp/Goal12_Terminal_Kind3Refill_Result.json |
| lifecycle/structural/shutdown | job4860573e5049488ca2255b1d2fd3c9d4 35/35 | Temp/Goal12_Terminal_LifecycleStructural_Result.json |
| full SelfCheck | PASS，2026-09-09 16:50:43UTC | Temp/Goal12_Terminal_SelfCheck.result |
| Runtime build | 0 error / 22 warnings | dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly；Temp/Goal12_Terminal_RuntimeBuild.txt |
| Editor build | 0 error / 104 warnings | dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly；Temp/Goal12_Terminal_EditorBuild.txt |
| Ledger validator | PASS / 440 records / 379 governed code files | Tools/Validate-ChangeLedger.ps1；Temp/Goal12_Terminal_Validator.txt |
| Scene | SHA256 D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11，dirtyfalse、root13 | Get-FileHash；Temp/Goal12_Terminal_ExitConsoleScene.json |

既有断言未修改且全部上述旧套件通过；新增夹具的类型、current-frame初始化、EditMode singleton绑定与JSON导出修正已按时间保留在前文及Temp attempt文件，不将夹具异常算作行为RED。尤其初始generic missing=false前置后来纠正，不声称早期RED覆盖已完整。

### 实际 scoped Play structural trace
指定instance gameplay-ability-system-for-unity@b1b02287，Unity2022.3.62f3，真实NTSD_Battle World；current RockLee OID7 action254（state15、weaponact1000、dvx100）与current OID123。holder slot50，child slot51/generation1。
通过production AttachOpointHeldObject建立current OPoint-kind2 held关系，显式ImmediateFrame254选择动作夹具，再由真实driver StepOneTick推进。child来自World logic pool，并绑定当前renderer pool borrower。没有直接调用held writer/Free伪造测试结果；这不是物理按键或整套充能技能验收。

| 顺序 | tick/pass | 实测 |
|---|---|---|
| 1 | 6 / held-refill:C09 / held-terminal | cursor51、actor50；child仍action20，未写1000，terminal请求1 |
| 2 | 6 / held-refill:C09 / free | slot51，Free1、child Unregister1、GenerationRelease1、Destroy0；旧handle已不可解析 |
| 3 | 6 / late-entity-update | 扫描仅见slots0/1/50，slot51不再出现；C20未再次请求/free |

terminal请求→free窗口：NativeRandom0、旧RNG0、音效队列0。whole tick NativeRandom0、旧RNG1；后者在terminal窗口外，未把它计入terminal，也不在无per-call observer证据时进一步归因。
JSON trace中的command/ordinal是当时“最后一条StructuralWriter命令”的快照；terminal请求本身是diagnostic事件，其Register/5仍来自tick前setup；free行的GenerationRelease/3属于本tick Free→Unregister→GenerationRelease序列。不能将诊断事件顺序与last-command快照混为一套ordinal。
完整序列：Temp/Goal12_Terminal_Play.result.json；首轮已PASS但未序列化events的counts文件另存attempt1。

child Free同时归还renderer与logic shell；renderer OnDisable另有presentation unregister，因此whole-tick全局Unregister不等于child单独Unregister，child的精确+1在对应free事件处采样。finally以预存handle解析仍活对象进行清理，未读取已回池child字段。
退出前World对象4→4、logic borrowers2→2、renderer borrowers2→2；owned holder/child handles均失效，renderer inactive且binding null。正常ExitPlay完成，manage_editor stop返回Already stopped，Console error0，Scene dirtyfalse/root13/SHA不变。

### 交付增量 / 不可越界项
实际改动4个production脚本、1个新focused/Play脚本及meta、Task/Record、Ledger/STATE/对齐总表，共11条授权路径。四production脚本已与本包事前Temp备份逐行比较，最小diff为Temp/Goal12_Terminal_Production.diff；最后git status共1493条（起始1489条，新添test/meta/Task/Record共4），范围外可读dirty文件未发现本包开始后的写入时间。没有提交/push、Scene/content写入，未改StructuralWriter或BattleEntityLinkLifecycleWriter。
terminal outcome及trace计数不进入snapshot/checksum；只有有sink时维护trace occurrence，无新runtime queue/manager/worker生命周期，不改变十一阶段关闭。
全局RNG、refill数值、kind3算法、nonterminal missing-action、non-kind3 DVX weaponHP、+2F8、schema/恢复等保持后置。回滚仍只允许用户明确批准后反向本包增量，不能回滚Goal1-11或用户已有修改。
GOAL13_USER_HOLD：本Goal报告后停止，不自动执行下一包。
