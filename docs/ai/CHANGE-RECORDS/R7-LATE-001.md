# R7-LATE-001 — late state-special chain / state9996 structural writer

<!-- CHANGE-RECORD
id: R7-LATE-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:/QQFile/NTSD2.4/ntsd_release/Makefile:11-35;src/entity/game_tick.cpp:352-428,577-584,687-692;include/game_world.h:23-258;include/ntsd_types.h:137-146
evidence: SOURCE-CONFIRMED-DIFFERENCE / CODE-WRITTEN / FRESH-UNITY-COMPILE / GT11-MATRIX-PASS / WARMED-NO-SLOT-0B / FULL-SELFCHECK-PASS
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING`

## 1. Authority / requirement

C++ release在late per-entity state-special中按三个独立`if`执行9995、4000、8000 transform并逐段reload
current DAT frame0；最终state9996 + character DAT + attacking1时执行五轮最低空槽structural spawn。
Unity当前两个提前return会截断chain，且没有9996 writer，旧GT-11断言与authority相反。

## 2. Unity before

- `RunStateSpecialPreCollision`在9995/4000后return；
- 8000仅在target DAT存在时写HitStun140；missing target不写frame0；
- exact-character gate把9996当no-op；
- 无4×217+1×218 writer；GT-11要求0 RNG/0 spawn。

## 3. Planned changes

| 文件 | before | after |
|---|---|---|
| `LF2Entity.cs` | 单段transform、提前return | 三段顺序reload；missing DAT frame0与8000 hit-stop对齐 |
| `SimulationWorld.Passes.partial.cs` | late只调用entity transform；9996 skip | world-owned五轮slot/RNG/factory writer；9996 participant |
| `BattleRuntimeSelfCheck.cs` | GT-11旧0 spawn断言 | full/missing/capacity/chain/generation/cursor/0 B矩阵 |

## 4. Protected boundaries

不改late pass顺序、通用opoint API、allocator、pool sizing、RNG class、collision/hit、CentralOnly、
Texture2DArray/dynamic Mesh/URP、1.5×、fixed camera、Mobile/Desktop extended capacity、30Hz、
FrameInputSet、T8或C++。

## 5. Acceptance

- 5 child成功时OID/slot/字段与34 RNG call顺序一致；
- missing217只允许218成功且6 calls；missing218为4×217且28 calls；no slot为0 child/0 calls；
- 9995→4000→8000→9996同调用闭合，final identity与HitStun140保持；
- parent低于child slot时child本pass继续late，parent高于child slot时留下一tick；
- warmed no-slot路径0 B；fresh compile/focused/full/validator/diff通过；
- Play Mode/C++ trace缺失时最高`RUNTIME_PENDING`。

## 6. Expected side effects / rollback

- state9996现在会按authority生成实体并推进global RNG，这是预期行为变化；
- 旧GT-11零spawn结论被本Change明确supersede；
- 回滚仅限本Record三份脚本和关联文档，不触碰其它用户修改。

## 7. Actual changes / verification

- `LF2Entity.RunStateSpecialPreCollision`现按三个独立`if`逐段reload current frame0；9995/4000不再
  提前return；missing target仍写frame0，8000无论target是否存在都写HitStun140；
- `SimulationWorld`在原late state-special segment内执行world-owned 9996 writer：每轮从slot50起取最低
  空槽；无槽break，缺DAT/definition continue且不耗RNG；成功child通过既有pooled task/factory创建并
  显式恢复C++ reset relation defaults、spawner slot、attack-exempt与cooldown；
- exact-character no-op gate只在state9996 + current character DAT + attacking1时进入writer；其它9996
  no-op仍可skip；
- 旧GT-11零spawn断言已supersede为full success、missing217、missing218、no-slot、三段chain、
  missing-transform、slot/generation、lower/higher cursor same-pass与warmed no-slot 0 B矩阵；
- 首次`dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:quiet`失败：
  `SimulationWorld.Passes.partial.cs`把实际位于`NTSD.Animation`的`GameDataManager`误写为`NTSD.App`，
  产生1条`CS0234`；已最小改为当前文件既有`using NTSD.Animation`下的`GameDataManager`，待重跑；
- 此first-failure仅为新代码命名空间引用，不改变Task Contract或生产行为设计。
- 第二次build在fixture中产生6条`CS0122`：测试直接引用了production private
  `SimulationWorld.DynamicRuntimeSlotStart`；已在self-check内改用C++/Unity合同均明确的私有测试常量50，
  不扩大production API；
- final `dotnet build Assembly-CSharp.csproj`：0 error / 43 warnings；
- final `dotnet build Assembly-CSharp-Editor.csproj`：0 error / 51 warnings；warnings均为既有nullable、
  dependency或unused-field告警；
- UnityMCP scripts force compile触发正常domain reload；fresh `Assembly-CSharp.dll`为20:41:14.612、
  `Assembly-CSharp-Editor.dll`为20:41:14.886，晚于20:40:38.868最新source，Console无C#编译错误；
- full `BattleRuntimeSelfCheck`于2026-08-22 20:42:47 +08:00写入`PASS`，实际执行上述GT-11矩阵；
- warmed no-slot writer的512次测量为0 B、0 RNG、0 child；
- `Tools/Validate-ChangeLedger.ps1`：44 records / 31 governed files，PASS；scoped diff check PASS；
- Console两条Error均是既有rest-binding negative control，不是本包失败。

## 8. Stop boundary

若需要改变通用factory/API、slot allocator、pool ownership/RNG abstraction或late pass order，立即停止并
拆分新包；不得在本Change内扩展。

## 9. Remaining evidence

- 真实DAT角色/技能触发链、GameObject pool表现与Scene/Game可见性留R8 Play Mode；
- R1-WP02 C++ runtime trace仍BLOCKED；因此本包不能提升为完整`VERIFIED`；
- 正式已seal battle若pool/task容量不足属于capacity fault，本包未调整既有prewarm sizing策略。
