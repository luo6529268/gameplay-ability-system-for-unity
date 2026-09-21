<!-- CHANGE-RECORD
id: NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001
status: VERIFIED
change-kind: FUSION_COMPOSITE_CONTENT_IDENTITY
code-path: Assets/NTSD/Scripts/Animation/LoganContentIdentity.cs
code-path: Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28TraceContentIdentity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05SemanticContentIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionCompositeContentIdentityEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Tools/NTSD28Parity/RawEntityCaptureComparator.cs
authority: Current formal fusion selection and Q06 immutable fusion input contract, explicit composite content V3.
evidence: Unity 33/33; Parity 81/81 and raw19/19; fresh native/Unity/independent header equality; artifacts/diagnostics/NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001/ACCEPTANCE.md.
-->

# NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001

VERIFIED / COMPOSITE_CONTENT_IDENTITY_ONLY.

Requirement: no fusion-enabled battle/publication may reuse identity that hashes only objectDAT. Existing source capture/Unity/Parity currentlyshareV2object-only content. Preserve old objectfingerprint algorithm and historical vectors/artifacts; add explicitcomponents/versionedcombination andcurrentheaderrejection. Formalauthority EXEB1E13A…19033/closure07CD47…778F; source4/fusioninputfreeze proofs inherited. No runtimefusion activation in this task.

## Frozen byte contract

Let O=old objectDefinition rawSHA32bytes,F=fusion InputFingerprint32bytes,S=fusion SemanticFingerprint32bytes. Composite raw C=SHA256(ASCII("NTSD28_LOGAN_BATTLE_INPUTS_V1") + NUL + O + F + S),exact96digestbytesafterprefix. Semantic M=SHA256(ASCII("NTSD28_LOGAN_DAT_SEMANTICS_V3") + NUL + C). Uppercase hex serialization. Catalog ulong=first8 Mbytes littleendian;zero→1. Existing fusion subhashBinaryWriter formats remain exactlyasFUSION-INPUT-FREEZE-001. No machineabsolute paths inhashes.

IndependentPython vectors in artifacts/diagnostics/NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001/COMPOSITE-IDENTITY-VECTORS.json:formalobject4EFE1D…C58C;F28E180…886D5F;S81CA49…8D369;C3A7FF5…C37CC4;MFD18D6…008147;projection0FEFD4B968D618FD. Includes fixedthree32byteranges. These are DESIGNvectors,notyetproducedcurrentruntimeheaders.

## Exact component/API and compatibility decisions

