# Q07 N30 original-Editor complete Driver witness

Status: `VERIFIED_SCOPED_DIAGNOSTIC`. This closes the diagnostic, not Q07 or D-024.

The original Unity Editor compiled the narrow raw-exporter schema with no C# errors. Two 20-tick requests ran sequentially through its existing complete `SimulationTickDriver` capture route and both returned `PASS`. They used the selected formal content, two Naruto OID2 characters at X500/X1200, seed `0x28A55A5A`, mode0/background23 and four physical-key edges at ticks2/4/6/8. The corresponding formal root EXE SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; its paired-source LFR replay reports `passed:true`, 20 trace rows and `nativeParityClaim:false`.

| Physical input | Formal root tick8 | Unity original-Editor tick8 | Selected first difference |
|---|---|---|---|
| L-K-L-K (Defend-Jump repeated) | actor action110, input history `[-1,9,0,9,0]`, OID998 count0 | actor action110; same history through tick7, but at tick8 history `[0,0,0,0,0]` and active OID998 born in slot50/epoch1 | Tick8 history clearing and extra OID998 birth. Unity retains the extra entity through tick20. |
| J-L-J-L (Attack-Defend repeated) | No OID998 | No OID998 | No difference in the selected actor action/history/OID998 count over 20 ticks. |

This corrects the earlier inference from internal key labels that Unity numeric `9,0` necessarily meant a different physical sequence. In the complete Driver, L-K-L-K yields the same selected input history as formal through tick7; the late producer, not a different input mapping, creates the tick8 divergence. The formal trace and Unity raw/domain-v2 traces provide this selected-field conclusion only, not whole-state, pixel or every-character parity. Formal stage23 maps to the project's existing Z542 runtime boundary; there is no Scene or DAT edit here. The production call site is `LF2Entity.RunLateTailBeforePrevFrame()` -> `RunLateCharacterDatInputTrigger()`, which clears the history and immediately creates OID998 for all three legacy N30 patterns. The paired playable production simulation has no corresponding N30 input-birth route. A separate governed production change must retire this caller generically, then rerun these two inputs.

The Menu/Battle Scene SHA-256 before and after the two original-Editor captures stayed `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Git Scene diff is empty. No second Editor or computer-use was used.
