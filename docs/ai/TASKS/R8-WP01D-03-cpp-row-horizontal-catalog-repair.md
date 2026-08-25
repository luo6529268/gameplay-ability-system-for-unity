# R8-WP01D-03 — C++ DAT row-horizontal sprite catalog repair

> 日期：2026-08-23  
> 状态：`PLANNED / PRODUCTION REPAIR`  
> Change ID：`R8-SPRITEMAP-003`

## Goal

删除 Unity sprite prewarm/catalog 对 DAT `row/col` 的物理尺寸猜测，恢复 C++ Release 的通用合同：
DAT `row` 永远是横向列数，`col` 只决定纵向格数；`localPic` 始终按
`x=(localPic % row)*(w+1)`、`topY=(localPic / row)*(h+1)`选取源图。该修复适用于所有加载定义，
不得出现角色、技能、OID、frame 或 BMP 文件名特判。

## Scope

- `CharacterAnimtorManager.ResolveEffectiveGrid`只保留 C++ 固定语义，不再根据纹理宽高交换解释；
- 保持`BuildIndexedSpriteRects`的Unity bottom-left坐标适配和out-of-bounds hole行为；
- 修正`BattleRuntimeSelfCheck`中把`col`当横向列、把物理尺寸猜测当正向合同的陈旧夹具；
- 由`R8-SPRITEMAP-002`全loaded-DAT Play probe重跑catalog/rect/binding/command矩阵。

## Authority / Evidence

- `J:\QQFile\NTSD2.4\ntsd_release\src\data\dat_parser.cpp:335-368`：`row/col`原值写入`SpriteRange`；
- `...\src\core\loading.cpp:108-120`：`sr.row`原样传给`load_sprite(..., cols)`；
- `...\include\renderer.h:9-24`：`src_rect`使用`pic % cols`与`pic / cols`；
- `...\src\render\renderer.cpp:581-624`：declared range后计算`local_pic`并调用`src_rect`；
- 上述源文件均参与 release Makefile；C++ authority保持只读；
- Unity first Play audit：4373 catalog entries、100 loaded definitions、6674 frames；累计1301个generic
  difference，其中记录样本呈横向换行点后持续错一格；`visualDataId 200/201`只是结构化首差样本，
  不是修复条件或特殊分支。

## Files likely involved

- `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `docs/ai/CHANGE-RECORDS/R8-SPRITEMAP-003.md`
- Ledger、STATE、D-RENDER-006、R8 matrix与handoff

## Unknowns

- 修复后是否仍有非row/col来源的catalog/path/rect/binding差异；
- 真实加载数据中是否存在可执行的authored state8000 live witness；
- Texture2DArray最终GPU像素、Game/Scene可见性、pivot/挂点/透明排序；
- C++ full runtime trace仍由R1-WP02 blocker约束。

## Deliverables

1. 通用row-horizontal production writer修复；
2. C++ source-derived self-check；
3. fresh compile、full self-check与全loaded-DAT Play audit结果；
4. 更新D-RENDER-006、R8 matrix、STATE、Ledger和handoff。

## Verification

1. scoped diff只含本合同列明的两个脚本及治理文档；
2. Unity fresh compile 0 error；
3. `BattleRuntimeSelfCheck`通过新的row-horizontal夹具及全套检查；
4. `R8-SPRITEMAP-002`重跑后`CPP_SOURCE_DESCRIPTOR_MISMATCH=0`，cleanup恢复；
5. 若仍有差异，输出新的首个generic reason，不在本包顺手修复；
6. `Tools/Validate-ChangeLedger.ps1`与`git diff --check`通过。

## Stop conditions

- 需要按角色、技能、OID、frame或文件名特判；
- 需要改变CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5×、fixed-world或容量/30Hz/ECS边界；
- first difference转移到atlas upload、shader/GPU pixel、render ordering或gameplay；
- 需要运行、构建、修改或写入C++ authority；
- 需要扩大为R8-WP01E/F/G、T8或Android。

## Out of scope

- 任何角色/技能专项修图；
- DAT/BMP/scene/resource修改；
- shader、Mesh、URP、透明排序、挂点、位置或逻辑tick修改；
- C++ executable、trace instrumentation、hook、patch或authority写入。
