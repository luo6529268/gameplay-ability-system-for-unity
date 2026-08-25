# R1-SOURCE-003 — Unity 帧推进 / 物理 / 移动 / 生命周期 crosswalk 与差异

> 状态：**IN_PROGRESS（静态 source inventory）**。  
> C++ authority：`J:\QQFile\NTSD2.4\ntsd_release` 中参与正式 release live path 的
> `src/entity/game_tick.cpp`、`frame_advance.cpp`、`physics.cpp` 及其实际调用者。  
> Unity evidence：当前仓库源码。未运行 C++ executable、Unity、trace、self-check 或 Play Mode。

本文件只回答“当前源代码如何不同”。它不把旧 C#、Unity self-check、ECS shadow、
checksum 或历史性能结论升级为 C++ 行为证据；也不授权修改 gameplay。

## 1. C++ → Unity checkpoint crosswalk

| C++ checkpoint | C++ release source contract | Unity 对应位置 | 当前静态结论 |
|---|---|---|---|
| F00 | `game_tick.cpp:1160-1200`：state 400/401 在 `g_frame_toggle == 0` 时，按升序 slot 扫描；400 取首个最短曼哈顿距离敌人，401 取首个最远同队角色；失败或命中均归零 Y/Vx/Vy/Vz。 | `LF2Entity.cs:3883-3950`、`SimulationWorld.Passes.partial.cs:899-1005` | 主筛选、严格比较和位置/速度写入有静态对应；target 的 relation/character-DAT 映射仍需与 R1-SOURCE-005 一并验收。 |
| F01 | `game_tick.cpp:1207-1214`：state500 在 `unk_33C == -1 \|\| unk_324 >= 0` 时只写 `frame=0`。 | `SimulationWorld.Passes.partial.cs:1157-1171` | `TransformTargetObjectId/TransformOriginalObjectId` 条件与 raw frame 写入可以静态映射；没有 runtime trace。 |
| F02 | `game_tick.cpp:1219-1245`：state501 由 `unk_33C` 找 DAT，替换 owner；再按 slot 扫描 `kill_count==ownerSlot && hp>0` child，按整数 Y 写 212/0。 | `SimulationWorld.Passes.partial.cs:1186-1237` | 主字段和升序 child scan 有对应；transform 后 child 的 slot/generation、frame cache 和 newborn visibility 归 R1-SOURCE-005。 |
| F03 | `frame_advance.cpp:25-87`：frame delay 先向 0 递进并 return；negative link / kind2 cpoint return；非角色 DAT 先应用 dvx/dvy/dvz，再进入 `physics_update`。 | `LF2Entity.cs:5278-5295,5356-5400`，`LF2WeaponBase.cs:943-976`，`LF2Character.cs:1141-1157` | delay、link、cpoint 和 non-character velocity/physics 顺序存在对应；另见 D-MOV-004。 |
| F04 | `physics.cpp:20-71`：x（按方向 block）→ type/oid x 修正 → z（按方向 block）→清 block→type3 visual-Z→基于上一整数 Y 的摩擦。 | `CharacterMechanics.cs:210-249`、`LF2Entity.cs:5372-5395`、`LF2WeaponBase.cs:952-970` | 基础顺序、阈值和 double runtime 均有静态映射。 |
| F05 | `physics.cpp:84-145`：先 `y += vy`；空中才加 gravity；角色 state12 的 180..189 选择与负 weapon-count 的 `(game_tick-1)%12` 覆盖；state18 进入 205。 | `CharacterMechanics.cs:233-249`，`LF2Character.cs:1160-1227`，`LF2Entity.cs:5646-5714` | 状态数值、阈值、tick phase 与 double 读取存在明确映射。 |
| F06 | `physics.cpp:147-320`：按 character/type1/type2/type4/type6/oid999 分支落地；各分支使用不同的 landing Vy、vx、frame、sound、weapon counter 写入。 | `LF2CharacterDamageStateResolver.cs:91-259,383-440`，`LF2Entity.cs:5403-5521`，`LF2WeaponBase.cs:943-976` | 主要 branch 可映射；直接帧写副作用存在 D-MOV-002。逐 type fixture 仍必需。 |
| F07 | `physics.cpp:326-342`：physics 成功执行的尾部才同步 x/y/z integer；`frame_advance` 在 delay/link/kind2 return 时不会到达该同步。 | `CharacterMechanics.cs:253-257`、`BattleEcsCharacterFrameAdvancePass.cs:125-136`、`LF2Entity.cs:5392-5400`；以及 `SimulationWorld.Passes.partial.cs:654-661` | 正常 physics branch 的同步有映射；Unity 另有一次全体同步，形成 D-MOV-003。 |
| F08 | `game_tick.cpp:1280-1421`：先 state9998 free；再 state14/HP/slot/hit-stop gate 的 respawn；最后 first stage-Z clamp。 | `SimulationWorld.Passes.partial.cs:640-895`，`SimulationWorld.cs:1231-1239`，`BattleEcsCharacterStageZPass.cs:168-198` | state9998、respawn 两分支、effect998、Z clamp 都有 source mapping；structural free/newborn slot visibility 和 effect998 exact write 留给 R1-SOURCE-005。 |
| F09 | `frame_advance.cpp:802-996`：late frame tick 使用 current key、frame delay、link/cpoint、counter、next、212 jump init、PP/turn、defend lock。 | `LF2Entity.cs:5767-5916`、`LF2Character.cs:1263-1358`、`BattleEcsCharacterFrameTickPass.cs:90-96` | counter/next/jump/PP/turn shape 有对应；D-MOV-001 与 D-MOV-004 会在此点改变门控。 |

