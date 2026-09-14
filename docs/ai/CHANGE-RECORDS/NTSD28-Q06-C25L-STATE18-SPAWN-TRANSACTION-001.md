<!-- CHANGE-RECORD
id: NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001
status: VERIFIED
change-kind: C25L_NATIVE_PARTICLE_SPAWN
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeState18ParticleWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State18SpawnEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25LState18ParticleOwnerEditorTests.cs
authority: Formal materialize_state18_broken_weapon_particles/spawn_at; 1550 original source/full-driver vectors and F08.
evidence: Legacy Match.Rng and ordinary birth in current C25L remain wrong despite correct owner placement.
-->

# State18/19粒子生产生成事务

事前准确五脚本。LF2Entity.C25L委托新writer，旧state13/200 SpawnTransitionEffectBranch1/共用旧tail保持；只移除不再被引用的Branch2方法。新writer复用decision kernel及NativeRandom同步callsite，source整数/精确位置和velocity分离，generic HP/MP500/default owner/group/right/action140。nativeWeaponPieceSpawn现有flag在这里同样表示原broken-weapon generic spawn：借用已有IsInitialActionAdmitted/InitializeBirth以及两个factory的目标World提前绑定，SpawnSemantic仍TransitionEffect，避免重复出生/复位字段。flag已有Clear/copy所属任务合同，不加持久字段或schema。

slot读实际World.FindEntityByRuntimeSlotForNativeDisplay语义：包含仍在slot的pending destroy，排除fusion OidMergeDormant与pendingUnregister，等价native live entity存在性。选首次null而非first free bit；原融合保留slot在raw entity不存在但仍claimed时会在required-slot register失败，tuple已经消费并break，不能跳到下一空位。slot50..当前批准profile容量，不全局释放/放宽registry或改变capacity。

生产default delay0保留原B8/Q08动态producer边界；writer可显式接收delay用于独立原函数向量，正delay只抑制持续态选roll，非离开态。Stopping在任何RNG/spawn前拒绝。每粒同步structural create后下一次扫描，不新增queue/manager；task try/finally归还原pool，pool不足break。Module去掉该immediate producer后的旧queued flush，保留活动检查/其他owner flush和previous078→fragment→lifecycle顺序。

验证先旧实际入口RED（不是仅缺API）：原同输入/775出生及完整driver适用场景、两profile/two backend、actual raw47/3和RNG；delay非零单独helper测，当前host无动态producer。新增reserved dormant slot失败、任务/逻辑池不足、Stopping禁止写入，普通spawn/fragment回归。旧owner tests保留位置/镜像保证，仅改Native RNG证据。真实Play两factory/正式999与隔离fixtures/完整SelfCheck/Scene恢复/有序关闭全0。失败原文保留，生成功能通过不代表B8动态gate或整个frame事务完结。

关闭：现有阶段3结构停止、6任务归还、7Renderer、8logic清理，复用现有关闭顺序。无非战斗/Scene/正式资源/Unity-GAS架构/Server/Gen/Plugins变更。用户HUDBg x30/hash bcd1047b保持。回滚只本Record新增差量且先批准，不覆盖用户工作。

RED实际三组文件750/757/757均有失败（legacy RNG/native sequence/birth字段），最后DataOriented组在外部Editor退出/重启期间中断；原PID58092消失，新PID71188指向同项目，Temp清除，无最后XML。未重启/kill Editor，原因未知，red/interrupted-job.json保存事实。三组实际RED充分支持开始修复，第四不计通过或完成。桥接会使用新Editor，不能启动第二实例。

CODE_WRITTEN：三生产文件已改，精确C25L委托/新Native writer/删除该路径额外flush；共享state13 branch保持。旧C25L test改Native RNG计数。首次连接新Editor时尚未ready，后日志证实StdioBridgeHost端口6401已启动；改用内联只读TCP桥接，不恢复被清Temp文件或启动额外Editor。后继编译/测试继续，不能报告已验。

首新定向11中10PASS/1FAIL：formal组合ordinary OPoint slot50的facing/Z/lives，state18/fragment字段无首差。核对PhysicsState.dir是独立compatibility字段，fixture只写Runtime.Dir没有经SwitchDir同步；先将本fixture改走正常SwitchDir，区分测试投影遗漏与真实Z/lives差异。Z定位两late opoint caller用Runtime.Z+1而native整数Z+1；generic生存计数未初始化是另一个出生事务，独立子Record后续处理。

验证执行调整：外部重启前单个大NUnit方法长期占用Editor，现按64向量一组拆为48个NUnit cases，并缓存不变的source JSON与共享只读DAT定义。四配置合计3021向量、全部断言与输入未减少；每组独立结果文件，便于恢复和主线程间隙响应，不改生产。

