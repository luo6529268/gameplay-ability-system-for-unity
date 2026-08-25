# NTSD 2.4.1 只读渲染帧率对比入口

## 当前实施状态

- 已实现：独立 `render-cadence.html`、30/60/120 三栏 sampler、`--read-only` server policy 与专用启动宏；
- 已验证：build、focused tests、真实 OID 2 Native `open → 16-tick preview → close`，以及 mutation 的 `403/read-only-mode` 拦截；
- 待验证：浏览器 Canvas 人工视觉验收。应选取有显著位置变化的技能；本页不能替代 C++ release gameplay authority trace。

## 目的

这是一个独立于 DAT 编辑器的 presentation diagnostic。它让同一份 Native skill trace 同时以 30、60、120 Hz 的渲染采样策略绘制，以观察角色、武器、投射物、阴影和镜头的**纯视觉位置**平滑差异。

它不修改 DAT，不模拟技能，不创建第二份战斗逻辑，也不把浏览器结果写回 Native trace。

## 数据流

```text
选择角色 + 技能
        │
        ▼
现有 NTSD 2.4.1 Native preview（一次）
        │
        ▼
同一份 nativeTicks + render resources
        │
 ┌──────┼──────┐
 ▼      ▼      ▼
30 Hz  60 Hz  120 Hz presentation sampler
 │      │      │
 ▼      ▼      ▼
Canvas  Canvas  Canvas
```

## 离散与可插值字段

| 分类 | 字段 / 行为 |
|---|---|
| 始终离散 | `frame`、`pic`、facing、DAT `wait`、state、HP、hit-stop、holder/link/target、命中、opoint、spawn/despawn |
| 仅 presentation 插值 | 同 lineage 的 `x/y/z`、`displayZ`、`renderOffsetX`、`cameraX` |
| 不可跨 tick 推测 | 未出现的对象、下一逻辑 tick 的技能状态、未确认的碰撞结果 |

## 延迟模型

为了与未来实时 Unity 表现层一致，60/120 Hz 不预测下一逻辑 snapshot；它们使用一 tick 延迟，在前一与当前逻辑 snapshot 之间插值。30 Hz 直接显示当前离散 snapshot。

例如前一 snapshot 为 `X=100`，当前 snapshot 为 `X=112`：

```text
30 Hz：100 → 112
60 Hz：100 → 106 → 112
120 Hz：100 → 103 → 106 → 109 → 112
```

Sprite 姿势仍在逻辑 tick 边界离散切换，不能由浏览器显示刷新推进。

## 入口隔离

- `index.html`：原 DAT 编辑器，默认不改变；
- `render-cadence.html`：只读播放页；
- `一键启动-渲染帧率对比.cmd`：启动只读 server 并直接打开新入口；
- `--read-only`：服务器仍允许 catalog/open/preview/assets，但拒绝所有编辑、保存、sidecar 与 workspace 写路由。

## 证据限制

当前 Native preview provider 的来源为 `ntsd_cpp` 与 `J:\QQFile\NTSD 2.4.1` 资源根。该页面是当前工具链的 presentation diagnostic，不能代替 `J:\QQFile\NTSD2.4\ntsd_release` 的正式 gameplay authority trace。
