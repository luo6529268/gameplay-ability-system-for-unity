# HANDOFF — R8-WP01C production combat object certification

> 日期：2026-08-23  
> 状态：`COMPLETE / 01-06 UNITY S4 VERIFIED / 07 SYNTHESIS COMPLETE`

## Completed in this planning package

- 已将父任务中宽泛的 interaction/opoint/lifecycle Play Mode 范围拆成 7 个有依赖顺序的认证包；
- 已固定 `01→02→03→04→05→06→07`，避免对象/slot/RNG/link 残留污染后续结论；
- 已为每包写明 Goal、Scope、主要 D-ID、Deliverable、Verification 和 Stop condition；
- 已明确认证发现差异时只登记 first-difference 并停止，不在认证包中顺手修复；
- 已明确逻辑挂点归 WP01C，中央像素/阴影/透明排序/可见挂点归 WP01D；
- 本次没有修改 Unity/C++ 脚本、场景、资源、DAT、项目设置，也没有运行 Play Mode 或测试。

## Next executable package

`R8-WP01C-01 — opoint birth / newborn / basic lifecycle`

它先覆盖 character、weapon、special attack、other/effect 四类 producer 的出生 action frame、Prev2、
slot/generation、same-pass/next-pass 可见边界和 active/pending/destroy/reuse。后续 pickup/held/hit/death
认证都依赖这一包。

## Approval boundary

当前只完成规划。必须等待用户明确批准启动 `R8-WP01C-01`。批准后：

1. 先做只读 preflight，确认 scene/DAT/现有 probe 是否足够；
2. 如果不需要脚本改动，直接运行最小 Play 序列并保存证据；
3. 如果需要新增/修改 probe 或 test 脚本，先创建独立 Task/Change Record、同步 Ledger/STATE/handoff；
4. 如果发现 production 差异，建立新 D-ID/repair WP 后停止 01，不自动改 gameplay；
5. 01 通过后仍需按授权边界决定是否进入 02。

## R8-WP01C-01 execution update

- 用户已于2026-08-23明确批准“执行 R8-WP01C-01，恢复目标”；上方approval pending口径已解除；
- 只读preflight确认W05隔离EditMode证据不能替代live S4；
- `R8-OPLIFE-001 / IN_PROGRESS` 已建立，唯一允许脚本范围为新增Editor-only
  `BattleOpointLifecyclePlayModeProbeEditor.cs`；
- probe将使用live world、正式catalog/factory/structural writer/slot table/pools认证type0/1/3/5；
- 当前没有发现运行中的Unity Editor，因此脚本准备可继续，compile/Play需Editor重新打开；
- 发现production first-difference时登记新D-ID并停止，不在本认证包修复。
- `R8-OPLIFE-001`探针代码现已写入（Editor-only explicit menu），状态`CODE_WRITTEN`；普通dotnet旧
  csproj未收录新文件，fresh Unity compile/Play/self-check仍待，不得报告为已通过。

## R8-WP01C-01 evidence result

- fresh Unity compile 0 error，Play result 09:05:09 PASS；
- OID33/120/203/999四类birth type/CLR/frame/Prev2/handle正确；slot53 generation 1→3→5→7；
- high-slot same-pass与low-slot next-pass witness均通过；release/reuse拒绝旧handle；
- cleanup恢复object/claimed/render pool/logic pool基线；
- W05 focused 8/8 PASS，full self-check 09:06:51 PASS，Play后Console 0 error/warning；
- final validator/diff尚待。通过后01可关闭，下一包为02 pickup/held/throw/landing，但仍需用户批准。

Final governance已通过：59 records / 59 governed code files，scoped diff check PASS。01现为
`VERIFIED`（仅Unity S4范围）。下一可执行包是`R8-WP01C-02`；在用户明确批准前停止，不修改gameplay。

## R8-WP01C-02 execution start

- 用户已于2026-08-23明确批准“执行 R8-WP01C-02，恢复目标”；approval boundary已解除；
- C++ pickup、held/wpoint/throw、physics landing 与 Unity writer/resolver 的只读观察点已闭合；
- 既有自动夹具不构成 live `NTSD_Battle` 整链 S4，因此建立
  `R8-HOLDPLAY-001 / PLANNED`，只允许新增 Editor-only explicit Play probe；
- 用户同时报告部分技能图片错误。该症状不属于02逻辑裁决，已新增
  `D-RENDER-006 / R8-WP01D / REPRODUCTION_PENDING`；WP01D尚未开始，未复现前不修改表现代码。
