# R4-HIT-01 — type3 normal kind0 vital/stat preflight

> 日期：2026-08-22  
> 类型：C++ release source preflight + Unity typed-writer crosswalk。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 关联差异：`D-HIT-001`。  

## 1. C++ contract（VERIFIED）

`Makefile:11-35` 将 `collision.cpp`、`hit.cpp`编入正式 release。normal kind0 的类型和顺序为：

1. `collision.cpp:561-585` 仅把 target type6分流至 `apply_hurt_reaction`；type3继续调用
   `apply_hurt`；
2. `hit.cpp:631-636` 令 `apply_hurt` 进入 `apply_hurt_impl(..., true)`；
3. `hit.cpp:104-155` 先做 normal vital/stat writes：HP、HP max、victim combo和global damage stat；
4. kill attribution、global kill stat与holder combo只在 `vic_dat_type == 0` 的支路发生；
5. `hit.cpp:162-488` 完成normal fall/motion/tail，`collision.cpp:644-918`随后处理type3专有
   identity/motion tail。

所以 type3 target的 required subset 是：

| 字段 | C++ normal type3 rule |
|---|---|
| HP | `-= injury`，不在此处钳制。 |
| HP max | `-= injury / 3`，C#对应为`Health.HPBound`。 |
| victim combo | `+= injury`。 |
| global damage stat | valid `unk_344` index时 `+= injury`。 |
| holder kill / combo、global kill stat | **不写**；它们是type0-only。 |

## 2. Unity source finding（VERIFIED）

`BattleDamageWriter.ApplySpecialAttackDamage:322-369` 原来的kind0路径只做sound和type3
motion/effect tail。它没有写上述四个公共字段；而`ApplySpecialObjectHurtTail:535-543`已经读取
`Health.HP`决定fall，因此遗漏不仅影响统计，也改变同 tick tail分支。

Unity 现有`ApplyStandardVitalAndStatWrites:490-523`不能直接复用：其为type0 normal route补了
holder kill、global kill和holder attack combo。把它调用到type3会形成新的错误。

## 3. 最小实现（CODE WRITTEN）

`BattleDamageWriter`新增`ApplyType3NormalVitalAndStatWrites`，且只写：

```text
HP -= injury
HPBound -= injury / 3
ComboCountVic += injury
DamageStats[Unk344] += injury   (valid index only)
```

`ApplySpecialAttackDamage`在kind0 sound后、`ApplySpecialObjectHurtTail`前调用它。未改candidate、
ITR、RNG、score helper、motion tail、CPoint、held/link、input、render或C++ authority。

## 4. Focused acceptance（FOCUSED TEST PASS）

`BattleRuntimeSelfCheck.CheckType3SpecialAttackNormalVitalAndStatWrites`从真实
`LF2SpecialAttack.Hit → ApplySpecialAttackDamage`入口构造一个lethal type3 target：

- injury=10、HP=8、HPBound=30、ComboCountVic=4、DamageStats[1]=29；
- 断言结果是HP=-2、HPBound=27、ComboCountVic=14、DamageStats[1]=39；
- 同时设置`KillCount=-1`与有效holder，断言holder KillStat/ComboCountAtk、world KillStats均不变；
- 断言type3 tail读到已更新HP，使fall累积到100，确认写入顺序而非只验证最终字段。

实际验证：Unity 2022.3.62f3 / UnityMCP port 6401 scripts refresh后Console `error CS`=0；
`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入**2026-08-22 05:26:41 +08:00**。

## 5. 未关闭项

- 未运行、构建、修改、复制或写入C++ authority；C++ runtime trace仍为`R1-WP02=BLOCKED`；
- 未做真实 Play Mode、type3 death/lifecycle / Karasu identity joint scenario；
- helper本身没有new collection/object，但本包未单独执行GC allocation profile；
- type1/2/4 common vital/stat、kind10/11/16 raw frame仍为独立D-HIT-002/003。
