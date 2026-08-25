# R4-COL-004B — weapon landing immediate-query removal

<!-- CHANGE-RECORD
id: R4-COL-004B
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\physics.cpp:228-320; J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:577-646,1645-1656,1818-1825; J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:91-129
evidence: SOURCE-CONTRACT-VERIFIED / CODE-WRITTEN / UNITY-COMPILE-PASS-20260822-0452+08 / FULL-SELF-CHECK-PASS-20260822-045229+08 / PLAYMODE-PENDING / CXX-RUNTIME-TRACE-BLOCKED
-->

> 创建日期：2026-08-22  
> 当前状态：`RUNTIME_PENDING` — 最小脚本与focused fixture已通过 Unity compile和full self-check；C++ runtime trace与真实 Play Mode仍未关闭。  
> 所属 Work Package：`R4-COL-04B`。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。

## 1. 目标

删除 Unity `LF2Weapon.OnLanded()` active state13/high-speed landing的额外即时 target hit；保留正式
candidate/consume系统及武器自身落地行为。

## 2. 可修改范围

- `LF2Weapon.cs`：只限 `OnLanded()` 的 landing splash query / direct target writer子块；
- `BattleRuntimeSelfCheck.cs`：只限本记录的 landing fixture与必要 test probe；
- 与本记录直接对应的治理文档。

## 3. 不可修改范围

`BruteForceSceneQuery`、`IsPureTransitionSmoke`、`ProcessAttack` / `ProcessAttackInternal`、candidate collect、
kind5、CPoint、held/link、opoint、scheduler、DAT/资源、render、pool/容量、C++ authority及所有R4-COL-005+。

## 4. 实际代码写入

- `LF2Weapon.OnLanded()` state13/high-speed branch不再创建 landing splash ITR、不再遍历自身BDY、
  不再调用 `QueryBodyHits`、`LF2Character.Hit` 或 `LF2CharacterDatHitResolver.TryResolveHit`；
- weapon自身 `Health.HP`、`Runtime.Y/Vy/Vz/Vx`、clamp和return保持原有分支；
- `BattleRuntimeSelfCheck`新增可被旧即时查询看到的重叠 target fixture，调用 test-only
  `InvokeOnLandedForSelfCheck()` 后断言 target HP不变，而weapon仍有C++对应的本地落地字段变化；
- `ProcessAttack` / `ProcessAttackInternal`、helper、candidate collect和其他 scope外路径未改。

## 5. Authority / 差异依据

- **C++ VERIFIED**：release `physics.cpp:228-320` weapon landing只写自身；正式命中由
  `game_tick.cpp:1645-1656,1818-1825` 的 collect / loop consume处理；held attack在
  `collision.cpp:91-129` 的 kind5 transform进入同一通用链。
- **Unity VERIFIED**：native weapon frame advance确实可调用 `OnLanded()`，该分支当前直接扫描 target并
  调 `Hit`。
- **C++ trace BLOCKED**：R1-WP02仍未解除；本记录不运行或写入 C++ authority。

## 6. 验收与回滚

- 验收：`TASKS/R4-COL-04B-immediate-landing-query-contract.md` 的 S0～S4；
- 最高状态：`RUNTIME_PENDING`；
- 回滚范围：仅本记录列出的两份脚本与关联文档；未提交；
- 若需要触及任何不可修改范围，立即停止并新建包。

## 7. 实际验证

| 检查 | 实际结果 |
|---|---|
| C++ authority | 只读复核 `Makefile:11-35`、`physics.cpp:228-320`、`game_tick.cpp:577-646,1645-1656,1818-1825`、`weapon.cpp:109-128`和`collision.cpp:91-129`；未运行、构建、修改或写入 authority。 |
| focused fixture | overlap precondition确认旧 immediate query可见 target；新landing call不改 target HP，仍正确写weapon自身 -100 HP / `Y=0` / `Vy=-3.5`。 |
| Unity compile | 2026-08-22 04:52 +08:00，现有 Unity 2022.3.62f3 / UnityMCP port 6401 refresh后，Console `error CS` 查询为0。 |
| Full self-check | `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 04:52:29 +08:00。 |
| Change Ledger | `pwsh -NoProfile -ExecutionPolicy Bypass -File .\\Tools\\Validate-ChangeLedger.ps1` PASS：19 records / 15 governed code files，两个本包脚本均由 `R4-COL-004B` 覆盖。一次 Windows PowerShell `powershell.exe` 调用在 `$PSScriptRoot` param default失败；仓库合同规定并最终使用的是 `pwsh`，未修改validator。 |
| Diff hygiene | `git diff --check` exit 0；只有既有 LF/CRLF warning。 |
| Play Mode / C++ trace | 未执行；前者仍待真实场景，后者受 `R1-WP02=BLOCKED` 限制。 |

本结果只关闭本记录的 source、code、compile和self-check层，不得写成完整weapon/held/R4或全战斗已对齐。
