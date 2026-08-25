# R8-WP01G-R03 — physical input → movement → interaction joint runtime certification

> 建立日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE / NO GAMEPLAY DIFFERENCE OBSERVED / C++ FULL TRACE BLOCKED`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source（只读）

## Goal

在真实 `NTSD_Battle` Play Mode 中，把物理 W/S/A/D/J/K/L 输入从 Input System 一直追踪到30 Hz
`FrameInputSet`、角色key/prev/cooldown/combo、移动/跳跃/落地、碰撞候选、交互consume和current-DAT命中writer，
以联合运行时证据确认此前分模块修复连接后仍满足C++ Release live合同。

本包先验证、不预设修复。只有取得first difference并能在本包范围内闭合C++ source时，才为该差异建立
独立Task/Change Record后修改Unity脚本。

## Scope

### 允许

1. 只读核对C++ `input_handler.cpp`、`game_tick.cpp`、`frame_advance.cpp`、`physics.cpp`、
   `collision_collect.cpp`、`collision.cpp`与`hit.cpp`对应调用链；
2. 复用现有Editor-only真实InputSystem/Play Mode探针，不绕过`CharacterInputModule`或canonical `FrameInputSet`；
3. 验证普通方向输入、跳跃保速/落地、DDJ与DRA/DLA组合；
4. 运行现有held/grab/cpoint/collision/hit Play probes，核对真实world slot、generation、candidate、vrest、
   HP/HPBound/stat/HitConfirm2与清理；
5. 运行fresh compile、focused EditMode、full `BattleRuntimeSelfCheck`与Play cleanup检查；
6. 更新STATE、all-diff register、main plan、research和handoff。

### 禁止

- 不修改、运行、构建、复制或写入C++ authority；
- 不改DAT、技能目标frame、组合窗口或按键映射以让探针通过；
- 不关闭worker、改变30 Hz tick或绕开FrameInputSet作为production修复；
- 不改CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×、fixed camera、扩展容量、SoA/ECS、pool/0-GC；
- 不处理T8、IL2CPP、Android、服务器、F1/F2 debug或本包外D-ID。

## Authority / Evidence

- C++ physical/current-held contract：`src/input/input_handler.cpp`；
- C++ pass order：`src/entity/game_tick.cpp::game_tick(...)`；
- C++ movement/landing：`src/entity/frame_advance.cpp`、`physics.cpp`；
- C++ collection/hit：`collision_collect.cpp`、`collision.cpp`、`hit.cpp`；
- Unity input：`CharacterInputModule`、`LocalSimulationFrameInputProvider`、`SimulationTickDriver`、
  `BattleCharacterInputActionResolver`；
- Unity movement：frame-advance/frame-tick/physics ECS passes；
- Unity interaction：scene query、candidate runner、interaction/damage writers；
- existing probes：`BattleComboPlayModeProbeEditor`、`BattleHeldWeaponLifecyclePlayModeProbeEditor`、
  `BattleGrabCpointLinkPlayModeProbeEditor`、`BattleCollisionHitDamagePlayModeProbeEditor`。

## Required fixtures

### F1 — physical input edge and combo

- Queue physical keyboard state through Unity Input System；
- observe L→S→K (DDJ) and L→facing-forward→J (DRA/DLA)；
- require combo step1/step2 persistence, authored target frame and release；
- record tick/frame/combo/cooldown/object-count trace。

### F2 — movement / jump / landing joint

- neutral→A/D hold→jump edge→airborne horizontal motion→release→landing；
- record FrameInputSet held/pressed/released, runtime key/prev/cd, frame/state, X/Y/Z、Vx/Vy/Vz、Dir；
- verify jump retains the C++-defined horizontal velocity and landing writes the source-confirmed raw frame/history contract；
- if no existing probe covers this end-to-end path, stop before adding a probe and create a test/diagnostic-only Change Record。

### F3 — interaction / hit joint

- live held/pickup/throw/landing probe；
- live grab/CPoint/link/held injury probe；
- live frozen collision/hit/damage probe；
- require no stale slots/generations, deterministic candidate order, correct target current-DAT writer and cleanup。

## Deliverables

1. `docs/ai/RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`；
2. structured Play reports under `Temp/`（不提交为authority）；
3. 若有first difference：独立Task Contract、Change Record、Ledger/STATE/handoff登记与最小修复；
4. final matrix区分source、compile、focused、Play、C++ trace。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | C++ source caller/callee/field/order闭合；C++目录保持只读。 |
| S1 | Unity producer→FrameInputSet→runtime→movement→candidate→writer crosswalk闭合。 |
| S2 | focused EditMode tests全部PASS。 |
| S3 | fresh compile 0 error；full self-check PASS。 |
| S4 | F1/F2/F3真实`NTSD_Battle` Play reports PASS并恢复场景/driver/input状态。 |
| S5 | validator/diff-check；任务外阻塞必须独立报告。 |

## Stop conditions

- first difference指向DAT内容、渲染、AI、服务器、stage或其他包；
- 需要改变pass order、30 Hz、worker architecture或受保护adapter；
- 需要新增脚本但尚未创建独立Change Record；
- Unity scene/Editor/InputSystem前置无法恢复；
- C++规则无法由release live source闭合。

## Out of scope

C++ executable/full trace（继续BLOCKED）、T8、IL2CPP/Player build、Android、服务器、性能压测、F1/F2 debug。

## Authorization

用户于2026-08-23明确批准执行`R8-WP01G-R03`并恢复总目标。

## Execution result — 2026-08-23

- F1：physical DDJ→frame271与DRA→frame263均PASS；
- F2：physical D/K→Right→frame210/211/212→DAT jump velocity→airborne→release→landing PASS；
- F3：held weapon、grab/CPoint/link、collision/hit/damage三份live-world报告均PASS并恢复基线；
- 未观察到production gameplay first difference；仅新增Editor-only F2 probe；
- 证据报告：`docs/ai/RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`；
- final fresh compile/Console 0 error；focused EditMode 257/257 PASS；full self-check 17:17:19 PASS；
- Change Ledger validator PASS（78 records / 93 governed code files），scoped diff/whitespace check PASS；
- 本包完成并停止；未把缺少C++ executable/full trace的分支升级为runtime VERIFIED。

### Evidence repeatability correction

完成后审计发现current Temp曾被一次性synthetic首键采样失败覆盖，因此以
`R8-JOINTINPUT-PROBE-002`临时重开。有限release→press修正后current F2/DDJ/DRA三份报告fresh PASS；
final focused257/257、self-check17:33:15、Console0、validator79/93 PASS。本包现重新完成，production0改动。
