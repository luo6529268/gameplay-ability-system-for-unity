# H-09 NTSD 2.8-Logan 正确性与移动证书绑定方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / ALIGNMENT_IN_PROGRESS`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前权威重新对齐仍在进行。性能达标只能证明某个具体构建在某个 workload 上足够快，不能证明 pass 顺序、AI、碰撞、命中、生命周期或内容已与当前 NTSD 2.8-Logan 权威一致。

## 解决方案

1. 所有移动证书绑定正式 EXE SHA-256、playable closure、Unity 内容 Manifest、源码/dirty manifest 和 APK SHA-256。
2. 对具名场景生成固定 seed、初始状态和逐 tick 输入；从当前权威获取可复核 trace/checksum，Unity/Android 消费同一输入并报告 first difference。
3. 发布结论分开记录：Android 可构建、普通战斗可玩、1000 AI 性能通过、非例外战斗域对齐，不相互替代。
4. 任何影响 tick、AI、碰撞、OPoint、presentation command 或内容闭包的改动按域使相关证书失效并触发重测。

## 实施步骤

1. 继续以 `docs/ai/CURRENT-AUTHORITY.md` 和新对齐总表为恢复入口。
2. 为每个差异建立具名输入、权威调用链、字段与 first-difference 证据。
3. 建立跨 Editor/Player/Android 的 replay/checksum runner。
4. 只有权威、自动检查和定向运行时证据齐备时才更新对齐状态。

## 验收条件

- 报告明确列出权威 EXE 与源码闭包哈希，不使用 NTSD 2.4 或旧 2.8 证书裁决当前行为。
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

- 当前权威入口：`docs/ai/CURRENT-AUTHORITY.md`；当前状态包含 `B3_ALIGNMENT_IN_PROGRESS`。
- 每次更新记录差异 ID、权威函数、Unity 符号、输入、first difference、验证命令和实际结果。
- 2026-09-06：方案文档建立；对齐工作仍在进行，本项未关闭。
