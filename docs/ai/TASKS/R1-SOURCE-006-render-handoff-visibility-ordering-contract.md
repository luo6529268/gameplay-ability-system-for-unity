# R1-SOURCE-006 — Render Handoff、可见性、层级与阴影同步源码合同

> 建立日期：2026-08-21  
> 状态：COMPLETED（静态 source inventory；runtime / visual fixture 待后续阶段）  
> 类型：只读 C++ / Unity source 审计；不修改任何 gameplay 或 production renderer。

## Goal

从 C++ release 的 game_tick render callsite 和 renderer live path 建立“逻辑结果如何交给表现层”
的合同，审计实体 frame、位置、方向、z、shadow、visibility、排序、camera boundary 与 render
snapshot 的读写时点；再将其映射到 Unity BattlePresentation、BattleCentralRenderSystem 和
CentralOnly 路径，区分必要的 Unity rendering adaptation 与真正会改变战斗可观察表现的差异。

## Authority / Evidence

- 唯一行为 authority：J:\QQFile\NTSD2.4\ntsd_release 中参与 release 的 live source；
- 权威入口：src/entity/game_tick.cpp 的 preframe / stage / render / postprocess 调用链，以及
  src/render/renderer.cpp 和实际调用的 entity render state、visibility、shadow、camera helper；
- 对 C++ render path 的研究仅决定战斗相关可观察表现的 handoff，不把 C++ 的渲染 API、
  camera implementation 或 binary asset pipeline 强行移植到 Unity；
- Unity CentralOnly、Texture2DArray、atlas、dynamic Mesh/quad、URP 是已批准实现边界。

## Scope

### C++ release source

- render 前后的 tick phase、哪些逻辑字段被 snapshot，是否有 render-side logic writeback；
- entity body、weapon、other object、special object、shadow、spark/smoke 的 visibility、
  frame source-rect、position/facing、z/sort、anchor/hotspot 与 layer relationship；
- stage / camera 在表现层的影响边界：确认其是否反写 battle simulation；
- entity birth/death/pool/reuse 与 render display 之间的 earliest / last visible tick；
- cpoint / held / weapon / opoint 相关对象在 render handoff 中的 parent, offset, sort,
  shadow contract；
- C++ source 能静态证实的 observable ordering；像素级实现差异或无法从 source 确认的结果
  必须标为 UNKNOWN。

### Unity source crosswalk

- BattlePresentation、BattlePresentationShadowBuild、BattleCentralRenderSystem、
  central command/descriptor、material/page/array、mesh segment 与 CentralOnly gate；
- LF2Entity / child presentation component、world-to-render coordinate、anchor/pivot、
  sorting formula、shadow command、camera / stage bounds；
- render command build/capture、late snapshot、worker/front-back buffer 的 read-only boundary；
- editor-only Legacy 对照仅作为诊断，不恢复 production SpriteRenderer。

## Required Deliverables

1. docs/ai/RESEARCH/R1-SOURCE-006-cpp-render-handoff-contract.md；
2. docs/ai/RESEARCH/R1-SOURCE-006-unity-central-presentation-crosswalk-and-diff.md；
3. 更新全量差异登记册、STATE，必要时更新重新对齐总计划；
4. docs/ai/HANDOFFS/HANDOFF-R1-SOURCE-006-render-handoff-visibility-ordering.md；
5. 不创建 Change ID，因为本 Work Package 不改脚本。

## Completion Record

- 2026-08-21：已完成 C++ `game_tick` render callback、`renderer.cpp` active/Z/painter/
  shadow/body/spark contract，与 Unity `RenderDispatch`、`BattlePresentation`、CentralOnly、
  dynamic Mesh、URP command submission 的静态 crosswalk。
- 已登记 D-RENDER-001～005 及 A-RENDER-001～004；它们分别表示待处理/待测试的 source
  差异与用户已批准的不可回退 Unity adapter。
- 未运行 C++ executable、Unity compile、self-check、Play Mode、GPU/CPU profiling 或 trace；
  本 Work Package 不构成任何视觉或运行时对齐验收。

## Static Acceptance Contract

完成前必须能清楚区分并记录：

1. “C++ battle state”与“C++ render-only derived state”的字段边界；
2. Unity central command 的每个战斗相关字段来自哪个逻辑 snapshot，以及它在何时被冻结；
3. Unity render layer 是否存在向 gameplay runtime 写回 position/frame/team/link 等逻辑字段；
4. character / weapon / special / other / shadow 的可见、挂点、offset 和 sort 由 C++ 哪个
   logical field 决定，Unity 是否读取同一逻辑真相；
5. CentralOnly / central Mesh 对每个 render command 的 skip/fallback 条件是否会造成一帧
   不显示、错误层级或错误 shadow；若只是在资源未就绪，必须明确资源前置条件；
6. Unity camera 不得成为实体移动或阴影移动的逻辑真相，任何证据相反的项必须登记；
7. 每一条静态差异都有一个后续最小 display fixture，且不要求像素级相同。

## Known Dependencies / Unknowns

- R1-SOURCE-005 必须先提供 object birth / hold / opoint / relation 的逻辑 handoff 边界；
- 中央渲染资源 page / Texture2DArray 的 batch 成本属于性能资料，不自动等同于 C++ battle
  behavior difference；
- 具体 asset pivot/source rect、DAT-to-sheet mapping、shader behavior 和最终屏幕像素可在
  source 中无法闭合，必须标 UNKNOWN 或由用户后续 Play Mode 验收。

## Stop Conditions

- 要获取结论必须运行、修改、注入、hook、copy 或重建 C++ release runtime；
- 要继续必须改 Unity gameplay、CentralOnly、production renderer 或 pass order；
- C++ source 不足以判定像素/asset runtime 结果：记录 UNKNOWN，不扩大为渲染重构；
- 用户提出新的 Change Request。

## Out of Scope

- 不改 C++ / Unity gameplay、renderer、shader、camera、mesh、texture、sort 或 prefab；
- 不回退 Unity 中央表现到逐实体 production SpriteRenderer；
- 不进行 Unity Play Mode、GPU/CPU profiling、Android 真机或长性能测试；
- 不处理 T8 默认 stage.dat 部署；
- 不实现 render trace、comparator 或 R2 gameplay。