## 2. 已确认的静态差异

### D-MOV-001 — current key 生命周期提前结束

- **C++ authority**：
  - `frame_advance.cpp:80-83` 在 non-character `dvz` 分支读取 `key_up/key_down`；
  - `frame_advance.cpp:941-951` 在跳入 frame 212 时读取四个方向 key；
  - `frame_advance.cpp:977-980` 在 MP turn-around 时读取 left/right key；
  - 这些读取均晚于 C++ input callback，且 `game_tick.cpp`、`frame_advance.cpp` /
    `physics.cpp` 中没有在 frame advance 前的全局 current-key clear。
- **Unity current code**：
  `SimulationWorld.Passes.partial.cs:599-612` 会在每个 entity 的 frame advance /
  transit 之前调用 `BattleCharacterInputWriter.ClearCurrentKeys(...)`。
- **静态差异**：Unity 使同 tick input 在 F03 前不可见，而 C++ 将其保留到 F03 和 F09
  的上述读取点。
- **状态**：`待处理（静态确认）`。不能直接删 clear；必须先由 R2/R3 定义不同 input
  来源、held edge、AI 写入与清除的 checkpoint。

### D-MOV-002 — 物理落地分支的 raw frame write 与 `ImmediateFrame` 副作用不同

- **C++ authority**：`physics.cpp:157-223` 的多个 landing branch 只直接写 `core.frame`，
  仅在部分 branch 写 `special.attacking=0`；该段没有写 C++ prev-frame，也没有通过
  一个通用 frame transition helper 重置 wait。
- **Unity current code**：
  `LF2CharacterDamageStateResolver.cs:221-241,383-440` 和
  `LF2Entity.cs:5548-5625` 在对应 landing branch 使用 `ImmediateFrame(...)`。
  `LF2Entity.cs:1196-1212` 显示该方法额外写 `Frame.PN`、`AttackingCounter=0`、
  Sprite 和 `FrameTransistor`。
- **静态差异**：在 early physics 与后续 candidate/collision/late frame tick 之间，
  Unity 的 frame-history、attacking counter 和 transition state 可早于 C++ 改变。
- **状态**：`待处理（静态确认）`。R1-SOURCE-004/005 需闭合 collision 对 prev/history 的
  消费，再决定是否要引入限定范围的 raw landing writer。

### D-MOV-003 — Unity 在 respawn 前无条件同步所有 active runtime 整数坐标