- LoganObjectCatalog.DefinitionFingerprint remains object-only exactly asbefore. Add immutable FusionInput and clearlynamed BattleDefinitionFingerprint (composite). ExistingContentIdentity becomescurrentV3builtfromall3components. Current single-root ForLoganRuntime is explicitlycanonicalportablemode:objectcatalog rootandcompleteVFSrootboth source.RuntimeRoot; decodedroot source.DatRoot. Pass bothroots explicitly toFusionInput.Capture,neverderiveextractedrootfromImageRoot. This documents currentcanonicalAPI restriction,not general nativealternate-root support. Do not introduce a newUI/rootselectionfield. Nativecapture supports independentrootargumentsandmusthashactualsessionarguments; crosscapturetestsusecanonicalsame-rootpair. Separate-rootUnity support is not implied.
- LoganContentIdentity.FromBattleComponents(O,F,S) validatesallhex and retainscomponents. NewcurrenttagV3. Preserve FromDefinitionFingerprint as historicalV2object-only factory (or explicitlyequivalentcompatibilityentry),not a means toforgecurrentcompleteidentity. Preserve ForDecodeContract historicalvectors. CurrentlocalvalidationsessionrequiresV3andallcomponents;oldV2andtag-onlyV3cannotcreatecurrentvalidatedsession. LegacyUnityprofile keeps its own legacytag/algorithm.
- Candidate freshness must call frozenFusionInput.AssertInputsCurrent as wellas existingobject/image/currentsemantic checks. Samebyteshigherpriorityfileappearance mustreject evenifhashcomponentsunchanged. ExistingSourceCacheKey/currentpublisher follownewContentIdentity; audit CharacterAnimtorManager/GameDataManager propagation read-only,add exactpaths before any needed edits,do not changepublicationtransactionorder.
- CurrentLogan contentheader adds objectDefinitionSha256,fusionInputSha256,fusionSemanticSha256; rawDefinitionSha256 nowexplicitlycompositeC. scope="catalog-object-fusion-definitions",decodeContractV3. semantic/projection asabove. Legacyprofile retainsoldexactpropertysetandlegacycontract. NestedcontentversionisexplicitV3;outerraw2/trace3 andentity15/aggregate23/checksum26/shell2/2 remainunchanged because entitypayload/outerlayoutunchanged. CurrentvalidatorsrejectoldnestedV2header/oldpropertyset;thisisintentionalcontentcontractmigration,not silentoldheaderupgrade.
- Unitytrace FromLoganCatalog (or equivalently realpreparedidentity) replacesbothFromLoganRaw(catalog.DefinitionFingerprint) callsites. No currentLoganheader may be generated fromonlyO.
- C++ capture uses options.resource_root forobjectcatalog AND extractedroot,options.complete_vfs_root/decoded_dat forfusiondecodedroot. ReuseofficialFusionCatalog parser/lockedfallback and nativepathorder;compute sameF/S/C/M. Retainselectedfusionpath forinitial/beforepublish/aftercapture freshness comparison separatelyfromhashes; no authoritysource edits. Do not assume two nativeargs equal.
- Parity CreateLogan must requirethreecomponents;Validate recomputesC/M/projection,checksnewexactpropertyset/tag/scope/components. LegacyCreate remainsunaffected. Updateall7Logan syntheticheaderfactorycalls inTraceContractSelfTest/RawEntityCaptureComparator,not arbitrarysingleRAWwithnewtag.

## Ownership

Accurateinitial11codepaths inRecord metadata. Otherpublicationowners are read-onlyreviewtargets until evidence requiresamendingownership. Changes are localbattlecontent/capture/validation only;noScene,Prefab,resources,menu/controller/UI,InputActions,networkwire/Server,Mono/GASframework,globalshutdownorder. No newmanager/queue/pool;immutableidentity follows existingcandidate/publication lifetime.

## Test-first/validation and rollback

Write new focused tests proving fusion-only value changesidentity whileOstable;comment-only changesF/C/MbutSstable;FILE/fallback differ;higherprioritysamebytesfreshnessreject;invalid/missingcomponents andoldV2 rejected;badC/M/projection rejected;unchangedLegacyprofile accepted. PreserveoldV1/V2mathvectorsashistorical. IndependentfixedbytevectorsmustmatchUnityandParity;buildfreshnativecaptureandcomparecurrentformalallcomponents/header withUnity andindependentPython. Existing source tools syntheticheaderfixturesmustrequireallcomponents. Verifypublicationcache/freshness narrowtests,compile0,relatedtoolselftests. Do not regeneratealloldtraceartifacts orrerununrelatedbattlecases. Oneappropriatecurrentpublication/SelfCheck/representativeclosure aftercompleteintegrationifaffected,notperedit.

Rootmustreviewatomiccombineddiff beforedeclaringverified. This taskdoesnotfinishpreparedWorldcatalog/fusionruntimecarriers/fulltransaction,Q06orQ07. No modifiedscripts yet atcreation. Rollback only exactreviewedtaskownedhunks,not userwork;no destructiveGit. Previousgoalprogressverifiedfusionfreeze14/14 remainsvaliduntilaffected;newcontentidentitymustnotclaimcurrentvalidbeforeallthreeendpointsagree.

