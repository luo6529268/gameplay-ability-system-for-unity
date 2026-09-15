<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001
status: VERIFIED
change-kind: NATIVE_INPUT_ACTION_AND_COST_FRAME_READERS
code-path: Tools/NTSD28AuthorityTrace/native_input_action_cost_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeInputActionCostEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboActionTransactionEditorTests.cs
authority: Current playable InputRouter28::step_sampled -> try_field/apply_action and native clamped/gated/rowing branches. DatDocument::frame defines native implicit 0..998, declared999 and invalid null; original source frame owns fees across encoded-state redirect.
evidence: Live source apply_action317 normalizes signed request/999 before frame lookup, preserves source descriptor for fees, and writes fallback without ordinary last-action mirror. Unity ApplyNativeInputActionCore195/203 and rowing836/854/builtin cost1585 still use legacy Has/Get.
-->

# Native input action / cost frame readers

IN_PROGRESS / SOURCE_WITNESS_FIRST。准确Task同ID。只处理BCAW.ApplyNativeInputActionCore的源帧准入/读取、RouteNativeRowingRedirect成本与目标读取、NativeBuiltinMpCost。LF2Entity.WriteNativeInputActionUnchecked已在前包验证，不重复改它。

先以真实InputRouter.step_sampled的hit_a/current input及rowing/builtin路径产生源前后状态与费用副作用，记录DAT原文、初值、raw/B2/RNG及InputActionAttempt。不要为了直接调用anonymous helper改权威源或构建闭包。非零/负/999/溢出请求、隐式/声明低高帧、编码state重定向、原始费用所有权、HP/PP边界和fallback优先级分别构造；无法通过真实caller触达的输入必须明确，不冒充生产场景。

保持signed flip、999归一化、source-frame费用、fallback mirror/counter、local resource gate、mode调整、统计、缓存/latch和后续生命周期顺序。读取API迁移不授权更改费用公式或全局state。旧typed测试若与当前native框架语义冲突，先保存RED，精准登记oracle路径再修改，不改生产迎合旧期待。

验收：源双跑和定向独立检查；Unity准确初值匹配后RED→修复→矩阵零差异；已有combo/action cost、ground/airdash相关回归；编译CS0、SelfCheck、必要真实Play/回放/关闭。source fixture不是Q07正式DAT部署或全输入/全战斗完成证据。

风险与回滚：仅这三个BCAW符号及单测试/工具脚本；如发现需要额外文件，先补准确Record。无新持久schema/owner/queue，十一阶段关闭保持。回滚仅本差量且按规则批准；保留用户/其它任务工作。禁止Scene/资源/非战斗/GAS/Mono/Server修改、computer-use、提交推送或删除文件。

源151已构建双跑exit0/627793 bytes一致SHA b00f0e3fb7fcf6cbef85ac41616a931e6bc3eebd358bdfd9e71c313eeb7b1d9b。7 groups含request16、encoded32、redirect_binding16、fallback_priority6、fallback18、rowing44、builtin19；定向独立检查边界在source/validation-summary.json。root已写单Editor fixture：DAT原文走完整CharacterAnimtorManager.BuildCharacterDataFromSource，复用B2/raw/RNG投影并比resources六字段、恢复精确velocity/facing/费用控制和统计初值，151两profile。生产尚未改，等待编译/RED。

Unity RED job78decb4315ce4936b069a8d0c6781e46两FAIL：两profile151 before0，完整差异见production-red JSON/XML。已仅修改BCAW三符号4处读取/门：ApplyNativeInputActionCore先Native getter再null门并保留原sourceFrame局部供费用；rowing原生cost getter+HasNativeFrame；builtin cost Native getter。未动公式、signed/999、重定向/fallback或统计writer，未改其它生产文件。等待编译和原151矩阵及动作成本回归。

jobafdfda353a834236a7562ec8dcbaa4ec联合42项39PASS/3旧oracle FAIL；151两profile before0/after0，全Ground/AirDash通过。独立只读review实际四处差量未发现确定错误，紧邻原子順序保持。修改前新增准确旧Editor路径三方法：RejectsLockMinMagnitudeAndMissingSourceFrame把不可用目标检查设1000，另保留21隐式接受控制；UndefinedRedirect…777与UsesFallback…71的描述符由null改为Native getter身份，不改原费用/统计/lastAction/facing断言。三原FAIL已归after-fix。

后继验收预声明：同源runner在原step_sampled输出之后追加一个完整SimulationTickDriver28.step，使用正式默认BattleConfig gate，记录可能null实体、完整输入/资源/RNG和生命周期。原endpoint源输出先保留，不把新的完整tick结果预判为通过；Unity同测试追加该相邻tick对照，以查原子费用与后续帧消费组合。不改源/生产主循环。

