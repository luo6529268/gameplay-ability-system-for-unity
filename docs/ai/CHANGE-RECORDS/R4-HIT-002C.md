# R4-HIT-002C — normal weapon victim raw-frame writer

<!-- CHANGE-RECORD
id: R4-HIT-002C
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:583-632
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / RUNTIME-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — 五处writer与五分支fixture已通过Unity compile/full self-check；C++ trace与Play Mode未关闭。

## 范围

仅覆盖normal kind0 weapon victim的type1、type4/type6、type2 raw frame。C++先在knockback处理raw-write
180/186，随后weapon tail raw-write final frame；Unity当前对应`ApplyWeaponDamage`与tail合计五处
`SetFrameDirect`，都会额外清attacking并重同步wait。现已以raw writer最小替换五处，并新增真实
`LF2Weapon.Hit → ApplyWeaponDamage → ApplyKind0WeaponVictimTail → RecordKind0Hit`夹具，锁定PN、attacking、
wait、final frame、hit-confirm、relation、self-vrest及总RNG call count。

禁止改global frame helper、weapon attacker、vital/stat、vrest/held算法、RNG engine、candidate、scheduler、
input、AI、render、DAT、C++ authority。所有改动后必须同步完整审计与实际Unity验证。

## 计划代码路径

| 文件 | 方法 | 目标 |
|---|---|---|
| `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs` | `ApplyWeaponDamage` + `ApplyKind0WeaponVictimTail` | 一处knockdown + 四处tail raw frame callsite最小替换 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 新/邻近weapon-victim fixture | type1、type4、type6、type2-ground、type2-air的PN/attacking/wait/RNG合同 |

## 验收状态

- C++ source：`PASS`（只读）；
- Unity code：`CODE_WRITTEN`；五个fixture分支为type1、type4、type6、type2-ground、type2-air。type1/4/6与
  type2-air各应保持一笔final-frame RNG，type2-ground应保持零笔final-frame RNG；每个accepted hit另保留既有两笔
  hit-record RNG；
- Unity compile：`PASS`，UnityMCP刷新后`error CS`=0；
- full self-check：`PASS`，`Temp/NTSD_BattleRuntimeSelfCheck.result`为`PASS`（2026-08-22 06:20:15 +08:00）；
  其后Console仅有两个既有rest-binding negative-control日志，未见C# compiler error或02C fixture失败；
- Play Mode：`PENDING`；
- C++ runtime trace：`BLOCKED`（R1-WP02）。
