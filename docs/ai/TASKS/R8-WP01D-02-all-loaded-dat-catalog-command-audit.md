# R8-WP01D-02 — all-loaded-DAT catalog / binding / live command audit

> 日期：2026-08-23  
> 状态：`IN_PROGRESS / DIAGNOSTIC-ONLY`  
> Change ID：`R8-SPRITEMAP-002`

## Goal

在真实 `NTSD_Battle` Play Mode 的production manager/world中，不指定角色、技能或OID，枚举所有已加载
DAT/frame与中央sprite catalog，按C++ Release通用range/source-rect合同检查sheet、rect、binding、slice/page、
UV元数据，并动态选择一个实际可达的state8000目标生成CentralOnly entity command witness。

## Scope

- 枚举`GameDataManager.GetAllObjects()`中所有已加载`LF2CharacterData`；
- 对每个DAT `SpriteFileInfo`按C++固定语义解释：`row=横向columns`、inclusive `[start,end]`、
  `localPic=effectivePic-start`、stride=`w+1/h+1`、top-left转Unity bottom-left；
- 对所有实际可见frame检查C++预期range与Unity catalog key/source path/pixel rect；
- 对所有catalog entry检查source texture、legacy sprite rect、central binding、atlas rect、normalized UV、
  同sheet mode/page/slice/placement translation一致；
- 动态扫描所有已加载DAT中的state8000 writer，选择第一个target frame0的`pic+140`确有catalog资源的
  候选；用通用probe shell运行正式`RunStateSpecialPreCollision`与`RenderDispatchAll`，检查snapshot、
  entity command和logical resource key；
- best-effort清理probe、恢复driver pause并重新发布无probe frame；输出结构化JSON。

## Authority / Evidence

- C++ `game_tick.cpp:352-383`、`renderer.cpp:581-624`、`loading.cpp:100-120`、
  `renderer.h:9-22`；对应translation units进入release Makefile；
- Unity被审计对象：`CharacterAnimtorManager`、`BattleSpriteCatalog`、`BattleSpriteCentralBinding`、
  `BattlePresentationCoordinator`与`SimulationWorld.RenderDispatchAll`；
- `R8-SPRITEMAP-001`已修正state8000字段/raw-hidden并取得fresh compile/self-check；
- 本包只提升Unity S4 diagnostic evidence，不替代C++ full trace/S5或人工像素确认。

## Files likely involved

- 新增`Assets/NTSD/Scripts/Test/Editor/BattleSpriteCatalogPlayModeProbeEditor.cs`及meta；
- 本Task、`R8-SPRITEMAP-002` Record、Ledger、STATE、D-RENDER-006、R8 matrix与handoff。

## Unknowns

- non-readable Texture2DArray/ordered page的实际GPU像素复制是否与source rect完全一致；
- Game/Scene最终视觉、pivot/挂点/透明遮挡；
- C++ executable full trace；
- 当前未加载、未来mod DAT的覆盖。

## Deliverables

1. 显式菜单触发的Editor-only Play探针；
2. JSON包含全部统计和最多64条first-difference：definition/frame/raw/effective pic、range、source、
   expected/actual rect、binding mode/slice/page/UV、reason；
3. 动态state8000 live command witness，不硬编码候选OID；
4. cleanup与driver/world/catalog基线恢复；
5. 文档状态分层更新。

## Verification

1. fresh Unity compile 0 error；
2. EditMode/isolated source-rect fixture或full self-check保持PASS；
3. live probe在tick>0、world/manager/catalog ready、worker idle下运行；
4. `auditedDefinitionCount>0`、`auditedFrameCount>0`、`auditedCatalogEntryCount==catalog.Count`；
5. source/range/rect/binding差异0；动态state8000 snapshot/command/effectivePic/logical key全部匹配；
6. cleanup后world object/claimed slot与pause状态恢复；
7. validator与scoped diff PASS。

## Stop conditions

- 找到catalog/range/rect/binding/live-command first-difference：输出D-ID证据并停止，不在本诊断包修生产；
- 只有硬编码某角色、技能、OID或资源文件名才能构造probe；
- 需要改DAT/BMP/scene/resource或中央渲染生产代码；
- 需要改变批准的Unity adapter；
- 需要运行、修改、构建或写入C++ authority；
- probe无法保证cleanup与driver恢复。

## Out of scope

- 修复本探针发现的第二处差异；
- GPU pixel baseline与人工技能视觉（后续WP01D-03）；
- input/gameplay/collision/held/opoint；
- T8、Android、1000 AI、Player、服务器与C++ full trace。

## First Play result

- 实际覆盖：100 loaded definitions、232 ranges、4373 catalog entries、6674 authored frames；
- `differenceCount=1301`；首批结构化差异全部为`CPP_SOURCE_DESCRIPTOR_MISMATCH`；
- C++ source复核后确认不是探针公式错误：DAT `row`在release中固定作为横向`cols`，Unity物理尺寸
  heuristic导致部分sheet保留`col`横向解释；production repair已拆为`R8-WP01D-03/R8-SPRITEMAP-003`；
- cleanup baseline在tick0采为0，审计后production world已自然初始化为4/2；该probe bug在本Record修正；
- state8000 no-candidate需要拆分计数；本Task继续`IN_PROGRESS`，不得把首次FAIL写成对齐完成。
