# Q07 Sasuke needle: formal-content and caller preflight (2026-09-22)

Status: `SOURCE_AND_CONTENT_PREFLIGHT_PASS / PLAY_NOT_RUN`. This is a new Q07 representative path; it does not reopen Q06 or certify natural controls, spawning, hit behavior, or pixels in Unity.

Authority identity: root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` was freshly measured. Formal release files under `resources/runtime` and the staged `Assets/NTSD/Content/LoganRuntime` copies compare byte for byte:

| File relative to runtime root | SHA-256 (both copies) |
| --- | --- |
| `decoded_dat/c/sasu/sasu.dat` | `f0296329c30a30771bbcd292b72296be413b9cf68a460d54c782c02305ab9583` |
| `decoded_dat/c/sasu/a/chi.dat` | `6c0f2a6b39904230152d83d1ecf8c97076c5c5bbf99ab157240e4574e735a47f` |
| `vfs/c/sasu/sasu.png` | `2aeb4ff667a9ee63e60ee0e866e4700c780974746d47e231caef1eafcad7c694` |
| `vfs/c/sasu/a/chi.png` | `ed17c9caf9d108668bafcaf1ca3afab888b1e3f8613edc3be00a224c8c8d2d62` |

Formal catalog declares selectable Sasuke OID 11 and object/type-3 OID 440. Sasuke standing frame 0–3 has `hit_Fa:261`. Frames 261→262→263→264 are the `chidori_needle` chain; frame 264 contains `opoint: kind:1 x:40 y:46 action:1 dvx:40 dvy:4 oid:440 facing:40`. OID 440 action/frame 1 is `pic:0 state:3000` with a kind-0 hit box (`injury:35`, `fall:70`). The formal playable `ObjectSpawnPlanner28::plan_frame` decodes `facing:40` as four spawn intents with facing mode 0. Unity's `BattleLogicObjectPointRuntime.ProcessOneLateOpoint` also decodes count 4 and facing mode 0, but this is source inspection only; the actual Sasuke Play path has not been measured.

The formal Sasuke sheet is 799×960. Its declaration `w:79 h:79 row:10 col:12` means ten columns and twelve rows. Pillow RGBA tile inspection found nonzero alpha for pics 44/35/36/37/38/39 respectively: 1415/1521/1520/1536/1573/1386 pixels. Formal OID 440 `chi.png` is 327×580 with four columns and seven rows (`w:81 h:82 row:4 col:7`); pic 0 has 132 nonzero-alpha pixels. An initial diagnostic incorrectly interpreted `col` as the column count and produced a false empty pic 35; it was corrected before this report. None of these counts proves the rendered screen output.

Next executable gate: use an unsaved Play clone or formal menu selection to make OID 11 the first human without editing the protected Battle Scene. A focused physical L/D/J probe should assert the published formal root/fingerprint, OID 11 starting `hit_Fa:261`, frame-264 entry, four OID-440 children with action 1 and source-matched positions/velocities at birth, then the child pic-0 binding and a representative nonempty screen region. A same-condition formal EXE/source trace and post-Play ordered shutdown are required before claiming alignment. Do not reuse the Naruto probe's OID-2/OID-33 preflight or its single-child expectation. This is a distinct multi-spawn and projectile path, so it adds coverage that the Naruto clone cannot imply.

No Scene, production script, resource, or legacy file was modified for this preflight. No Unity tests or Play verification were run.
