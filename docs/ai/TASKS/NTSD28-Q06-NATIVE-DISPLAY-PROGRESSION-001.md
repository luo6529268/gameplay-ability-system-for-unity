> 2026-09-21 VERIFIED / NATIVE_DISPLAY_PROGRESSION. OPoint、普通/Stage、state9996出生依赖全部闭合；新joint13/13含980通过，当前SelfCheck/相关Play/关闭通过。下一POST-DISPLAY-RESOURCE；下文WAIT及出生未完是历史，不能重新开已闭合职责。

> 2026-09-21当前回访：父任务仍IN_PROGRESS，递推/slot已验；OPoint出生资源已VERIFIED，不再重复实施。现在依次完成ORDINARY-STAGE-DISPLAY-BIRTH-001普通/Stage出生初值，以及state9996 direct clone完整资源语义（source500/500，Unity目前写10且混入OPoint百分比）。后者尚待独立源见证/Task，不允许直接把display同步到错误HP10。两者完成后回父任务联合关闭，再推进post-display；详DISPLAY-BIRTH-RETURN-AUDIT-001。下文早期checkpoint保留历史。

# Q06完整显示值推进owner

2026-09-21补充：ORDINARY-STAGE-DISPLAY-BIRTH-001现已限定VERIFIED（joint21/SelfCheck/真实Play2/关闭通过），普通与Stage出生子条件满足。下一唯一出生依赖是state9996 direct-spawn资源事务；formal源HP/MP500与Unity OPoint后10差异需完整见证。未关闭本父任务，不重做递推、OPoint、weapon piece、融合或普通出生。

READY_FOR_EXACT_PRECHANGE_RECORD。前置DISPLAY-POST-DISPLAY-RESOURCE-AUDIT已完成，其REPORT及SOURCE-WITNESS原函数980 display/2379 post是当前依据，不重复源码大审计，不直接重跑已验HP/MP。

目标完整C25d：四显示值独立目标/步长与非限幅跨越语义；所有原版可见slot的更新；出生HP显示初值等于spawn request HP，累计/step按原出生初始化，pool reset/copy/snapshot恢复保持其不同职责。现有8字段/13,21,24,2,2已齐，不先升级schema、不改普通HUD或头顶血条例外。

实施前最窄补读：LF2Character.Initialize/Init、LF2WeaponBase.InitializeHealth及派生hook、LF2Other/Special生命周期、BattleLogicEntityFactory两入口/PostInitLiving、renderer-backed LF2ObjectPointFactory与现有bootstrap/clone/复活初始化；追踪何时最终HP值确定、何时公开到slot、何时是克隆或原地恢复，不把Health.BindRuntime/HP setter当通用出生hook。声明准确生产/测试code-path和符号、副作用与回滚后动脚本。

C25生产插入点是资源pre-display后、RefreshNativeComputerState/frame前；不能仅hook recovery类，因为资源type0/frame/pending门与display不同。现有late traversal会过滤pendingUnregister/OidMergeDormant/PendingFlushDestroy；依据source slot存在关系逐项处理display访问，不全局放宽registry active或重跑其余tail。

最小测试要求：原函数980向量逐值对照；四不同target/step，0/负step与跨越下一tick纠正；HP/MP/累计真值保持；出生HP137及pool复用非默认初值；非type0/无frame/pending与empty slot；资源chp结果先被display读取、frame/timer变更后不能追改display；同版本snapshot/replay/校验；真实Scene注入后checksum恢复及有序关闭全0。先RED再实现，保留HP134/完整SelfCheck基线，必要fixture修正独立Record，不能删断言掩盖差异。

回访R05/R07，出生变化追加R09/R16；post-display事务由下一NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001接在display之后。Q06不能因C25d闭合而完成。禁止computer-use、非战斗/GAS/Mono重构、Scene/资源/Gen/Plugins/Server修改，用户HUDBg x30与bcd1047b场景及任务外文件保护。
