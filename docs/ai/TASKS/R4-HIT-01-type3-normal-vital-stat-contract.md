# R4-HIT-01 — type3 normal kind0 vital/stat writes

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — source、最小脚本、Unity compile与full self-check已通过；C++ trace / Play Mode待补。  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-HIT-001`。  
> 前置调查：`RESEARCH/R1-SOURCE-004-cpp-collision-hit-contract.md`、`RESEARCH/R1-SOURCE-004-unity-collision-crosswalk-and-diff.md`。  

## Goal

让 current DAT type3（`LF2SpecialAttack`）作为 kind0 normal-hit **target** 时，在既有 type3
motion/lifecycle tail前得到 C++ normal `apply_hurt` 的公共 vital/stat 写入：HP、HPBound、
ComboCountVic 与 `world.DamageStats`。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs`
   - `ApplySpecialAttackDamage` 的 `kind == 0` 分支；
   - 一个 type3 专用的最小 vital/stat helper，写入顺序在 `ApplySpecialObjectHurtTail` 之前。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - type3 normal-hit vital/stat 正例；
   - lethal type3 control，证明不会错误写 character-only `KillStat` / `KillStats` / holder
     `ComboCountAtk`。
3. 与该 Change ID 关联的 ledger、STATE、差异登记、主计划与 handoff。

禁止：

- 改动 candidate collect/consume、kind9、weapon type1/2/4、type6 reaction、kind10/11/16、
  CPoint、held/link、opoint、scheduler、input、AI、render、DAT/资源、scene、C++ authority；
- 改动或泛化 `ApplyStandardVitalAndStatWrites` 的 type0 score contract；
- 将本包扩展为 death/lifecycle、Karasu identity、raw-frame 或 R5 工作包。

## Authority / Evidence

### VERIFIED — C++ release source

- `collision.cpp:561-585`：kind0 target 除 type6 reaction-only 分流外进入 `apply_hurt`；
  type3 target没有在该 callsite被排除；
- `hit.cpp:631-636`：`apply_hurt` 调用 `apply_hurt_impl(..., true)`；
- `hit.cpp:104-155`：normal damage 先按 `fall_damage_div` 调整 injury，随后写
  `HP -= injury`、`HP max -= injury/3`、`combo_count_vic += injury`，并在有效 `unk_344`
  index写 `g_damage_stats += injury`；
- 同一段只有 `vic_dat_type == 0` 才写 kill attribution / `g_kill_stats` 与 holder
  `combo_count_atk`（`119-151`）。因此 type3不能复用会写这些 character-only score 的 Unity helper；
- `hit.cpp:162-488` 的 normal fall/motion/tail 在上述 vital/stat 写入之后执行；
  `collision.cpp:644-918` 再追加 type3 identity/motion tail。

### VERIFIED — Unity current source

- `BattleDamageWriter.ApplySpecialAttackDamage:322-369` 的kind0分支仅执行
  `RecordDamageEffectSound → ApplySpecialObjectHurtTail → ApplyKind0Type3Tail`；
- `ApplySpecialObjectHurtTail:535-543` 会读取当前 `Health.HP` 决定 fall/death reaction，故 vital
  write必须在该 tail之前；
- `ApplyStandardVitalAndStatWrites:490-523` 已正确服务 character type0，但会附加 kill / holder
  combo写入，不能直接用于type3。

### UNKNOWN / excluded

- C++ runtime trace与真实 Play Mode仍未执行；
- type3 death、Karasu identity replacement、late lifecycle consumer和 hit record/render可见性不在本包验收内；
- `D-HIT-003` type1/2/4与本包共享部分字段，但保持独立，不得一次性泛化。

## Required behavior

在 `ApplySpecialAttackDamage` 的kind0 branch：

1. 保留现有 null/kind guards与sound调用；
2. 先写 type3 common fields：
   - `victim.Health.HP -= itr.injury`；
   - `victim.Health.HPBound -= itr.injury / 3`；
   - `victim.ComboCountVic += itr.injury`；
   - valid `victim.Unk344` 时 `world.DamageStats[index] += itr.injury`；
3. 不写 `holder.KillStat`、`world.KillStats`、`holder.ComboCountAtk`；
4. 随后保持原顺序执行 `ApplySpecialObjectHurtTail`、`ApplyKind0Type3Tail`、`RecordKind0Hit`；
5. 不分配新集合/对象，不改变 RNG、candidate 或 ITR authored data。

## Deliverables

1. type3-special normal kind0 typed writer补齐上述四类字段；
2. focused self-check覆盖普通与lethal type3场景及 character-only score negative control；
3. Unity compile、full self-check、ledger validator与`git diff --check`的实际结果；
4. 完整 Change Record、STATE、差异登记、主计划与handoff；最高状态不超过 `RUNTIME_PENDING`。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 | C++ callsite、`apply_hurt(..., true)`、type3后续 tail和Unity typed writer顺序复核。 |
| S1 | positive type3 target：HP、HPBound、ComboCountVic、DamageStats按injury精确变化。 |
| S2 | lethal type3 target：上述公共字段仍改变；KillStat、KillStats、holder ComboCountAtk均保持不变。 |
| S3 | self-check仍证明motion/tail可以在写后HP上运行；无 authored ITR变异或RNG追加。 |
| S4 | Unity scripts compile=0 error、full self-check PASS、`pwsh` ledger validator和`git diff --check`通过。 |
| S5 | 仅提升至`RUNTIME_PENDING`；C++ trace、Play Mode与type3 lifecycle联动保持未关闭。 |

## Stop conditions

- source显示 type3实际从normal path跳过 vital write，或同一字段由未读 live writer在同 tick 覆盖；
- 需要改动 `ApplyStandardVitalAndStatWrites`、weapon writer、candidate/pass order或type3 lifecycle才能使fixture通过；
- 需要DAT/C++/scene/resource改动；
- focused fixture揭示 score fields并非当前 `Health`/world array 的同义映射。

## Out of scope

`D-HIT-002`、`D-HIT-003`、`D-COL-005B`、R5～R8、T8、C++ executable、Unity Play Mode、服务器、Android、
性能与render。

## 实施进度（2026-08-22）

- `BattleDamageWriter.ApplySpecialAttackDamage` 的kind0 branch现于sound后、special tail前调用
  `ApplyType3NormalVitalAndStatWrites`；该helper只写HP、HPBound、ComboCountVic与DamageStats；
- `BattleRuntimeSelfCheck`新增lethal type3 direct-`Hit` fixture，断言四个字段、tail read-after-write的
  `FallCounter=100`，以及type0-only holder/global score保持不变；
- 现有Unity 2022.3.62f3 / UnityMCP port 6401 refresh后Console `error CS`=0；full self-check结果
  `Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入2026-08-22 05:26:41 +08:00；
- C++ trace、真实Play Mode、type3 lifecycle/identity联合验收仍未取得，故不得超过`RUNTIME_PENDING`。
