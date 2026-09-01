# Task Contract — S0-FORMAL-CONTENT-CLOSURE-001

> Status: `IN_PROGRESS / ACTIVE / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / INTERNAL_CHECKPOINT_3_BDY_FOCUSED_GREEN / INTERNAL_CHECKPOINT_4_OPOINT_FIXTURE_COMPILE_BLOCKED / OPOINT_FIXTURE_DUAL_SHA_PASS / OPOINT_MANIFEST_PASS / OPOINT_BASELINE_CAPTURE_NOT_RUN / OPOINT_B1_PARSERV2_EDIT_NOT_STARTED / OPOINT_A33_RESOURCE_EDIT_NOT_STARTED / FROZEN_AUTHORITY_ROWS_ONLY / OLD_EXTRACTOR_NOT_RUN / NO_NTSD / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`
> Queue: `CAP-S0-1 / ACTIVE`

The complete capability contract, seven internal checkpoints, frozen authority
sources, exact initial file scope, three-tier validation, invariants and rollback
are owned by:

`I:\GitHub\Unity_GAS\NTSD_Server\docs\ai\TASKS\S0-FORMAL-CONTENT-CLOSURE-001.md`

The initial Client script scope is limited to one new capability fixture:

- `Assets/NTSD/Scripts/Test/Editor/FormalContentClosureEditorTests.cs`;
- its `.meta`;
- `BattleRuntimeSelfCheck.cs` only after focused expectations stabilize.

The Frame-scalar fixture compiled and executed all 38 frozen rows. Full job
`6db65f8d64674420a14320361d040474` and aggregate job
`63a7e35a4e514da09bd5da8e56d3843f` established the expected-red exact
60-field baseline recorded in the Server Task. This is not a passed checkpoint.

The exact Client resource scope is the following 23 DATs；20 are encrypted
and 3 are plaintext UTF-8 (`naruto_clone.dat`, `chars/weapon3.dat`,
`FrameConfig/weapon8.dat`):

```text
Assets/NTSD/Config/Character/sasori.dat
Assets/NTSD/Config/Character/naruto_clone.dat
Assets/NTSD/Config/Character/naruto.dat
Assets/NTSD/Config/Character/yamato.dat
Assets/NTSD/Config/Character/rock_lee.dat
Assets/NTSD/Config/Character/neji.dat
Assets/NTSD/Config/Character/shikamaru.dat
Assets/NTSD/Config/Character/kankuro.dat
Assets/NTSD/Config/Character/temari.dat
Assets/NTSD/Config/Character/deidara.dat
Assets/NTSD/Config/chars/weapon3.dat
Assets/NTSD/Config/FrameConfig/weapon8.dat
Assets/NTSD/Config/effect/heart.dat
Assets/NTSD/Config/effect/heart2.dat
Assets/NTSD/Config/effect/heart3.dat
Assets/NTSD/Config/effect/heart0.dat
Assets/NTSD/Config/specialattack/wind.dat
Assets/NTSD/Config/specialattack/clay_bird2.dat
Assets/NTSD/Config/specialattack/water.dat
Assets/NTSD/Config/specialattack/4TK_ball.dat
Assets/NTSD/Config/specialattack/earth_creature.dat
Assets/NTSD/Config/specialattack/shadow.dat
Assets/NTSD/Config/specialattack/katon_kakuzu.dat
```

Before any write, an Editor-only mechanical helper must be declared in both
same-ID records and must validate all current values across all 23 files, retain
encrypted 123-byte headers and plaintext BOM/strict UTF-8 identity, round-trip
the matching production-format transform, prove that
only the 60 declared last-wins scalar tokens change, and use temporary siblings
plus atomic replacement. Any mismatch means zero writes. Do not run NTSD or any
extractor, and do not use a whole-file/binary patch.

Declared Editor/test helper path:

- `Assets/NTSD/Scripts/Test/Editor/FormalContentClosureResourcePatcher.cs`
  plus `.meta`.

Its focused preflight is read-only. Its explicit apply action may run only after
that preflight passes in the current worktree.

Fresh Unity job `1ffcf2087f94467da8a9ba8e9ccf344e` passed the renamed
mixed-format preflight `1/1`, proving 23/23 files, 60/60 operations, the 20/3
format split and zero mutation. The explicit apply action is READY.

Seven typed `rock_lee.dat` rows (`Frame332/334/336/338/340/342/344`) have a
blank preceding `dvx:` token. Frames332/334/336 already have raw `dvy:550`, so
filling `dvx:0` is sufficient. Frames338/340/342/344 also have raw `dvy:0`, so
they require both `dvx:0` and direct `dvy:550` repairs.

