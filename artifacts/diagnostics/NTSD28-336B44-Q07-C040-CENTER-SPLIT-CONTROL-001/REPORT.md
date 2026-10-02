# C040 current-center / vaction-CPOINT split control

Status: `VERIFIED_SCOPED_FORMAL_PASS_AND_UNITY_WRITER`; parent C040/Q07 remains open. Authority identity: selected formal root `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`, corresponding playable `BattleWorld28::settle_catch_relations`, and current formal Hinata/Bee DAT. The diagnostic binary is built from the current playable source closure and is not the formal root EXE.

The previous controlled Hinata125/Bee132 certificate separated the current Bee CPOINT from vaction130 CPOINT but both frames had center (39,79). This continuation adds the current formal Bee137 frame: center (39,71), kind-2 CPOINT (33,68), against vaction130 center (39,79), CPOINT (41,39). The same reciprocal relation is tested with victim hold5 and hold0. The four formal pass rows are:

| Bee current action | Hold before pass | Bee action after | Center Y selected | Expected Y | Actual Y |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 132 | 5 | 132 | 79 | 1 | 1 |
| 132 | 0 | 130 | 79 | 1 | 1 |
| 137 | 5 | 137 | 71 | -7 | -7 |
| 137 | 0 | 130 | 79 | 1 | 1 |

All four rows also passed formal active/synchronized relation and exact XYZ checks. `run-01/held-pose.csv` and `run-02/held-pose.csv` have identical SHA-256 `B4F5412D0F1E85C91DB7F88D47C17BAEDB50F9153D5131AA83B885F44F1CF6BA`. Both probe processes exited 0. The original Bee132 two rows, including CSV header, are byte-identical to the earlier certificate. Source probe compilation with the current playable closure exited 0; exact argv and output are preserved in this directory.

The unchanged Unity production `BattleCpointWriter.SyncHeldPosition` was tested through the original Editor using a center-divergent synthetic frame pair corresponding to the source rule. New named EditMode job `b43caddb2f2449c69e5db151233aaa8e` passed 2/2: positive hold retained current center Y71 and projected Y206, zero hold selected vaction center Y79 and projected Y214; both cases also checked source-rule X/Z and projected X/Z. The existing two held-pose cases plus eight adjacent preflight tests passed 10/10 in job `d4719bbd58d2473698e00acc6d749c5c`. Raw job responses are saved as `original-editor-center-two-pass.json` and `original-editor-adjacent-ten.json`. Generated `Assembly-CSharp-Editor.csproj` build passed with 0 errors/253 warnings. The original Editor compiled the new tests, returned idle/non-Play and kept Battle Scene clean; Battle Scene SHA-256 remained `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`. Menu Scene and project mode asset SHA values also remained unchanged.

This controlled pass and focused writer test independently check center ownership and CPOINT ownership. They do not prove that a normal input sequence reaches positive victim hold in the formal root EXE, that Unity's full Driver reaches it in the original Battle Scene, or that all rendering/physical projections are aligned. No production code, DAT numeric value, Scene, image, audio, non-battle path or existing evidence was removed or changed for this package.
