# Q07 Hidan natural-input Unity driver raw comparison

Status: `VERIFIED` for 11 shared raw fields in original Editor, two 40-tick cases. Parent Q07 remains `IN_PROGRESS`.

The original Unity Editor compiled the strict natural Hidan schema, consumed formal staged OID24 DAT, and completed X580 and X1200 request-file raw captures with `PASS` and 40 tick rows each. The fixture starts both characters at action0/HP500/PP300 and applies zero-based carrier keys J on rows0–1, K on rows2–3. In this project the crossed carrier maps J to physical attack and K to physical jump, matching the source/root EXE's attack completed ticks1–2 and jump ticks3–4.

Independent comparison to the formal root EXE natural-input trace found no difference among 11 shared fields on either path: actor/target action, actor/target current PP, target HP and actor/target X/Y/Z. X580 matched 440/440 tick-fields, X1200 matched 440/440, 880/880 aggregate. The X580 action249→236 path and tick13 HP/PP values match; X1200 stays outside the catch path. This does not claim equality for Unity catch relation slots because the raw-capture schema omits actor CaughtSlotIndex and target CatchSourceSlot90. A direct focused full-driver relation test is the next narrow gate.

The raw header still says `certificateEligible:false`. Two `PASS` results and selected-field equality establish the described 40-tick diagnostic, not physical-key Battle Play, complete state or pixel parity. The original Editor DLL was rebuilt after the exporter source, checked Console showed no C# compile error, and protected Scene/config files were not edited by this package. Only the Editor diagnostic schema and two new JSON fixtures were written.
