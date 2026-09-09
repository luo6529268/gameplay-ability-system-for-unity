# M-03 中央动态 Mesh 构建与上传优化方案

> 优先级：中  
> 状态：`OPEN / SOLUTION_DOCUMENTED / DEVICE_PROFILING_REQUIRED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

中央渲染消除了每实体 SpriteRenderer draw，但每个可见帧仍需解析命令、写 Quad、更新 submesh 并上传顶点。约 1000 主体可能对应约 3000 条表现命令，桌面 GPU 低耗时不能外推手机 driver 与带宽。

## 解决方案

1. 为 BuildCommands、ResolveCommands、WriteQuads、MeshUploadChunks、SetSubMesh、Execute 和 GPU 分别计时。
2. 保留持久、预分配 chunk；上传只覆盖有效范围，避免每帧扩容、LINQ、临时数组和整 buffer 清零。
3. 对不可见、未变化或无有效 binding 的命令做有证据的早期过滤，同时保持 hit-stop、排序和 first-visible tick。
4. 评估双/环形 buffer、Jobs 写顶点或更低层 API 前，先测主瓶颈并保持 canonical 路径 A/B。

## 验收条件

- warmup 后命令构建与 Mesh 上传 `0 B/frame` 或所有残余分配均有明确许可。
- 1000 visible 下无 buffer resize、capacity reject、stale submission 和未知 submesh 重建。
- CPU render stages 与 GPU P95 满足设备预算，优化前后像素/排序一致。
- 关闭、重进和 chunk 缩放无旧数据 ghost 或 Native 资源泄漏。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 微分段计时 | 100/500/1000 visible | 各阶段随规模曲线可解释 |
| 上传范围 | 部分可见、跨 chunk 边界 | 只上传有效数据，无越界/旧 Quad |
| 0GC | 1800 sampled frame | managed allocation 为 0 |
| 像素/排序 | actor/weapon/effect/shadow/health | 与基线一致 |
| Android GPU | Adreno/Mali × Vulkan/GLES3 | driver、CPU native transition、GPU 分离记录 |

## 证据与留痕

- 当前观察：有效帧仍执行命令解析、顶点写入、submesh 与 `SetVertexBufferData`。
- 保存 Profiler marker、chunk/command 计数、上传字节、设备/API 和 A/B 报告。
- 2026-09-06：方案建立；尚无 Android 热点结论。
