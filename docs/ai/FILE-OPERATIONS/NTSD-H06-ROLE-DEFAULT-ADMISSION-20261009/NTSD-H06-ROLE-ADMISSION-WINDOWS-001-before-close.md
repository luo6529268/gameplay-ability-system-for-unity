<!-- CHANGE-RECORD
id: NTSD-H06-ROLE-ADMISSION-WINDOWS-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/H06RoleDefaultAdmissionEditorTests.cs
authority: User approved named Role admission and producer reuse tasks, D1=A
evidence: docs/ai/TASKS/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009.md
-->
# Role当前准入窗口

脚本前登记；准确路径/符号、风险、原状/副作用/容量关闭、验证及回滚详Task；仅Editor冷接线和输出隔离，不改生产默认或比较标准。
实施：Suite新增独立output/cold字段、沿用81请求和observer/owner；新6case保障同配置。首轮测试缺ProductionEntityStressRequest命名空间CS0246，已按实际namespace补NTSD.Animation.Rendering.Editor；生产仍未改。复用指纹对比此前保护仅FindEffect与本次Suite变化，其余生产/Driver/测试依赖同字节。
