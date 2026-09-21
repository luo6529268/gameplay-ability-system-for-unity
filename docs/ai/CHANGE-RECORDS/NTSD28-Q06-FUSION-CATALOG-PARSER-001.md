<!-- CHANGE-RECORD
id: NTSD28-Q06-FUSION-CATALOG-PARSER-001
status: VERIFIED
change-kind: FUSION_CATALOG_STRICT_PARSER
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganFusionCatalog.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganFusionCatalogParser.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionCatalogParserEditorTests.cs
authority: Current playable FusionCatalog28.parse_text line grammar and actual formal fusion.dat.
evidence: Formal source parser and source4 record witness; Unity parser tests pending.
-->

# NTSD28-Q06-FUSION-CATALOG-PARSER-001

IN_PROGRESS / IMMUTABLE_CATALOG_AND_STRICT_PARSER_ONLY.

Authority current formal EXE B1E13A…19033/closure07CD47…778F, source/data/fusion_catalog.cpp FusionCatalog28.parse_text and header FusionRecord28. Actual formal fusion.dat SHAe0d7bf92f222c63c04d6728ecd423369f0604d77ff12df958ce4ae76feba5fee and source4+1737 witness prove two records. General Lf2DatTokenizer strips structure/line context and onlyhash comments, so do not reuse it where it changes native line-level grammar; implement same dedicated line state machine,not regex reconstruction.

Exact new scripts:
- Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganFusionCatalog.cs: immutable readonly record of16 numeric fields+SourceLine, immutable diagnostic severity/line/message, catalog SourceAvailable/IsValid/readonlyRecords+Diagnostics. No retained mutable backing supplied bycaller.
- Assets/NTSD/Scripts/DatParser/Runtime/Parsing/LoganFusionCatalogParser.cs: ParseText only,puremanaged,nofile I/O,noSingleton,noUnitytypes.
- Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionCatalogParserEditorTests.cs: actual formalfile recordvalues against source4 catalogrecords; explicit source grammar and malformed controls.

Native exact grammar:trim only space/tab/CR/LF,strip atfirst;or# on eachline;exact outertags;firstcolon splitskey/value;fusion: anyvalueignored startszero-defaultrecord,dupstart emits error and resets record;fusion_end: closes,recordmax50,keepsvalidcollectedprefix+error;fields outside record/outsideouter or unknown/no colon warning; recognizedfieldstrict signeddecimal int32,no leading+,no suffix/float,overflowerror;lastduplicatefieldwins,missingfields0;outerclose while recordopen emits error/discardsunfinished;EOFmissingbegin/recordend/outerend errors. Warning-only catalog remains valid. Preserve source line indexes and record order. ParseText null treated as emptytext. Nativefile bytesBOM not silently stripped byparser. No production fallbacktable or implicit acceptance of malformedcontent.

Tests cover currentformal16fields×2; comments/whitespace/duplicate/negative/default/order,strictintegercases(+/float/suffix/overflow),limits50/51,missing/nestedboundaries,unknownwarning,vacuousemptyouter,recordstartvalueignored and immutablecollection rejection. Use existing literal formalpath read-only; no resources copied or changed. Source4 comparisons are semanticrecord evidence,notwholeC++parserexhaustive oracle.

This task does not wire GameDataManager/World/Host,change schema/snapshot/hash,add runtimecarriers or execute fusion. Subsequent catalog contentidentity/preparation and fulltransaction require separate exactTask; current Q06 remainsunfinished. Immutable load-time values have no active shutdown hooks,workers/pools or new service; eventual lifecycleowner existing preparedworldcatalog. Compile+focusedparser tests sufficient here; no battleSelfCheck/Play for unwired pureparser. Rollback only reviewednewscriptdiff preserving userfiles; no destructiveGit or deletion. No nonbattle/UI/framework/resources edits.

Final VERIFIED / IMMUTABLE_TEXT_PARSER_ONLY: actual3scripts,jobb1873ce0593347a99062b15d6537fc99 24/24PASS,CS0,independentreviewPASS. NoSelfCheck/Play/runtimeintegration claim; exact evidence artifacts/diagnostics/NTSD28-Q06-FUSION-CATALOG-PARSER-001/ACCEPTANCE.md.
