<!-- CHANGE-RECORD
id: NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: OPOINT24_CONTENT_AND_TASK_TRANSFER
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/ObjectPoint/BattleObjectPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/ObjectPoint/BattleObjectPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05OpointContentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalKernelObjectPointValueSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Active Q05-A2; Q03 OPOINT-AND-HELD-DEPTH-CONTRACT; formal Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable object_spawning.cpp decode.
evidence: RED_49_FAIL / GREEN_98_PASS / NATIVE_45_BY_24 / FULL_SELFCHECK_PASS / SOURCE_INTEGRATION_PENDING
-->

# Q05 OPoint24 内容及复制合同

状态IN_PROGRESS / TEST_FIRST，父Q05-A2。准确七脚本范围见metadata。正式ObjectSpawnPlanner28.decode的24字段映射与Q03冻结合同为依据，严格int32/exact-case last-win/default0。原版source/EXE身份复用已验证构建manifest并重新核对。

原状Value8/DTO10（objectId与dvz额外）；Adapter丢dvz。改后Value24，DTO加其余15项、保留objectId但不晋升内容；Adapter双向完整复制24项。8参数构造保留可调用，新16int默认0。新LoganCombatRecordDecoder.ObjectPoint入口复用strict numeric helper；旧共享Converter/manager仍不切来源、不发布候选，不提前接factory新规则。值对象hash/equality包含全部24项。

单/多task自身已有opoint struct整值复制及Clear default，本包先测试现有路径不改生产复制/生命周期。重点测试BattleLogicObjectPointRuntime.CopyMultipleTaskToSingle实际私有静态复制方法与BattleLogicReferencePool的真实回收/再借，renderer两处inline struct赋值静态确认；renderer真实完整materializer/Play新字段消费仍Q06及Q05联合出口，不将静态判断冒充实际运行验证。

验证：先RED24字段/新decoder缺失；source-linked native工具生成真实24字段fixture输出；Unity逐字段对照、24个独立identity变化、strict边界/重复/未知key、single/multi复制和pool清零、旧8字段行为回归及SelfCheck。更正旧测试属性数量和dvz被丢弃断言，保留旧内容golden原始文本作为8字段历史向量，不改其历史digest来伪造新证据。正式24字段用新native向量单列。

风险：dvz与task.dvz/pos.z混同，必须只改变值传递不重排spread/方向/物理；新hp/team/reserve只是内容，生产覆盖/资源初始化归Q06。当前CPoint27仍SOURCE_INTEGRATION_PENDING，OPoint同窗口迁移不另升一次版本。Gen/Plugins/Server外部包/Unity-GAS架构/非战斗/Scene/资源/33ms/十一阶段保持；无新manager或queue，无关闭阶段新增。回滚经批准仅本包精确增量并保留CPoint/前批工作，不做破坏性Git。

## 实际验证

RED待运行。


## 实际进度

RED job413bc5fe58be43cc92ab4b516ba1e73c，49项全部因缺少decoder/24属性失败（RED.xml）。已完成七脚本限定增量：Value24、ObjectPoint新增15int（原dvz已在）、Adapter完整24项、新ObjectPoint decoder、旧属性数量/dvz断言更新、新49项测试。task/factory生产代码未改。内容工具source-linked dotnet build 0warning/0error；Unity重载/Green待完成。原版43文件45向量parse成功，43个warning均为纯block夹具省略bmp，不是内容decoder错误；exe与原manifest一致。


## 当前交付状态与待回访

FOCUSED_TEST_PASS/SOURCE_INTEGRATION_PENDING保持活跃。98/98、native45x24、实际多转单与pool复用、完整SelfCheck PASS，源码链接工具0诊断、138旧加载0新增投影差异，CS0/Scene clean/root14。完整事实、实际命令、静态与运行证据区别、未验证项和下一项见同ID artifacts/diagnostics/REPORT.md。renderer完整materializer和新规则消费未验，不标VERIFIED；同窗口来源/identity/版本/Play完成后回访。

最终Ledger校验实际PASS：477 records /70 governed code files（ledger-final.txt）；准确七脚本及现有共享文件分段复核完成，未触及任务factory/队列生产代码。

## Source/typed接线回访（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001）

当前frame来源已接入实际Logan manager，55348完整typed投影/330构建/557不同focused及SelfCheck通过，同ID REPORT为证据。本Record早先source-pending的帧层子条件现PARTIAL_RETURN；definition头部、semantic identity/联合schema、对应runtime consumer/Play仍待，状态保持FOCUSED_TEST_PASS，不把此回访扩大成整域完成。