旧Combo三处oracle修订后jobbce94e2edd6a434cbb22d79d1bd3477f本类18/18PASS；其它原39PASS无需重做。已在同Editor测试新增151两个profile的完整相邻tick消费：endpoint先0差异，再RunReleaseTick对following raw/B2/resources/存活及世界RNG/调用序列。源扩展正在build session35718，尚未输出新fulltick或跑该新测试。

源相邻tick扩展已构建，151双跑exit0/877177 bytes一致SHA ead3f7859b1f74e38204a441b397fa298bdee20c484e79995aa29a84a9687c8a；逐行比对原endpoint全部字段0变化，source-endpoint保留旧b00f…以及对应验证。新following有27个终止实体，不预判Unity结果。新C#编译CS0，完整tick两个profile测试开始。


相邻完整tick job d8b2919e9e034d0ba6ee525a3e63a35a 两profile FAIL，原始JSON/XML归档following-initial-red。已读代码确认两项分离线索：测试世界未设置Stage.ZMin，默认180导致source Z0与Unity Z180；BattleEcsCharacterFrameAdvancePass.ExecuteCharacterDynamics仅在ShouldResolveCharacterLanding时调用旧HandleLandingEventForFrameAdvance，未调用LF2Character普通路径已有的ApplyCurrentDatType0State1218EnvironmentDamage/ContactAction。前者为夹具配置不一致；后者是待以相同配置和Legacy/DataOriented对照验证的生产路径缺口，不以静态阅读标为根因或完成。本轮未新增生产修改；下一先统一源/Unity世界边界、保留两种路径证据，再为独立physics修复建立准确Task/Change，不扩大当前BCAW费用Record。

预声明测试隔离：源SimulationTickOptions.stage_bounds未设置，source driver678/904/912跳过边界；Unity fixture设Stage.ZMin=-10000/ZMax=10000，在本151有限位置范围不触发深度钳制，不改正式Stage配置。FollowingTick测试扩展两profile×Legacy/DataOriented显式物理模式，输出独立文件；用于分离同源差异，不能用Legacy通过掩盖默认快速路径。


世界/路径隔离已执行：补using NTSD.Simulation.Ecs后刷新CS0；job53acd6f9d30f483daebeda0f0cdc12c2终态4FAIL，四组各151且before0。Legacy两profile各44差异，仅22案例position.x/preciseX负向位移被Unity钳制到0；DataOriented各370差异，仍包含state12接触action230/231、counter和Vy。完整JSON/XML在following-path-isolation-red，未缩减断言。下一需要统一横向边界（源未提供stage_bounds，Unity有正式边界pass），再完整确认快路径差异并另建physics Task/Change。当前fixture明确两路径，禁止以Legacy结果替代默认DataOriented验收。

边界统一后继：保留source-following-unbounded原151，诊断runner options.stage_bounds明确StageBounds28{800,-10000,10000}，Unity fixture同时明确StageWidthPx/BaseStageWidthPx=800、XMaxOverride=0、Z范围相同。场景/原正式资源不变，所有endpoint向量和断言保留；只改变后继完整tick世界边界，使其具有同样0左边界。

费用验收新增同文件snapshot replay：151全部案例×两profile，tick0采样前capture，执行费用事务+两完整tick，恢复同World重跑；比较raw/B2/resources/native RNG且旧cursor必须失效。新测试不作为跨Worldepoch或正式内容证据。

真实Play预声明：同一费用Editor文件新增请求probe，在真实Scene稳定暂停边界执行151×两profile×两物理mode×logic/renderer工厂，检查场景checksum与Renderer borrower保持；退出走既有Q05关闭probe。测试世界原生完整tick与正式Scene共存，不宣称物理按键或视听对齐；renderer工厂与logic工厂同源before/after/fulltick断言，不改Scene。

Canonical physics独立修复后，联合joba456cae2273e42cbb592ebf3828d2c96费用类8/8全部PASS：151两profile端点0差异、151四profile/mode完整tick0差异、302同World replay场景/604重放tick。该联合总49里2新physics oracle错误已由独立Record修订并复验2/2，不改变费用断言。源bounded SHA3278f3…875056替代无bounds following数据，旧数据保留。等待完整SelfCheck与新真实Play1208/关闭。


最终 VERIFIED / DECLARED_NATIVE_INPUT_COST_AND_FOLLOWING_TICK。此前IN_PROGRESS与失败记录保留历史。Unity最终刷新CS0；完整SelfCheck19:09:57Z PASS。费用原151两profile端点+四完整tick零差异，302同World replay/604tick；新physics56pass通过；旧B4四类39PASS。真实Scene1208 source/factory/mode对照PASS、独立56physics Play PASS，两次borrowers2→2/checksum保持；最终19:11:02Z关闭恢复4→4、World/slots/logic/render0、两帧Stopped、Scene dirtyfalse/root14/hash BCD1047B…0E9FB6保持。生产fast diff独立review无确定问题；oracle仅修新合同下无效目标。实际范围/原失败/合成内容边界见artifact与父报告；未完成整个Q06/Q07正式迁移/全部按键或视听。
