<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001
status: FOCUSED_TEST_PASS
change-kind: NATIVE_ARMOR_PIECE_AST_AND_DEFINITION_SOURCE
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2LoganDefinitionBlocks.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatFile.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganDefinitions.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganStrength.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Animation/LoganDefinitionMetadata.cs
code-path: Assets/NTSD/Scripts/Animation/LoganWeaponPieceDefinition.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05ArmorPieceSourceEditorTests.cs
code-path: Tools/NTSD28ContentAudit/UnityContentCapture.cs
authority: Formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; dat_parser.cpp, combat_records.cpp, battle_world.cpp confirmed in playable build.ps1 coreSources.
evidence: RED_443_FAIL_3_PASS / FOCUSED_1829_UNIQUE_PASS / SELFCHECK_PASS / VERIFIED_ARMOR_PIECE_SOURCE_ONLY
-->

# Native护甲与武器碎片来源接线

父metadata AST Task，准确12脚本（其中4新增）及capture.csproj source links。先native原版fixtures/corpus witness与RED，再实施，现阶段不改战斗消费者。

原状：native parser仍把armor/piece交给旧generic残余，piece_end/group/variant归属丢失；旧armor大小写不敏感、type只看存在而不看有效性、列表Split数字前缀不等于原版。18正式normalized已匹配应保持，不能重写无关护甲runtime。

新增LoganDefinitionBlockReader并接同一物理行router：armor当前指针与context分离（新armor覆盖指针，close允许其他context，EOF当前未关即error）；weapon_piece唯一block、close需要piece context、最多5组/每组5variant、同piece归组保声明顺序，piece字段严格整数，amount必须group归属，piece_end只在piece context识别/清group与variant，global stats保持context。其他major只退出active读取，不擅自清待关闭pointer。所有边界以原版diagnostic parseSuccess而非惯例裁决。

AST保留armor/root/group/variant原始ordered字段与opening/closing行号。NativeMetadata扩展不可变Armors及WeaponPiece，复用LoganDefinitionFieldSet last/optional值，不建第二套数值容器；manager从正确AST深复制不可变分组/变体。现有LF2ArmorData保持形状，native decoder用strict int/type invalid回退ptype、formatted流前1/2int、ordered列表和optional sound；legacy ApplyNativeArmorDefinitionData默认false不改变。新增列表语义为load期helper，沿用NumericDecoder严格整数overflow检查，不改数值公共算法。

Q06待消费武器碎片block team/group amount/variant oid(默认-1映射999)/action/frame/dvx/dvy/dvz等字段；当前记录只负责精确数据来源，随机序列、内建碎片先行/slot与destroy时序不能提前宣称对齐。所有raw/typed/presence/group/variant排序需要进入Q05 identity，当前schema不变。max_mp/运动/BMP表现等前包回访仍保持。

验证：全405原版AST和18armor normalized/三indexed piece、语法边界fixtures双跑、330manager/非法catalog整批拒绝、immutable isolation、旧138与BMP/stats/frame/strength/typed回归、真实Unity编译、完整SelfCheck、ledger及scene/hash保护。无新runtime manager/queue/worker，不需新增shutdown阶段；Unity/GAS、非战斗、33ms/3ms、十一阶段、Scene旧精度差异、stage.dat USER_HOLD和所有批准例外保持。

回滚须明确批准，仅本Record差量，保留既有工作，不自动重置资源/场景。没有资源部署或批量删除。

用户明确禁止computer-use，后续不再使用。此前仅尝试只读窗口发现/截图（截图接口不支持，未取得截图），无点击/按键/输入操作；Unity已自行完成重载，由桥接确认idle并启动RED。后续全部桥接/日志/结果检测。

## 已写/当前验证

RED原版446项：443FAIL/3PASS，XML 08:19:50Z至08:19:55Z为本类；查询job时Unity已正常shutdown导致bridge状态删除，不把缺失查询当新job失败。当前12脚本已写：AST与native有序context reader，armor strict/list decoder、不可变piece/group/variant metadata、实际source manager接线；dotnet source-linked build 0warning/0error。NTSD Editor进程49020已退出，仅另一项目34460在运行。下一无界面Unity测试，禁止computer-use，完整SelfCheck/identity/schema/runtime未验。

Unity执行状态：准备headless期间用户环境重新出现NTSD Editor PID58092（此前49020已退出），unity test因同项目实例锁拒绝，CLI退出1/COMMAND_FAILED，无测试结论；未删除锁/未重试并发启动。改用当前实例桥接，状态ready6401、idle/无编译中。Unity启动清空Temp，旧bridge helper已按现有MCP源码协议重建；正式证据均artifacts保留。新447测试含immutable AST→metadata嵌套复制隔离；联合回归已请求。

## 限定出口

FOCUSED_TEST_PASS / VERIFIED_ARMOR_PIECE_SOURCE_ONLY。39 native fixture/405语料/18armor/三piece+330 actual metadata完整对照；RED446→首轮1829中的构造兼容1与Temp探针缺失11已修复，最终467全过，1829不同测试有证据。完整SelfCheck 08:32:42Z > 08:31:58Z PASS，CS0/ledger通过。实际命令、头文件/数据hash、不可变深复制、测试更正与legacy完整hash证据见artifact REPORT.md。

任务外Foot资源18删除/新blue-red-yellow目录已观察，保留现状并在后续保护检查排除归属推断；未执行这些变更。Scene旧SHA保持。No computer-use长期约束已同步入口。当前不改变schema/producer/正式资源。下一NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，不重做本包与前BMP/stats来源。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，488 Records / 102 governed code files，ledger-final.txt已保存。
