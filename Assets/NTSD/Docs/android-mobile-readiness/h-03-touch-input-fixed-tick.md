# H-03 Android 触屏输入接入固定 Tick 方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

正式 Input Actions 当前只观察到键盘绑定，没有 NTSD 触屏控件进入 `FrameInputSet` 的证据。触屏 UI 不得直接移动 Transform、写角色状态或调用招式，否则会破坏固定 tick、回放和未来 Lockstep 边界。

## 解决方案

1. 建立正式横屏触控 Prefab：方向、Attack、Jump、Defend、暂停及必要功能键。
2. 触控控件只投影到现有 Input System actions；由统一采集器在逻辑 tick 边界生成按下、按住、释放状态。
3. 保持键盘、手柄和触屏共用同一 `FrameInputSet` 语义，不能为手机另写直接技能路径。
4. 加入多点触控、手指滑出、失焦、后台恢复、设备断开和布局重建时的安全释放。
5. 布局使用 M-10 的 Safe Area/底部黑区合同，不反写逻辑坐标。

## 实施步骤

1. 冻结动作表和每个动作的逻辑边沿语义。
2. 建立触控布局与输入适配器，先做输入记录/回放对照。
3. 接入正式 Scene，并加入失焦和取消清空事务。
4. 在多种宽高比真机完成组合键和后台恢复验收。

## 验收条件

- 无外设时可完成移动、跳跃、防御、普通攻击和至少两条正式组合技。
- 触屏与键盘对同一动作脚本生成相同的逐 tick 输入序列。
- 多指同时操作无粘键、漏释放、跨 tick 重复按下或直接状态写入。
- 切后台、来电模拟、焦点丢失和 UI 重建后所有虚拟键回到安全释放状态。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 输入映射 | 每个触控控件逐一操作 | action、player 与 bit 映射唯一正确 |
| 回放对照 | 键盘和触屏执行同一脚本 | `FrameInputSet` 与 checksum 一致 |
| 多点触控 | 方向+攻击、方向+跳跃+防御 | 所有边沿按 tick 正确出现 |
| 取消边界 | 滑出按钮、抬指、失焦、后台恢复 | 无 stuck input，下一 tick 安全清空 |
| 分辨率矩阵 | 16:9、19.5:9、20:9、平板、cutout | 控件可达且不遮挡关键画面 |

## 证据与留痕

- 当前证据：`Assets/NTSD/Config/InputConfig/NTSDInputConfig.inputactions` 仅观察到 Keyboard bindings。
- 实施时保存：动作表版本、逐 tick 输入 trace、设备/触点记录、组合技步骤和后台恢复日志。
- 2026-09-06：方案文档建立；实现未开始。
