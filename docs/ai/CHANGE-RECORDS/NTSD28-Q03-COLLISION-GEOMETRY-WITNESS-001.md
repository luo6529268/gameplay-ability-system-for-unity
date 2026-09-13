<!-- CHANGE-RECORD
id: NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001
status: VERIFIED
change-kind: DIAGNOSTIC_TOOL_AND_EDITOR_CAPTURE
code-path: Tools/NTSD28Q03Geometry/AuthorityGeometryWitness.cpp
code-path: Tools/NTSD28Q03Geometry/Build-And-Capture.ps1
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q03GeometryWitnessEditorTests.cs
authority: User active realignment goal; Q03 task; formal NTSD2.8-Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 playable battle_world/hit_candidates/collision_geometry path.
evidence: VERIFIED_CAPTURE_ONLY / NATIVE_14_DOUBLE_RUN_STABLE / UNITY_EDITOR_CAPTURE_42 / PARITY_15_EQUAL_27_DIFFERENT / COMPILE_CONSOLE_0_CS / SCENE_CLEAN / LEDGER_471_43_PASS / NO_PRODUCTION_CHANGE / NOT_PLAY_PARITY
-->

# NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001

## 事前合同

完整范围、不变量、验证与回滚见同ID Task。Unity原状：Body converter只输出四项几何，ITR zwidth默认15，候选若干路径用严格深度区间。原版live几何使用body depth、ITR z及包含端点比较，需同输入见证而非只读推断。

预期改动只添加隔离diagnostic source与Editor测试，直接调用现有生产方法，不修改命中规则或资源。不得以测试PASS掩盖已输出的不一致，不修改期望值使差异消失。工具进程退出释放文件，test finally注销两实体；无新的runtime manager/queue/pool，十一阶段关闭不变。schema12/20/23保持，正式资源不变。没有不可回退的数据迁移。

## 实际实施与验证

尚未写脚本；后续在本节追加实际编译、capture、comparison及未验证边界。

### 最终限定出口（覆盖上条事前状态）

实际新增metadata三脚本、Unity生成meta、工具README及14组共享DAT/位置fixture和诊断工件。原版真实parser/geometry函数与Unity实际Parser/Converter/collector均已执行；没有新增或替换生产公式。native编译/14用例双跑稳定；真实Unity capture job `8c6b30b020fa4569b928072785a8d5e8` 1/1完成，生成42项数据，15相同/27不同。三种模式各9项差异。输入29文件hash保持。

完整实际命令、结果、第一次连接超时后确认无live job、scope和风险见 `artifacts/diagnostics/NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001/REPORT.md`。CS0、Scene dirtyfalse/root14、Ledger471/43PASS。没有执行Play/完整SelfCheck，capture PASS不是战斗对齐PASS。Q03父任务仍未冻结；新BDY字段与presence契约必须进入Q05同窗口，精确规则修复留Q06。
