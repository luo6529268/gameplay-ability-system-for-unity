<!-- CHANGE-RECORD
id: NTSD28-Q07-OID30-UNITY-BOUNDARY-PIXEL-001
status: VERIFIED
change-kind: EDITOR_BATTLE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid32EntityPixelProbeEditor.cs
authority: formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired OID30/action31 source/release trace
evidence: original Editor compile; OID30 catalog/central camera Play PASS, OID32 default regression PASS, zero post-exit derived textures and Scene SHA stable
-->

# NTSD28-Q07-OID30-UNITY-BOUNDARY-PIXEL-001

Before code edit: the existing OID32 opt-in Editor probe has already passed on the original Battle Scene, including full Driver activation, central command, camera pixels and zero post-exit derived textures. It is hardcoded to OID32/frame95/pic64. Formal OID30/action31/pic81 is now source/release-reachable in a controlled world, but Unity publication and pixels remain unknown. A read-only MCP `execute_code` query failed before execution because its Mono compiler command line was too long; that failure says nothing about gameplay.

Declared change: extend only the existing Editor probe with a fixed second scenario selected by a request field, with the original OID32 default unchanged. Add catalog identity/binding/dimensions to the report, then use the same production World/Driver/render/camera/cleanup path for OID30. Invariants: no production/authority/content/Scene/nonbattle change, no unchecked arbitrary OID/frame selector, no draw-state writeback, no extra persistent borrower or scene serialization. Expected transient side effects are one test-only pooled character and temporary RenderTexture/PNG. Validate original Editor compile, one controlled Play, cleanup and Scene hashes; report root-EXE GUI and natural input as pending. Rollback limited to this exact diagnostic extension under protected-worktree rules.

Actual edit: extended only `NTSD28Q07Oid32EntityPixelProbeEditor` with request target `oid30-frame31`, leaving an absent/default target mapped to OID32/frame95/pic64. Other targets fail closed. The existing production fixture/Driver/render/camera/cleanup path now selects fixed OID/frame/pic properties; new catalog fields record source path, 79x79 texture and Legacy/Central validity, with OID30-specific precondition before spawning. The original `oid32CommandSummary` stays OID32-only and a new target summary covers both scenarios. No production code/content/Scene changed. Original Editor compile and Play are pending at this status.

Validation: original Editor imported both script revisions with Tundra success and no current C# error. Explicit OID30 Play `oid30-frame31-pixel-20260925-01` PASS: formal content, production catalog OID30/pic81 79x79 derived texture, valid Legacy/Central binding, full Driver tick6 exact stable102/slot50 Entity command, and 2,870 white pixels in 56x57 camera ROI. OID32 default/no-target regression `oid32-default-regression-20260925-01` PASS with its previous 2,835 white pixels in 56x56 ROI and unchanged legacy command summary. Each run restored 4/2/2/2 world/slot/pool counts and had zero post-exit derived textures; Menu/Battle SHA stable. Exact JSON/PNG hashes and limits are in `artifacts/diagnostics/NTSD28-Q07-OID30-UNITY-BOUNDARY-PIXEL-001/ACCEPTANCE-20260925.md`. Natural input, OID31/frame41 and direct root-EXE GUI pixel A/B remain unverified; Q07/R17 stay open. Rollback only this diagnostic code delta under protected-worktree rules.
