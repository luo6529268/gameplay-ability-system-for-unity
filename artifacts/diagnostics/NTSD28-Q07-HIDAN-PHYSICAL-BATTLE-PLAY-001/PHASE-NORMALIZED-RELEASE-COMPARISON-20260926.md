# Q07 Hidan physical Battle Play versus formal release: bounded phase comparison

Status: `VERIFIED_SCOPED_SELECTED_FIELDS / FIRST_ACTION_FRAME_DIFFERENCE`. This is a read-only comparison of saved traces, not a new Play run or a same-world certificate. Q07 and BATCH-04 remain open.

The root `NTSD2.8-Logan.exe` was freshly hashed as SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The root-release natural-input traces use formal Stage23, actor X500, target X580 or X1200, Z350, action0 and input phase0. The original Unity Battle Scene physical-key reports use actor X500, the same target X, Z650, action0 and input phase1 after five bootstrap ticks. Both runs enter the same ordinary Hidan attack-then-jump sequence; Unity's physical J/J/K/K is measured as canonical Jump/Jump/Defend/Defend. Because the stage, Z and initial phase differ, these reports are not strictly identical initial worlds.

| Input | SHA-256 |
| --- | --- |
| `hidan-physical-x580-20260926-c.json` | `9AA9FABF08B224B8B0F1713CC84BFD989CD83B856D90F0B9C7D239D53D3D12E7` |
| `hidan-physical-x1200-20260926-a.json` | `0B428EC801FC20B1F3252C199BF3AD5086E1C6968AAF3EC10B4E67399692B5A9` |
| `release-natural-x580-trace.jsonl` | `ADF2D0C529EC20166BCF034765CE32CAEBE210C88E9D08F319C3B094E76D84BE` |
| `release-natural-x1200-trace.jsonl` | `8141300C22E0AA5869C5AE13E69E62D2FB46E41D9E668C20A74091370EC3CDAA` |

For each Unity sample at relative tick `r=1..40`, compare the root-release row at tick `r+1`. This offset was selected after scanning offsets -2 through +3; +1 had the most matches for both cases. Compare exactly six fields: actor action/frame, actor current PP/root MP, target current PP/root MP, target HP, actor catch-target slot, and target catch-source slot. The different PP/MP property names are the already established formal/Unity resource mapping for this diagnostic. No position, velocity, RNG, event, pixel or whole-world checksum field is included.

| Distance | Actor action | Actor PP | Target PP | Target HP | Actor catch target | Target catch source | Total |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| X580 | 39/40 | 40/40 | 40/40 | 40/40 | 40/40 | 40/40 | 239/240 |
| X1200 | 39/40 | 40/40 | 40/40 | 40/40 | 40/40 | 40/40 | 239/240 |

Both first differences are the same: Unity relative tick1 / complete tick6 shows actor action60, whereas formal tick2 shows action65. All six selected fields match from relative tick2 through tick40 in both cases; the five non-action fields also match at relative tick1. In the positive case, Unity relative tick10 / complete tick15 and formal tick11 both record reciprocal catch; Unity relative tick12 / complete tick17 and formal tick13 both record target HP470 and the post-catch PP increase. The negative case records no catch or injury in either trace.

The one-tick action offset is consistent with the measured different input phase, but this comparison alone does not prove that input phase is the sole cause. It does **not** erase the first action difference, make Z650 equivalent to formal Z350, or prove formal and Unity pixels. The selected-field result adds a direct physical-Play-to-root-release bridge to the prior independent source/EXE and Unity raw checks. The next Q07 exit still needs matched initial world/phase where supported and natural skill/lifecycle plus presentation checks. No DAT values, Unity code, Scene, config, assets or nonbattle behavior were changed.

## Expanded coordinate-domain check

