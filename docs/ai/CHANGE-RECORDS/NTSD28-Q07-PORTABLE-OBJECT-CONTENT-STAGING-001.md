<!-- CHANGE-RECORD
id: NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: D-023 formal NTSD 2.8-Logan runtime resources; Q01 indexed object and image manifest
evidence: READINESS.md; 1343 source and target SHA-256 matches, 0 missing or extra files, protected Scene unchanged
-->

# NTSD28-Q07-PORTABLE-OBJECT-CONTENT-STAGING-001

Status: `VERIFIED` for exact resource-byte staging only (2026-09-22; no script change). Production publication and Q07 content availability remain pending.

Authority/need: D-023 formal DAT and character-related image content, Q01 manifest, Q02 portable source contract, Q06 scoped exit. Existing Unity `Assets/NTSD/Config` and `Assets/NTSD/Sprite` remain the production content because GameConfig battle-content root is empty.

Declared paths: only the 1,343 `targetRelative` files listed in `artifacts/diagnostics/NTSD28-Q07-CONTENT-MIGRATION-READINESS-001/copy-manifest.csv`, under `Assets/NTSD/Content/LoganRuntime`, plus Unity-generated `.meta` for that new subtree if import occurs. No C# symbol or existing asset is changed in this package. New root is a portable candidate, not an approved build-packaging contract.

The validator's `GOVERNANCE_ONLY`/`code-path: NONE` metadata denotes no governed **script** path; the actual resource changes are explicitly enumerated by the copy manifest. It does not mean this record was documentation-only.

Expected side effects: new Unity asset import work and new tracked/untracked files; no active battle content change, no GUID rebinding, no Scene dirty, no old-file deletion. Preserve all user work and current exceptions.

Acceptance: source+target 1,343/1,343 hashes and sizes agree, target set has no unexpected files (ignoring only generated `.meta`), existing source and configured root unchanged; next package must test actual candidate/publication/Play before switching production. Rollback is exact new-root removal only with separate deletion approval; no implicit cleanup.

Prechange evidence: 2026-09-22 fresh manifest check `rows=1343`, `uniqueTargets=1343`, `bytes=46594829`, `issues=0`, manifest SHA-256 `E09BEC00503D577ECB401275C7E066CBF2DC8A13CBEBC87109B7E73184301792`.

Actual change: copied only the declared 1,343 files, totaling 46,594,829 bytes, to the new root. Post-copy SHA-256 matched every row; independent target enumeration was 1,343 expected/actual, 0 missing/extra, 0 `.meta` yet, and default stage.dat absent. No existing Config/Sprite path changed; GameConfig still has its default empty root. Actual `Assets/NTSD/Scene/NTSD_Battle.unity` SHA-256 `BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6` matches the protected baseline. `git diff --check` exited 0 with line-ending warnings. No Unity compilation, candidate capture, publication or Play was run for this resource-byte package. Remaining risk: Unity import/meta and build packaging, complete candidate/caller acceptance, production switch, dynamic references and deletion classification. See READINESS.md.
