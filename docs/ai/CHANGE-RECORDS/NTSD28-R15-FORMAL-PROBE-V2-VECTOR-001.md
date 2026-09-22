<!-- CHANGE-RECORD
id: NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07CloneCentralPixelProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08FormalSourcePixelPlayModeTests.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07WindowsNaturalSkillProbe.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06SpawnVitalsEditorTests.cs
authority: formal Logan selected mode input and Q07 five-component V2 content identity
evidence: artifacts/diagnostics/NTSD28-Q07-MODE-COMBO-PUBLISHED-ACTIVATION-001/pure-check.log
-->

# NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001

Created before script modification. Nine battle probe/test scripts retain ten hard-coded formal V1 semantic/projection values. They are no longer the identity of current formal/staged mode content, so preflight would fail before exercising skill, publication, pixels or birth values. This change is limited to exact expected current formal-root identity literals; no input sequence, assertion logic, production behavior or output schema is changed. The Task Contract declares exact paths, risk, acceptance and rollback. The V2 vector has been rechecked from the actual formal and staged DATs using the pure-source check. Original Unity Editor compilation/probe runs are pending; old evidence remains historical V1.

Actual: seven semantic literals became `FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB`; three projection literals became `9F40EB3FFF1812FF` (including SpawnVitals' output `catalog` field). The nine declared script files have exactly 1/1 or 2/2 changed lines by `git diff --numstat`; a scoped search finds no remaining old formal V1 semantic/projection literal in `Assets/NTSD/Scripts`. No production/gameplay script, Scene, resource, input sequence or result schema was changed in this package.

Validation: actual pure-source formal/staged V2 check reran successfully; output is `artifacts/diagnostics/NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001/vector-recheck.log`. This is an external pure-code/data check, not original Unity compilation. Original Editor assemblies remain stale; all nine affected Editor/Player test paths, same-seed native trace and actual battle behavior remain unrun for this package. Status `CODE_WRITTEN / VECTOR_RECHECK_PASS / UNITY_COMPILE_PENDING`.

Final scoped checks for this edit: `git diff --numstat` showed seven one-line semantic changes, one one-line projection change and one two-line projection change (10 literal-bearing lines total); `rg` found zero old fixed V1 semantic/projection strings in `Assets/NTSD/Scripts/*.cs`; `git diff --check` exited 0. `Tools/Validate-ChangeLedger.ps1` passed with 683 Records and 20 governed code files covered in the shared worktree, full validator output at `artifacts/diagnostics/NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001/ledger-validation.log`. No original-project Unity script assembly timestamp changed; both remain at 2026-09-22 04:23:43/44 UTC. Thus focused runtime checks are pending rather than passed.
