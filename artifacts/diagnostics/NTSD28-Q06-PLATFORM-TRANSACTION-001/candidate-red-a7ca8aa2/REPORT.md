# Platform candidate measured RED

Job a7ca8aa2733c49b1b344f4a57c0d0681，4 executed / 2 passed / 2 failed。
Source nominal(index0) default + ForceBruteForce 两入口：初态位置/reference断言通过；候选后目标 integerY 应 -20，实际 -10；报告 reference 实际0、期望-20。
Source strict_x_edge(index1) 两入口：位置/reference拒绝投影通过。
这仅证明候选位置/reference投影，尚未绑定previousY/platformSlot/shadow；不证明完整源初态、fulltick或阴影。名为default的用例使用当前World默认配置，不额外指定优化模式。
初次编译失败：项目 NUnit 不支持 Assert.Multiple；移除新测试内该API后成功重载，实际运行上述4项。JSON数值比较按double逐轴比较，避免int/float token类型误报；无期望值弱化。
Unity生产未改，Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。未运行fullSelfCheck/Play。Ledger验证见Logs/Q06-Platform-Unity-Ledger.log。
下一步：追加完整平台生产路径前，确认native previousY准确物理生产时点/初始化/持久化合同及平台参与时有序候选路径。保留两个RED作为修复验收，不重跑未变全套。