The same saved traces were also compared at the `r → r+1` mapping without adding coordinate fields to the 239/240 result. The coordinate domains and stage starting points differ. The Unity report exposes physical `x/z` plus `sourceX/sourceZ`; the formal trace exposes integer `x/y/z` and `preciseX`. Exact `sourceX` versus formal integer `x` is 0/40 for the actor in both cases: at the first two mapped ticks Unity is 501/502 while formal remains 500. In the X1200 control this early +2 difference persists through the later stationary segment. The X580 target's `sourceX` agrees for its first nine mapped ticks, then differs after the grab; the X1200 target's `sourceX` agrees 40/40. This rules out claiming complete position equality from the six-field result.

Unity `sourceZ` is 650 for the actor throughout, whereas formal `z` is 542 at all 40 mapped rows, a measured 108-unit separation. Unity's X580 target moves to `sourceZ=649` after the grab; formal stays at 542. The formal trace begins at Z350 before this step sequence; the Unity fixture starts at Z650. These data do not isolate the reason formal Z becomes 542, and the project stage/background exception prevents treating this cross-stage difference alone as a production bug. Unity target `y` has fractional values after injury while formal `y` is integer; this report does not equate those two representations.

The unobstructed X1200 control gives a useful motion-ratio check. At Unity relative steps 9→10, 10→11 and 11→12, formal actor `x` advances exactly 50 each step (500→550→600→650). Unity physical `x` advances `76.81920480120027` each step (503.072768192048→579.8919729932483→656.7111777944485→733.5303825956488). Each measured quotient is `1.5363840960240054`, matching the approved `2048/1333 = 1.536384096024006` within floating-point precision. This is one Hidan unobstructed-dash witness for the shared proportional displacement rule, not proof for catches, other entities or the entire D-024 exception. The X580 grab reanchors positions; this report does not apply the simple dash quotient across that transaction.

The next coordinate-sensitive Hidan comparison must first align input phase and stage/position semantics or explicitly normalize the approved world-scale exception. Do not repair the measured 1–2-unit source-coordinate differences or Z separation from these cross-context traces alone. No new source/Unity run or code/resource/Scene change was made for this expanded read-only check.

## First-action RNG ownership audit (2026-09-26)

The first-action value cannot be attributed to input phase alone. The formal root EXE identity was rechecked at `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Its saved X580 trace starts at input phase 0: tick 1 advances to phase 1 with action 0 and no synchronized RNG call; tick 2 advances to phase 0, records the `native standing attack RNG 0x82` event, and chooses action 65. `source/ntsd28_core/src/simulation/input_routing.cpp:821-849`, included by `source/ntsd28_playable/scripts/build.ps1 -Target playable`, defines this choice as `(random.synchronized_next(0x82, 2) + 12) * 5`.

Unity's `BattleCharacterActionWriter.RouteNativeStandingAttack` uses the same callsite and two-result formula. The original Editor's separate matched Hidan natural-input raw fixture starts from the formal Stage23/Z350 scenario and records action 65 at completed tick 2; its saved input/RNG stream records synchronized callsite 130 (`0x82`) with result 1 at that tick. Thus action 65 is reachable in the current Unity implementation under a controlled matched scenario. The physical Battle Play report starts at input phase 1 after five bootstrap ticks, captures action 60 on its first attack step, but does **not** capture the Play world's pre-attack synchronized RNG state or call result. Action 60 is consistent with the other value of the shared two-result formula; the available Play report does not prove that this RNG branch, rather than another branch, produced that frame.

Classification: `FIXTURE_CONTEXT_OR_RNG_FIRST_DIFFERENCE_UNRESOLVED`. The prior phase-offset comparison still accurately reports 239/240 selected-field matches, but phase difference alone is **not** an established explanation for the 60/65 value. Do not change the shared attack formula or DAT from these traces. A future same-initial-world check must capture the synchronized RNG state and `0x82` call result immediately before the first attack in both hosts, along with input phase, before calling this a production rule difference. This audit reuses saved traces and source; it adds no Play run or production-code change, and Q07 remains open.
