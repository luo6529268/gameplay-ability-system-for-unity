<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-CLONE-CENTRAL-PIXEL-WITNESS-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07CloneCentralPixelProbeEditor.cs
authority: formal OID33 ncl.dat/ncl.png and Q07 real Naruto physical L/D/J evidence
evidence: q07-clone-pixel-3 and q07-naruto-clone-5 Play PASS; first two FAILs retained; ACCEPTANCE.md; Scene SHA unchanged; Ledger651 PASS
-->

# NTSD28-Q07-NARUTO-CLONE-CENTRAL-PIXEL-WITNESS-001

Before: natural physical-input Play proved OID33 hidden-to-visible catalog binding but did not observe central render commands or camera pixels. Existing Q07 combo probe is modified and verified; it must remain unchanged. `BattleCentralFailClosedOwnershipPlayModeProbeEditor` contains a camera RenderTexture capture pattern, but its ownership probe is not this natural clone sequence.

Planned exact edit: one new Editor-only request-driven observer script and meta. It observes the already-running Q07 physical-input Play without creating entities or changing simulation, finds a current central command for OID33 pic1, captures the bound battle camera to a temporary RenderTexture, counts non-clear pixels in the projected command bounds and writes a unique PNG/JSON result. No production, Scene, resource, GameConfig, old asset or nonbattle change. Camera state and RenderTexture.active must restore in `finally`; no persistent GameObject.

Expected side effects: one Editor script import/recompile and one target Battle Play; temporary output in its declared diagnostics directory. No change to the established Q07 request/result semantics. Non-clear region only supports a limited pixel-route claim; formal EXE pixel parity and sorting/shadow remain open.

Rollback: remove only the new test script/meta and its isolated request after confirming no other owner, preserving prior Q07 probe and all dirty files. Validation: compile 0 new CS errors, unique natural Play result plus central pixel report, Scene SHA/dirty, idle exit, focused Ledger; record any first FAIL and limitations before status advancement.

Written: the new isolated Editor request observer captures a materialized central OID33 pic1 command and performs a temporary production-camera RenderTexture readback in its projected bounds. `pwsh -File Tools/Validate-ChangeLedger.ps1` PASSED (651 records, 13 governed code files); the first attempted legacy Windows PowerShell invocation failed before validation because its `$PSScriptRoot` default was empty, and is not counted. Unity MCP direct local socket refresh imported the new script and refreshed `Assembly-CSharp-Editor.dll` without new `error CS` in the current log tail.

First Play retained: `q07-clone-pixel-1.json` FAIL with "Formal content root/fingerprint is unavailable." The observer checked the manager fingerprint as soon as the battle driver existed, before formal content prewarm/publication completed. The concurrent unmodified natural-input probe `q07-naruto-clone-3.json` later PASSed (physical ticks2/4/6, OID33 hidden→visible and formal source key). This is an observer readiness error, not evidence that production pixels failed. Correct only the observer's prewarm wait and rerun with unique request/output IDs; preserve both first results.

Second Play retained: `q07-clone-pixel-2.json` FAIL. The natural-input `q07-naruto-clone-4.json` PASSed. The new probe found OID33 pic1 in a current tick12 central command (stableId103, runtimeSlot51, visualDataId33/effectivePic1), but camera readback was all white (full/region nonclear=0; projected region area3192). Source review shows the probe held a central submission read lease while calling `Camera.Render`, whose render feature must acquire its own lease; this can make the capture self-block. Test-only correction will copy the command under lease, release it, then render. The all-white image and FAIL remain as an unresolved test first difference until a fresh corrected capture.

Verified: the test-only read order now releases its lease before rendering. Fresh `q07-clone-pixel-3.json` PASS with tick12 OID33 pic1 central command and 2583 full/1087 projected-region non-clear camera pixels; PNG SHA `6920BBC528956DE904793B561CC45A578F8CF509B9CC22AD7052B25B09C22B1A`. Same Play `q07-naruto-clone-5.json` PASS physical ticks2/4/6 and formal clone hidden→visible. Unity import/compile had no new `error CS` in current log tail; Editor outside Play, Battle Scene dirtyfalse/root14, Battle/Menu hashes unchanged, requests false. Ledger651/13 PASS and targeted diff-check PASS. This closes only one representative central command-to-pixel route; exact EXE pixels, sorting, shadow, whole skill and other assets remain open. Full evidence in the same-ID `ACCEPTANCE.md`.
