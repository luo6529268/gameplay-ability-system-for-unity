# Q09/P-07 当前共享阴影门的中央自然 Play 回访

状态：`CURRENT_CENTRAL_NATURAL_PLAY_PASS / P07_AGGREGATE_OPEN`。仅对所选正式 BMP `shadow:1` 在当前 Unity 生产中央出口的自然生成链给出限定运行证据，不等于 Legacy 真实画面、OID518 专项像素或正式根 EXE 同视口画面对齐。

正式根 `NTSD2.8-Logan.exe` SHA-256 本轮重核为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。正式 OID204 与 OID518 的定义均可进入相同 BMP `shadow:1` 通用禁显读者；OID204 的李 J→L 原 Battle 探针已具有完整 Driver、自然子体和同帧普通阴影控制，因此选它回访共享代码，不按 OID 再造特例。配对正式 Session 的李子体五个本体/零子体阴影见 `NTSD28-Q09-P07-FORMAL-LEE-SNAPSHOT-001/ACCEPTANCE.md`。Unity 原 `NTSD28-Q09-P07-NATIVE-SHADOW-GATE-001` 聚焦测试 4/4 包括中央冻结快照和 Legacy 管理态，本次只补较晚代码版本的真实中央 Play。

原项目唯一 Editor PID11944、原保存 `NTSD_Battle.unity`，请求前 MCP `get_editor_state` 为 idle、非 Play、无编译和测试。`Assembly-CSharp-Editor.dll` 的本轮时间戳晚于共享门生产文件和现有李探针文件。只用既有 `Temp/NTSD28_Q09_P07_LeeShadow.request.json` 可选入口提交 `lee-q09-p07-shadow-current-03`，不开第二 Editor、不改任何脚本/DAT/Scene。旧 consumed 请求先复制到 `prior-lee-shadow-request-before-current-play-20260928.json`；结束后原路径已逐字节恢复到请求前 SHA-256 `7E9E073049E920007C061E68DD2ACDD8EABF5A11B26E41328A76AB70A4ED9AA8`。

原始结果 [`lee-q09-p07-shadow-current-03.json`](../NTSD28-Q07-LEE-JL-BATTLE-PLAY-001/lee-q09-p07-shadow-current-03.json) SHA-256 `CA1BBC770357135EFEF5CA2F023EDAB09F67BE7532F6B9BE26CFC0095B1F3312`，`status=PASS`。正式 LoganRuntime，完整 Driver ticks5–50；tick12 自然生成李 owner 的五个 OID204，BMP `shadow:1` 为5/5、中央实体阴影快照5/5均受抑制、本体命令5、子体阴影命令0、同帧普通阴影命令5，中央计划有效。报告 `stopped=true`、退出借用 `borrowersAfter=0`、相机状态恢复。结束后 MCP 状态 idle/非 Play/无编译或测试；Battle/Menu Scene SHA-256 分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，GameConfig/项目 mode Asset 分别保持 `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。

本轮没有相机 PNG 或正式 EXE GPU 输出；不能由零子体 Shadow 命令推导所有模式、对象或显示层均对齐。Legacy 生产像素和正式根 EXE 同状态同视口仍属 P-07/Q09 后继出口。Q07/D-024 的统一碰撞域选择、Q08、Q10、Q11/Q12及总目标状态均未改变。未运行全套案例，因为通用 BMP=1 路径已有本次自然样例和原聚焦正反门。

文档/审计收尾：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit 0，当前978条Record与15个diff内受治理代码文件覆盖；历史非当前diff路径提示不改变PASS。五个相关文档 `git -c core.safecrlf=false diff --check` exit 0。此回访未修改项目脚本，所以没有重复运行生成工程编译、原Editor聚焦4/4或整套SelfCheck。
