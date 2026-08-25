# R8-WP01D-05 — generic catalog GPU / binding pixel evidence

> 日期：2026-08-23  
> 状态：`VERIFIED / EDITOR GPU S4`  
> Change ID：`R8-SPRITEMAP-005`

## Goal

在不恢复逐实体`SpriteRenderer`/GameObject表现所有权的前提下，为当前logic-only + CentralOnly架构建立
无角色、技能、OID、frame或文件名特判的production catalog像素证据：源BMP裁剪内容、中央绑定的
Texture2DArray slice/UV内容与dynamic Mesh输出必须保持一致。

## Scope

- 新增显式Editor-only Play probe；
- 枚举全部loaded `BattleSpriteCatalog` entry；
- 对每个可见entry比较source pixel与central binding实际pixel内容/透明度/hash；
- 覆盖SourceTexture2D与Texture2DArray，partial clip和任意float pivot必须进入统计；
- 使用统一central command做离屏GPU边界/partial witness并输出PNG/JSON；
- finally恢复pause/世界表现状态，不修改production runtime。

## Authority / Evidence

- C++ `renderer.cpp:581-624`、`renderer.h:9-24`定义source rect与colorkey可见内容；
- Unity final all-DAT descriptor audit已为5537 entries / 0 differences，但还没有验证array slice内的实际像素；
- 2026-08-23 P8-C synthetic GPU matrix的legacy/central、array UV、transparent order、4097 chunk均PASS；
- 同一P8-C production case失败于旧工具强制从对象池取得逐实体renderer/runtime handle；当前正式架构允许
  logic-only实体由central snapshot绘制，所以该失败不能裁决production像素。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleSpriteCatalogGpuPlayModeProbeEditor.cs`（新增）
- 对应`.meta`
- 本Task、Change Record、Ledger、STATE、register、matrix与handoff

## Unknowns

- runtime atlas texture在当前graphics device上的readback能力；
- 5537 entries逐像素全量比较的Editor耗时；
- Game/Scene相机内最终可见性、挂点与透明排序是否仍有独立first difference；
- C++ full trace继续BLOCKED。

## Deliverables

1. 全catalog source→binding像素比较与reason code；
2. SourceTexture2D/Texture2DArray、slice/UV、partial/pivot统计；
3. 统一central GPU witness PNG与结构化JSON；
4. cleanup与fresh compile/focused/self-check证据；
5. 若出现差异，只登记通用first difference，不在本Task修改production。

## Verification

1. Unity fresh compile 0 error；
2. probe在loaded battle Play Mode完成，所有可读visible entries pixel difference=0；
3. 至少一个partial clipped entry进入GPU witness；若当前数据没有则如实SKIPPED；
4. central GPU输出非透明，reference-vs-central像素差在既有容差内；
5. cleanup恢复，Console无新增异常；
6. focused resolver/atlas/mesh、full self-check与Change Ledger validator通过。

## Stop conditions

- 首差指向production atlas upload/resource resolver/dynamic Mesh/shader；
- 需要修改production脚本、DAT/BMP/scene或恢复Legacy owner；
- 需要角色/技能/OID/frame/file特判；
- 需要运行、构建或写入C++ authority。

## Out of scope

- gameplay/input/opoint/collision修改；
- 逐角色或逐技能修补；
- 人工Game/Scene最终验收、C++ full trace、T8、Android、1000 AI、Player、服务器。

## Actual result

- 5537/5537 catalog entries、84,327,319 source→central pixels matched；
- 232 source texture readbacks、30 Texture2DArray slice readbacks；4 SourceTexture2D、5533 array bindings；
- source/central aggregate hash均为`8ECA0CBA6D4724D1`，pixel differences=0，同一Play域重复一致；
- 动态可见partial witness：450×5、pivot(0.5,-28)、array slice3；dynamic Mesh与Legacy均340
  non-transparent pixels，mean/max difference=0/0；
- cleanup 4/2→4/2，focused job `ecaf8255752e4515bbcc76787c61aba3` 35/35，11:37:22
  full self-check PASS；
- 两次前置FAIL分别为全透明partial选样与负pivot视口中心错误，均只修Editor probe；production代码0改动。
