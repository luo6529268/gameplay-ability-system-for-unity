# NTSD28-USER-D024-AI-CHILD-SCENE-PLAY-001

Status: `RUNTIME_PENDING`. Probe script compiled in the original Editor, but first request was rejected before Play because the active Menu Scene is dirty. See Change Record. D-024 non-perceptual verification before Q07 resumes.

Authority: formal root NTSD2.8-Logan EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, its playable `NativeAi28` target selection and late OPoint spawn call chain, and user-approved full-background physical displacement ratio without DAT edits.

Pre-change evidence: original-project EditMode formal-content controlled Driver human12/AI3 test 2/2 proves source-carrier continuity, target selection and physical/source X ratio. Extending AI to tick4 attempted a late OPoint, but the isolated replay fixture has no Renderer pool and failed at `LF2ObjectPool.Get`. Existing real Play pooled Renderer test directly exercises a formal late OPoint birth, not the AI decision through full Driver. Natural Menu-to-Battle AI child birth remains unproven.

Declared script path: `Assets/NTSD/Scripts/Test/Editor/NTSD28D024AiChildScenePlayProbeEditor.cs` only. Create a request-driven original-Editor Play probe that starts from the saved Menu scene, uses current formal `GameConfig` root, creates one human and one AI battle participant through `AppManager.InitializeBattleAsync`, steps the production Driver with the real pool, records target/source/physical child state and ordered shutdown, then exits Play. Before entering Play, reject a dirty Scene. No DAT, Scene, Prefab, menu code, production battle code or camera changes.

Acceptance: exact request/result lifecycle; confirm formal content identity and scene boundary; AI target selection in full Driver; if a child spawns, prove source carrier, raw source-local birth, scaled physical output, Renderer and no pool borrower leaks after shutdown. A no-child run is an evidence gap, not a parity pass. Verify original Editor compile, a fresh result, Scene disk SHA unchanged, focused ledger validation and diff check. Do not call direct late-OPoint Play proof equivalent to natural AI chain. If the existing runtime cannot reach this with no production code changes, preserve failure and define the next bounded work rather than manufacturing a special gameplay rule.

Rollback: remove only the new probe and its `.meta` after reviewing provenance; retain user work and all existing Play probes.
