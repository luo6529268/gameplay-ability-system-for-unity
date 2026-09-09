# NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001 — Task Contract

Goal13b用户明确授权，先独立守卫包，再收口包1，随后包2，最后一轮共享回归。IN_PROGRESS / TEST_FIRST，脚本修改前建立。

Authority为当前B1E13AE1正式EXE对应playable BattleWorld28::step_frames_range，battle_world.cpp:8556-8578。源码原文：
> A kind-2 child is updated by FUN_00417F80 from its holder's
> wpoint. Its on-hand frame commonly has wait=0,next=1000, but
> native traces prove that action remains active for the entire
> relation instead of entering ordinary lifecycle resolution.

## 原状、谓词与精确修改
Unity HandleFrameTickExit 的frameId<0或>=MaxFrameIdExclusive无条件Free，Sakon -888在C09/C20 preserved后C25误Free，用户已复核根因。
仅在该Free分支前加guard，返回true使当前late slot停止普通后续处理，不写状态，不调用Free。11/12 frameGroup分支和MaxFrameIdExclusive判定完全保留。
复用SimulationQueryAndLinkModule.HeldObjectProcessAll的入口谓词：world.IsActiveForCurrentPassInternal(entity) && entity.Runtime.LinkState < 0。既有RuntimeSlots/current-pass基础设施定义active；LinkState即Authority interaction_state的canonical signed carrier。C25 Run已有active occupant门，guard显式复用该方法，不建立第二关系规则。
该入口谓词与reciprocal-valid检查分开：负关系即held候选；holder缺失/越界/mismatch由held pass记录failure并preserve，其C25仍满足Authority interaction_state<0。不得改用ResolveActiveHolderSlotIndex>=0或强加reciprocal才能held，避免把invalid-preserve变成清理。guard不提前拦截C09/C20，也不吞诊断。非held/已release LinkState>=0仍Free。

## 授权范围
- Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs，仅HandleFrameTickExit负/越界分支guard。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldNegativeFrameLifecycleGuardProductionEditorTests.cs及meta，新focused。
- 本Task/同ID Record、Ledger/STATE/对齐总表、Temp证据。
其余原Goal13授权保持；本守卫不改原72测试/probe、terminal/kind3/callback/资源/Scene/架构/上界，未启动包2。

## 验收、风险、停止、回滚
test-first真实RED：held负action完整tick仍存活；非held负action、release后负action仍Free控制组，invalid/mismatch仍诊断+preserve；补real/generic、负/upper边界及11/12不变。
focused通过后复跑原Sakura10/Sakon-888真实driver Play，均Free0/live/linkPreserved/下一child正常；原72复跑。随后串行包2与共享最终守卫/72/包2/B6(275+新增)/refill9/kind3 92/terminal80/fullSelfCheck/双build/validator/Scene。
SceneSHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11；Editor b1b02287/2022.3.62f3/NTSD_Battle，不第二实例。
任一旧测试或控制组变化、谓词需扩文件、清单外diff/Scene变化即停。风险为过宽guard吞普通Free，必须非held及release控制；guard过窄破坏invalid-preserve，必须invalid诊断保持。
无新持久化、queue、manager或关闭owner；保留十一阶段关闭，fixture finally清理自身handle。
回滚须用户批准，仅本包增量；不能回退Goal1-13/用户内容，不改既有terminal/kind3合同。

Goal13b最终状态：VERIFIED_HELD_LIFECYCLE_GUARD_SUBSET / RED6_FAIL_11_CONTROL_PASS / FOCUSED17_PASS / SAKURA_SAKON_DUAL_PLAY_PASS / B6_388_PASS / REFILL9_PASS / SELFCHECK_PASS / BUILDS0 / CONSOLE_SELFCHECK_DIAGNOSTICS_RETAINED / SCENE_UNCHANGED。详细证据以同ID Record最终共享验收为准；原BLOCKED/PLANNED等历史事实不删除。GOAL14_USER_HOLD。
