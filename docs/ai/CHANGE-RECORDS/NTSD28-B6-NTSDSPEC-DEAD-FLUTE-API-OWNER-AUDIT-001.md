# NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-NTSDSPEC-DEAD-FLUTE-API-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan impact family has no FluteForce/mass virtual API; Unity LF2Entity base and LF2WeaponBase empty override repo-wide reference audit; EXE B1E13AE1, closure 39DDDA15.
evidence: exactly two declarations and zero repo production/test/serialized callers; actual kind10/11 confirmed independent; old threshold/mass behavior rejected as shared impact owner; two-symbol retirement package defined with architecture guard; runtime blocked; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / DEAD_API_RETIREMENT_PACKAGE_DEFINED / IMPACT_SEPARATE / PRODUCTION_HELD`

`FluteForce()`仅有base旧mass/threshold实现与weapon空override，全repo无调用或序列化引用；actual kind10/11和
Authority都不经过它。后续单包删除两个符号并加防回归guard，不把shared impact塞回virtual shell dispatch。
impact仍由独立三包闭合；runtime阻塞未变，本轮无code。

