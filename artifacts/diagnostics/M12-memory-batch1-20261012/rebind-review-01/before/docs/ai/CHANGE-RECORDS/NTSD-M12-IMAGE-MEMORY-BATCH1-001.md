<!-- CHANGE-RECORD
id: NTSD-M12-IMAGE-MEMORY-BATCH1-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Animation/Runtime/BMPLoader.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleAtlasResources.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12DecodeUploadBackpressureEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleImageMemoryBudget.cs
authority: Direct human first image-memory batch approval on 2026-10-12
evidence: artifacts/diagnostics/M12-memory-batch1-20261012/REPORT.md
-->

# Native image memory first batch

Task: NTSD-M12-IMAGE-MEMORY-BATCH1-20261012
Operation: OP-NTSD-M12-IMAGE-MEMORY-BATCH1-20261012

Before: native PNG byte RGBA -> Color[] -> Color32[]; all managed atlas pages coexist; per-job leases do not bound batch residency.
After: compact internal native PNG; one cleared upload page for arrays and fallback; checked whole-batch image estimate before decoding and actual atlas preallocation recheck. Refuse batch and retain previous publication.

Invariants: original PNG/DAT, pixel/alpha/rect/UV/catalog/black-key rules, shutdown/drain order, old Q/F/P1 and public legacy loaders remain. Old byte gate DEFAULT_OFF and H08 ON; no Scene/Settings/package changes, Play/P1/build/Profiler/old ABBA.
Provisional engineering image envelopes: resident 512 MiB and loading 768 MiB; not measured Android/physical process allowance. CPU/GPU driver, decoder internal, GC latency and non-image process costs remain separate unknowns.

Validation: pixel/alpha/orientation/mutation guards, page clearing/extrusion/array/fallback, aggregate/duplicates/old overlap/overflow/refusal, original Editor narrow compile and isolated focused tests. No runtime acceptance claim.
Rollback: expected-current-byte preimages in before-manifest, own delta only.

2026-10-12 CODE_WRITTEN: exact internal PNG compact loader, one-page assembly and native batch admission/preallocation guard plus independent 10-case EditMode fixture appended. Old diagnostic/Q/F/P1 test file prefix byte-identical. Implementation diff saved. Compile/tests not yet accepted. Ordinary guarded text-placement errors were fixed before manual compilation; no runtime/P1 was executed.
