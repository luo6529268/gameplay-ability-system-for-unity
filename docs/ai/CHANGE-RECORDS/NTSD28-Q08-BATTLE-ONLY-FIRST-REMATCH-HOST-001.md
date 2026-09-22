<!-- CHANGE-RECORD
id: NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleOnlyRematchRedPlayModeTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleResultHostPlayModeTests.cs
authority: formal Logan first battle-only result rematch; source RNG and second-cycle witnesses
evidence: direct Scene Play2/2, revised diagnostic1/1, ordinary3/3, full SelfCheck PASS; FIRST-REMATCH-HOST-ACCEPTANCE-PENDING.md
-->

# NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001

Created before production script edits. Task: `docs/ai/TASKS/NTSD28-Q08-BATTLE-ONLY-FIRST-REMATCH-HOST-001.md`. Before: direct Scene bootstrap owns initial roster but has no re-entry; driver sends all command2 results to ordinary AppManager requiring Menu+Battle; direct real Play leaves the same World frozen. `RecreateWorld` alone makes an empty World and its `true` map-cleanup parameter does not prove the Scene carrier is clear. After: driver has an explicitly registered direct result owner, direct bootstrap performs ordered shutdown/real map cleanup and full same-scene reconstruction with restored random/phase then BGM draw. Existing ordinary selection route is preserved. Invariants: no new tick/spawn after Stopping, no World recreation on failed shutdown, no orphan renderer/pending task, no nonbattle/Scene/asset change; one direct rematch maximum. Expected side effects: old renderer borrowers returned, old World destroyed, roster and presentation rebound to new World, host tick restarts. Unknown: second result ordinary selection when no Menu Scene; track separately and do not claim Q08 exit from first-rematch acceptance. Validation and rollback are in Task; all previous diff remains user work and must be preserved.

Actual: `SimulationTickDriver.cs` registered `IBattleOnlyResultHost` dispatch; `BattleTestBootstrap.cs` direct-scene owner, ordered shutdown/map cleanup, World/services/roster recreation, random/phase restoration and BGM draw; new Play test extended with synchronous random/close check; old pending-command diagnostic rewritten for its now-correct direct-owner expectation. Four exact paths only. Pre-change target RED retained. Isolated D3D11 real Scene focused Play2/2 plus revised diagnostic1/1, adjacent ordinary3/3 and full SelfCheck PASS, Scene hashes unchanged. Full command/XML/log evidence and limitations: `artifacts/diagnostics/NTSD28-Q08-RESULT-TRANSITION-HOST-AUDIT-001/FIRST-REMATCH-HOST-ACCEPTANCE-PENDING.md`. Natural KO/formal frontend/second-result no-Menu selection not verified; hence `RUNTIME_PENDING`, not Q08 exit. No script rollback performed.
