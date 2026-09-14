<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001
status: VERIFIED
change-kind: NATIVE_WEAPON_PIECE_TRANSACTION
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Tasks/OPointCreateTask.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs
authority: Formal playable battle_world.cpp materialize_weapon_piece_fragments and generic spawn_at.
evidence: NTSD28-Q06-WEAPON-PIECE-SOURCE-WITNESS-001 original 157 synthetic and 3 formal cases.
-->

# 完整武器碎片生产事务

IN_PROGRESS / TEST_FIRST。原状LF2Entity只arm pending/code1000并sound，无两阶段producer。仅上述六脚本：LF2Entity接线并复用纯数量表；新writer按原顺序内置/DAT、同步RNG、slot扫描和立即结构创建；task增加独立nativeWeaponPieceSpawn并Clear重置；两factory只在该flag执行generic出生适配，普通OPoint百分比不变；新focused以原函数JSON比较出生与RNG。

数据合同：HP/bound/base/MP500，max_mp保留metadata，动作四镜像及Native零帧、计数和显示初值；内置默认身份，DAT显式继承。复用当前StructuralWriter和materializer、reference pool，不创建新manager/queue/worker。先检查World停止接单，任务try/finally归还；十一阶段shutdown顺序不变，Stopping禁止创建，无pending新队列。生成即时完成后现有C25尾部关系安全删源。槽50..当前capacity保留用户容量例外；variant随机早于缺定义/无槽，内置无槽break/DAT continue。随机逐callsite顺序来自正式源码。

验收：先运行缺writer RED，完整157向量的生产输出及随机比较、实际Logan三个定义、两factory和高低slot生命周期、编译CS0、相关focused、完整SelfCheck与真实Play关闭均需新证据。当前SelfCheck失败已保留，其旧fixture修订必须另立Record，不能借本包只改断言。快照15/23/26/2/2不变（flag仅瞬时task且Clear归零），raw47/3不扩大。

风险：Init旧帧访问器和OPoint出生继承可污染通用碎片，需要比较完整出生；两路径必须一起适配。尚未证明父frame全tick对齐。非战斗、Unity/GAS、Scene/HUDBg、资源、Server/Gen/Plugins保持。回滚仅本次六脚本差量，保留原未提交内容，执行回滚需用户批准。禁止computer-use。

实施检查点：六脚本已写。新增Materialize/BuiltinAction/Spawn/InitializeBirth，LF2Entity破碎资格后即时调用；两factory native flag分支与task.Clear。producer缺失测试实际1/1失败已存producer-missing-red.xml；157原函数向量×两profile完整raw47、每call和state比较已写，尚未跑完。初次新fixture构造函数参数CS1503已按现有constructor补容量，待编译复验。未修改SelfCheck，不报告生产已验证。

49/49联合通过（job1c81a83c4a294038868d9000a820ff3e，66.273秒）。两profile各157/762片与实际3/50片已raw47和全部同步call/状态0差异；group继承修复首差异已归档。SelfCheck已越过本武器fixture，后续LC02失败另立Record。现于原声明测试文件补真实Scene八向量probe：两materializer×两frame backend×破碎/健康terminal，实际World.LateEntityUpdateAll与每次checksum恢复；此为完整C25 Late pass，非完整input/physics tick。不新增runtime manager，Editor hook仅Play+snapshotready才执行，无新停止接单阶段。

Play首尝试被reset-only backend setter拦截，未生成source，报告objectsAfter0是未赋值字段而非World实际清空；Q05真实恢复4→4/全0关闭已核实。保护不改，probe修为当前后端两materializer×破碎/健康终止四向量，不动态更换backend。其它backend专用World验证另补。

Play第二次四向量生成/终止断言均通过，最后presentation extras恢复被EntityShellMismatch拒绝；现有快照并非新增Renderer回收入口。修测试原声明文件：记录原claimed slots，恢复前与finally使用FreeEntityLikeExe显式回收本probe新实体，再恢复快照。不得修改Snapshot框架绕过shell检查。失败报告及后续14→14恢复/有序关闭全0已保存，不能误写该次baseline恢复4→4成功。

已观察新增边缘：native spawn_at不按OID正数排除，两个Unity factory仍统一oid<=0拒绝；weapon_piece默认-1→999，但显式OID0如catalog存在需原函数向量确认再修专用准入，不可套用OPoint<=0门限。当前157+3数据没有覆盖此项，不能关闭完整fragment事务。还需999声明/缺失、非法动作、失败和高低slot完整driver参与。下一先按GT08-LIFECYCLE-FIXTURE-REBASELINE恢复完整SelfCheck，再处理上述边缘。

当前FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / FULL_TRANSACTION_INCOMPLETE；细节见同名artifacts REPORT。完整SelfCheck下一GT08仍FAIL，边缘准入未闭合。

最后源码联合复验：job75fe40badce24741a8e0b9a28e4b43ae，49/49 PASS，64.8175753秒，五个选择器均实际执行（15/12/10/7/5）。focused-final-49.xml与final-validation.json留证。完整SelfCheck最新GT08 FAIL不变。

后继NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001完成OID0/动作999资格/槽位/full driver及pool失败边缘、两factory目标World初始化，新增5152向量/20+12/完整SelfCheck/Play173及旧内容Scene4例/关闭全0有据，恢复VERIFIED / WEAPON_PIECE_PRODUCER_SCOPE_ONLY。原157+3及早期失败保留，不能推广为父C25/frame全部对齐。回父NATIVE-FRAME-TRANSACTION-INTEGRATION做组合调用链/trace最后联验，再剩余reader/display-post/Q07。详见后继REPORT。
