# NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan playable C25 pre-display negative EnvironmentState320 resource transaction; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority uses EnvironmentState320<0, native phase12, rule +0x90/default9, 900/IncomingDamageScale340, two-hop credit, exact HP/score/KO counters and no clamp/ComboVic. Both Unity recovery paths instead use WeaponCount/FallDamageDiv/step-wait/clamp/ComboVic. Flute WeaponCount=-20 makes the false-positive live. Existing exact carriers suffice except world rule +0x90; data-carrier then shared-transaction packages defined. No code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / WEAPONCOUNT_WRONG_CARRIER / EXACT_TRANSACTION_REQUIRED / TWO_PACKAGE_ROUTE`

> **CORRECTED BY `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001`：** 原审计的
> “no clamp”一句错误；当前无漂移 playable source及其tests要求exact accounting后HP/HPBound clamp到0。
> 其他owner/trigger/scale/counter结论不变。

完整证据、差异与后继顺序见同ID Task Contract。
