# L-07 AI Profile 说明漂移修正方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / SCRIPT_CHANGE_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

GameConfig tooltip 描述空值使用 `LegacyCanonical`，resolver 当前空值返回 `DataOrientedCanonical`，生产 asset 也显式使用后者。现状不改变当前 asset 行为，但会误导配置、测试和证据解释。

## 解决方案

1. 以 resolver 实际合同和当前生产配置为准，盘点 tooltip、文档、测试名和报告字段。
2. 在独立脚本 Change 中更新说明或明确调整 resolver；不能把文案修复顺手变成 AI 行为变更。
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
