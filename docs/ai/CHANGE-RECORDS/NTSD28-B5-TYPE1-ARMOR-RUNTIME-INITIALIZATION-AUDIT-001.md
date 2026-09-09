# NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001 — runtime profile audit

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-RUNTIME-INITIALIZATION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan battle_world.cpp spawn 1280-1284, recovery 3806-3839, selection/break 7440-7522 and 6803-6808; EXE B1E13AE1, closure 39DDDA15.
evidence: Read-only caller/field audit completed; Unity carrier/snapshot/C25i kernel exist, profile seam false and birth state 0/-1; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`

Authority出生与C25i都读取首armor block；Unity已有runtime fields、snapshot/checksum和已验证C25i kernel，
但profile seam固定false且出生默认0/-1。下一包只接profile与birth init并保护snapshot restore；破甲tail留给
后续atomic hit integration，正式content继续受H/B11门约束。
