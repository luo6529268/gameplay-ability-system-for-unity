# R8-SPRITEMAP-005 — generic catalog GPU / binding pixel evidence

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-005
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSpriteCatalogGpuPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\render\renderer.cpp:581-624; include\renderer.h:9-24; Makefile
evidence: 5537-entry descriptor Play audit passed, while production GPU evidence is still missing under the approved logic-only CentralOnly architecture
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / editor / GPU render certification

## 1. 状态与范围

- 当前状态：`VERIFIED`（只裁决本Record的全catalog binding像素与dynamic Mesh GPU witness）
- Work Package：`R8-WP01D-05`
- 唯一允许脚本：新增`BattleSpriteCatalogGpuPlayModeProbeEditor.cs`及meta；
- 禁止production、角色/技能/OID/frame/file特判、DAT/BMP/scene、C++、Legacy owner回退。

## 2. 修改前证据

- all-DAT descriptor audit：5537 entries、0 differences、cleanup PASS；
- focused resolver/atlas/mesh job `608b9f8515a646fb97ecd2a5c36c4707`：29/29 PASS；
- P8-C report `Temp/P8-C-R8-WP01D/P8-C-report.json`：synthetic central/legacy、array UV、
  transparent ordering、chunk、missing-resource均PASS；
- production pool case FAIL：旧harness先耗尽GameObject池再要求opoint实体拥有`LF2ObjectRenderer`，
  但当前正式world使用logic-only materialization/central snapshot；因此没有取得production pixel sample。

## 3. 计划改动

- 对所有catalog entry比较source rect与central binding实际像素/透明度/hash；
- 对partial clipped与任意pivot做统一GPU command witness；
- 输出结构化first difference、PNG和cleanup；
- 不修改production行为。

## 3.1 实际改动

- 新增ASCII菜单Play probe及meta，等待battle-ready后暂停driver并等待worker idle；
- 按source texture分组，以`AsyncGPUReadback`读取232张production源纹理和30个中央array slice；
- 对5537 entries的source `PixelRect`与central `AtlasContentPixelRect`逐像素比较，覆盖
  SourceTexture2D与Texture2DArray；累计84,327,319像素；
- dynamic Mesh witness从全catalog动态选择“partial且实际含非透明像素”的entry，构造统一logical-key
  entity command，通过正式resolver/backend离屏绘制并与同descriptor Legacy参考比较；
- 首次witness选到全透明partial，legacy/central均0像素；第二次已选到可见partial但视口仍以逻辑锚点
  居中，负pivot把等价quad移出视口；最终改为按通用`position + (0.5-pivot)*size`视觉中心取景；
- 上述两次失败均为diagnostic选样/取景缺陷，source→binding全量像素在三次中始终0差异；
- 没有写入任何角色、技能、OID、frame、文件名或证据路径分支，production脚本0改动。

## 4. 验收与状态

| 层级 | 结果 | 状态 |
|---|---|---|
| Task/Record/Ledger/STATE | 本Record已建立 | `PASS` |
| compile | `Assembly-CSharp-Editor.dll` 11:34:48晚于final probe source；Console compiler error=0 | `PASS` |
| all-catalog pixel | 5537/5537 matched；84,327,319 pixels；source/central hash=`8ECA0CBA6D4724D1`，同域重复一致 | `PASS` |
| GPU witness | dynamic visible partial 450×5 / pivot(0.5,-28)；legacy/central 340/340；mean/max=0/0 | `PASS` |
| cleanup | object/claimed 4/2→4/2 | `PASS` |
| focused regression | job `ecaf8255752e4515bbcc76787c61aba3` 35/35 | `PASS` |
| full self-check | 2026-08-23 11:37:22 | `PASS` |
| Game/Scene人工 | 本Record不裁决 | `OUT OF SCOPE` |
| C++ full trace | R1-WP02 | `BLOCKED` |

## 5. 回滚与交接

- 回滚只删除新增Editor probe/meta并把本Record标记`ROLLED_BACK`；
- 不触碰脏工作树其他内容；未提交；
- 结构化证据：`Temp/NTSD_R8_WP01D_05_SpriteCatalogGpu.result.json`；
- PNG：`Temp/R8-WP01D-05-GPU/partial-legacy.png`、`partial-central.png`、`partial-diff.png`；
- D-RENDER-006仍不得整体标VERIFIED：真实Game/Scene最终表现、无authored state8000 witness与C++ full trace
  仍是独立边界；
- handoff：`HANDOFF-R8-WP01D-generic-sprite-mapping.md`。
