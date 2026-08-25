# HANDOFF — R1-SOURCE-006 Render handoff / 可见性 / 层级 / 阴影

> 交接日期：2026-08-21  
> 状态：COMPLETED（静态 source inventory）  
> 不代表：C++ executable trace、Unity 编译、BattleRuntimeSelfCheck、Play Mode、GPU/CPU
> profiling、像素级比对、性能验收或 gameplay 已对齐。

## 1. 本包完成范围

已只读检查 C++ Release live source：

- `Makefile` 的 `ntsd_new.exe` release source list；
- `src/core/main.cpp` 的 battle tick render callback；
- `src/entity/game_tick.cpp` 的 PreFrame / stage / render / postprocess / late 顺序；
- `src/render/renderer.cpp` 的 active list、Z sort、shadow、body、spark、camera/perspective。

已只读映射 Unity：

- `NTSDBattleTickSystem`、`SimulationWorld.StageRender.partial`；
- `BattlePresentationShadowBuild`；
- `BattleCentralRenderSystem`、`BattleDynamicMeshBackend`、`BattleRenderFeature`；
- `LF2ObjectRenderer`、`LF2Entity`、`LF2Sprite`、`NTSDRenderSpace`；
- `BattleCentralTransparent*.shader` 的 URP transparent source contract。

未修改任何 C++、Unity gameplay、renderer、shader、scene、prefab、resource 或测试；未启动
任何 C++ executable，也未运行 Unity compile、self-check、Play Mode、trace 或性能测试。

## 2. 已完成的 C++ render source 合同

1. C++ release 将 `renderer.cpp` 同 `game_tick.cpp`、`main.cpp` 一起编入
   `ntsd_new.exe`；它不是 legacy/C# 或 diagnostic authority。
2. render callback 位于 PreFrame、wave phase / immediate stage spawn 之后，FramePostProcess /
   late entity update 之前。标准 late opoint child 因此不会在生成同 tick 进入 C++ render input。
3. renderer 从 active slot 升序收集 entity，以 stable Z asc 排序；同 Z 保留 slot order；
   每 entity painter order 是 perspective offset → shadow → body → labels → hit records。
4. shadow 和 body 的 hit-stop blink / threshold 不同；type-3 body 使用 display Z，而 shadow /
   order 使用 ZInt；frame-delay 的 X jitter 为 `6*(tick&1)-3`。
5. C++ render 会写 `render_offset_x`，并会在 spark blit 后推进/回收 hit record。它不是
   严格“render-only 无状态”实现。

详细 source contract：

- `docs/ai/RESEARCH/R1-SOURCE-006-cpp-render-handoff-contract.md`
- `docs/ai/RESEARCH/R1-SOURCE-006-unity-central-presentation-crosswalk-and-diff.md`

## 3. Unity crosswalk 已确认的静态 mapping

- Unity `RenderDispatch` 同样位于 `CurrentWaveStage` 后、`FramePostProcess` 前；
- CentralOnly capture 保留 runtime slot 初始顺序，并以 stable radix Z 排序；
- command order 为 shadow → entity → overlay → hit record；
- dynamic Mesh 只合并相邻且 resource-compatible 的 command，unresolved command 会截断 segment，
  `BattleRenderFeature` 按 segment stream 依次提交；
- central shader 使用 premultiplied transparent、`ZWrite Off`、`Cull Off`；
- Unity body/shadow 的 hit-stop gate、type-3 display Z、pic offset、facing pivot、frame-delay
  shake 已有同角色 source reader。

这些是静态 source mapping，不是运行时或像素一致证明。

## 4. 新登记的差异与保护边界

| ID | 结论 | 后续最小验收 |
|---|---|---|
| D-RENDER-001 | CentralOnly 有 feature/material/camera/catalog/backend fail-closed ownership gate；C++ active entity 直接逐个 blit。 | feature/resource/route 四态 display fixture，并读取 central diagnostic reason。 |
| D-RENDER-002 | C++ 在 tick 内 render callback 推进 hit spark；Unity 在 LateUpdate / worker acknowledgement 后 finalize frozen cycle。 | spark age / expiry next-tick fixture。 |
| D-RENDER-003 | Unity capture 有 OidMergeDormant、PendingFlushDestroy、FirstPresentationTick、runtime handle 额外 gate。 | pending destroy / slot reuse / last-visible fixture。 |
| D-RENDER-004 | C++ shadow 223/224 gate 用 char_data->oid；Unity 用 ObjectId，而 body resource 用 CurrentDatObjectId。 | data identity switch / 223-224 shadow fixture。 |
| D-RENDER-005 | Unity EntityVisible/ShadowVisible 是 C++ source 未直接出现的额外 gate。 | Hide、death blink、pool reuse、hit-stop fixture。 |
| A-RENDER-001～004 | CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× scale、fixed-world logic camera、扩展容量均为用户已批准的保护边界。 | 只能验证逻辑 snapshot 与可观察显示，不能回退 Legacy 或恢复 C++ camera_x 为 Unity logic truth。 |

## 5. 显式未闭合项

- C++ renderer mutating hit records 是否影响同 tick 后续 gameplay；
- CentralOnly route failure 在真实 URP world camera / asset loading 时的 exact display outcome；
- actual GPU transparent ordering、Texture2DArray slice/UV、asset pivot；
- `LF2Sprite.Hide/HideShadow` 的所有 production reachability 与 C++ lifecycle 对等性；
- dynamic current DAT identity 发生切换时，shadow special OID 应读取何种 Unity identity；
- Unity `FirstPresentationTick` 在当前 non-test source 未发现直接 writer，当前是
  **UNKNOWN / reachability 待证**，不能当作已生效规则。

## 6. 推荐下一包

**R1-SOURCE-007 — 全量差异盘点闭合、依赖图与分层验收矩阵。**

仅可：

1. 汇总 COV-001～006 与 D-/A-条目；
2. 去重并标明 source、UNKNOWN、approved adapter；
3. 建 producer→consumer / pass 依赖图；
4. 设计 R2+ 的最小闭合修复批次、Change Record 前置、静态检查和 joint fixture；
5. 写清 runtime / Play Mode / future trace 的验收分层。

不得开始任何 Unity gameplay 修改、C++ trace、fixture 实现或 R2。

## 7. 持续边界

- C++ authority 严格只读：不运行、重建、复制、插桩、hook、patch，且不向其目录写入任何文件；
- R1-WP02 full trace 继续 **BLOCKED**；
- CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× visual scale、MobileExtended/
  DesktopExtended、30 Hz、FrameInputSet、SoA/ECS、pool、worker、zero-GC 方向与 T8 暂缓
  均保持不变；
- R2、所有 gameplay 修复、Unity compile / self-check / Play Mode / 性能测试仍未开始。

