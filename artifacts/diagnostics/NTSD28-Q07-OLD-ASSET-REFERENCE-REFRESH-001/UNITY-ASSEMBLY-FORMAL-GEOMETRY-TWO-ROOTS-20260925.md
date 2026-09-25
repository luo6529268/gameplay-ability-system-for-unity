# Q07 formal geometry through current Unity assemblies, 2026-09-25

Status: `OFFLINE_UNITY_ASSEMBLY_MEASURED / ORIGINAL_EDITOR_FOCUSED_TEST_PENDING / NO_SCRIPT_OR_DAT_EDIT`. The machine-readable paired result is `UNITY-ASSEMBLY-FORMAL-GEOMETRY-TWO-ROOTS-20260925.json` (SHA-256 `E8DB88F4B8E7F1173648492663B13A4A420CBAF8B620815D5D9050A6FA958E9D`). This measurement uses current `Library/ScriptAssemblies/Assembly-CSharp.dll` SHA-256 `5EF4A473DF1C99FCDF929E01349C3008C5CBA8C0508BB538E8AA0FDDDE266184`, not a Python reimplementation of the parser. It ran in a separate PowerShell/.NET process, not in the original Unity Editor. No source, asset, DAT, Scene or Editor state was modified.

The diagnostic loaded `UnityEngine.CoreModule.dll`, disabled only that external process's `UnityEngine.Debug.unityLogger.logEnabled`, and invoked the actual `BattleContentSource.ForLoganRuntime`, `LoganObjectCatalog.Read`, `CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog`, `LF2FrameCache.Load`, `GetFrameDataById` and `HasFrame` methods by reflection. A fresh default `ProjectBattleModeConfig.Snapshot` was passed to both roots to avoid the diagnostic no-snapshot native mode-DAT fallback. This is **not** the serialized project Asset snapshot; mode values are not part of the geometry conversion measured here. An earlier run without logging suppression failed at `Lf2DatConverter.ConvertFrameDataCore`'s `Debug.Log` ECall outside Unity; with logging suppressed both full roots converted all 330 objects.

| Current Unity converter/cache measure | Formal runtime | Staged Unity runtime |
| --- | ---: | ---: |
| Converted objects | 330 | 330 |
| Authored frame entries and unique IDs in 0..856 | 55,348 | 55,348 |
| Authored frame IDs outside 0..856 | 0 | 0 |
| `GetFrameDataById` visits over 330 × 857 slots | 282,810 | 282,810 |
| Resolved itr / bdy entries | 19,461 / 86,383 | 19,461 / 86,383 |
| Nonpositive itr / bdy | 109 / 0 | 109 / 0 |
| Zero-width, positive-height itr | 27 | 27 |
| Itr with `hasGeometry=false` | 82 | 82 |
| Positive-width, negative-height bdy | 0 | 0 |

A second focused pass over the same current Unity conversion path classified the 109 nonpositive itr entries in the staged root exactly: **27** `kind=0, w=0, h=79, hasGeometry=true`; **82** `kind=100100, w=0, h=0, hasGeometry=false`. The current formal C++ `CollisionGeometry28::decode` treats kind-100100 records without complete geometry as control-only, with no projected rectangle; `collision_geometry_tests.cpp` checks this, and `itr_dispatch_corpus_tests.cpp` locks 82 kind-100100 records in the formal corpus. This source authority supports treating the 82 as a separate control-record class in the SelfCheck. It does not by itself prove every Unity hit-consumer route handles them identically; keep that focused behavior check.

The existing `BattleRuntimeSelfCheck.CheckDeployableResolvedGeometryRisks` expects five old positive-width/negative-height bdy and zero other nonpositive itr. Those content assertions contradict the measured formal/staged Unity data: bdy count is zero and the other 82 are control-only records. Preserve its synthetic `CheckZeroDimensionCollisionGeometry` and `CheckNegativeHeightCollisionGeometry` rule tests. A subsequent scoped script change should select the formal catalog and assert explicit formal geometry/control classes, then run a focused original-Editor test and full SelfCheck after the separately owned legacy DAT fixture readers are migrated. The aggregate SelfCheck exit remains open.
