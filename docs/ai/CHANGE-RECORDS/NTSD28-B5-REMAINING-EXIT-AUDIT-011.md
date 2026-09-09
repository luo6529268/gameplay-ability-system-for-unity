# NTSD28-B5-REMAINING-EXIT-AUDIT-011

<!-- CHANGE-RECORD
id: NTSD28-B5-REMAINING-EXIT-AUDIT-011
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28 confirmed hit tails after reduced state2000 away damping closure; EXE B1E13AE1, closure 39DDDA15.
evidence: read-only audit confirmed formal combo record bound1/facing1/respond50/caughtact1 and playable mapping; Unity lacks dedicated count/last-tick, applied-hit/caughtact producers and actual CoreComboExpire despite pass descriptor; next owner audit routed; no behavior writes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NATIVE_COMBO_RUNTIME_ROUTED / B5_EXIT_NOT_READY`

从Authority reduced state2000分支之后继续逐语句复核reduced tail、unarmored/non-character continuation、
hit-record与命中后生命周期边界，并反查Unity actual/HitPlan和既有B5 records。当前不预设结论，不修改
任何脚本、内容或Scene。

## 审计结论

- Authority `simulation_tick_driver.cpp`：每个applied hit后调用ordinary producer；仅target type0参与，
  `facing==1`选择attacker，否则target；非type0 source只做一次`owner_slot(+0x354)`解引用；count加1并把
  last tick写为当前process tick。catch settlement后，`caughtact==1`对真实state9 injury event复用同一producer。
- C25全部live-slot尾结束后执行expiry：record缺失不执行、negative respond不执行，且
  `last+respond<=battle_tick`时清count；未来render只消费这些字段，属于B10后续。
- 正式playable每tick从`native_combo_hud`映射options；正式decoded `data/mode/ntsd.dat`为
  `bound=1/facing=1/respond=50/caughtact=1`，因此ordinary producer和expiry正式可达。
- Unity没有`combo_hit_count_1e0/combo_hit_last_tick_1e4`等价字段；`ComboCountAtk/Vic`是伤害累计，
  `ComboD*`是输入组合键状态，都不得复用。`CoreComboExpire`只出现在`NTSD28BattlePassOrder`，
  `NTSDBattleTickSystem`没有对应调用；也未找到selected-mode combo world carrier或命中生产者。
- 下一包：`NTSD28-B5-NATIVE-COMBO-RUNTIME-OWNER-AUDIT-001`；应拆分mode/entity carrier、ordinary producer、
  C25后expiry，以及依赖B6 catch settlement的caughtact producer。HUD/图集/命中数字命令保持B10。
