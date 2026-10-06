# H-09 NTSD 2.8-Logan 正确性与移动证书绑定方案

> 优先级：高  
> 状态：`OPEN / SCOPED_CLOSURE_ACCEPTED / OPTIMIZATION_REGRESSION_GATE_PENDING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前336B44目标已获用户接受限定收尾（USER_ACCEPTED_SCOPED_CLOSURE），不等于全角色/全模式/全设备一致。本项是后续优化正确性及证书绑定门，不自动重启旧全量对齐。性能达标不能替代受影响pass、RNG、命中、生命周期、声音及表现合同回归。

## 解决方案

1. 绑定正式EXE完整SHA、已核对的冻结源码/trace来源、Unity内容、Client与Kernel dirty manifest、APK和运行配置。当前可变源码与正式EXE逐字节对应未确认时明确标待确认，不能用候选EXE或开发源码替代336B44权威。
2. 对具名场景生成固定 seed、初始状态和逐 tick 输入；从当前权威获取可复核 trace/checksum，Unity/Android 消费同一输入并报告 first difference。
3. 发布结论分开记录：Android 可构建、普通战斗可玩、1000 AI 性能通过、非例外战斗域对齐，不相互替代。
4. 任何影响 tick、AI、碰撞、OPoint、presentation command 或内容闭包的改动按域使相关证书失效并触发重测。

## 实施步骤

1. 继续以 `docs/ai/CURRENT-AUTHORITY.md` 和新对齐总表为恢复入口。
2. 为本批实际受影响链路建立具名输入、权威调用链/冻结来源、字段和first-difference；不把历史开放Record自动变成本批全部必跑任务。
3. 建立跨 Editor/Player/Android 的 replay/checksum runner。
4. 只有权威、自动检查和定向运行时证据齐备时才更新对齐状态。

## 验收条件

- 报告列出正式336B44身份、固定来源和未确认范围；不使用旧2.4/旧EXE/候选或未经核对的可变源码裁决当前行为。
- 同 seed/input/tick 的强一致域没有未解释 first difference。
- 用户批准例外与未覆盖域被明确列出，不宣称整个应用逐像素完全一致。
- Android 性能报告可以追溯到具体对齐状态；后续规则改动能准确标记受影响证书。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Authority identity | 每次 trace/证书生成 | EXE、closure、内容哈希完全匹配当前权威 |
| Golden replay | 固定 seed/input/tick | checksum 和关键事件无 first difference |
| 定向 Play | 用户报告的角色/按键/场景 | 可观察结果符合当前权威 |
| Android replay | 相同输入运行 ARM64 Player | 强一致域无平台分叉 |
| 失效传播 | 修改一个受管域指纹 | 对应证书被标为 stale，不能继续宣称通过 |

## 证据与留痕

- 2026-10-06入口：`docs/ai/CURRENT-AUTHORITY.md:33,39`。用户接受限定收尾与源码对应边界分别记录；旧B3状态不自动恢复执行。音频等用户另指定的局部来源保持原任务范围，不扩展为新全局权威。
- 每次更新记录差异 ID、权威函数、Unity 符号、输入、first difference、验证命令和实际结果。
- 2026-09-06：方案文档建立；对齐工作仍在进行，本项未关闭。
- 2026-10-06：更正当前操作基线；本项保持优化回归门OPEN，不因文档整理声称全域对齐或Android证书通过。
