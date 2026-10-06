# NTSD 优化文档整理文件操作记录

Operation ID：`NTSD-OPTIMIZATION-DOC-REBASELINE-20261006`
状态：`VERIFIED (DOCUMENT_ONLY) / IMPLEMENTATION_WAITING_USER_APPROVAL`
类型：文档原位修订与新增；不删除、不移动、不替换资源、不执行 Git 丢弃。
Task：[文档整理任务](../../TASKS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006.md)；Change ID：不适用（无脚本修改，不登记为代码实施）。
执行者：当前 Codex /root；工作目录：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity`。
操作前时间：2026-10-06T16:26:55.0266503+08:00。

## 用户授权与边界

用户本轮原文：“那就开始整理吧，然后整理没问题后，告诉我，我来告诉你是否可以开始”。
承接明确的“仅文档修订”范围：更新优化登记表、对应独立方案及三份关联计划，
不改 C#/shader/Scene/Prefab/资源/ProjectSettings/Packages，不启动 Unity、测试、Profiler、
Frame Debugger、GPU capture 或 M0；实施与测量等待用户再次批准。
EXT-1 不升格，仍为 `PROPOSED / MODIFY_REQUIRED`。既有未提交修改均保留。

## 操作前逐文件清单

固定恢复 commit：`11069c9fc468d8066e8d6cd4fe9e33dd7740b069`。除 INDEX 外，下列文件在操作前 git status 中无差异；
可从该 commit 的精确路径读取旧正文，按新授权生成逆向补丁，不使用 reset/checkout/restore。
INDEX 有用户修改，不能以 HEAD 为备份；先复制到本目录 `INDEX-before.md`，
必须核对与下表 SHA-256 相等，之后只追加本任务记录。

| 相对路径（绝对路径均为上述工作目录加此路径） | 字节 | 操作前 SHA-256 | 状态与恢复来源 |
|---|---:|---|---|
| `Assets/NTSD/Docs/android-mobile-readiness/m-11-mono-core-presentation-layering.md` | 4127 | `BE7EE698E9E059503062E800FC1A3BCB8CB123A154AD76AC5EE051FFE67C990D` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-10-mobile-viewport-safe-area.md` | 2035 | `A88E28522C60829096C0744EE27C872383F347106D8698A9AD3D264DD72EFF7D` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-09-performance-evidence-fingerprint.md` | 2007 | `4B45174F21FB0F63F3ABB2CA3749DB2066087E58FA275F2E47C25FF143096F57` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-08-sustained-thermal-performance.md` | 1957 | `D13FD658A24855FD654E186AA8D6EF3249380C9214A4B5DEDFE13367C0C5028E` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-07-android-graphics-api-policy.md` | 2049 | `392BDBE043A65A0FFF6BBD8FD4FFAC8C3DCA1694167194F3034FFB844D7BC0EE` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-06-pool-capacity-prewarm.md` | 2184 | `4DC6D0130C74C76679C337223D95D70E46F04FCE6560CFFB3F675975C3A2D55F` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-05-gameobject-shell-cost.md` | 2243 | `2AC4AD2EEE4C13AD23A753018E3DF9B9E013A1D9D8A516364B0B412E12329B3B` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-04-centralonly-fail-closed-diagnostics.md` | 2249 | `C19BEADFEC6DC77C3306E8789521269935DD35CFE8D233154D9AD7999A7A9396` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md` | 2220 | `1F8D891FFCF2985092EDFD6E9433DA8731F58293A616DDE80EFC5588B7543552` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-02-central-render-draw-segmentation.md` | 2179 | `763216774EED94C5A32F484E22025398721462EF995960B2AF29D4603FD2BD84` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/m-01-dedicated-worker-throughput.md` | 2292 | `0BE5293DEDC98E4F1533A0341562173836CCBB9CEEBED734AAB18E77676303A0` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-08-release-packaging.md` | 2003 | `056E0BC04E077019DA41B1BEF2C85A3F92A902815B0F9BF78DB1AA32D37D03DE` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-07-ai-profile-description-drift.md` | 1758 | `DC4F61047B75CC92D66C890E6040B0E58144989F089C4666BF7BAD6612CEC653` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-06-urp-hdr.md` | 1694 | `B6C9A0C0F6627B296518999B64D0E059CF872AB7270D91CC715ABDFB690C45D3` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-05-graphics-jobs-frame-timing.md` | 1805 | `6423D8E9A7BEE5790CC81D733D3AB8ACC57BC6231DC42590D10DEB176DFB01B7` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-04-orientation-policy.md` | 1651 | `5B66139B0123CACCFE46A7DED623A6E1A10E833C62896901DA54D0D00EB66C29` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-03-target-api.md` | 1741 | `1FB93F09AEDFF6BA6A59BAAAD187D50996625A91115B66B91E2671CB261F12A1` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-02-signing-keystore.md` | 1759 | `E93AC01025646D80F3CFB0782FE6921B104025E639172378578C9A6127A8D47F` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/l-01-application-identifier.md` | 1653 | `051D1F165C21E4FD776DCAC1813493789FB0B95A27D332E8A5285E712936C2A6` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-09-ntsd28-correctness-alignment.md` | 2788 | `34A9A01FFAC52C1F0A8493C14CF5A99260951A058D5B3BD670B3C06FE3C4B427` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-08-prebaked-visual-content-memory.md` | 3724 | `1B38F993C223D7EC0423E54CB798280899AF0EA41416D7ED712A4776BF02D4E6` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-07-fresh-1000-ai-certificate.md` | 2556 | `0FF5D2D2528AB7DE13B5126878EB4D91E4C68D1982373A49F34D5FAF81D54DCB` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-06-collision-broadphase-1000.md` | 2776 | `B7418F371C5CF726938592F1BF95AB840BBE75B90ED9CCE92658F395BF0261A5` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-05-device-gpu-certification.md` | 2580 | `9448401E47ABD302F6E364327F526B31B6878A2344A3AC91B364010BEE9359C9` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-04-il2cpp-arm64.md` | 2506 | `04676A0AF2D3554C0B9BA2912931C4CED55CF6C5AC66DEB804CB1A90FD81BCAF` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-03-touch-input-fixed-tick.md` | 2665 | `74E3A1AC50483F7853DD7486907F01C99F1478DF1691A1AEEE7A7E550154123B` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-02-android-build-scene-closure.md` | 3373 | `E683266672B3545A7E2BC18A82F4E48AB148B682C82D082BD586A36A3296A7D0` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness/h-01-content-deployment.md` | 3675 | `B03B89FFEF2245789A1C24C17CEEB6E42D0F027B757A3103D96C9B37FF95761B` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md` | 13772 | `B48A9C0D0EE67CC3CECEA27F2B05FCEF8AB08A4571BFD86E6BF0BC2828E7FCF8` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/battle-performance-stats120-roadmap-plan.md` | 26589 | `A74C41CAFB5DC9985151F24AAFCE1246CBD82403E9999285D7331B3110232F16` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/battle-atlas-memory-lowend-roadmap-plan.md` | 21554 | `192475165F7CF25A581EB1454F8B4E2ECE9D0BD493BB0DF44B560C3C5212C182` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/simulation-mono-nonmono-boundary-refactor-plan.md` | 40228 | `9A82F34649F027A46DA9D33808FAA627BC22CF0B146C3CEA0F0F1B86F52250F9` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `Assets/NTSD/Docs/battle-optimization-proposals-index.md` | 26728 | `ADC250860C80C197AB57D098DA04E3FF374A9DFA11F51AC8E5FDB1C1B2FB933D` | 已跟踪且工作区无差异；按固定 commit/path 恢复 |
| `docs/ai/FILE-OPERATIONS/INDEX.md` | 21110 | `3B9182073F8080087EEEB183AFC9660C970E0279BBCC9A1A2D88DD720877D4A8` | 已有用户修改；完整字节另备份 |

