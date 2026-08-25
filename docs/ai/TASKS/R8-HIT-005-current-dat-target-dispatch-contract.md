# R8-HIT-005 — current-DAT-first target hit dispatch

> 日期：2026-08-23  
> 状态：`IN_PROGRESS`  
> D-ID：`D-HIT-005`  
> Change ID：`R8-HIT-005-001`

## Goal

建立单一current-DAT-first target hit dispatcher，使Character、shared Character-DAT、Weapon和SpecialAttack
attacker对同一target current DAT选择相同writer；CLR/GameObject壳只承载Unity生命周期，不再裁决战斗类型。

## Scope

- `BattleDamageWriter`增加统一current-DAT target dispatch；把现有weapon/type3 helper从历史CLR参数泛化为
  `LF2Entity`，保持已有字段、顺序和常量；type5使用C++ common non-character kind0 hurt且不执行type3 tail；
- 四个attacker consumer改为调用统一dispatcher；
- exact matching-CLR target继续由统一dispatcher调用既有`LF2Weapon.Hit`/`LF2SpecialAttack.Hit`，mismatch target使用generic typed writer；
- self-check覆盖weapon CLR+current type3、special CLR+current weapon、weapon CLR+current type5及四类attacker入口。

## Authority / Evidence

- C++统一Entity始终从current `char_data->obj_type`选择damage/reaction/tail；
- C++ `collision.cpp:559-632`与`hit.cpp:86-479,631+`不具有CLR subclass gate；
- Unity shared Character-DAT与SpecialAttack attacker路径在type0外按CLR壳优先；其他路径虽type-first，仍要求
  matching CLR壳，导致mismatch时丢失writer；
- Unity已有weapon/type3/common hurt字段实现，本包只统一ownership与victim参数，不重写玩法常量。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterInteractionResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatInteractionResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## Deliverables

1. 统一current-DAT dispatcher；
2. weapon/type3 helper接受generic `LF2Entity` target；
3. type5 common kind0 hurt且无weapon/type3 tail；
4. 四attacker入口与三种shell/current-DAT mismatch矩阵；
5. compile、focused/full self-check、ledger/diff证据。

## Verification

- weapon CLR current type3：HP/stat/type3 frame/hit-confirm变化，weapon durability不变；
- special CLR current weapon：weapon vital/durability/tail变化，不执行type3 tail；
- weapon CLR current type5：common non-character vital/reaction变化，不执行weapon/type3 tail；
- Character/shared/special/weapon attacker均使用同一current type结果；
- existing D-HIT-001～004与kind10/11/14/15/16/raw frame回归通过；
- fresh compile0、full self-check、相关focused tests、validator/diff PASS。

## Stop conditions

- 泛化需要改变C++字段顺序、pass order、candidate/RNG顺序或对象生命周期；
- helper使用CLR专属状态且无法映射到generic runtime字段；
- 回归差异指向D-HIT-001～004之外的新玩法规则；
- 需要修改C++、DAT、render、capacity、T8、IL2CPP或服务器。

## Out of scope

新的damage玩法、D-LIFE-001、F1/F2 debug、candidate collector、render/pool/architecture重构、C++ executable。
