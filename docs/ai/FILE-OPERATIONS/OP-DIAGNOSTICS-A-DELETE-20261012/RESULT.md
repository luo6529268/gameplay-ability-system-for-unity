# A 清单删除执行结果

**本次删除未执行，状态 REJECTED。** 执行工具在启动 PowerShell 进程前拒绝了已批准的删除命令，返回 blocked by policy，没有提供更具体原因。

A 清单 1,558 个文件（433.47 MiB）全部仍在，逐文件 SHA-256 与操作前一致。实际删除 0 个文件，移除 0 字节。没有改用其他工具重试。

恢复来源已保留：1,539 项准确 Git commit/blob 已在预检中直接读取 289 个唯一对象核对内容；19 项副本恢复已另存 5 份独立副本，原 D 来源与独立副本都再次校验通过。Git commit 为 9fa291bf154269ae67ce34d3121ed40c7673440b。

范围外 22622 个原文件存在性、大小与 mtime 核对：缺失 0，变化 0。该检查不等于第二次重读所有非 A 文件内容。未控制 Unity、未编译或运行游戏测试。

- [操作记录](RECORD.md)
- [工具拒绝诊断](rejection.json)
- [准确尝试命令](EXECUTION-COMMAND.txt)
- [删除前逐文件记录和恢复来源](before-manifest.json)
- [删除后逐文件核对](after-manifest.json)
- [最终结果](result.json)

用户已明确批准本范围，无需重复授权。删除动作仍待执行环境允许后继续。
