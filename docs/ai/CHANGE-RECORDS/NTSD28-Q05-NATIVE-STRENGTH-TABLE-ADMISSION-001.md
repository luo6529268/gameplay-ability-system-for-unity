<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001
status: FOCUSED_TEST_PASS
change-kind: NATIVE_STRENGTH_TABLE_PARSER_AND_BUILD_INTEGRATION
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2DatFile.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Models/Lf2WeaponStrengthRow.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatTokenizer.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganStrength.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05StrengthTableEditorTests.cs
code-path: Tools/NTSD28Q05StrengthTable/AuthorityStrengthTableWitness.cpp
code-path: Tools/NTSD28ContentAudit/UnityContentCapture.cs
authority: Current formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable dat_parser.cpp strength/scan_fields and object_catalog.cpp document.ok gate; Q05 active goal.
evidence: RED_34 / GREEN_189 / RELATED_74 / FULL_SELFCHECK_PASS / VERIFIED_TABLE_LOAD_ONLY / FULL_SOURCE_IDENTITY_PLAY_PENDING / SCENE_ORIGIN_PENDING
-->

# Native strength table事前实施合同

状态IN_PROGRESS / TEST_FIRST，准确九脚本见metadata，另更新既有UnityContentCapture.csproj编译引用。现有场景差异ORIGIN_PENDING继续保护，不改Scene。前轮单记录19字段/ITR40不重做。

采用现有ParserV2 partial扩展，不替换通用parser或给全部token/prop新增位置字段。新增Lf2WeaponStrengthRow AST（Index/Caption/OpeningLine及既有Lf2DatProperty列表），Lf2DatFile用非序列化property保存native rows；ParseLoganContent先按物理
行检查strength，失败不返回dat，成功后通用AST照旧保留并附typed rows。legacy Parse与JSON字段保持原路径。

在既有Lf2DatTokenizer新增明确命名的native scalar字段扫描方法，按C++ ASCII identifier/一个value token/<停止/end-marker/layer过滤规则生成既有property模型，不包装伪frame、不更改legacy tokenizer的#语义。Native lexer并不把#整行当注释：active row下其中可识别字段仍扫描，row前非空#行会报error。表头entry1..9、跨section重复拒绝、caption保留但不当字段；marker为大小写敏感starts_with，空白按C ASCII处理，EOF可不闭合strength（native不为此报错）。其他major marker离开strength，stats行不改变当前context。这里只验证strength域，不冒充完整DAT全语法对齐。

CharacterAnimtorManager明确按source.IsLoganRuntime消费typed rows并调用已验证单记录decoder；旧提取入口保留。native构建时strength generic block不得再被weapon_hp/移动参数提取扫描，防止caption或strength私有字段越界污染metadata；其他namespace的提取不改。不能修改武器HP规则、lookup或19项replacement算法。真实manager build与候选fail-closed需测试。

验证先source-linked native document.ok/row index/caption/line/raw fields及RED，再覆盖普通/多表/重复/非法/EOF/caption/行首/大小写/字段在entry前/#/scalar scanner、原版全量有strength的indexed定义、legacy入口及metadata边界。Compiler、focused/相关回归、SelfCheck、代码Ledger与Scene hash对比；不能强行恢复来源未知Scene来制造绿灯。工具source-linked项目引用/源hash清单同步，避免新增partial使工具无法构建。

无新runtime owner/queue/worker；loading-only AST与转换不影响关闭顺序、33ms或框架。非战斗/Gen/Plugins/外部包/Input Actions不改，不部署资源或升schema。本包可完成strength接线子条件，完整frame/native source、identity与Q05联合版本/Play仍后继。回滚经批准仅本包差量，保留所有前批与用户工作。


## 实施前细化：native专用区域的解析视图

为防止entry caption及opening/closing marker同一行文字泄漏到generic metadata，native扩展会消费strength区域并提供保持换行的其余文本视图给原ParseCore；原文由Lf2DatFile.LoganOriginalText property保留，完整行/字段/caption在typed rows中。它修正native模式此前错误的扁平表AST，不更改文件或legacy Parse，不生成假frame。该策略优于在多个metadata getter各自过滤，因closing行尾文本否则会漏到top properties。上文“通用AST照旧保留”在native strength区域由本条更正：保留真实原文/native AST，而非保留错误扁平化结果。Manager因此无需新增weapon_hp/移动参数过滤，只切strength列表来源，其他getter保持。其他major marker由视图保留并退出strength；整DAT其他领域的语法一致性仍完整source后继。

## 生产落盘 / 待编译

2026-09-13 RED实际34/34失败（job7f9a871c356340b4917459025decacfd，RED.xml），确认缺失typed rows与非法/重复准入。随后九路径合同内落盘native区域解析视图/原文与typed row、scalar scanner、source条件下manager完整19字段接线；旧路径保持。source-linked工具新增三个生产引用与hash清单；未升schema/部署资源。下一编译与focused回归。

## 限定出口与实际验证

准确九脚本按本合同实现：typed row/property、native partial+tokenizer、manager完整19字段接线及测试/source-linked工具。生产职责和副作用见上述合同，无新增owner。报告 `artifacts/diagnostics/NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001/REPORT.md`；RED34→189PASS，补正确namespace后74PASS（45重复，共218不同测试），完整SelfCheck PASS、Unity CS0、dotnet0error、旧138零新增投影差异、Ledger481/85PASS。每个命令/输入/失败/未验项已保存在报告及XML。

状态FOCUSED_TEST_PASS/VERIFIED_TABLE_LOAD_ONLY，完整source/identity/schema/Play仍待，保持活跃回访。Scene disabled现在恢复，余精度差异ORIGIN_PENDING，不回退、不宣称Scene unchanged。下一Task NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001，不重复本包，不升schema或部署资源。

## Source/typed接线回访（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001）

当前frame来源已接入实际Logan manager，55348完整typed投影/330构建/557不同focused及SelfCheck通过，同ID REPORT为证据。本Record早先source-pending的帧层子条件现PARTIAL_RETURN；definition头部、semantic identity/联合schema、对应runtime consumer/Play仍待，状态保持FOCUSED_TEST_PASS，不把此回访扩大成整域完成。
