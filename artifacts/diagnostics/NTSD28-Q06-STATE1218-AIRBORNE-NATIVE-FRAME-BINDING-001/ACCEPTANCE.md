# State12/18 native frame binding joint acceptance

VERIFIED_SCOPED / 2026-09-21. Source contract comes from the current formal playable closure07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F, derived witnesses are not formal EXE gameplay recordings.

Production scope is exactly two LF2Entity.ApplyCurrentDatType0State1218ContactAction raw binder calls and one ApplyCurrentDatType0AirborneAction call switched to existing native binding. Selector, phase, motion, pending transaction and counter/latch/snapshot order preserved. Independent static review confirmed scope.

Contact source480 double-run SHA d898b84df517ebc3b5c21b1113c1a390baaedadffa961f35d8e385608dffc6c1, independent228481 checks PASS. Original four Unity matrices each before0/immediate410/following120; contact-only fix before0/immediate18(all airborne)/following0. Airborne source55 SHA a4d7c8fd5eb0e3b957486993ef99295f1ce70e2592288b58c1f5d32f5452e691, independent24696 checks PASS. Its Unity RED Authority/DataOriented55 immediate30 and Mobile/Legacy8 immediate7, before/following0. After single airborne fix final source comparisons all0: contact480 Authority/DataOriented once, airborne55 same path and8 Mobile/Legacy smoke. Do not claim all four contact combinations rerun after the final fix.

Joint job e6d47ce73ce2473180ce73ea5b4eca50 PASS29/29 (oldcontact9, oldairborne17, source comparisons3), duration8.03s. Full SelfCheck PASS2026-09-21T00:45:27.002427Z. No subsequent production edits, test-only representative harness changes do not require duplicate full SelfCheck.

Representative replay job a7368d7317564de1be6344801b16aca6 PASS4/4, duration3.48s: contact12+airborne8 per paired configuration,40scenarios/80 replayed full ticks after restore, plus original comparison executions. Invalid prior RNG cursor checked. Contact indices0,78,144,216,288,294,300,301,410,416,444,456; airborne0,4,8,15,19,32,36,44.

Real Scene Play PASS40 (Authority/DataOriented/renderer20 + Mobile/Legacy/logic20); Scene checksum unchanged and renderer borrowers2->2. Q05 in-place restore/shutdown PASS2026-09-21T00:48:58.612721Z: restore4->4; World objects/slots/logic/render borrowers0; Stopped across two frames. Final Editor idle/notPlaying, scene dirtyfalse/root14, hashBCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6 unchanged. No new lifecycle module; no repeat reentry cycle claimed for this package.

Representative limits: contact12 all floor0 (negative floor is in full480); airborne8 covers negative floor, but selected negative-environment rows resolve181, not182 (182 covered by full55). These are deliberate representative runtime checks, not all-scenario Play. Existing old failures preserved. Explicitly no certification of previousXYZ Unity carrier, raw3 missing fields, platformop30, formal asset migration, physical keyboard, images/audio or crossWorld recovery. Formal330 static default contact target references all declared and19461ITR custom picked/picking tokens0; synthetic RED is not proof of a currently user-visible formal-content bug.

CS error query0, ledger599/17PASS, diffcheck clean. Detailed source, RED, joint-pass, representative-replay-pass and representative-play-pass artifacts retained. Goal/Q06 remain incomplete.
