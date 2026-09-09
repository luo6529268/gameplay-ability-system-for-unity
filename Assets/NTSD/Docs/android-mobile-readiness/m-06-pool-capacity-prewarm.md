# M-06 对象池容量与预热方案

> 优先级：中  
> 状态：`OPEN / SOLUTION_DOCUMENTED / CAPACITY_PROFILE_REQUIRED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前 GameConfig 初始池容量较小，而 MobileExtended 允许 1000 active。盲目在启动时预建 1000 个完整 GameObject 也可能制造内存峰值，因此容量必须按“逻辑 slot、可表现对象、临时特效”分别规划。

## 解决方案

1. 从 roster、最大 active、OPoint/分身/召唤峰值和表现策略生成 `BattleCapacityPlan`。
2. 在 allocation seal 前、加载界面内分批预热必要 pool，记录耗时和内存；不需要表现的 logic-only entity 不预建 GameObject。
3. 建立 hard/soft capacity、拒绝 reason、增长策略和峰值统计；战斗中禁止无界 Instantiate 洪峰。
4. 与 H-08 资源 Lease 协同，资源准备完成后再创建依赖该资源的表现对象。

## 验收条件

- 目标 workload 开始前完成所需预热，allocation seal 后无意外扩容。
- Combat1000/OPoint burst 无 critical reject、无突发大批 Instantiate、无超预算启动峰值。
- 实际峰值与 CapacityPlan 可追溯，过量预热不会长期占用不可接受内存。
- shutdown 后 pool quiesced、active borrower=0、旧 World/Entity 绑定=0。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Plan 计算 | 多种 roster/OPoint 闭包 | 容量确定、可解释、无负值/溢出 |
| 分批预热 | 低内存 Android | 峰值和单帧耗时在预算内 |
| Seal 后运行 | Combat1000 | 无扩容与 critical reject |
| 峰值生成 | 分身/召唤/OPoint burst | 回收正确，无无界创建 |
| 关闭 | 正常/中途取消/错误退出 | pool quiesced，borrower 全归零 |

## 证据与留痕

- 当前证据：`PoolInitialSize=10`、`PoolMaxSize=200`，而 MobileExtended 允许 1000 active。
- 保存 CapacityPlan、预热时长/内存、动态创建计数、拒绝计数和 teardown 诊断。
- 2026-09-06：方案建立；正式容量 Profile 尚未冻结。
