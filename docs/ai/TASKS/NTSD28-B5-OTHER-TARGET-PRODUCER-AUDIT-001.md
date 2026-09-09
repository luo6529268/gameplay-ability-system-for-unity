# Task Contract — NTSD28-B5-OTHER-TARGET-PRODUCER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / OTHER_TARGET_SPLIT_DEFINED`

## 目标

只读闭合 type 1/2/3/4/5/6 在 confirmed unarmored standard-hit 中对 encoded status、
join/mimic side effect 与 hit-motion arm 的真实共享范围，并给出可独立验证的后续实施包。

## Authority 结论

| Target type | damage/resource/status tail | join side effect | mimic activation | hit-motion arm |
|---|---|---|---|---|
| 0 | 是 | 是 | attacker也为0时是 | 是 |
| 1/2/4 | 是 | 是 | 否 | 是 |
| 3 | 是 | 是 | 否 | 是 |
| 5 | 是 | 是 | 否 | 是 |
| 6 | **跳过** | **跳过** | 否 | 是 |

依据：`battle_world.cpp` 中 `skips_native_damage_tail = target->object_type == 6` 只包围
damage/resource、`apply_confirmed_input_statuses`、join/mimic和weapon-strength self-cost；
`arm_native_unarmored_hit_motion` 位于该条件块之外，因此type 6仍执行arm。

## Unity 当前映射

- type 1/2/4/6：`BattleDamageWriter.ApplyWeaponDamage`。已有type6跳过normal vital、共同weapon
  counter/reaction/horizontal/vertical的大体骨架；缺type1/2/4 status+join，且全部缺arm。
- type 3/5：`ApplySpecialAttackDamage -> ApplySpecialObjectHurtTail`。已有normal vital、reaction、
  horizontal/vertical和type3专用tail；缺status+join与arm。
- mimic可复用已验证的`ApplyNativeJoinAndMimicSideEffects`；其type gate会使非type0不激活，
  join仍按timer工作。

## 实施拆分

1. `NTSD28-B5-NONTYPE0-ENCODED-JOIN-001`：只给type1/2/3/4/5接encoded status与join，
   明确type6 zero-RNG/zero-status/zero-join。
2. `NTSD28-B5-NONTYPE0-HIT-MOTION-ARM-001`：给type1/2/3/4/5/6在reaction之后、horizontal
   response之前接arm；type6必须覆盖。
3. resource transfer、weapon-strength self-cost、armor选择/破坏、effect tail等其余B5差异继续
   独立审计/实施，不并入上述producer包。

## 不变量

- 不因共享helper而让type6消费status RNG或触发join。
- 不把type3/5专用post-hit continuation合并进producer包。
- 不改Config/DAT/Scene/Authority/资源，不宣称完整B5或完整hit已对齐。

## 验证

Authority source、playable build closure、Unity四个owner方法与现有carrier/lifecycle静态闭合；
本包无脚本、Config、Scene或Authority修改。
