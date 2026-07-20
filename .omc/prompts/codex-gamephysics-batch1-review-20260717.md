# Independent review: GameTick / Physics parity batch 1

Review the production changes for GT-01, GT-02, PH-03, PH-04, PH-05 and PH-06.
The only authority is `J:\QQFile\NTSD2.4\ntsd_release_C#`. Do not read, cite,
or infer behavior from C++, disassembly, pseudocode or legacy implementations.

Authority entry points:

- `src/BattleCore/Simulation/GameTick.cs`
- `src/BattleCore/Frame/FrameAdvance.cs`
- `src/BattleCore/Frame/Physics.cs`

Audit report and Unity files are supplied as context. Verify exact ordering and
field semantics, especially:

- NeedClearInput ordering and early whole-tick return;
- whether current, previous, cooldown, combo and history input fields are reset
  at the same boundary as authority;
- per-active-slot key clearing immediately before frame advance while retaining
  previous-edge state;
- double constants and no implicit float arithmetic;
- landing damage retaining negative HP/HpMax-equivalent values;
- oid999 exact-ground versus crossed-ground frame 101;
- no unauthorized WeaponState writes during weapon landing;
- real Character, shared Character-DAT and transformed current-DAT routing;
- whether focused tests could pass while production behavior is still wrong.

Review only; do not edit files. Lead with severity-ordered findings grounded in
exact file/line references. If there are no blockers, explicitly state PASS and
list residual test gaps. Write the review to the output file.
