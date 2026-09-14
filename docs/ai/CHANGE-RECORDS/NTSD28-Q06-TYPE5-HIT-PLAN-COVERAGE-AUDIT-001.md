<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE5-HIT-PLAN-COVERAGE-AUDIT-001
status: VERIFIED
change-kind: CURRENT_DAT_HIT_PLAN_COVERAGE_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06BdefendFieldFamilyEditorTests.cs
authority: Current playable standard unarmored type5 damage path; BDEFEND-FIELD-FAMILY-SOURCE-WITNESS-001 and current Unity candidate pipeline.
evidence: After verified weapon migration direct256 passes; each Shadow profile has16 type5 no-armor observations failing with previous mask0-only messages.
-->

# Type5 Shadow覆盖取证

IN_PROGRESS / DIAGNOSTIC_ONLY。准确只改已有Bdefend测试报告，附加失败case的完整plan diagnostics、原row和两端CLR/current DAT类型；保留256向量、原source raw、HitStateCount241、C25 timer和所有断言。不改变生产或跳过观察。

先实际读取valid/count/failure与入口，再为所需生产路径追加准确范围。只读追踪CanProject/Observe和typed backend与当前源；不以旧类型名猜所有权，不修改planner架构/schema/Server/非战斗/Scene/资源。验收为可复现的16例详细诊断及源路径解释，生产实现必须有后继证据。回滚只本差量，不覆盖既有未提交修改，禁止computer-use。

DIAGNOSTIC_COMPLETE / TYPE5_TRANSACTION_DEPENDENCY_OPEN：独立2022 batch已exit2，4项中direct2PASS、Shadow2FAIL。各16例plan valid=true/failure0、candidate/disposition/dispatch各1、writer0；target CLR LF2OtherObject/current DAT5。CanProjectDamageWriterEffect无type5分支，继而排除于weapon和type3 guards。已归档headless-tests.xml与四份coverage报告。静态发现实际type5旧反应也不匹配源，先TYPE5-UNARMORED-UNITY-001完整源事务；本父不因诊断完成而关闭。

VERIFIED / DECLARED_NO_ARMOR_TYPE5_WRITER_COVERAGE。通过TYPE5-UNARMORED-UNITY-001先修真实事务再补独立预测；BDEFEND四组256全PASS，原每profile16观察缺口已清；type5完整585四组与Play2340、SelfCheck、关闭均通过。带armor/特殊matched及其它DAT-CLR组合并未凭此全部验收，仍为父回访。
