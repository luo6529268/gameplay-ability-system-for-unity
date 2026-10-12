# 收尾澄清与后继审阅约束

本补充仅澄清报告及记录后继测量设计要求，没有修改任何源码，没有运行额外测试或 Player。REPORT.md、RESULT.json 及失败版源码保持原指纹。

## Editor 进程与 domain reload

压缩恢复后的首条聊天更新使用了“Unity 重启后”，沿用了早期用户消息，不是本轮 H10 的观察结果。此表述在聊天中已更正。本轮原件记录的是脚本导入/编译及显式 EnterPlayMode、ExitPlayMode 导致的 domain reload；当前执行记录没有 Editor 进程启动/停止命令，也没有本轮 Editor 进程重启的直接证据。不补写重启来源、时间或授权。REPORT 中本轮无 Editor 重启的操作说明保持。桥接 job 的 stale running 与实际 NUnit XML、idle/非Play 状态分开报告。

## 验证范围与日志

本执行会话的 protection-audit.json 检查4094条具名保护指纹且无漂移，final-audit.json 复核8份源码/程序集和10个初始写域文件的最终哈希。讨论会话发来的独立审阅说明只确认重算了8份当前源码/程序集SHA；不得把4094条扩写为该会话独立验证。

两次 Windows PowerShell 调用失败原件完整保留。现有 PowerShell7 执行原 ChangeLedger validator 为 exit0 / PASS；没有改脚本、忽略检查或修改 Git 配置。REPORT 现在正确链接已存在的 final-audit.json 与 change-ledger-pwsh7.log，并保留两个失败日志的直接链接。14个报告本地链接存在，第一份 artifact-manifest.json 的103条哈希均验证一致。

## 后继计量设计要求：仅供独立审阅

三个热测范围需先明确实际调用关系。publish+dispatch 范围可以包含 sink.PresentSounds，不能共用同一个 recorder 嵌套 Begin/Reset。后继实现必须明确选择独立 recorder 实例及其生命周期，或明确排他的采样安排，分别保留各范围前后校准、ownerThread、Unit、capacity、valid、wrapped、saturated、sampleCount及原值。校准原件需要覆盖失败路径；byte[97]失败原件保留。generic borrowed负控改选真实 data/016 的建议不授权创建缺失资源。

BattleScopedGcAllocationRecorder 仍为可复用工具候选，本轮未接入或测试。以上不构成新增Play、源码修正、Player构建或Settings写入批准；本Task仅收尾，后继准确修正范围由讨论会话独立审阅后派发。H10父项继续OPEN，不自动暂停、不恢复Goal或其他优化队列。
