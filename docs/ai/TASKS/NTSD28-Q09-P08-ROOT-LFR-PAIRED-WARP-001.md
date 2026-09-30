# NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001

Status: `VERIFIED_SCOPED_PAIRED_LFR_WARP / P-08_OPEN`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-08`. [限定验收](../../../artifacts/diagnostics/NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001/ACCEPTANCE.md)。

Authority: unchanged formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `GameSessionLfrPlayback28`, `RenderSnapshotBuilder28` and `D3D11Renderer28`. Reuse only the immutable 22-tick LFR and root replay evidence from `NTSD28-Q09-P08-NATURAL-EQUAL-HP-ROOT-LFR-001`.

Scope: add one Tools-only playback diagnostic. Restore the LFR's evidenced initial action/facing/MP, advance all declared ticks through `GameSessionLfrPlayback28`, inspect the tick-22 snapshot, and render an unchanged copy and a copy with only the selected bleed command removed at 1333×730 through paired WARP. Verify final LFR headers and the selected tick/mark properties. Preserve a bounded failure if the playback path does not reach the mark. Compare the two resulting PNGs independently. No production Unity script, DAT, scene, project mode, original background deployment, or nonbattle code changes.

Declared code path: `Tools/NTSD28Q09Diagnostics/ita_equal_hp_lfr_playback_warp_probe.cpp` only. New outputs live under `artifacts/diagnostics/NTSD28-Q09-P08-ROOT-LFR-PAIRED-WARP-001/` and cannot overwrite existing files.

Acceptance: paired playable closure compile; LFR load/initial overrides/full tick/headers pass; tick-22 survivor/standing mark exactly one with threshold10, width1, height3, red; both offscreen PNGs generated and independent pixel diff attributes exactly three pixels to the command. Check root EXE identity, protected Unity hashes, validator and diff. Report this as paired-source playback GPU evidence only: headless root EXE itself still has no captured pixel witness, and Unity same-state/same-viewport remains open. Do not rerun already passed Unity scene tests.

Rollback: preserve existing dirty work and all earlier evidence. Removing this new script/artifacts requires the repository's explicit deletion approval.
