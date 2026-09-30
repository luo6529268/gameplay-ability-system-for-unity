<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C017-POISON-SCENE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07PoisonZeroHpBattlePlayProbeEditor.cs
authority: selected formal 336B44 root natural Kankuro/Pur poison input trace and current playable source
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C017-POISON-SCENE-PLAY-001.md
-->

# NTSD28-336B44-Q07-C017-POISON-SCENE-PLAY-001

Created before code. Only a new original-Editor Battle Play diagnostic; all setup remains in Play clone. It follows the already checked request/pause/complete Driver/exit pattern. It will observe DAT-delivered poison and zero-HP input, not seed poison or implement a special role rule. Scope, invariants, timing, risks, acceptance and rollback are in the Task. Root evidence does not substitute for Unity Play.

Actual code is the one declared Editor-only probe, following the existing Guren CAG request/lifecycle carrier. Changed diagnostic responsibilities: roster14/2, controlledHP500/35, native seed and phase0,40 complete Driver ticks, target external held Attack only after natural HP0/idle, poison and input samples. Original Editor Assets/Refresh at17:50 compiled the new Assembly-CSharp-Editor.dll; Editor.log Tundra build success4.54s, existing warnings only. Domain reload returned ready17:53:37. No generated-project substitute or second Editor. Scene request is separate, runtime outcome pending.

Final unique poison-zero-hp-scene-01 PASS/DONE: original Scene clone initialized, stableglobaltick5→45, relative6Pur birth/7contact/19HP0/28sampledAttack65. Source/Unity40tick8fields320/320, additional5fields200/200 no difference. Formal root same8fields320/320/exit0; poison threefields are source/Unity only. Editor exitedPlay/sceneCleanAftertrue/MCPidle. Protected Scene/config hashes unchanged. Earlier MCP statusread timedout during startup; not a failed/repeated Play. Evidence artifacts/diagnostics/NTSD28-336B44-Q07-C017-POISON-SCENE-PLAY-001/REPORT.md. VERIFIED applies only to this scoped naturalcontact→HP0→input witness; no physicalkey/fullWorld claim or production change.

Final sharedTools/Validate-ChangeLedger.ps1 PASS1054 records/14 governedcodefiles; git -c core.safecrlf=false diff --check exit0. protected-hashes-after.json independently compares allfour Scene/config hashes equal. No fullUnitysuite rerun; newprobe actualEditorcompile and unique targetedPlay complete the package's evidence.
