<!-- CHANGE-RECORD
id: NTSD28-Q07-CLAMPED-CELL-PREWARM-RETENTION-001
status: VERIFIED
change-kind: BATTLE_PRESENTATION_CONTENT_MEMORY
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
authority: formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable point-clamp renderer
evidence: formal 1212-cell sizing audit; original Editor focused 3/3, catalog Play and OID32 pixel Play pass; actual process high-water pending
-->

# NTSD28-Q07-CLAMPED-CELL-PREWARM-RETENTION-001

Before code edit: the generic clamped-cell adapter retains `(Color32[] Pixels, string Hash)` per out-of-bounds cell until serialized upload, while releasing the CPU semaphore before entering the upload queue. Thirty-one formal sheets contain 1,212 such cells totaling 48,533,904 raw pixel bytes; actual Unity high-water is not measured. The existing 36 unique derived textures/sources, Battle SpriteCatalog and OID32 controlled pixel Play have already been observed. The new Task states exact scope, invariant, risk, validation and rollback.

Actual symbol: only `CharacterAnimtorManager.ProcessAndCreateSpritesForCandidateAsync` clamped-cell dictionary construction/consumption. The per-sheet dictionary now stores `Dictionary<int, string>`; the CPU phase computes the existing dimensions/RGBA content hash from a temporary array, while the first serialized upload for each unique hash rematerializes the same pixels for the existing texture/Sprite/atlas-source transaction. Previously the dictionary held `(Color32[] Pixels, string Hash)` across the upload wait. The immutable `processedSheet` remains the sole sampling source. The queued per-cell Color32 arrays are no longer referenced across `uploadSemaphore.WaitAsync()`; this is a source-level retention bound, not measured process memory savings.

Validation on the original project Editor 2022.3.62f3: MCP forced refresh returned ready and the runtime Assembly-CSharp.dll write time followed the code edit; focused `NTSD.Test.Editor.NTSD28Q07NativeClampedCellEditorTests` job `894822c56d4f4c5eb21d2302e72dc2a8` passed 3/3. Formal Battle Scene catalog Play `oid32-catalog-clamp-20260925-03.json` observed 1,212 derived entries, 36 unique source paths/textures, zero invalid central bindings, OID32/pic64 Legacy and Central binding both valid, and no Scene dirty state. Controlled full-tick OID32/frame95 camera Play `oid32-entity-pixel-20260925-04.json` passed: tick6 stable102/slot50, one matching central entity command among nine, 56×56 ROI with 2,835 pure white and 3,136 nonclear pixels; object/slot/pool counts stayed 4/2/2/2 and post-exit live derived Texture2D count was zero. Menu/Battle on-disk SHA remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. `git diff --check` exited 0. Full Q07, natural-input route, formal root-EXE GPU parity and actual Editor process high-water are still open. Rollback scope is this exact one-function delta; do not remove other changes already present in the file.

Governance validation after documentation update: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exited 0 with 834 Records and 30 governed code files in the current diff. The three previously recovered progress documents were rechecked NUL-free; no v3 candidate was recopied over their newer status addenda.
