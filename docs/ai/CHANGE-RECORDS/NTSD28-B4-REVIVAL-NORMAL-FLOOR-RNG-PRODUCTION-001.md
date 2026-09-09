# NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001
status: VERIFIED
change-kind: BEHAVIOR_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalNormalFloorRngProductionEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::advance_native_revivals normal branch, SimulationTickDriver28 C06 floor handoff, PhysicsIntegrator28 effective floor and synchronized RNG 0x90/0x91; EXE B1E13AE1, closure 39DDDA15.
evidence: Focused RED was 0/7. Production changed only normal revival to sumX-gated type0/group peer averaging, synchronized RNG 0x90/0x33 then 0x91/0x1f with -25/-15, precise-only X/Z writes, C06-equivalent effective floor, exact MP/HP/render/action/Y/Vy tail and no PPBound write. Focused 7/7, combined B4/C25/physics/RNG 59/59, targeted NTSD_Battle Play 7 cases, builds 0 errors (47/104 warnings), Console 0 and Scene unchanged. Full SelfCheck remains blocked earlier by unrelated CPoint mode0 victim-Vz. B4 joint exit trace remains next.
-->

> 状态：`VERIFIED / RED_0_OF_7 / FOCUSED_7_OF_7 / RELATED_59_OF_59 / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXIT_AUDIT_NEXT`

RED0/7后只改normal helper；focused7/7、related59/59、Play7与builds/Console/Scene通过，SelfCheck仍为独立
CPoint阻塞。下一B4 exit trace，不把本包单独冒充整个revival完成。
