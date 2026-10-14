# OP-NTSD-M12-DAT-SPRITE-PAGING-PILOT-20261013

Status: PLANNED
Task: NTSD-M12-DAT-SPRITE-PAGING-PILOT-20261013
Change: NTSD-M12-DAT-SPRITE-PAGING-PILOT-001
Executor: current main agent; bounded generator worker.
Authorization: 2026-10-13 user: 那就按照你说的来处理，看看最终结果。 Accepted preceding first-pilot proposal for win2/Kirin/sui3-sui: independent pages/mapping/pixel proof, minimal loader, narrow compile/tests, offline ASTC comparison. No source PNG/DAT edit or Play/P1.
Operation: exact-preimage guarded patches to two existing C# files; append governance. New declared helper/test/runtime source and derived pages. No existing image/DAT deletion, replacement, movement or Git write.
Preimages/manifest: artifacts/diagnostics/M12-sprite-paging-pilot-20261013/before-manifest.json.
Existing paths:
- docs/ai/CHANGE-LEDGER.md
- docs/ai/STATE.md
- Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md
- docs/ai/CURRENT-AUTHORITY.md
- Assets/NTSD/Docs/ntsd28-logan-336b44-vs-unity-battle-alignment.md
- docs/ai/FILE-OPERATIONS/INDEX.md
- Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
- Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
New code paths:
- Assets/NTSD/Scripts/Animation/Runtime/BattleDerivedSpritePages.cs
- Assets/NTSD/Scripts/Test/Editor/M12SpritePagingEditorTests.cs
- Tools/Build-NtsdSpritePages.py
New content root: Assets/NTSD/Content/LoganRuntime/derived_sprite_pages/; enumerate exact output paths before generation, refuse all collisions.
Evidence root: artifacts/diagnostics/M12-sprite-paging-pilot-20261013/.
Planned APIs: apply_patch or guarded writes; append governance; python Tools/Build-NtsdSpritePages.py; existing Editor refresh/run_tests only after idle/nonPlay state confirmed.
Risks: effectivePic mapping, cache freshness, budget, rebind and texture ownership. Preserve 11-stage shutdown.
Rollback: own delta from byte-verified preimages under recorded authorized operation; never overwrite existing user dirty.
Pre-change status: backups verified, project C# not yet edited.

RUNNING: declared runtime helper created and exactly two baseline-guarded C# patches written with Node fs APIs. All preimage SHA checks passed; compile pending.

2026-10-13 Deployment: create exactly 19 PNG pages and manifest under the declared new derived_sprite_pages root; exact SHA/bytes in deployment-planned.json. No source overwrite. Focused tests may create retained GUID fixtures under Temp/M12Pg; negative-case replacements only same-test-owned files, before-image plus path/SHA JSON recorded before each replacement. Unity may create metadata for the declared new assets/scripts only; protected baseline saved before Editor import.

Incidental output correction: worker self-audit importlib load created Tools/__pycache__/Build-NtsdSpritePages.cpython-312.pyc outside the originally declared single helper-file scope. Exact identity in incidental-cache.json. Retained and disclosed; no deletion, move, overwrite or ignore-rule change. Future Python self-checks use -B. This is tooling cache, not runtime content.

2026-10-13 asset deployment completed: asset-receipt-final.json contains46 exact new source/metadata paths; protected-after-01.json confirms26/26 unchanged. Initial asset-receipt-01.json was truncated by tool timeout and is retained; final receipt supersedes it. Own Editor110700 stopped after19m23 without test XML, recorded editor-outcome-01.json; no other Editor stopped and no cache deletion. Existing code/governance preimages plus revision log preserved; no source PNG/DAT delete/move/overwrite. Runtime acceptance pending per REPORT.md.
