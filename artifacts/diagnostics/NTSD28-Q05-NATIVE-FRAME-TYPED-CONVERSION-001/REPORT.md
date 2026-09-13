# Q05 新版帧数据转换与实际构建限定出口

状态 FOCUSED_TEST_PASS / VERIFIED_TYPED_FRAME_CONTENT_AND_CONSTRUCTION_ONLY / DEFINITION_HEADER_IDENTITY_SCHEMA_CONSUMERS_PLAY_PENDING。总目标及Q05仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE，正式DAT/图片未部署。

## 实现与原状修正

显式ConvertLoganFrameData与旧ConvertToFrameData共享转换骨架，CharacterAnimtorManager仅按source.IsLoganRuntime选择新入口。native literal key/strict int/wait缺省0、CPoint27（含三个float32及独立hurt/伤害）、OPoint24、ITR40与presence/raw zwidth、BDY6、WPoint9/BPoint2未知字段忽略、first/ordered catalogs及seal已进入实际构建。原始未知字段仍留在AST，严格DTO只保留其契约字段；旧入口的宽松整数、大小写文化规则/旧CPoint等行为保持。

完整frame vocabulary审计发现原37int外另有centerz131、dz107、cmp7、chp2声明，且正式live path还读取dx/dy及double dvx/dvy/dvz。现新增centerz/chp/cmp三int，dx/dy/dz与nativeDvx/nativeDvy/nativeDvz六double，UsesLoganFrameNumbers profile。37原int投影保留，因为原版不同caller分别使用integer和double，不能全局互换。新增FrameSounds只读有序声明，原sound标量保留兼容用途。原版音频实际处理前20声明并跳空值，consumer在Q10；本包没有修改audio emitter。

精确数值helper复用BigInteger词法/单次舍入实现32和64位。float32保留原CPoint的NUL/trailing-space语义，binary64按battle_world.cpp私有field_number_or_zero的finite、全长strtod语义读取，不依赖旧Mono double.Parse精度；负零/subnormal/十进制/hex/溢出与无效返回值有bit证据。

## 原版对照与真实构建

source-linked witness使用正式dat_parser/dat_document/combat_records/object_spawning/collision_geometry源；binary64私有helper精确文本复制自当前battle_world.cpp，SHA2ca23838eab32e41666d509fbe307b617c14c304b7a90ebfc4c6c21268268e94。它是source-helper诊断模型，未伪称直接调用private符号或正式EXE运行trace。identities-v2冻结源/header/405 DAT/26fixture；复核0漂移，正式EXE authority仍B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。

原版输入双跑byte-stable，canonical统一UTF8/LF/tab，文本hex、float/double按int32/int64 raw bits比较，避免先前CRLF假差异。范围：405 DAT中的55348声明帧、40帧int+六double+名称/有序sound/首BDY flags、86383 BDY、19461 ITR、4019 CPoint、7290 OPoint、38163 WPoint、318 BPoint、40 strength与8864 sound声明。三个非gameplay配置仍按native无效frame语法拒绝；402个有效document逐文件完整已声明typed投影一致。ppoint仅raw AST保留，未发现playable typed consumer，不发明行为。

真实manager/candidate验证：全部330 indexed定义构建成功，逐entry id完整、每个声明frame的native profile为真。以前六文件九frame（上包剩五/八）的DAT构建异常均消除；ank/ssnk/hir/min/min/sag/nar专项真实构建通过。这不证明各角色全部战斗时序/HP资源事务/视听已经对齐。另有一合法+一重复frame非法定义的候选测试，明确抛AggregateException而不返回部分配置，fail-closed保持。

## 测试与诚实状态

- 初始37投影RED jobb26e49685d9146b6bc6341cd7f9e3ed5：438项5PASS/433FAIL，缺新转换入口及旧五DAT拒绝；RED-initial37.xml保留。随后按实测补全40int/六double/ordered sound，而非只修异常文件。
- binary64 RED job701d366e81a3438e8fd367138b0c16d9：1FAIL，缺helper入口；RED-binary64.xml保留。
- 首次完整绿色job22bf222f9a7d4e04b7a39e6a3426af87：556项555PASS/1FAIL。新439 typed/64/旧六文件、原43numeric（含14742展开比较）、45 strength及其他catalog/caller通过。唯一失败是旧测试还期待五文件构建失败，而当前330已经成功。
- 按事前声明更新旧断言为完整330成功，并新增非法candidate整体拒绝；生产代码不因旧测试回改。定向jobab0df501ba8740a2a1923a9fabcc655c：2/2 PASS。当前557个不同测试均有通过证据（555+修正/新增后2），不宣称单次557全绿。
- binary64覆盖2567个原生helper样本，包括133种正式frame motion原始值、随机decimal/hex、舍入中点、边界、负零、NUL与空白语义；每一项Unity raw bits与native一致。此前binary32原43测试/14742展开比较重新通过。
- dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-restore --verbosity quiet：0warning/0error；新增production helper编译引用及source hash清单同步。
- dotnet run --project Tools/NTSD28ContentAudit/UnityContentCapture.csproj -- --input-root Assets/NTSD/Config --input-mode unity --output Temp/NTSD28ContentAudit/unity/q05-typed-frame-legacy138.jsonl：138/138 parse/Converter通过，相对前包旧投影无新增差异（仅source audit metadata变化）。
- 完整SelfCheck新请求2026-09-13T06:09:14.225337+00:00，结果2026-09-13T06:09:59.574406+00:00为PASS，mtime晚于请求；旧结果标NOT-CURRENT。
- 最终Unity error CS查询0，Scene isDirty=false/root14；Scene SHA仍a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f。既有UI精度差异ORIGIN_PENDING保护，不认证与Git HEAD相同。
- 保护3059文件：3026相同/33既有差异/0缺失，相对上包无新增受保护基线差异。九个脚本实际变更及新增测试工具均有Record；Ledger483/90通过。
- 未运行新增定向Play/整场trace。本包验证内容转换和候选构建，未认证新字段runtime consumer、snapshot或内容身份闭合。

## 下一完整性门槛与必须回访

下一唯一Task NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001：只读核对BMP移动值/序列、stats、armor、weapon metadata/piece等尚未被本包帧投影覆盖的真实native reader与Unity构建值。已观察共享legacy float.TryParse/ParseInt仍在这些入口，不据此直接断言正式数据必然有差异；必须测量再决定精确修改，不能以330可构建代替完整definition正确。

本轮新增FrameSounds、UsesLoganFrameNumbers、centerz/chp/cmp、nativeDvx/Dvy/Dvz与dx/dy/dz必须进入Q05 semantic identity/hash/适用数据契约清单。之后回父Q05 retired carrier、独立+2F8、双OPoint capture guard及entity13/aggregate21/checksum24/两shell2/trace的同一次窗口，不能额外发布中间ABI。

Q06按native各reader分别接int/double motion和chp/cmp，同时继续CPoint资源/OPoint materializer等既有后继；Q09接centerz；Q10消费ordered sound前20/跳空值。当前正式语料没有多sound同帧，组合fixture已覆盖，不能因此取消规则。其余source/header、运行时producer、identity/版本/Play不随本包通过自动关闭。既有stage.dat暂缓、音频/视觉例外、Unity/GAS/非战斗/Scene/InputActions/33ms/十一阶段保持。没有提交或推送。
