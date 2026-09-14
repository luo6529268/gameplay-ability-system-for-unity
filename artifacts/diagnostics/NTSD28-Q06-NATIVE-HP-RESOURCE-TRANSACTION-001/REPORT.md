# Q06 HP资源事务限定交付

VERIFIED / SCOPED_HP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS，2026-09-14。Q06及总目标仍ACTIVE / FULL_ALIGNMENT_INCOMPLETE。

正式playable的GameSession/scenario默认mode28=1，不能采用孤立ResourceSystemRules默认0。BattleWorld28资源pre-display成员函数定义HP分支；本包移植stats.regen_dhp/regen_hp、双倍计时、frame.chp及向0除3、无stats/default mode分支、weak时按baseMaxHp上限回MP，保持HP段不限幅。两生产caller使用World phase12及共同type/frame/lifecycle资格，顺序HP→既有负环境→MP；MP算法与所有字段/schema保持。

验证证据：

- native-hp.tsv：3768原函数向量；native-build-manifest.json记录当前75个正式源码/header身份及runner，未改正式EXE或源码。
- red-results.xml：原13项11失败/2通过；real-logan-red-results.xml：两profile均tick12 Unity101/native100。原失败保留。
- focused-134-results.xml：真实Unity EditMode job f5b1e641ff4642d2b77da01317640d10，134/134通过，127.385秒；包括HP3768和MP2028向量、两caller、边界/分配及两profile实际Logan 12tick。Authority400-hurt-comparison.json与MobileExtended-hurt-comparison.json记录HP100持续12tick，HPBound/MP也逐值一致。
- 完整SelfCheck首次GT-06失败来自未注册World的旧夹具，独立HP-SELF-CHECK-WORLD-CONTEXT记录只补私有World及phase，原断言保持。最新SelfCheck-final.result为PASS；首次失败留在该独立记录artifact。
- play-hp-pass.json：实际NTSD_Battle旧内容Scene tick11→12，phase12=0；无stats角色default HP101、显式mode0 HP101、weak HP100/MP500、双倍HP102，完整checksum恢复，World对象4→4。此Scene无stats，不能拿其101与正式Logan有stats的100直接作差异。
- play-cleanup-pass.json：真实恢复后有序关闭，World/slots/logic borrowers/render borrowers全0，两帧仍Stopped，Editor已退出Play。没有物理输入或整套技能验证声明。
- scene-final.json：CS错误0、Scene dirtyfalse/root14；用户确认HUDBg x30及SHA bcd1047bf912c6a4a8bc9f3a76eaf3fa954211ad064e0402b1c01bf3ba0e9fb6保持。
- workspace-protection.json：3059保护文件，2926未变，115既有变更、18既有缺失；相对MP包无新增变化路径/缺失。final-code-scope.json冻结五脚本及独立SelfCheck修正的hash。

实际命令/入口：Build-AuthoritySourceCapture.ps1构建声明native witness；Goal13_bridge.py run_tests/get_test_job；NTSD_BattleRuntimeSelfCheck.request；manage_editor play及HP/Q05请求文件探针；get_editor_state/manage_scene get_active/read_console。测试均使用现有Editor，禁止computer-use。账本验证单独记录ledger-final.txt。

仅三个既有生产文件的战斗资源段、两个新诊断/测试文件，以及独立SelfCheck测试块；未改非战斗逻辑、Unity/GAS框架、Scene、资源或关闭顺序。五联合版本13/21/24/2/2保持。正式选中mode记录投影留Q08，display/post-display、CPoint/OPoint/+2F8/revival/pieces等留Q06，正式DAT/角色图片部署留Q07；不能报告整个B5/B6/B7或总目标完成。

下一唯一Task：NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001，先只读核对native每slot资源→display→post-display及Unity实际caller和字段写入，冻结最窄后继；不要重做已验证HP/MP或跳Q07。

账本最终结果：Validate-ChangeLedger.ps1 PASSED，510 Records，当前10个governed code diff均被覆盖；既有历史Record路径不在当前diff的WARNING保留，不是本包失败。
