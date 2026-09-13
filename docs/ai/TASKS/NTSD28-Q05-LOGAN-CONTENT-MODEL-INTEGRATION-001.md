# Q05-A2 Logan内容模型与转换入口接线

状态 IN_PROGRESS / CONTENT_AND_STRENGTH_TABLE_FOCUSED_PASS / NATIVE_FRAME_SOURCE_NEXT。属于NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001同一协调窗口；Q05-A1 helper已限定验证，不再重做数值算法或Q03源审计。

先读取父Task、Q03-EXIT-REPORT与三个冻结合同、Q05-A1 REPORT，以及父Q05工件prechange-reference-inventory.json的115个候选路径。候选清单不是批量写入许可；修改前按真实调用者建立准确code-path、测试范围和Change Record。

当前已观察入口：Lf2DatParserV2.ParseLoganContent是显式source入口，Lf2DatFile尚无source/profile标记；Lf2DatConverter.ConvertToFrameData仍是旧共享转换；CharacterAnimtorManager.cs当前1035附近直接调用它。BattleCatchPointValue仍19int，BattleCatchPointCatalog.ScalarsPerEntry=19，Adapter及canonical测试需协调。禁止只全局替换ParseInt或只改CPoint throw类型而遗漏caller/copy/identity。继续检查完整config candidate/manager两条数据入口，来源隔离不得扩散到非战斗工具。

实施目标依父Q05步骤1：明确Logan转换语义，消费已验证numeric helper，CPoint27含3float32与独立hurt字段，OPoint24/task复制，BDY geometry presence/zwidth、weapon_strength index+19及原版WPoint未知字段/数字key准入。可以按行为依赖拆准确子Record，但不得另开不兼容发布窗口；CPoint canonical严格遵循冻结27单元顺序、float raw bits及bitwise相等/hash。raw AST与raw fingerprint保持原含义，解码版本/semantic identity必须在Q05出口前协调，不能用rawhash冒充新语义身份。

先编译并用Q03固定见证、六正式DAT九frame、真实native投影对照和旧入口回归验证。新模型进入实际生产读取链前，核对throw float接收类型、两条OPoint task/factory/initializer链、public API及external package边界；外部Server/package/Gen/Plugins保持只读，不能根据类名推断实际所有权。Q06的新规则producer不提前混进本包。

Q05后继仍需：退休carrier清理及+2F8独立int32、双OPoint snapshot队列边界、身份绑定、entity12→13/aggregate20→21/checksum23→24/character1→2/base1→2与trace协调，旧midbattle拒绝、新capture/restore/replay、pool复用、完整SelfCheck与真实Play。禁止半迁移宣布Q05完成或部署Q07正式资源。

范围、风险、回滚仍依父Task；不改Unity/GAS与非战斗功能，不动Scene/Input Actions/33ms/十一阶段，不部署默认stage.dat。A1出口只提供数值helper；尚未创建A2脚本Change，先收敛准确范围再写测试/脚本。


当前CPoint27 Change NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001保持FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING，110项/SelfCheck通过；完整报告同ID工件。下一OPoint24 Task已准备；不能重复新开CPoint模型包或把其旧19投影/未接来源忘记。


当前增量：OPoint24 NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001已FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING，98项/完整SelfCheck通过。下一Geometry内容Task准确Record；CPoint与OPoint都保持未关闭回访，不重复模型实现，不把旧投影或旧来源当作新身份。


当前Geometry NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001已FOCUSED_TEST_PASS，native88/完整375/SelfCheck通过，来源及算法未接。独立4旧夹具修正已VERIFIED_TEST_ONLY。下一ITR40/strength19 Task，CPoint/OPoint/Geometry三项保持来源/identity/Play回访，不重复模型实现或重跑已关闭审计。


当前：单记录ITR40/strength19 NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001已FOCUSED_TEST_PASS，489/SelfCheck通过；表头1..9/重复/caption合同更正及真实FieldBag词法区别已记录。下一Native strength table admission Task，非重新做字段模型。Scene文件额外变化ORIGIN_PENDING已询问用户，不能覆盖或写基线未变；parser独立工作可继续，Play出口须确认。

当前整表 NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001 已限定验证，218不同focused/SelfCheck通过，真实manager消费完整19字段。Lf2DatFile已有LoganOriginalText/LoganWeaponStrengthRows，旧上文“尚无模型/下一ITR”仅历史；当前下一唯一Task NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001。CPoint/OPoint/Geometry/ITR以及整表均保留完整source/identity/schema/Play回访，正式资源未迁移。Scene disabled已恢复，仍留精度差异来源pending。

当前AST前置 NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001 已限定交付；ssnk可构建，原六/九变为五/八。source父Task进入TYPED_CONVERTER_NEXT，完整字段/默认值/大小写/geometry/WPoint9/投影与identity仍继续，不把AST合格当内容数值合格。518不同focused+SelfCheck见REPORT。

当前帧内容接线 NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001 已限定交付（55348/330/557 focused/SelfCheck）。完整Q05步骤1仍须 NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 测量definition header域，避免只以frame投影/可构建认证全部内容；然后继续既定步骤2～4。新FrameSounds/profile、centerz/chp/cmp和六double必须入semantic identity；Q06分reader motion/resource、Q09 centerz、Q10音频仍有明确回访，不得丢失。

Definition头部审计 NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001 已限定交付；具体数值/缺载体与排除项见REPORT。下一 NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001，完成Q05步骤1的metadata门槛再身份/载体/版本；不重写18armor及已匹配sequence/weapon sound，不恢复平台取景/HUD/选择流程排除。

当前出口更正：NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001与前BMP/stats、typed frame、strength及模型子包完成本父Q05步骤1的来源门槛，SOURCE_MODEL_FOCUSED_PASS（非runtime全对齐）。下一唯一NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，依父步骤2→3→4→5，不直接跳identity或部署。CPoint/OPoint/Geometry/ITR旧SOURCE_INTEGRATION_PENDING已有typed frame/manager证据满足来源子条件；identity/schema/consumer/Play仍待。禁止computer-use；任务外Foot18删除与新目录、Scene旧精度差异保护。
