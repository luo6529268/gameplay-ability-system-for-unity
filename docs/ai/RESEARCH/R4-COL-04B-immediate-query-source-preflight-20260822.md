# R4-COL-04B — immediate `QueryBodyHits` source preflight

> 日期：2026-08-22  
> 类型：只读 C++ / Unity source preflight；本文件建立时未修改 Unity 或 C++ gameplay。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 关联项：`D-COL-004` 的 immediate-query 子范围。  

## 1. 结论

`R4-COL-04A` 之后残留的 `IsPureTransitionSmoke` immediate-query 调用，不能作为“同一条
frozen candidate collection 规则”继续直接删除。只读追溯确认它们属于两种不同的 Unity 旧路径：

1. `LF2Weapon.OnLanded()` 的**实际可达**分支会在武器 state 13 / 高速落地时构造临时 kind0 ITR，
   对自身 BDY 逐个 `QueryBodyHits` 并直接调用角色 `Hit`；
2. `LF2Weapon.ProcessAttack()` 也会做一次即时全体扫描，但目前 `rg` 的全仓库静态调用图中只有
   `ProcessAttackInternal` 的转发定义、没有生产调用者；现有 self-check 还明确断言普通 held `Act`
   不会调用它。

第 1 条与 C++ release live path 形成明确静态差异：C++ weapon landing 只修改落地对象自身的速度、
frame、计数和声音，不在 landing branch 扫描其他实体或直接写其他角色的 HP。它必须作为独立的最小
repair，不能通过仅删除 target 的 oid999 filter 来处理；那样会让一条本就多余的 Unity 伤害链命中更多目标。

## 2. C++ release 合同（VERIFIED）

`Makefile:11-35` 将 `physics.cpp`、`weapon.cpp`、`collision.cpp`、`collision_collect.cpp` 和
`game_tick.cpp` 都编入正式 `ntsd_new.exe`。

### 2.1 正式 tick 位置

- `game_tick.cpp:577-646` 的 `run_late_entity_update` 调用 `frame_tick` 和实体物理分发；
- `game_tick.cpp:1645-1656` 在之后才执行 `collision_collect_candidates` 与 step7 consume；
- `game_tick.cpp:1818-1825` 执行 step9 consume，随后才是 CPoint / held weapon sync。

因此 C++ 的正常命中来源是 candidate collect + loop1/loop2 consume，而不是在物理落地分支内另行
扫描 target。

### 2.2 物理落地没有全体 target query（VERIFIED）

`physics.cpp:228-320` 是 release 的非 character weapon 落地 type 分支：

- type 1 / 4 / 6 / 2 分支只读写落地武器的 `unk_31C`、`y/vx/vy/frame/facing/attacking` 和声音；
- oid999 default 分支只写自身 `y/vx/vy/frame/attacking`；
- 全文件的 `weapon_drop_hurt` 使用只在这些自身字段分支（`231-232`、`262-266`、`291-307`），
  没有 target 枚举、BDY overlap、`collision_*` 调用或 `apply_hurt_*` 调用。

这不是“落地 ITR 在另一文件里继续执行”：`game_tick.cpp` 已将正式 collect/consume 固定在后续 pass；
`collision.cpp:32-1266` 是该 consume writer，`collision_collect.cpp:363-376` 是其唯一正式 collection
入口。全 release source 的 `weapon_drop_hurt`、`WPointData`、`collision_collect_candidates` 和
`apply_hurt_alternate` 交叉检索没有找到 weapon landing → target scan 的 live caller。

### 2.3 held weapon 攻击的 C++ route（VERIFIED）

- `weapon.cpp:109-128` 的 `weapon_sync_held` 只做 held relation 完整性检查；
- `game_tick.cpp:1526-1625` / `1923-2015` 的 held WPoint path做 frame、坐标、cover、throw/release
  同步；没有即时 target query；
- `collision.cpp:91-129` 的 kind5 local ITR transform 在 `holder` 当前 frame 的
  `wpoint.attacking` 指向 holder ITR 时，复制该 ITR 的伤害字段并转为 kind0，然后仍由通用
  collision consume 处理。

