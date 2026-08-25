# R8-WP01D-04 — declared-range and clipped-source sprite contract

> 日期：2026-08-23  
> 状态：`RUNTIME_PENDING / PRODUCTION REPAIR COMPLETE`  
> Change ID：`R8-SPRITEMAP-004`

## Goal

恢复 C++ Release 对 sprite localPic 的第二层通用合同：DAT declared range决定可请求的localPic数量，
`row`只决定横向列数，`col`不限制可访问帧；source rect超出BMP时按source bounds裁剪。Unity通过裁剪后的
PixelRect/UV/quad size与换算pivot保持C++目的位置，不按角色、技能、OID、frame或文件名处理。

## Scope

- `BuildIndexedSpriteRects`按inclusive declared range分配，而非`row*col`；
- source rect与纹理无交集时保留hole；部分相交时发布裁剪rect；
- 新增通用clipped pivot换算，使裁剪quad仍位于完整逻辑frame中的C++位置；
- sprite prewarm和immutable catalog发布同一rect/pivot；
- self-check覆盖range>row*col、fully inside、fully outside与partial clipped；
- `R8-SPRITEMAP-002`复跑全部loaded DAT/catalog/binding。

## Authority / Evidence

- C++ `renderer.cpp:594-624`只检查declared range并计算`local_pic=render_pic-frame_lo`；
- `renderer.h:16-24`只使用`cols=sr.row`计算source rect，不读取`col`或`row*col`上限；
- SDL blit在source bounds上裁剪；C++将黑色设为colorkey；
- 第四次Unity Play audit：4933 catalog entries中source descriptor mismatch为0、cleanup PASS；
  60 fully-outside与167 clipped-black/colorkey等价不可见，只剩2个非黑可见missing entry：一个完整
  1×1 localPic位于实际纵向sheet内，一个79×4部分裁剪区域。它们只是首差证据，不进入实现条件。

## Files likely involved

- `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`
- `Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `R8-SPRITEMAP-002` Editor probe及治理文档

## Unknowns

- Unity `Sprite.Create`是否保留超出0～1的pivot；若不保留，CentralOnly仍可用catalog pivot，但Legacy
  descriptor需明确诊断边界；
- 修复后是否出现新的atlas binding/UV/command first difference；
- GPU pixel与Game/Scene最终可见结果；
- C++ full trace仍BLOCKED。

## Deliverables

1. declared-range rect构建与partial clipping；
2. 通用clipped pivot与catalog builder overload；
3. source-derived self-check；
4. fresh compile/full self-check/all-DAT Play复跑；
5. D-RENDER-006/STATE/Ledger/matrix/handoff更新。

## Verification

1. fresh Unity compile 0 error；
2. full self-check PASS，synthetic非对称/partial fixture精确验证rect和pivot；
3. all-DAT Play：source/path/rect/pivot/binding差异0，两个非黑missing key被发布；
4. cleanup恢复、CentralOnly保持、state8000 no-source如实SKIPPED；
5. validator与scoped diff PASS；
6. 真实GPU/Game/Scene证据仍独立，不以catalog PASS冒充最终视觉。

## Stop conditions

- 需要角色/技能/OID/frame/file特判；
- 需要改变shader、Mesh顶点格式、pass order、gameplay或已批准adapter；
- clipped pivot不能由现有resource size/pivot合同表达；
- first difference转移到atlas upload/GPU/排序/挂点；
- 需要写入或运行C++ authority。

## Out of scope

- DAT/BMP/scene/resource内容修改；
- 技能、输入、碰撞、opoint、位置或逻辑帧修改；
- T8、Android、1000 AI、Player、服务器；
- C++ instrumentation/executable/hook/patch。

## Actual result

- fresh compile 0 C# error；首次full self-check暴露同源旧oracle后已修正，10:59:06 PASS；
- repaired all-DAT Play：100 definitions、232 ranges、6674 authored frames、5537 catalog entries、
  23 clipped references、0 source/path/rect/pivot/binding differences，cleanup 4/2→4/2；
- loaded data没有authored state8000 source，live witness如实为`SKIPPED_NO_AUTHORED_SOURCE`；
- focused resolver/atlas/mesh regression job `608b9f8515a646fb97ecd2a5c36c4707` 29/29 PASS；
- production未加入角色、技能、OID、frame或文件名分支；GPU/Game/Scene与C++ full trace仍独立待证。
