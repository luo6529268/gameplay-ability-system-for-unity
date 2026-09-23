# NTSD28-USER-RASENGAN-WINDOW-PARITY-001

Status: `FOCUSED_TEST_PASS / PHYSICAL_PLAY_PENDING`. Parent: `NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001`. The user's Rasengan-to-Spiral-Shuriken input report has priority over new Q07 work.

Authority: formal NTSD 2.8-Logan EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable/core source, and its runtime DAT. The rebuilt source-model sweep in `Temp/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/` is diagnostic evidence, not an observation of the formal EXE window.

Observed: source-model Naruto action 241 progresses through 253 at completed ticks 24-25 and 254 at tick 26 without attack. Two-tick physical J begun at scenario tick 25 reaches action 301; beginning at tick 26 misses the transformation. Unity has only a three-tick isolated action-253 witness; the full natural 241-to-253 interval and its first difference are unknown.

Ownership: add only `Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganWindowEditorTests.cs` to drive the existing `NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests` helper against an external temporary, frozen three-tick Stage 23 seed scenario. Output per-tick Unity diagnostics under `Temp/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/`. The seed and outputs are generated diagnostics, not production assets. Do not modify production scripts, DAT, camera, Scene, Prefab, input bindings, unrelated tests or nonbattle code under this Task.

Acceptance: original-project Unity Editor compiles the new test; focused cases capture 32 completed ticks with physical-J canonical mapping for attack starts at scenario ticks 23, 25 and 26. Compare action/counter/MP and raw input state to the rebuilt source-model cases, record first difference or exact equality, and state source-model versus formal-EXE evidence separately. A diagnostic match does not itself close the user's perceived timing problem; real Play and formal EXE observable comparison remain required. Run `Tools/Validate-ChangeLedger.ps1` after the test script edit.

Rollback: remove only this new test script and its own `.meta` if Unity creates one, and remove this Task/Change's registration by reviewed inverse edits; preserve all pre-existing dirty content. No destructive Git commands.

Focused result: original Editor job `c8aa5cebb03e4879aa90beee82ec67f1` executed the three declared cases, 3/3 PASS. Each source-model/Unity pair has 32 complete tick rows; Naruto frame/action/counter/MP agree in all rows, and each diagnostic shutdown reached `ObjectPoolQuiesced` with zero objects/slots/borrowers. Formal EXE visible and natural physical-key Play remain open. See `artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/FULL-WINDOW-COMPARISON.md`.
