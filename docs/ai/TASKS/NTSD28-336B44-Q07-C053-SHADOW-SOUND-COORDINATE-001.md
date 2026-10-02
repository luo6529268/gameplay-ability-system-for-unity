# NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001

状态：`RUNTIME_PENDING / SCOPED_SCENE_PASS / SOUND_FOCUSED_PASS / ADJACENT_TEST_FAILURES`。父项：新版336B44 G1/BATCH-04/Q07/C053及`NTSD28-UNITY-BATTLE-REALIGNMENT-001`。

问题：原Battle Scene三人自然双命中在ShadowCompare首次音频writer期望X897、真实队列X584；正式336B44 playable同条audio_event世界X584。除随X变化的指纹外，音效Count/Cue/Tick/Queued/Rejected全部相同。实际 `LF2Entity.QueueBattleSound` 使用共用 `NTSDEntityRuntime.ResolveBattleSoundWorldXInt`，只读 `BattleEcsHitExecutionPlan.ProjectQueuedSound` 的17处实体音效投影仍传比例后物理X。不能改 DAT 或真实队列；应让投影在同一实体/同一 fallback X 下使用生产已有共用坐标入口。

范围：只改 `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs` 的所有实体源 `ProjectQueuedSound` 调用和方法签名，显式传入当前真正发声的 attacker/target，统一调用 `emitter.Runtime.ResolveBattleSoundWorldXInt(physicalFallbackX)`；保留 fallback 坐标以及现有cue/顺序/计数。`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053NaturalDoubleBattlePlayProbeEditor.cs` 只换独立Green请求/结果及ShadowCompare零差验收条件。此前RED、Q10事件及诊断原件不覆盖。无其它生产路径、DAT/图、Scene/Prefab/ProjectSettings、非战斗变动。

权威：正式根EXE `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，对应playable `WorldAudioEvent28.world_x`；原Unity同案例真实队列10事件42/42同正式。现有 D-024 比例域和 Q10 音效X共用入口是实现约束；不从旧版判断。

出口：生成工程/原Editor编译0错；原Battle Scene同三人受控初态12生产Driver tick，所选战斗字段132/132同正式；tick7两条自然Damage/consume并ShadowCompare writer/整体Mismatch=0，实际待播10事件仍42/42同正式；再运行适当的命中计划聚焦测试，原Editor退出clean、四保护SHA稳，Change Ledger通过。若其它分量首差露出，保留结果并如实报告，不能通过放宽断言标PASS。此限定出口不证明物理键选招、其它角色、音频播放、全World或Q07整体。回滚需审阅本包具体diff及原件，删除/restore另需明确授权与记录。

实际出口：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/REPORT.md)。原Scene 174/174、tick7两自然Damage及计划零差，直接声音focused1/1通过、四SHA稳定；扩展命中聚焦34项中17项非bit63/非音频字段失败，留给独立336B44可达审计。只记限定场景通过，父项不关闭。
