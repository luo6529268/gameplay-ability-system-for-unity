<!-- CHANGE-RECORD
id: NTSD28-Q06-DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5RecoveryStatusConsumerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NegativeEnvironmentRecoveryProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Q06 original-source MP matrix and four-mode causal capture; default selected_mode_mp_regen_gate_2c=1 suppresses nonnegative regen, resourcePhase3 is world-owned.
evidence: artifacts/diagnostics/NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001/REPORT.md; final-119-results.xml; native-mp.tsv; SelfCheck-final.result; play-mp-pass.json; play-cleanup-pass.json
-->

# Q06 默认模式恢复夹具纠正

IN_PROGRESS / TEST_ONLY。准确三个测试脚本，生产MP规则保持。105项中33失败来自旧无stats/default模式仍期望普通MP增长（11个场景×Legacy/ECS/Derived），另1项把host tick1当作资源phase3非0，而World字段仍0。完整SelfCheck的ExpectRecoveryFixture也仍断言默认MP0→4，实际失败待当前运行结果归档。

修正策略：仅改变被mode默认1否定的普通MP期待值，保留WeakTimer12C在HP12分支中的+1、HP恢复、timer递减、negative timer保持、PP150/151门槛及cap断言。测试fixture显式设置World.ResourcePhase3=tick%3/Phase12=tick%12，以构造想测试的phase；错误phase负环境测试同时把Phase3设1，保留其no-op目的。SelfCheck默认模式应HP+1、MP保持0，不再声称默认MP恢复；共享角色与真实角色类型覆盖保持。

MP bonus在mode允许时的加值已由父2028原版向量覆盖；父新增测试将用regen_mp=-7（原版mode不抑制的正向族）验证bonus最后一tick先消费后递减，防止仅更新默认模式预期而丢失timer顺序覆盖。该新增测试属于父已声明文件，不在本Record下修改。

逐项留存原失败，不删除测试、不扩大生产范围、不改DAT/Scene/资源/配置/非战斗。测试数据是明确fixture，不冒称正式Logan角色画面。准确preimage/hash在同名artifact。验收三个fixture、父focused/恢复回归、完整SelfCheck及账本；回滚需批准仅本差量。没有新Runtime生命周期或schema，Q06未完整关闭。

实际SelfCheck已在GT-06 real character的旧PP==4断言失败，结果归档SelfCheck-initial-failure.result。现已写11行默认MP期待值、两个World phase fixture及SelfCheck该断言/描述；HP和weak例外值全部保留。待实际复跑，不把预期修订本身当通过。


## 当前限定出口

VERIFIED / VERIFIED_TEST_ONLY。最终119/119通过，2028 native MP向量/同内容44字段一致且只剩6MISSING、完整SelfCheck及实际MP/恢复/关闭全0通过。完整报告见artifacts/diagnostics/NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001/REPORT.md。Scene独立HUDBg x50→30变化来源待用户确认，未覆盖/不宣称hash unchanged；下一HP独立工作可继续。Q08正式模式投影、其余Q06及总目标未完成。
