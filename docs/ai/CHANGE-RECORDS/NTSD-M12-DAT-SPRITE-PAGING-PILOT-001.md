<!-- CHANGE-RECORD
id: NTSD-M12-DAT-SPRITE-PAGING-PILOT-001
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Runtime/BattleDerivedSpritePages.cs
code-path: Assets/NTSD/Scripts/Test/Editor/M12SpritePagingEditorTests.cs
code-path: Tools/Build-NtsdSpritePages.py
authority: Direct human approved DAT-derived first paging pilot on 2026-10-13; current336B44 battle/content preserved
evidence: artifacts/diagnostics/M12-sprite-paging-pilot-20261013/REPORT.md
-->

# First DAT sprite paging pilot

Task: NTSD-M12-DAT-SPRITE-PAGING-PILOT-20261013
Operation: OP-NTSD-M12-DAT-SPRITE-PAGING-PILOT-20261013
Authorization: 2026-10-13 user: 那就按照你说的来处理，看看最终结果。 Accepted preceding first-pilot proposal for win2/Kirin/sui3-sui: independent pages/mapping/pixel proof, minimal loader, narrow compile/tests, offline ASTC comparison. No source PNG/DAT edit or Play/P1.

Before: full source PNG decoding, native cumulative row*col pic ranges, physical body reuse by path+SHA and runtime RGBA atlas.
Target: optional captured derived-page manifest for exact four pilot sources; preserve original configs and DAT; translate decode jobs into shared small PNG pages and use existing per-Sprite rect/source override, candidate freshness, budget and publication ownership. ASTC stays offline comparison in this pilot.
Affected symbols: CharacterAnimtorManager native grouping, memory preflight, sprite plan/create, existing catalog per-sprite override, rebind; LoganVisualContentCandidate capture/hash; new BattleDerivedSpritePages immutable schema/validation; isolated M12SpritePagingEditorTests; offline Tools/Build-NtsdSpritePages.py.
Invariants: original PNG/DAT/meta, existing user dirty, native keys/pivot/frame metrics/alpha, logic/pass/checksum and11-stage shutdown. Existing diagnostics/P1 untouched. No Play/P1/build/hooks/Settings/Scene/package changes or 14 multi-piece oversized frames.
Expected side effects: new derived PNG/manifest only, candidate visual fingerprint changes; smaller but more textures; existing whole-batch gate retained and can still reject full catalog. No gameplay identity or content-number changes.
Validation: exact pixel reconstruction, SHA/protection, sharing and bounds, malformed/hash/path rejection, real Unity Sprite/texture/key/pivot, budget/rebind/cancel/cleanup focused checks and original Editor narrow compile. Offline ASTC separate from GPU acceptance.
Rollback: own delta and precise backup; no irreversible action.
Initial status at registration: PLANNED, before C# changes or tests.

2026-10-13 CODE_WRITTEN: immutable derived manifest validation added; candidate visual identity/freshness includes selected pages. Body jobs and budget use actual small pages; catalog reuses existing per-Sprite source/rect override; packed pages retain SourceTexture2D through prewarm and match rebind, no redundant runtime atlas or retained page pixels. Original DAT/configs and other source routes unchanged. Compile/focused and generated files still pending.

> 2026-10-13 DAT PAGING PILOT DELIVERY: NTSD-M12-DAT-SPRITE-PAGING-PILOT-001 RUNTIME_PENDING. Four source paths ->19 deployed PNG pages,88/88 exact mappings,63 unique frames; shared sui pages. Final independent Runtime+Editor compile0 errors; original Editor initial compile completed but prolonged ReloadAssembly/VirtualArtifacts warnings yielded no test XML, owned batch stopped after~19m23s. No focused PASS or Play/P1 claim; last H08 two-argument compatibility shim Editor rerun pending.26 exact protected SHA unchanged, original PNG/DAT and user dirty preserved. PNG pages4.29MiB; RGBA164.96MiB; ASTC4x4 payload41.24MiB offline only. Minimal loader wired via optional manifest, no remaining51 expansion or parent M12 closure. Evidence: artifacts/diagnostics/M12-sprite-paging-pilot-20261013/REPORT.md. Next: original Editor14 focused cases; no Library cleanup or P1. Prior PLANNED entry is historical.

Actual validation commands, runtime timeout, precision limits, final code/asset scope, offline ASTC and protected SHA are recorded in REPORT.md. Preserve original inputs and existing dirty work; rollback remains own-delta-only.
