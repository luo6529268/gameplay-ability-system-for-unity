# R1-SOURCE-001 — C++ game_tick(...) 主 tick 源码行为合同

> 状态：COMPLETED（R1-SOURCE-001 的静态主 tick contract）；本文件只记录已读取的 C++ release live source 与 Unity static mapping。  
> Evidence：源码静态证据；未启动 C++ executable、未取得 full trace、未运行 Unity。  
> C++ authority：J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp::game_tick(...)；Makefile 已列入 game_tick.cpp、frame advance、physics、collision、hit、weapon、cpoint、input 和 renderer 源文件到 ntsd_new.exe。

## 1. 证据范围与限制

- VERIFIED（source）表示该顺序/字段写入已直接出现在当前已读取的 C++ release source 和 Makefile build list 中。
- INFERRED 表示已找到名称和局部调用关系，但仍需在后续 source Work Package 闭合具体 helper 的全部语义。
- UNKNOWN 表示不能由已读 source 片段确定；不得用 C#、Unity self-check、Authority400、checksum 或性能数据补全。
- 本合同不将当前 source tree 自动等同于实际 ntsd_new.exe binary；source/executable identity 仍是 B-R1-WP02-04 blocker。

## 2. C++ 主 tick 的已确认骨架

| C++ checkpoint | Source 坐标 | 已确认顺序 / 写入 | Evidence | 后续所有权 |
|---|---|---|---|---|
| T00 — tick header | game_tick.cpp:945-951 | world.game_tick++、双缓冲 input_phase 翻转、g_frame_toggle 翻转。 | VERIFIED（source） | R1-SOURCE-001 |
| T01 — cooldown | 990-1000 | 先运行 cooldowns_tick(world)；随后处理 F2 单帧 gate。 | VERIFIED（source） | R1-SOURCE-002 / R3 |
| T02 — post-cooldown input callback | 1002-1005 | 仅当 callback 存在且 F1/F2 slow gate 未阻断时调用 post_cooldown_input()。 | VERIFIED（source） | R1-SOURCE-002 |
| T03 — 0..19 特殊维护 | 1006-1159 | 在 input callback 之后扫描前 20 个 slot，维护 OID 7/8/51 相关 unk_338、合并/拆分等状态。 | VERIFIED（source） | R1-SOURCE-002 / R3 |
| T04 — 400/401/500/501 state special | 1160-1245 | 400/401 目标重定位、500 frame 归零、501 runtime identity/frame 迁移均在 frame logic 前。 | VERIFIED（source） | R1-SOURCE-003 |
| T05 — frame logic | 1247-1260 | 按 0..MAX_OBJECTS-1 升序；仅 active、存在 DAT 且 entity_uses_frame_logic_pass(...) 的实体调用 dispatch_frame_logic。 | VERIFIED（source） | R1-SOURCE-003 |
| T06 — frame advance | 1271-1276 | 第二个独立升序扫描；满足 entity_uses_frame_advance_pass(...) 时调用 dispatch_frame_advance。 | VERIFIED（source） | R1-SOURCE-003 |
| T07 — death / 9998 / respawn special | 1280-1421 | state 9998 释放；state14/HP/respawn gate、OID 998 等在后续 Z/held 前处理。 | VERIFIED（source） | R1-SOURCE-003 / R5 |
| T08 — first character Z clamp | 1423-1439 | active character DAT 扫描，double z 夹到 stage boundary，再写 z_int=(int32_t)z。 | VERIFIED（source） | R1-SOURCE-003 |
| T09 — first negative-link held loop | 1441-1643 | 对 link_state < 0 的实体检查 holder/target reciprocal relation；读取 holder 当前 frame、wpoint、cover、velocity 和整数 center/wpoint 公式，更新 held object 或 release。 | VERIFIED（source） | R1-SOURCE-005 |
| T10 — collision snapshot / collect | 1645-1652 | active entity 先写 prev_frame2=frame，随后调用 collision_collect_candidates(world, world.game_mode)。 | VERIFIED（source） | R1-SOURCE-004 |
| T11 — character collision consume | 1653-1656 | run_collision_loop1_pass 在 random weapon 前执行。 | VERIFIED（source） | R1-SOURCE-004 |
| T12 — natural random weapon | 1657-1817 | weapon count、RNG gate、从 slot 50 起找首个空 slot、对象候选和 spawn 均发生在 object collision 前。 | VERIFIED（source） | R1-SOURCE-004 / R7 |
| T13 — object collision consume | 1818-1822 | run_collision_loop2_pass 在 CPoint/weapon sync 前执行。 | VERIFIED（source） | R1-SOURCE-004 |
| T14 — CPoint + weapon sync | 1823-1825 | run_cpoint_and_weapon_sync_passes 紧跟 object collision；内部先 run_cpoint_runtime_pass，再 weapon_sync_runtime_pass。 | VERIFIED（source） | R1-SOURCE-005 / R2 |
| T15 — positive-link validation | 1827-1846 | link_state > 0 时校验 target slot、target active、target holder slot；无效则清 link_state。 | VERIFIED（source） | R1-SOURCE-005 / R2 |
| T16 — second Z + negative-link held loop | 1848-2019 | 再次 character Z clamp，随后第二轮 link_state < 0 held processing；位于 CPoint/weapon sync/positive-link validation 后。 | VERIFIED（source） | R1-SOURCE-005 / R2 |
| T17 — preframe / camera / stage / render handoff | 2021-2077 | apply_preframe_bounds、C++ camera_x carrier、background counter、stage wave advance/spawn 后调用 pre_postprocess_render()；F1 slow gate 在 render 后、postprocess 前 early return。 | VERIFIED（source） | R1-SOURCE-006 / R2 |
| T18 — postprocess / late / tail | 2078-2090 | 依序执行 frame postprocess、late per-entity update、mode2 random weapon tail、entity postframe tail，再清全局 flags。 | VERIFIED（source） | R1-SOURCE-003/004/005/006 |

