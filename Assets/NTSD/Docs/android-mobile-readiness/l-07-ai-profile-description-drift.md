<!-- OPTIMIZATION-REVIEW-20261009 -->
# L-07 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 维护 / 低 |
| 当前状态 | 方案明确／未执行；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 核resolver实际空值语义；不依赖H07性能通过 |
| 下一步与具体方向 | 只同步tooltip/说明，不改resolver或asset；既有测试优先复用 |
| 完成边界 | 文案与实际解析一致，不宣称加速，不扩大为AI改造 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# L-07 AI Profile 说明漂移修正方案

> 优先级：低  
> 状态：`OPEN / SCRIPT_CHANGE_NOT_STARTED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

GameConfig tooltip 描述空值使用 `LegacyCanonical`，resolver 当前空值返回 `DataOrientedCanonical`，生产 asset 也显式使用后者。现状不改变当前 asset 行为，但会误导配置、测试和证据解释。

## 解决方案

1. 以 resolver 实际合同和当前生产配置为准，盘点 tooltip、文档、测试名和报告字段。
2. 独立脚本Change只同步tooltip/说明和报告，保持resolver实际默认不变；若要调整resolver另立行为合同并获批，不能把文案修复变成AI规则切换。
3. 加入空值、合法值、未知值的 resolver focused test，并在报告中写 resolved profile。

## 验收条件

- Inspector tooltip、resolver、生产 asset 和报告对默认/空值含义一致。
- 未修改用户未批准的 AI 决策行为、RNG 或 pass 顺序。
- 未知 profile 的 fail/fallback 行为有测试和明确诊断。

## 测试条件

| 测试 | 输入 | 通过标准 |
|---|---|---|
| 空值 | `""/null` | resolved 值与书面合同一致 |
| 正式值 | `DataOrientedCanonical` | 精确解析且报告一致 |
| 未知值 | 非法名称 | 明确 fail/fallback，无静默漂移 |
| 生产资产 | 当前 GameConfig | 行为未因文案修正改变 |

## 证据与留痕

- 当前证据：tooltip 与 resolver 空值语义不一致；生产 asset 显式为 `DataOrientedCanonical`。
- 脚本修改前必须建立 Change Record；保存 focused test 和实际 resolved profile。
- 2026-09-06：方案建立；未修改脚本。
