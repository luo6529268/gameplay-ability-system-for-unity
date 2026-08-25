# R1-SOURCE-003 — 帧推进、物理、移动、落地与生命周期源码合同

> 状态：COMPLETED（静态 source contract）。  
> Evidence：目前为 C++/Unity source 静态阅读；未运行 C++ executable、Unity、trace 或
> Play Mode，以下没有 runtime VERIFIED 结论。

## 1. 已闭合的 C++ 主路径骨架

| checkpoint | C++ source | 已确认合同 |
|---|---|---|
| F00 | `game_tick.cpp:1160-1245` | state 400/401、500、501 在 frame logic 前分三个升序 full scan 处理。 |
| F01 | `game_tick.cpp:1252-1260` + `entity_pass_gates.h:11-16` | frame logic 只处理 active、runtime data、非 character DAT 且 current `hit_Fa > 0` 的 entity。 |
| F02 | `game_tick.cpp:1271-1276` + `entity_pass_gates.h:18-20` | frame advance 是独立的第二次升序 full scan，处理每个 active runtime entity。 |
| F03 | `frame_advance.cpp:25-97` | `frame_delay` 先向 0 递进并 early return；negative link / cpoint-kind2 也阻断 physics；non-character先应用 frame velocity，随后进入 `physics_update`。 |
| F04 | `physics.cpp:12-343` | x → oid correction → z → clear bounds → ground friction → y → gravity/landing → double-to-int sync；所有阈值和 per-type landing 由 source分支决定。 |
| F05 | `game_tick.cpp:1280-1421` | state9998 free 在 respawn scan 前；state14/HP/respawn/effect998 仍位于 first Z clamp 前。 |
| F06 | `game_tick.cpp:1423-1438` | first Z clamp：character DAT `double z` clamp 后显式 `z_int=(int32_t)z`。 |
| F07 | `game_tick.cpp:577-647,687-691,2078-2087` | postprocess 后对每 slot：state special → recovery → `frame_tick` → entity collision → death/opoint/cleanup/N30/tail/prev-frame。 |
| F08 | `frame_advance.cpp:802-996` | late `frame_tick` 的 counters / wait-next / frame212 jump-init / PP / defend-lock tail 仍使用 entity current key fields。 |

## 2. Unity 已读映射

| C++ contract | Unity source | 当前 static mapping |
|---|---|---|
| F00 | `SimulationWorld.Passes.partial.cs:899-1237` | `EarlyFrameAdvanceSpecialsAll`：teleport，再 state500，再 state501；保持独立 ordered scan/handle fallback。 |
| F01 | `SimulationWorld.Passes.partial.cs:1240-1262` | `FrameLogicBeforeAdvanceAll`：non-character + hit_Fa > 0，per-slot call / flush。 |
| F02/F03/F04 | `SimulationWorld.Passes.partial.cs:589-638`、`BattleEcsCharacterFrameAdvancePass.cs:72-136`、`CharacterMechanics.cs:198-258` | `SerialTickAll` 调用 frame advance/dynamics；character mechanics维持 x/z bound、friction、y/gravity/landing outcome；non-character另走 native compatibility path。 |
| F05 | `SimulationWorld.Passes.partial.cs:640-702,744-896` | state9998 cleanup 后，独立 respawn scan/respawn effect。 |
| F06 | `SimulationWorld.StageRender.partial.cs:244-261` | two scheduler call sites共用 `ClampCharacterZToStageBoundsAll`；`PhysicsState`绑定 Runtime，因此 `PS.z` 是 runtime Z proxy。 |
| F07/F08 | `SimulationWorld.Passes.partial.cs:1358-1613`、`LF2Entity.cs:5767-5892` | late state/recovery/frame tick/exit/death-opoint/tail 分段执行；common frame tick保留 counter、link/cpoint、wait-next、jump-init、PP/defend lock。 |

## 3. 已确认静态差异

### D-MOV-001 — Unity 在 frame advance 前清除本 tick current key

| C++ contract | Unity source | 状态 |
|---|---|---|
| C++ `input.poll` / `apply_input` 写入的 `key_*` 不会在 `game_tick.cpp`、`frame_advance.cpp` 或 `physics.cpp` 的 frame advance 前被统一清除；`frame_advance.cpp:80-83`、`frame_tick:943-951,977-980` 都仍读取 current keys。 | `SimulationWorld.Passes.partial.cs:599-612` 在每一个实体进入 `battleEcsCharacterFrameAdvancePass.TryExecute` / `SimTransit` 前调用 `battleCharacterInputWriter.ClearCurrentKeys(entity.Runtime)`；它清 `KeyUp/Down/Left/Right/Attack/Jump/Defend`。 | **待处理（静态差异已确认）** |

