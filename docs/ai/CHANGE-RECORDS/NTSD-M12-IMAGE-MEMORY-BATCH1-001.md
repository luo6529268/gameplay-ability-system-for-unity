<!-- CHANGE-RECORD
id: NTSD-M12-IMAGE-MEMORY-BATCH1-001
status: FOCUSED_TEST_PASS
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

2026-10-12 final source review: declared manager rebind path still used float PNG decoding. Same batch/scope additionally covers PrepareBattleCentralBindings/DecodeSource and TryBuildMatchCentralAtlasPublication pre-pixel and pre-atlas guards; budget adds decode/readback-only accounting for already resident source textures. No lifecycle, production resource, old diagnostics or P1 scope expansion. Exact current preimages: artifacts/diagnostics/M12-memory-batch1-20261012/rebind-review-01/before-manifest.json. Previous 10/10 focused pass remains tied to the earlier source; necessary narrow recompile and affected focused validation will follow this delta.

2026-10-12 FINAL BATCH1: NTSD-M12-IMAGE-MEMORY-BATCH1-001 FOCUSED_TEST_PASS. Original Editor narrow compilation/domain reload succeeded after retained ordinary import/namespace corrections; final isolated fixture 12/12 Passed, 0 failed/skipped (job 18cd851285b64abca719b67b09f4760e). Native prewarm and match rebind use compact PNG; one managed page reused; whole-batch predecode plus actual-atlas guards installed. Original Menu loaded/active/clean8, nonPlay/idle/nontest; final Console 0 error/warning. Protected 2353 paths unchanged; 1869 old artifacts metadata unchanged, no content re-read in final audit; original 135746-byte diagnostic/Q/F/P1 prefix identical. Parent M12 OPEN, native byte gate DEFAULT_OFF/H08 ON, automation stays paused. No Play/P1/ABBA/build/device/Profiler/prebuild paging or original PNG/DAT changes. This completes the requested source/compile/focused batch, not runtime/device acceptance. Evidence: artifacts/diagnostics/M12-memory-batch1-20261012/REPORT.md. ChangeLedger validation result will be appended after execution.

Final validation addendum: Tools/Validate-ChangeLedger.ps1 executed under bundled PowerShell 7 with -RepositoryRoot; exit 0, Change ledger validation PASSED. Full log contains 4433 WARNING lines, retained without fixing unrelated records. First Windows PowerShell attempt/error retained separately; no Git config/hook changes. Final Git status delta has 0 unexpected paths and staged index remains empty.
