# Q05-A2 完整ITR与武器强度内容契约

状态READY_FOR_EXACT_PRECHANGE_RECORD。父Q05-A2同一窗口。CPoint27、OPoint24、Geometry三Change均有focused/SelfCheck证据，但仍SOURCE_INTEGRATION_PENDING；不要重做它们或误关Q05。几何关联旧HitPlan夹具修正是独立VERIFIED_TEST_ONLY。

依据当前playable combat_records.cpp InteractionRecord28及weapon_strength_interaction，复用Q03 OPOINT-AND-HELD-DEPTH/VERSION-IDENTITY合同。ITR native40项为kind,x,y,w,h,dvx,dvy,fall,arest,vrest,respond,effect,drain,spark,recover,dbdefend,bdefend,injury,zwidth,z,dvz,sound,cover,caughtact,catchingact,pickedact,pickingact,delay,poison,confus,weak,manacle,join,mimic,bound,facing,dx,dy,dz,gain；source_line仅诊断。caughtact/catchingact/pickedact/pickingact取有效最后字段的首整数，其余strict int。confuse拼写不能别名化为confus。

当前已观察Unity InteractionArea新增z/hasGeometry及copy/projection/fingerprint，但缺独立drain/sound/cover。不要复用gain/frame.sound/WPoint.cover表示它们。caughtact/catchingact是legacy int[]，完整native入口应依据实际当前reader确定首整数表示，不能恢复背面第二值选择或盲套旧ParseIntPair。Geometry的ApplyInteractionGeometry只负责几何，需在完整Interaction decoder中正确组合；旧额外字段vaction/throw等不能凭历史假定native有同名消费。

weapon_strength当前index+8字段；native19字段为dvx,dvy,fall,arest,vrest,respond,effect,drain,spark,recover,dbdefend,bdefend,injury,zwidth,z,dvz,sound,cover,caughtact。caughtact同首整数，其余strict。先读CharacterAnimtorManager.ExtractWeaponParameters实际AST入口和全部数据复制/缓存/测试，再登记准确Record。行顺序/重复/index含0原样保存；旧GetStrengthEntry拒绝<=0的行为选择修复归Q06，不能仅因读表方便把它当native真相。

本阶段完成字段、值复制、native decode和必要fingerprint/投影载体，必要时分有独立出口的精确子Record；禁止整目录修改。ITR clone/CopyFrom/HitPlan projection及replacement必须核对实际用途，不对native未定义字段机械复制。strength实际19项替换及held candidate深度选择在Q06同语义时点接线，不能提前重排。

验证以source-linked native输出逐字段对照，涵盖strict/首整数、重复/case/未知、完整40/19形状、copy/reset/缓存、旧入口回归与SelfCheck。更新内容审计工具时准确声明路径，补齐目前仍19/8/旧几何形状的Unity投影，不能以旧projection认证新字段。完整source manager转换、raw/semantic identity和版本仍在本A2/Q05后继一起闭合。

无新增框架/非战斗/Scene/Input Actions/Gen/Plugins/外部包修改；33ms/十一阶段保持，不部署资源、不另开schema窗口。Q05所有后继carrier、+2F8、OPoint capture guard、entity13/aggregate21/checksum24/两shell2、trace与Play要求保持。旧phase断言留Q12、landing除法一ULP留Q06、stage.dat暂缓保持。


## 2026-09-13 更正：表头准入与运行时查表分开

已核实正式source/ntsd28_core/src/data/dat_parser.cpp第438～480行：weapon_strength entry必须严格为1..9，重复编号为parse error并清current row；entry行其余文本是caption，不是该行字段。object_catalog.cpp第217～224行通过document.ok()拒绝含parse error的整个definition。故本任务此前“index含0/重复原样保存”不得用于native DAT准入。runtime weapon_strength_entry无<=0检查仅说明lookup函数，不构成文本入口可接受0或重复的证据。旧原始AST可留存诊断，正式模型不纳入这些非法行。完整source parser与table接线须按此合同后继验证；不能简单使用当前扁平properties丢失行边界的旧AST来宣称caption/重复准入一致。


当前：单记录ITR40/strength19 NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001已FOCUSED_TEST_PASS，489/SelfCheck通过；表头1..9/重复/caption合同更正及真实FieldBag词法区别已记录。下一Native strength table admission Task，非重新做字段模型。Scene文件额外变化ORIGIN_PENDING已询问用户，不能覆盖或写基线未变；parser独立工作可继续，Play出口须确认。
