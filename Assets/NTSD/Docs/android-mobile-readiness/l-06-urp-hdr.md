# L-06 URP HDR 移动端评估方案

> 优先级：低  
> 状态：`OPEN / SOLUTION_DOCUMENTED / A_B_REQUIRED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前 URP HDR 开启，但尚无证据证明战斗画面需要 HDR，也没有 Android 带宽、RenderTarget 内存和 GPU A/B。不能因为“移动端通常关闭 HDR”就直接改变正式表现。

## 解决方案

1. 建立 HDR on/off 的独立渲染 Profile，保持其他参数、相机、分辨率和 workload 相同。
2. 比较中央 actor、特效、透明混合、背景、颜色和截图；记录 RenderTarget 格式、内存和 GPU。
3. 若画面无必要差异且目标设备持续受益，再独立批准关闭；否则保留并记录成本。

## 验收条件

- HDR on/off 画面差异有截图或像素证据，不凭主观描述。
- CPU/GPU、RenderTarget 内存、带宽和热稳态差异完整。
- 最终选择在 Adreno/Mali 的正式场景无颜色、透明或后处理回归。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 像素 A/B | 固定相机与帧 | 差异被量化和解释 |
| 特效覆盖 | 透明/叠加/高亮场景 | 无剪裁、色带或混合异常 |
| 内存/GPU | 1000 visible/普通战斗 | RenderTarget 和 GPU 数据完整 |
| 热稳态 | 目标设备持续运行 | 选择不会后期回退 |

## 证据与留痕

- 当前证据：URP `m_SupportsHDR=1`、MSAA=1、Render Scale=1。
- 保存 URP Profile、截图/像素 diff、RenderTarget 格式、内存与 GPU 报告。
- 2026-09-06：方案建立；尚未决定关闭 HDR。
