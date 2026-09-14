<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE3-NATIVE-AUDIO-ORACLE-001
status: VERIFIED
change-kind: FOCUSED_TEST_ORACLE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3MatchedPairEarlyBranchEditorTests.cs
authority: Current playable battle_world.cpp unarmored noncharacter audio: attacker type3 emits only nonempty definition weapon_broken_sound at this point; effect23 has no generic cue.
evidence: DifferentPairStates_ContinueOrdinaryDamagePath in job f25fb55daf6941ca8c49a4cbe64d36b1 expects1 PendingSounds but actual0 after removal of non-native generic effect sound; fixture defines no sound resource.
-->

# Type3原生命中音频测试预期修正

PLANNED。仅上述测试DifferentPairStates_ContinueOrdinaryDamagePath的PendingSounds预期由1改0，其他HP/HitCount/spark断言保留。读取实际fixture CreateEntity确认没有weapon_broken_sound/weapon_hit_sound；不为测试加入生产通用音效。等当前回归job终态归档后才编辑并补跑。回滚仅本断言，保留失败原XML；不改生产、Scene/资源/非战斗，禁止computer-use。子项完成不等于parent reduced或全部type3音频通过。

原回归终态18项17PASS/1旧音频FAIL，原984四组/684early/Bdefend及prelude replay均通过。XML归档parent regression-18-one-old-audio-fail.xml。现只修正该1断言，待编译/补跑。

VERIFIED / TEST_ORACLE_ONLY。job4b866e2569774961ad93f303a054d398终态4/4 PASS，parent/type3-audio-oracle-4-pass.xml归档。仅PendingSounds断言修改，非生产变化；旧失败保留。