可受影响的 C++-defined observable subflows至少包括：

- frame 212 初始跳跃的 horizontal / depth velocity；
- non-character frame `dvz` 对 current up/down/cooldown 的选择；
- late `frame_tick` 中 negative-mp turn-around 与 direction-dependent frame transition。

这不是授权立即删除 ClearCurrentKeys：它也可能承担旧 Unity adapter 的其他边界职责。后续
R2/R3 需要先建立“current key 保留至哪个 C++ checkpoint、何时清、human/AI分别怎样清”的
独立 contract，并以 Change ID 分批修改。

### D-MOV-002 — landing raw frame write 的中间态不同

C++ `physics.cpp:157-223` 的多个 landing branch 只直接写 `core.frame`（仅部分 branch
另写 `special.attacking=0`）；Unity
`LF2CharacterDamageStateResolver.cs:221-241,383-440` 与
`LF2Entity.cs:5548-5625` 使用 `ImmediateFrame(...)`。后者在
`LF2Entity.cs:1196-1212` 还会写 `Frame.PN`、`AttackingCounter`、Sprite 和
`FrameTransistor`。这会在 C++ 的 next candidate/collision pass 到来前提前改变 Unity
中间态，因此是 **待处理（静态差异已确认）**。

### D-MOV-003 — respawn 前的全体 integer position sync

C++ 的 x/y/z integer sync 只位于成功进入 `physics_update` 的尾部
(`physics.cpp:326-342`)；frame delay、negative link 或 kind2 cpoint 的 early return
不会执行它。Unity `PostFrameAdvanceDeathCleanupAll`
(`SimulationWorld.Passes.partial.cs:654-661`) 却在 respawn scan 之前对每个 active entity
无条件 `SyncIntegerPosition()`。这可改变同 tick respawn 平均坐标和之后读取的 integer
字段，是 **待处理（静态差异已确认）**。

### D-MOV-004 — Unity-only ThrowFrameGuard gate

当前已读 C++ release `src/` 只在 `game_tick.cpp:137`、`weapon.cpp:122,130` 找到
`throw_frame_guard=-1` reset；`frame_advance.cpp` / `physics.cpp` / late `frame_tick`
没有对应 read gate。Unity 在 `LF2Entity.cs:5278-5295,5767-5775` 和
`BattleEcsCharacterFrameTickPass.cs:90-96` 会在 `ThrowFrameGuard == Frame.N` 时跳过
frame advance/frame tick。当前 Unity production source 也没有读到正常战斗路径把它设为
非负值，因此它是“可能 dormant 的多余 gate”，而非已复现 gameplay root cause。状态为
**待处理（静态确认；运行时可达性待验）**。

### D-MOV-005 — optimized exact-character FrameTick 少了 state2000 facing branch

C++ `frame_advance.cpp:884-887` 对所有 entity 的 `state==2000` 写
`facing=(vx>0)?0:1`。Unity compatibility common path
`LF2Entity.cs:5839-5840` 有相同的 state2000 方向更新，但默认的
`BattleEcsCharacterFrameTickPass.ExecuteExactCharacter%%
（`BattleEcsCharacterFrameTickPass.cs:90-209`）对 exact character-DAT 不含此 branch。
因此若 exact character-DAT 可以出现 state2000，production data-oriented path 会漏写方向。
状态为 **待处理（静态确认；DAT 可达性待验）**。

## 4. 已转交后续 Work Package 的依赖

1. 每种 object category 的 `physics_update` / landing 与 Unity weapon/special/other
   adapter 的逐分支 mapping；
2. character landing：C++ state12/state13/state18 的 velocity/frame/HP/weapon-count
   条件，与 Unity `HandleLandingEventForFrameAdvance` 的逐字段对照；
3. state400/401 target selection、state500/501 identity transfer 的 all fields、newborn
   visibility 与 slot reuse；
4. respawn HP/PP/HP max/HP3、random coordinate、effect998 的 float/int/Z offset；
5. frame postprocess、late `frame_tick`、entity collision 的确切跨模块边界。

这些项不再构成本包的 source reading 缺口，但在对应 Work Package 完成前仍保持 `待盘点%%
或 `UNKNOWN`，不应依据旧 C# 或 Unity existing self-check 提前说“逻辑已对齐”。

详细 Unity crosswalk、每项最小 fixture 合同和保持不动的边界见：
`docs/ai/RESEARCH/R1-SOURCE-003-unity-crosswalk-and-diff.md`。