- **C++ authority**：`physics.cpp:326-342` 的 integer sync 属于 `physics_update` 的
  尾部；`frame_advance.cpp:35-48` 遇到 frame delay、negative link 或 kind2 cpoint 会
  return，因此这些 gate 不会执行 x/y/z integer 写回。
- **Unity current code**：
  `SimulationWorld.Passes.partial.cs:654-661` 在 `PostFrameAdvanceDeathCleanupAll`
  开头无条件遍历当前 active entity 并调用 `Runtime.SyncIntegerPosition()`，其后才执行
  respawn gate/坐标平均。
- **静态差异**：Unity 对被 F03 gate 跳过的 entity 仍提前刷新 integer position；这可以
  改变同 tick respawn 平均坐标、后续 integer-field 读取或快照。
- **状态**：`待处理（静态确认）`。需 R1-SOURCE-005 先闭合 held/cpoint/structural writer
  是否依赖它，再通过 fixture 判断最小同步集合。

### D-MOV-004 — Unity-only `ThrowFrameGuard` 额外跳过 frame advance / frame tick

- **C++ authority**：对 release `src/` 的静态搜索只找到
  `game_tick.cpp:137`、`weapon.cpp:122,130` 对 `throw_frame_guard=-1` 的 reset；
  当前已读 live `frame_advance.cpp` / `physics.cpp` / `frame_tick` 路径没有相同 field
  的 read gate。
- **Unity current code**：
  `LF2Entity.cs:5278-5295`、`LF2Entity.cs:5767-5775` 和
  `BattleEcsCharacterFrameTickPass.cs:90-96` 都在
  `ThrowFrameGuard == Frame.N` 时直接 return。
- **静态差异**：这是 C++ 已读 live path 中未出现的 Unity execution gate。当前 Unity
  production source 也没有读到正常战斗路径把它设为非负值，仅见 reset / snapshot /
  diagnostics；因而它目前是“**可能 dormant 的多余逻辑**”，不能写成已复现 gameplay bug。
- **状态**：`待处理（静态确认；运行时可达性待验）`。R1-SOURCE-005 要确认 throw/held
  writer 与 state restore 是否还能赋非负值；若不能，应作为死代码清理候选。

### D-MOV-005 — data-oriented exact-character FrameTick 漏掉 state2000 facing 写入

- **C++ authority**：`frame_advance.cpp:884-887` 无条件处理
  `state == 2000`，以 `vx > 0 ? 0 : 1` 写 facing。
- **Unity current code**：
  - compatibility `RunCommonFrameTick` 在 `LF2Entity.cs:5839-5840` 对
    `LF2States.HeavyWeaponInSky (2000)` 写方向；
  - 但 production default 的
    `BattleEcsCharacterFrameTickPass.ExecuteExactCharacter`
    (`BattleEcsCharacterFrameTickPass.cs:90-209`) 对 exact
    `LF2Character + Character DAT` 绕过该 virtual path，且没有 state2000 分支。
- **静态差异**：若 exact character-DAT runtime 能进入 state2000，optimized path 不会执行
  C++ 要求的 facing write，而 legacy path 会执行。
- **状态**：`待处理（静态确认；DAT 可达性待验）`。不能假设 state2000 “只会出现在
  武器”；先由 fixture / DAT inventory 确认 exact-character profile 的可达性，再决定补写或
  作为不可达 contract 固化。

## 3. 已映射但尚不能验收的子流程

