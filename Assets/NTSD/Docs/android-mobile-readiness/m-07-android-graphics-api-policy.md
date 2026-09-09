# M-07 Android 图形 API 策略方案

> 优先级：中  
> 状态：`OPEN / SOLUTION_DOCUMENTED / API_POLICY_NOT_FROZEN`  
> 最后更新：2026-09-06  
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