定向17/17 PASS（0062a9974ae24c258a27d813be6e148c，13.5548935秒）：正式192比对全部0差异，18显式delay、保留slot/池失败/Stopping与原owner4、Z/lives6均通过。完整SelfCheck与Play、原大矩阵后继，当前尚非完整VERIFIED。

Play额外失败确证为fixture资源前置：当前旧Scene仅8空闲Renderer，composite需要同slot尾24个同时借用（普通1+粒子7+内置15+DAT1），实际NativeCalls38/108、Renderer拒绝2。先前已修ordinary binding的frame差异已消除。测试在进入正式矩阵前使用现有PrepareObjectCapacityImmediateForDiagnostics补到baselineBorrowers2+24=26，临时解除/恢复原pool seal；只准备本probe注入的图形，不改逻辑slot容量、GameConfig/资源/框架、不将原不足报告改为成功。新GOs随现有正常Play关闭处理。

正式Play96/96通过（实际Logan999/两factory/完整tick，prototype父与777明确覆写）；随机逐调用和raw47字段一致，Scene checksum保持、Renderer2→2。probe资源前置用现有诊断API为本次峰值24准备26 total，逻辑slot400不变、无配置/资产变更。最终大矩阵/回归/关机验证继续，尚不推广整个父frame或Q06已对齐。

最终回归正在同一job aa6b0c9f033e4b8b83174826936e782c执行；尚不关闭。current-test-progress.json记录当前48 chunk进度；最终SelfCheck（绑定修复后）需紧接其终态运行。

最终联合82中58PASS/24FAIL；完整3021向量执行，direct1507零差异；待处理pending motion/physics上游首差及C17例外guard范围。继续IN_PROGRESS，不能以此前17/Play96关闭整体验收。证据final-regression-82.xml/current-test-progress。

## 最终限定出口（2026-09-14 03:23Z）

VERIFIED / SCOPED_STATE18_NATIVE_PARTICLE_TRANSACTION。实际证据按改动职责使用，不推广整个B或Q06完成：

- source state18 1550行、formal96，以及weaponHp210重复一致；原正式EXE SHA B1E13AE…9033与75源码manifest07CD47…778F保持。
- 原82测试58PASS/24FAIL；旧non-RNG276实际含252 pending位置和24 weaponHp（此前全归pending的说法已纠正）。pending原6中4FAIL→6PASS，补源实体位置/速度/current/latch/previous/collision/state/pending/code后仍通过；weaponHp原10FAIL→全绿。
- job5a104f86aa1147a3921e27253a5fcc8f：24组22PASS/2FAIL，1514向量只剩weaponHp；修复后jobce0f7a50c7524df990c290e4d9a12831实际30/30 PASS，含weapon10（两caller420原向量、7type真实池复用、手工fallback）、pending6、原SpawnVitals12、两失败chunk5。24组最新合计1514向量0差异/legacy546次恰为批准例外；22组来自前一终态、2组来自修复后，不能称同一run24全绿。direct1507原通过且不受普通OPoint初始化差量影响；full-matrix-final-validation.json记录来源。
- 最终真实Play：pending12含新增源状态；weaponHp84（7type×kind1/2×pointHp负/零/正×两actual factory）；正式Logan999组合96（两factory、原raw47/RNG）。全部PASS，Scene checksum保持，每组Renderer借用2→2。各目录Play-final-pass.json。synthetic父/777及旧内容Scene边界已声明，非图片/物理按键全验收。
- Shutdown-final-pass.json：4→4恢复、World/slots/logic/render borrower全0，两帧Stopped并正常退出。最后SelfCheck请求03:21:56.9990036Z、结果03:22:37Z PASS，归weaponHp目录SelfCheck-final-pass.result。CS error0；Editor idle非Play/非compiling，Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。
- Ledger与diff-check已通过（历史未出现在diff的声明路径WARNING不作失败），交付前再运行。无正式资源/Scene/InputActions/非战斗/Unity-GAS/Server/Gen/Plugins修改，无computer-use、commit/push或用户工作清理。

后继边界：普通OPoint的effect/reserve/join/join_reserve/join_pic及type0/5-parent credit门、defend等明确仍由OPoint remaining consumer Task承担；generic出生1/0/0不是最终所有OPoint复活字段。当前frame父事务仍需高动作/成本fallback连续完整driver与真实Play联合，不能用端点2676或本组合覆盖代替。其后回原reader/display-post/Q07。15/23/26/2/2、raw47/3及全部已批准例外保持。
