# Q01 当前 DAT 原始字节差异的逐文件复核（2026-10-05）

状态：`READ_ONLY_EXACT_BYTE_DECISION_PENDING`。本轮没有改动 DAT、PNG、`.meta`、Scene、生产脚本或配置。正式根 `NTSD2.8-Logan.exe` SHA-256 复核仍为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。

对既有 338-DAT 清单中 25 条原始 SHA 不同的文件，重新从正式 `resources/runtime/decoded_dat` 与生产暂存 `Assets/NTSD/Content/LoganRuntime/decoded_dat` 读取当前磁盘字节并逐文件计算 SHA-256。结果为对象 DAT 20、全局 DAT 5；25/25 在严格 CRLF→LF 归一后逐字节相同。生产侧 25/25 是 Git 已跟踪且当前无本地差异的文件，邻接 `.meta` 25/25 存在并含 GUID。[逐文件清单](manifest.csv)记录双方 SHA、字节数、对象 ID、Git 状态与 GUID；清单自身 SHA-256 为 `FF6D30B027C3A7EB6A326627087EB70146DCE2BBE2AD90E81483918FCBEC6C9A`。

这 25 份文件已经使用正式版相同的 DAT 文本内容；差异是原始换行字节。既有[跨根内容测试](../NTSD28-336B44-Q01-CANDIDATE-SEMANTIC-ORACLE-001/REPORT.md)在不修改生产身份算法的前提下，按严格归一内容和图片原始 SHA 通过。生产 `LoganObjectCatalog.DefinitionFingerprint` 仍包含原始 DAT SHA，故正式目录与暂存目录的原始/派生身份确实不同；不能把归一内容等价写成原始字节身份相等。此事实本身没有证明战斗行为首差，且当前 336B44 执行队列不因单纯原始换行差异开新的战斗修复包。

若最终要求两根 DAT **逐原始字节**一致，唯一内容层处理是将这 25 份生产暂存文件精确替换为正式版原文件字节，保留原 `.meta`/GUID。这样会覆盖现有文件，即使不改变任何 DAT 字段或数值，也必须先按 `docs/ai/file-removal-audit-contract.md` 建立逐文件 Operation ID、备份现有字节和核对恢复来源。用户先前同时要求使用正式 DAT 且不要在未要求时修改任何 DAT 数据；在范围决定明确前，本轮不执行覆盖，也不以修改指纹算法伪造原始一致。