## 计划新增文件（创建前须确认不存在）

- `Assets/NTSD/Docs/battle-optimization-rebaseline-and-start-gates-20261006.md`
- `Assets/NTSD/Docs/android-mobile-readiness/h-10-battle-audio-pcm-memory.md`
- `Assets/NTSD/Docs/android-mobile-readiness/h-11-presentation-capacity-zero-gc.md`
- `Assets/NTSD/Docs/android-mobile-readiness/m-12-decode-upload-byte-backpressure.md`
- `Assets/NTSD/Docs/android-mobile-readiness/m-13-sound-event-aggregation.md`
- `Assets/NTSD/Docs/android-mobile-readiness/m-14-content-verification-io.md`
- `Assets/NTSD/Docs/android-mobile-readiness/m-15-kernel-build-reproducibility.md`
- `docs/ai/TASKS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006.md`

本记录及 `INDEX-before.md` 位于业务文档范围之外，仅用于本次操作审计；不修改 .meta。
现有历史正文和修订记录保留，过时操作条款以注明日期的新基线更正，不伪造旧结果。

## 操作前保护对象

以下已存在的脚本修改不在本任务写入范围；哈希仅用于观察同期变化，不据此覆盖外部工作。

| 路径 | 操作前 SHA-256 |
|---|---|
| `Assets/NTSD/Scripts/Test/Editor/NTSD28ShadowRetirementEditorTests.cs` | `70357996DD876DF63E23D05464BF6ED6D19FA4545D869B1A0115F89C51566801` |
| `Assets/NTSD/Scripts/UI/CharacterSelectionBoard.cs` | `BB4E1006047D55043E297360EFD9DB6B9C8CAD50E2C03B17E985E6D4B318B049` |
| `Assets/NTSD/Scripts/UI/CharacterSelectionController.cs` | `9F70C6CACD3C84BB1DCBAB3B1053D656C5DE811315E8F0848EFFE5A39B58FE52` |
| `Assets/NTSD/Scripts/UI/SelectRoleItem.cs` | `1EF76855E15D6F3BFBA2853F8291FFCAB35115751C37A6BBF459348249E63CD1` |

