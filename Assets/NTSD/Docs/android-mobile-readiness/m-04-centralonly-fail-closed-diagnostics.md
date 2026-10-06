# M-04 CentralOnly Fail-Closed 诊断方案

> 优先级：中  
> 状态：`OPEN / DIAGNOSTICS_EXIST / DEVICE_FAULT_MATRIX_PENDING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

CentralOnly失败不无条件回Legacy，避免双像素owner。当前已有CommitCentralFailurePlan、failure reason、资源未解析及首错诊断；不能再写成完全没有诊断。剩余是设备端完整故障矩阵、计数覆盖和有界导出闭环，不是放宽fail-closed或静默双渲染。

## 解决方案

1. 先盘点并复用现有reason/首错字段，对catalog/pack/binding/generation/stale/capacity/shader/API缺口补最小字段，不新建重复诊断系统。
2. 首次失败保存 tick、handle/generation、visualDataId/pic、resource key、atlas mode、device/API 和调用阶段。
3. Development Player 提供有界汇总与可导出报告；Release 避免日志洪泛但保留计数和首错。
4. 加载阶段在允许开始战斗前验证依赖闭包；运行中异常保持逻辑与表现所有权一致。

## 验收条件

- 人为制造每类资源/提交错误时均能得到稳定、唯一、可操作的 reason code。
- 正常 Android 战斗 unresolved/unsupported/stale command 为 0。
- 失败时不激活 Legacy Renderer 造成双画面，也不无诊断地隐藏角色。
- 日志和诊断缓冲有容量上限，1000 实体故障不会产生无界 GC/IO。

## 测试条件

| 测试 | 注入故障 | 通过标准 |
|---|---|---|
| Catalog miss | 删除一个测试 key | 首错包含 OID/pic/key |
| Pack unavailable | 模拟异步包失败 | 加载门阻断且可重试/取消 |
| Stale generation | 复用 slot 后提交旧命令 | 命令拒绝，不影响新 occupant |
| API fallback | 禁用 TextureArray 能力 | OrderedPages 正常或明确 fail-close |
| 1000 故障洪峰 | 大量相同错误 | 聚合计数、0 日志洪泛、无额外 GC |

## 证据与留痕

- 2026-10-06静态证据：`Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs`的CommitCentralFailurePlan调用及runtime diagnostics；已有reason不等于所有设备错误闭环通过。
- 保存 reason schema、故障注入报告、首错 payload 和正常路径零计数证据。
- 2026-09-06：方案建立；诊断闭环未实现。
- 2026-10-06：更正已有诊断进展，保持DEVICE_FAULT_MATRIX_PENDING；本轮未故障注入/运行Unity。
