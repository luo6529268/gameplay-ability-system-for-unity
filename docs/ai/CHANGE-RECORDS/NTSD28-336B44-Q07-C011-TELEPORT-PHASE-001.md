<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportPlayModeProbeEditor.cs
authority: selected 336B44 formal NTSD 2.8-Logan release playable C011 teleport phase
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001.md
-->

# NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001

Created before script edits. Source, Unity candidate, affected paths, invariants, test-first sequence and rollback are in the [Task](../TASKS/NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001.md). No C011 script has been changed yet. A direct call to the teleport mechanism must remain available; only the canonical complete-tick schedule may be phased. Formal root EXE same-state and original Battle Play remain pending.

Test-first edit: added two parameterized complete-tick cases (state400 enemy and state401 teammate) to the existing native teleport production Editor test class. Both start phase 0, require tick1/phase1 to preserve source X100 and tick2/phase0 to use the authored target-relative X180 or X240. No production code or old C05 Play probe changed yet. Exact original-Editor RED is pending.

Actual original-Editor RED job `129213d57b694bf7a3480c7ed20e38d9` ran both exact state cases and failed 2/2 at tick1/phase1: state400 observed X180 and state401 observed X240, where both should have retained X100. The test reached the complete battle tick and identified the canonical teleport call as the first difference. Production change may now be made at that callsite only.

Production code written: `NTSDBattleTickSystem.NativeTeleport` invokes the existing direct `world.NativeTeleportAll()` only when the already advanced `world.FrameToggle` is zero. The direct world teleport API, state500 special path, frame motion and physics order were not changed. The declared C05 Battle Play probe was rebaselined to check phase-1 skip and phase-0 teleport separately for both state400 and state401 over four full ticks. Original-Editor compile/GREEN and Play remain pending.

Original-Editor compile and exact GREEN job `5789a9a79da1492099806100d0fb57e8` passed 3/3: complete-tick state400, complete-tick state401, and old direct method control. The first rebaselined original Battle Play probe returned `FAIL` after proving state400 phase-1 skip: its phase-0 position expected the old identity viewport X180/Z131, while the current project's shared 2048/1333 spatial projection and stage clamp produced X115/Z238 with correct phase 0, target collision Y -31 and zero velocity. The failure was an obsolete geometry assertion outside C011; Play was exited and the probe's own cleanup passed. The probe was narrowed to C011 scheduling (phase-1 X unchanged; phase-0 X changed with target collision Y and zero motion) without replacing the existing separate geometry tests. Recompile and repeat Play are pending; do not count this first Play as pass.

After recompilation, the second original Battle Scene Play probe passed: four complete ticks 928–931, both state400 and state401 phase-1 skips and phase-0 teleports, collision Y and zero motion, with cleanup passed. Raw JSON SHA `0A797ED394B01C68C6C00AF776FED084327CCEE033A1A96269271CC0B418B447` is saved in the [C011 report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001/REPORT.md). The Editor exited Play, Battle Scene was clean and the four protected file hashes stayed unchanged. Status is `RUNTIME_PENDING` solely because formal-root same-state and natural selected-content integration are not established; Q07 is still open.

Post-edit `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path` passed (1034 records and 81 governed code files in the whole existing dirty diff). Scoped `git -c core.safecrlf=false diff --check` passed for both active packages' owned scripts and the current status docs. Historical validator warnings about old records outside the current diff do not change this package's code coverage.


2026-09-30 C011限定关闭：336B44正式根v3四例12tick/864声明字段PASS；原Battle Scene Lee02与Sakura01各12tick/144字段严格一致，合计288/288、首差0。李tick4 X680/Z401，小樱tick4 X740/Z401；两轮均PASS/DONE/exitedPlay/sceneCleanAfter，MCP确认原Editor idle/nonPlay。两Scene、GameConfig、ProjectBattleModeConfig四SHA保持，DAT与生产脚本未因补证修改。初次Lee01计数字段/地图边界夹具失败和根v1 EOF46原记录保留。该出口证明受控初态后的正式DAT自然帧链与相位，不声称物理键选招、全World/全画面或Q07整组完成。下一G1/C012。

Closure governance checkpoint: Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path exit0/PASS,1056 records/16 governed code files; git -c core.safecrlf=false diff --check exit0. Existing historical record warnings retained. These checks are change tracking/whitespace only, not additional behavior proof.
