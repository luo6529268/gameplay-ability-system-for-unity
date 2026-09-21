<!-- CHANGE-RECORD
id: NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001
status: VERIFIED
change-kind: FUSION_CATALOG_TRANSACTION_SOURCE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/fusion_catalog_transaction_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_fusion_catalog_transaction_witness.py
authority: Formal GameSession fusion.dat and current playable BattleWorld28.advance_native_fusions.
evidence: Source4 double-run SHAff7083f57d73ee34fd709a08cf78a2fcdd785880e7c4712e74051959a4d0b4bf; independent1737 PASS; Unity data contract pending.
-->

# NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001

IN_PROGRESS / FORMAL_CATALOG_SOURCE_WITNESS_FIRST.

Authority current formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033/playable closure07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F. Exact GameSession fusion.dat load and BattleWorld28.advance_native_fusions. Formal SHA documented in artifact/formal-fusion-identity.json. Independent audit in AUTHORITY-GAP-AUDIT.md identifies whole catalog transaction discrepancy, not just frame getter. Preserve prior C12/C25h scheduling and timer decrement.

Only initial script write scope Tools/NTSD28AuthorityTrace/fusion_catalog_transaction_witness.cpp. No Unity or formal source/resource changes, no adding fusion.dat to Unity yet. Diagnostic executable loads actual formal FusionCatalog file, asserts two expected records, uses managed synthetic ordinary object definitions for exact side-effect witnesses; do not treat synthetic definitions as migrated content.

Four initial representatives:
0 record1 eligible7+8→51 then controlled timer0 boundary→defuse;
1 record2 eligible10+11→52 (nonzero starting timers, cover1) then controlled timer0→defuse,chp1/hitja1;
2 record1 HP exactly177 strict rejection, feature gatesfalse;
3 record1 actual merge then defuse catalog lacking original partner definition, preserving whole pre-defuse state on unresolved failure.

primary slot0 X340/partner slot1 X300,Z250,group3; current primary20/partner10 bothstate2,HP100/120 first record,200/300 second; effectiveMax250/275,baseMax400. Distinct wait counters7/8,latches13/11,previous14/12,snapshot20/10; nonzero independent pending/effect/input/statistics markers only if exact source fields confirmed. Declared fusedaction290/310 and split112,state0/wait41,nextself, noITR/BDY. Definition publication witness should supply distinct use_ai/drop/max_mp/weapon_hp/defend so complete data writes observable. Do not invent field names/defaults. Capture beforeMerge/merge result/afterMerge, explicit test-only timer0 intervention then beforeDefuse/defuse result/afterDefuse; after valid defuse capture fullfollowing tick with same formalcatalog and unchangedoptions. Rejectcase captures no-defuse states. Record actual suspended partner as entity(slot)=null; never dereference pre-merge pointers after optional move. Use only public APIs, no private access modifications.

Extra capture source fusion saved ids/slot,timers/gates,latches,AI/drop/damage multipliers,maxMP,positions/snapshot/current descriptors and retained-partner fields, alongside existing raw/B2/RNG. Existing raw47/schema boundaries remain; extra outputs do not promote raw3. Controlledtimer0 means branch boundary evidence, not4500/200 elapsed-tick proof (old cadence/placement evidence reused).

Build through existing source wrapper into Build/NTSD28FusionCatalog, first/repeat bytehash, validate actual record loading and branches. Source compile/run/independent checks must be recorded before exact Unity carrier/catalog plan. More admission/range/frame/corruption witnesses may follow based on observed gaps; four initial controls do not close all fusion requirements. No SelfCheck/Play until Unity production selected/implemented. No new manager/queue/pool/shutdown order changes here. Rollback only this diagnostic source hunk/artifacts reviewed with user-work protection, no destructive Git. Goal/Q06active/Q07notmigrated.

Formal six-object static audit:all7/8/10/11/51/52 indexedtype0,source DAT hash matches captured manifest. Originals all explicit112;51 explicit290;52 does NOT explicitly declare310 but native implicit310 valid. Update source case1 to omit310 in fused52 definition, directly represent current formal boundary; other cases declared as planned. False explicit declaration must not be misreported missing native descriptor. Artifact FORMAL-FUSION-OBJECT-DEFINITIONS.json separates both values.

Source4 build81447exit0;double-run58471bytes each/SHAff7083f57d73ee34fd709a08cf78a2fcdd785880e7c4712e74051959a4d0b4bf. Actualmerge/defuse2,HP177reject and missingpartnerdefinition unresolved observed. Predeclare independent Python validator new Tools/NTSD28AuthorityTrace/validate_fusion_catalog_transaction_witness.py:derive merge/defuse visible capture from before with source-identified mutations, hardcoded synthetic DAT metadata31/32/33,drop1/2/3,maxMP600/700/800,weapon21/22/23,defend110/130/150 andmode125/150. Validate fullpubliccapturedstate andfailurepublicnonmutation,not inaccessible private suspension bytes or fullfollowing model. No Unity production scope yet.

Source4 build/double-run verified SHAff7083f57d73ee34fd709a08cf78a2fcdd785880e7c4712e74051959a4d0b4bf,58471bytes each.Independent visible-state model1737checks PASS deriving complete emitted publicmerge/defuse state frombefore plus formal writes,synthetic definition metadata/mode scale. Cases0/1 bothfuse+defuse,case1implicit310 valid;case2HP177reject withpublicstateunchanged;case3missingoriginalpartnerdefinition defuseunresolved1/publicstateunchanged. Does not claim inaccessible suspended-state bytes unchanged or model fullfollowing; validcase fullfollowing captured for future Unity comparison. Controlledtimer0 not natural4500/200 elapsedtest.

Current status SOURCE4_PASS_UNITY_DATA_CONTRACT_PENDING, noUnity production or resources changed. Next exact work is read-only Unity field/carrier and immutablefusioncatalog preparation design using UNITY-CARRIER-ENTRY-AUDIT. Need resolve fusionDisplay190/revive318/global secondfeature/definitionAI-drop field mappings and snapshot/hash implications before adding carriers; do not add duplicates based on grep. Prepare a separate precise catalog/carrier Task/Change and then source4UnityRED through actual C12 module, preserving C12/C25h schedule. Existing dormant partner architecture can be retained only if observable sourcepreservation/preflight is proven; no generic partner.Reset permitted merely for convenience. Fulladmission matrix and published-content selection remain later scoped dependencies,not declared aligned by4cases. Current buildworker terminal0,noUnitytest/build/agentactive. NoSelfCheck/Play run in this diagnostic-only task.

## Final scoped acceptance

VERIFIED / SOURCE_MODEL_PUBLIC_STATE_WITNESS_ONLY.
Evidence: artifacts/diagnostics/NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001/ACCEPTANCE.md. Previous IN_PROGRESS checkpoints above are history; deferred Host keys/poststory/NativeAI and raw/diagnostic limitations remain explicitly open in shared acceptance.
