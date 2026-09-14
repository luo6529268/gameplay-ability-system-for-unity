<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE2-LANDING-FACING-SOURCE-WITNESS-001
status: VERIFIED
change-kind: TYPE2_LANDING_ORIGINAL_FUNCTION_WITNESS
code-path: Tools/NTSD28AuthorityTrace/type2_landing_facing_witness.cpp
authority: Formal physics_integrator.cpp type2 threshold>9 facing_flipped; battle_world.cpp step_physics applies flip; full SimulationTickDriver28::step.
evidence: Existing SelfCheck right-to-left bounce matches physical flip but incorrectly expects later state2000 velocity-facing overwrite; needs full driver measurement.
-->

# type2落地方向原函数见证

准确一个runner，type2已绑定目标definition（不测试同tick变身时序）；state1000/1002/2000、初始双朝向、vx-8/0/8、vy5/9/9+epsilon/10、Y-1/0/-10、collisionYRef0/-5/3，物理端点与完整tick分开捕获。声明frame0与20(state2004)/wait100，定义weaponHP32/dropHurt4，运行时HP20，对应现有heavy fixture；普通HP/MP500。捕获raw、RNG、完整messages与端点成功，复跑字节一致，原authority身份由构建器核验。

不改Unity或权威源码/EXE，不晋升新binary；对齐生产/测试若需修改另立准确Record。全tick结果不自动意味着所有动作变更都来自物理，本目标只追踪已确认flip及C25后的保持。输出只在Temp/artifacts；无服务/关闭/资源/schema改变。回滚仅此新runner且需批准。

VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY：1296行=648physics+648full，成对facing差异0、无diagnostic、frame/lifecycle无错误、重复stdout相同。确切reported case右→左，frame0/vx4/vy-5/HP19，完整tick保持。初始runner错用字段名/漏显式PhysicsContext导致编译失败，原log保留；按真实header修正后构建成功，未改权威源。最终manifest与validation见同名artifacts。

最终VERIFIED，scope按本Record及同名REPORT限定；原1296、Unity2592/4组及新完整SelfCheck PASS，生产未改。原失败与首轮native编译错误留证，下一碎片准入边缘，整体对齐未完成。