The same raw/typed preflight found three more structure-enabling repairs:
`clay_bird2 Frame134` fills blank `hit_j:` with `0` so the following `opoint:`
is parsed as a child and the already-correct top-level `dvx:15` remains
last-wins；`water Frame45/46` each fill blank `dvz:` with `0` so the existing
raw `centerx:60` is parsed. Thus the 60 typed differences map to exactly 54
direct value replacements plus 10 blank-token repairs (64 text operations),
with no parser change. The initial 60-operation apply changed exactly 23 DATs；
post-run job `6c6e300f64e24783b8c571a125bb6b83` ran 39 tests with 34 pass / 5
fail, leaving aggregate plus four rock_lee residual rows. The helper now
declares a one-file/four-token residual action；the checkpoint remains PARTIAL.

Residual preflight job `91a405ebeb0d4fa3bef0795400404e45` passed `1/1` and
the four-token action completed the 64-operation set. Final focused job
`396cbbcb057f431f8bd2d85a249ca09b` passed `39/39`. Internal checkpoint 1
is CLOSED；CAP-S0-1 remains ACTIVE and the frozen Itr10 test-first checkpoint is
next.

The user-authorized one-time isolated read-only exception was consumed in the
Server repository. Forty-seven release DAT copies were hashed and made
read-only；a newly named read-only tool linked the real release
`src/data/dat_parser.cpp` and froze the exact 193-row Itr/Bdy/OPoint/WPoint/
topology projection in the Server Task Appendix A. The old extractor and NTSD
were not run. The exception is closed and `FROZEN_AUTHORITY_ROWS_ONLY` is
restored.

Server Task Appendix B freezes the exact Itr10 current-before and release
expected sequences. Client diagnostic job
`1f65dca848c841ff960414f8e4d93ad3` passed `1/1` and measured current-worktree
production projections. The existing fixture now contains one aggregate Itr
test-first assertion. Unity compiled with zero C# errors；Itr-only job
`4304b8da0ddd4fb4aa05b0d7947b4c7f` failed with exactly the ten expected
mismatches while all current-before assertions passed. No Itr DAT/resource or
production parser/runtime source changed. The next gate is review of the exact
minimal Itr resource correction scope；checkpoint2, CAP-S0-1 and S0 remain open.

That read-only review is now complete in Server Task Appendix B.1. It freezes
three A token operations, no B parser difference, eight C structure actions,
witness-only exclusions and the current scalar-checkpoint 23-DAT baseline
manifest SHA-256
`bfa23bf8d918b138ebeefe8367ef3f3b47c2d3328c1799187bbd9c4eb97d440d`.
No resource apply/preflight ran；resource writing remains unauthorized.

The subsequently authorized Appendix B.1 A3/C8 apply completed and changed
exactly the nine whitelisted DAT files. All sixteen non-target scalar hashes
remained frozen, the independent raw audit passed all ten rows while preserving
witness-only values and duplicate Frame occurrences, and no parser/runtime was
changed. The fixture now preserves the frozen red-baseline inequality and
compares the live production projection to the frozen release expected value.
Existing-Editor compile reported zero `error CS`; exact Unity MCP job
`5ac29491449d4c40a4dcfc837774282a` passed the aggregate Itr focused test
`1/1` with all ten mismatches cleared and no new difference. Server Task
Appendix B.2 is the full hash/typed/raw authority. Checkpoint2 is focused green;
checkpoint3 Bdy55 is read-only and Bdy writing remains unauthorized.

The same fixture now also contains an `Explicit`, read-only Bdy current-before
diagnostic. Existing-Editor compile had zero `error CS`; Unity MCP job
`87705b60476f43688634be699ee8fb37` passed `1/1` and emitted exactly 55 raw and
typed rows. Server Task Appendix C freezes the classification at A63/B0/C1,
witness-only limits and Bdy18 manifest SHA
`1f64f02a94ffacc30fca3398548f5a2e9e70c86fbca2e6974af0307fa3903177`.
No Bdy DAT, production parser or runtime was written. Checkpoint3 review is
complete, but Bdy apply remains unauthorized pending user approval.

The user's subsequent apply approval required a same-name release-raw token
audit before preflight. Reading only the existing 47-file isolation proved the
previous A63/B0/C1 classification incorrect: only sasori Frame45 A4 has
same-name raw authority; 49 former `h` actions derive from raw `zwidth`, and 10
weapon4/heart `x/y` actions derive from malformed inline-Itr tolerant parsing.
Current classification is A4/B59/C1. The required semantic stop occurred before
Bdy preflight/apply; no Bdy resource, Client source, parser or runtime was
written. Server Task Appendix C.4 is the current authority.

