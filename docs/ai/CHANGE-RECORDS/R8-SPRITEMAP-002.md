# R8-SPRITEMAP-002 — all-loaded-DAT sprite catalog Play diagnostic

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-002
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSpriteCatalogPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:352-383; src\render\renderer.cpp:581-624; src\core\loading.cpp:100-120; include\renderer.h:9-22; Makefile:12,34
evidence: user requires generic C++-derived verification without character/skill/OID special handling
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / render certification

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`
- 所属Work Package：`R8-WP01D-02`
- 唯一允许脚本路径：新增`BattleSpriteCatalogPlayModeProbeEditor.cs`；
- 不属于范围：任何production脚本、DAT/BMP/scene/resource、shader/Mesh/URP、角色/技能/OID专项处理。

## 2. Authority / 需求依据

- C++所有实体共享的DAT range/local pic/source rect、state8000 offset与render selection合同；
- 用户明确要求根据C++整体检测，避免提供具体角色/技能导致专项补丁；
- `R8-SPRITEMAP-001`已取得source/compile/self-check，但D-RENDER-006仍缺all-loaded-DAT与live command；
- Evidence目标：Unity S4 diagnostic；C++ full trace继续BLOCKED。

## 3. Unity 原状与证据缺口

- 既有P8-C production parity只选择第一个有效character与weapon sample，不能证明全部DAT/frame；
- 既有catalog/self-check使用synthetic rect，未输出每个production DAT的C++ expected descriptor；
- 现有diagnostic window可看单命令，但没有无特判的state8000动态候选和完整cleanup JSON；
- 因此新增一个显式、只读为主、Editor-only的live audit，不修改production行为。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/BattleSpriteCatalogPlayModeProbeEditor.cs` | explicit Play probe | 不存在 | 全DAT/frame catalog/binding矩阵、动态state8000 command witness、structured first difference与cleanup |

## 5. 不可回退边界

- 不修改CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×、fixed-world；
- 不修改容量、30 Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC；
- 不修改C++、production、DAT、scene、resource、T8、Android、Player、服务器；
- 不允许candidate选择硬编码角色/技能/OID。

## 6. 实际改动

新增显式Editor-only Play probe及meta：

- 等待tick>0、world/catalog/loaded DAT ready后暂停driver，并等待dedicated worker idle；
- 以loaded visualDataId/key排序枚举全部catalog entry和全部authored frame；
- C++ expected rect固定使用`row`为横向columns、inclusive range与`w+1/h+1`stride；
- 检查source path/pixel rect/legacy descriptor/central binding/normalized UV，以及同sheet的mode、
  central texture、slice/page与atlas translation稳定性；
- 动态扫描actual authored state8000 source，选择frame0 `pic+140`已发布的target；注册generic probe shell，
  调用正式state transform与`RenderDispatchAll`，检查snapshot/entity command/logical key；
- finally unregister probe、重新发布无probe frame、恢复pause并记录object/claimed cleanup；
- JSON最多保存64条first-difference，同时保留总difference count；没有任何固定角色、技能或OID候选。
- 中文菜单经MCP 9.6.9传输后乱码且`ExecuteMenuItem`返回not found；增加纯ASCII菜单别名，二者调用
  同一`RunFromMenu`，不复制或改变审计逻辑。
- 首次真正导入新脚本后编译发现`BattleSpriteEntry.MatchesCommand`不存在；探针已改用现有
  `BattleSpriteValueDescriptor`公开字段逐项核对logical key、sprite/texture/material ID、pixel rect与pivot。

## 7. 验收与证据

| 层级 | 实际结果 | 状态 |
|---|---|---|
| Unity compile | baseline/state8000分型修改后fresh compile，主Editor idle、Console compiler error=0 | `PASS` |
| focused/self-check | 004修复后full self-check 10:59:06 PASS；resolver/atlas/mesh 29/29 PASS | `PASS` |
| live Play audit | final 5537 catalog entries / 6674 frames / 0 differences | `PASS` |
| cleanup | object/claimed 4/2→4/2 | `PASS` |
| C++ source | generic descriptor/writer合同已闭合 | `PASS` |
| C++ full trace | R1-WP02 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 风险：全量遍历只在显式Editor probe运行，不能进入battle hot path；
- 风险：调用`RenderDispatchAll`会发布诊断frame，cleanup后必须重新发布无probe frame；
- 未关闭：GPU pixel、人工Game/Scene视觉与C++ full trace；
- 首次真实Play audit已执行：100 definitions、4373 catalog entries、6674 authored frames；
  `differenceCount=1301`，首批均为`CPP_SOURCE_DESCRIPTOR_MISMATCH`，已把production first difference
  转交`R8-SPRITEMAP-003`；
- 首次cleanup失败`objects 0->4 / claimed 0->2`是probe在tick0过早记录baseline，非production leak；
  本Record将把baseline延后到battle-ready/worker-idle边界并重跑；
- dynamic state8000未选到candidate，需在probe中区分“无authored source”和“有source但binding缺失”；
- baseline现改在battle-ready且worker-idle后采集；state8000增加authored/loaded target/visible frame0/
  effective catalog四级计数，无authored source时明确`SKIPPED_NO_AUTHORED_SOURCE`而不伪造映射差异；
- 003后的第二次Play：catalog 4933、原1301 source-descriptor mismatch清零、cleanup 4/2恢复；剩余229条
  均为declared range尾部的missing entry。C++仍计算这些rect但它们完全落在source texture外，Unity hole
  与C++无可见像素等价；probe现仅在C++ rect仍与source sheet相交时把missing entry判为差异，并单独
  统计`fullyOutsideSourceFrameCount`；待重编译/第三次Play确认。
- fully-outside口径首次编译发现line424/429局部变量与后续同scope变量重名（CS0136）；已仅重命名为
  `missingExpectedRect/missingLocalPic`，逻辑不变，首次失败保留；
- 第三次Play将229降为169并识别60个fully-outside；只读BMP像素确认其余相交区域在每个实际
  C++ rect内均为黑色colorkey（绿色只在不属于rect的separator列）。probe现以path缓存
  `BMPLoader.BmpData`并通用扫描clipped交集：全黑计`colorKeyOnlyMissingFrameCount`，存在非黑像素才报差异；
  cache在probe停止时清空，不进入production或battle hot path；
- 004 repaired final Play：100 definitions、232 ranges、6674 authored frames、5537 catalog entries；
  23个partial clipped引用已按C++裁剪合同发布，source/path/rect/pivot/central binding/slice/page/UV
  `differenceCount=0`，cleanup 4/2→4/2；loaded data的authored state8000 source count=0，因此动态命令
  witness为`SKIPPED_NO_AUTHORED_SOURCE`而不是PASS。Record升级`RUNTIME_PENDING`，仍缺GPU/Game/Scene
  和C++ full trace。
- 回滚：只删除新增probe/meta与本Record文档状态，不触碰其他脏工作树。

## 9. Git / 交接

- 修改前工作树：大量用户/历史未提交修改；本包仅新增文件；
- 实际diff：新增probe及meta；production文件0；
- 提交：未提交；
- validator：代码写入后PASS，62 Records / 61 governed code files；
- fresh compile：Unity 2022.3.62f3，force scripts reload后Console error=0；
- 首次Play触发：中文菜单路径被MCP编码为乱码，菜单未执行；不是探针或production runtime失败；
- 首次真实编译失败：`BattleSpriteCatalogPlayModeProbeEditor.cs:558` CS1061；只修探针API假设，
  production未改，失败证据保留；
- handoff：`HANDOFF-R8-WP01D-generic-sprite-mapping.md`。
