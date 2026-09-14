<!-- CHANGE-RECORD
id: NTSD28-Q06-HIT-SPARK-SELF-CHECK-ORACLE-001
status: VERIFIED
change-kind: TEST_FIXTURE_AUTHORITY_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 源append事务需要World CRT上下文、target Z；独立旧selfcheck未注册entity使用旧fallback随机及attacker Z范围。
evidence: Fresh full SelfCheck or 22-test lifecycle run failed; original failures archived under HIT-SPARK-UNITY-001.
-->

# 火花测试前置纠正

IN_PROGRESS。CheckKind0HitRecords限定纯endpoint测试，显式独立World+seed并直接调用公共Append；保留owner/capacity/ID断言，将Y范围按源target Z改为-37..-29。实际Record入口由新282×2路由测试负责。

仅精确测试脚本，不改生产、schema、Scene、资源、框架或断言强度。验收对应测试/完整SelfCheck、ChangeLedger。回滚仅本差量并遵守用户授权规则。禁止computer-use。

08:02:15Z第二次完整SelfCheck已越过Kind0检查，在CheckWeaponVictimRawFrameWriterContract的旧legacy RNG+2断言失败。修改前扩展准确同文件三个方法：该方法、CheckWeaponAttackerRawFrameAndOrderingContract、CheckWeaponTailIdentityTimingContract共七对旧spark NextInt(0,9)，现有每次hit仍从同Record wrapper生成；改为独立CRT delta2并保留武器frame/legacy随机次数等其余兼容回归断言。武器反应本身未在本包修复/宣称对齐。

VERIFIED / TEST_ONLY。准确Kind0 endpoint前置和三个武器回归中的七处spark RNG预期已修正；原owner/capacity/frame/时序/legacy武器随机断言保持，另加CRT delta2。08:06:43Z完整SelfCheck PASS（request08:05:42Z），两次旧FAIL保留。无生产变化。
