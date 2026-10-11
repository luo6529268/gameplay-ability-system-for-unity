## 2026-10-10 当前阶段更正：M-07

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| 后期平台适配与发布 | 不单独排队 | 中 |

待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-07 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 平台 / 后续 |
| 当前状态 | 设备策略待决定；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-02包可生成初始API矩阵；最终策略消费H-05功能数据 |
| 下一步与具体方向 | 先候选Vulkan/GLES矩阵后定策略；Automatic并非已证错误 |
| 完成边界 | 实际API/能力/回退可追溯，最终策略有设备证据 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# M-07 Android 图形 API 策略方案

> 优先级：中  
> 状态：`OPEN / API_POLICY_NOT_FROZEN / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

Android Graphics API 当前为 Automatic，Texture2DArray shader 使用 target 3.5，但没有 Vulkan/GLES3 的设备 allowlist、denylist 或独立报告。Automatic 不是错误，缺少认证和降级合同才是风险。

## 解决方案

1. 建立 Vulkan 与 GLES3 的独立 Build/Runtime Profile，显式记录实际 API。
2. 启动时探测 TextureArray、format、CopyTexture 与关键能力；选择预认证的 Array 或 OrderedPages 路径。
3. 通过 H-05 设备矩阵比较功能、首次渲染、GPU/CPU、内存、热性能和 driver 异常。
4. 证据完成后决定保留 Automatic、固定优先顺序或维护设备降级表；未知设备保持可诊断策略。

## 验收条件

- 报告记录实际图形 API、GPU 和 driver，不只记录 ProjectSettings。
- Vulkan/GLES3 至少各完成一轮中央 actor/weapon/effect/shadow/health 正式场景。
- 能力不足时 OrderedPages 正常回退或明确 fail-close，无静默不可见。
- 最终策略有版本、设备证据和回滚方式。

## 测试条件

| 测试 | 组合 | 通过标准 |
|---|---|---|
| 功能矩阵 | Vulkan/GLES3 × Adreno/Mali | 正常战斗和生命周期通过 |
| 能力探测 | Array on/off、format 支持变化 | 选择结果与能力一致 |
| 首帧 | 清数据首次进入中央战斗 | 无 shader/texture 故障和长时间黑屏 |
| 性能 | 同 APK 内容与 workload | CPU/GPU/内存差异可比较 |
| 回退 | 强制不支持 Array | Pages 正常且诊断完整 |

## 证据与留痕

- 当前事实：Graphics API 为 Automatic，尚无正式 API/设备策略。
- 保存 PlayerSettings、实际 API、capability snapshot、设备/driver 和 A/B 报告。
- 2026-09-06：方案建立；未决定固定 Vulkan 或 GLES3。
