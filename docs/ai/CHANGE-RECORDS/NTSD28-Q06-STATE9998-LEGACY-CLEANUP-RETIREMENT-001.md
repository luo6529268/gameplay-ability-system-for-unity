<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE9998-LEGACY-CLEANUP-RETIREMENT-001
status: VERIFIED
change-kind: REMOVE_NON_NATIVE_STATE9998_SWEEP
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9998LifetimeEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.cs
authority: Original full SimulationTickDriver28::step witness224 scenarios/672 outputs, states0/9998, type0..6, HP0/500, y0/-20, slots0/70.
evidence: All native entities survive3 full steps; Unity SerialTickAll unconditionally FreeEntityLikeExe for state9998 immediately after C25 in normal tick.
-->

# 退休额外state9998删除扫描

IN_PROGRESS / TEST_FIRST。原函数已实测224/672全存活且state/action与输入符合；新runner带frame/lifecycle错误检查复建进行中，需通过后才能关闭。本Record准确五脚本：仅SimulationWorld.SerialTickAll末尾去除CleanupState9998Entities调用及该无其它caller私有扫描；保留其它Serial runtime/snapshot/type3职责和主pass顺序；NTSDBattleTickSystem和LF2OtherObject两处注释删除旧state9998权威声明，保留diagnostic enum数值兼容。SelfCheck GT09次tick期望改持续存活/无销毁事件及sound，与原证据一致。新focused以224原函数场景比较实际Late+Serial邻接前后存活/state；此为Unity尾部组合，对照原完整driver的此项生命周期，不伪称全部字段完整tick已齐。

数据/关闭：无新字段/schema/服务/queue，移除不属于native的过早删除；正常terminal/weapon break及有序shutdown继续按现有入口回收，不能变成不可回收实体。需目标RED、修后focused/已有frame+weapon regression、完整SelfCheck新首差异和真实Scene定向存活/显式回收/恢复关闭。非战斗/框架/资源/Scene不变。源码删除限已声明私有方法与callsite代码，不删除文件。回滚仅本五文件差量且需批准，已有用户改动保留。

CODE_WRITTEN / FOCUSED_PARTIAL；原7/7 RED，五脚本已完成准确改动，28/29联合通过。224场景删除差异全消除；type0 HP0仍48次action/state首差异（C25旧RunLateDeathOpointPreCleanupPhase强制186），保留失败断言并另Task核验，不改预期迁就。准备在原测试文件补真实Scene Serial读当前frame9998的定向存活/checksum恢复/关闭验证；不修改FrameCache共享数据、场景或框架。

最新fixture已对齐HP/HPBound/HP3、revive lives1与display出生字段；最终6/7仍48个type0动作差异，beforeSerial=186证明差异在C25后已存在；删除差异0。没有删除或放宽这项失败断言。原checked driver最终672均成功、诊断只为HP0输入抑制，见source REPORT。

最终COMPILE_PASS / FOCUSED_PARTIAL / SCOPED_PLAY_PASS。真实Scene当前descriptor9998通过实际SerialTickAll后存活、4→4 checksum恢复、随后关闭全0/两帧Stopped。最终6/7保留type0 HP0的48次上游186差异；完整SelfCheck仍type2落地方向FAIL，详见同名REPORT。未关闭本Record的整组动作一致出口。

后继C25额外death prelude退休后，原type0 HP0的48差异消除，224场景lifetime/state全部通过；64联合/此前真实Serial9998和关闭证据齐全。恢复VERIFIED / NO_EXTRA_STATE9998_SWEEP_ONLY，完整SelfCheck仍独立landing失败，不扩大为全部Serial对齐。
