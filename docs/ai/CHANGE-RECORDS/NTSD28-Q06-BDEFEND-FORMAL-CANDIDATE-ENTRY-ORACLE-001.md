<!-- CHANGE-RECORD
id: NTSD28-Q06-BDEFEND-FORMAL-CANDIDATE-ENTRY-ORACLE-001
status: IN_PROGRESS
change-kind: TEST_ENTRY_BOUNDARY_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06BdefendFieldFamilyEditorTests.cs
authority: Original bdefend witness calls full resolve_ordinary_unarmored/type1 candidate entry, including armor/prelude routing; Unity formal ownership now lives in common candidate sequence before typed damage backend.
evidence: Direct test previously called a lower typed damage fragment, bypassing the exact newly aligned prelude/feedback boundary; Shadow test already uses the formal PostInteraction pipeline.
-->

# Bdefend 正式候选入口测试边界

IN_PROGRESS / TEST_ONLY。只将direct组从DamageWriter.TryApplyCurrentDatTargetHit改为已有冻结候选的正式统一consumer。source256的before/after raw、独立HitStateCount241、C25 held/free恢复断言全部保持；Shadow组保持。lower damage writer是路由后的行为片段，不应被当成C++完整ordinary入口；不在backend重复执行prelude/RNG来满足错误测试边界。

验收四组实际运行并保存剩余weapon/reduced等差异，不能把这次改动视为生产行为已全对齐。依赖NONCHARACTER-ARMOR-FEEDBACK-001所声明的新测试helper；两个同命名空间测试均为Editor/test条件编译，不进入正式runtime。回滚仅此测试差量并遵守用户授权规则，不改生产/Scene/资源/非战斗，禁止computer-use。

入口修改已编译并实际运行四组，各256 before匹配；当前direct192/Shadow208，原96反馈问题已不再由错误测试入口制造失败。剩64武器raw和16type5观测guard对应父任务实际依赖，状态保持IN_PROGRESS / ENTRY_FIXED_PARENT_DAMAGE_PENDING，原断言及失败证据均保留。
