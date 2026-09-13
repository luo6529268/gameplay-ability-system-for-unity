# Q05-A2 Native武器强度整表准入

状态 DELIVERED_TABLE_ADMISSION_ONLY / FULL_SOURCE_IDENTITY_PLAY_PENDING。ITR40/strength19单记录decoder已FOCUSED_TEST_PASS，489测试/SelfCheck通过；不重做数值helper和各字段模型。CPoint/OPoint/Geometry/ITR-record四Change仍待来源、身份与Play回访。

先读当前CURRENT-AUTHORITY及单记录REPORT，特别是SCENE_BASELINE_CHANGED_ORIGIN_PENDING：本地12:15:20场景多出HUDCamera/ScenesCamera/Canvas disabled及UI坐标精度差异，已向用户确认，未归因，不得覆盖或默默认证。此项不阻断只涉及parser的独立工作，Play/源迁移出口必须解决归属。

权威：dat_parser.cpp约438～480及object_catalog.cpp约217～224。只允许entry1..9，严格index token；重复编号（跨section也要核实）为错误，current row清空；整definition有error由catalog拒绝。entry行其余文本是caption，不能解析为字段。普通strength字段只消费一个词法token；不要将authored '-7 99'作为FieldBag值，因为native实际是'-7'。具体原始值已由witness --raw-strength直接捕获，不能手工拼出期望。

当前旧ParserV2将weapon_strength_list扁平化为Properties，Lf2DatProperty仅Key/Value，缺物理行信息，CharacterAnimtorManager仅Find第一个block并把entry当普通属性。不能以当前flat AST实现caption/重复/字段在entry之前等正式准入并声称一致。先检查现有tokenizer/parser能否携带必要位置/结构元数据，用现有解析能力扩展准确source入口；不另造整套parser，不把strength字段拼成假frame送production解析。旧通用入口的非战斗/历史用途保持。

修改任何脚本前列准确Task/Change及code-path，包含必要parser/tokenizer/model/manager和测试；不得目录glob授权。采用明确native分支，逐项核对opening/closing tag大小写、entry位于行首、caption、空行/comment、字段在entry之前、重复/非法编号、multiple sections和源诊断/失败策略。以native actual document.ok/entries/FieldBag为oracle；保留旧AST/原文诊断，不返回partial candidate供发布。

接线只能使用已验证LoganCombatRecordDecoder.WeaponStrength，并验证完整19值进入真实BuildCharacterDataFromSource配置；不变更weapon_hp/音频等无关参数提取，不把runtime lookup无<=0 guard推断成DAT可导入0，不提前修改held candidate/19项replacement算法。

完成后还有native frame全转换、WPoint未知字段准入、完整40/19/27/24/geometry投影与source/decode identity；entity13/aggregate21/checksum24/两shell2、carrier清理、+2F8、OPoint capture guard及真实Play仍Q05同窗口待办。不能部署Q07资源或发布半迁移ABI。非战斗/GAS/Unity框架/Gen/Plugins/外部包/Scene/Input Actions/33ms/十一阶段保持；旧phase/landing ULP/stage.dat暂缓保留。


准确实施合同已建：ParserV2 partial+既有tokenizer的native scalar扫描、专用AST rows及manager分支，见同ID Record。原场景变化继续保护，独立parser工作不受阻。

## 当前出口

见同ID REPORT与Change Record：34语法/10正式定义共45新测试，189及74两轮通过（218不同测试），SelfCheck/编译/旧138加载通过。整表解析与manager接线完成加载子条件，下一 NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001。场景开关已恢复，但精度差异来源未确认；不重复询问/回退，不标完整Scene基线通过。
