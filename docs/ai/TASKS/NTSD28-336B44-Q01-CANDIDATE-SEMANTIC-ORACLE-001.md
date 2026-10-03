# NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001

状态：`PLANNED`。父项：336B44 对齐总表 BATCH-01/Q01。权威为正式根 EXE `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 `resources/runtime`，DAT/角色图范围服从 D-023 及用户排除项。

当前原 Editor 测试 `NTSD28Q07StagedCandidateIdentityEditorTests.StagedCandidateMatchesFormalRuntime` 将正式根与项目暂存根的原始对象 DAT 指纹、派生“SemanticFingerprint”和视觉缓存指纹要求相等。已封存的 330 对象内容审计证明 20 份 DAT 仅 CRLF/LF 字节不同；这些指纹按原始 SHA 计算，故原断言会在核对所有定义和图片前失败。修正对象只是该测试的**跨根内容核验口径**，不改变生产身份、缓存失效或原始指纹合同。

唯一脚本路径：`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs`，改前 SHA-256 `E1E9870FCBAC32D4791A9B3F3E4A419C0D1764834191A4AECF81F5E8215AE52B`。保持 330 对象/906候选图、正式 fusion 输入、两端各自 `AssertInputsCurrent` 与不同根 cache key 的断言。逐索引对照对象 ID、type、路径和 CRLF→LF 后 DAT 文本；逐相对路径对照候选图片 SHA；保留明确的原始指纹关系，不把它们改称语义等价。发现非换行文本或图片差异就保持失败并登记 first difference。不得修改 DAT、PNG、Scene、配置、生产代码或非战斗功能。

脚本前建立 Task、Change Record、Ledger、STATE、handoff。脚本后先构建生成 Editor 工程 0 error，再由已保存且空闲的**原项目唯一 Editor**刷新并仅运行这个具名 EditMode 测试；检查 Battle/Menu/GameConfig/ProjectBattleModeConfig SHA、候选内容目录 Git 状态、`git diff --check` 和 Change Ledger validator。测试通过只关闭 Q01 候选跨根内容核验子项，原始指纹仍不同，Game View/非对象图消费者及 Q01 整组继续开放。

回滚只针对本测试脚本的本 Change 精确差异并需遵守仓库删除/还原审批规则；现有用户未提交修改均保留。

2026-10-03 进度：`IN_PROGRESS`。首轮原 Editor 具名测试 1 项失败于所选 `fusion.dat` 的原始输入指纹等号，未进入对象/图片循环。该输入指纹按原始字节计算；本包已在同一测试脚本中改为 fallback 选择一致及文件严格 CRLF→LF 字节等价，同时保留融合解析语义指纹等号。首轮结果留存，重编及第二轮原 Editor 具名测试待完成。

2026-10-03 出口：`VERIFIED_SCOPED_CANDIDATE_CONTENT_TEST`。第二轮原 Editor 具名测试 1/1 PASS，生成 Editor 工程0错误；330对象 DAT、906候选图片与 fusion 选择全部完成跨根检查，原始指纹等号未伪造。四保护SHA、LoganRuntime限定Git状态稳定，Editor回空闲干净Menu，Ledger及diff检查通过。详细证据见 [报告](../../../artifacts/diagnostics/NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001/REPORT.md)。Q01父项仍开。
