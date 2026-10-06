<!-- CHANGE-RECORD
id: NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditorTests.cs
authority: User 2026-10-06 requests configurable standalone editor for active-scene TextMeshProUGUI components with unassigned FontAsset
evidence: docs/ai/TASKS/NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001.md
-->

# NTSD-SCENE-MISSING-TMP-FONT-EDITOR-001

原状：MenuFont3500MigrationEditor为静态一次性3500→Plus工具，写死Menu/字体/材质/保护manifest，操作包含图集重建、fallback、Input/SubMesh/RawImage迁移并自动保存Scene/font。当前脚本干净；用户已有字体删除/Plus更新/Scene及其它UI、治理dirty均保留。

需求及边界：用户当前明确要求改为独立编辑器、直接选择目标FontAsset，只给当前场景中未指定FontAsset的TextMeshProUGUI补齐。覆盖原工具内容但保留文件/.meta/类名；不执行旧迁移或改字体数据。现有旧批次事实保留，属于用途替代。

预期实现：EditorWindow/CreateGUI，目标TMP_FontAsset ObjectField、当前活动场景/缺失数量/对象预览、扫描和执行。读序列化m_fontAsset判定null（含失效引用），包含inactive/disabled；跳过已指定字体、其他Scene和TextMeshPro 3D。按钮执行时重扫当前Scene，仅赋值目标字体/对应共享材质，记录Undo与Prefab实例覆盖并标脏，不自动保存。Play/Preview Scene禁用。无写死Font/Scene路径、无字体图集编辑、无TMP_InputField批量操作。

验收：生成工程编译、原Editor导入/窗口打开，聚焦临时Scene过滤与Undo/重扫；仅Editor空闲且允许TestRunner保持原Scene时运行，不丢用户Scene。实际本场景字体替换由用户在窗口选择目标后执行。验证不等于场景已批量替换或美术验收。

风险/恢复：TMP setter的材质同步及Undo需验证；重复执行重扫避免覆写已指派字体。Unity自带TMP默认字体若已写入组件即视为已指定。操作前精确备份/SHA及原Scene/fonts保护在同ID prechange-manifest.json，恢复必须检查后继编辑，不能从HEAD覆盖用户dirty。代码不接入BattleRuntime。

2026-10-06 已写：原类改为EditorWindow，OpenWindow/CreateGUI/FindMissingFonts/AssignMissingFonts，移除旧一次性迁移全部硬编码与资源保存操作。序列化字段targetFont由窗口ObjectField指定并保留窗口重载选择；执行只当前活动普通Scene、含inactive/disabled、空/失效字体引用。同步字体共享材质，单Undo组与Prefab覆盖、失败回撤、手动保存。新增两项Editor测试覆盖双Scene隔离/已分配跳过/重复执行/Undo及窗口目标配置，无真实Scene批量赋值。当前CODE_WRITTEN，验证待执行。

2026-10-06 编译：dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly，compile-01.txt exit0 / 297warning / 0error / 27.35秒，生成工程包含新测试。原Editor非Play/idle/原Menu clean，保护项Scene/font/meta四项SHA相同；已启动定向Editor测试，不运行Play或在真实Scene赋字体。

2026-10-06 首轮测试原件：job83b48335ff234ef29bd795e8b2b2abf6 completed2，窗口配置项通过，场景/Undo项在测试前置失败：TestRunner untitled unsaved Scene不能由EditorSceneManager.NewScene叠加。没有发生赋字体；保留tests-result-01.json。测试改用SceneManager.CreateScene创建两份纯内存临时场景，finally只关闭其自身场景并恢复测试原活动Scene；工具逻辑未因该失败扩大。

2026-10-06 第二轮前置：compile-02.txt exit0 / 297warning / 0error / 14.12秒。原Menu随后观测isDirty=true（来源未确认，磁盘SHA检查另行保留），本任务未保存/清除其未保存状态，未发第二run_tests。新增测试文件内Diagnostics入口，直接调用同两项断言于原Editor，使用纯内存Scene，关闭仅临时场景，恢复原活动Scene并断言原dirty状态相同，全部通过后写唯一新JSON；不经TestRunner替换用户Scene，不运行Play。当前验证仍待最终结果。

2026-10-06 验证适配更正：SceneManager.CreateScene只能Play，首次直接诊断在创建临时Scene处失败，console-direct-check-01.json保留，无通过JSON。最终改为保存过的原Scene旁新增一份未保存的Editor临时Scene；原Scene仅快照并核对现有字体引用作为另一Scene隔离证据，不新增/删除其中对象，不给其字体赋值。临时Scene通过NewScene(Additive)，finally恢复原Scene并关闭自身；TestRunner空未保存入口会Ignore这项，可从Diagnostics在原Scene执行。不为测试保存临时资产或清除原dirty。

最终生产材质处理补充：只用TMP字体setter，该项目LoadFontAsset已负责图集/材质兼容判断；不强制覆写仍兼容的自定义材质。早期“同步共享材质”指不兼容时沿TMP自身行为，非统一覆盖所有材质。compile-04.txt exit0 / 297warning / 0error / 8.64秒；最终验证测试适配后另编译。

2026-10-06 最终限定通过：VERIFIED（仅可配置当前Scene缺失TMP字体工具）。compile-05.txt exit0 / 297warning / 0error / 9.55秒。原Editor Diagnostics两项断言完整通过：editor-check-passed-20261006-100954336.json，检查临时Scene inactive/disabled缺失字体、已指定跳过、原Scene字体引用不变、非活动Scene调用被拒绝、重复执行0、默认材质匹配与Undo恢复、原activeScene及dirty状态保持；另一项验证ObjectField指定TMP资产和未选目标时按钮禁用。不是把旧TestRunner首轮失败改写成成功，直接Editor断言结果独立保留。

最终窗口入口NTSD/UI/补齐当前场景 TMP 字体，已在原Editor调用打开（open-window.json）；目标由用户选择，无默认写死路径，未执行原Menu批量赋值。仅临时场景对象发生字体写入且其自身回收。Scene/font/.meta磁盘保护项及五个before镜像后核对见postchange-manifest.json；原Menu root8、活动Scene恢复、dirty=true保留，非Play/无测试/无编译。首轮后dirty来源未知，不声称恢复到此前clean，不保存/清除该状态。

最终执行Tools/Validate-ChangeLedger.ps1与git diff --check，原件validator-final.txt/diff-check-final.txt；详细用户使用说明与限制见同ID REPORT.md。未运行Play或实际当前Scene字体替换/文字美术验收，未改字体图集/fallback/图片/Input/角色列表/战斗逻辑，未删除/移动/提交/push。当前用户请求的工具开发已完成。
