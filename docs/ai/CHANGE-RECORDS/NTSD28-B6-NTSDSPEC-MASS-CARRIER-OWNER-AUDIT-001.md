# NTSD28-B6-NTSDSPEC-MASS-CARRIER-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-NTSDSPEC-MASS-CARRIER-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan PhysicsIntegrator28 unconditional grounded X/Z friction and zero mass field in playable closure; Unity CharacterMechanicsContext/LF2Character/character shell snapshot chain; EXE B1E13AE1, closure 39DDDA15.
evidence: sole behavior reader mass>0 and all writers/capture/restore enumerated; non-null old mass IDs are not current/release type0 characters; formal character mass always default1 and output unchanged; generic shell already constant1; latent synthetic/checksum inconsistency identified; schema1-to2 retirement package frozen; runtime blocked; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / CHARACTER_MASS_RETIREMENT_PACKAGE_DEFINED / FORMAL_OUTPUT_UNCHANGED / PRODUCTION_HELD`

Authority grounded friction无mass gate。Unity mass唯一行为reader是`ctx.mass>0`；所有正式type0初始化都得到1，
所以当前/release正式输出不变，但synthetic mass可制造无Authority行为，且该值进入character shell snapshot却不属于
主规则checksum。后续单包删除context/character/snapshot mass并把character-shell schema从1升2，保留原grounded
predicate与unit friction数值。dead Flute lookup另包；runtime阻塞未变，本轮无code。

