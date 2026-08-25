# R5-LIFE-01B — pending/free/generation 与 render logic gate 预检

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、focused tests、compile request与full self-check已通过；Play Mode/C++ trace待验）  
> 对应登记：`D-SCHED-012`（pending/free subset）、`D-LIFE-001`、`D-RENDER-003`（logic half）

## 1. 结论

本轮只读预检没有发现需要立即修改 Unity production runtime 的普通 free/reuse 差异。

1. C++ `free_entity(slot)` 只把当前槽位 `active=false` 并递减 `object_count`，下次生成到该槽位时才 `Entity::reset()`；
2. Unity 用 `PendingFlushDestroy` 先从所有 active pass 与表现 capture 隐藏实体，再释放 slot/generation，最后按旧对象引用完成 pool finalization；
3. Unity 的 generation 与延迟 pool finalization 是安全 adapter：旧 handle 立即失效，旧对象的最终释放不会清掉同槽 newborn；
4. `FirstPresentationTick` 在当前 production writer 中始终保持 reset 默认值 0。当前唯一非零写入来自 test-only fixture，因此它目前没有改变正式 opoint 的 first-visible tick；
5. `OidMergeDormant` 与 C++ oid7/8→51 合体伙伴的 `active=false` 对应。Unity 保留并占用原 low slot 是结构差异，但当前 release live battle 的正式动态分配域不会消费该槽位，静态上可作为安全 adapter；运行时 trace / Play Mode 仍待取得。

因此，本包应认证现有 adapter，而不是为了字段形态一致拆除 generation、pool 或 CentralOnly gate。

## 2. C++ Release source contract

### 2.1 release build 参与性

- `J:/QQFile/NTSD2.4/ntsd_release/Makefile:22,32` 把 `collision.cpp` 与 `game_tick.cpp` 纳入正式 `SRCS`；
- 本文只读取 source，没有修改、运行、构建、复制或向 authority 目录写入。

### 2.2 普通 free/reuse

- `game_tick.cpp:577-691`：late pass 按 slot 升序，entity 被 free 后后续 active gate立即看不到它；
- `game_tick.cpp:2190-2194`：`free_entity` 只写 `active=false` 和 `object_count--`；
- `include/game_world.h:216-258`：完整字段 reset 位于 `Entity::reset()`；
- `collision.cpp:1271-1285` 及 `game_tick.cpp` 各正式生成路径：选到空槽后才调用 reset、写 active/slot/identity。

可观察合同：free 的旧实体从当前 tick 后续 active consumer 消失；该 slot 可被后续生成立即复用；复用后的字段来自 reset + 当前 spawn writer，而不是旧实体残留。

### 2.3 render handoff

- `game_tick.cpp:2061-2073`：render callback 位于 preframe/stage 后、frame postprocess/late entity update 前；
- `renderer.cpp:1306`：实体 render 起点以 `world.objects[i].active` 为资格门。

因此，render callback 前已 free 的实体不可见；render callback 后由 late opoint 创建的实体不会反向修改已经发布的本 tick画面，只能在下一次 render callback 被观察。

### 2.4 oid7/8→51 dormant partner

- `game_tick.cpp:1017-1094`：merge 只允许 self oid7/8，partner scan限定 `pi < 20`；成功时记录 partner slot 到 `unk_32C`，随后 `partner.active=false`、`object_count--`；
- `game_tick.cpp:1098-1154`：split 读取记录的 partner slot，reset 该 slot，恢复 partner 并 `object_count++`；
- release live battle 中，stage immediate spawn 从 slot20 开始（`game_tick.cpp:699-719`），opoint、effect、broken fragment、random weapon与 frame-advance spawn 均从 slot50开始；
- `GameWorld::spawn()` 从 slot0开始，但 release-listed调用者是 battle bootstrap / character select，发生在 battle tick 建立前；diag-only caller不能定义正式 live battle中间态。

静态推论：当前正式 battle tick 内没有 allocator 会在 merge→split 期间占用 partner 的 `0..19` 槽位。因此 Unity 保留 dormant partner 并继续占用该 low slot，不改变当前已定位 live path 的生成选择。若未来加入 battle-time slot0..19 writer、改变 stage/dynamic start 或允许 merge partner>=20，必须重开本条。

## 3. Unity mapping

### 3.1 pending/free/generation

