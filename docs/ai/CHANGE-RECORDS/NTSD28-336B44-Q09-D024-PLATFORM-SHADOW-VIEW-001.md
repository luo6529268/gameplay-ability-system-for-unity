<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationCommandWriterEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040NaturalScenePlayProbeEditor.cs
authority: user D-024 all-entity screen-fraction requirement; current formal 336B44 playable platform30 source-height producer and render_snapshot shadow consumer
evidence: artifacts/diagnostics/NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-20261005/REPORT.md
-->

# D-024 平台阴影高度比例

2026-10-05 终态更正：`VERIFIED (scoped shadow display conversion)`。原Editor精确4/4 GREEN，原Battle Scene单次 `d024-shadow-height-20261005-01 / CAPTURED/DONE` 全局5→36、31tick；三项源10C=-50/-58/-64的实际中央阴影高度分别-78.904106/-91.528752/-100.997259px，与独立1152/730期望最大残差0.000016px内。既有规则samples与旧自然链前31tick逐叶2945/2945同；正常有序关闭五残留0、World解绑/Pool quiesced，后验非Play/Scene clean/Console0error/四保护SHA稳。原件及两个生产位置表达式、独立审阅、失败历史和未验边界见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-20261005/REPORT.md)。没有GPU像素/真人键或Legacy原Scene自然截图证据，不关闭Q07/Q09/Q12/总目标。下方PLANNED/待终态均为过程快照，不再是当前状态。

2026-10-05 原Scene probe生成工程退出0、301warnings/0errors、10.82秒；MCP刷新后DLL已晚于脚本、idle/nonPlay/Console0error、原Scene clean及四SHA稳定，按[具名Operation](../FILE-OPERATIONS/NTSD28-336B44-Q09-D024-SHADOW-REQUEST-20261005-001/RECORD.md)启动 `d024-shadow-height-20261005-01`，尚无终态。请求原本不存在，本次CreateNew后仅消费requested，不删除或覆盖旧内容。四参数GREEN不重跑，只有当前Scene单项运行。

2026-10-05 消费者探针已写：限定前缀的31tick及三个实际ShadowSample，使用已有中央PrepareFrame并恢复30显示policy，源-50/-58/-64/同tick/alpha1/唯一slot2 Shadow及0.002px容差；旧96tick与其它模式保持。4个代码路径均在本Record预声明，新增probe当前仅CODE_WRITTEN待生成/原Editor导入/真实运行；生产状态仍RUNTIME_PENDING。没有新增Manager或改变旧关闭owner。

2026-10-05 原Editor GREEN：MCP刷新后的同两方法精确job `6eaa10789c534d5eb3902db8d1767b47` 为4/4 PASS、0fail，正式summary duration0.8665422s；两出口fixed/-50与identity/+23全部通过，原件editor-green-result.json。生产修复已到聚焦通过，原Scene自然消费者尚待，状态RUNTIME_PENDING；不运行整类/全套，也不据此关闭Q09/总目标。

2026-10-05 原Scene消费者脚本前增量：仅给已有C040探针的mode=platform新增 `d024-shadow-height-` 前缀opt-in；`StepOneTick/CompleteMeasurement`在该前缀用既有自然输入前31tick，旧96tick及其它模式不变。新增只读三项ShadowSample和 `CapturePlatformShadowPresentation`，于29～31经既有中央PrepareFrame/当前快照取slot2实际Shadow命令，暂用30显示policy并finally恢复，核同tick/alpha1、源10C=-50/-58/-64与实际投影高度，不拍全角色矩阵。Report新列表单独保存、规则samples不变；关闭继续旧有序owner。该方法未写/未跑，临时请求操作必须另登记Operation后才发起。新路径旧字节将保存同before目录；此增量属于同一Shadow共用消费者验收，不是另一战斗行为。

2026-10-05 修改后生成Editor工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 退出0、334warnings/0errors、15.14秒，原件editor-green-build.log。已请求原Editor MCP刷新；精确4项GREEN及原Scene消费者仍待，不能由生成工程编译晋升运行时。

2026-10-05 生产增量：`LF2Entity.UpdateShadow` 的显示10C使用已注册World的统一 `SourceDeltaToViewY`（无注册World恒等）；中央 `BuildCommands` 只在Shadow的位置使用冻结frame同一投影。源10C快照/平台写者不变，不改排序Z/脚标/body/名字、DAT/图片/Scene/非战斗。测试仅去掉过时OverlayGlyph位置断言，两方法四参数保持；修改后的生成Editor构建已启动，尚无终态，未刷新或跑GREEN。

2026-10-05 原Editor RED：生成工程0错/301warnings，MCP刷新后精确job `b60f56e93a2a48318a7a6170af014ea6` 实际完成4项（8841是发现总数，不是执行数），1通过/3失败。两个fixed/-50均在Shadow/Legacy目标位置失败，符合漏投影候选；identity/+23的Legacy保持通过，中央第三失败落在非Shadow位置比较。该夹具Entity隐藏且无HitRecord，其余只有OverlayGlyph；当前姓名牌本来消费源平台10C，旧“所有非Shadow位置不变”断言已经过时，且名字已由用户排除。原件完整保留，中央测试将只排除OverlayGlyph的这一无关比较；仍检查同类型/排序/Z、10C源值及唯一Shadow实际位置，不改姓名牌生产或资源。MCP该job最终result字段null，计数和失败由终态progress.completed=4及failures_so_far三项直接证明，不虚构正式NUnit汇总。

2026-10-05 测试先行：既有中央命令/冻结复制与真实Legacy SpriteRenderer两方法各参数化identity/+23、fixed/-50，共4项；期望独立使用正式源高度×1152/730，源10C/排序/其它命令/归零保持。Legacy夹具显式绑定独立World并在finally解绑，避免读取其它World的倍率。改前3个精确文件字节已保存到artifacts/diagnostics/NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-20261005/before；生产尚未改，生成工程和原Editor尚未运行。

2026-10-05 脚本前登记。Unity原状：两呈现出口都将源10C直接加viewZ，平台规则写者已正确保留源值；当前自然三人记录已包含-50/-58/-64。准确路径、符号、预期副作用、保留边界、最窄RED/GREEN及回滚见[Task](../TASKS/NTSD28-336B44-Q09-D024-PLATFORM-SHADOW-VIEW-001.md)。本轮尚无脚本修改、构建或实际测试，不把公式差异称为GPU实测。

本行为只修Shadow显示偏移；不用新manager/queue/pool/cache，无新关闭阶段。已有命令/Renderer owner、有序关闭及世界source字段保持；不改平台规则、DAT、1.5倍sprite、Scene或非战斗。生产改动前先由已有中央命令和Legacy真实Renderer两方法的identity/fixed参数验证候选。
