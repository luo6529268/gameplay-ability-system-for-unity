# NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001 — B4 frame-motion entry audit

<!-- CHANGE-RECORD
id: NTSD28-B4-ENTRY-FRAME-MOTION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan frame_motion.cpp FrameMotion28::apply and input_routing.cpp depth_intent producer; EXE B1E13AE1, closure 39DDDA15.
evidence: XYZ-FORMULA-MATRIX-CLOSED / FIRST-DIFFERENCE-BOTH-DEPTH-KEYS / NEXT-F02-KERNEL / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIRST_DIFFERENCE_DEPTH_INTENT`

Authority `up != down`才产生negative/positive intent；Unity cooldown tie-break是confirmed behavior difference。下一用纯kernel与production双按fixture闭合。
