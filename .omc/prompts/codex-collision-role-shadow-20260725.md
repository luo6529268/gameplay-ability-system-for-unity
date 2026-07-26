Role: collision architecture reviewer, read-only analysis.

Review the newly completed default-off role-aware collision shadow in:
`Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
and its focused tests:
`Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs`.

Authority is:
`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\CollisionCollect.cs`
plus called geometry/role helpers.

Current formal 1000 dispersed broadphase still reports pair peak 184181 and CandidateCollect ~39.72 ms.

Determine:
1. whether the shadow pair set is conservative and authority-complete for current/Prev2 frames, kind=5, invalid/degenerate/unbounded geometry, suppressed/dormant/pending entities;
2. whether diagnostics compare against the correct formal and accepted pair semantics;
3. the minimum large-sample parity instrumentation needed before switching formal consumption;
4. the safest way to run shadow parity without adding an O(N^2) brute reference that distorts the stress timing;
5. exact blockers to enabling role-aware broadphase formally.

Do not edit files. Return severity-rated findings and a concrete validation gate.
