<!-- CHANGE-RECORD
id: NTSD28-Q07-OID434-PUBLISHED-SPRITE-001
status: VERIFIED
change-kind: EDITOR_DIAGNOSTIC_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid434PublishedSpriteProbeEditor.cs
authority: NTSD 2.8-Logan formal EXE/playable ras.dat OID434 frame396 and current Q07 visual-owner audit
evidence: original Editor formal-root Battle Play OID434/pic36 published entry PASS; exact selected ras.png SHA matches formal; Editor compile 0 errors, Scene/Config stable; see ACCEPTANCE-20260925.md
-->

# NTSD28-Q07-OID434-PUBLISHED-SPRITE-001

2026-09-25 implementation and result: added only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid434PublishedSpriteProbeEditor.cs` and `.meta`. The opt-in request poller enters the saved Battle Scene, waits for live formal prewarm, reads the existing production catalog without modifying it, writes a collision-safe per-run report and exits Play. It creates no manager or Scene object. The original Editor imported it, Console error0; generated Editor project including the file built with 0 errors/164 warnings. Unique `oid434-publication-20260925-1` Play PASS at tick5: key434/36, exact `ras.png` source path, 48×48 crop, 490×198 shared texture, central binding valid; selected source-file SHA equals the formal VFS SHA. Editor returned idle Edit mode, Scene clean/root14, Console error0, Battle/Menu/GameConfig SHA unchanged. Consumed request and result are archived. This verifies only catalog publication; entity binding at tick29, renderer command, pixels and natural input remain Q07/Q09/R17 work. See `artifacts/diagnostics/NTSD28-Q07-OID434-PUBLISHED-SPRITE-001/ACCEPTANCE-20260925.md`. Rollback remains the new diagnostic code/request/result only under protected-file rules.

Pre-edit contract: the formal and staged `ras.png` are byte-identical, and existing controlled native/Unity traces have OID434/action396 at tick29, but neither trace records the Unity production `BattleSpriteEntry` selection. Add only an opt-in original-Editor readout of the published OID434/pic36 catalog entry after the saved Battle Scene's formal prewarm. Do not infer actual entity draw or pixel parity from the catalog query. This new file has no production behavior path and must not alter any existing test probe or user-owned request. A result path is create-new and uniquely named; failure must be preserved, not overwritten. No DAT values, PNGs, Scene, Prefab, ProjectSettings, Menu, nonbattle or production Runtime changes.

Affected symbols: new `NTSD28Q07Oid434PublishedSpriteProbeEditor` request poller and serializable report only. Expected side effects: one Editor request, one Play entry/exit, one result JSON, no saved Scene mutations. Invariants: formal content root, OID434/type3, authored pic36/`ras.png`, no auto-created singleton solely for test, unrelated probe behavior unchanged. Validation/rollback are in the Task Contract; this Record must be updated with exact files, compile, Play, Console, hashes, Ledger and limits before delivery.
