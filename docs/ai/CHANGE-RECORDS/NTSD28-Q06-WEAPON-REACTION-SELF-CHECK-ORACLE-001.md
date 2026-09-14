<!-- CHANGE-RECORD
id: NTSD28-Q06-WEAPON-REACTION-SELF-CHECK-ORACLE-001
status: VERIFIED
change-kind: CURRENT_AUTHORITY_TEST_ORACLE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06WeaponReactionEditorTests.cs
authority: WEAPON-REACTION-SOURCE-WITNESS-001 source2100 and current playable unarmored weapon reaction/post tail; UNARMORED-WEAPON-REACTION-001 production8400 PASS.
evidence: 2026-09-14 10:16:55Z full SelfCheck fails BATTLE-AUDIT7-F7 legacy HitConfirm2=1 assertion; old R4 raw victim random/team/self-rest and two-stage attacker post assertions disagree with current source.
-->

# 武器SelfCheck旧权威断言修订

IN_PROGRESS / TEST_FIRST。先在已有weapon Editor测试中定向反射运行准确旧检查（Audit7 carrier、WeaponVictimRawFrame、WeaponAttackerRawFrameAndOrdering、WeaponTailIdentityTiming），保留失败。然后仅按当前源纠正相关旧期望，保留PN/counter/wait、资源、CRT、RNG与副作用断言，不删除覆盖。需显式测试无旧hit-confirm写入/阵营不变/无victim随机/正确native同步post顺序。

不改生产、Scene、资源、schema、GAS或非战斗。风险是旧fixture混合生命周期与模拟含义，遇到不一致先核对实际入口和源顺序，不能机械改期望。验收为定向检查+完整SelfCheck；回滚仅本差量，旧失败永久保留且不得回退其它既有修改。

CODE_WRITTEN：四个精确SelfCheck方法已按源更新；carrier先断言命中不写HitConfirm2，再显式注入旧carrier=1保留原C25清理覆盖。raw victim仍验证PN/counter/wait/CRT/资源；取消旧随机/team/self-rest期望，新增Fall80。state1002用只读Native cursor预测0xEE/16并比较调用数/counter/index/site；state3000只执行一次post，不因新frame为state1002再耗随机。定向4方法原全部FAIL已存red-four-methods.xml，生产未再改。

定向四方法PASS；完整SelfCheck10:22:33Z第二处旧期待为CheckCurrentWeaponDatOnSpecialShell.HitConfirm2==1，source DAT类型1而CLR为Special。增加该方法准确oracle修订，保留所有HP/stat/durability断言，并将其所属CheckAudit4ArchitectDefectContracts加入定向入口以覆盖后续既有检查。

扩大定向父检查后明确下一旧断言在CheckWeaponHitResolveAuditContracts：C27旧self-rest3/19/30，C28旧通用音效+target weapon_hit，均由本批原source排除。准确增加该方法的oracle修订，保留原伤害、vrest、位移与状态断言；C26原随机range断言应收紧为低fall原action不变。

FOCUSED_TEST_PASS：c1818dfb38ac4371bee3fec0602626ce终态7/7（5个oracle入口含完整Audit4Architect聚合+父整数坐标2例）。所有修改均测试逻辑；完整SelfCheck最终重跑中。

完整SelfCheck10:27:55Z第三处旧断言为CheckAudit7IronBallPreprocessContracts，期待kind0 heavy dvx7/dvy-5减半为3/-2；原2100的effective itr均证明应保持。准确增加该单方法修订和定向入口，保留源ITR不变、kind/injury、其它目标及RNG不消费断言。

合并最终48/48 PASS，含六个oracle测试入口。准确修改七个SelfCheck方法：Audit7HitConfirmCarrierTail、CurrentWeaponDatOnSpecialShell、WeaponHitResolveAuditContracts、WeaponVictimRawFrameWriterContract、WeaponAttackerRawFrameAndOrderingContract、WeaponTailIdentityTimingContract、Audit7IronBallPreprocessContracts。所有旧失败保留，完整SelfCheck最后重跑中。

VERIFIED / TEST_ORACLE_ONLY：最终完整SelfCheck于2026-09-14 10:33:50Z实际PASS，fresh result已归档；之前三次FAIL与定向RED保持。生产不属于本子项修改。
