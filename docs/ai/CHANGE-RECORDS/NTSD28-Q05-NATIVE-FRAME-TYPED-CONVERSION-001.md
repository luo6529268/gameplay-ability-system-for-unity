<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001
status: FOCUSED_TEST_PASS
change-kind: NATIVE_FRAME_TYPED_CONVERSION_AND_MANAGER_SOURCE
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TypedFrameEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11LoganCatalogEditorTests.cs
code-path: Tools/NTSD28Q05TypedFrame/AuthorityTypedFrameWitness.cpp
code-path: Tools/NTSD28ContentAudit/UnityContentCapture.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganNumericDecoder.cs
authority: Formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable FieldBag/combat_records/object_spawning/collision_geometry/frame_machine/native_ai/render_snapshot; parent NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001.
evidence: COMPLETE_TYPED_FRAME_55348 / BINARY64_2567 / BINARY32_14742 / FOCUSED_555_PLUS_2 / CANDIDATE_330 / FULL_SELFCHECK_PASS / HEADER_IDENTITY_SCHEMA_CONSUMERS_PLAY_PENDING
-->

# Native typed frame转换事前合同

准确七脚本及既有capture.csproj编译链接。原ParseLoganContent AST前置已验证，复用，不修改parser。当前manager逐帧仍统一ConvertToFrameData旧分支：wait缺省1、全ToLower、旧宽松ParseInt、CPoint alias/19字段、OPoint8、BDY无zwidth/有效性、ITR旧值与WPoint未知字段严格拒绝。旧legacy入口selfcheck和工具使用不能一并改语义。

在Lf2DatConverter新增显式ConvertLoganFrameData，两个入口复用同一core骨架；37个已建模frame int按native literal key/strict helper、wait缺省0、sound原文last-win。旧入口保留ToLower兼容（六个hit_Fj/Fa/Da/Ua/Dj/Uj明确旧大小写映射）。native子块复用已验证CPoint27、OPoint24、ITR40、BodyBox6；只新增native WPoint9/BPoint2与raw strict scalar辅助，未知字段保留AST但不进严格DTO rawProperties。保留first/ordered catalogs与seal；primary body kind/respond从native首BDY strict读取。

CharacterAnimtorManager按source.IsLoganRuntime选择显式转换；default/legacy仍旧入口。ConvertAllFrames默认行为不变；没有隐式推断来源或全局替换ParseInt。capture工具只添加新生产依赖链接/hash，不暗改其历史projection/input profile。

权威调用者：FieldBag整数literal last-win；combat_records.cpp的CPoint27/WPoint9/ITR40/strength19，object_spawning.cpp的decode24，collision_geometry.cpp整数presence/raw zwidth，render_snapshot.cpp:1781 BPoint x/y；frame_machine.cpp wait缺省0，native_ai.cpp hit_Fa大小写。ppoint目前仅parser出现，保持raw AST，不发明typed行为。帧补缺0..998及Q06consumer/physics/碰撞/投掷/resource事务不在本包。

验证先source-linked C++完整typed witness：37帧int/名称sound/首BDY flags、CPoint27 float bits、OPoint24、ITR40+geometry、BDY6、WPoint9、BPoint2与strength19；canonical UTF8/LF，不能重复前包CRLF hash错误。正式405 DAT frame转换对照，330 indexed实际manager candidate构建；合法数据全部转换后原五失败测试应改为完整330成功并保留独立错误candidate拒绝测试，不为旧5断言恢复错误行为。新增语法数值/大小写/缺省/未知字段/独立hurt/geometry/multiple first契约夹具，RED再生产，旧138/profile回归、相关focused/SelfCheck/Unity编译/Ledger。float使用raw int32 bits避免shortest decimal假差异，记录所有失败/纠正/未验项。

新内容模型进入load构建不代表版本/identity/schema已完成，禁止发布半迁移baseline或Q07正式资源。既有candidate事务/owner/关闭职责不新增或重排；不改GAS/Unity框架/非战斗/Scene/InputActions/Gen/Plugins/外部包/33ms/十一阶段/stage暂缓。Scene精度差异来源pending保护。验证Play与完整identity仍父Q05后继。回滚经批准仅本包差量，保留用户及前包工作。

## 实施前权威纠正：有序sound内容载体

实际battle_world.cpp:224调用values.all(sound)，按声明顺序取前20条，空条目占声明索引但不发事件。上文sound last-win仅能描述旧兼容标量，不是native正式音频行为，禁止把它作为完整sound权威。增加准确第八脚本LF2FrameData.cs：私有只读有序FrameSounds内容集合及内部seal，native转换保留全部原始sound声明顺序/空值；既有sound标量兼容仍last-win。旧路径保持原行为/空新集合，不修改任何audio emitter或预加载consumer。C++/Unity witness比较完整ordered sound列表，现存audio单字符串reader和first20/empty事件规则留Q10；Q05 semantic identity必须覆盖新集合，不能忘记字段ABI。该数据载体loading-only无新runtime owner。

## 实施前扩展：完整帧字段与双精度合同