Server Task Appendix D now freezes the user-approved read-only B59 parser-gate
review. Release behavior is character-scan suffix recognition, not substring
search; current 138-DAT static scope is exactly zwidth49 and the isolation adds
dvx5/injury5. The selected future seam is ParserV2 nested-marker containment
plus `ConvertToBodyBox` suffix/last-wins projection, with five Frame48 inline
restores, exact full-catalog B59 audit, deferred sasori A4+C1 and one final
focused run. Nothing was implemented, written or tested in this review.

The authorized pre-change gates are now complete. The mechanically generated
Appendix-A Bdy TSV contains 82 child rows for 55 rows, with parent SHA
`614822964144cf3e6a153f7bb9f85932c5e563d701c7cc2e14a0fb913bdacc43`,
payload SHA `da9dc3d8279165f7681f261c181b4a842ef2e300c031836985b2ac30d85a1478`
and file SHA `7a076f65d657c80736258d0c2ee349327f792ebfcbf80e0916cc73d600135bc4`.
Explicit Unity job `6c28e067dfc14a4c83846cdd7a175309` passed `1/1` and
captured 138 DAT / 180,190 normalized rows at SHA
`03eb072da4647577a0891bc7071d95e4ebf9ca48484b494e309a894fc3ef9d5b`.
An independent filesystem check reproduced both hashes and the baseline row
count. Production/resource work may now begin only inside Appendix D.5.

The exact-B59 gate subsequently stopped the package. The two production seams
compiled with zero Console errors, and five-inline apply job
`6f470f23eb3d4a2b8d797afc5aea6b19` passed `1/1`. Pre-sasori catalog
job `09c62e54d9404a83b0c33b201c38bebf` found 61 changed keys rather than
the frozen 59; the first extra is `Character/jiraiya.dat / occurrence63 /
Frame66 / Bdy0 / h`. Sasori A4+C1 and all later validation remain unstarted;
no retry or rollback was performed.

Authorized trace rerun `2bb8e22ccb4b431d95fc94fa272490aa` resolved the
61/59 set arithmetic as four unexpected jiraiya Frame66 Bdy0 fields plus two
missing frozen H keys (kisame340 and itachi301). Server Task Appendix D.10 is
the full raw/ownership/release-boundary authority. Jiraiya is outside Bdy55 and
must return to baseline `28/17/29/63`; kisame/itachi are Appendix-A rows and
must become `15/999`. No new expected or extraction is required. The frozen
non-hardcoded narrowing is: only suppress a nested marker without its own end
before the Bdy/frame boundary, and resolve exact body tags before lowercase
suffix aliases while retaining order within each group.

Checkpoint3 then closed focused-green. Exact-B59 job
`f986fddc65414cb58c6da8ce70d47bfb` succeeded with 180,190-row candidate
SHA `8bfc5a17d844f6ad3f1f7a3aac2a4cb5e9a96405351232bde5e02c7a804db7b6`.
Sasori job `7e1ff0464ede4374badb124833def9ea` passed `1/1`; final SHA is
`45a74fb460c006c6b72fbed3ae8d7fd393cd4f16daa061748fa6fb29e3425f29`.
Independent Bdy18 hashes passed 18/18, Unity compile had zero `error CS`, Bdy
job `baac93f767934d26b29e65a5de3126dc` passed `55/55`, and Itr job
`9d2eeab39fa44712b09eeaa5c3db4cbd` passed `1/1`. CAP-S0-1 and S0
remain open; OPoint36 read-only review is now active.

The OPoint36 review is now frozen in Server Task Appendix E at `A31 rows/A33
tokens, B1, C0, witness4`. Same-name isolated-release source checks passed for
all A33 tokens. Frog Frame11 is the only B row: empty numeric `hit_j:` causes
ParserV2 to consume `opoint:`; raw Client/release OPoint values already match.
The 11-file current manifest is `20797a085278ebcd20dd5033220983b2b920f246277ef0d6305e02f4761f2ad0`.
No OPoint resource or production source was written by this review; combined
ParserV2/A33 implementation still requires user approval.

The user has now authorized the combined ParserV2 B1 + A33 package. Server Task
Appendix E.6 is the pre-change amendment and sole detailed authority. The
OPoint-only `read_int` slice, explicit Frog evidence extension, kisame292
WPoint non-effect and later WPoint B-candidate obligation are frozen before any
production/resource write.

Pre-change stopped on fixture CS0029 at line 431 before baseline capture. The
fixture dual SHA and 11-file manifest passed; ParserV2 and A33 resources remain
untouched. Server Appendix E.7 is the exact failure evidence.
