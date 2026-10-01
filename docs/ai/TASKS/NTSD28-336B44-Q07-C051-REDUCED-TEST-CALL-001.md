# C051 相邻减伤测试反射调用修正

状态：VERIFIED（仅测试fixture）。总目标Q07/C051验证依赖，不改变战斗规则。原Editor重新编译后的相邻17项均PASS，原异常与结果保留在C051诊断目录。

新鲜证据：原Editor job `9dba91085c074d668bbcf049bc521d95` 执行17项，三个 `HitPlan_*` 因 `TargetParameterCountException` 失败。当前 `BattleEcsHitExecutionPlan.ProjectWriterEffect` 有7参数，最后 `nativeFeedbackOnly=false` 与 `nativeRoute=default` 可选；本类反射夹具仍传5参数。CaptureWriterEffectSnapshot的3参数数量正确，不能混改。

唯一脚本范围：`Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedState2000AwayDampingEditorTests.cs` 的 `ProjectWriterEffect` 测试helper，补两个 `Type.Missing`，让反射使用生产接口声明的可选默认值。保留五个原参数、ref投影索引4、断言与生产接口。不得改runtime、DAT、Scene、其它未提交内容。

验收：生成C#工程0错，原Editor本次相邻17项PASS，保留原失败证据；Change Ledger与diff检查通过。失败若转为行为断言首差，另按权威链调查，不改期望掩盖。回滚仅本helper两项参数（仍须授权），不覆盖文件其它内容。无新manager/queue/关闭阶段。