全55348 frame raw vocabulary复核发现除了37int+sound，还有centerz(131)、dz(107)、cmp(7)、chp(2)，不是可忽略未知字段。render_snapshot.cpp:623/708读centerz；battle_world.cpp:2201/2291读chp/cmp；:137 field_number_or_zero以strict finite std::strtod读取，:1722/1735/1744 linked frame dvx/dvz/dvy及:1767/1778/1787 dx/dy/dz消费double。即使当前dx/dy语料未声明也有正式live consumer，必须一起保存六个double，而不能把它们压入整数槽或float32。现未改生产，已有37投影仅初始诊断，完整出口必须扩展为40int+六double+有序sound/record。

新增准确第九脚本LoganNumericDecoder.cs，在现有BigInteger single-round实现基础上支持binary64，保留float32原词法（含其NUL/trailing space规则），binary64采用该private helper的全长strict strtod规则（允许前置C whitespace，拒绝尾随空白/嵌入NUL/nonfinite，保留负零/underflow sign）。复用词法与有理数舍入，按32/64显式参数，不依赖旧Mono double.Parse精度。必须用原生strtod精确helper文本/源hash见证十进制/hex/边界/随机向量和Unity bit验证，再回归原43 numeric/14742扩展比较，不能以32bit PASS代替64bit。

LF2FrameData新增centerz/chp/cmp整数、dx/dy/dz与nativeDvx/nativeDvy/nativeDvz六double和明确native profile标记。37既有int dvx/dvy/dvz仍保留strict整数投影，原版不同caller确实有integer和double两种读取，不允许互相替代。native转换填入新值；旧入口新值默认/profile false，旧运行行为保持。完整typed witness逐值比较64bit raw bits与新40int；Q05 identity必须纳入新字段/profile/sound集合，Q06接double motion/chp/cmp，Q09接centerz，Q10接声音。未接consumer时不得报告新行为已对齐或提前部署。

## RED与扩展oracle进度（生产未写）

初始37字段witness RED jobb26e49685d9146b6bc6341cd7f9e3ed5：438实际5PASS/433FAIL，缺显式native converter及原五文件构建失败。原RED-initial37.xml保留。扩展完整40int/六double/有序sound原版输出已双跑：405/55348帧，25,569,624 raw bytes；26组合fixture。另2567个strict binary64输入含133个真实frame motion原始值，原版helper双跑稳定。field_number_or_zero源码文本SHA2ca23838eab32e41666d509fbe307b617c14c304b7a90ebfc4c6c21268268e94精确复制到witness，明确source-helper model而不是声称直接调用private源码符号或正式EXE trace。identities-v2.json冻结扩展输入，旧37输出保留initial37。下一新增64入口RED再生产。

## 生产落盘

binary64专项RED job701d366e81a3438e8fd367138b0c16d9实际1FAIL（缺helper入口）已保留RED-binary64.xml；随后九路径合同内实现共享32/64精确rounding、40int/六double/profile/FrameSounds载体、显式ConvertLoganFrameData复用core、WPoint9/BPoint2/首BDY读取和manager按source接线。旧入口保留原ToLower文化规则与宽松整数/CPoint历史语义，非战斗/默认资源不变。source-linked引用/hash同步，dotnet构建0warning/0error。当前CODE_WRITTEN，等待Unity完整typed/native64/原43numeric回归；旧catalog五失败断言将在实际转换已成功后按证据改为330成功及独立非法candidate拒绝。

## 首次绿色：typed全量已通过，旧失败断言待更新

job22bf222f9a7d4e04b7a39e6a3426af87实际556：555PASS/1FAIL。新439 typed/64/旧6文件、原43numeric（含14742展开比较）、45strength与其余caller/candidate通过。唯一旧FormalCandidate_RejectsRemainingFiveFiles期待AggregateException，但当前330构建已成功；该成功来自实际source分支，不再用旧失败数维护错误状态。按已声明TestCatalog范围将其改为完整330逐entry/profile成功，另加一坏一好定义的明确候选整体拒绝测试，保持fail-closed。原GREEN-attempt1.xml保留。

## 限定出口

FOCUSED_TEST_PASS / VERIFIED_TYPED_FRAME_CONTENT_AND_CONSTRUCTION_ONLY。具体改前/改后职责、九脚本范围及新增40int/六double/sound/profile合同见上文，实际命令/原始失败/检验见同ID REPORT。55348帧全typed投影及330 actual candidate，557不同focused有通过证据（555+2）；binary64 2567、原binary32/整数14742展开回归，完整SelfCheck 2026-09-13T06:09:59.574406+00:00 PASS、CS0/dotnet0error、旧138零新增投影差异、输入0漂移/保护无新增差异，Ledger483/90PASS。

当前Record保持活跃回访：definition header尚未完整测量、identity/schema/consumer/Play未闭合。下一个唯一入口 NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001；不重做AST/各字段模型或已消除的DAT构建异常。旧sound标量不是native完整音频，Q10名单/profile与所有新数字字段必须纳入Q05 identity，依赖已回链。
