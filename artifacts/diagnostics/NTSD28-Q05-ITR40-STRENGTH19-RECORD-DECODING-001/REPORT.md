# Q05-A2 ITR40/strength19单记录解码进度

状态：FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / TABLE_SOURCE_INTEGRATION_PENDING / SCENE_BASELINE_CHANGED_ORIGIN_PENDING。保持活跃，不作为完整Q05出口。

## 已实现

InteractionArea补drain/sound/cover三独立int，并同步CopyFrom、ItrProjection及两路Fingerprint；不混同gain、frame.sound或WPoint.cover。旧kind5兼容replacement仍保留source的这三值，与actual ShallowCopy一致，新规则消费归Q06。

WeaponStrengthEntry补11字段，现为index+19项；没有修改manager、list注入或lookup/factory。新增LoganCombatRecordDecoder.Interaction完整40项与WeaponStrength单条19项。ITR复用已有几何有效性，四动作取首整数，caughtact/catchingact以单元素数组表示，缺省为[0]；当前kind1/kind3仅读[0]并按符号翻转方向，已用实际ResolveRelationAction验证。原版未消费的legacy vaction/throw/kill等在新入口保持default，原AST不变。

Strength单记录入口接收已解析字段列表并只接受1..9索引，不承诺整表准入。原始字段与词法结果必须区分，不能在普通int decoder中为了空格值放宽为首整数。

## 权威合同更正

原版dat_parser.cpp 438～480只接受entry 1..9，拒绝重复编号；entry行其余文字是caption。object_catalog.cpp 217～224以document.ok()拒绝有错误的整个definition。此前任务中“index含0/重复原样保存”仅能用于原始诊断，不可用于正式DAT准入；已在父Task及Q03 OPoint/held-depth合同追加更正。runtime lookup无<=0检查不证明文本可导入0。

source-linked原版实测：index0/10/重复document.ok=false；caption含dvx:99并未写dvx，下一行dvy:7生效；index9合法。当前Unity旧flat properties无行边界，尚不能认证这些行为。

## 验证

- Native56条记录夹具分别产生ITR40和strength19对照，另5个admission夹具；原版176输出行含document状态和诊断partial行，不能把每行都当正式有效配置。正式EXE B1E13A…9033已核对。
- RED job8c2b5f706c5d451ca111b3a7948e1483：120/120缺入口/字段失败，RED.xml保留。
- 初次联合job9ecb64460a7147e2a81bcf5878cdc128：489项488PASS/1FAIL。仅strength numeric_09的测试输入层级错误：authored '-7 99'实际被native scanner裁为FieldBag '-7'，测试却把完整authored文本直接送strict decoder。原始GREEN-attempt1.xml保留。
- 扩展同一C++ witness的--raw-strength模式直接导出FieldBag key/value（hex UTF8），测试改用该真实字段。没有改生产整数规则或数值期望；新witness的原数值输出与native.tsv逐字节一致，见identities-v2.json/native-strength-fields.tsv。原authored .fields.tsv作为历史输入保留，不再冒充native AST。
- 最终完整复验job60b3e9c71d854631abd72cbe08ed8265：489/489（新120+Geometry92+HitPlan185+CPoint43+OPoint49）。覆盖112个实际记录逐字段、index边界、copy/clone/projection/fingerprint与现有signed action reader。
- dotnet build UnityContentCapture.csproj：0warning/0error；138旧Unity DAT parse/Converter成功，相对Geometry轮旧投影无新增差异。旧投影不含完整新字段，不能认证新来源。
- 完整SelfCheck：04:25:26.185759 UTC新请求，结果mtime晚于请求且PASS；旧结果另存NOT-CURRENT。
- 最终CS0。编辑器NTSD_Battle isDirty=false/root14，但文件基线已发生变化，不能报告Scene unchanged，见下节。

## 场景异常与保护边界

保护3059文件3028不变/31差异/0缺失。比上轮多出的唯一基线文件是NTSD_Battle.unity：HUDCamera、ScenesCamera和Canvas的enabled 1→0，另一个RectTransform y从122.61到122.609985。文件最后保存时间为本地12:15:20，当前SHA见scene-baseline-observation.json；diff已保存。

本任务没有发出Scene编辑或保存命令，现有日志不足以确认这些变化来自用户、其他任务或编辑器/测试保存。已向用户异步确认来源，当前按用户工作保护，未回退。Scene.isDirty=false不等于磁盘基线未变。后续代码/表语法可以继续；在完整Play/来源迁移出口前必须解决场景基线归属，不把此项隐藏成已通过。

## 后继

本包只改既定五脚本，没有改manager/生产消费/框架或正式资源；Scene额外变化单独未归因，不纳入本包允许改动。新内容ABI与外层身份/版本仍未一起完成，禁止发布中间状态。

下一Task NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001：以现有parser/tokenizer为基础保留必要行边界，正确处理1..9/重复/多section/entry caption/字段值词法和整表错误，再接native构建分支。随后完整Logan frame转换与27/24/40/19/geometry投影、source/decode identity、carrier/schema与Play仍同Q05出口回访。CPoint/OPoint/Geometry/本record四Change均保持pending。Q06算法和旧landing一ULP、Q12旧phase、stage.dat暂缓不变。

脚本Ledger最终PASS：480 records /79 governed code files。此校验不认证未归因的Scene文件变化；Scene归属gate继续pending。