## 3. 已确认的顺序不变量

1. T14 不在 candidate collect 之前。CPoint 和 weapon sync 在 object collision（T13）之后；任何 Unity adapter 若在 T10 前执行相同副作用，都必须被视为静态顺序风险。
2. T09 与 T16 是两轮不同位置的 held/link 处理。不能将它们合并为一个“任意时点的 held 更新”而不证明所有副作用等价。
3. T08 与 T16 前的 Z 夹取都是 double → int 的显式写回。Unity 不能只刷新 Transform 或只修改整数缓存。
4. T10 先写 prev_frame2，后收集候选。Candidate/itr/bdy 的 frame carrier 必须以这个边界为准。
5. T11 → T12 → T13 → T14 → T15 → T16 的相对顺序固定。该段是 R2 主调度器和 R4/R5 的共同前置合同。
6. T17 的 render callback 位于 postprocess 之前。Unity 中央 renderer 可以采用 Mesh/URP，但不能把 render handoff 的逻辑观察点移动到 C++ 规定的 postprocess 之后。

## 4. 容量与 slot 语义

- C++ source 在上述主要循环中使用 MAX_OBJECTS 的升序 scan；当前 source 语义是固定 slot table。
- Unity 必须将 C++ 同槽比较限制在 Authority400，但不能把此固定容量传播为生产限制：
  - Authority400：固定 400 slot，对照/诊断；
  - MobileExtended：1,050 initial slot、1,000 active；
  - DesktopExtended：page-normalized initial capacity、dynamic growth、无 production active 硬上限。
- 因此“循环上限不同”不是自动 gameplay mismatch；要检查的是当前 profile 内升序 cursor、slot reuse、generation、newborn visibility 与 C++ 可观察顺序是否保持合同。

## 5. 主 tick 的未知项

| 项目 | 状态 | 原因 / 后续 |
|---|---|---|
| post_cooldown_input 内部人类/AI 输入顺序 | VERIFIED（source） | 已由 R1-SOURCE-002 闭合到 `main.cpp` callback、`InputHandler::poll`、`prepare_ai_input` 和 `apply_input`；其 Unity scheduling difference 见 `R1-SOURCE-002-input-contract.md` 的 D-SCHED-005。 |
| T05/T06 的各 category dispatch 完整字段副作用 | INFERRED | 需 R1-SOURCE-003 追踪 frame/physics helper。 |
| T11/T13 candidate consume、hit、grab 细节 | UNKNOWN | 需 R1-SOURCE-004。 |
| T09/T16 与 Unity held writer 的全部 release/cpoint 语义 | INFERRED | 需 R1-SOURCE-005。 |
| T17 C++ camera_x 中哪些属于 Unity 必须可观察表现 | UNKNOWN | 需 R1-SOURCE-006；不可反写 Unity simulation。 |
| full C++ runtime first difference | BLOCKED | R1-WP02 仍缺只读、可重复 full trace。 |
