> 最新状态：VERIFIED_AUDIT_ONLY；原函数980+2379见证已取得，生产仍待。完整报告 artifacts/diagnostics/NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001/REPORT.md。下一 NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001。

# Q06 display/post-display资源交接审计

IN_PROGRESS / READONLY_PRODUCTION。前置HP和MP事务已限定验证，报告分别位于同名artifacts；不重复实施。总目标及Q06保持ACTIVE。

只读追踪正式playable SimulationTickDriver28::step每slot资源pre-display→advance_native_display_values_slot→资源post-display（simulation_tick_driver.cpp约982-993）及battle_world.cpp实际成员函数，继续到字段读写、常量、资格、显示值/真实值区别、限幅、计时/状态清理、统计和生命周期副作用闭合；确认参与正式构建闭包。

Unity从当前NTSDBattleTickSystem/C25、真实LF2Character/LF2Entity及相关writer/pass追踪，而非按名字推断。产出字段/顺序对照、确认差异与未知项、最窄可复现输入、source-linked向量和真实Play方案。特别区分HP段故意不限幅与post-display限幅，不把显示插值反写为逻辑真值；若新差异依赖Q08 mode投影，拆出可先验证的不可变规则输入合同。

本审计不改脚本、资源、Scene、schema或框架；任何新工具/测试/生产实施前另建准确Task/Change及路径。保留13/21/24/2/2、33ms/3ms、十一阶段、用户HUDBg x30/Scene bcd1047b、所有既有例外和任务外修改。禁止computer-use。CPoint/OPoint/+2F8/revival/pieces、Q07正式资源、Q08 mode和Q12整场仍需后继；不得由本审计宣布整个资源域或Q06完成。

原函数向量由独立NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001提供；本父Task本身不改脚本。
