# Input combo regression architecture review

Review the current Unity input implementation against the authoritative C# input runtime.

Authority:
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs`

Unity files:
- `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs`

The reported regression was that ordinary WSADJKL and all combos stopped working. Ordinary keys were previously restored, while combos still failed because local held state was incorrectly used as the previous-key baseline. The current patch reads runtime Key* as the baseline, keeps the local held snapshot, reapplies fresh cooldown edges after frame advance clears runtime keys, and restores local-shadow combo commit order.

Verify with exact source citations:
1. Held local keys form fresh edges after runtime Key* is cleared by frame advance, matching authority.
2. Defense/direction/action combinations can be recognized on one logical tick and are not split or suppressed by the adapter.
3. DJA early-return paths preserve the authority's local-shadow commit semantics.
4. Ordinary single-key input is not delayed or swallowed by combo processing.
5. Tests actually exercise these contracts and do not encode a non-authoritative behavior.

Do not modify files. Report findings ordered by severity. If no blocking findings exist, state that explicitly and identify any remaining Play Mode-only risk.
