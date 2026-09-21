# NTSD28-Q07-WINDOWS-PLAYER-NATURAL-SKILL-001

Status: VERIFIED_PLAYER_NARUTO_REPRESENTATIVE_ONLY. Parent: BATCH-04 / Q07 formal content availability and R17 Player return.

Authority: D-023 formal Logan DAT/character images; formal selectable Naruto OID2 `hit_Fa:285`, OID33 clone authored frames 241/242 and `c/nar/ncl.png`. Existing Editor natural-physical witness and clone-sprite binding are scoped PASS, while Q07 Windows Player acceptance currently stops at content bootstrap/ordered close. The formal EXE identity and playable source closure remain as stated in `docs/ai/CURRENT-AUTHORITY.md`.

Exact write scope: one new `Assets/NTSD/Scripts/Test/NTSD28Q07WindowsNaturalSkillProbe.cs` (and generated `.meta`) guarded by `DEVELOPMENT_BUILD && !UNITY_EDITOR` and a unique command-line flag. Reuse existing Q07 Windows build probe unchanged. Do not alter production battle logic, input routing, Scene, GameConfig, Build Settings, old assets, UI or GAS. The Scene's serialized `BattleTestBootstrap.overrideCharacterIds` already selects Naruto OID2; no unsaved override is needed.

Acceptance: built Windows Player with default formal root reaches Running OID2; physical InputSystem L/D/J persists through canonical combo state, reaches frame285, spawns OID33, and shows authored hidden pic999 then visible frame242/pic1 bound to formal `c/nar/ncl.png` at 79×79. Record exact ticks and source fingerprint. Shut down by the existing ordered owner with `Stopped` and zero active pool borrowers, capture process exit code. A failure must write a terminal report and exit nonzero. A single representative is intentional: it tests Player portability of shared input/spawn/sprite plumbing, not all characters or whole-session parity.

Rollback: remove only the new diagnostic script and its meta after recording a failure; leave existing build/test infrastructure and user files untouched. No computer-use.

Acceptance: `artifacts/diagnostics/NTSD28-Q07-WINDOWS-PLAYER-NATURAL-SKILL-001/ACCEPTANCE.md`. Build-1 failed on probe-only CS0165, build-2 passed but probe direction-state expectation was reversed and run exited 1; both failures are retained. Corrected build-3 compiled with 0 errors and the built Player exited 0/PASS. This closes only the Windows Player Naruto representative, not Q07 aggregate or formal EXE same-tick parity.
