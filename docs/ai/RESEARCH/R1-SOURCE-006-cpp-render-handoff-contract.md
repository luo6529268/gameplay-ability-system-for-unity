# R1-SOURCE-006 — C++ Release render handoff / visibility / ordering 源码合同

> 状态：COMPLETED（静态 source 审计；runtime / visual fixture 待后续阶段）。  
> 行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` 中实际参与 `ntsd_new.exe` release 构建的 live source。  
> 证据等级：除非另有标记，以下均为 **VERIFIED(source)**；不是 executable trace、像素比对或 Unity Play Mode 证据。

## 1. 审计边界与 release 参与性

- `Makefile:11-35` 将 `src/core/main.cpp`、`src/entity/game_tick.cpp` 和
  `src/render/renderer.cpp` 一同列入 `SRCS`，并由 `all: ntsd_new.exe` 构建；
  因此本文件使用的 render source 属于 release live build，不是 diagnostic-only 文件。
- 本审计只读取 source；未启动、复制、修改、重建、hook 或向 C++ authority 目录写入任何内容。
- “render handoff”只覆盖战斗实体、影子、hit spark、挂点、排序、camera / perspective 对显示的
  影响。菜单、普通 HUD 美术、SDL API 和像素级 shader/texture 输出不在本次行为合同内。
- C++ renderer 有少量对 Entity 字段的写入；这些字段是否应在 Unity 继续作为 simulation truth，
  必须由下游 fixture 决定，不能仅因它们位于 renderer.cpp 就直接删除或照搬。

## 2. C++ release 的 render callsite 合同

| 合同 | C++ source | 静态规则 | 直接后继 / 风险 |
|---|---|---|---|
| C06-C01：release callback wiring | `main.cpp:4022-4050`、`battle_tick_scheduler.cpp:5-9`、`game_tick.cpp:945-947` | release battle path 通过 `simulation_tick_driver.step_one_tick` → `game_tick` 传入 callback；非 diagnostic branch 的 callback 依次调用 `rend.clear(world)`、`rend.render_world(world)`。 | renderer 是在逻辑 tick 内被调用的 callback，而不是由 renderer 自行推动 battle state。 |
| C06-C02：render 的 tick 时点 | `game_tick.cpp:2061-2083` | PreFrame 已更新 background layer animation；wave phase 与 immediate stage spawn 已完成；随后执行 `pre_postprocess_render`，之后才是 `run_frame_postprocess_pass` 与 `run_late_per_entity_update_pass`。 | 标准 late opoint / late cleanup 在本 render callback 之后；普通 late opoint child 不属于同 tick 的 C++ render 输入。 |
| C06-C03：active list 与 depth order | `renderer.cpp:1300-1318` | 从 slot 0..MAX_OBJECTS-1 收集所有 `active` entity；插入排序只在前者 `z_int > key.z_int` 时移动，因此同 Z 保留原始 slot 升序。 | 对同 Z 的 entity，slot 是可观察 painter order 的稳定 tie-break。 |
| C06-C04：每 entity 的 painter 序列 | `renderer.cpp:1319-1438` | 已排序 entity 逐个执行：perspective offset → shadow → body/entity → respawn counter / label → hit records；HUD 在整个 entity loop 后。 | 同一 entity 的 shadow 必定先于 body，spark 在该 entity overlay 后。 |
| C06-C05：renderer side effects | `renderer.cpp:1321-1330`、`687-758` | `render_world` 会写 `e.render_offset_x`；`draw_hit_records` 会推进 `hit_record_damage[i]`，并在条件满足时减少 `hit_record_count`。 | C++ presentation 不是严格只读。该副作用的 Unity 适配时点必须以专项 fixture 关闭。 |

## 3. Entity / shadow / spark 的精确显示合同

### 3.1 影子

`Renderer::draw_shadow`（`renderer.cpp:517-556`）的 C++ source gate 与位置为：

1. 必须存在 background shadow surface 与 `char_data`；
2. `hit_stop > -70`，且 `abs(hit_stop) % 4 < 2`；
3. `link_state >= 0`；被 held / caught 的 child 不画影子；
4. 当前 frame 必须存在，且 `state != 3005 && state != 9997`；
5. `char_data->oid != 223 && oid != 224`；
6. destination 为
   `x_int + render_offset_x - shadowWidth/2 - camera_x`，
   `z_int - shadowHeight/2`。

注意：type-3 的 `type3_visual_z_offset` 不用于 shadow。影子始终取 `z_int`；
body 才会使用 type-3 的 display Z。

### 3.2 实体 body

`Renderer::draw_entity`（`renderer.cpp:558-685`）的 source contract：

1. body gate 为 `hit_stop > -25` 且 `abs(hit_stop) % 4 < 2`；
2. `frame_delay < 0` 时，X 额外加 `6 * (game_tick & 1) - 3`；
3. 基础 screen X 是 `x_int + render_offset_x - camera_x + extra_x`；
4. 普通 body screen Y 是 `z_int + y_int`；仅 `WeaponType::CONSUMABLE3`
   使用 `(int)(z - type3_visual_z_offset) + y_int`；
5. current frame 必须存在、frame 的 `pic != 999`；最终 pic 为
   `fd->pic + e.unk_318`；
6. facing=1 使用预翻转 surface，并用 `frameWidth - centerx` 调整 X；否则用 `centerx`；
   Y 使用 `centery`；
7. `state == 9997` 不是 body skip 条件，而是 body destination X 的 clamp 条件。

### 3.3 hit spark

`Renderer::draw_hit_records`（`renderer.cpp:687-758`）按当前 entity 内 slot 顺序处理：

- age 区间决定 spark pic 与 `xoff/yoff`；
- position 为 `hit_record_x + render_offset_x - camera_x - xoff`、`hit_record_z - yoff`；
- 若该 age 可画，C++ 在 blit 后递增 `hit_record_damage[i]`；
- 若 age 不可画且该 record 正好是 tail，递减 `hit_record_count`。

这说明 C++ 的 spark life 并非纯 GPU-only 数据；但 source 尚不足以证明它被同 tick 后续 gameplay
读取。因此“它一定是 simulation field”仍为 **UNKNOWN**。

## 4. Camera、perspective 与 display-only 派生字段

| 合同 | C++ source | 静态事实 | 说明 |
|---|---|---|---|
| C06-C06：camera target | `game_tick.cpp:2026-2059` | 在 render callback 前，C++ 根据存活 character 的 x 计算并平滑更新 `camera.camera_x/camera_vel`。 | 该 C++ display camera 链存在于 authority source；其 Unity 适配是否保持同样镜头视觉，是独立于 battle position truth 的问题。 |
| C06-C07：perspective offset | `renderer.cpp:1321-1330` | 若 stage perspective 数据有效，renderer 以 `camera_x`、`x_int`、`z_int` 和 near/far 参数写 `render_offset_x`；否则写 0。 | `render_offset_x` 是 C++ renderer-derived field，不能当作碰撞/输入的逻辑位置。 |
| C06-C08：camera consumers | `renderer.cpp:460-505`、517-575、687-716 | background、shadow、body、spark 都使用 `camera_x`；同一 offset / camera 必须应用到 body 与 shadow / spark 的相对 X。 | 本文件只确认渲染 consumer；camera 在音频或其他 C++ 模块的 consumer 不在此表中断言。 |

## 5. 已闭合的静态 Unity mapping（不是 runtime 对齐证书）

| C++ contract | Unity source mapping | 当前静态结论 |
|---|---|---|
| preframe → stage → render → postprocess / late 的宏观位置 | `NTSDBattleTickSystem.cs:355-380` | Unity `RenderDispatch` 位于 `CurrentWaveStage` 后、`FramePostProcess` 前，主 tick 位置已映射。 |
| active → Z asc → slot tie-break | `SimulationWorld.StageRender.partial.cs:385-396`；`BattlePresentationShadowBuild.cs:2186-2301` | CentralOnly 先按 runtime slot 收集，再稳定 radix 按 Z 排序；fallback comparer 也是 Z→slot→stableId。主排序合同可静态映射。 |
| shadow → entity → overlay → spark | `BattlePresentationShadowBuild.cs:2496-2813` | command base order 为 shadow、body、overlay、hit record 的连续子序；对应 C++ painter 序列。 |
| hit-stop gate | `LF2ObjectRenderer.cs:380-388` | Unity body 使用 `> -25` / modulo-4；shadow 使用 `> -70` / modulo-4，与 C++ gate 一致。 |
| pic offset / type-3 display Z | `LF2Entity.cs:4523-4544` | `RenderPicOffset` 对应 C++ `unk_318` 的 source role；type-3 body 用 `Z-Type3VisualZOffset`，shadow/order 保持 `ZInt`。 |

## 6. 无法仅由 source 关闭的项

1. C++ SDL blit 的最终像素、Unity URP transparent pass 的最终像素、纹理切片过滤、实际 pivot
   和 GPU transparent primitive order 不可由本文件的静态阅读证明一致；
2. C++ `hit_record_damage` 在 render callback 内递增，与 Unity presentation consumption 的实际
   wall-clock / render-frame 关系需要最小 spark fixture；
3. entity DAT identity 在 runtime 可能切换时，C++ `char_data->oid` 与 Unity
   `ObjectId / CurrentDatObjectId` 哪个应承担 shadow special OID gate，需要 data-switch fixture；
4. C++ camera / perspective 的屏幕视觉与已批准 Unity fixed-world presentation adaptation 不要求
   像素级相同；其可观察“实体 / 影子不能被另一实体的逻辑移动错误带走”仍需用户 Play Mode 验收。

## 7. SOURCE-006 后续 fixture 输入

后续仅在 R1-SOURCE-007 闭合并获准进入修复/验收时，为每一个 render-related item 记录：

- tick、runtime slot、stable generation、active / pending state；
- current frame / pic / render pic offset、state、hit-stop、frame-delay、facing；
- x/y/z/zInt、displayZ、renderOffsetX、cameraX；
- relation/link 与 held attachment offset；
- source command order（shadow/entity/overlay/hit record）、resource resolve reason、
  central segment / submission state；
- 最短复现步骤及 C++ source contract，而不是像素截图单独裁决。