| 子流程 | C++ source evidence | Unity mapping | 当前结论 / 后续 fixture |
|---|---|---|---|
| state400/401 teleport | `game_tick.cpp:1160-1200` | `LF2Entity.cs:3883-3950` | 逻辑已映射，待测试：无 target、同距/同最远 tie、frame-toggle gate、type3/character-DAT relation。 |
| state500/501 transform | `game_tick.cpp:1207-1245` | `SimulationWorld.Passes.partial.cs:1157-1237` | 逻辑已映射，待测试：raw frame history、owner/child same-tick visibility、slot reuse。 |
| character landing states 12/13/18 | `physics.cpp:116-224` | `LF2Character.cs:1160-1227`、`LF2CharacterDamageStateResolver.cs:91-259,383-440` | 主阈值已映射；D-MOV-002 未关闭。 |
| type1/type2/type4/type6/oid999 landing | `physics.cpp:228-320` | `LF2Entity.cs:5403-5521`、`LF2WeaponBase.cs:943-976` | 主阈值及 type 分支已映射；要按 data type 和 pooling shell 分别验收。 |
| state9998 free | `game_tick.cpp:1280-1290` | `SimulationWorld.Passes.partial.cs:640-652` | `UNKNOWN`：`FreeEntityLikeExe` 的 deferred structural visibility / slot reuse 归 R1-SOURCE-005。 |
| respawn count / stored HP branch | `game_tick.cpp:1295-1421` | `SimulationWorld.Passes.partial.cs:744-895` | 主字段写入可映射；effect998 的 direct int-Z / pool slot 语义归 R1-SOURCE-005。 |
| first/second Z clamp | `game_tick.cpp:1423-1438` 及 late path | `BattleEcsCharacterStageZPass.cs:168-198` | 逻辑已映射，待测试：character-DAT shell、newborn、first/second clamp 之间的写入。 |
| frame 212 / MP turn | `frame_advance.cpp:941-982` | `LF2Entity.cs:5812-5887`、`LF2Character.cs:1324-1358` | 主逻辑已映射；D-MOV-001 / D-MOV-004 未关闭。 |
| state2000 facing | `frame_advance.cpp:884-887` | `BattleEcsCharacterFrameTickPass.cs:90-209` | D-MOV-005；先确定 character-DAT 可达性。 |

## 4. 后续可执行验收夹具合同（不在本 WP 实现）

每个夹具必须记录同 seed、初始 runtime slot/generation、DAT version、stage snapshot 和
`FrameInputSet` journal。没有 C++ executable trace 时，验收先分为“source-contract
通过”和“Unity runtime 待测”，不得称为 C++ runtime VERIFIED。

| Fixture ID | 最小初始条件 | C++ source checkpoint | 必须比较的字段 |
|---|---|---|---|
| F-MOV-001 | 角色等待转入 212；分别 held 左/右/上/下、互斥/同时按下 | F09 `frame_advance.cpp:941-951` | frame、Vx/Vy/Vz、current/prev key、FrameWaitCounter、tick。 |
| F-MOV-002 | state12、state13、state18 各一组，覆盖 low/high Vy、Vx 正负边界、weapon count 正负 | F05/F06 | Y/Vx/Vy/Vz、frame、attacking、prev-frame、wait counter、HP/HPBound、weapon count。 |
| F-MOV-003 | frame delay、negative link、kind2 cpoint 各一组，带 fractional X/Y/Z；另有可 respawn entity | F03/F07/F08 | float 与 int position、respawn mean X/Z、candidate 前 snapshot。 |
| F-MOV-004 | type1/type2/type4/type6/oid999 的低/高 landing | F04/F06 | flight counter、frame、Vx/Vy/Vz、dir、sound event、attacking。 |
| F-MOV-005 | state9998、respawn no-count、respawn stored-count/oid998 | F08 | active/slot/generation、HP/PP/HP3/HPBound、frame delay、relation、effect position/int Z。 |
| F-MOV-006 | state400/401、500/501，含 tie、frame-toggle、child | F00-F02 | slot ordering、target/transform ids、frame/history、all mutation order。 |

## 5. R1-SOURCE-003 stop boundary

- 本包不会修改 `ClearCurrentKeys`、landing writer、integer sync、`ThrowFrameGuard`、
  CPoint、held/link、pool、opoint 或 render；
- 不能以 CentralOnly、Texture2DArray、移动端/桌面容量 profile 或 Unity-native
  presentation 作为“修回 C++”的手段；
- `stage.dat` 默认部署仍不进入本包；
- 发生 C++ runtime trace、Unity runtime fixture 或 gameplay 修复之前，必须先完成
  R1-SOURCE-004～007 的 inventory，并建立对应 Change ID。
