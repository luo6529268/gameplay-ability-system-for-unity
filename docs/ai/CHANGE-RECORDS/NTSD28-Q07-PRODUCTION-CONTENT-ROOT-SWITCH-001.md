<!-- CHANGE-RECORD
id: NTSD28-Q07-PRODUCTION-CONTENT-ROOT-SWITCH-001
status: VERIFIED
change-kind: BATTLE_CONTENT_CONFIGURATION_AND_TEST
code-path: Assets/NTSD/Scripts/Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs
authority: D-023 formal DAT and character images; Q07 staged candidate and Player content bootstrap certificates
evidence: q07-serialized-build-1 succeeded; serialized-root Player exit 0 and terminal PASS; production GameConfig one-line diff
-->

# NTSD28-Q07-PRODUCTION-CONTENT-ROOT-SWITCH-001

Before: `Assets/NTSD/Config/GameConfig/GameConfig.asset` has no serialized `BattleContentRuntimeRoot`; GameConfig.cs defaults it to empty, so the battle Scene and Menu use pre-migration content. Explicit injected-root Play/Player probes have validated the formal staged source, but not a production serialized selection.

Planned exact edits: insert the single root field in the referenced GameConfig asset; add a command-line-gated serialized-root mode in the existing `NTSD28Q07WindowsPlayerRuntimeProbe.Install()` and report. In that mode the probe must not write `config.BattleContentRuntimeRoot`, and must reject a missing/wrong root before running the same content/World/shutdown checks. Existing explicit-root mode stays intact. No other script, Scene, prefab, old asset or framework edit.

Expected side effects: battle App/Menu and Scene bootstrap will select the staged formal DAT/images by default; a new unique Development Player build and run create output outside the production asset tree. Invariants: current Logan authority and fingerprint, 33 ms/runtime/shutdown contracts, Scene HUDBg user change, nonbattle UI/Unity/GAS and old resources untouched. Rollback and acceptance are in the Task Contract. Record actual diff, tests, warnings and unresolved returns immediately after the edit.

Validation: actual Windows Mono Development build `q07-serialized-build-1` Succeeded with 0 errors/130,681,174 reported bytes, 1,343 sidecar files and SPARK. Hidden serialized-root Player run returned exit code 0 and terminal PASS; `selectionMode=serialized`, `serializedRootBefore=Assets/NTSD/Content/LoganRuntime`, `sceneConfigMatched=true`, formal semantic fingerprint, equal three owner keys, World4, 29,400 resources, `RuntimeMapCleared`, two frames Stopped, 0 borrowers and 0 survivors. The YAML asset diff is exactly one added line; GameConfig SHA changed from `685E13823A6D726219B37B1B0B2A8E9B9A76B5B7975ED9CC8E598763B366B004` to `C4DB45C7426045D09FB481B2FC1D3FF27089CAF902A7AFE0CA413DE1795DF4B2`. Scene SHA unchanged at `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6`; Scene/Menu/ProjectSettings no Git diff. Player log still has 13 missing legacy audio-directory reports. No natural-skill, full visual, Menu serialized-default or other-platform acceptance is claimed; Q07 remains active. See Q07 PRODUCTION-ROOT-SWITCH-ACCEPTANCE.md.

Written: the GameConfig asset now serializes `BattleContentRuntimeRoot: Assets/NTSD/Content/LoganRuntime` immediately after its Unity script identifier; no other asset field was intentionally changed. The existing Development Player probe recognizes `-ntsd-q07-serialized-root-probe`; in that mode it records the pre-existing root and scene-config identity, never assigns the content-root field, and fails unless the serialized scene config already supplies the formal root. The old injected-root flag continues to assign the test root.
