# Q07 Naruto DDJ original-Editor state witness

Status: `FOCUSED_PLAY_PASS / SAME_STATE_NOT_ESTABLISHED / Q07_OPEN`.

The already-running original Unity Editor (PID 11944) recompiled `BattleComboPlayModeProbeEditor.cs`; `Assembly-CSharp-Editor.dll` UTC 04:29:46 is newer than the edited source UTC 04:29:05. Its unique `ddj-state-20260925-5` request was consumed in `NTSD_Battle.unity`. The preserved JSON reports PASS: physical L/S/K requests Naruto frame 271, resolves action 272, reaches full-tick frame 495, spends PP 500→150, and first spawns one OID518 at tick 16. The project-mode content fingerprint was `27CAE01489909C46A5145A5867988CCD8C10E20FEF165D847B7AC7A6DE2DE02D`, matching the earlier original-Editor formal DDJ PASS and Q07 project-mode readiness. The former hardcoded `B8B13894...` preflight represented a different native-mode composite and was incorrect for this approved project-mode Asset route. Only the new diagnostic branch's predicate changed; DAT and production runtime were untouched. Prior FAIL results -1, -3 and -4 remain preserved, as do consumed request archives; request -2 was archived pending and never claimed as a result.

The new witness resolves the prior unknown initial-state fields but shows that the formal release playback and Unity Play do **not** start from the same world:

| Field | Formal release action110 trace tick 0 | Unity `-5` pre-input capture | Classification |
| --- | --- | --- | --- |
| Participants | OID2 Naruto / OID7, two entities | two captured actor slots OID2/OID2; world object count 4 | Different roster and object count |
| Actor action | Naruto action110 | Naruto action0, then frame110 observed at tick2 | Different starting action |
| Positions | Naruto (500,0,350), other (1200,0,350) | Naruto (800.746521,0,478.461670), other (1412.746582,0,478.461670) | Different positions |
| RNG | CRT 3374725112, calls3000, table hash 58181f48cc1f3bb5 | CRT 3878484156, calls3000, table hash 0713A19CF87F90DE, shared state1314149188 | Different RNG state/table; identical call count only |
| Mode/map | battleMode0, formal background23 | battleMode0, project backgroundId -1; Scene map is Sunagakure | Mode value equal; background/map authority differs and original background DAT is user-excluded |
| Input | formal packet route, native masks and phase | physical L/S/K queued before ticks0/2/4, release at6; raw Unity proxy arrays/masks | Numeric mask equality unknown: producer/remap/index semantics and capture phase differ |

Unity sampled ticks `2,4,5,...,26`: 24 rows, with missing observed ticks 1 and 3. `missedObservedTicks=2` and `continuousTickCapture=false` are correctly reported. The previously established 26-row formal source/release and scoped Unity milestone comparison remains valid, but this Play is not a same-state or continuous consumed-input proof. A further tick-boundary witness would need a separate governed task; this diagnostic does not justify changing the saved Scene roster or user-excluded background/mode DAT.

The Editor returned to Edit mode after the result. The Battle Scene SHA-256 remained `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, and GameConfig asset `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, equal to the pre-Play baselines. Renderer borrower count was not exposed by this probe and remains unverified; Editor Edit-mode return alone is not zero-borrower proof. This scoped witness does not close Q07 or the overall alignment goal.

Focused governance checks after the current documentation update: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repository>` exited 0 with 803 Records and eight governed code files in the current shared diff (`Temp/NTSD28-Q07-DDJ-ledger-validation-20260925.log`); `git diff --check` exited 0. The three restored live progress documents have zero NUL bytes; their original `*-corrupt.bin` snapshots and exact v3 candidates remain available under `NTSD28-Q07-PROGRESS-DOC-RECOVERY-20260925`. These checks do not substitute for the missing same-state or borrower evidence.
