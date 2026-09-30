# Q09/P-09 revive lives camera-left gate, scoped acceptance (2026-09-28)

Status: `FOCUSED_TEST_PASS / NATURAL_GPU_PENDING`. Q07/D-024, P-09/Q09 and the master alignment remain open.

Formal source: root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `source/ntsd28_core/src/rendering/render_snapshot.cpp` lines 1835–1838. Revive-lives text is emitted only for selected mode gate other than 3/4, lives above one, and `camera.x <= entity.position.x + 14`.

Unity first difference before change: `BattleEntityOverlayLayout.TryBuild` had the first two conditions and omitted the camera-left gate. The shared layout now uses captured `XInt - CameraX + 14 >= VisibleLeftPixel`; labels retain their independent clamp. The literal 14-pixel display margin does not change the user's fixed-camera movement-distance scaling or any gameplay position.

Original Unity Editor 2022.3.62f3 PID11944 was reused; no second project or computer-use. The new focused EditMode edge test passed 1/1 (`b5f6f6c913e54a9c872156090ce1f018`), then adjacent mode/viewport tests passed 2/2 (`ce07cd0291aa4afa9c6d92ff1b78945d`). An inherited self-check initially failed 0/1 (`d5ee9f7cc7a74d3f99916a89a6d8f9fb`): its old offscreen-left counter expectation contradicted the newly enforced formal gate. After changing only that expectation, final original-Editor rerun passed 2/2 (`5679e297af6848889349620bc9a3cc62`). The final test checks X105 vs X106 at cameraX20/visibleLeft100 and mode3; labels remain present. Original Editor ended idle, not Play, in Menu, with compilation and test runner idle.

Generated `Assembly-CSharp-Editor.csproj` build: 0 errors/230 warnings, both before and after the self-check correction. The final `Battle/Menu/GameConfig/ProjectBattleModeConfig` Git status is clean; SHA-256 respectively `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`. `Tools/Validate-ChangeLedger.ps1` exited 0 with 971 records and five governed changed scripts covered; `git -c core.safecrlf=false diff --check` exited 0.

Scope limit: no natural Battle Scene GPU glyph comparison, Legacy body pixel capture, or formal EXE same-viewport visual A/B was performed for this gate. These remain P-09/Q09 exits; the isolated test result is not aggregate alignment completion.
