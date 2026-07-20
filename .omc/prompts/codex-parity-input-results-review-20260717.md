# NTSD battle parity architecture review

Review the current full per-tick parity effort between the authoritative C# project and Unity.

Authoritative project: `J:\QQFile\NTSD2.4\ntsd_release_C#`
Unity project: current working directory.

Focus on the current tick-2 first difference and the proposed fixes:

1. Authority `SimulationTickDriver.ApplyFrameInput` invokes `InputRuntime.PollHumanInput` before `GameTick.Run`; `PollHumanInput` rolls previous state, writes held state, ticks cooldowns, then applies edges. Unity currently queues a complete frame input set before the tick and consumes it in the post-cooldown human input phase. Determine the correct Unity production contract that preserves LocalFreeRun, LockstepBuffered, Manual/replay, and the existing combo input path without double-consuming edges.
2. Authority world camera fields change, while Unity intentionally uses a fixed-world camera and must not restore player-driven world/camera movement. Review whether a named comparison profile may normalize only cameraX/cameraVel while retaining all entity/world combat fields.
3. Authority `UpdateBattleResultsFlow` writes HadBoth, TeamCount, TeamIds, BattleEndPhase, and PendingWinner during normal combat. Unity trace currently emits constants. Determine the minimum real Unity battle-results runtime required for combat simulation parity while excluding results UI/menu behavior.
4. Inspect the v3 trace/hash/manifest boundary for any way a diagnostic mode could be mistaken for a production certificate. Confirm that production DAT manifest mismatch remains a hard rejection.
5. Identify likely regressions or missing focused tests, with exact file references where possible.

Do not modify files. Produce an evidence-based review with prioritized findings and a clear PASS/FAIL gate for the proposed iteration.
