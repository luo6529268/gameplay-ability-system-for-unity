<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001
status: FOCUSED_TEST_PASS
change-kind: NATIVE_BMP_STATS_AST_AND_METADATA_SOURCE
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganStrength.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganHeaders.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatFile.cs
code-path: Assets/NTSD/Scripts/Animation/LF2CharacterData.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05BmpStatsSourceEditorTests.cs
code-path: Tools/NTSD28ContentAudit/UnityContentCapture.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
authority: Current formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable dat_parser.cpp BMP/global stats, native input/definition callers and verified fieldset.
evidence: RED_431_FAIL_3_PASS / FOCUSED_1390_UNIQUE_PASS / SELFCHECK_PASS / VERIFIED_BMP_STATS_SOURCE_ONLY
-->

# Native BMP/stats AST与实际metadata来源合同

父Task NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001。本包闭合BMP/stats域：精确literal/last/order/raw、BMP bare字段/inline global stats、sequence计数/整数行/隐式context/EOF规则、同一行sprite字段和multiple BMP合并。复用已验证frame/strength路由；新增私有LoganHeaderReader并替换native dat.Bmp及merged stats，不改旧Parse。其他metadata（armor/weapon_piece等）仍原generic残余视图，明确后继，不称全DAT语法对齐。

准确八脚本，另capture.csproj增加Header partial及LoganDefinitionMetadata引用。Lf2DatFile增加native merged stats属性；LF2CharacterData增加NativeMetadata property（legacy为null），manager仅source.IsLoganRuntime时从准确BMP/stats构建已验证不可变fieldset。caller仍Q06/Q09，legacy float compatibility字段不全局重设，不能以metadata承载已正确就声称实际运动已改。native exact值和optional fallback通过NativeMetadata读取，后续identity包含它。

Sprite原声明range/line字段准入按原版project_sprite_sheet，width/height/row/col取同一行最后有效整数（invalid最后声明应为缺失→0），effective累积范围仍现有Q02工厂负责。原Sprite组件/导入器不改；只是native AST同一数据入口补齐，Q02全部回归。BMP sequence必须复现std::istringstream整数行（允许+、连续整数前缀，失败是否eof影响返回），不能用Split猜语法；使用原版fixture覆盖EOF/overflow/空行/多header/计数/close/上下文。

全局stats在任何context捕获inline并保持原context；只到同行stats_end，尾部不泄漏BMP/root。多个stats合并有序字段。BMP close若sequence尚未关报error，EOF missing bmp_end/sequence报error；其他major只退出active BMP，保留未闭合sequence至验证。BMP名称/头像/小图由literal last构建，其他源码DataModel不被当逻辑权威。

先当前工具原版raw capture与RED，再生产；覆盖正式405解析、330实际metadata、16double值/optional stats/完整raw列表与sequence/sheet AST，native fixtures捕获精确成功/失败，旧138和frame/strength/typed/numeric回归、compile/SelfCheck/Ledger。测试第一层证明入口，不发布资源/新schema。

无新runtime manager/queue/worker/生命周期，只有load期数据；不改变Unity/GAS/非战斗/Scene/InputActions/33ms/十一阶段/stage暂停/平台取景/HUD/selection例外。weapon_piece结构及armor严格解码后续仍本父Task，Q06消费后验。回滚经批准仅本包差量，保留现有工作。

## 实施前补全source边界（第九脚本）

增加准确Converter路径以给ApplyNativeInputDefinitionData显式source参数：native literal key/strict int与merged stats，legacy缺省false保持。manager native兼容float槽由精确BMP值转float（精确值仍NativeMetadata，Q06改reader），速度缺省0/rate缺省1；native weapon参数只读BMP，禁止root/armor/stats越界污染。原8脚本因此增为9，不改runtime消费者或全局旧API。测试需同时校验metadata精确值、source profile、已映射int及兼容槽缺省，避免只加无人使用的新字段。

## 实施进展

原版28夹具/405语料capture已落盘并双跑一致；RED XML已保存。九脚本已写：native逐行BMP/stats reader、merged LoganStats、实际NativeMetadata binding、native兼容速度0/rate1与严格input/weapon来源；旧入口保持。dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --nologo -v quiet：0警告0错误。Unity compile/focused/SelfCheck尚待，不标运行时完成。

## 回归更正与最终focused

RED 434=431FAIL/3PASS。首轮1362=1360PASS/2FAIL，兩例是capture缺省尺寸sentinel -1被当真实值；保留原完整与薄投影，原版optional helper补5项valid=false将薄投影表示为null，不把真实负一改为零。第二轮462=459PASS/3FAIL：两例仍由测试静态cache旧数据引起，新增OneTimeSetUp清空capture cache；另旧Q02无条件405均可解析预期错误，独立test-only NTSD28-Q05-SPRITE-CORPUS-ADMISSION-FIXTURE-CORRECTION-001按native parseSuccess断言402成功/3拒绝。最终job6486f670f52f43229f83ab465f425ca3 462/462 PASS，完整GREEN.xml与第一轮928个frame/strength/typed通过合计1390不同测试。完整SelfCheck已新请求，结果待；不得提前关闭runtime。

旧138重新Parse/Convert全部成功，排除源码身份字段后与前typedframe轮投影完全相同。63个fixture/原版optional源码头等身份0漂移；保护3059中3025same/34既有或准确声明差异/0missing，较前包新增LF2CharacterData（本Record准确路径）。Scene SHA仍a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f，来源pending旧精度差异保持。

## 限定出口

FOCUSED_TEST_PASS / VERIFIED_BMP_STATS_SOURCE_ONLY；完整SelfCheck新PASS（08:09:08Z > 08:08:22Z），CS0、Scene clean/root14/旧SHA保持。报告与失败更正/实际命令见artifacts/diagnostics/NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001/REPORT.md。Q05未完；下一NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001，其后identity/schema/Q06/Q09/Q10/Play。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，487 Records / 98 governed code files；完整ledger-final.txt保留。
