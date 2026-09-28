# Q09/P-13 Han catch-to-earthquake full Session witness

Status: `VERIFIED_SCOPED_PAIRED_SESSION_CATCH_TO_BACKGROUND_SNAPSHOT` on 2026-09-27. This closes only the selected **current formal DAT + paired playable source** midcombo catch-to-background-publication gate. It does not close natural physical action0 input, formal root EXE pixel/device output, Unity project's background-only visual consumer, P-13, Q09, BATCH-05 or the master goal.

Authority identity: root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` rechecked; the root EXE was **not executed** by this diagnostic. The compiled source is the paired playable GameSession/Selection plus 28 core translation units. Both Session roots point to the read-only formal `resources/runtime`, which supplies current `catalog.csv`, `decoded_dat` and `vfs`. Han OID726 frames146–148 contain kind-3 `catchingact:149 caughtact:130`; the target action is facing-sensitive and the observed Lee OID7 endpoint uses action131, whose CPoint is also kind2.

Controlled initial condition: mode0/background23/seed2833, Han action147/X500/Z650 and Lee action0/X520/Z650, no subsequent player inputs, 24 full `GameSession28::step()` calls. No code assigns catch relation slots or forces action149/150. The initial state is **already at action147**, so this is not a natural keyboard sequence from action0.

Observed rows from `controlled-han-147-near-v4.tsv`, independently parsed as 25 rows including tick0:

| Tick | Han action/state | Lee action | Reciprocal slots | Published background offset |
| --- | --- | --- | --- | --- |
| 0 | 147/15 | 0 | -1/-1 | (0,0), no owner |
| 1 | 149/9 | 131 | 1/0 | (0,0), no owner |
| 7 | 150/55052 | 131 | 1/0 | owner0, (+2,0) |
| 12 | 151/55050 | 185 | 1/0 | owner0, (0,0) |

The first v1 compile passed with 28 core units. Two initial executions exited 4 due to incorrect fixture roots (`vfs` and then `decoded_dat` used as a constructor root), and their logs remain. With both roots corrected to `resources/runtime`, the v3 run completed the 24 steps and generated the **same TSV bytes** as v4, but our assertion returned 8 because it incorrectly required Lee action130. Paired `native_relation_action` selected 131 for the observed facing; current Lee frame131 has kind2 CPoint. After the Task/Record correction and one-line assertion fix, v2 compile exited 0 and the v4 run exited 0. The old direct-frame150 mode also exited 0 with the new binary. The v3 false negative and both root errors are retained; no formal/Unity behavior was changed to make the check green.

SHA-256: diagnostic source `BFFFC688ABDDAB2F83F83A90F65F410B8CC1D65612CC9FD8BE990B5AA30DC733`; v2 diagnostic EXE `86C7980690ED4A93C92E9A209B7A063A7FE548A058B826088E61EC8BFC548F75`; v3 false-negative TSV and v4 passing TSV both `8F0B771C1367C33D3E0D177653EDE7DE489FF925ED06487FF5B224C7ECF5DAC6`; direct150 regression TSV `825887D97D7EDA4BA4F98B39FE56D09F0E1BB2940583286F2BCF511FEF1FDCCE`.

Only `Tools/NTSD28Q09Diagnostics/han_earthquake_session_trace.cpp`, this package's evidence and governance documents were modified. No Unity code, DAT, PNG, WAV, Scene, camera, mode asset, project setting or nonbattle feature changed. Menu/Battle Scene SHA-256 remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0 (`PASSED`, 936 records, one governed code path covered by this and the predecessor diagnostic record); `git diff --check` exited 0 with line-ending notices only. Unity compile, SelfCheck and Play were not run for this Tools-only source package.

Next P-13 gate: obtain the formal root's natural action0/input route and same-tick visible background output, then verify the project background's visual offset without moving the fixed camera, entities or logic truth. A Unity implementation must use the project's own background and preserve the user's exclusion of original background/mode DAT.

The read-only Unity owner/seam mapping is in `UNITY-CONSUMER-BOUNDARY.md`: current pass descriptor exists, but a producer and project-background consumer were not found in the searched production paths; the project camera derives its neutral frame from background SpriteRenderer bounds. This constrains the next implementation Task without changing Unity code.

Read-only natural-input candidate: current formal Han DAT declares `hit_aj:145` on several preceding attack/weapon frames (for example `han.dat:197,204,218,232,615`); frame145 advances through 175→174→146, and frame146–148 can establish the demonstrated catch relation. This identifies a possible action0-to-145 input route to test with actual tick inputs; the DAT edge alone does not prove the root EXE accepts that sequence in a real battle. No natural-input PASS is claimed.
