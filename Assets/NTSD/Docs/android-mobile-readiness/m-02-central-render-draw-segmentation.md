# M-02 中央渲染 Draw Segmentation 控制方案

> 优先级：中  
> 状态：`OPEN / SOLUTION_DOCUMENTED / DEVICE_MEASUREMENT_REQUIRED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

中央渲染按资源、材质、binding mode、atlas page 与 chunk 形成 segment；FootSelf 和血条另行提交。目标不是虚构“全战斗固定一个 draw”，而是让 segment 数量有界且与资源分包、GPU 时间相匹配。

## 解决方案

1. 统一统计 source/resolved command、resource segment、chunk、submission draw、SetPass 与 unresolved。
2. H-08 纹理 bank 在加载粒度与 draw segment 之间设预算：避免全量超级 atlas，也避免每角色一个资源。
3. 合并相同 shader/material variant，减少不必要关键字和材质实例；保持 TextureArray 正常路径、OrderedPages 回退。
4. FootSelf/血条是否合批以真机 GPU/带宽证据决定，不牺牲排序和可见规则。

## 验收条件

- 所有 draw/segment 能追溯到资源或材质原因，无未知拆批。
- 常见战斗与 1000 visible workload 的 segment/SetPass 在明确预算内。
- 优化前后 actor、weapon、effect、shadow、FootSelf、health 排序与像素一致。
- Android GPU P95 建议 `<8 ms`，且没有为了低 draw 增加不可接受的纹理常驻。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Segment 归因 | 不同 roster/资源 bank | 每个拆分原因可报告 |
| Array/Pages A/B | 相同画面 | 像素一致，draw 与 GPU 差异完整 |
| 材质变体 | 开关正式关键字组合 | 无隐式实例化和异常 SetPass |
| 1000 visible | Foot/health on/off | draw、SetPass、GPU、内存均有分位数 |
| 排序回归 | 重叠角色/武器/阴影 | 无层级反转或闪烁 |

## 证据与留痕

- 当前证据：`BattleDynamicMeshBackend.cs:131-343`、`BattleRenderFeature.cs:248-299`。
- 保存命令/segment/chunk/draw 对照表、设备/API 和纹理常驻报告。
- 2026-09-06：方案建立；没有设定“必须 1 draw”的错误验收门。
