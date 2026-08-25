# Handoff — R1-WP02 C++ Release 只读 trace 采集准备

> 日期：2026-08-21
> Work Package：R1-WP02 — C++ Release read-only trace acquisition
> 状态：**BLOCKED 并已停止**。
> C++ authority：J:\QQFile\NTSD2.4\ntsd_release 中实际构建的 ntsd_new.exe；本次没有启动、修改、重建或写入该工程。

## 1. 本次范围与文档修正

用户将 R1-WP02 的 C++ 边界明确为：

> 从未修改的 C++ Release runtime 以只读方式获取 trace，并在非 authority 目录中保存采集结果和比较资料。

因此已更新：

- Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md 的 R1.1；
- docs/ai/TASKS/R1-WP01-trace-contract-planning.md 中的 WP02 定义与 amendment；
- docs/ai/STATE.md；
- docs/ai/DECISIONS.md，新增 D-006；
- docs/ai/HANDOFFS/HANDOFF-R1-WP01-trace-contract-planning.md。

现行规则：不得修改 C++ 源码、头文件、Makefile、可执行文件、DLL、资源、DAT、配置或 C++ authority 目录中的输出文件；不得新增 instrumentation、trace sink、fixture bootstrap、input bridge、CLI 或诊断写入。

## 2. 已确认的只读事实

| 项目 | 证据等级 | 结果 |
|---|---|---|
| Release executable 存在 | VERIFIED | J:\QQFile\NTSD2.4\ntsd_release\ntsd_new.exe，长度 957,072 bytes，最后写入 2026-06-12 14:38:01。 |
| Executable identity | VERIFIED | SHA-256：9F2C56875F6ADC786C159D3483ABD596191D22405F46812D1A3CD286B5E92C5D。 |
| Makefile 静态 release target | VERIFIED | Makefile target 为 ntsd_new.exe，并列出 game_tick、frame advance、physics、collision、hit、weapon、cpoint、input、renderer 模块。 |
| Binary 内可见字符串 | VERIFIED（字符串存在） | ntsd_new.exe 含 NTSD_DEBUG_TICK、NTSD_RNG_SEED、NTSD_BATTLE_P1_OID、NTSD_BATTLE_P2_OID、NTSD_BATTLE_STAGE、diag_auto_result.txt。字符串存在不等于完整 trace 可用。 |
| 当前 source 的局部日志代码 | VERIFIED（source 静态事实） | game_tick.cpp 的 NTSD_DEBUG_TICK 通过 stderr 输出部分 phase / position 信息；多个 source 模块以相对路径 append 写入 diag_auto_result.txt 等诊断文件。 |
| 输入入口 | VERIFIED（source 静态事实） | main 忽略 argc/argv；主循环读取 SDL keyboard state 与 Windows GetAsyncKeyState。 |
| C++ runtime 实际只读运行 | UNKNOWN | 本次未启动 ntsd_new.exe，避免出现 authority 目录写入或不可控 GUI/输入副作用。 |
| 现有日志是否足以覆盖 R1 schema | UNKNOWN，当前判定为不足 | 未发现文档化通道能输出完整 tick/pass/slot/field/candidate/consume/lifecycle/render-handoff 合同。 |

## 3. Blocker

### B-R1-WP02-01 — R1 full-trace coverage 不足

已发现的 source/binary 线索只说明存在局部 stderr 与诊断文本输出：

- NTSD_DEBUG_TICK 的静态输出包括部分 phase marker、character position 和少量 frame 信息；
- diag_auto_result.txt 等诊断文件是专项/局部 carrier；
- 没有发现一个既有、文档化、可验证的输出接口能覆盖 R1 要求的完整 checkpoint、所有需要的 slot/field、candidate/consume、lifecycle 与 render handoff。

因此当前不能把任何已有局部日志包装成 cpp-release full trace。

### B-R1-WP02-02 — 固定逐 tick 输入不可验证

当前 main 入口忽略命令行参数，运行循环使用实时 SDL/物理键盘轮询。虽然 binary 字符串显示存在 seed 和 direct-battle OID/stage 环境变量，但尚未证明存在：

- 可记录的 held/pressed/released journal；
- 按 logic tick 精确应用的输入入口；
- 非交互、可重复、可自动退出的 release replay 模式。

不能用人工键盘时序或 UI 自动化猜测替代 R1 input journal 合同。

### B-R1-WP02-03 — authority 写入隔离不可证明

source 中已有大量相对路径的 append 写入，如 diag_auto_result.txt。C++ authority 根目录当前也已有该文件。

可以推测外部 working directory 也许能隔离相对写入，但尚未证明 runtime 资源加载在该模式下仍可工作，也未证明 binary 不会有其他写入。因此不能为了验证这个假设启动 runtime；这会违反“不向 authority 目录写入”的 fail-closed 规则。

### B-R1-WP02-04 — source / executable identity 未闭合

ntsd_new.exe 的最后写入时间是 2026-06-12 14:38:01；当前 src/core/main.cpp 的最后写入时间为 2026-06-12 14:45:58。没有找到把当前 source tree、Makefile 与该 executable 精确绑定的 build manifest。

这不证明 executable 行为错误，也不否定 executable 是行为 authority；但它阻止把当前 source 中每个静态日志分支直接宣称为该 executable 的已验证 trace capability。

## 4. 未执行的事项

- 未启动 ntsd_new.exe、ntsd_diag.exe 或 ntsd_diag_current.exe；
- 未改 C++ 文件、Makefile、可执行文件、资源、DAT、配置或日志；
- 未创建 C++ trace、非 authority trace、fixture、input journal 或 comparator；
- 未开始 Unity trace、R2 gameplay 修改、Play Mode、Unity build、C++ build 或性能测试；
- 未宣称发现了 C++ / Unity behavior difference。

## 5. 结果与恢复条件

当前 R1-WP02 的正确结果是 **BLOCKED**，不是“改用 instrumentation”。

恢复本 WP 前，需要有一个经用户确认的、已有且不修改 C++ Release 工程的方案，同时满足：

1. 可从实际 ntsd_new.exe 获取足以覆盖目标 R1 checkpoint 的只读输出，或用户明确缩小本 WP 的可接受证据域；
2. 所有 trace、stdout/stderr、run manifest 与比较资料都能写到非 authority 目录；
3. 能定义或提供可重复的运行/输入方式，至少能把 seed、初始状态和输入时序纳入可审计合同；
4. 能说明当前 executable 与可供静态映射的 release source 的身份关系，或明确改以 binary-only observation 为范围并相应调整 trace 合同。

在这些条件满足前，不得通过修改 C++、使用 debug executable 替代 release、向 authority 目录写日志或开始 Unity/comparator/R2 来绕过 blocker。

## 6. 下一次会话的起点

先阅读：

1. AGENTS.md；
2. docs/ai/STATE.md；
3. docs/ai/DECISIONS.md，尤其 D-006；
4. docs/ai/TASKS/R1-WP01-trace-contract-planning.md 的 R1-WP02；
5. 本 handoff。

然后只根据用户提供的现有只读采集条件重新判定 R1-WP02。未获得新的条件时，保持 BLOCKED。
