# 本包最终治理检查

实际命令：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity。最终exit0，Change ledger validation PASSED，Records1276，Governed code files in diff25；完整原件ledger-final-validation.log。

实际命令：git -c core.safecrlf=false diff --check。exit0，diff-check.log空；未持久修改Git配置。两个检查独立只读并行，未重跑任何Unity测试。

先前ledger预检查exit1为artifact非governed code-path元数据误分类，后按validator既有范围改diagnostic-source-path，Task/Record/Operation/源文件/哈希全部保留；ledger-preclose-validation.log失败原件不删除。没有改validator或绕过拒绝。
