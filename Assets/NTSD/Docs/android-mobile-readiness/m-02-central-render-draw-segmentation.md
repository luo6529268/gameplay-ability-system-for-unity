## 2026-10-10 当前阶段更正：M-02

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| P2／当前后续评审 | 7 | 中 |

第7项：复用已有合批，核多资源segment剩余成本；不重复共享绑定优化。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-02 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 渲染 / 中 |
| 当前状态 | 共享绑定成果保留／设备开放；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | H-08 bank改变时需绑定合同；设备证据需H-02和M-07矩阵 |
| 下一步与具体方向 | 保留80同Sakura SetPass1995→5、CPU DrawMesh约2；多资源场景有证再优化 |
| 完成边界 | 透明顺序/segment边界/fail-closed不变；CPU命令不是GPU batch |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

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
