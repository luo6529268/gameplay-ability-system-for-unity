# Audit6 transformed-human input review

Act as an independent architect/code reviewer. Read only; do not modify files.

The sole gameplay authority is `J:\QQFile\NTSD2.4\ntsd_release_C#`.

Review the current Unity implementation of transformed human input polling against:

- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\SimulationTickDriver.cs`, especially `ApplyFrameInput`.
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs`, especially `ApplyCharacterInputPass`.
- `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs`, especially `PollHumanInput`.

Unity files:

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`, especially `CheckAudit6InputPhaseOrder`, `BindHumanRosterSlot`, and `TransformedHumanInputSelfCheckCharacter`.

Verify these claims:

1. Initial roster discovery remains strict enough not to bind arbitrary entities.
2. An already-bound active human remains bound across current DAT and ObjectId changes, using runtime slot/stable id.
3. Human input polling continues while current DAT is non-character.
4. Character action application remains gated by current Character DAT.
5. The focused test proves press, release, previous-key rolling, cooldown ticking, history, no action while transformed, and no stale edge after restoring Character DAT.
6. No non-roster or AI entity is newly polled.

Report findings first, ordered P0-P2, with exact file/line references. If no defect is found, say so explicitly and list residual test gaps. Do not accept compile/self-check as proof beyond their covered assertions.
