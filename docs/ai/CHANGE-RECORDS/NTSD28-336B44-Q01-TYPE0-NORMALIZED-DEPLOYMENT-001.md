<!-- CHANGE-RECORD
id: NTSD28-336B44-Q01-TYPE0-NORMALIZED-DEPLOYMENT-001
status: VERIFIED
change-kind: BATTLE_Q01_EDITOR_TEST_LINE_ENDING_CONTRACT
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs
authority: current 336B44 formal resources/runtime and D-023 content decision
evidence: docs/ai/TASKS/NTSD28-336B44-Q01-TYPE0-NORMALIZED-DEPLOYMENT-001.md
-->

# Q01 type-0 正式 DAT/角色图片部署测试口径

脚本修改前记录。当前测试在 OID77 的原始 DAT SHA 相等断言处停止，后续 158 名角色的帧数和图片逐 SHA 未执行；已封存的正式/暂存磁盘清单证明此 DAT 与其它部分对象仅换行字节不同，生产 parser 对两端 330 个定义解析成功。测试职责是核对正式内容能被暂存根用于战斗，不能把仅换行不同误判为数值不同，也不能把正式原始指纹相同伪造出来。

本包只改上述一份 Editor 测试：保留相同原始 SHA 的快速路径，在 SHA 不同的 DAT 上对原始文件字节作 CRLF→LF 严格归一再比较；路径、帧数、全部声明图片 SHA 断言不放宽。作用、未改范围、验收及回滚详同 ID Task。先建 Task/Record/Ledger/STATE/handoff；若测试继续失败，保留首差，不扩到生产或 DAT 改值。

代码已写：仅 `CharacterAssetDeploymentEditorTests.FormalTypeZeroCharacterDatAndImagesAreDeployed` 将原始SHA不同时的断言转为 `NormalizeCrLf` 后逐byte比较；相同原始SHA直接通过。新私有 `NormalizeCrLf` 只删除CRLF配对中的CR，保留其它所有字节，图片原始SHA、角色路径和解析帧数断言保持。下一做独立type-0字节清单、生成Editor编译、原Editor定向单测，按结果如实更新状态。

2026-10-02 实际验证：独立 type-0 DAT 字节重读 158 份中原始相同157、严格CRLF→LF后相同158，只有OID77原始SHA不同；见 [限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q01-TYPE0-NORMALIZED-DEPLOYMENT-001/REPORT.md)及审计JSON。最终生成Editor工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` 0 error/251 existing warning，原Editor `refresh_unity(force/scripts/compile=request)` 后重新编入脚本。只跑具名 EditMode 测试 job `3e70cde305c8445d84b026633cfdff14`，1/1 PASS、0 failed、17.06秒；原始和紧凑作业结果均封存。活动原Battle Scene isDirty=false、Editor idle/nonPlay/noncompiling，四保护SHA与 `LoganRuntime` 限定Git状态稳。实际代码仅该测试一个DAT条件断言及其私有归一函数；没有编辑DAT/PNG/生产/Scene。此ID只限角色内容磁盘测试出口，不称原始DAT指纹一致或Q01/战斗画面已完成。

交付审计：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit0、`Change ledger validation PASSED.`，原日志见 `artifacts/diagnostics/NTSD28-336B44-Q01-TYPE0-NORMALIZED-DEPLOYMENT-001/change-ledger-validation.txt`；首次 `git diff --check` 仅指出总表文末多一个空行，已精确去除后重跑 exit0，仅现有 CRLF 提示。没有扩大测试到其它角色/战斗场景，因为本包单个测试已覆盖全部158名type-0角色磁盘断言，视觉出口另门。
