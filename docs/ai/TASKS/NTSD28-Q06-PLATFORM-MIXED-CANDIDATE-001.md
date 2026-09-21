# Platform and ordinary candidate ordering

IN_PROGRESS / SOURCE_FIRST. Current source battle_world.cpp rebuild_geometric_hit_candidates scans unordered slot pairs with ordinary A/B, platform A/B, ordinary B/A, platform B/A. Ordinary geometry reads current integer position after previous pairs. Unity CollectCollisionCandidatesWithPlatforms follows this order but must prove actual geometry consumer respects updates.

Exact initial code path Tools/NTSD28AuthorityTrace/platform_mixed_candidate_witness.cpp only. Three entities: platform20 snaps rider21 fromY-10 to-20; ordinary attacker19 or22 has narrow geometry atY-20. Before-platform ordering should miss, after should overlap. Add no-snap control. Preserve target preciseY-10 despite integerY-20. Actual source candidate API, not hit application or full tick. Double-run capture with current build manifest. Source result first, then separately declare Unity fixture; no production/Scene/resources changes yet. No new owner/schema. Existing platform source21 and frame-motion evidence retained. Reversible diagnostic addition only; no user-file deletion.

Unity exact fixture addition: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06PlatformMixedCandidateEditorTests.cs. Three source cases with actual World candidate entry and ordered cleanup; no production edit.
