# Q07/C053 命中投影音效坐标共用入口

状态：`SCOPED_SCENE_PASS / SOUND_FOCUSED_PASS / ADJACENT_TEST_FAILURES / RUNTIME_PENDING`。只在三人受控自然双命中原 Battle Scene 出口，`ShadowCompare` 的音频 bit63 首差已消失；17个共用投影调用的其它组合尚不能由此宣称全部对齐，Q07/C053与总目标开放。

RED：[前包](../NTSD28-336B44-Q07-C053-SHADOW-SOUND-FIELD-001/REPORT.md)首次 writer 期望世界X897、生产实际X584、正式336B44原始事件X584，其它独立音频字段相同。修正只在 `BattleEcsHitExecutionPlan.ProjectQueuedSound` 的统一投影末端复用生产 `NTSDEntityRuntime.ResolveBattleSoundWorldXInt(physicalFallbackX)`；17处实体声音调用显式传真正发声的 attacker/target，保留原fallback X。真实 `QueueBattleSound`、cue/队列顺序/数量、DAT值与资源没有改动。无角色或音效ID特判。

GREEN：[原 Battle Scene 结果](ank580-ank580-jira500-shadow-sound-green-01.json) 为 `SCOPED_PASS / DONE`，相对tick1～12的[正式/Unity逐字段对照](source-unity-comparison.json)为战斗 **132/132**、实际待播音效 **42/42**、合计 **174/174** 首差0。tick7两条OID875→OID808自然Damage候选均观察到；`ObservationMismatchCount=0`、`FailureCount=0`、`CurrentTickPlanValid=true`、writer掩码0、首次声音差异空，目标HP450、两攻击者rest10。旧RED与Q10无Shadow音频结果保留且未覆盖。初始三角色action为受控设置，并非玩家物理键自然选招。

生成工程 Editor 脚本编译 exit0、281 warnings/0 errors；原Editor刷新后完整 Play 通过，退出后 Editor idle/非Play、Battle Scene clean。Battle/Menu/GameConfig/ProjectBattleModeConfig 的[前](protected-before.json)/[后](protected-after.json)四SHA一致，随后聚焦测试后复核亦一致。Change Ledger exit0、PASSED（1122 Records/当前diff 3脚本均COVERED），`git diff --check` exit0，仅Git的行尾转换提示。

定向EditMode `ShadowCompare_NonConvertedKind9WeaponOnlyRecordsEffectSound` [1/1 PASS](sound-focused-status.json)，覆盖原无源规则坐标的音效fallback。另选的三个命中计划方法组[34项中17项失败](focused-tests-status.json)：15项 `ShadowCompare_StandardType3DamageWriterEffectMatchesAuthorityState` 的直接断言为`target.HitStateCount`期望45/实际0；2项 `ShadowCompare_StandardCharacterDamageWriterEffectMatchesAuthorityState` 的writer掩码分别为`0x200020000000`、`0x200000000000`，均**不含音频bit63**。这些失败不是本次声坐标字段的通过证据，也没有在本包改断言或其他战斗writer。它们是否与336B44正式可达规则不符仍需独立同初态核验；本包因此保持`RUNTIME_PENDING`，不能称17处所有分支均验证完成。

字段定位更正：先前口头及初版报告把15项`45/0`称作vrest，逐行复核测试表明真正失败断言是`target.HitStateCount`；`world.GetRawRestVrest(1,0)==3`在该断言之后，尚未执行。原始测试JSON不变，按本更正开展后续权威审计。

未验证：其它实体/比例组合的所有声音投影、物理键自然选招、整场/全World、实际声音播放与设备、Q07/Q10整阶段。后继优先按当前336B44权威审查上述非音频聚焦失败，再对改变过的未覆盖音效分支做最窄可达证据；不得以修本包为由改DAT或非战斗功能。
