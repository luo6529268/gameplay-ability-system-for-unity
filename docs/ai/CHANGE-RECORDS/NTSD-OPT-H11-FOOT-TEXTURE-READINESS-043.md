<!-- CHANGE-RECORD
id: NTSD-OPT-H11-FOOT-TEXTURE-READINESS-043
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeFootMarkerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user 2026-10-07 stage completion boundary clarification and six-item H11 contract; preserve existing Foot configuration and presentation semantics
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH43-FOOT-TEXTURE-READINESS-20261007/REPORT.md
-->
# 第43批 Foot纹理准备

当前限定交付：`PARTIAL_REENTRY_WINDOW_ZERO_GC_PASS / RUNTIME_PENDING`；下方PLANNED/结果待记录为历史快照。39/39聚焦通过，01完整1800仍FAIL12迟发未知，02完整1800重进可靠camera/observer0event PASS。阶段H11仍未收口，不用02抹去01、不追加第三同构长窗；H07性能未达、不称本批FPS收益。完整计数、原件SHA及下一动作见REPORT文首与terminal-audit-01.json。

本次交付复核：原39/39 JSON具名summary已重新读出，未重跑；主agent逐hunk审Central28行/测试129行，Probe对43 before仅12菜单行，热采样方法未变，未发现本批跨界修改。新鲜Validate-ChangeLedger（pwsh）exit0/error0/4263匹配WARNING行，git diff --check exit0；CRLF提示不是新编译警告，不将旧声明警告清零。精确before/after与恢复来源在Operation，RUNTIME_PENDING不晋升VERIFIED。

02新鲜实际终态：PASS/DONE，1800/1800、tick8→1275、URP；四校准PASS，camera1800scope/0event、observer3600scope/0event、invalid/unattributed0；每帧Foot/Health下限2，两slot，11570 CPU DrawMesh录制=执行/逐帧差0；1800build（1262publication/538alpha）、1414336 entity顶点bytes/1800上传；fail/reject/growth/非零CPU lease帧均0。十一阶段关闭三残留0、Scene clean/SHA同。无Profiler/capture/千人新测，Play期间未派MCP命令。

01计数口径更正：原JSON累计camera1801/observer3602scope，存储帧仍1800；此前简写observer3600仅存储1800帧的成对scope口径，不得改写原累计数。12event全在存储ordinal937，raw300是TimeNanoseconds不是bytes。01仍有效FAIL；02受控不派MCP观察的PASS不证明01工具因果。关闭原件保留，既有Foot诊断默认false字段没有启用，不当作runtime禁用事实或新缓存实际6帧反射证据。

结果DONE后原PID19040真正idle/nonPlay/noncompiling/testsInactive；Battle11roots clean→正常加载saved Menu单Scene8roots clean，未保存Scene。21guards/9backup/5pre-camera指纹及HEAD同，全部原dirty保护。无第三窗口、其它专项/代码更改或Goal停止；H11未知归因留门，后继H07基于当前普通Brute残余热点继续必要判断，不重开四已过评估。

PLANNED。Task与Operation先于任何脚本写入；实际验证随后追加。

test-first实际：仅既有Foot测试添加8个具名readiness断言/private storage反射；原Central生产SHA保持，21guards mismatch0。原Editor all-scope刷新并真实重载至UTC11:02:46新Editor DLL，桥短时6402拒绝为既有domain reload，原PID19040未重启；新状态idle/compile false。仅8具名RED已提交，尚未实现/采集。

有效RED结果：job4b879277b5a34bd5b9ebccae2c6698b5终态failed，实际completed8，八个断言均因新增纹理持有storage不存在失败，red-01.json保留。progress.total9493是runner库存口径，不当作9493执行，本轮只有8。CODE_WRITTEN仅测试已写；生产修复/GREEN/新相机验收尚未执行，不称H11通过或FPS改善。用户此时询问当前条目，说明H11与H07低帧率主线分别状态，不暂停/结束Goal。

本次状态交付前Validate-ChangeLedger实际exit0/error行0/4281匹配WARNING行（历史声明路径警告保留）；diff初次检查发现四个本批文档尾部多余空行，已仅用apply_patch去除本批新增空行，复查git diff --check exit0。无生产修复或新测量；后续继续当前窄域实现。

