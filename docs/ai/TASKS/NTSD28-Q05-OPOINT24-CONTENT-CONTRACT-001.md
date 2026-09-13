# Q05-A2 OPoint24内容与任务复制契约

状态FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING。父Q05-A2/同一联合迁移窗口；CPoint27 Change现FOCUSED_TEST_PASS、110测试/完整SelfCheck通过，来源接线/身份/Play尚未完成并保持活跃。

必须复用Q03冻结OPoint24/held-depth及VERSION-IDENTITY合同，不重做审计。先读取当前BattleObjectPointValue、Adapter、LF2FrameData.ObjectPoint、OPointCreateTask/多发task、BattleLogicObjectPointRuntime.CopyMultipleTaskToSingle和LF2ObjectPointFactory两处singleTask.opoint赋值，再逐文件列准确Record。

已观察：immutable value只有8项；ObjectPoint已有dvz但adapter双向忽略或清零，该旧排除合同必须改为完整24项。新增16内容项为z,dvz,hp,mp,team,reserve,effect,pic,centerx,centery,centerz,framea,attacking,join,join_reserve,join_pic。objectId是不同的legacy runtime标识，不能与oid混同晋升。Native ObjectSpawnPlanner28.decode读24个strict int32，exact-case last-win/default0；source_line非内容。加载期复用LoganNumericDecoder。

目标：24字段immutable equality/hash、ObjectPoint完整值、adapter双向复制、单/多task复制与Clear/default不留旧值，新增LoganCombatRecordDecoder的OPoint block入口。现有两个复制点是struct整值赋值，若已覆盖不为留痕修改生产代码，优先实际测试证明。不得把task.pos.z/task.dvz、point.z/point.dvz的规则优先级提前写入本包；factory/team/vitals/random/reserve完整消费归Q06。

既有FormalKernelObjectPointValueSeamEditorTests及SelfCheck中8属性数量、dvz被忽略断言需按新合同更新，保留原多OPoint/重复/缺definition行为测试。需验证24个单独字段、负值/溢出/重复key、value→task→多转单→Clear→复用；native可复用Q03形状与source-linked内容capture。若修改tool projection，准确scope先入Record；CPoint19项旧projection也在A2最终回访清单内，不能用旧工具证明全27/24项。

本包不升entity/aggregate/checksum/shell版本、不切manager/发布半迁移candidate、不部署正式资源、不改非战斗/Unity/GAS/Scene/33ms/十一阶段。后续还有BDY/ITR几何契约、weapon-strength19+index、来源转换全链与身份、carrier和版本协调及真实Play。Q05和总目标保持ACTIVE/FULL_ALIGNMENT_INCOMPLETE。


准确七脚本Record已建，生成native夹具和RED后实现。原CPoint Change继续活跃，不覆盖其metadata/证据。


当前98项/native45x24/完整SelfCheck通过；报告同ID。保持活跃待来源、身份、materializer及Play回访。下一Geometry内容Task，不能因为本子项通过而关闭Q05。