- `SimulationWorld.Registry.partial.cs:1090-1104`：pending/dormant 不进入 active pass；
- `SimulationWorld.Registry.partial.cs:1117-1184`：分配前调用 `ReleasePendingDestroySlots()`，再按最低空槽 claim；
- `SimulationWorld.Registry.partial.cs:1264-1306`：pending entity 的 slot 在生成前可释放，并保留旧对象引用供稍后 finalization；
- `SimulationWorld.Registry.partial.cs:1308-1378`：release校验当前 occupant，解绑rest，推进generation，释放SoA writer与表现绑定，最后把旧对象slot写为-1；
- `SimulationWorld.Registry.partial.cs:1054-1088`：旧对象最终 pool释放按对象引用执行，不会根据旧 slot 清理 newborn generation。

### 3.2 presentation

- `BattlePresentationShadowBuild.cs:1831-1843,1977-1988`：capture 排除 dormant、pending、future-first-tick和无有效current handle；
- `W05OpointLifecycleEditorTests.W05B` 已覆盖：RenderDispatch(T)发布后 late opoint 不反向进入T；RenderDispatch(T+1)首次出现；同槽复用后旧generation不再解析，也不会产生ghost command；
- `BattlePresentationBeginFrameReuseEditorTests` 与 `BattleRuntimeSelfCheck` 的 P3 fixture覆盖 dormant/pending/future gate。

### 3.3 FirstPresentationTick

全仓 production 搜索只发现：

- `NTSDEntityRuntime.Reset()` 写0；
- snapshot/ECS/presentation只读或复制；
- nonzero赋值只存在于 test-only fixture。

normal late opoint 的 first-visible tick 由“RenderDispatch在late opoint之前”这个 pass顺序自然形成，而不是通过把 `FirstPresentationTick` 写为 `T+1` 形成。现有 `W05B-A05` 已显式断言 newborn 的该字段仍为0。

## 4. 状态判定

| 子项 | 判定 | 证据级别 |
|---|---|---|
| 普通 free→same-slot reuse | 等价 adapter；无需 production 修改 | source + Unity mapping + existing fixtures |
| generation / stale handle | Unity-only safety adapter；不得删除 | Unity focused fixture；C++无generation字段 |
| delayed pool finalization | 等价 adapter，旧对象不能清 newborn | existing lifecycle fixture |
| FirstPresentationTick production gate | 当前 production 不可达（始终0） | 全仓 writer inventory；runtime trace仍缺 |
| OidMergeDormant low-slot reservation | `INFERRED` safe adapter | C++完整分配域静态扫描；Play Mode/trace待验 |
| render视觉/descriptor/order | 本包不裁决 | 留给R6 |

最高状态只能是 `RUNTIME_PENDING`；不能由静态合同和 Unity tests 声明 C++ runtime 完整对齐。

## 5. Fresh Unity evidence

- UnityMCP `refresh_unity(mode=force, scope=scripts, compile=request)` 于17:47完成，并在domain reload断线后恢复为editor ready；
- 本包没有任何C# diff，因此 `Assembly-CSharp.dll` 保持17:14:38，不能虚构为重新产出的程序集；refresh后的Editor日志与后续tests/self-check未出现`error CS`或`Compilation failed`；
- EditMode job `582b9e9212264d39b4377b72d7e0374d`：两个focused class共19/19 PASS，0 failed/skipped；
- full request self-check：`Temp/NTSD_BattleRuntimeSelfCheck.result` 于2026-08-22 17:49:18返回`PASS`；
- 这些证据只认证Unity adapter与现有回归，不替代C++ runtime trace或真实战斗Play Mode。

## 6. 重开条件

出现任一条件必须重开独立 gameplay Change Record：

1. production 代码开始把 `FirstPresentationTick` 写成大于当前 tick；
2. battle-time allocator 可以写入 slot0..19；
3. oid7/8 merge partner 允许位于 slot20以上；
4. pending destroy 的旧对象 finalization 能清理同槽新 generation；
5. focused test、Play Mode 或未来 C++ trace显示 first-visible tick、ObjectCount、slot选择或split结果不同。

## 7. Out of scope

- 不修改 C++、Unity gameplay、allocator、registry、renderer、pool或pass顺序；
- 不处理 R6 的sprite descriptor、排序、阴影、camera、1.5 visual scale；
- 不处理 T8 stage.dat、Android、1000AI性能或R1-WP02 trace blocker。
