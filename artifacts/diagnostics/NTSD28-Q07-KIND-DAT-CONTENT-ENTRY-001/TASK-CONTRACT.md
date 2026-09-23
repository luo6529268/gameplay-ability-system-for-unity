# NTSD28-Q07-KIND-DAT-CONTENT-ENTRY-001

Status: `PLANNED` before copy. Parent: BATCH-04/Q07; D-023 formal DAT authority. This package adds exactly one previously absent battle DAT and its Unity `.meta`. It does not change a script, Scene, ProjectSettings, old DAT, image, sound or nonbattle behavior.

## Authority and current state

- Formal EXE SHA-256: `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`.
- Formal source: `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime\decoded_dat\data\kind.dat`, 229 bytes, SHA-256 `39E30DF8D86A5FC374B26C80BE358A6503B2C3096D8681C586478D73A0900011`.
- The corresponding playable build includes `kind_catalog.cpp`, `hit_candidates.cpp` and `game_session.cpp`; `GameSession28` loads the table or an identity-attested fallback. The parsed `effect=209`, `frame=40`, `bound={8,209,213}`, `respond={200,203,205,206,207,215,216}` record is used before ordinary candidate geometry and by the type-3 transform path.
- Unity staged `Assets/NTSD/Content/LoganRuntime/decoded_dat/data/kind.dat` and `.meta` are absent. Unity has matching locked-value code in `BruteForceSceneQuery.IsBlockedReleaseOidInteraction` and `BattleDamageWriter.IsNativeLockedKindTransformCandidate`, but no formal DAT reader/publisher. Code behavior and DAT-driven identity are separate gates.
- Fresh pre-copy DAT state: formal 405, staged 337, exact common SHA matches 337/337, formal missing 68. Older 70/74 missing counts were before later content packages.

## Exact change and acceptance

Copy only the formal `kind.dat` bytes to the absent staged path and allow the original Unity Editor to create its `.meta`. Verify source/staged SHA equality and the new GUID's uniqueness across `Assets`; verify unrelated files and the saved Battle Scene remain untouched. Update Q07 recovery state with `VERIFIED_EXACT_CONTENT_STAGING_ONLY` and rerun the current DAT count/hash inventory. No claim that Unity gameplay now consumes `kind.dat` follows from this copy.

Rollback boundary: the two new files only, subject to the repository's explicit deletion approval rule. This Task does not authorize deleting them, any old file, or any other current work. The next parser/publication package requires its own pre-change Task/Change Record and current-authority evidence before script edits.
