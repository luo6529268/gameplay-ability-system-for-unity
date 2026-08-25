# R8-HOLDPLAY-001 — production pickup / held / throw / landing Play probe

<!-- CHANGE-RECORD
id: R8-HOLDPLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHeldWeaponLifecyclePlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:996-1081; src/entity/game_tick.cpp:1527-1640,1924-2006; src/entity/physics.cpp:228-320; user approval 2026-08-23
evidence: Existing EditMode/self-check coverage cannot provide R8-WP01C-02 live NTSD_Battle pickup-to-landing S4 evidence
-->

> 创建日期：2026-08-23
> 最后更新：2026-08-23
> 类型：test / editor / battle certification

## 1. 状态与范围

- 当前状态：`VERIFIED`
- 所属 Work Package：`R8-WP01C-02`
- 唯一允许脚本路径：新增 `BattleHeldWeaponLifecyclePlayModeProbeEditor.cs`
- 不属于范围：production gameplay、scene、DAT、resource、render、WP01C-03～07、WP01D

## 2. Authority / 需求依据

- C++ release pickup、held/wpoint/throw、physics landing source contract；只读，不运行、构建或写入；
- 用户明确批准：2026-08-23“批准执行 R8-WP01C-02，恢复目标”；
- Evidence 目标：只提升 Unity S4；full C++ trace 保持 `BLOCKED`。

## 3. Unity 原状与证据缺口

- production 已有 `BattleInteractionWriter`、held/release resolver 与 shared non-character landing writer；
- self-check/EditMode 已覆盖多个孤立字段和 0 B 回归，但没有 live `NTSD_Battle` 的 pickup→held→throw→landing
  整链、真实 slot/registry/driver cleanup 证据；
- 当前不是已确认 gameplay 差异，而是 R8 S4 证据缺口；若探针发现 first-difference，本 Record 立即停止。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleHeldWeaponLifecyclePlayModeProbeEditor.cs` | Editor-only explicit probe | 不存在 | 在 live production world 记录 pickup、held wpoint、四类 throw/landing、no-immediate-hit 与 cleanup JSON |

## 5. 不可回退边界

- 不修改 CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5 scale、fixed-world camera；
- 不修改扩展容量、30 Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC；
- 不修改 C++、DAT、scene、resource、T8、Android、服务器或已关闭 Change ID；
- 用户报告的技能图片异常归 `D-RENDER-006 / R8-WP01D`，不在本 Record 处理。

## 6. 验收与回滚

- 编译：fresh Unity compile 0 error；
- 自动：相关 held/landing focused tests 与 full `BattleRuntimeSelfCheck` PASS；
- 运行时：显式 Play probe result PASS，记录逐项字段与 cleanup；
- 治理：`Tools/Validate-ChangeLedger.ps1` 与 scoped diff check PASS；
- 回滚：只删除本 Record 新增 probe 和 meta，保留结果/失败证据；不得回退其他脏工作树内容。

## 7. 代码写入时的未验证项（历史，已由第9节覆盖）

- 已新增显式菜单触发的 Editor-only probe及meta：等待worker idle后，在暂停的live world逐类注册
  probe-owned holder/weapon，调用正式Character-DAT pickup writer、held/throw consumer和shared landing writer；
- 使用data.txt中的OID120/150/121/122确认type1/2/4/6 catalog身份，以确定性frame/wpoint夹具记录
  link/slot、FrameDelay、wpoint integer position、spawner/picker、velocity、release与四类landing；
- 另以live registered overlap pair调用正式`LF2Weapon.OnLanded`，断言不发生Unity-only immediate target hit；
- finally/best-effort unregister所有probe实体，记录world/slot/render-pool/logic-pool基线；
- 当时compile、focused test、self-check、Play和cleanup尚未执行；最终结果见第9节；
- C++ full trace、像素挂点/图片/排序、WP01C-03～07 均未关闭。

## 8. 首次 Play 失败与修正

- 首次结果在 tick623 为 `FAIL`：type1 kind2 pickup 被拒绝；cleanup完整恢复
  object 9→9、claimed 7→7、render pool 2→2、logic pool 7→7；
- 原因是探针误调用只允许“非`LF2Character`但current DAT为character”的
  `LF2CharacterDatInteractionResolver`，真实`LF2Character`按设计在该入口被拒绝；这不是production first-difference；
- 探针已改用真实角色的`LF2CharacterInteractionResolver.TryApplyPreInteraction`，并在pickup前把武器置于
  type1/4/6 frame60地面态、type2 frame20重武器地面态，从而经过production ground gate和reference writer；
- 修正仍只在Editor probe内，production gameplay未改。需要重新fresh compile与Play。

第二次运行通过type1/type2全部链路，并推进到type4高速反弹；位置/frame/速度/耐久正确，但探针期望
attacking保持1时实际为0。只读定位确认是探针在写attacking=1之后又调用`ImmediateFrame(40)`，该初始化
API先行清零；landing使用的`SetFrameTickRawDirect(0)`本身不清attacking，因此不是production差异。
探针已把attacking sentinel移到frame初始化之后。

第二次结果同时显示tick0、object/claimed/pool全0、worker inactive，说明菜单在战斗bootstrap前触发。
探针现改为先等待`CurrentTickIndex>0`、world object/claimed均非0；满足后才暂停driver、捕获基线并执行，
超时前不污染world。需要再次fresh compile与Play。

## 9. Final verification

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | source 09:36:23 < Editor DLL 09:36:40；Console compile error=0 | PASS |
| Play S4 | 09:37:31 result PASS；tick1、worker active、四type整链与no-immediate-hit通过 | PASS |
| cleanup | object4/claimed2/render2/logic2全部恢复，无异常 | PASS |
| focused | job `36440d545fe64659ae3c73ff1febf03c`，23/23 | PASS |
| full self-check | 09:38:54 `PASS`；清空既有负向夹具预期日志后Console 0 error/warning | PASS |
| governance | 60 Records / 60 governed code files；scoped/no-index diff check | PASS |
| C++ full trace | R1-WP02观察通道未闭合 | BLOCKED |

实际新增文件只有本Editor probe及meta；production gameplay、scene、DAT和render均未修改。
持久证据见`RESEARCH/R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md`。
