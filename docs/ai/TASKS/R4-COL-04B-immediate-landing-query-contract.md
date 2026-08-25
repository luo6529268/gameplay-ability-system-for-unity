# R4-COL-04B — weapon landing immediate-query removal

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — source、最小脚本、Unity compile与full self-check已通过；C++ trace / Play Mode待补。  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 关联差异：`D-COL-004B`。  
> 前置调查：`RESEARCH/R4-COL-04B-immediate-query-source-preflight-20260822.md`。

## Goal

移除 Unity weapon state13/high-speed landing 分支中 C++ release不存在的“即时 body scan → 直接命中
其他角色”副作用。武器落地自身的物理字段变化必须保留；正式命中仍由 existing frozen candidate / consume
path 决定。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs`
   - `OnLanded()` 中创建 landing splash ITR、扫描 BDY、直接调用 target `Hit` 的子块；
   - 不动同一分支的自身状态写入和 return。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 为重叠 target 的 landing 建立 focused fixture；
   - 保留/复核现有 held `Act` 不调用 `ProcessAttack` 的断言。
3. 关联治理文档、Change Record、ledger、STATE、总差异登记、主计划与 handoff。

禁止：

- 修改 C++、`BruteForceSceneQuery` 或 `IsPureTransitionSmoke` helper；
- 修改 dormant `ProcessAttack` / `ProcessAttackInternal`；
- 修改 candidate collect、kind5 transform、collision consume、CPoint、held/link、opoint、scheduler、
  DAT/资源、render、pool/容量、网络或性能架构。

## Authority / Evidence

### VERIFIED

- `Makefile:11-35` 将 `physics.cpp`、`weapon.cpp`、`collision.cpp`、`collision_collect.cpp`、
  `game_tick.cpp` 编入 `ntsd_new.exe`；
- `game_tick.cpp:577-646,1645-1656,1818-1825` 将 frame/physics与正式 collision collection / consume
  分为不同 pass；
- `physics.cpp:228-320` 的 weapon landing只改自身字段，无 target scan/hit；
- `weapon.cpp:109-128` 和 `game_tick.cpp:1526-1625` 的 held sync无 immediate target scan；
- `collision.cpp:91-129` 是 C++ held WPoint attack进入 common collision consume 的 kind5 transform；
- Unity `LF2Weapon.cs:291-353` 在实际 `OnLanded` 调用链中额外直接伤害 target。

### INFERRED / UNKNOWN

- `ProcessAttack` 在当前 Unity static call graph不可达，且 self-check覆盖 held `Act` 不调用它；
  runtime/reflection reachability仍是 `UNKNOWN`，不在本包处理；
- C++ runtime trace与真实 weapon landing Play Mode未获得，最高不能超过 `RUNTIME_PENDING`。

## Files likely involved

| 文件 | 责任 |
|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` | 移除 active landing direct-hit 子块。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 重叠目标 landing focused fixture。 |
| `docs/ai/CHANGE-RECORDS/R4-COL-004B.md` | 改动、证据、回滚边界。 |

## Deliverables

1. landing branch不再直接遍历 target并调用 `Hit`；
2. focused fixture实际走到落地 branch，并证明 target HP未被该 branch改写；
3. Unity compile、full self-check、ledger validator和diff hygiene的真实结果；
4. 完整 Change Record / ledger / STATE / 差异登记 / 主计划 / handoff留痕。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 source | C++和Unity完整调用链与字段分支如前置调查。 |
| S1 behavior | overlapping target保持HP，武器仍写自身C++对应落地字段。 |
| S2 regression | held `Act` 仍不触发 `ProcessAttack`；现有R4 fixture不回归。 |
| S3 runtime | Unity scripts compile=0 error，full self-check PASS，ledger validator和`git diff --check`通过。 |
| S4 evidence | 状态仅可为`RUNTIME_PENDING`；C++ trace/Play Mode明确未关闭。 |

## Stop conditions

- 需要修改 immediate query helper、dormant held path、DAT/资源、pass ordering或generic candidate system；
- fixture无法隔离landing direct writer与正式candidate consume；
- C++ source发现另一个实际参与 release 的 landing target writer；
- 需要回退任何已批准 Unity adapter边界。

## Out of scope

R1-WP02、C++ executable、dormant `ProcessAttack` cleanup、D-COL-005+、D-HIT、R5～R8、T8 default
`stage.dat`、服务器、Android、长时间性能与Play Mode。

## 实施进度（2026-08-22）

- 已从 `LF2Weapon.OnLanded()` state13/high-speed branch 移除 landing splash临时 ITR、BDY target scan和
  direct `Hit` writer；自身HP、`Y/Vx/Vy/Vz`、clamp和return未改。
- self-check新增“overlapping target可被旧 immediate query看到”的前置断言，并验证触发落地后target HP
  未变化、weapon自身仍执行 -100 HP 和 `Y=0/Vy=-3.5`。
- 现有 Unity Editor（UnityMCP port 6401，Unity 2022.3.62f3）在 2026-08-22 04:52 +08:00 refresh/compile后，
  Console `error CS` 查询为0；`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入
  04:52:29 +08:00。
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\Tools\\Validate-ChangeLedger.ps1`：PASS（19 records /
  15 governed code files；`LF2Weapon.cs → R4-COL-004B`、`BattleRuntimeSelfCheck.cs → R4-COL-004B`均被覆盖）。
  曾用 Windows PowerShell `powershell.exe` 调用而在 param default的 `$PSScriptRoot` 处失败；该调用方式不符合
  仓库规定的 `pwsh` 命令，未改 validator。
- `git diff --check`：exit 0；只有工作树既有 LF/CRLF warning。
- dormant `ProcessAttack`、C++ runtime trace与真实 landing Play Mode均未关闭；本包保持 `RUNTIME_PENDING`。
