<!-- CHANGE-RECORD
id: NTSD-OPT-M03-RUNTIME-FOOT-CONFIG-022
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCentralRuntimeFootMarkerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs
authority: user six item bounded phase and Foot GameConfig direction; start execution 2026-10-07; formal336 unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007/REPORT.md
-->
# 第22批生产 Foot 配置接线

脚本前 PLANNED。准确原状、路径/符号、副作用、固定测试/生产窗、风险、关闭依赖与经批准回滚见 Task。
只补无显式 authoring 的 GameConfig fallback；显式设置含禁用保持优先。复用现有资源/动画/锚点/batch，不新增生命周期模块或模拟写入。
原 Editor 6401 已观察 idle/nonPlay/Menu clean/testnone；CLI STATUS_NO_INSTANCES 为 Pipeline 包缺失，不启动新 Editor/安装包。只读动态 execute_code 因 CodeDOM argv 路径过长失败，尚无代码执行结果；后续复用已存在 MCP 与编译菜单入口。
本批占有限首阶段新子批1/8；修复复验轮0/3，新诊断0（复用第21批）。不称编译、测试或运行时通过，不关闭M03/H11。

test-first追加：新增8个 RuntimeConfig 方法（duration两case，共9case）及隔离fixture；生产else未修改，独立菜单尚未新增。fixture只借用/恢复私有global config并回收测试创建对象。固定RED请求先只选 RuntimeConfig 方法，不把已通过负例算生产修复；尚待原Editor执行。

RED原件：job52bf1cf1e14e4cb99491d1816b35bb22，实际completed9，6failure/3pass；9294为discovered而非执行数。5failure证明fallback缺口，1failure为新增禁用断言错误：现有Preview允许保留Sprite但disabled。已仅把该断言改成保留原引用、disabled仍要求false，不改Preview语义。red-results.json保留。
实现追加：生产仅无authoring else读取既有GameConfig/动画引用/duration；新增独立Batch22入口（严格1800窗+catalog replay，旧门不改）及只读DetectPipeline。status CODE_WRITTEN；原Editor重载/最终固定focused/实际窗口待运行。本候选首轮实现，尚无实施后失败，parent修复复验轮0/3。

compile/focused：原2022.3.62f3 Editor实际domain reload后idle、Menu、error CS检索0；生产DLL SHA1B80F9829E5365C98359D10D9719FF722CAFAE67FB4A5582BE2FA797F93B967A，非第21批旧DLL。job5f97de1b112442929da6bd49e3637e36固定三类实际32/32 Passed（新9+旧23），0fail/skipped；9294仍仅discovered。texture selection4096 warm循环0B只证明该局部路径。Ledger预检exit0，3脚本全覆盖，既有warning不清理。生产1800严格窗口/退出与Menu恢复待执行。

最终限定：SCOPED_RUNTIME_FOOT_CONFIG_PASS；VERIFIED只覆盖本Task。原Battle1800camera/tick8→1499，每帧Foot2/Health2，两slot、11815 CPU draw录制=执行、growth0；三档100/500/1000重复production命令各64warm+1800samples/局部0B/growth0，非1000AI。Editor两硬门false且global三代collection各3，不称完整全域0GC。11阶段objects/slots/borrowers0、Scene clean/SHA同、Menu8root恢复。30保护/11备份保持，HEAD未变，无生产候选失败修复轮；新测试误断言纠正留原件。H11/M03父项OPEN、六项阶段IN_PROGRESS、新子批1/8完成，Goal不得complete；无Scene/资源/设置/专项门变更。实际命令、结果、局限见REPORT及原JSON。

