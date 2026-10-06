# M-02 中央渲染 Draw Segmentation 控制方案

> 优先级：中  
> 状态：`OPEN / DEVICE_MEASUREMENT_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

中央backend按资源/材质/binding mode/命令顺序及chunk、StrictOrderedDraw形成物理segment；Foot/health另提交。逻辑连续兼容run不等于物理segment，一个实体可有多命令，segment也不是每实体一个。每有效segment有中央DrawMesh命令，但CPU命令数、RenderPass/ExecuteCommandBuffer与真实GPU batch不同；SRP Batcher开关不是GPU合批证据。

## 解决方案

1. 分开记录逻辑run、物理segment/chunk、中央CommandBuffer.DrawMesh、生产RenderPass/ExecuteCommandBuffer、benchmark-local Graphics.DrawMesh、全帧Profiler draw及GPU batch/SetPass，后者由获批Frame Debugger/GPU capture等实际证据确认。
2. H-08 纹理 bank 在加载粒度与 draw segment 之间设预算：避免全量超级 atlas，也避免每角色一个资源。
3. 兼容key使用实际绑定身份：SourceTextureIdentity、AtlasPageIdentity、TextureArrayIdentity与独立slice，material/variant、shader/render state和binding mode；opaque key可还原tuple。仅排序后兼容且连续、不跨现有segment边界的复用可评估，不改变painter顺序。
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
- 2026-10-06：重扫`BattleDynamicMeshBackend.cs:213-224,429-440`、`BattleRenderFeature.cs:272-282,316`；GPU真实batch未测。EXT-1仍PROPOSED / MODIFY_REQUIRED，不预设draw减少、不跨chunk合并、不启动专项M0；活跃Q06排序内部仍待确认。
