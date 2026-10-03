<!-- CHANGE-RECORD
id: NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001
status: VERIFIED
change-kind: BATTLE_Q01_EDITOR_TEST_CONTENT_ORACLE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs
authority: current 336B44 formal resources/runtime and D-023 content decision
evidence: docs/ai/TASKS/NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001.md
-->

# Q01 暂存候选跨根内容核验口径

脚本修改前记录。原测试要求正式/暂存候选的原始对象定义指纹及其派生指纹相等；20份对象 DAT 仅换行字节不同，使测试在实际核对全部定义与图片前停止。生产身份是原始字节身份，不应为让测试通过而修改 DAT、淡化指纹或改加载器。

唯一脚本路径及改前 SHA 见同 ID Task。预期仅将跨根断言改成逐对象元数据与严格 CRLF→LF 文本对照、逐图片相对路径与 SHA 对照，继续验证两端候选各自未过期。不可回退边界：原始指纹及同根 cache key 逻辑不变，所有用户场景、资源和非战斗文件不改。验收、失败保留和回滚方式见 Task。

2026-10-03 实施更新：唯一脚本中将旧跨根原始对象/派生/视觉指纹等号改为 330 个对象的注册索引、ID、type、路径和严格 CRLF→LF 后 DAT 原始字节对照，906 张候选图以相对路径及原始 SHA 对照；两端各自的 `AssertInputsCurrent`、不同根 cache key 保留。生成 Editor 工程首轮编译 exit0、273 警告、0 错。原 Editor 已确认单一干净 Menu、非 Play，脚本刷新成功；第一次只运行具名测试 job `d085808ab559488383d59aa4d858f40d`，实际 1 项执行但失败在此前未单列的 `FusionInput.InputFingerprint` 原始字节等号（期望28E180...、实际F1D886...），尚未进入对象/图片循环，原始结果保存于 `artifacts/diagnostics/NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001/named-test-result.json`。这不是正式 fusion 语义首差：生产 `LoganFusionCatalogInput` 的 `InputFingerprint` 对所选 `fusion.dat` 原始 bytes 做 SHA。已在同一唯一测试脚本中将该跨根断言改为双方 fallback 选择一致、文件选择时严格 CRLF→LF 字节等价；原有 `FusionInput.SemanticFingerprint` 等号保留。仍待重编、原 Editor 第二次具名测试和保护核查；状态 `IN_PROGRESS`，不得报告通过。

2026-10-03 最终限定验证：同一生成 Editor 工程第二次编译 exit0/0 error/273 warning；原 Editor 脚本刷新、唯一具名 EditMode job `96e7743087e146fb953fb1c2615ed49e` 结果1/1 Passed，330 对象、906 候选图、fusion 文件和两端 freshness gate 均走完。四保护SHA逐项稳定，`LoganRuntime`无新增Git差异，Editor idle/nonPlay/Menu clean。`git diff --check` exit0；`Tools/Validate-ChangeLedger.ps1` 报 PASSED/1198 Record。唯一实际代码文件仍为所声明 Editor 测试，生产原始指纹、DAT、图片、Scene和非战斗均未改。回滚仍仅该测试差异且遵守用户批准边界。Game View、非对象图和Q01父项未验证；完整结果见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001/REPORT.md)。本 Change 仅测试口径子项 `VERIFIED`。
