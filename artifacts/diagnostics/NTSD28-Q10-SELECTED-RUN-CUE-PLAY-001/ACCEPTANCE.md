# Q10 selected Naruto running cues — original Editor Play acceptance (2026-09-27)

Status: `VERIFIED_SCOPED_SELECTED_CUE_PLAY`. This closes only the selected-content Unity event→per-tick sink→pooled voice Play gate. Q10/O-01/O-02/O-03, formal audible parity and the total alignment goal remain open.

Authority and content: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; formal and staged `decoded_dat/c/nar/nar.dat` both SHA-256 `6BE721524C8CCA0E293BEB8D6BF1DFEE306CCB948181BDAA94545EDB29418ED9`. Naruto running frames 9 and 11 declare `data\003.wav` and `data\004.wav`. Both WAV files exist in Unity and formal locations. Whole-file SHA-256 differs (Unity/formal: 003 `A1C815385586C0DF72A9DE01EAAA3091C826DF10D23E93969AEAC8182E455532` / `74885981A2003F1BC1C7F2CD22493C47C4B8059088AEFFAED652EEEF996F9C7A`; 004 `3948B7D3885E2C97589D577F5DE75C7029C99BCBDEDC257661399462AF7B4896` / `4B501F67BEC53F364530E041012FAA40334E7F4F0F82088B6B7CAA472C2207AC`), but a follow-up standard WAV decode found each pair's format and complete PCM payload identical. Exact frame counts and PCM hashes are in `PCM-AUDIT.md`. No WAV was replaced in this package.

Original Editor and input: the existing idle Editor used saved `Assets/NTSD/Scene/NTSD_Menu.unity`; the Editor-only probe was compiled after refresh, then MCP `manage_editor play` and `execute_menu_item` launched the same Menu→AppManager selected `LoganRuntime`→Additive Battle production route. Match seed 2833, P1 human Naruto OID2/slot0, P2 human Lee OID7/slot1, mode0. Probe asserted a Running Driver, formal runtime catalog, Naruto source-rule birth position, enabled production movement action, sealed battle sound catalog and preloaded 003/004 clips. It sent physical Input System `D` tap, release, then held `D`; Naruto moved X620→787. It did not set character position, frame, event, sound cue, DAT or production runtime fields.

Raw `play-result.json` SHA-256 `09A7E8D27F42C027314C339C7AB5CD84BF65A81345AEFA400F95F1EAD37ED3BF`, status `PASS`. The probe's temporary sink copied event observations and forwarded each original batch unchanged to the production `NTSDSoundPlayer`:

| Cue | World event tick | Sink callback tick | Main thread / World event | Pooled voice count | Prepared clip assigned / playing |
|---|---:|---:|---|---|---|
| `data\003.wav` | 8 | 8 | true / true | 0→1 | true / true |
| `data\004.wav` | 13 | 13 | true / true | 1→2 | true / true |

Driver dispatched event count 0→2, player rejected-unprepared count 0→0. Both callbacks were on separate accepted ticks and neither was delayed into a later tick. This Play did not force two successful logic ticks into the same Unity Update; the earlier three-tick focused sink test covers that host timing separately. `AudioSource.isPlaying` and pooled voice counters prove Unity voice start, not the waveform at the physical output device or equivalence to XAudio2.

Cleanup: probe released the physical key, cleared its diagnostic sink, restored its prior entity-stress flag, unloaded its Additive Battle, and recorded `battleUnloaded=true`. MCP then stopped Play; the same Editor returned idle in `NTSD_Menu`. Saved Menu SHA-256 `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13` and Battle SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A` equal their pre-run values. Neither Scene is in Git status. Only the new Editor-only diagnostic script and its Unity-generated `.meta` were added under this Change ID.

Final governance checks: `Tools/Validate-ChangeLedger.ps1` exit 0 and `git diff --check` exit 0 after the script and status records were updated.

Remaining Q10 gates: compare native-versus-Unity scheduling/gain/pitch/spatialization/mixer/device output for these selected cues; classify other WAV payload differences, BGM/WMA, voice/channel/cap/loop/stop behavior and broader frame/hit/KO events. Do not treat this two-cue run or identical PCM as full Q10 alignment.
