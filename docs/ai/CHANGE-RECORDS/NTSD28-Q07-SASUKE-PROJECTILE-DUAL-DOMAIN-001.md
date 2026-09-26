<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-PROJECTILE-DUAL-DOMAIN-001
status: FOCUSED_TEST_PASS
change-kind: Q07_D024_SASUKE_PROJECTILE_CONFIGURED_VIEW_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07SasukeProjectileDualDomainEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; paired playable full Session; formal OID11/OID440 DAT
evidence: artifacts/diagnostics/NTSD28-Q07-SASUKE-PROJECTILE-DUAL-DOMAIN-001/ACCEPTANCE.md
-->

# NTSD28-Q07-SASUKE-PROJECTILE-DUAL-DOMAIN-001

The existing same-initial-state Sasuke no-hit trace has 26-tick root-EXE/Unity factor1 parity, but it does not test the user's approved configured-view physical displacement and independent formal source-rule coordinates for the naturally spawned OID440 projectiles. This scoped diagnostic will reuse that trace and scenario, read a small projected authoritative fixture, and run the original Editor complete Driver at the configured view. The existing raw-capture helper and production code remain untouched.

Actual edit: added `NTSD28Q07SasukeProjectileDualDomainEditorTests.cs` and its `.meta`. The test reuses the existing complete-Driver fixture, seeds both participant source positions from the post-stage-placement starting state, configures the user-approved 2048×1152 view, compares 48 projected formal projectile rows, and writes its observed source/view coordinates even on an assertion failure. The formal projection was derived without source edits from the saved root-EXE trace (source SHA-256 `2450850DC5D2548E8904DD2C815146B550D6708EF5172807B545D44E9DAFD957`; projected CSV SHA-256 `965BDF4CD3F08CDD4C5BAB47DFEDF5533D5B95687926476410E9D19673E589FD`). No production, DAT, image, Scene, configuration asset, nonbattle path or old resource was edited by this task. Current status `CODE_WRITTEN`; fresh original Editor compile and focused run are still pending. An observed first difference requires separate production scope, not automatic fix in this record.

First original Editor refresh compiled with two test-only `CS1061` errors: `LF2Entity` has no `CurrentFrameId` member at the new capture/assertion lines. The existing fusion test reads the action through `entity.Frame.D?.frameId`; correct only those two new test expressions and refresh the same Editor. This is not a production or formal first difference.

Final validation: that two-expression correction compiled in the original Editor; exact filtered EditMode job `38b62db6c43e4dd796fca35e851f4d8a` executed/passed 1/1. The fresh 48-row configured-view raw capture matches the formal OID440 source coordinates and action/integer fields; all 44 post-birth X and 11 unblocked positive-Z physical displacements match the user-approved factors within floating-point noise. The helper's zero-residue shutdown gate passed. `git diff --check`, Change Ledger validator and four protected hashes passed. The mistaken interpretation of `progress.total=8546` as unfiltered was corrected by final job `summary.total=1`; unsupported cancel emitted only a bridge Console error. This record is `FOCUSED_TEST_PASS` for this route, not an aggregate D-024/Q07 closure. See the acceptance artifact for limits.
