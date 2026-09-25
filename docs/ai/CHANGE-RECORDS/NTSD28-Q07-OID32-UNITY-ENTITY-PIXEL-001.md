<!-- CHANGE-RECORD
id: NTSD28-Q07-OID32-UNITY-ENTITY-PIXEL-001
status: VERIFIED
change-kind: EDITOR_BATTLE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid32EntityPixelProbeEditor.cs
authority: formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable OID32/action95 D3D11 point-clamp pixel witness
evidence: original Editor Tundra compile pass; controlled tick6 OID32/pic64 central command and white camera pixels; exact pool cleanup and post-exit derived texture count zero
-->

# NTSD28-Q07-OID32-UNITY-ENTITY-PIXEL-001

Before code edit: the formal OID32/pic64 cell was previously absent from Unity publication and is now in the real Battle Scene catalog as a shared derived 79x79 Sprite with valid central binding. The paired playable renderer draws 6,241 white pixels in that ROI. No Unity entity pixel witness or post-exit derived texture count exists. This package only adds an opt-in Editor diagnostic; it does not alter the production adapter, DAT/PNG/Scene or battle rules.

Exact code path and symbols: new `NTSD28Q07Oid32EntityPixelProbeEditor` request poll, production fixture, central-command/camera capture, guarded cleanup and post-Play derived-texture count. Expected side effects are transient Play-only pooled entity and temporary render texture/PNG report. Invariant: preserve existing world/pool/roster, scene serialization and production cadence. Acceptance and rollback are in the Task. Status remains IN_PROGRESS until real Editor evidence is reviewed.

After code edit: added only the declared Editor probe. It waits for formal content and a paused stable tick, creates one production OID32 from the existing pools, requests frame95/pic64, dispatches a presentation frame, selects the exact stable-ID command, reads a black-clear camera texture, records white/visible ROI pixels and baseline world/pool counts, frees the fixture and exits Play. No production script, DAT, PNG or Scene edit. Compilation and Play are pending; status `CODE_WRITTEN`, not behavioral verification.

First original-Editor attempt `oid32-entity-pixel-20260925-01` retained FAIL before fixture creation: a diagnostic-only hardcoded semantic fingerprint from the prior content snapshot differed after the worktree acquired `data/bgm.dat`. The report's baseline-count failure was a probe artifact because no baseline/fixture existed. Both Scene hashes remained unchanged and the Editor exited Play. Corrected the probe to require the formal root and a nonempty published identity while recording its actual fingerprint, and to compare cleanup counts only after a fixture baseline exists. No production behavior changed; rerun pending.

Second original-Editor attempt `-02` retained FAIL with production fixture frame95/pic64 and exact before/after object/slot/pool counts restored, but the same-tick central plan contained only four pre-existing commands and no new entity. `SimulationStageRenderModule.GetPresentationEntitiesNoAlloc` filters by current-pass active entities, so the diagnostic now schedules one full production Driver tick after spawn before reading the plan; this is a test-fixture activation correction, not a production rendering rule change. Third run pending.

Final original-Editor `-03` PASS: after one full production Driver tick, OID32/stable102/slot50 is frame95/pic64 and exactly one matching Entity command appears in the nine-command central plan. Real WorldCamera RenderTexture readback has 2,835 white pixels in the projected 56x56 ROI; PNG inspected. Counts restore 4/2/2/2, cleanup error empty; original Editor exits Play, post-exit `native_clamp_` Texture2D count zero and both Scene hashes unchanged. The final Editor assembly was rebuilt with zero C# errors after adding the post-exit counter. Full details, artifact hashes and limits: `artifacts/diagnostics/NTSD28-Q07-OID32-UNITY-ENTITY-PIXEL-001/ACCEPTANCE-20260925.md`. This diagnostic Change is VERIFIED for its controlled Unity pixel scope. Production memory peak, natural skill entry, direct root-EXE GPU and Q07/R17 aggregate acceptance remain open. Rollback would remove only the declared diagnostic script/meta and its own governance/output, subject to the user's explicit deletion approval.
