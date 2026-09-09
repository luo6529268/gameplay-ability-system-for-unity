# NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001 — C25g frame-body owner audit

<!-- CHANGE-RECORD
id: NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp step_frames_range/step_frame_slot and frame_machine.cpp; EXE B1E13AE1, playable closure 39DDDA15.
evidence: C25G-ORDER-CLOSED / UNITY-OWNER-MAPPED / IMPLEMENTATION-SPLIT-DEFINED / NO-CODE-ASSET-SCENE-AUTHORITY-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / C25G_OWNER_AND_ROUTE_COMPLETE`

## 已观察事实

- Authority C25g在单一升序slot事务内执行：pending/relation/hold/terminal gates → type3 HP body → source sound → frame machine → 212 side effects → next999 → state14-exit render phase → destination sound → negative transition costs → invalid action pending → defend cooldown。
- Unity生产入口已位于同一`BattleLateEntityLifecycleModule` slot循环，exact character走`BattleEcsCharacterFrameTickPass`，其余走`SimFrameTick/RunCommonFrameTick`；因此B3的结构位置已具备。
- 两条Unity路径仍含confirmed behavior differences；不能把“已在per-slot调用”写成完整C25g行为对齐。

## 结论

- B3不再新增第二个frame-body writer，也不把`SimFrameTick`移回global barrier。
- gate/type3/state3007/next999/blink/transition语义归B4；terminal pending/free归B7；多sound与同ticksource+destination顺序归B10；negative MP/HP cost的DAT/schema与内容依赖归B11/H。
- `SuppressLateFrameTickUntilTick`是Unity birth-visibility适配，必须在B7以同tick birth scenario证明，不能在本审计删除。
- C25i仍在C25g与C25j之间缺production owner，独立处理。

## 验证

- 只读闭合Authority `battle_world.cpp:170..250,894..1007,8529..8700`、`battle_world_tests.cpp`的motion-hold、defend、audio、type3 drain与terminal fixtures。
- 只读闭合Unity `BattleLateEntityLifecycleModule.Run`、`BattleEcsCharacterFrameTickPass`、`LF2Entity.RunCommonFrameTick`及四类entity override。
- 无C#/asset/Scene/Authority写入；不运行Unity行为测试，不冒充implementation certificate。
