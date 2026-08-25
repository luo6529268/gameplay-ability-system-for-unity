# R8-SPRITEMAP-004 — declared-range / clipped-source sprite contract

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-004
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\render\renderer.cpp:581-624; include\renderer.h:9-24; src\core\loading.cpp:108-120; Makefile
evidence: fourth all-DAT Play leaves exactly two non-black visible missing entries after 60 outside and 167 colorkey-only references
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：production / sprite descriptor / generic Unity adapter

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`
- Work Package：`R8-WP01D-04`
- 允许脚本：manager rect/pivot构建、catalog builder pivot输入、同源self-check；
- 禁止：角色/技能/OID/frame/file特判，gameplay、DAT/BMP/scene、shader/Mesh/pass、C++ authority。

## 2. Authority / first difference

- C++ declared range决定localPic合法性；`SpriteSheet.cols`只来自DAT row；没有`row*col`帧上限；
- C++ source rect可超出surface，blit按source bounds裁剪；black colorkey不产生可见像素；
- Unity已修row-horizontal，但`BuildIndexedSpriteRects`仍分配`row*col`并要求完整rect在纹理内；
- 全量probe通用像素过滤后仅余两个非黑可见missing entry，证明需要range-length与partial clip；证据中的
  ID/frame/path仅用于复现，不允许进入production分支。

## 3. 计划改动

| 文件 | 符号 | 改前 | 改后 |
|---|---|---|---|
| `CharacterAnimtorManager.cs` | rect/prewarm/catalog | row*col长度；partial为hole；固定pivot | range长度；bounds intersection；通用clipped pivot |
| `BattleSpriteCatalog.cs` | builder Add | pivot固定0.5/0 | 保留旧overload并新增显式pivot overload |
| `BattleRuntimeSelfCheck.cs` | P2 fixtures | 只覆盖完整cell/hole | 增加纵向range与partial clipped rect/pivot合同 |

## 4. 不可回退边界

- 保留CentralOnly/Texture2DArray/dynamic Mesh/URP、共享Texture2D和atlas架构；
- 保留1.5×、fixed-world、容量、30Hz、ECS/pool/worker/0GC；
- 不修改逻辑位置/centerx/centery/Transform或碰撞；
- fully-outside仍为hole，黑色colorkey missing无需强制发布。

## 5. 实际改动

- `BuildIndexedSpriteRects`长度改为inclusive declared range；仍要求DAT row/col有效，但不再用乘积限帧；
- 每个requested C++ rect与source texture求intersection；无交集留hole，partial发布裁剪rect；
- 新增`ComputeIndexedSpritePivot`，以完整frame bottom-center与裁剪offset换算任意float pivot；
- prewarm `Sprite.Create`与catalog builder均使用同一pivot；catalog builder保留旧overload并新增显式pivot overload；
- self-check将partial `etc`从4个full cell扩展为8个intersecting cell，精确锁定localPic7 rect79×4、
  pivot(0.5,-18.75)及后续fully-outside hole；
- 首次full self-check在同一P2组合断言失败：weapon6/weapon3仍期待full-contained count40/7；
  C++ clipped合同下实际应含下一行2px与右侧42px交集，夹具已修为count50/8并锁定rect49/7；
- all-DAT probe按requested rect计算expected clipped rect/pivot，partial引用改为统计而非错误；
- 未增加角色、技能、OID、frame或文件名分支，未修改Mesh/Shader/gameplay。

## 6. 验收

| 层级 | 结果 | 状态 |
|---|---|---|
| C++ source | declared range + row cols + clipped blit链闭合 | `PASS` |
| Unity current Play | 修复前2个non-black visible missing，cleanup PASS | `FAIL / FIRST DIFFERENCE` |
| compile | Unity 2022.3.62f3 fresh scripts compile，Console C# error=0 | `PASS` |
| full self-check | 首次暴露stale partial oracle；同源修正后2026-08-23 10:59:06 | `PASS` |
| repaired all-DAT Play | 100 definitions、232 ranges、6674 frames、5537 catalog entries；23 clipped引用；source/path/rect/pivot/binding differences=0；cleanup 4/2→4/2 | `PASS` |
| GPU synthetic matrix | central/legacy pixels、array UV、透明顺序、4097 chunk均通过；production pool case被logic-only架构前置阻断 | `PASS / PRODUCTION WITNESS PENDING` |
| Game/Scene | 未取得最终人工可见证据 | `PENDING` |
| C++ full trace | R1-WP02 | `BLOCKED` |

## 7. 风险与回滚

- 风险：catalog条目数增加，atlas plan与resource cache容量会变化；只发生在loading/prewarm边界；
- 风险：partial pivot可能超出0～1；dynamic Mesh已按任意float pivot计算，必须用focused fixture验证；
- focused resolver/atlas/mesh job `608b9f8515a646fb97ecd2a5c36c4707` 为29/29 PASS；
- repaired all-DAT Play结构化结果：`Temp/NTSD_R8_WP01D_02_SpriteCatalog.result.json`，
  `PASS`、5537 entries、0 differences、23 clipped、60 fully-outside、state8000因loaded data无authored source
  明确`SKIPPED_NO_AUTHORED_SOURCE`；这不是GPU/Game/Scene或C++ full trace证据；
- 回滚：只反向恢复本Record三文件的本合同diff。

## 8. Git / handoff

- 工作树很脏；本Record只拥有上述三文件中declared-range/clipping相关差异；
- 未提交；handoff为`HANDOFF-R8-WP01D-generic-sprite-mapping.md`。
