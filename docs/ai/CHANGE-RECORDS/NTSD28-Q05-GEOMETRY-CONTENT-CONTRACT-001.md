<!-- CHANGE-RECORD
id: NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: GEOMETRY_CONTENT_VALIDITY_AND_DEPTH
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/BodyBox/BattleBodyBoxValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/BodyBox/BattleBodyBoxValueAdapter.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05GeometryContentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalKernelBodyBoxValueSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Tools/NTSD28Q05Geometry/AuthorityGeometryContentWitness.cpp
authority: Active Q05-A2; Q03 geometry witness and joint field contract; formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable CollisionGeometry28.decode.
evidence: RED_92_FAIL / FINAL_375_PASS / NATIVE_88_DOUBLE_STABLE / FULL_SELFCHECK_PASS / SOURCE_INTEGRATION_PENDING
-->

# Q05 几何内容事前实施合同

状态IN_PROGRESS / TEST_FIRST。九个准确脚本见metadata，父Q05-A2。当前正式EXE B1E13A…9033及playable CollisionGeometry28.decode决定几何有效性；Q03几何14x3见证和联合字段合同已读，不重做规则审计。

改动：BodyBox Value增加ZWidth、HasGeometry；mutable BodyBox增加zwidth/hasGeometry并双向复制。保留legacy default(Value)==new(0,0,0,0)与合成DTO默认几何有效的已有含义，immutable以private geometryMissing存储、HasGeometry为其反值。native缺失或非法字段必须显式new(...,hasGeometry:false)，从不return default替代缺失。显式0尺寸/负尺寸可有几何，非正尺寸不等于缺失。

InteractionArea增加独立z与hasGeometry，legacy默认z0/hasGeometry true/zwidth15保持；新native ApplyInteractionGeometry根据strict x/y/w/h成功状态赋值并raw zwidth缺省0，不在数据层回退15；不与dz/dvz混同。新增LoganCombatRecordDecoder.BodyBox及ApplyInteractionGeometry，只解码几何，不假冒完整ITR decoder。kind100100且不完整保留kind+hasGeometry false，不丢record/frame。

CopyFrom与MemberwiseClone完整保留新字段；HitExecutionPlan ItrProjection ctor及两个Fingerprint同步捕获z/hasGeometry。现有kind5 compatibility replacement保留源X/Y/W/H/z/有效性，与实际ShallowCopy路径一致，不在本包新增native replacement规则。Zwidth原替换行为保持；Q06统一审查native strength19及candidate深度。Body列表本为struct整值传递，无需改collector/preview。武器两个cache是一次new InteractionArea且新字段无production writer，不为此改无caller ProcessAttack；实际hit-resolver缓存依赖CopyFrom，需测试反复覆盖新字段而非保留前次数据。

验证：新增source-linked C++工具逐bdy/itr直接调用CollisionGeometry.decode，输出实际has-box/control-only和raw int字段。复用Q03 14对fixtures，增加缺失/非法/正零/负宽/重复/大小写/100100等边界。先RED缺字段/decoder，再原版逐字段对比、adapter/default差异、CopyFrom/clone反复覆盖、projection/fingerprint、旧Body与HitPlan定向回归、完整SelfCheck。C++工具输入只读/stdout，无authority写入，编译物Temp，不新增Unity服务。

本包不改候选算法、不把Q03 27个首差关闭，不切manager或改外层版本，不发布资源；完整ITR其余字段drain/sound/cover、weapon-strength以及native转换接线另在同A2准确Record内处理。CPoint/OPoint既有字段保持，两个Change仍待接线。非战斗/GAS/Unity/Scene/Gen/Plugins/外部包/33ms/十一阶段不动；无新runtime manager/queue/worker，回滚经批准仅逆本包增量并保留他人工作。


## 当前实现与实际命令

Native58文件88几何记录双跑一致，含Q03原14对；source-linked工具精确调用bdy/itr的CollisionGeometry.decode，构建0error。RED jobbc20a0f2174b473baa66adc4b82a1ea9为92/92缺入口/字段失败，XML保存。九脚本增量已写，未改collector或kind5 replacement；geometryMissing编码保留legacy default语义，native明确HasGeometry false。dotnet build UnityContentCapture.csproj 0warning/0error；Unity重载完成，Green正在运行。


## 最终本轮证据与边界

最终375/375、native88双跑一致、完整SelfCheck PASS、CS0/Scene clean/root14；旧138加载无新增旧投影差异。初次371/375的4旧夹具由独立TEST_ONLY Record纠正，原失败保存，production规则未变。完整报告同ID artifacts/diagnostics/REPORT.md。保持FOCUSED_TEST_PASS/SOURCE_AND_ALGORITHM_INTEGRATION_PENDING，Q06旧27候选首差未关闭，不发布Q05中间ABI。

最终Ledger实际PASS：479 records /77 governed code files，见Geometry工件ledger-final.txt。几何九脚本与独立测试一脚本已人工复核；未声明脚本无本轮额外变化。

## Source/typed接线回访（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001）

当前frame来源已接入实际Logan manager，55348完整typed投影/330构建/557不同focused及SelfCheck通过，同ID REPORT为证据。本Record早先source-pending的帧层子条件现PARTIAL_RETURN；definition头部、semantic identity/联合schema、对应runtime consumer/Play仍待，状态保持FOCUSED_TEST_PASS，不把此回访扩大成整域完成。
