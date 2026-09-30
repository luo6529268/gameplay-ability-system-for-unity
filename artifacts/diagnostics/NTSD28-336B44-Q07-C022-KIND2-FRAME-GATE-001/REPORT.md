# NTSD28 336B44 Q07/C022 current-frame kind2 gate

Status: `UNITY_FULL_TICK_PASS / RUNTIME_PENDING`. Parent: BATCH-04/Q07. This report is scoped to the current action/counter gate, not C029 physics or all of Q07.

Authority: formal root EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` (rechecked this turn). Playable source `source/ntsd28_core/src/simulation/battle_world.cpp::BattleWorld28::step_frames_range` returns a held frame event before action/counter mutation when the current first CPOINT is kind2. Current source `battle_world_tests.cpp::test_real_kind2_cpoint_holds_physics_and_frame_in_one_tick` supplies OID52 Naruto(1T) action130 state1700/kind2 and a reciprocal holder, and expects action130/counter1 after one tick. This source test is live-path implementation evidence, not a same-state observation from the formal root EXE.

Content: formal `resources/runtime/decoded_dat/c/nar/kyu.dat` and staged `Assets/NTSD/Content/LoganRuntime/decoded_dat/c/nar/kyu.dat` both hash `F1313A21EC21CB3E29C5DAEC73D3CBA7D18C7EE1891A680777E68A51D7AA88EC`. The three declared frame-sheet PNG paths `c/nar/{kyu,kyuu,4tk}.png` and three character/UI PNG paths `sprite/face/kyubi_f.png`, `sprite/small/kyubi_s.png`, `sprite/smallb/kyubi_s.png` also have staged/formal SHA-256 equality (six of six). This is a selected Q01 content check, not a global content audit. An initial `rg --files` missed ignored DAT files; direct path/hash checks supersede the mistaken absence inference.

| Declared PNG | Matching formal/staged SHA-256 |
| --- | --- |
| `c/nar/kyu.png` | `F7A829966A9B003BC53D7817409E4525D1C32BA8F0080633099D811A5351A580` |
| `c/nar/kyuu.png` | `B5320D3BF91C1177EA8F01533EB9F5552EE7806FF92E7C22C374EB0524C55BB9` |
| `c/nar/4tk.png` | `68EDFFC3BD492F94A34A3DDB09189DD974B9FA1E5F50F790E0A0E9F618B2F090` |
| `sprite/face/kyubi_f.png` | `747110C3C39021DABAF9C8F20CBAF7D179719309C0F32578038C7E8F3C72ECEE` |
| `sprite/small/kyubi_s.png` | `4000E195FE56799933C89C28183BD13A07BE6319798FB5BBC9192FD0EA2788BB` |
| `sprite/smallb/kyubi_s.png` | `3C189690C199C4D8812D5E3A1507B93762BAF27823DDF5135C6A6C6F0501AAFB` |

Original Unity Editor exact test jobs:

| Job | Observation | Interpretation |
| --- | --- | --- |
| `3af69fb0ecbf4e87a9dc586a6cac3389` | State10/no-kind2 control passed; OID52 action130 without holder became212. | The incomplete fixture triggered existing orphan relation cleanup; not evidence against the frame gate. |
| `47bc7efe33fb42e283bc9e2f4e91c836` | Reciprocal holder supplied. State10/no-kind2 control passed; OID52 remained action130 but `AttackingCounter` incremented1→2. | Confirmed Unity full-tick first difference. `RunNativeC25FrameBodyForWorldPass` routes native mode directly to `RunNativeC25FrameTransaction`, where the general kind2 early gate is absent. |
| `f29a923b6c7f4e27bfd0b4ea35c476fd` | Type3 native-body current kind2 preserved HP20/action0 but incremented counter1→2; its no-kind2 control was not reached. | Confirms the shared frame transaction gate is missing beyond characters. Fix one common gate, then verify the full positive/negative sequence. |
| `aa72aced155f4f7a9e08747292232424` | Post-fix OID52/holder and state10 control passed; type3 kind2 passed, but no-kind2 HP expected16/actual20. | Fixture used CLR character fallback type0; merely setting runtime.ObjType did not establish type3. Corrected with the existing explicit typed-character test pattern. The initial bridge request timed out, but the same live job ID was recovered from Editor state and polled; no duplicate run was started. |
| `06938512781d4430aa5cadcf6b8058e7` | All three exact cases passed 3/3 after type3 fixture correction: OID52 action130/counter1 held in complete tick; state10/no-kind2 counter advanced1→2; type3 kind2 HP20/action0/counter1 held, no-kind2 HP20→16/counter1→2. | Bounded Unity full-tick and native-body GREEN for the current source rule. |
| `78a2bd40f0be447ebf7dca9e09c0b9d1` | Four exact adjacent C25G cases passed 4/4. | No observed regression in terminal state14, type3 state3007, type2 grounded heavy-weapon, or ECS wrapper ownership. An earlier wrong-namespace job `f0fd19671e7b4040adf0baf553ee96d2` selected 0 tests and is excluded. |

Implementation: one early current-frame first-CPOINT kind2 return was added in `LF2Entity.RunNativeC25FrameTransaction`, before type3 HP drain, frame sound, counter and next-action writes. The legacy/non-native branches already had kind2 gates; no DAT/Scene/asset or unrelated battle rule changed. Original Editor runtime and Editor DLL timestamps followed the source/test edits. Change Ledger validation passed (1036 records, 83 governed code files in the dirty diff); scoped `git diff --check` passed. Battle/Menu Scene, GameConfig and ProjectBattleModeConfig SHA-256 matched their prior protected values.

Remaining: formal root EXE same-state trace, natural Battle Scene Play with valid OID52 grab and C029 physics remain unverified. The selected Unity evidence and current source test support the scoped mechanism, but do not close C022 or all of Q07. No full-suite claim.
