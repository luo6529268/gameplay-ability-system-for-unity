# NTSD28-B3 C25g frame-body owner crosswalk

> Change：`NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001`
> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`
> Authority：`battle_world.cpp::step_frames_range`，playable closure `39DDDA15...6109`

## 1. 精确顺序与Unity现状

| 顺序 | Authority C25g | Unity当前owner | 审计结论 / 后续 |
|---:|---|---|---|
| 1 | clear per-tick render transition marker | `BeginNativeC25FrameTickForWorldPass` | `MATCHING_STRUCTURE` |
| 2 | skip missing/pending entity | runtime-slot current lookup；另有deferred mutation | lifecycle细节归B7 |
| 3 | `interaction_state < 0`整段held | `RunCommonFrameTick`/ECS均以`LinkState < 0`早退 | gate存在；held relation表现归B6 |
| 4 | nonzero motion hold整段早退，type3例外 | generic匹配；ECS exact character无type3例外需求 | `MATCHING_GATE`；`SuppressLateFrameTickUntilTick`额外gate归B7 |
| 5 | terminal physical slot0..19 type0 dead state14无revival时保帧 | Unity C25g没有等价terminal retain gate | `CONFIRMED_DIFFERENCE -> B4/B8` |
| 6 | type3 frame `hit_a/hit_d` body；kind2 cpoint bypass；state3007特殊 | generic只有普通positive hit_a，缺state3007与hit_d=0 fallback10 | `CONFIRMED_DIFFERENCE -> B4` |
| 7 | source frame sound list once，最多20，声明顺序 | Unity仅单`frame.sound`，且依赖frame-id/wait mirror | `CONFIRMED_DIFFERENCE -> B10/B11` |
| 8 | `frame_machine.step` counter/next | ECS exact + generic virtual两份算法 | owner存在；逐字段/双路径统一归B4 |
| 9 | real transition to212加载jump速度；raw next999不加载 | Unity有212 jump init，但需对exact/generic与raw999矩阵复验 | `REBASELINE_REQUIRED -> B4` |
| 10 | next999仅type0且`Y!=0 && Y!=collision_y_reference`到212，否则0 | Unity只看Y/type，缺collision-Y reference | `CONFIRMED_DIFFERENCE -> B4` |
| 11 | state14 exit按group/class/difficulty/no-blink table写render phase15 | Unity仍含旧OID算式；字段binding已正确 | `CONFIRMED_DIFFERENCE -> B4/B11` |
| 12 | destination sound在cost fallback前发出 | Unity transition callback只处理单sound | `CONFIRMED_DIFFERENCE -> B10` |
| 13 | destination negative MP/HP cost，waive/recmp/mode/double，失败取destination.next | Unity旧PP-only逻辑不等价，缺HP/effectiveMaxHP与完整倍率 | `CONFIRMED_DIFFERENCE -> B4/B11/H` |
| 14 | post-cost action越界标lifecycle pending，不立即free | Unity随后`HandleFrameTickExit`立即free | `CONFIRMED_DIFFERENCE -> B7` |
| 15 | surviving action110/114刷新defend cooldown=3 | ECS/generic均写`CdDefendLock=3` | `MATCHING_OWNER`，B4复验值/顺序 |

## 2. B3结构裁决

- C25g已在C25f之后、C25h之前，位于同一升序live-slot循环；B3不再移动它。
- exact-character ECS与virtual compatibility双实现必须在B4用同fixture A/B闭合，不能各自成为权威。
- 当前`RunReleaseFrameTickCounters`在native marker下no-op，C25h统一消费render/reaction/status；这部分已由前包验证。
- C25i仍未位于C25h与C25j之间；下一包先闭合其source/content依赖与B3 placement。

## 3. 后续包

1. `NTSD28-B3-C25G-FRAME-BODY-001 / VERIFIED`：exact/fallback core已统一，terminal hold、type3 state3007与Unity-only heavy早退已闭合；focused4、related85、broad432、SelfCheck PASS。
2. `NTSD28-B3-C25I-ARMOR-RECOVERY-OWNER-AUDIT-001`：只读确认placement、parser/content blocker与最小owner合同。
3. B4 frame-body behavior packages：next999/collision-Y、transition side effects/cost algorithm及剩余逐字段矩阵。
4. B7 lifecycle pending与birth-visibility。
5. B10 multi-sound/order；B11/H schema/content。
