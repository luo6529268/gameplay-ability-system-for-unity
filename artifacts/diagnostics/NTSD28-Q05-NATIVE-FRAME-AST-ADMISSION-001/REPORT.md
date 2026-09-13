# Q05 Native frame AST准入限定出口

状态 FOCUSED_TEST_PASS / VERIFIED_FRAME_AST_ONLY / TYPED_SOURCE_IDENTITY_PLAY_PENDING。父Task NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001继续，Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 行为与范围

ParseLoganContent现在用原ParserV2 partial扩展，在同一物理行路由中处理strength/frame区域。私有LoganFrameReader复用原frame/subblock/property模型：严格frame编号0..999/重复拒绝、完整caption、行首子块、pending/隐式结束/inline frame_end、跨major context保留current/pending、ppoint及native大小写/ASCII字段。原文保留，消费过的区域不泄漏到generic metadata；legacy Parse不变。已有字段扫描器仅按指定itr/armor context读第二整数前缀，strength默认仍scalar。

实际修改为Record列出的八个脚本及既有capture.csproj链接；没有新增runtime owner、queue/worker或更改Unity/GAS、关闭顺序、33ms、Scene、资源资产。typed frame converter仍旧入口：本包修正确读取的AST，不能冒充27/24/40/19/geometry内容数值已完整接线或全局语法/诊断行列信息对齐。

## 权威与对照

Current formal Logan EXE身份仍B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；source-linked C++ witness链接data/dat_parser.cpp和dat_document.cpp，原版声明的playable closure及source/header hash沿用Q03并fresh核对。此输出是源码诊断，不是伪称EXE运行trace。

37语法夹具、405 runtime DAT各双跑byte-stable。正式内容共55348声明frame，逐文件完整有序frame index/caption、raw key/value及subblock顺序规范UTF8/LF摘要与Unity实际Parser输出一致。402文件按frame语法接受；另外INKHUD.dat、INKHUD2.dat及data/resource.dat三非gameplay配置原版拒绝frame header，Unity也拒绝。不能把全部405当330 indexed gameplay定义。

真实旧AST差异位于ssnk、rai、raiT、data/broken_weapon四文件，原始首差见formal-ast-first-differences-before.json。ssnk的数字字段名7不符合native lexer，现不产生该字段，真实BuildCharacterDataFromSource成功；原six-DAT/nine-frame失败基线因此推进到five-DAT/eight-frame。其余角色的运行时完整行为不由这项加载成功证明。

## 验证与纠正记录

- 原始RED job874c8d6cdb254cceaa1a8538ab8fa5af实际443：73PASS/370FAIL。随后发现C++ stdout CRLF与Unity LF摘要不一致；336项是诊断编码问题，不是行为差异。原native TSV/RED.xml/实际payload和旧index全部保留。以同一原版记录规范LF后，旧payload等价409PASS/34实质FAIL；见red-canonical-correction.json，未修改任何字段/行为oracle。
- 第一轮绿色job6b6870a16b644f4d8edb3a4ff49d3114：517中516PASS/1FAIL。全部443 AST与45 strength通过；唯一失败是旧catalog要求六失败，但实际五失败，符合ssnk已修复。
- 修改旧catalog回归前补齐第八脚本Record。测试现核对五个明确文件并保持AggregateException、不允许partial publication；另增加ssnk真实构建断言。
- 第二轮job1edd269296064cffb4b1997bc8791084：518中517PASS/1FAIL。444 AST/ssnk、45 strength及其他catalog/caller通过；仅新测试以/比对原版异常中的\导致显示路径断言失败。五个实际文件正确，原输出与GREEN-attempt2.xml保留。
- 只规范化测试显示路径分隔符，生产代码不动。单独复验job06798eb738ef4095bcb76aa1ba4436a7：1/1 PASS。当前production对应518个不同测试均有通过证据（517+修正后1）；没有宣称曾有单次518全绿运行，也没有无故重复其他通过项。
- `dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-restore --verbosity quiet`：0warning/0error。新增partial编译引用及source哈希清单同步；已检索Tools csproj引用，只有该项目直接链接ParserV2。
- `dotnet run --project Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-build -- --input-root Assets/NTSD/Config --input-mode unity --output Temp/NTSD28ContentAudit/unity/q05-frame-ast-legacy138.jsonl`：138/138 parse/Converter成功，相对上包仅source metadata变化，旧投影差异0。
- 完整SelfCheck：新请求2026-09-13T05:29:25.004442+00:00，结果2026-09-13T05:30:15.232845+00:00为PASS，结果mtime晚于请求；旧结果另标NOT-CURRENT。
- Unity最终CS查询与Scene状态另存；Scene保持本包开始前的a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f，仅旧UI精度差异ORIGIN_PENDING，不恢复/不冒充与Git HEAD一致。保护3059：3026相同/33差异/0缺失，相对上包无新增受保护基线差异。所有source/header/corpus哈希复核0漂移。
- Change Ledger通过482 records/88 governed scripts。未运行新增定向Play；本包是解析/加载入口前置，整场与新typed值消费/identity/版本/Play仍未完成。

## 下一唯一入口与回访

恢复父Task NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001，下一准确Record针对typed frame decoder/manager接线与完整投影，不重做AST或已写字段模型。剩余ank319 WPoint effect；hir40/52 WPoint dircontrol、hir414 CPoint drain；min/min414和min/sag414 drain；nar30/52 WPoint dircontrol。五个文件八个帧仍待，旧六/九只作为历史。

还必须处理无异常却错误的值：native frame wait缺省0、literal hit_Fa等大小写、strict integer、CPoint独立hurt/伤害与float、OPoint24、ITR40/geometry presence以及完整WPoint9。不能仅删除未知字段检查就宣布全部内容正确。之后统一semantic identity、carrier/独立+2F8、双OPoint capture guard、entity13/aggregate21/checksum24/两shell2及trace/Play；Q06算法/Q07正式资源仍按原硬依赖后置。stage.dat及音频/视觉例外保持。
