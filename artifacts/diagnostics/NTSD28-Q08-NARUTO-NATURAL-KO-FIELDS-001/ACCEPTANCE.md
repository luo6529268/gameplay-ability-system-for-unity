# Q08 Naruto natural knockout event fields: focused acceptance

Status: `FOCUSED_TEST_PASS / NATURAL_FULL_DRIVER_EVENT_FIELDS_ONLY`. Q07, Q08, BATCH-04 and the overall alignment goal remain open.

The unchanged formal root `NTSD2.8-Logan.exe` SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Its Naruto ordinary-punch tick8 release trace has SHA-256 `4C19F76B103B9129FB643FAFBD9A4FE116A10A06B6876D1ACFDEF35907F16BCB`. The exact knockout object extracted from that file is saved in `formal-tick8-knockout-fields.json`: battleTimeTicks7, sourceObjectType0, fourOwnerSlot0, sourceSlot0, victimSlot1, creditSlot0. The formal root LFR CRT state starts from its playback seed0 while the paired source/Unity scenario declares seed682973786; no whole-RNG or whole-state same-seed claim follows from this event check.

Only the existing `NTSD28Q07KnockoutFeedProductionRowEditorTests.NarutoPunchPublishesKoFeedThroughNativeThirtyTickBoundary` method changed. Its real `SimulationTickDriver` 38-tick Naruto replay now asserts one event and all six formal event fields at completed tick8. All prior count/time, published-row lifetime and ordered-shutdown checks remain in the same method; its output CSV schema and SHA-256 stayed `1C299F63CB2ECD84B63977E877D827AFE53C7F405A6641AB60D9728809A69AB1`.

The original Unity Editor PID11944 in the original project accepted a local MCP refresh/recompile, then exact EditMode job `498abdee1c6540a8b912809fab79d9cc` succeeded 1/1, zero failed. Raw MCP start/result JSON are beside this report; the result includes verbose converter output, so inspect its structured summary rather than printing the raw file. The Editor returned idle, non-Play, not compiling. No second Editor or computer-use was used.

Protected SHA-256 values stayed unchanged: Battle Scene `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`; Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`; ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`. No production battle script, DAT value, asset, Scene or nonbattle logic changed in this package.

This closes only the six event-field assertion for one naturally reached KO in a full Driver EditMode replay. Other KO producers and modes, SelfCheck, integrated Battle Scene Play, root EXE full same-state parity, Q07 content/skill exit, Q09 visuals, Q10 audio and Q12 full integration remain separately open.
