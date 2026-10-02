# NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001

状态：`FOCUSED_TEST_PASS / SYNTHETIC_FIXTURE_ONLY`。父项：新版336B44 G1/BATCH-04/Q07，来自Q07/C053声音投影扩展聚焦17项失败中的15项；总目标`NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

事实与权威：正式根EXE SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable `battle_world.cpp::BattleWorld28` 普通命中在被害者休止门后写 `target->bdefend_accumulator=45`（当前源码约6580行），正式Entity字段是`bdefend_accumulator`。Unity `BattleHitCandidateSequenceRunner` 对同一native ordinary未装甲路径写`target.Runtime.Bdefend=45`。既有`BattleHitExecutionPlanEditorTests.ShadowCompare_StandardType3DamageWriterEffectMatchesAuthorityState`合成OID7301的15种effect均在`target.HitStateCount==45`断言得到实际0，断言后的vrest等未执行。`HitStateCount`仅在OID300专项赋45，不能据旧移植测试推导所有type3目标都赋45。

范围：只在`Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`该方法内将错误的`target.HitStateCount==45`断言更为正式对应的`target.Runtime.Bdefend==45`，其它断言、参数、夹具及生产代码不改。此仅修测试契约，不宣称合成OID7301是正式DAT可达，也不处理另2项朝向/KnockbackVx聚焦失败。无DAT/图片/Scene/Prefab/ProjectSettings或非战斗修改。

**2026-10-02 同一方法内范围补充（第二次脚本编辑前）：** 首次更正后原Editor15例均越过45字段，新的首差在`target.GetHitRecordX(0)`，期望6/实际9。测试用旧`world.Rng.Seed`及其LCG求火花抖动；正式playable `append_confirmed_native_spark` 在当前命中路径明确调用`random.crt_next()`两次（先Y后X），Unity生产/投影读`world.NativeRandom` CRT。故本包再只更正本方法的随机种子、预计两次CRT状态/调用数断言为`world.NativeRandom`；其余命中断言、参数及生产代码保持。若新首差出现，先核权威再决定，不连环放宽。

**2026-10-02 同一方法内范围补充（第三次脚本编辑前）：** 第二轮原Editor15例均越过随机坐标与CRT调用断言，余下共同失败为`PendingSounds.Count`期望2/实际0。正式playable `battle_world.cpp` 的普通无甲路径只在`type0_target`分支追加基础和effect音效；攻击者为type3时可发其`weapon_broken_sound`，但本夹具攻击者是type0、受击者是type3，受击者`weapon_hit_sound`并非这条路径的声音源。因此本方法保留非空自定义受击音效作为阴性控制，将15项过时音效参数和断言更正为`PendingSounds.Count==0`；不修改生产、别的测试或DAT。第二轮失败JSON保留，仍只重跑本方法。

验收：修前原Editor方法组15/15在此断言失败的JSON保留；修后生成Editor编译0 error、原Editor只运行该方法组（非整套），要求15例均通过或如实记录新的第一失败。若仍失败，不放宽剩余断言；仅按当前正式源核判。原Editor保持非Play/Battle Scene clean/四保护SHA稳、Change Ledger通过。回滚需审阅准确测试diff与原始失败结果，禁止未经批准删除或Git恢复。

实际出口：第三轮原Editor同一方法15/15通过、0失败，原始结果`artifacts/diagnostics/NTSD28-336B44-Q07-TYPE3-BDEFEND-TEST-001/focused-status-v3.json`；生成Editor编译0错/249 warning，原Editor DLL时间晚于脚本。四个保护资产磁盘SHA与改前逐项相同，未启动第二Editor或进入Play。该合成OID7301测试只证明本方法在当前生产投影/实际路径无差；正式DAT可达、自然战斗和相邻两项 writer mask 不归此证书，Q07仍开放。
