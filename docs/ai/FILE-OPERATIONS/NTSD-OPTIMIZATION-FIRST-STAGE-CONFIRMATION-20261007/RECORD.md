# 首阶段范围与 Foot 方向确认文档操作

Operation ID：`NTSD-OPTIMIZATION-FIRST-STAGE-CONFIRMATION-20261007`。
状态：`VERIFIED`（仅文档）。仅文档局部修订；无代码 Change ID，无优化子批计数，无删除或移动。下方事前计划为历史，实际结果另行追加。

## 授权、范围与恢复来源

用户本轮确认六项有限首阶段和“Foot 采用此前建议的方向”。本操作仅登记确认，不执行生产修复，不创建 Goal 或启动 Unity/测试/性能测量。执行者为当前 Codex 主任务；工作目录及批准根目录为 `I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity`。

操作前检查时间 `2026-10-07T03:08:24.2690482+08:00`，检查 PowerShell PID `63560`；HEAD `8107196b1f17ee0f7ce9e7fcbb7ffb9fc260956c`。现存工作区修改及未跟踪内容保留；以下三份当前工作区字节已 Copy-Item 到本目录 `before/`，各源与备份 SHA-256 相同，不以 HEAD 代替恢复源。

| 相对路径（相对于批准根目录） | 字节数 | 操作前 SHA-256 | Git 状态 | 备份 |
|---|---:|---|---|---|
| docs/ai/TASKS/NTSD-OPTIMIZATION-BOUNDED-AUTOGOAL-20261007.md | 14171 | EEE1CABBCBAE7FC225800F04C00FDD11825B62D3C85A1F994D16B892FE37C387 | 未跟踪 | before/NTSD-OPTIMIZATION-BOUNDED-AUTOGOAL-20261007.md |
| Assets/NTSD/Docs/battle-optimization-progress-tracker.md | 42767 | E3231BF35E2F3792670B648008D4C6427DF7FEBF0882EC2D95BB3B07555DD62C | 已跟踪、工作区已修改 | before/battle-optimization-progress-tracker.md |
| docs/ai/FILE-OPERATIONS/INDEX.md | 39933 | CE8BC2F738084170AB85D8B29DA5D0E73F85B7D15B778613AB2B724969AA7C53 | 已跟踪、工作区已修改 | before/INDEX.md |

操作范围仅为上述三个完整文件路径的局部 apply_patch：合同更新确认状态/Foot 门、总表增加确认记录、索引登记本操作。旧批次失败证据、父项实现/验收状态、PERF/ATLAS/Mono/EXT-1 合同均不改。新增本 Record 和 before 备份位于三个目标文件范围之外。

恢复仅在用户明确批准后，用上述准确备份恢复本操作涉及的文件，并保护后来并发改动；不得执行 reset/checkout/clean/restore。未计划缺失或哈希冲突时停止写入并报告。

## 拟执行与验证

1. apply_patch 新建本 Record 并在 INDEX 登记 PLANNED，随后局部修订合同和总表；不运行任何项目脚本。
2. 重读确认条款，检查本地链接、尾部空白、git diff --check；复核三份备份，以及操作范围外原有 23 个 dirty/untracked 文件 SHA。
3. 追加实际结果和操作后目标文件 SHA，推进本操作状态；文档通过不能冒充实现/性能验收。

准确执行调用与原始结果为本次会话的 exec_command/apply_patch 工具记录。尚未执行主文档修订，尚无操作后结论。

## 实际结果

实际调用为 apply_patch（退出正常，无错误）新建事前 Record/索引登记，再局部修改合同与总表；随后 exec_command 只读验证。首次验证结束于 `2026-10-07T03:09:58.1334123+08:00`：三份 before 备份 SHA 均与操作前相符，范围外原有 23 个 dirty/untracked 文件 SHA 不一致 0；合同 5 个本地链接缺失 0，尾部空白 0，git diff --check exit 0（仅既有 LF/CRLF 提示，无空白错误）。HEAD 保持操作前身份，暂存文件数 0。确认状态为 SCOPE_CONFIRMED / FOOT_DIRECTION_CONFIRMED / EXECUTION_NOT_STARTED。

| 文件 | 操作后 SHA-256 |
|---|---|
| docs/ai/TASKS/NTSD-OPTIMIZATION-BOUNDED-AUTOGOAL-20261007.md | 48579472D10BC06C140A3CEEB5365C0F69777F74FCBFF4EB984510008B748A9A |
| Assets/NTSD/Docs/battle-optimization-progress-tracker.md | C4F2F17B29615E8C461743B3B25E71171EC3862D5E01B48BAC20A3C28EF6EC47 |

INDEX 随后将本操作登记状态由 PLANNED 改为 VERIFIED，最终身份在收口检查中追加；Record 不计入自引用哈希。实际触及范围为事前列出的三份目标文档、新 Record 和三份新备份。没有运行项目脚本/Unity/测试/Profiler/GPU capture/M0，没有读取活跃 Q06 方法体，没有创建 Goal、启动新优化子批、修改脚本或父项完成状态。未执行原实施/性能验收，文档 VERIFIED 不表示优化 VERIFIED。

收口检查 `2026-10-07T03:10:26.4153235+08:00`：INDEX 最终 SHA-256 为 `F39E657C4EC89FDB7101C2EFC3424AA0A6255593ECEC8C16588BDD66A32F3260`；合同第 3/116–118 行与总表第 11 行确认范围/方向/未启动状态相符，git diff --check 再次 exit 0。无文件删除、移动、Git 提交或工作区回退。
