<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: selected 336B44 playable battle_world.cpp ordinary bdefend_accumulator writer
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001.md
-->

# Type3合成命中测试的45字段合同更正

脚本前记录。Q07/C053扩展命中聚焦中该方法组15个effect均先在`target.HitStateCount`期望45/实际0失败；该测试行之后的vrest断言未运行。正式336B44普通命中写`bdefend_accumulator=45`，Unity生产映射是`Runtime.Bdefend=45`；合成OID7301不具有OID300专属HitStateCount写者。只更正单条测试断言读取口，测试其余条件和生产逻辑不动。准确范围、回滚和验收见Task。原始失败JSON在Q07/C053声音坐标包下保留。

实际编辑：仅`BattleHitExecutionPlanEditorTests.ShadowCompare_StandardType3DamageWriterEffectMatchesAuthorityState`内一行断言从`target.HitStateCount==45`改为`target.Runtime.Bdefend==45`；测试其它断言和所有生产代码均未改。Temp-only targets生成Editor工程`dotnet build --no-restore -v:q -clp:ErrorsOnly` exit0，250 warning/0 error，[日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001/offline-editor-build.txt)。原Editor导入和定向运行仍待，当前只`COMPILE_PASS`。

第一轮原Editor结果（覆盖上段待测快照）：15例全部越过原`HitStateCount`行，在后续`GetHitRecordX`期望6/实际9失败，[新首差JSON](../../../artifacts/diagnostics/NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001/focused-status.json)保留。静态追踪正式playable `battle_world.cpp::append_confirmed_native_spark` 明确以`random.crt_next()`先Y后X；Unity生产`BattleEcsHitExecutionPlan.ProjectSparkCrt`及命中记录也读`NativeRandom.CrtState`，旧测试只种`world.Rng`是下一处过时夹具。第二次脚本编辑前把本Task范围扩为同一方法内CRT种子/状态断言的更正；不改其它测试或生产。第一轮失败不得记PASS。

第二轮原Editor结果（第三次脚本编辑前）：生成Editor编译0错/249 warning，原Editor刷新后程序集时间晚于脚本；精确方法组15例全部越过45字段、火花坐标、CRT状态与调用数断言，停在`PendingSounds.Count`期望2/实际0。[第二轮JSON](../../../artifacts/diagnostics/NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001/focused-status-v2.json)保留。正式playable普通无甲声道追加只在`type0_target`分支，另可由type3攻击者携`weapon_broken_sound`；本夹具type0攻击者→type3目标，目标的自定义`weapon_hit_sound`不触发该路径。Task已在脚本前扩围为只更正本方法的过时音效参数及零音效断言，保留自定义cue作阴性控制，不改生产或其他测试；第二轮不得记PASS。

实际最终脚本范围：仍只修改`BattleHitExecutionPlanEditorTests.ShadowCompare_StandardType3DamageWriterEffectMatchesAuthorityState`：45字段断言改`Runtime.Bdefend`；旧共享RNG夹具改原生CRT seed/两次状态与调用数；移除15例过时音效参数并断言本type0→type3路径`PendingSounds.Count==0`，保留目标自定义`weapon_hit_sound`阴性前置。第三轮生成Editor工程编译0错/249 warning，原Editor同项目程序集更新后精确方法组[15/15 PASS](../../../artifacts/diagnostics/NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001/focused-status-v3.json)，0失败。原Battle/Menu Scene、GameConfig、ProjectBattleModeConfig四资产SHA与本包改前逐项相同，未运行Play，未改生产/资源/DAT。状态`FOCUSED_TEST_PASS`仅此合成fixture；正式DAT可达自然链、另2 writer mask、Q07整组仍未验收。回滚须审阅本方法diff与三轮原始结果，不删除记录。
