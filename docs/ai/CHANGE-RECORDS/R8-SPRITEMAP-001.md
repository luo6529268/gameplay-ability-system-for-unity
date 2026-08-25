# R8-SPRITEMAP-001 — C++ generic effective-pic field and raw-hidden contract

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:352-383; src/render/renderer.cpp:581-624; include/game_world.h:59,83; Makefile:15,34
evidence: state8000 C++ writes unk_318=140 while Unity writes HitStop=140; C++ raw pic999 gate precedes unk_318 addition
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：battle / render handoff / test

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`
- 所属 Work Package：`R8-WP01D-01`
- 本 Record 只覆盖所有实体共用的 state8000 render-pic offset writer、raw pic999 隐藏顺序与陈旧
  self-check oracle；
- 不属于本次范围：catalog/atlas生产代码、某个角色/技能/OID、DAT/BMP/scene/resource、排序/挂点、
  WP01C、WP01E～G。

## 2. Authority / 需求依据

- C++ `run_state_special_pre_collision` 在 current DAT 切换与 frame0 写入后设置 `e.unk_318=140`；
- C++ `Renderer::draw_entity` 先对 raw `fd->pic == 999` return，再执行 `fd->pic + e.unk_318`；
- `game_world.h` 明确区分 `hit_stop` 与 `unk_318`，二者不是别名；
- 用户明确要求从 C++ 通用流程检测，不接受具体角色/技能特判；
- Evidence：`VERIFIED` source contract；C++ runtime full trace仍`BLOCKED`。

## 3. Unity 原状与已确认差异

- `LF2Entity.ApplyStateDataTransform(targetObjectId, applyHitStop140)` 把 state8000 的140写入
  `HitStun -> Runtime.HitStop`，而 `Runtime.RenderPicOffset`保持0；
- `GetRenderPicIndex()` 对所有非负 raw pic先加offset，导致raw `999`在offset非零时不再保留隐藏哨兵；
- `BattleRuntimeSelfCheck.CreateTransformedLandingShell` 反向要求`HitStun==140`，因此旧PASS保护的是
  Unity错误行为；
- 目标是修正共享字段/顺序，不更改任何技能、OID、DAT或renderer后端。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` | `ApplyStateDataTransform` | state8000写HitStop 140 | 写`RenderPicOffset=140`，不触碰HitStop |
| 同上 | `GetRenderPicIndex` | raw pic999可被offset改变 | 先保留raw 999隐藏，再对普通pic加offset |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | transformed-state fixture | 期待错误HitStop写入 | 断言offset/hit-stop独立、普通pic+140与raw999隐藏 |

## 5. 不可回退边界

- 保留 CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× scale 与 fixed-world camera；
- 保留 Authority400/MobileExtended/DesktopExtended 容量合同；
- 保留 30 Hz、FrameInputSet、slot/generation、SoA/ECS、对象池、worker、0 GC；
- 不改 C++、DAT、scene、resource、T8、Android、服务器或已关闭 Change ID；
- 不允许按角色、技能、OID、frame ID 或资源路径特判。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` | `ApplyStateDataTransform` | boolean职责改名为render offset；state8000写`Runtime.RenderPicOffset=140`，不再写`HitStun` | 所有state8000 current-DAT转换统一选择`raw pic+140`；不额外冻结逻辑 |
| 同上 | `GetRenderPicIndex` | raw pic<0/999在offset前返回；普通pic才加offset | 隐藏帧在offset非零时仍隐藏 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | transformed landing/state fixture | 移除HitStop 140/139陈旧断言；新增offset/hit-stop独立、ordinary effective pic与raw999断言 | 旧错误oracle会被source合同替代 |

没有新增角色、技能、OID、frame或资源路径分支；没有修改DAT、资源、scene或中央渲染后端。

### 首次 full self-check 失败与同源 oracle 修正

- 2026-08-23 10:11:07 首次 fresh self-check 为 `FAIL`；production代码已编译，失败发生在既有
  `GT-10` 断言，仍要求state8000产生“authority HitStop=140”；
- 随后全文件搜索发现同一旧解释还存在于`GT-11` transform-chain与missing-target断言；
- 这三处都是本Record已声明的陈旧test oracle，不是新的production first-difference；已统一改为
  `HitStop=0 + RenderPicOffset=140`，保留attacking、frame0、wait和structural writer断言；
- 失败结果已保存在`Temp/NTSD_BattleRuntimeSelfCheck.result`；修正后必须重新fresh compile和重跑，
  不得复用首次失败前的任何PASS。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity force scripts compile | source 10:12:16 < `Assembly-CSharp.dll` 10:12:32；Console C# error=0 | `PASS` |
| focused / self-check | full `BattleRuntimeSelfCheck` | 首次10:11:07 stale GT-10 oracle FAIL；同源oracle修正后10:13:22 PASS | `PASS` |
| Play Mode / 集成 | generic all-DAT/CentralOnly子批 | 未运行 | `PENDING` |
| C++ authority 对照 | source field/order crosswalk | 已闭合 | `PASS` |
| optional full trace | R1-WP02 | 无安全观察通道 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 风险：旧 self-check 把错误字段当作authority；必须与代码原子修正；
- 未关闭：all-loaded-DAT catalog/slice/UV/command与实际GPU像素归后续WP01D子批；
- 回滚：只反向撤销本 Record 的精确行与断言，不得回退其他脏工作树；
- 若发现独立 catalog/atlas first-difference，创建新的 Change ID，不扩大本 Record。

## 9. Git / 交接

- 修改前工作树：大量用户/历史未提交改动；目标两文件本身已有历史改动，必须做最小补丁；
- 实际 diff 范围：上述两文件的精确方法/断言；两文件其他既有脏diff不属于本Record；
- 提交 hash：未提交；
- validator：脚本修改前与代码写入后均PASS；代码写入后61 Records / 60 governed code files；
- Console：self-check内两条既有负向registry fixture error读取后清空；最终error/warning=0；
- 交接优先阅读：本 Record、`TASKS/R8-WP01D-01-generic-effective-pic-contract.md`、
  `HANDOFFS/HANDOFF-R8-WP01D-generic-sprite-mapping.md`。