- `R8-HOLDPLAY-001` probe及meta现已写入，状态`CODE_WRITTEN`；只新增Editor test脚本，未改production。
  下一步必须fresh Unity compile，再执行显式Play probe、held/landing focused tests、full self-check和final治理。

## R8-WP01C-02 final result

- `R8-HOLDPLAY-001 / VERIFIED`（仅Unity S4）；final Play 09:37:31 PASS；
- OID120/150/121/122四type的pickup link/frame、held wpoint/cover/FrameDelay、throw
  frame/velocity/spawner/picker/release和landing均通过；overlap target HP/frame未被landing直接修改；
- cleanup恢复object4、claimed2、render pool2、logic pool2；
- fresh compile、focused23/23、09:38:54 full self-check、Console clean、validator60/60与diff均PASS；
- 两次probe-only失败均已留痕，未修改production；persistent evidence见
  `R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md`；
- 下一包为03 grab/CPoint/link/held injury，仍需用户明确批准。D-RENDER-006继续属于尚未开始的WP01D。

## Persistent boundaries

- C++ authority只读，不运行、构建、修改、复制或写入；
- R1-WP02 full C++ trace保持BLOCKED；
- approved Unity render/capacity/30 Hz/FrameInputSet/SoA/pool/worker/0-GC边界保持不变；
- T8默认stage.dat和Android真机排除；
- R8-WP01D已取得部分GPU/Game/SceneView S4但仍有证据边界；E/F/G尚未开始；不得宣称R8或完整战斗对齐完成。

## 2026-08-23 WP01C-03 resume

- 用户已明确批准`R8-WP01C-03`并恢复目标；
- `R8-GRABPLAY-001 / PLANNED`已建立；
- 只读复核确认R5-CPT-001～005、R5-LINK-001～002均已有production修复及自动证据，03只补live
  production world joint S4；
- 唯一允许新增脚本为`BattleGrabCpointLinkPlayModeProbeEditor.cs`及meta；
- matrix覆盖valid grab/injury/stats、mismatch throw、escape dircontrol、positive/negative residue和四pass表；
- first-difference必须另拆repair，禁止在认证probe顺手修改gameplay。

## WP01C-03 final

- `R8-GRABPLAY-001 / VERIFIED`（仅Unity S4）；
- compile0、focused8/8+2/2；
- worker-active clean Play tick16→17 required matrix全部PASS；
- objects/claimed/pools/global stats/pause恢复，Console0 error，12:23:59 self-check PASS；
- production0改动；persistent evidence为
  `R8-WP01C-03-grab-cpoint-link-held-injury-runtime-evidence-20260823.md`；
- 下一包为04 collision/hit/damage/abort，当前`APPROVAL PENDING`，不得自动启动。

## 2026-08-23 WP01C-04 resume

- 用户已明确批准`R8-WP01C-04`并恢复目标，上方approval pending口径已解除；
- C++ Release只读preflight确认正式pass边界为snapshot/collect→type0 consume→random-weapon→type>0 consume；
- `R4-COL-001～003/005A`与`R4-HIT-001～004`已有source/compile/self-check，当前只缺live joint S4；
- 已建立`R8-HITPLAY-001 / IN_PROGRESS`；唯一允许新增脚本为Editor-only
  `BattleCollisionHitDamagePlayModeProbeEditor.cs`及meta；
- matrix覆盖character/weapon/special正向命中、candidate order/HitConfirm2整attacker abort、caught/hurtable
  current-candidate skip、effect21 state18/19 attacker abort，以及HP/HPBound/stats/durability/vrest与三pass表；
- production first-difference必须另拆repair，禁止在认证probe中顺手修改gameplay。

## WP01C-04 final

- `R8-HITPLAY-001 / VERIFIED`（仅Unity S4）；
- compile0、focused178/178+11/11+9/9；
- clean Play 10-candidate matrix通过：三类positive、HitConfirm2/caught/effect21、kind10 raw frame、
  character→random no-op→object pass boundary全部符合合同；
- objects/claimed/pools、RNG、global stats、pending sounds、baseline rests、mode/pause均恢复，Console0 error；
- 13:19:39 full self-check和69/68 validator PASS，production0改动；persistent evidence为
  `R8-WP01C-04-collision-hit-damage-runtime-evidence-20260823.md`；
- hit-plan mode=Disabled、worker inactive，故本包不提供ShadowCompare/worker-active/full C++ trace；
- 下一包为05 death/respawn/integer state/AI boundary，当前`APPROVAL PENDING`，不得自动启动。

