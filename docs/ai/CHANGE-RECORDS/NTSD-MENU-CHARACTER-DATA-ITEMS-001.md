<!-- CHANGE-RECORD
id: NTSD-MENU-CHARACTER-DATA-ITEMS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/CharacterSelectionController.cs
code-path: Assets/NTSD/Scripts/UI/CharacterSelectionBoard.cs
code-path: Assets/NTSD/Scripts/UI/SelectRoleItem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterSelectionBoardEditorTests.cs
authority: User 2026-10-06 requests data.txt-driven character Item generation with default Random; reference Logan list generation only and retain project selection flow
evidence: docs/ai/TASKS/NTSD-MENU-CHARACTER-DATA-ITEMS-001.md
-->

# NTSD-MENU-CHARACTER-DATA-ITEMS-001

原状：Controller拥有槽位初始化、倒计时/弹窗/MatchConfig；Board已有CharacterChoiceItem模板与MMMiniObjectPooler、点击交给activePlayer。SelectRoleItem在初始化时枚举所有加载字典键、以HeadSprite存在筛选；未按data.txt类型/hidden条件显式过滤，晚预热不主动更新。当前LoganRuntime已发布目录在GameConfig中，catalog/data.txt 158个type0，默认hidden0共50个。原Menu/Board绑定存在；当前原Editor6402非Play/clean Menu。目标三脚本均无pre-existing diff，治理文档和Shadow测试dirty均保留。

预计路径/符号：SelectRoleItem.RefreshAvailableCharacters与OnJoin改为注册表列表；Controller订阅现有PrewarmCompleted并刷新槽位；Board.RefreshCells随机/普通小图绑定。新增聚焦测试同Task。引用NativeMetadata.Bmp.hidden，不把美术有无当名单过滤；默认隐藏条件仅0可见。本任务不修改random字段解析、随机最终角色算法或解锁门。

副作用：可选名单从“所有有头像的对象”变为“data.txt注册顺序的默认可见type0”；随机首项。晚加载更新现有选中ID索引，若定义消失退回随机，不改槽位状态。模板与对象池生命周期保持现有所有权。没有新增Battle manager，不进入有序关闭合同。

验收/回滚：见Task；生成工程0error、定向测试和真实Menu生成/重开证据分别报告。操作前逐文件备份与SHA已经核对，不恢复无关工作；治理文档按追加更正。Scene、Prefab、DAT、GameConfig与输入资源均是只读保护项。

2026-10-06 实际代码：三个UI脚本按上述符号实现；新增 CharacterSelectionBoardEditorTests 三项覆盖默认hidden过滤/注册顺序/缺图不删角色、当前发布data.txt与catalog一致、原Menu生成51项/随机默认/点击确认取消/重开复用。当前仅CODE_WRITTEN，编译及原Editor测试待执行；没有保存Scene或资源。

2026-10-06 编译进展：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly` exit0，334 warning / 0 error，26.12秒；新测试已被生成工程包含。原Editor刷新并完成重载、Console无C#错误；定向EditMode job `eb17907e7f3241e89dd11af5f9cdf612`运行中，第一规则测试已通过，原Menu Play测试正在预热。证据 compile-01.txt / tests-start-01.json / tests-poll-01.json；当前COMPILE_PASS，运行结果待回收。

2026-10-06 验证更正：测试等待条件后来新增MenuLoopCarousel但漏using NTSD.UI.Menu，原Editor曾报CS0246，原件 console-after-01.json保留；补引用后 compile-02.txt 0error。首轮job最终规则/data.txt两项通过、原Menu等待120秒失败（tests-poll-03.json）；保存Menu的LoadingPanel未激活，测试未走预热入口，并不证明生成失败。测试新增ShowLoading沿既有启动入口、等其返回模式面板，超时上限600秒；没有修改加载脚本/场景。compile-03.txt exit0 / 301warning / 0error / 8.93秒，Unity刷新完成，第二job `42ef8aa039424e928697d240580aa320`已启动。辅助execute_code诊断因本机CodeDom命令行过长、Roslyn不可用失败，未据其报告状态，也未安装编译器/修改MCP包；使用原桥与TestRunner继续验证。

2026-10-06 最终限定验收：VERIFIED（菜单Item生成范围）。原桥两轮Enter/ExitPlay后丢失部分TestRunner回调，job残留running不能当成功计数；添加测试末尾WritePassed独立输出后，最终job `08c88d5f80e549ae976e3a0fb309cf6e` 三项均执行到全部断言之后，直接记录 passed-rules-20261006-083542081.json、passed-original-menu-reopen-20261006-083842727.json、passed-data-index-20261006-083849834.json。最终编译 compile-04.txt exit0 / 301warning / 0error / 6.22秒。

实际Play断言：现有加载面板完成资源预热、原Menu生成51项（随机-1＋50个默认可见角色）且顺序与当前data.txt一致；随机图/每个角色SmallSprite或HeadSprite绑定；点击首个真实角色、刷新保留ID、再次点击进队伍选择、取消回角色选择；关闭列表回收，重开51实例ID及顺序全部相同，默认随机。两Scene SHA相同且Editor Scene clean，GameConfig外部保护SHA相同。实际Game View截图 character-items-20261006-083841064.png（已点击鸣人后的列表），当前布局/字号未改。

桥回传残留的第二/三job仅在直接结果存在、Editor确认为非Play/无测试/idle后清掉元数据；没有取消运行中的测试。editor-final.json / scene-final.json：原Menu、非Play、无测试、无编译/重载、root8、isDirty=false；console-final.json无C#错误（仅桥客户端日志）。审计校验validator-01.txt PASSED，最终文档同步后另跑validator-final.txt；后镜像与保护项见postchange-manifest.json。

职责结论：Controller仍拥有项目选择/弹窗/倒计时/MatchConfig，只增预热事件订阅；SelectRoleItem只改名单与刷新；Board仅补随机图和普通小图兜底，复用原模板/对象池/点击路径。未测试完整多人键盘加入、倒计时开战/设备，也未移植原生解锁或random字段过滤；本记录不宣称这些流程或Native整套选择UI对齐。无不可回退操作、无Git提交/push，无Scene/Prefab/DAT/InputActions/配置资源改动。
