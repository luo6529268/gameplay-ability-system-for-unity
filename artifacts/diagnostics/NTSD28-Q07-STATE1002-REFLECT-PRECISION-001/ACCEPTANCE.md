# Q07 state1002 reflect precision SelfCheck acceptance

Formal playable `battle_world.cpp::apply_native_reduced_attacker_post_hit` sets a state1002 attacker's X motion to `-target.pending_hit_impulse.total.x * 0.5`. The ordinary reduced `dvx=5` fixture's target impulse is 2.5, so formal X is -1.25; Y is -4 and attacker Z=6 is divided by -1.5 to -4. The existing Unity production path has the same formula. No DAT value or production rule was changed in this package.

After the preceding half-dvx SelfCheck correction, the original Editor's full `BattleRuntimeSelfCheck` failed at the combined state1002 assertion. This package first preserved that predicate and added actual-value output. Diagnostic RED measured `frame=3, vx=-1.25, vy=-4, vz=-4`; raw result is `original-editor-selfcheck-diagnostic-red.result` (SHA-256 `EB3A6969A253EF5F79472BB6DD81E979C2ADE9BB982632C19A6C09CBF12072C1`). Thus only its historical Vx=-1.0 expected value differed. The exact test expectation changed to -1.25; the actual-value message remains for future failures.

After an original Editor refresh, the full `BattleRuntimeSelfCheck` request was consumed and returned **PASS**. Raw result: `original-editor-selfcheck-after-reflect-fix.result` (SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`). The Editor assembly postdates the test script edit, and the recent Editor log tail has zero C# errors. Four protected Menu/Battle Scene and config hashes remain unchanged. This is full SelfCheck evidence for the current original Editor version, not natural Battle Play, root formal EXE same-world parity, all R returns or Q07 completion.

Final governance checks: `Tools/Validate-ChangeLedger.ps1` exit0 and `git diff --check` exit0.
