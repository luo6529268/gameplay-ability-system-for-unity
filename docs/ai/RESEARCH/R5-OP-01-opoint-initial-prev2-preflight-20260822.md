# R5-OP-01 — normal opoint child initial Prev2 source preflight

> 日期：2026-08-22  
> 状态：RUNTIME_PENDING — source合同已实施并通过Unity编译/full self-check；C++ trace / PlayMode待验。  
> 对应差异：D-OP-001  
> Change ID：R5-OP-001

## 结论

C++ Release 的 normal opoint child 在 `spawn_from_opoint(...)` 中先执行
`Entity::reset()`，因此 `prev_frame2=0`；随后只把 current `frame` 写为
`op.action`。normal opoint 位于 `game_tick(...)` 的 late per-entity update，而本
tick 的 collision snapshot（`prev_frame2=frame`）早已完成。因此 child 的出生
tick history 必须保持 0；到下一 tick 的 collision snapshot 才镜像当时的 current
frame。

Unity 当前三条 normal opoint materialization 路径把 `Prev2/Prev2D` 直接初始化为
action/current frame data：

- `LF2Character.InitializeFromOpoint`；
- `LF2WeaponBase.InitializeFrame`；
- `LF2OtherObjectLifecycleModule.InitializeFrame`。

`LF2SpecialAttack.InitializeFrame` 没有该额外 id 写入，reset 后的 0 已保留；但它也
没有把 Unity 的 `Prev2D` cache解析为frame0。`GetCollisionFrameData()`在Prev2D为null时
会回退current action data，因此同一合同还需要为SpecialAttack补齐frame0 cache。本差异
只收窄三条extra id writer并补一条cache adapter，不改变current action、slot/generation、
出生pass、对象池或表现路径。

## C++ Release contract

| 顺序 | Authority | VERIFIED 行为 |
|---|---|---|
| 1 | `Makefile:17,22,32` | `frame_advance.cpp`、`collision.cpp`、`game_tick.cpp` 均参与 `ntsd_new.exe` release 构建。 |
| 2 | `src/entity/game_tick.cpp:1646-1652` | collision collect 前，对当时 active 的实体执行 `prev_frame2=frame`。 |
| 3 | `src/entity/game_tick.cpp:630-632` | normal opoint 在 late per-entity update 中调用 `process_opoint_spawn`。 |
| 4 | `src/entity/collision.cpp:1285-1299` | child `reset()` 后只写 slot/active/identity/current `frame=op.action`。 |
| 5 | `include/game_world.h:216-258` | `Entity::reset()` 明确写 `frame=0; prev_frame=0; prev_frame2=0`。 |

## Unity current mapping

| 路径 | 当前行为 | 差异 |
|---|---|---|
| `LF2Character.InitializeFromOpoint` | `Prev2=action; Prev2D=current action data` | 出生 tick history 提前镜像。 |
| `LF2WeaponBase.InitializeFrame` | 同上 | 同上。 |
| `LF2OtherObjectLifecycleModule.InitializeFrame` | 同上 | 同上。 |
| `LF2SpecialAttack.InitializeFrame` | Prev2 id保持reset 0，但Prev2D为null | id正确；需补frame0 cache，防止碰撞reader回退current action data。 |
| `SimulationWorld.CaptureCollisionFrameSnapshotsAll` | 下一 collision snapshot 原子写 `Prev2=current` | 正式 next-tick mirror owner，保持不变。 |

## Minimal implementation direction

1. 三条存在 extra writer 的 normal opoint initializer 都将 `Prev2` 保持为 reset
   default 0；`Prev2D` 对应 frame 0（存在则 frame0 data，不存在则 null）。
2. current action / `Frame.D`、wait/next、position、velocity、relation、slot 与 registration
   完全不变。
3. 下一次 `CaptureCollisionFrameSnapshotsAll()` 继续成为把 current frame写入 Prev2 的
   唯一 collision snapshot owner。
4. SpecialAttack不新增action→Prev2 writer，只显式保持Prev2=0并解析frame0 cache。

## Focused acceptance matrix

使用 action 非 0、同时具有 frame0 与 action frame 的生产 factory fixture：

1. Character / LightWeapon / Other 三种 child 在 materialize 完成后：current frame=action，
   `Prev2=0`，`Prev2D.frameId=0`，`Runtime.PrevFrame2=0`；
2. SpecialAttack保持Prev2 id=0，并补齐`Prev2D.frameId=0`，证明cache不回退current action；
3. 调用一次 `CaptureCollisionFrameSnapshotsAll()` 后：四种 child 的 `Prev2` 与
   `Runtime.PrevFrame2` 均镜像各自 current frame；
4. 断言 runtime slot仍位于 dynamic band，current frame data / identity不变；
5. 不通过推进整个 tick来伪造出生状态；focused fixture直接观察 materialization 边界与
   下一 snapshot 边界。

## Explicit exclusions

- 不改 opoint action 0→999 的既有 Unity DAT adapter；
- 不改 multiple spawn、kind2 link、position/velocity、late cursor、newborn suppression、
  slot allocator、generation、pool、presentation、render或pass order；
- 不改 C++ authority，不运行/构建/写入 C++ Release；
- 不启动 R1 trace/comparator、R6 render、Play Mode或性能压测。

## Evidence status

- C++ source/default/order/release participation：VERIFIED；
- Unity extra writer / next snapshot owner：VERIFIED；
- 脚本实现与 four-type birth→snapshot self-check：PASS；
- Unity compile：PASS（UnityMCP force refresh；fresh Tundra 23.19s，Assembly-CSharp 17:14:38，`error CS`=0）；
- full self-check：PASS（fresh assembly，2026-08-22 17:15:48）；先前16:54:37 stale-assembly PASS不计；
- C++ runtime trace：BLOCKED by R1-WP02；
- real Play Mode：PENDING。
