<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-OUTOFIMAGE-WARP-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q09Diagnostics/hun_outofimage_warp_probe.cpp
authority: formal root NTSD2.8-Logan EXE and paired playable render snapshot/D3D11 build closure
evidence: docs/ai/TASKS/NTSD28-Q09-P20-OUTOFIMAGE-WARP-001.md; artifacts/diagnostics/NTSD28-Q09-P20-OUTOFIMAGE-WARP-001/ACCEPTANCE.md
-->

# NTSD28-Q09-P20-OUTOFIMAGE-WARP-001

Created before any script edit. Exact Task: `docs/ai/TASKS/NTSD28-Q09-P20-OUTOFIMAGE-WARP-001.md`. Existing formal OID32 action95 pic64 source rectangle is below the PNG height; paired renderer CLAMP and Unity PNG rect rejection may diverge, but no paired GPU or root/Unity natural same-state image has yet been measured. The OID55/pup preflight candidate has a fully transparent clamp row and was replaced before script creation. This Record owns only the new C++ diagnostic main under Tools. Expected side effects are a new local diagnostic executable, two guarded PNG outputs and bounded logs under a same-ID artifact folder. It will not write World, formal content, Unity runtime or protected Scene. Acceptance requires compiler/run exits, independent pixel comparison, identity/Scene hashes and Ledger/diff validation. Rollback is limited to this ID's new files and follows the repository deletion-approval rule. Current state `PLANNED`.

Postchange: the declared Tools C++ main was added, compiled with the existing paired offscreen closure (compiler exit0), run against the formal DAT/VFS (exit0), and produced an exact 6241-pixel white 79×79 body A/B difference. Overwrite guard exit3 preserved PNG hashes. The initial shell wrapper exit1 only failed while reading the absent empty log; an empty log was created and compiler/executable success checked separately. Source, executable and image hashes, protected SHA and detailed limits are in `ACCEPTANCE.md`.

Reconciliation: the original pre-edit statement that Unity would reject the body was incomplete. `BuildIndexedSpriteRects` returns no ordinary rect, but the existing `BuildNativeClampedCellPixels` path publishes the full derived Sprite, and prior original-Editor catalog/controlled pixel evidence already covers OID32 pic64. The paired GPU white block also had a prior independent witness. Therefore this Change is `VERIFIED` only for the new same-snapshot paired diagnostic; it establishes **no new production first difference** and does not authorize a new runtime fix. P-20/Q09 remain open. No Unity build/test or Scene Play was repeated.

Governance: final documentation reconciliation was followed by `Tools/Validate-ChangeLedger.ps1` exit0 (`PASSED`, 1001 Records, 33 governed code diff files covered) and `git -c core.safecrlf=false diff --check` exit0. Four protected asset SHA values were unchanged as recorded in Acceptance. Historical missing-current-diff warnings from other Records did not fail the validator.