## 拟执行命令 / API

1. 只读：`rg`、`Get-Content`、`Get-FileHash`、`git status --porcelain`、
   `git diff` / `git diff --check`、本次进程内 Markdown 链接/条目一致性检查。
2. 备份：`Copy-Item -LiteralPath docs/ai/FILE-OPERATIONS/INDEX.md -Destination docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006/INDEX-before.md`；
   目标已存在则拒绝，不覆盖。
3. 正文修订和新建：仅通过 `apply_patch`，仅触及本清单；INDEX 仅追加。
4. 验证：读取实际 diff、ID/优先级/状态/相对链接，核对新增路径与脚本保护哈希。
   此为文档静态校验，不执行任何项目代码、测试或测量。

## 执行后记录

2026-10-06 收口补充计划：在本 Operation 目录新增 `validation.json`（本次文档
静态核对结果）与 `after-manifest.json`（实际文档写入清单、SHA-256及保护对象观察）。
两文件须不存在才创建，均仅由 apply_patch 写入，不运行项目测试。
新增文档对应的 .meta 若由同期其他进程生成，只记录观察，不修改、不清理，
不把执行来源未核实的自动生成归为本任务写入。

### 实际修订与核对（2026-10-06）

1. 仅使用apply_patch修订清单内Markdown；dirty INDEX先按计划Copy-Item创建
   `INDEX-before.md`，备份SHA与操作前INDEX相等。未删除/移动文件，未执行Git丢弃。
2. 33份原有业务文档修订，7份新业务文档创建（共同复核+6份独立方案）；
   另建Task及本审计材料。主表34项，高12/中14/低8，保留原28项ID/路径。
3. 首轮静态核对42份Markdown/127个本地链接，无ID、优先级、状态、章节或链接错误。
   新增行尾空格曾令首次diff检查失败，已用最小补丁修正并重跑成功；
   最终机器可读结果见 [validation.json](validation.json)。
4. 操作后文档、INDEX/Task/Record、保护对象及同期metadata观察清单见
   [after-manifest.json](after-manifest.json)。该清单不递归哈希自身或validation附件。
5. EXT-1第七节至文件末尾UTF-8正文SHA-256操作前后均为
   `02A6712DA32E9F104EF9ADCAECA2237F98EE584EA1AF411EF21DB9375BDEEC26`；
   仍为PROPOSED / MODIFY_REQUIRED，不修改该节、不升格、不启动专项M0。
6. 4个已登记dirty脚本在本次观察中SHA不变；未触及任何C#/shader/Scene/Prefab、
   已有资源、ProjectSettings、Packages或Server。其他任务的dirty内容不回退、不覆盖。
7. 6份新独立方案对应的.md.meta在操作后status中出现；本任务未创建/修改它们，
   生成进程来源未确认。仅记录SHA并保留，不清理、不把该观察称为Unity验收。
8. 未启动Unity/测试/Profiler/Frame Debugger/GPU capture/M0/真机测量；
   未安装hook、未改Git配置、未提交或push；无代码ChangeLedger状态晋升。

恢复限制：本记录是文档整理审计，不授予回滚权限。需要恢复时由用户明确批准，
先核对当时工作树及本次before/after SHA，生成审查后的精确逆向补丁；
不得覆盖记录后由其他任务追加的INDEX或任何业务内容。

