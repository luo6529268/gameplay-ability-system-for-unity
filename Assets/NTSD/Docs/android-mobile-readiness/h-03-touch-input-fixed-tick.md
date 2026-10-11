## 2026-10-10 当前阶段更正：H-03

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 高 |

待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-03 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 平台 / 中 |
| 当前状态 | 待产品与写域决策；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | L-04方向策略、M-10布局合同；设备验收需H-02可运行包 |
| 下一步与具体方向 | 复用现有Input Actions和FrameInputSet，处理多指/失焦释放；Prefab/Scene/Input Actions另批 |
| 完成边界 | 键盘/触屏同输入脚本边沿一致，组合技与取消行为通过；不是AI性能优化 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# H-03 Android 触屏输入接入固定 Tick 方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
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
