# NTSD28-B4-F04-TYPE0-PHYSICS-CORE-AUDIT-001 — type0 physics core audit

<!-- CHANGE-RECORD
id: NTSD28-B4-F04-TYPE0-PHYSICS-CORE-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan PhysicsIntegrator28::step type0 common integration and floor crossing; EXE B1E13AE1, closure 39DDDA15.
evidence: TYPE0-CORE-SEAM-DEFINED / FIRST-DIFFERENCE-ZERO-FLOOR-ASSUMPTION / NEXT-TEST-FIRST / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TYPE0_CORE_SEAM_DEFINED`

Unity `CharacterMechanics`已经封装type0与current-character-DAT共享路径，因此可先在该owner中
消费新carrier，且默认0时保持原正常行为。strict previous-floor crossing也会关闭“已在floor但
Vy>0重复触发landing”的差异。type0 action selection和所有非角色对象明确排除。