Implementation update: caller audit additionally found Q05TraceIdentity tests reflect FromLoganRaw; declare exact test path before adapting to FromLoganCatalog. Candidate wraps fusion stale-selection InvalidOperationException into existing InvalidDataException so established cache invalidation/reload remains effective without modifying manager transaction. Unity 7 original paths written; native worker built source diagnostic and matched 7 formal vector fields; Parity integration pending. No claim of measured pre-edit RED (new APIs were implemented before running new fixture).

## Final implementation and evidence

Status: VERIFIED / COMPOSITE_CONTENT_IDENTITY_ONLY.

Current Logan identity now retains object-only O and combines O, fusion input F and fusion semantic S into C, followed by V3 semantic M and LE64 projection. Unity candidate/cache, current local validation session, source capture and Parity current validators use the new contract. Historical V1/V2 math vectors and artifacts remain historical; current validators reject old V2 or component-less V3. Legacy profile unchanged.

## Actual evidence

- Existing Unity Editor, refreshed scripts and completed domain reload; Console error-CS query returned 0. No second Editor started.
- Unity job 906f2e19a88c4e1d82bbe615868b17e6: 33/33 PASS, 31.4659859 seconds; final XML in unity-results.xml. Includes new composite 11 cases, Q05 semantic 16 cases and Q05 trace 6 cases, with actual formal-content 3-tick raw capture and manager-reference restoration. No measured pre-edit RED was taken; do not imply test-first execution.
- Native capture rebuilt with Build-AuthoritySourceCapture.ps1; native-build/build-manifest.json and native-formal.jsonl. Formal roots both canonical runtime; binary F3317CC6DB2DAB23175F1C514BCB68B92A95150C02E94538DB2EF86B74C56D8C. Evidence class SOURCE_MODEL_DIAGNOSTIC_ONLY, not formal EXE recording.
- Parity build 0 warnings / 0 errors; self-test 81/81; self-test-raw-entities 19/19. Reports parity-self-test.json and parity-raw-self-test.json. Legacy acceptance plus invalid/old/missing/tampered C/M/projection and fusion-only mismatch controls included.
- Independent Python design vector, native content header, Unity catalog header and Unity actual raw capture header match all 11 properties; three-endpoint-header-check.json. All 7 frozen hash/tag/projection fields match independent vector.
- validate-authority-capture accepted fresh native stream: native-parity-validation.json, valid-source-model-capture, 3 ticks/6 entity records, certificate=false.
- compare-raw-entities accepts both new content headers but exits1/different: 3 ticks/6 pairs, 300 field occurrences, 18 differences in exactly 3 pre-existing MISSING bindings: combat.platformSourceSlot, combat.environmentState, combat.environmentSourceSlot. 47 unique fields equal. This is not a fully aligned combat certificate. See neutral-raw-comparison.json.
- Independent read-only review found no material blocker in hash serialization, roots, candidate cache exception contract, complete-identity guards and source selection freshness. Root also reviewed Parity recomputation and exact property sets.
- Scene dirty=false/rootCount14; SHA256 BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6 preserved. No Scene/resource/nonbattle changes, no computer-use, no commit/push.

## Validation boundary

No full BattleRuntimeSelfCheck or Play sweep repeated: this change alters load-time content identity/capture acceptance only, does not activate fusion behavior or change tick/runtime/shutdown owners. Related actual raw capture verifies publication restoration. Frozen parser24/input14 and previous combat evidence reused. Native distinct-root routing and path freshness are source-reviewed; this run tests canonical equal roots, not a runtime file-switch injection. Unity candidate same-bytes higher-priority invalidation is exercised.

Remaining: prepared World fusion catalog, independent persistent carrier/schema contracts and full source-equivalent fusion transaction; Q06 incomplete, Q07 formal DAT/images not migrated. Overall goal ACTIVE. Ordinary platform/raw3 and all other deferred gaps unchanged.
