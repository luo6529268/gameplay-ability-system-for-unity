<!-- CHANGE-RECORD
id: NTSD28-Q07-SERIALIZED-MENU-CALLER-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs
authority: D-023 and Q07 production GameConfig root; preserve existing Menu prewarm caller
evidence: q07-serialized-menu-1 terminal PASS; serialized root and menu prewarm caller verified; Scene and GameConfig hashes held
-->

# NTSD28-Q07-SERIALIZED-MENU-CALLER-001

Before: existing Q07 App/menu Play probes force the formal root through a request-created GameConfig clone. They prove loading and cache behavior with the chosen root, but not that the current production GameConfig asset supplies it. The Menu Scene and battle Scene reference the same GameConfig GUID; the production asset now has the formal root, so the menu caller should be revisited before treating the switch as nonbattle-safe.

Planned edit: add `useSerializedRoot` to the existing Editor probe Request; when true, `PrepareRequest` validates the persistent asset path and formal root and passes the serialized value into the existing isolated Play clone. Default false leaves prior requests unchanged. No change to production game/menu code, Menu Scene, or GameConfig asset. Focused Play acceptance, rollback and unverified real-Menu-scene boundary are in Task.

Written: the one declared Editor probe now supports `useSerializedRoot`, pins the existing production GameConfig asset path, validates its formal root before Play, and records/verifies the asset root equals both prepared request and isolated Play clone. Prior default explicit-root requests retain their branch.

Validation: after Unity refresh/domain reload there were no Editor script compile errors. Unique `q07-serialized-menu-1` Play request returned terminal PASS: serializedRootSelected true, rootFromAsset formal path, menuReady true, cache hit1, formal fingerprint in source key, equal manager/data/UI keys, World4, 29,400 resources, previous/current owner survivors0, pool borrowers0, RuntimeMapCleared and two-frame Stopped. Editor returned idle/not playing. GameConfig SHA remained `C4DB45C7426045D09FB481B2FC1D3FF27089CAF902A7AFE0CA413DE1795DF4B2`; protected battle Scene SHA remained `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6`; Menu Scene has no Git diff. This is actual menu prewarm caller validation in an isolated battle-scene Play fixture, not actual Menu Scene visual interaction. Q07 remains active.
