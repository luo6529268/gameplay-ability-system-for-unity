# Q09/P-08 original Battle Scene central bleed mark pixel witness

2026-09-26. Status `VERIFIED_CONTROLLED_CENTRAL_GPU_ONLY`. This is a bounded P-08 sub-exit, not Q09 or full P-08 completion.

Formal authority: root NTSD2.8-Logan EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `render_snapshot.cpp` body→bpoint emission and `d3d11_renderer.cpp` solid draw. Current formally staged Ita OID9 frame0 has one X/Y-only bpoint. No DAT values were edited.

Original Unity Editor PID11944, original saved `NTSD_Battle.unity`, formal `Assets/NTSD/Content/LoganRuntime`, CentralOnly. The opt-in probe instantiated a temporary Ita OID9 frame0, fixed its logical position and compared high HP500 with current HP166 at the same paused production World state. Each state passed the production presentation dispatch and original world-camera `Camera.Render()` into a temporary white-background target. The camera configuration was restored and the fixture unregistered before exiting Play. This is a controlled rendering witness, not natural player input or a complete production logic tick.

Runs and corrections:

- `bpoint-ita-central-01.json` FAIL: the probe incorrectly wrote base max HP3 rather than current HP, leaving current HP at 500. No production first difference was inferred. The probe was corrected to write `Health.HP`; Scene hash and object/slot/pool counts had recovered.
- `bpoint-ita-central-02.json` reported `PIXEL_OWNERSHIP_UNPROVEN` due to a probe-only color predicate that wrongly required the green channel to change. Independent read-only comparison of its high/low PNGs found four newly pure-red pixels at image coordinates (1040,474..477), where dark red or sprite skin color had been before. The original report remains preserved.
- Final `bpoint-ita-central-03.json` `PASS_CONTROLLED_CENTRAL_PIXEL`: high HP500 has 0 bleed commands; low HP166 has one mark in slot50 at command index2 after body index1. Current formal defaults are 1×3. Projected 5×8 pixel A/B region x[1038,1043), bottom-origin y[600,608) includes 3 newly pure-red pixels, with sample (1040,603). Independent Python/Pillow read of the two saved PNGs reproduces exactly those three pixels: before (89,0,0), after (255,0,0) at (1040,603..605). The report's `cameraStateRestored` and `fixtureUnregistered` are true; object count4→4, claimed slots2→2, renderer borrowers2→2.

Original Editor compiled the final probe revision: `Assembly-CSharp-Editor.dll` timestamp later than source, Console error query 0. After the final request, original Editor returned to non-Play idle. Menu Scene, Battle Scene, GameConfig and ProjectBattleModeConfig SHA-256 stayed respectively `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, `88E10D43B047952FD3053A87B7E5F60D0F87A6CD1A23313F37EA1503C686F55C`.

Remaining P-08 gates: natural Ita/Sasu low-HP Play, Legacy outlet, and formal EXE same-state visible mark comparison. The controlled pixel pass cannot close these. No computer-use, second Editor, DAT edit, asset deletion or Scene save was used.
