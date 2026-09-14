<!-- CHANGE-RECORD
id: NTSD28-Q06-BDEFEND-TEST-ORACLE-001
status: VERIFIED
change-kind: NATIVE_BDEFEND_SELFCHECK_ORACLE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current native +0x0B8 maps Runtime.Bdefend; source256 overwrite/signed-add/armor gates verified, legacy HitStateCount is not that field.
evidence: Fresh SelfCheck07:14:34Z fails StandardCharacterDamageAlignmentContracts because its snapshot still captures/asserts legacy HitStateCount45 after producer migration.
-->

TEST_ONLY。准确更新StandardCharacterHitSnapshot及其私有case初始化/断言到Bdefend；C30无护甲type3的45断言；原AlternateDamage测试中模拟压力的初始化/31累加断言和说明。保留HP/PP/统计/rest/动作/原共享对照断言，不改legacy HitCounters API独立测试。不全文件替换所有HitStateCount，OID300/kind7其他fixture不在自动范围。

原FAIL保留；完整SelfCheck实际重跑后才能验证。单测试文件，无生产/schema/Scene/资源副作用。回滚仅本测试差量且按规则授权；禁止computer-use。

实际已更新准确StandardCharacter私有snapshot/fixture、C30及Alternate压力输入/输出字段。原07:14:34Z FAIL保留，重跑请求07:17:48Z、07:18:42Z完整SelfCheck PASS。仅自检观测纠正VERIFIED，父256扩展失败未关闭。
