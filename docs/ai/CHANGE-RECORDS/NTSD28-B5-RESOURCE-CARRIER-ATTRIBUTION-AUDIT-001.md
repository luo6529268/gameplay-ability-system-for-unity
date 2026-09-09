# NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001 — carrier and attribution audit

<!-- CHANGE-RECORD
id: NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan resolve_native_hit_resource_attacker, EntityState28 suppression, NativeHitResourceRules28 and GameSession28 projection; EXE B1E13AE1, closure 39DDDA15.
evidence: TWO-OWNER-HOPS-READ / MISSING-NEXT-FAILS / OWNER-SLOT-B0-BINDING-VERIFIED / SUPPRESSION-CARRIER-MISSING / WORLD-RULE-DEFAULTS-AND-F6-PROJECTION-READ / STATS-ATTACKING-MODEL-MISSING / BASEMAX-B11-DIFF-PRESERVED / NO-CODE-CONTENT-SCENE-AUTHORITY-CHANGE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_ATTRIBUTION_SPLIT_DEFINED`

Resource attacker固定使用`OwnerSlotIndex`两跳且missing next失败；suppression缺carrier；F6已有session
state但缺world transaction投影；stats.attacking与baseMax仍受B11/H约束。下一先建suppression carrier，
再实现resource-attacker resolver。本包无脚本、Config、Scene或Authority修改。
