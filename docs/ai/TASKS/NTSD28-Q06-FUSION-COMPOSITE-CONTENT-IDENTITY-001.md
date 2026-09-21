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

Scope addendum before Q05TraceIdentity test edit: exact additional path Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs. Its reflection factory and formal header assertions must move to prepared catalog / composite C. Total declared scripts now12. Production manager remains unchanged; candidate translates fusion stale-input error to existing IOException-derived cache invalidation contract.

Final evidence: artifacts/diagnostics/NTSD28-Q06-FUSION-COMPOSITE-CONTENT-IDENTITY-001/ACCEPTANCE.md. Accurate final12 scripts; current header verified, runtime fusion activation not included.