所以 C++ 的 held weapon attack 不等价于 Unity `ProcessAttack()` 的“即时 body scan + 直接 Hit”。

## 3. Unity 现状（VERIFIED）

### 3.1 可达的落地额外伤害

`LF2WeaponBase.cs:555-567` 的 native weapon frame advance会进入
`RunFrameAdvancePhysics`，其在 `943-970` 对没有 holder 且实际 landed 的武器调用 virtual
`OnLanded()`。

`LF2Weapon.cs:277-357` 的 state 13、高速落地分支除了自身落地字段写入，还：

1. 创建 `landingSplashInteraction`；
2. 按 weapon 当前 frame 的每个 BDY 调用 `sceneQuery.QueryBodyHits(...)`；
3. 对异 relation character 直接调用 `LF2Character.Hit` 或 `LF2CharacterDatHitResolver.TryResolveHit`。

这条直接 target writer 在 C++ `physics.cpp` 对应 landing contract 中不存在。

### 3.2 静态不可达的 held immediate query

`LF2Weapon.cs:481-570` 的 `ProcessAttack()`也会把 `wpoint.attacking` 转为临时 ITR并即时查目标。
但全仓库 `rg "ProcessAttackInternal" Assets -g "*.cs"` 只有
`LF2WeaponBase.cs:250-252` 的转发定义；没有 runtime caller。
`BattleRuntimeSelfCheck.cs:11432-11435` 已验证普通 held `weapon.Act(...)` 不调用
`ProcessAttack`。当前结论是 **INFERRED: dormant/unreachable**，不是“可安全删除的 dead code”；
没有 runtime trace 前不得把它作为本包的修改对象。

### 3.3 不能只删除 `IsPureTransitionSmoke`

`BruteForceSceneQuery.QueryBodyHits` 的 cache 和 scan 分支在 `1240`、`1270` 过滤 target。
若直接删除这两行，active landing splash 会从“错误地伤害一部分对象”变成“错误地伤害更多对象”。
因此 04B 的最小实现只能先移除 active landing branch 的即时 target writer；helper 与 dormant
`ProcessAttack` 保持不动，留给后续独立 reachability / cleanup work。

## 4. 最小实现方向（PLANNED）

新建 `R4-COL-04B / R4-COL-004B` 脚本包，允许：

1. 从 `LF2Weapon.OnLanded()` 的 state13/high-speed landing 分支删除
   `QueryBodyHits` → `Hit` 的额外 target side effect；
2. 保留同一分支原有的自身 HP、落地坐标、反弹速度、clamp 和 return；
3. 在 self-check 中构造“重叠 target + 触发 landing”的夹具，断言这个物理分支不直接伤害 target；
4. 不改 `BruteForceSceneQuery` helper、dormant `ProcessAttack`、candidate collect、kind5 transform、
   scheduler、CPoint、held/link、opoint、DAT/资源或 render。

## 5. 验收与限制

| 层级 | 条件 |
|---|---|
| S0 | C++ `physics.cpp` / `game_tick.cpp` / `collision.cpp` source contract与Unity callable chain已复核。 |
| S1 | 重叠 target的 weapon landing fixture只发生 C++ 对应的自身 landing side effect，不直接改 target HP。 |
| S2 | existing held `Act` 不调用 `ProcessAttack` 的断言保持通过。 |
| S3 | Unity compile、full `BattleRuntimeSelfCheck`、ledger validator、`git diff --check` 实际通过。 |
| S4 | 最高只能是 `RUNTIME_PENDING`；C++ runtime trace和真实 weapon landing Play Mode仍待。 |

## 6. Unknown / stop conditions

- **UNKNOWN**：没有 C++ runtime trace，不能用本静态结论宣布所有 weapon landing / held attack 完整对齐；
- 假如最小删除会要求重排 generic candidate pass、改变 WPoint/kind5 或修改 weapon DAT，停止并拆包；
- dormant `ProcessAttack` 的未来调用者、反射调用或工具用途当前没有证据；不得在本包清理；
- C++ authority、T8、CentralOnly/Texture2DArray、1.5×视觉缩放、扩展容量、30 Hz、FrameInputSet、
  SoA/ECS/pool均不在修改范围。

