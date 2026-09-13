<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001
status: FOCUSED_TEST_PASS
change-kind: NATIVE_FRAME_AST_ADMISSION
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganStrength.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganFrames.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatTokenizer.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05FrameAstEditorTests.cs
code-path: Tools/NTSD28Q05FrameAst/AuthorityFrameAstWitness.cpp
code-path: Tools/NTSD28ContentAudit/UnityContentCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11LoganCatalogEditorTests.cs
authority: Current formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable data/dat_parser.cpp scan_fields and frame context; parent NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001.
evidence: NATIVE_FRAME_AST_PARITY_55348 / RED_CANONICAL_CORRECTED_34 / FOCUSED_517_PLUS_TARGETED_1 / FULL_SELFCHECK_PASS / TYPED_SOURCE_IDENTITY_PLAY_PENDING
-->

# Native frame AST事前合同

现有ParseLoganContent仅strength按物理行处理，其他帧仍旧generic tokenizer。与native的行首frame/subblock、标题、严格编号0..999/重复拒绝、pending子块/隐式结束、inline frame_end和paired itr字段不一致。原版源码src/data/dat_parser.cpp、dat_document.cpp，实际build参与性沿用Q03已冻结closure/hash并fresh核对。C++ combat_records在src/simulation，不在src/data。

准确七脚本及capture.csproj编译链接。复用原ParserV2 partial/AST和上一包native区域扫描：strength与frame共用一遍物理行路由，保留其他metadata文本视图；新增私有LoganFrameReader维护current/pending与frames，不另建完整DAT parser。跨major context只改变active context，不擅自清空native仍存current frame/pending；EOF/下一frame/global frame_end按native处理。native帧替换dat.Frames，旧Parse不变。原文仍完整保留。

扩展已有ScanLoganScalarFields为有context可选参数，只有itr四action与armor frame读取第二整数前缀；strength调用仍scalar，不改变上一包规则。精确大小写ASCII字段、#扫描、空字段、marker与caption，保留既有properties/subblock顺序。全DAT其他metadata领域错误不在本包冒充已验证，实际frame错误立即拒绝候选。

先建立source-linked C++ oracle对照frame index/caption、全部raw属性和subblock顺序，RED再生产。语法夹具包括非法/重复/边界、EOF、嵌套/未结束子块、同行标记、大小写、数字键、成对值和跨strength/bmp/stats切换；正式405 DAT完整AST差分。旧138/source caller/strength回归、Unity编译/SelfCheck/ChangeLedger。捕获保留失败first-difference及输入hash；new helper只为加载期，无新owner/queue/worker，无关闭阶段变更。

只认证AST前置；不接新typed frame converter或更改碰撞/命中、OPoint等算法，不升schema/部署资源。完整27/24/40/19/geometry、WPoint9、manager来源接线和identity/Play仍父Task，六DAT九frame转换失败不会在本包假称修完。Unity/GAS/非战斗/Scene/InputActions/33ms/十一阶段/stage暂缓保持。Scene精度差异来源pending继续保护。回滚须经授权，仅本包差量，保留所有用户工作与前包。

## Oracle已捕获，生产尚未写

37语法夹具（7拒绝）与405正式DAT（55348声明frame；3非gameplay数据文件因frame语法被native拒绝），均双跑byte-stable。完整有序AST输出75,890,318 bytes，逐文件规范SHA索引供测试对照；first-difference保留原始TSV与实际差异。拒绝的正式文件为INKHUD/INKHUD2/resource.dat，均原版frame header错误，不能据此声称405都可作为gameplay定义导入。

## RED与生产落盘

RED job874c8d6cdb254cceaa1a8538ab8fa5af：443实际测试73PASS/370FAIL，RED.xml及原始差异TSV保留。随后在已声明四生产parser路径接入单遍region/私有FrameReader、strict frame准入、pending/inlineclose、context paired扫描；strength保留，原文与metadata视图不变。source-linked工具新增partial引用/hash登记。当前CODE_WRITTEN待编译/完整AST复验，不报typed帧接线完成。

## RED证据更正：诊断换行编码

首次443的370FAIL包含Windows C++ stdout CRLF与Unity显式LF摘要不一致，不能把370都当内容差异。原native TSV/RED.xml/RED实际payload保持；旧index另存raw-newlines-index-NOT-CANONICAL。现从原native行统一UTF8/LF后重建SHA索引，不改变字段/顺序/编号/数值期望。用旧实际payload离线复核，336项（326正式+10夹具）仅换行差异；其余34为实质RED（4正式AST+3正式错误准入，19夹具AST+7夹具拒绝+1数量），证据red-canonical-correction.json。故真实旧结果等价409PASS/34FAIL，而原始实际Unity运行仍如实443/370FAIL。生产代码无需因此调整。下一绿色用canonical LF复验。

## 接线回归差异：旧六失败断言退休（修改前扩展准确范围）

首次绿色517中516PASS/1FAIL，AST443与strength45全部通过。失败仅旧FormalCandidate_RejectsKnownSixFiles：实际5。完整AST已证明ssnk的数字键7被native词法忽略，Q03所列该文件唯一converter失败原因已消除。新增准确第八脚本 Assets/NTSD/Scripts/Test/Editor/NTSD28B11LoganCatalogEditorTests.cs，改为显式剩余五文件路径+仍抛AggregateException，不弱化为任意失败计数；本包FrameAst测试再加真实ssnk manager构建成功断言。生产不用回改，也不凭旧断言恢复错误数字键。原GREEN-attempt1.xml保留；其余五文件具体frame转换仍父Task。

## 第二次绿色及测试路径显示修正

job1edd269296064cffb4b1997bc8791084：518项517PASS/1FAIL，444 AST/ssnk、45 strength、其他catalog/caller均PASS。剩余1是新路径断言使用/而catalog异常保留源文件的\，输出真实五个文件与预期完全对应（Object2 nar、65 ank、27 min、449 sag、63 hir），不是新增production缺陷。测试在比较显示路径时规范化分隔符；不改SourcePath/规则/资产。GREEN-attempt2.xml保留，下一只复验该失败方法，其他517已经有当前production通过证据。

## 限定出口

FOCUSED_TEST_PASS/VERIFIED_FRAME_AST_ONLY，source-linked405文件55348 frame raw结构+37语法、ssnk actual manager加载通过。当前518不同focused有通过证据（517+单独修正后1），两次测试夹具纠正/原始失败保留，详见同ID REPORT。完整SelfCheck 2026-09-13T05:30:15.232845+00:00 PASS，dotnet0error、旧138无新增投影差异，输入hash0漂移/保护基线比上包无新增差异，Ledger482/88PASS。正式资源/外层identity/schema/Play未闭合，Record保持活跃回访。下一父Task NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001 的typed转换准确Record，不重新做AST。

## Source/typed接线回访（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001）

当前frame来源已接入实际Logan manager，55348完整typed投影/330构建/557不同focused及SelfCheck通过，同ID REPORT为证据。本Record早先source-pending的帧层子条件现PARTIAL_RETURN；definition头部、semantic identity/联合schema、对应runtime consumer/Play仍待，状态保持FOCUSED_TEST_PASS，不把此回访扩大成整域完成。
