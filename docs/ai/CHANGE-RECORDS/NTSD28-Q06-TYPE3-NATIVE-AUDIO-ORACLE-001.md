<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001
status: VERIFIED
change-kind: FOCUSED_TEST_ORACLE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3MatchedPairEarlyBranchEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current playable battle_world.cpp unarmored noncharacter audio: attacker type3 emits only nonempty definition weapon_broken_sound at this point; effect23 has no generic cue.
evidence: DifferentPairStates_ContinueOrdinaryDamagePath in job f25fb55daf6941ca8c49a4cbe64d36b1 expects1 PendingSounds but actual0 after removal of non-native generic effect sound; fixture defines no sound resource.
-->

# Type3原生命中音频测试预期修正

PLANNED。仅上述测试DifferentPairStates_ContinueOrdinaryDamagePath的PendingSounds预期由1改0，其他HP/HitCount/spark断言保留。读取实际fixture CreateEntity确认没有weapon_broken_sound/weapon_hit_sound；不为测试加入生产通用音效。等当前回归job终态归档后才编辑并补跑。回滚仅本断言，保留失败原XML；不改生产、Scene/资源/非战斗，禁止computer-use。子项完成不等于parent reduced或全部type3音频通过。

原回归终态18项17PASS/1旧音频FAIL，原984四组/684early/Bdefend及prelude replay均通过。XML归档parent regression-18-one-old-audio-fail.xml。现只修正该1断言，待编译/补跑。

VERIFIED / TEST_ORACLE_ONLY。job4b866e2569774961ad93f303a054d398终态4/4 PASS，parent/type3-audio-oracle-4-pass.xml归档。仅PendingSounds断言修改，非生产变化；旧失败保留。

重新打开同一音频预期修正：SelfCheck16:28:19Z BATTLE-C30失败，fixture attacker type5、target type3配置weapon_hit_sound，旧断言要求generic+target两声。source battle_world.cpp6765仅attacker3自带broken资源，6986仅target0内置音频，非角色target weapon_hit_sound只存在reduced target0音频函数的其它分支，不属于本路径。需要parent生产同时移除target3自带hit声，SelfCheck该组合应零声音，其余伤害/rest/action/速度断言保留。旧4/4证据不抹除；本次追加SelfCheck路径，待补跑。

再次VERIFIED / TEST_ORACLE_ONLY：parent修正type3 target自带hit声及对应标准/D1预测后，987+type3联合27/27 PASS，完整SelfCheck2026-09-14 16:36:40Z PASS。保留此前旧4/4和C30失败；本子项两测试文件，生产改动仍归parent。
