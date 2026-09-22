# NTSD28-R15-FORMAL-PROBE-V2-VECTOR-001

Status: CODE_WRITTEN / VECTOR_RECHECK_PASS / UNITY_COMPILE_PENDING. Parent: BATCH-04/Q07 and R15. Formal/staged mode parent and child DAT are now part of the published five-component `NTSD28_LOGAN_BATTLE_INPUTS_V2` identity. Current formal/staged semantic SHA-256 is `FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB`, projection `9F40EB3FFF1812FF`, confirmed by the independent pure-source check. Ten literal uses of the former formal V1 semantic/projection remain in nine battle validation scripts; they would fail before their actual scenario assertions.

Exact script ownership:

- `Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07CloneCentralPixelProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q08FormalSourcePixelPlayModeTests.cs`
- `Assets/NTSD/Scripts/Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs`
- `Assets/NTSD/Scripts/Test/NTSD28Q07WindowsNaturalSkillProbe.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06SpawnVitalsEditorTests.cs`

Only the exact formal-root V1 fingerprint/projection literals become V2 values. The SpawnVitals evidence JSON `catalog` literal must match its preflight assertion. Preserve legacy/no-mode synthetic V1 tests, all gameplay/physical-input assertions, roots, scene/probe wiring and output schema. No production runtime, Scene, Prefab, resource, nonbattle code or third-party change. Native V2 capture and Q07 Unity runtime acceptance remain separate.

Acceptance: inspect all ten occurrences before/after; independent formal/staged V2 digest recheck; source diff only literal values; `git diff --check` and ChangeLedger validator. Original-project Unity compile and the affected focused tests/probes must run before any runtime status is claimed. If original Editor assemblies remain stale, status stays CODE_WRITTEN. Rollback by reviewing/reversing only these nine literal-only script diffs, preserving all user work and earlier Q07/R15 changes.
