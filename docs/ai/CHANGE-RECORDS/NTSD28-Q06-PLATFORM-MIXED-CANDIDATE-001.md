<!-- CHANGE-RECORD
id: NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001
status: VERIFIED
change-kind: MIXED_CANDIDATE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/platform_mixed_candidate_witness.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformMixedCandidateEditorTests.cs
authority: Current formal playable battle_world.cpp geometric candidate and platform interleaving.
evidence: SOURCE-RESULT.md and UNITY-RESULT.md source3/Unity3 PASS; parent SCOPED-ACCEPTANCE.md stable SelfCheck/Renderer/close
-->

# Platform and ordinary candidate ordering

IN_PROGRESS / SOURCE_FIRST. Current source battle_world.cpp rebuild_geometric_hit_candidates scans unordered slot pairs with ordinary A/B, platform A/B, ordinary B/A, platform B/A. Ordinary geometry reads current integer position after previous pairs. Unity CollectCollisionCandidatesWithPlatforms follows this order but must prove actual geometry consumer respects updates.

Exact initial code path Tools/NTSD28AuthorityTrace/platform_mixed_candidate_witness.cpp only. Three entities: platform20 snaps rider21 fromY-10 to-20; ordinary attacker19 or22 has narrow geometry atY-20. Before-platform ordering should miss, after should overlap. Add no-snap control. Preserve target preciseY-10 despite integerY-20. Actual source candidate API, not hit application or full tick. Double-run capture with current build manifest. Source result first, then separately declare Unity fixture; no production/Scene/resources changes yet. No new owner/schema. Existing platform source21 and frame-motion evidence retained. Reversible diagnostic addition only; no user-file deletion.

Initial source output SHAE35240E7 all candidate0: fixture attacker atX100 was also eligible platform target and snapped before attack in late-slot case, moving attack geometry. Archived initial-source; expected assertion failed, not a pass. Correct fixture attackerX120 outside strict platform90..110; itr localX-24 preserves world96..104, same narrow Y. Source production unmodified.

Final source3 expected0/1/0 PASS; double SHA62D0258D7DE8061625D0BE85F764BCF58CA878062F1F94E5C70C25F627DEB77E, exact reference/slot/integerY/preciseY checks. See SOURCE-RESULT.md. Initial fixture failure preserved. Unity remains unmodified; status IN_PROGRESS, next exact fixture declaration. Ledger632/12PASS.

Unity pre-change exact new fixture path declared above. Actual Logan DAT parser, World/factory, source-final3 cases, attackerX120 outside platform; capture snapshots then actual CollectCollisionCandidatesAll. Compare candidate counts and target integer/preciseY/reference/link, attackerY unchanged. Also inspect formal candidate range count. Existing ordered World shutdown in finally; no new owner/schema/production. Three cases only; no broad rerun.

Unity actual3/3 PASS jobbde7da1f; candidate and range counts0/1/0, integer/preciseY/link/reference and attackerY match source. No production changes needed. XML archived unity-bde7da1f. Stable parent package now requests one fullSelfCheck; renderer/Destroy closure remains pending.

Stable package check: mixed candidate actual3/3PASS and single fresh fullSelfCheck 2026-09-21T12:14:34.368890+00:00 PASS; artifacts/diagnostics/NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001/UNITY-RESULT.md. Existing relevant evidence reused. Renderer/ordered closure not yet completed; do not mark production package VERIFIED.

Exit correction: parent PLATFORM-TRANSACTION-001/SCOPED-ACCEPTANCE.md records actual pooled Renderer, ordered close and fulltick/replay after this source3/Unity3 focused pass. This mixed-candidate diagnostic has no production changes and is VERIFIED in its declared order/geometry scope; Q09 visible shadow and Q07 DAT-triggered recheck remain downstream. Historical pending line above is retained.