## 2026-08-23 WP01C-05～07 continuous authorization

- 用户已明确要求“直接推进 WP01C 剩余的三项即可，不需要我批准”；
- 先前关于05、06、07逐包`APPROVAL PENDING`的记录保留为历史事实，但已由本条 supersede；
- 固定执行顺序仍为`05 → 06 → 07`，每包继续使用独立Task/Change Record和分层证据；
- 授权不包含production first-difference的顺手修复，不包含scene/资源/架构变更，也不改变C++ authority只读、
  R1-WP02 full trace BLOCKED、T8默认stage.dat与Android排除边界；
- 当前开始`R8-WP01C-05 — death / respawn / integer state / AI boundary`。

## R8-WP01C-05 execution start

- Task：`R8-WP01C-05-death-respawn-ai-integer-execution.md`；Change：`R8-DEATHPLAY-001 / PLANNED`；
- C++ source与Unity静态crosswalk已闭合，未发现需先改production的静态首差；
- 允许的唯一脚本是Editor-only `BattleDeathRespawnAiIntegerPlayModeProbeEditor.cs`及meta；
- matrix覆盖HP=0 AI input、state14 arm/decrement、no-count/stored-count/free、stale integer/RNG、
  relation/link/holder/target写入边界、OID998和cleanup；
- 发现production首差时不得在probe里顺手修复。
- probe及meta已写入，`R8-DEATHPLAY-001 / CODE_WRITTEN`；production0改动；下一步必须fresh compile，
  再执行focused、clean Play、full self-check与final治理。

## R8-WP01C-05 final

- `R8-DEATHPLAY-001 / VERIFIED`（仅Unity S4）；
- compile0、AI85/85、W05 exact1/1 + isolated8/8；
- clean Play逐tick death/AI/state14/no-count/stored/free/OID998矩阵全部PASS；
- objects/claimed/pools/RNG/sounds/pause恢复，Console0，13:52:04 self-check和validator PASS；
- production0改动；persistent evidence为
  `R8-WP01C-05-death-respawn-ai-integer-runtime-evidence-20260823.md`；
- worker inactive、C++ full trace BLOCKED；按连续授权直接进入06。

## R8-WP01C-06 execution start

- Task：`R8-WP01C-06-random-weapon-late-effect-execution.md`；Change：`R8-LATEPLAY-001 / PLANNED`；
- source preflight已闭合natural random、late chain、9996 five-child与exhaustion；
- R7旧state8000 HitStun140口径已按R8纠正为RenderPicOffset140；
- 唯一允许脚本为Editor-only `BattleRandomWeaponLateEffectPlayModeProbeEditor.cs`及meta；
- production首差不得在认证probe中顺手修复。
- probe/meta已写，`R8-LATEPLAY-001 / CODE_WRITTEN`；下一步fresh all-scope compile，再执行focused、
  clean Play、full self-check和治理。

## R8-WP01C-06 final

- `R8-LATEPLAY-001 / VERIFIED`（仅Unity S4）；
- compile0、focused14/14、worker-active clean Play四矩阵PASS；
- natural OID122/slot50/8 RNG；live 4×217+1×218/34 RNG；synthetic full chain；满槽exhaustion通过；
- objects/claimed/pools/RNG/sounds/pause恢复，Console0，14:08:15 self-check和validator PASS；
- production0改动；persistent evidence为
  `R8-WP01C-06-random-weapon-late-effect-runtime-evidence-20260823.md`；
- C++ full trace BLOCKED；按连续授权进入07 synthesis。

## R8-WP01C-07 final synthesis

- 01～06全部`PASS / VERIFIED（Unity S4限定范围）`；07文档汇总完成；
- synthesis：`TASKS/R8-WP01C-07-synthesis.md`；persistent evidence：
  `RESEARCH/R8-WP01C-07-production-combat-object-synthesis-20260823.md`；
- 六个认证probe均只新增Editor test脚本，production gameplay0改动；
- C++ full trace、未知DAT分支、WP01D视觉、WP01E性能、WP01F Player、WP01G总汇总仍未关闭；
- 因此WP01C完成不等于R8或完整战斗对齐完成。
- `B-R8-WP01C-06-TEARDOWN-01`保留：WP06 runtime cleanup通过，但probe退出Play后有两个AutoCreated
  manager scene-cleanup warning；无probe对照不复现。该项不否定战斗字段S4，也未被隐藏为Console clean。