实际实现：仅Central新增reference Texture2D与帧Texture2D[]强引用；既有settings refresh尾部冷准备所有帧，同长度复用、无feature清引用/空配置清空。ResolveRuntimeFootMarkerTexture及ResolveSprite完全原样，未加新的热访问/分配/排序分支；Probe只加两个fresh43/1800camera入口、不打开42 capture。确切wrapper推断待完整camera验收。当前新代码尚未GREEN。

工具复核限制：只读execute_code首次缺action而拒绝；读取既有工具contract后codedom执行仍因mono命令文件名过长失败，没有跑检测代码或改设置，不重复/不改插件。当前pipeline用42真实URP结果与本次GraphicsSettings绑定同SHA保护复用，43入口还会实时DetectPipeline。原Editor仍可用MCP、非SafeMode，不启动另一实例。

GREEN实际：原Editor runtime/Editor DLL UTC11:11:32/35后已重载，error CS查询0。jobb863733a9f304c8487c693338224371d实际39/39 PASS、0skip（2.7153243s）：Foot24含新增8/原16，辅助容量与lease15。强引用、同长度复用、配置移除/替换、显式authoring、强制GC后4096次texture采样0B及整份overflow/已lease保护通过；仅覆盖此断言范围，不是完整相机/FPS证书。21guards/9backup/HEAD同，git diff --check exit0。下一按Task仅必要原saved Battle两次完整1800camera，不启Profiler/capture/1000AI。

camera-01提交：原Menu单Scene8roots clean/idle/nonPlay/无test，新三源及GameConfig/GraphicsSettings当前SHA在pre-camera.json冻结；正常打开saved Battle并调用43新入口，实际terminal结果待。校正上段pipeline“同SHA保护”措辞：21原guards未含GraphicsSettings，之前只复用42URP运行事实及本次绑定读取，不称execute_code成功；现在GraphicsSettings/GameConfig另在pre-camera冻结，43 report将由既有DetectPipeline实时读取。没有保存Scene、启动第二Editor或Profiler。

camera-01终态：完整1800/1800、实时URP、tick8→1292，四正反校准PASS、observer3600scope/六子块0event及invalid/unattributed0；严格camera FAIL12，全部集中ordinal937/unity4048/tick678/slot1，前8及其余1799frames0。Foot/Health每帧至少2、两slot，11536 CPU DrawMesh录制=执行、growth0/CPUlease0、11stage三残留0、Scene clean/SHA同。12事件的真实调用点UNKNOWN；不从raw TimeNanoseconds300换算bytes，也不因早期40B未再现就宣称完整0GC已闭合。

下一必要重进仍用已声明camera-02/同代码同scope，不重测第三长窗：此次仅有一个迟发事件簇，并非最初8帧Foot事件；本批在观察中执行过MCP get_editor_state，返回observed_at1791371814280，时间与迟发簇接近，但只有相关性，不能当归因。第二次原定重进时不在Play期间派发任何MCP查询/Console/代码，改为进程外只读结果文件和PID，核查工具观察扰动并做必要重进；不改变生产/首帧/完整scope/校准，不丢弃01FAIL。若02仍非零，不盲重采，按新site缺口处理；若02为零，只报告该窗口通过，01异常归属未闭合时不晋升整个H11。本动作回答不同的观察扰动问题且覆盖原定重进，不以次数停Goal，也不降低验收。

原状：既有Refresh只取reference Sprite，未来帧.texture不被中央绑定缓存强持有；42真实40B调用栈定位该动画选择链，wrapper首次物化仅推断。预期：设置时冷取全部纹理并持有，动画选帧/回退/RenderPass原样，既有6帧资源无需修改。

准确路径、owner/容量/生命周期/内存、8 test-first与原两次1800camera完整验收、回滚均见[Task](../TASKS/NTSD-OPTIMIZATION-BATCH43-FOOT-TEXTURE-READINESS-20261007.md)与[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH43-FOOT-TEXTURE-READINESS-20261007/RECORD.md)。没有新规则字段/Runtime owner、Sprite/Texture像素分配、segment更改或shutdown重排；原dirty/失败证据保留。H07/H11未达、Goal active，次数只复盘。
