# NTSD full battle parity certificate architecture review

Act as the independent architect reviewer. The only behavioral authority is
`J:\QQFile\NTSD2.4\ntsd_release_C#`. Do not read, cite, or infer behavior from
any C++ project, binary disassembly, pseudocode, or legacy implementation.

Review the current full battle parity strategy and implementation skeleton:

- `Tools/NTSDParity/README.md`
- `Tools/NTSDParity/AuthorityTraceCommand.cs`
- `Tools/NTSDParity/TraceCompareCommand.cs`
- `Tools/NTSDParity/DataAuditCommand.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs`
- `Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs`
- `Assets/NTSD/Scripts/Simulation/NTSDEntityRuntime.cs`
- `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`

User goal: same input, same seed, compare every 30 Hz logic tick, and prove all
battle behavior observable in Unity matches the formal C# project even when the
framework implementation differs. T8 default `stage.dat` deployment remains
explicitly deferred and must not block the non-stage certificate.

Identify concrete schema/runner/certification gaps which could yield a false
positive. Check at least: fixed 400-slot identity/lifecycle, full entity runtime
state, DAT manifest separation, input edge/history, RNG state and call count,
arest/vrest, ownership/link/target/holder, hit candidates and pass boundaries,
stats, queued spawns/destroys, sound/event timing where behaviorally relevant,
world flow/toggles/bounds, render-observable attachment/sorting/visibility, and
scenario coverage. Distinguish required deterministic logic fields from Unity-
native presentation fields that need separate Play Mode evidence.

Do not edit source. Write a severity-ordered, repo-grounded report to the output
file. Conclude with a minimal but sufficient certification gate and state whether
the current implementation is ready to claim full parity (it is expected not to
be ready yet unless evidence truly proves otherwise).
