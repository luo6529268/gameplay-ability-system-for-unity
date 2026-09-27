# Q09/P-13 Han earthquake current-content paired Session diagnostic

Status: `VERIFIED_SCOPED_DIRECT_FRAME_CONSUMER` on 2026-09-27. This closes only the current-formal-DAT frame150-to-render-snapshot consumer gate. Natural skill entry, root formal EXE same-frame pixels, Unity project-background consumer, P-13, Q09, BATCH-05 and the master alignment goal remain open.

The formal root EXE SHA-256 was rechecked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; it was **not executed** by this diagnostic. The tool compiled against the paired playable `GameSession28`/`selection_flow.cpp` plus 28 core translation units, with both Session roots pointed at the read-only formal `resources/runtime`. The current formal `decoded_dat/c/han/han.dat` defines frame149 state9/next150, frame150 state55052/wait1/next151, frame151 state55050, and the alternate 264→268→269/hit_g265 branch. `NativeEarthquake28::step` runs after the full simulation step and `GameSession28::snapshot` publishes only a background offset.

The first g++ build failed because the diagnostic read `DatDocument::object_id`, a field that does not exist. After correcting that line to `EntityState28::object_id`, the v2/v3/v4 builds exited 0. No formal or Unity production source was changed. Each diagnostic invocation refused existing output and wrote its own TSV/log.

| Controlled initial action | Observed complete Session result | Outcome |
| --- | --- | --- |
| 149, first run | Tick1 already action0; 24 ticks no 150 or quake | Exit8, negative precondition evidence, `controlled-han-v1.tsv` preserved |
| 149 with tick0 row | Tick0 authentic 149/state9; tick1 action0; no quake | Exit8; action149 alone lacks the CPoint relation/context needed to persist |
| 264 with tick0 row | Tick0 authentic 264/state15; tick1 268, tick2 269; through tick24 remained 269 without `hit_g:265` | Exit8; grounded transition not reached by this fixture |
| **150 direct-frame control** | Tick0 authentic 150/state55052 with offset0; tick1 remains 150 and publishes owner0/background `(2,0)`; tick2 action151/state55050 and publishes owner0/background `(0,0)` | **Exit0; current DAT→full Session→render snapshot consumer verified only** |

The direct-frame successful TSV SHA-256 is `E1EEAD83FC6B50F6674271286DCDED03154FDC5EF1801288BE23854C2F1C3EBB`; final diagnostic source SHA-256 `2E40E8E71263D358BAA1F506F8EF432C22D56C0856F6D64C71BDEC206EEC3FA3`, v4 binary SHA-256 `9D788AD57EEB2CB42B70AA13DBD619D2E12070C3BFFBB71BADC1BFFB76388AB9`. The three failed TSV hashes are `1ABB948089006705C79B55E2B77D6ECA0C9BB8095B8D9CF66F248493C6446DC9`, `7B2BE8190073F0BBA389A431DF4B01579C32F22E8D21F2E8D8C6B9256D23C9D6`, and `588AB0E5B107DCA31C34A37E8808FB445C69494FF73C7CB9230BBE027B823C61`. These RED outputs are intentionally not erased or reinterpreted as natural skill passes.

The direct-frame test begins after Han has already been assigned frame150, so it proves neither a physical action0→149/264 path nor the missing CPoint/grounded preconditions. It does prove that the current formal Han DAT's 55052/55050 state is consumed by the paired playable Session and handed to a background-only render snapshot. The Unity project currently lacks the corresponding background visual consumer; adding one must respect the user's own background, fixed camera and D-024 exception, and must not move entities or global camera. A future Task needs natural formal reachability and a focused Unity background-versus-entity Play/pixel check before P-13 is closed.

No Unity script, DAT, image, WAV, Scene, camera, mode asset, project setting or nonbattle file was modified. The saved Menu/Battle Scene SHA-256 values remained `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` / `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`. No Unity compile, SelfCheck or Play was run for this source-only diagnostic.
