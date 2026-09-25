<!-- TASK
id: NTSD28-Q07-SOUND-TABLE-DAT-STAGING-001
status: CONTENT_STAGED / BYTE_HASH_PASS / UNITY_ASSET_DB_RESOLVED / Q10_RUNTIME_PENDING
-->

# NTSD28-Q07-SOUND-TABLE-DAT-STAGING-001

Parent: BATCH-04 / Q07; Q10 owns later audio parsing, WAV resources and playback validation.

Authority: D-023 selects NTSD 2.8-Logan formal DAT content except user-excluded native background and both mode DAT families; default `stage.dat` deployment remains held. Formal root EXE SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Corresponding playable `audio_backend.cpp` constructs `XAudio2Backend28` by reading `data/sound.dat` from available source roots, and the formal DAT contains 18 ordered `file:` records. This is an active audio-content input, not a Q07 battle-rule implementation claim.

Pre-change state: formal `resources/runtime/decoded_dat/data/sound.dat` exists (397 bytes, SHA-256 `7DA4AAD9E3729C44FD6F1D9A4659BC1C925069A7FDD405DD30D2E5BEA11B1C12`); `Assets/NTSD/Content/LoganRuntime/decoded_dat/data/sound.dat` is absent. It is one of 17 remaining nonexcluded DAT paths in `CURRENT-SCOPED-DAT-INVENTORY-20260925.json`.

Scope: add exactly the formal `data/sound.dat` byte-for-byte to the existing staged LoganRuntime decoded DAT tree and add a new unique Unity `.meta` using the adjacent `DefaultImporter` format. Do not edit any existing DAT value, copy WAVs, change `ProjectBattleModeConfig`, background/mode DAT, Scene, script, importer or audio routing. Do not delete old files.

Acceptance: staged file SHA and length equal formal source; 18 ordered paths remain unchanged; excluded-DAT presence remains zero; updated nonexcluded inventory becomes 337 staged/353 target with 16 owner-separated missing paths; Git diff/status shows only this Task, new DAT/meta and bounded audit records from this package. Unity import, Q10 audio behavior and formal EXE listening comparison are separately pending.

Rollback: after provenance review and repository-required deletion approval, remove only the new DAT/meta pair; preserve all preexisting resources and user work.

Acceptance evidence (2026-09-25): the new staged DAT is 397 bytes and SHA-256 `7DA4AAD9E3729C44FD6F1D9A4659BC1C925069A7FDD405DD30D2E5BEA11B1C12`, identical to the formal source. It contains 18 ordered `file:` records. The new `.meta` GUID is unique in the scanned `Assets` meta set. Full current staged-DAT rerun: formal 405, excluded 52, target 353, staged 337, all 337 same-path SHA identical, extra 0, mismatch 0, remaining 16 owner-separated paths. Exact result and limits: `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/SOUND-TABLE-DAT-STAGING-ACCEPTANCE-20260925.md`. Original Unity Editor import and Q10 playback were not run.

2026-09-25 original Editor correction: the running original Editor's read-only AssetDatabase `manage_asset/get_info` resolved this exact `sound.dat` path as `UnityEditor.DefaultAsset`, GUID `84222ed9e2854318a86a4b5d6ba591ee`, with a nonzero instance ID. The saved response is `artifacts/diagnostics/NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/original-editor-sound-dat-import-20260925.json` (SHA-256 `BB3E0D36190BAB2ED74F438141C45DA865637FDE4DE0ACDC3F70E3284E345349`). Both Scene disk hashes stayed unchanged. This supersedes the earlier blanket `UNITY_IMPORT_PENDING` wording only for AssetDatabase registration; it does not test DAT parsing, 18 WAV availability, audio event routing, or audible playback. Q10 remains `RUNTIME_PENDING`.
