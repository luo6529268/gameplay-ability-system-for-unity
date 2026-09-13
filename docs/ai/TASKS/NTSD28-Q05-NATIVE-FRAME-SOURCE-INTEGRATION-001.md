# Q05-A2 Native frame来源与完整记录接线

状态 DELIVERED_TYPED_FRAME_CONTENT_ONLY / DEFINITION_HEADER_IDENTITY_CONSUMERS_PLAY_PENDING，属于现有Q05联合迁移窗口；不能创建第二套内容authority或提前发布中间ABI。

恢复先读CURRENT-AUTHORITY、Q03-EXIT-REPORT及JOINT-FIELD-MATRIX/decoder-contracts、各Q05字段Change和native strength table REPORT。数值helper、CPoint27、OPoint24、BDY/ITR几何、ITR40/strength19单记录及strength整表已经存在，禁止重做相同模型。整表native入口和manager武器表接线只关闭加载子条件，四个记录Change仍有frame来源/identity/Play回访。

当前入口：CharacterAnimtorManager.BuildCharacterDataFromSource按source选择ParseLoganContent，但BuildCharacterDataCore逐帧仍调用旧Lf2DatConverter.ConvertToFrameData。新的LoganCombatRecordDecoder未完整进入该frame路径。Lf2DatFile已带LoganOriginalText/LoganWeaponStrengthRows；native strength按物理行消费并避免污染metadata。legacy Parse及转换入口保持独立用途。先读真实callers再确定改动路径，不能只给旧Converter统一换ParseInt。

修改脚本前，收敛并登记准确Task/Change code-path、原状、依赖与RED。先对照playable dat_parser.cpp及combat_records.cpp的frame/subblock语法和字段选择顺序，验证现有ParseLoganContent产生的AST是否保留native实际FieldBag。特别核对大小写、数字key7、未知字段、单/双token、重复字段、多个同种subblock及缺失几何；不能将旧AST或人工拼字段当作native lexer输出。如果必须补lexer/AST，保持在显式native入口，不替换非战斗工具/旧入口的语义，也不以全套新parser绕过现有扩展。

接线目标：复用既有decoder消费CPoint27、OPoint24、ITR40、BDY ZWidth/HasGeometry；核对WPoint原版9字段、未知effect/数字key忽略及完整数据默认值。先确认完整字段/选择规则与Unity调用者，保持raw AST/原文与raw fingerprint的含义。native武器表19字段已经接通，作为回归条件，不重新实现。

验收：六个正式DAT九个已知失败frame必须通过真实BuildCharacterDataFromSource；同份正式内容的native/Unity完整27/24/40/19/geometry投影逐字段对照，不再用旧19/8投影证明新内容；保存first-difference和原始输入hash。核对原有138旧入口与相关source cache/candidate回归，Unity实际编译、focused和SelfCheck。新增工具同样先登记准确code-path；若工具链接production新增partial，编译引用和source hash清单同步。

此包不授权提前修Q06的candidate/held strength replacement/CPoint资源settlement/OPoint materializer/landing除3精确性或+2F8 AI reader；这些差异按原归属保留。semantic decode identity、外层entity13/aggregate21/checksum24/character2/base2、retired carrier清理、+2F8独立载体、OPoint双队列capture guard与trace迁移仍必须在同Q05窗口闭合后才允许Q07资源部署。

Scene归属项SCENE_BASELINE_CHANGED_ORIGIN_PENDING保持：上一观察的相机/Canvas disabled在2026-09-13 04:56:23 UTC保存后已恢复，当前仅UI RectTransform精度差异，来源未确认。不得回退用户工作或把isDirty=false写成Scene unchanged；只读parser独立工作继续，完整Play/资源迁移出口前解决基线。默认stage.dat、音频/图片例外、33ms、十一阶段、Unity/GAS及非战斗功能边界保持。回滚只允许经批准的本包差量，不覆盖已有修改。

当前先执行子Change NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001：读到原版行边界/子块语义后确认旧AST不能直接接decoder；先RED及源对照修AST，完整converter/identity/Play仍本父Task后继。

AST前置测得的完整正式首差为ssnk、rai、raiT和data/broken_weapon；native三非gameplay文件帧语法拒绝另计。第一轮AST443通过后，整目录构建失败由六减五，ssnk数字键已消除；仍应明确验证ank/319 WPoint effect、hir40/52 WPoint dircontrol及hir414 CPoint drain、min/min414与min/sag414 drain、nar30/52 WPoint dircontrol。原“六文件九frame”作为历史不删除，当前剩余五文件八frame。下一typed转换要保持native frame wait缺省0（frame_machine.cpp），不能沿用LF2FrameData.wait=1；frame key大小写/整数及raw property读取必须按native FieldBag而非全局ToLower。

待AST出口后恢复时：先读AST REPORT中换行编码和旧catalog数量/路径断言两次修正，不把原370FAIL当370个行为差异；native TSV正确，首次SHA索引少了CRLF→LF规范化。模型/grammar source源码无需为测试显示路径改动。父Task继续typed converter，先列准确code-path/Record，不能只停在AST。

Typed接线定位补充：native_ai.cpp明确读取大小写敏感hit_Fa；battle_world.cpp读取hit_g/hit_j、dvx/dvy/dvz，collision_geometry.cpp读取centerx/centery，frame_machine.cpp的wait缺省0。现旧Converter.ToLower全局归一和CPoint伤害alias仍不符合native；接线应显式分source，旧入口保持。请先从这些live caller核实完整frame标量字典，复用单记录decoder，避免只修五个异常而漏掉无异常的错误值。

AST限定出口 NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001 已交付，518不同focused有通过证据（517+1）与完整SelfCheck PASS。下一唯一动作：读取本父Task和AST REPORT，列typed converter/manager/WPoint9/完整投影的准确code-path与Change Record，继续修剩余五文件八帧及全字段语义；不要再新开AST审计。

新确认sound权威不是last-win：battle_world.cpp:224遍历按序声明的前20项、跳过空value。当前typed子Change增加只读FrameSounds内容载体并投影全部声明，旧sound仅兼容标量；Q05 identity需包含新列表，Q10负责audio consumer/前20规则，不改当前所有非战斗音频功能。

完整vocabulary核对后typed子包扩为40int（新增centerz/chp/cmp）+六double（dx/dy/dz及nativeDvx/Dvy/Dvz，原版field_number_or_zero严格finite strtod）+ordered sound。37int初始witness不够作为出口，必须扩展64bit oracle与helper回归。Q05 identity还需新增字段/profile，Q06 motion/resource、Q09 centerz、Q10音频回访不能忘记。

当前typed子Change NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 已限定交付：405/55348 frame数据、557不同focused、330实际构建、完整SelfCheck通过。此前六/九→五/八现全部构建通过；不等于runtime已对齐。下一唯一 NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 核对剩余definition域，再推进同Q05身份/载体/版本/consumer回访。
