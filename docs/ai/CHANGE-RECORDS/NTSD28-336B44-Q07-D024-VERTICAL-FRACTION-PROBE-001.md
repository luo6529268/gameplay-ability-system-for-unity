<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-PROBE-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07AirborneIdleBattlePlayProbeEditor.cs
authority: User D-024 all-entity screen-fraction movement; official 336B44 playable sprite projection and existing Guren OID85 natural root/Scene trace
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001.md
-->

# NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-PROBE-001

2026-10-05 交付前最终校验：准确脚本范围未扩大；`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 退出0/PASSED，1267records/18dirtycodefiles；`git diff --check` 退出0。总表、STATE、handoff、当前authority、Task及调度清点已回链限定R120结果，必要ONE恢复0；不运行全套NUnit或新增SelfCheck矩阵。

2026-10-05 R120新增出口已限定通过：[原Scene报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/R120-SCENE-ACCEPTANCE.md)及唯一原始 `d024-vertical-r120-20261005-01.json` 为PASS/DONE。原Editor经MCP刷新，单次原Battle Scene32tick；相对23/全局28的自然alpha0.0156545455392932，源preciseY -22→-20.3、Z402不变，正式lround deltaY=-2；生产本体观测差3.1561852206927217视图px，对独立2×1152/730误差0.000020837131078，阴影差0。完整32samples与修复后基线严格相同，相关源/正式根原件SHA复核不变，复用1690/1658字段对照和独立CRT seed限制，不声称新native运行或全World一致。生成Editor0错/原Editor Console0error，退出nonPlay/idle/Scene clean/root11/SHA稳；请求按预登记Operation恢复68原字节且SHA一致。没有生产、资源、Scene或非战斗改动；本探针状态FOCUSED_TEST_PASS仅覆盖本样例中央命令，不是设备实际120FPS、GPU Present或全部角色。新增必要ONE已完成，不重跑已过邻例；父生产Record及Q07/Q09/Q12保持开放。下方PLANNED/COMPILE_PASS是执行前历史快照。

2026-10-05 R120脚本后增量：只改既有声明路径，抽取 `CapturePresentation(sample,requestedRenderFps)` 共用原本体/影子读者，原R30字段意义保持；增加 `InterpolationSample`、`d024-vertical-r120-` opt-in和相对23自然中间alpha采样。记录已发布相邻motion states前后preciseY/Z、实际alpha、源lround delta、统一Y倍率预期、本体及地面命令观测差；只要求本体高度偏移、Z恒定时影子不动。两次显示策略切换均finally恢复。未改生产、DAT、图片、Scene/Prefab/配置或非战斗。生成 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 退出0、301warnings/0errors；原Editor导入及单次原Scene取证尚未运行，状态COMPILE_PASS，不用旧R30 PASS替代新增验收。

2026-10-05 R120必要出口脚本前增量：此前R30原Scene取证仍为历史限定PASS；当前版本准备在同一声明路径加入 `d024-vertical-r120-` opt-in。继续使用原Guren84→OID85、原32个生产Driver tick、相对21/23的旧完整位置采样，不增加角色/操作矩阵。在相对23、Driver暂停且无worker飞行时，使用已发布相邻motion states，临时设置Play clone的120显示策略，通过生产 `BattleCentralRenderSystem.PrepareFrame` 读取自然时钟实际alpha、本体/阴影命令；再读取同tickR30完整位置，策略在finally恢复。新增独立interpolationSamples，记录前后preciseY/Z、正式lround位移与观测命令差，要求0<alpha<1、源Y差有区分力、本体仅按统一Y倍率偏移且影子不随Y。旧请求/32tick规则样本/旧完整visualSamples保持含义。

authority为当前336B44正式 `presentation_interpolation.cpp::sample_render_presentation28` 的相邻tick/身份/关系/连续性门、preciseY先lround再本体deltaY、影子只deltaZ，与用户D-024及1.5倍显示例外。风险是自然命令生成超过33ms使alpha到1、动作/关系断开插值、缓存同tick不刷新、测试临时策略未还原；失败原件保留并安全退出，不反射改时钟、注入alpha、sleep/忙等或改生产。一次具名原Battle Scene有效试验；生成Editor/原Editor0错、同1690规则样本不变、退出Scene clean/SHA稳为验收。原Scene命令不等同于GPU Present或设备实际120FPS。回滚只手工移除本Record新增分支/样本与共用采样抽取，保留原探针及其它用户修改；临时请求另登记逐文件覆盖/恢复Operation。当前PLANNED，尚未修改脚本或运行新增分支。

脚本修改前登记。原探针仅记录 Guren OID84→OID85 的32个生产 tick 规则字段，现有正式根/原Scene相对tick21～23的 OID85/action212/Y=-22→-20一致，但未记录中央本体/阴影画面命令。正式视口高730，Unity当前1152，现有本体pivot直接用未投影Y；[只读报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/REPORT.md)只得到公式候选，不能当成原Scene实测像素。

预定改动仅在已存在的 `NTSD28Q07AirborneIdleBattlePlayProbeEditor.cs` 增加 `d024-vertical-` opt-in 请求：在相对tick21和23，复用既有生产 `StepOneTick(buildPresentation:true)` 后的当前发布帧/中央 `BattlePixelFramePlan`，记录 OID85 所在 slot 的 Entity/Shadow 命令世界Y、相对地面高度、tick、动作/Y/Z、相机视口与是否物化；旧runId及原32tick规则报告保持原逻辑。唯一新结果文件，不覆盖旧原件。没有生产、DAT、图片、Scene、Prefab、配置、Unity/GAS或正式源码改动。

风险：中央命令在该 tick 可能尚未物化、Camera不可用、原 Editor/Scene被其他任务占用，或发布副作用使原探针时序变化。仅在原Editor非Play/idle、单个干净 Battle Scene、正式内容、脚本已导入、无worker飞行前运行；条件失败时记录并安全退出。相对tick21/23与原样本规则字段和帧号需同，退出 Scene clean/磁盘SHA不变；不重复角色矩阵或全套测试。

验收：生成Editor工程0编译错误、原Editor编译与一轮具名原Scene探针 `PASS/DONE`、两帧同slot/action212/Z402、发布/计划tick一致、本体/阴影命令有效、规则字段与既有正式根行同；`body-ground`画面位移按实际记录与2/1152、2/730比较。此测试只证共用生产画面命令，不证明正式GUI实际GPU逐像素或全Y碰撞合同。若结果未支持差异，不进行生产修改。脚本改动后立即在此追加实际路径/验证/未验项；完成时运行 `Tools/Validate-ChangeLedger.ps1`。回滚限本Record新增opt-in代码，保留所有用户改动和诊断原件。

2026-10-05 实际代码：仅改上述 Editor 探针，增加 `d024-vertical-` opt-in 的 `VisualSample`，在相对 tick21/23 用生产 `BattleCentralRenderSystem.PrepareFrame(world)` 读取同 slot 的本体/阴影命令位置；暂将 Play clone 的 presentation policy 设为 R30 并在 `finally` 恢复，记录 alpha、发布/计划 tick、动作、源 Y/Z、相机高度、命令个数和本体减阴影距离。原 request/32-tick 路径未改变。当前仅 `CODE_WRITTEN`，编译、Editor import、定向 Play、Scene clean 与实际比例结果待验；没有生产 Y 修复。

2026-10-05 编译验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功，301 warning / 0 error。`Tools/Validate-ChangeLedger.ps1` 返回 `PASSED`，本次两份受治理脚本差异均被 Record 覆盖；众多旧 Record 路径不在当前 diff 的提示是历史警告。定向 `git diff --check` 0 退出。原 Editor 导入及 Play 仍待；状态仅 `COMPILE_PASS`。

2026-10-05 原 Editor 定向结果：[唯一原始 JSON](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-20261005-01.json)为 `PASS/DONE`，Play 全局 tick5→37，OID85 slot51 于相对tick21出生；tick21/23同action212、Z402、源Y-22→-20，发布/plan tick一致、alpha1，本体/阴影各1命令。相对阴影距离由21.9999313到19.9999809视图像素，变化绝对值1.9999504；[比较与界限](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/REPORT.md)说明其占比为正式投影的0.6336648。原Editor已退出Play，Scene clean，磁盘 SHA前后及复核同为`253B2EBA…78F9010`。这使测试探针达到 `FOCUSED_TEST_PASS`；生产Y/floor/碰撞修复、正式GUI同输入GPU逐像素和最终Q07/Q09对齐均未完成。
