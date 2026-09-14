# Native帧访问器与资源资格限定出口

VERIFIED / NATIVE_ACCESSOR_AND_RESOURCE_OWNER_ADMISSION_ONLY，2026-09-14。父ZERO-FRAME-CACHE-CONTRACT与所有其他reader迁移未完成，不能报告完整高位动作tick对齐。

准确四脚本（两生产+新Editor测试/探针+native witness），另独立INVALID-FRAME-FIXTURE-CORRECTION只改原HP/MP两个测试的invalid动作7→9999。没有新增World字段或schema；13/21/24/2/2保持。

## 实现合同

LF2FrameCache新增GetNativeFrameDataById/HasNativeFrame与NativeMaxFrameIdExclusive1000，单个声明索引数组由857扩到1000。有效Wrapper内先返回声明帧，缺失0..998返回id各自正确、wait0/next0/零数据的模板，未声明999及越界返回null。声明999保留。没有把模板插入Wrapper.frames或内容fingerprint。

原Max857/GetFrameDataById/HasFrame/GetFirstFrameByState范围和行为保持；旧缺失帧仍是原EmptyFrame，不把未迁移/非战斗接口暗中改成另一语义。明确这是迁移接入，不允许旧battle reader永久冒充对齐完成。

模板通过显式static ctor在第一个cache实例前创建999份纯定义数据，无World/entity/UnityObject引用，读路径无分配；运行时按不可变definition合同使用，未将公共可变LF2FrameData改成新架构。缓存单实例索引增加143个引用槽，进程级模板一次初始化；不新增World关闭阶段，不在Stopping自动创建服务。

BattleRecoveryStatusWriter只改共同资格和chp/cmp的帧读取到Native；HP/MP公式、World phase、模式默认及两个caller顺序不变。其它frame/input/motion/hit/snapshot读取尚未迁移。

## 来源和原函数见证

正式EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；最终75-source/header manifest07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。dat_document.cpp:82-116、dat_parser.cpp:381、object_catalog.cpp:217-221及BattleWorld资源成员函数直接调用。

native-zero.tsv共81行：63合法文档点查/资源输入，18错误AST诊断。未声明857/998可用，id正确/wait0，显式mode0下HP100→101/MP100→104；未声明999不可用，声明999可读chp7/cmp5。非法声明1000的raw AST accessor仍可能返回指针，但document.ok=false且正式catalog拒绝，不能据此扩大正式可加载边界。native-validation.json包含边界实例和重复stdout字节一致验证。

首次runner以非法initial_action直接spawn，被合法出生门拒绝；改为先action0合法出生，再设置观察用当前action，以独立观察accessor/resource而非出生准入。原失败native-initial-spawn-rejection.tsv保留。Native witness是SOURCE_MODEL_DIAGNOSTIC_ONLY，不是正式EXE键盘交互。

## Unity与运行时验证

- 初始RED11FAIL；其中6caller fixture把Runtime.Frame设置在Register前，被注册同步回0而读到声明chp7，原结果保留。将设置移到Register之后，单独6项RED全部HP100/期望101失败（6d594643f30e427395f2da596f208d9e），证明原资格拒绝隐式帧。
- 实施后新11/11 PASS（352fb848ccfe47dd9a1f7d0366b9fbe7）；63合法原函数值对照、全部999零模板数据、旧接口、空context/Clear/reload/声明999、6caller及零热分配通过。
- 原HP/MP invalid测试使用未声明7。Native资格正确放行后原5项3PASS/2FAIL（0abcd1a024864fce9fb43c3634f578d3，HP恢复+负environment后93而旧预期100）。独立Record只改invalid7→9999，保持所有拒绝结果断言。
- 联合回归223/223 PASS（6fb2da5fcef048cb93197fc93c780ad0，141.926秒），包含新11、原HP/MP/出生3716和两profile329、显示、owner及快照回放。focused-223-pass.xml。请求中额外旧boundary测试命名空间写错未选中，因此另以正确名称运行59958312d0d249b2ad50cddfa0674d53，1/1 PASS（legacy-boundary-one-pass.xml）；不把它算进223，也不把旧测试名FormalAuthorityMaximum当原版边界证据。
- 完整SelfCheck新鲜PASS（SelfCheck-pass.result），CS错误0。
- 真实NTSD_Battle旧内容Scene暂停资源owner验证：两caller分别在857/998得到HP101、MP100，在未声明999保持HP100、MP100；Native零帧id/wait正确且旧getter仍null。全部World/checksum恢复4→4（play-resource-frames-pass.json）。这只调用资源owner，没有把高位动作送入完整input/movement/frame tick，后者仍未迁移。
- 随后既有Q05真实恢复和有序关闭，World/slot/logic borrowers/render borrowers全0、两帧Stopped，Editor正常退出Play（play-cleanup-pass.json）。
- Scene用户HUDBg x30/SHA bcd1047b…不变，dirtyfalse/root14。保护3059中2921未变，较出生包新增变化路径仅声明FrameCache，0新增缺失。final-code-scope.json冻结本四脚本及独立两测试修正。

实际入口：Build-AuthoritySourceCapture.ps1 -RunnerSource Tools/NTSD28AuthorityTrace/native_zero_frame_witness.cpp -OutputDirectory Temp/NTSD28Q06ZeroFrame；原exe重复运行；Goal13_bridge.py refresh/get_editor_state/read_console/run_tests/get_test_job；SelfCheck请求；manage_editor play+NativeFramePlay和Q05 ReplayPlay请求。禁止computer-use，未启动第二Editor或改正式源码/资源。

## 状态与下一步

HP/MP此前重开的资源owner内部帧资格现由本包补证，可恢复各自限定资源事务VERIFIED；这不证明高位动作在完整生产tick中能正确到达该owner。其它旧reader仍可能拒绝或错误绑定，所以父零帧/完整战斗任务保持未完。

下一NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001，从实际frame绑定/direct/next/快照FrameDataId及source调用链划分准确修改包；后续输入、碰撞/命中/CPoint/held、生成/表现/声音各自迁移并回访。原清单388条为改前位置；reader-inventory-after-accessor.json提供406条当前检索引用（含声明/测试/Editor，非全为live caller），必须按真实入口核对，不能为了清grep迁移死代码。

完成reader迁移后回DISPLAY-PROGRESSION其余出生初始化/联验，再POST-DISPLAY；Q07正式资源仍待。R07仅owner资格子条件闭合，R02/R05/R09/R15随真实reader改动继续，R16已验本包关闭。Unity/GAS/非战斗、33ms/3ms、十一阶段和用户例外保持，总目标ACTIVE。

最终账本验证PASS：517 records/25 governed code diff；父artifact/ledger-final.txt。历史Record不在当前diff的WARNING保留。下一实际reader迁移仍可推进，总目标保持ACTIVE。
