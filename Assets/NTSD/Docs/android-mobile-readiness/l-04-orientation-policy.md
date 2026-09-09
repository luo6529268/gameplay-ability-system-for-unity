# L-04 Android 屏幕方向策略方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / PRODUCT_DECISION_REQUIRED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前允许全部自动旋转方向，而战斗取景和触控很可能按横屏设计。方向切换可能触发布局重建和输入边沿异常。最终是只支持横屏还是允许动态旋转，需要产品决定。

## 解决方案

1. 推荐正式战斗先冻结为横屏左右方向；如只允许单侧横屏则显式记录原因。
2. 若支持旋转，切换期间暂停输入采集、清空触摸状态、重建 M-10 viewport/safe area 后再恢复。
3. 旋转不得改变逻辑 World、相机真值或 checksum。

## 验收条件

- PlayerSettings 与产品方向策略一致。
- 不支持的方向不会造成异常重建；支持的方向切换无粘键和布局错位。
- 方向变化前后相同逻辑输入产生相同 checksum。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 启动方向 | 四种持握方向启动 | 最终方向符合策略 |
| 战斗中旋转 | 多指按住时旋转 | 输入安全释放、布局正确 |
| 后台旋转 | 后台改变方向再返回 | 无旧触点和错误 viewport |
| 确定性 | 同输入不同方向 | Core checksum 一致 |

## 证据与留痕

- 当前证据：四方向 autorotation 均允许，`useOSAutorotation=1`。
- 记录用户产品决定、PlayerSettings、设备截图和输入 trace。
- 2026-09-06：方案建立；方向策略未确认。
