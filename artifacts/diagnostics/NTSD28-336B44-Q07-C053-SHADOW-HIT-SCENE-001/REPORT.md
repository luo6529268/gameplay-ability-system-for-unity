# C053 自然双 Uj 原 Scene 的只读 hit plan 观测

2026-10-02 后继更正：同初态的正式 `audio_events` 与 Unity 实际 `PendingSounds` 已完成逐事件对照，12 tick 的 10 条事件路径、X、顺序及计数 **42/42 相同**；详[Q10 音频审计](../NTSD28-336B44-Q10-C053-DOUBLE-HIT-AUDIO-AUDIT-001/REPORT.md)。因此下文“逐音效未核/下一步核音效”只是不同时点快照。这里的 bit63 仍是 ShadowCompare 内部投影首差，不能作为生产音频不同的证据；具体 bit63 分量尚待诊断观测。

2026-10-02 再后继更正：bit63 已直接定位为投影世界X897/实际及正式X584（指纹随X差）；[共用坐标Green包](../NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/REPORT.md)使同原Scene两自然Damage的ShadowCompare为0差、所选战斗/音效174/174。下文RED两轮是修正前真实历史，原结果不覆盖；Green后仍有非音频相邻聚焦失败，C053/Q07整体开放。

状态：`RUNTIME_PENDING / AUDIO_SHADOW_FIRST_DIFFERENCE`（2026-10-02）。本包在上一轮[自然双 producer 原 Scene 限定出口](../NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-SCENE-001/REPORT.md)上，只在 Play 副本的 World tick0 启用现有 `BattleHitExecutionPlanMode.ShadowCompare`，不切换实际战斗 writer、不修改正式 DAT、生产逻辑或 Scene。正式权威仍为根 SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable live path。

生成 Editor 工程定向编译两轮均 0 error（第二轮 250 warning），原 Editor refresh 后 DLL 时间晚于探针。两个唯一结果原件分别为[首次 01](ank580-ank580-jira500-shadow-hit-scene-01.json)和[补采失败原因 02](ank580-ank580-jira500-shadow-hit-scene-02.json)，没有覆盖上一轮无 ShadowCompare 的 JSON。每轮均在同初态运行 12 个生产 Driver tick，所选 11 字段×12 tick 对正式源码均 **132/132 零差**（[01](source-unity-selected-comparison-01.json)、[02](source-unity-selected-comparison-02.json)）。两轮均 Play 退出 clean；Battle/Menu/GameConfig/Mode Asset 的 [01 事前](protected-before.json)/[01 事后](protected-after-01.json)及[02 事前](protected-before-02.json)/[02 事后](protected-after-02.json)四 SHA 稳。

第二轮相对 tick7 的 hit plan 直接观察到两条按顺序的 `attacker51,52 / Object pass / target50 / kind0`，各自 `ExpectedDisposition=Damage`、`ObservedDisposition=Damage`、`PreprocessObserved=true`、`ConsumeEffectsObserved=true`，各条 expected/observed consume fingerprint 相同；目标 HP450、两个独立 rest10。正式源码同 tick 记录两条 `applied/effect2/Uj156`。Unity 本探针未逐条导出 effect 值或 Uj action 写入瞬间，不能将源码的这两个字段直接列为 Unity 内部同态。

`ShadowCompare` 仍在 tick7 报 `ObservationWriterEffectMismatch` 两次，首次 attacker51/candidate0，`LastWriterEffectDifferenceMask=9223372036854775808`（仅 bit63），其余 consume/first-body/lifecycle 掩码为0。因此本诊断的完整出口**没有通过**。现有 [DifferenceMask](../../../Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs) 把 bit63 专用于待播音效数量/指纹/cue/worldX/tick/队列计数与拒绝计数；不是动作、HP、rest或运动字段。Unity 实际 [QueueBattleSound](../../../Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs) 经 `NTSDEntityRuntime.ResolveBattleSoundWorldXInt` 选择已初始化的源规则 X，而 hit plan 的 `ProjectQueuedSound` 多处仍传 `Runtime.XInt` 物理坐标。两者在当前放大位移场景可能不同，是 bit63 的**代码级候选原因**；本包尚未导出两侧逐音效期望/实值，不能断言 bit63 仅由 X 一项造成，更不能据此断言正式版音频已经或尚未一致。下一步按 Q10 做同 tick 正式源码与 Unity 待播音效序列核对，再决定是否修投影或实际音频。

原 Scene 132/132 末态限定证据仍有效；C053/Q07/总目标不关闭。初始 action 受控、物理键自然选招和完整 World 仍待。
