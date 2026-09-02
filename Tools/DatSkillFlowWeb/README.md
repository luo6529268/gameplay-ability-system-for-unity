# Naruto DAT 技能流程播放器（网页原型）

> **NTSD24_AUTHORITY_SUPERSEDED（2026-09-02）：** 本文仍保留 NTSD 2.4 旧权威路径，仅作为历史证据；不得据此定义当前战斗规则、pass、timing、slot、RNG、字段、生命周期、表现或“已对齐”状态。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前权威是 NTSD 2.8-Logan 正式 EXE 及其对应 playable 源码，旧结论一律 `REBASELINE_REQUIRED`。

在仓库根目录启动静态 HTTP 服务：

```powershell
python -m http.server 8765
```

打开 `http://127.0.0.1:8765/Tools/DatSkillFlowWeb/`。

页面读取真实 `naruto.dat`、`data.txt` 和对应 BMP sheet。右侧编辑器只修改浏览器内存草稿，不会写入或覆盖 DAT 文件。

## 权威 C# Trace（默认）

页面默认读取 `data/authority-trace.json`。该文件由同目录 `AuthorityTraceExporter` 调用
`J:\QQFile\NTSD2.4\ntsd_release_C#` 的 `SimulationTickDriver` 生成；每个 snapshot 都包含完整 tick 后的
实体、绝对 `X/Y/Z`、`CameraX`、帧号、朝向和 checksum。网页只负责按 OID/DAT/BMP 渲染，不在浏览器中复刻
AI、碰撞或随机数。因此 tick 8 的一个、tick 13 的两个 `oid 33` 都来自实际 C# 场景，而非循环累计。

Trace 模式的“游戏镜头”直接读取 snapshot 的 runtime `CameraX`；“编辑器跟随”仅改显示用 `viewerCameraX`。
Trace 是只读回放，右侧 DAT 草稿编辑已禁用。切换到“DAT 草稿模拟（近似）”才可修改草稿；该修改不会重写既有
权威 Trace，重新导出 Trace 后才可得到新的权威效果。

## 当前 DAT 草稿模拟范围

- 固定 30Hz，按“输入/AI → frame 速度 → Physics → wait/next → opoint”推进。
- 主角和子对象统一保存 DAT world 坐标 `x/y/z`、整数同步值 `xInt/yInt/zInt` 以及 `vx/vy/vz`。
- 支持 `dvx/dvy/dvz`、重力、落地、`next`、负 `next` 翻向、`opoint` 和递归子对象。
- `opoint` 使用父对象的整数 X/Y 与 `Z + 1`。只有 `facing > 10` 的权威多生成规则才会应用 `-5..5` 的 Vx/Vz spread；普通 Naruto clone 不会被显示层人为错开。
- Canvas 内部尺寸固定为权威视口 `794 × 550`。默认“游戏 1:1”投影遵循 Host renderer：`screenX = XInt + RenderOffsetX - CameraX + frameDelayJitter`、`screenY = ZInt + YInt`、`groundY = ZInt`。`frameDelay < 0` 时 jitter 是 `6 * (GameTick & 1) - 3`；该模式不减 `cameraZ`。
- “深度增强”是非 1:1 的编辑器展示：它归一化 stage Z 并放大 Z 的屏幕深度；world 坐标和逻辑完全不变。

## 镜头模式

- 权威 Trace 的“游戏镜头”直接读取 C# runtime snapshot 的 `CameraX`。DAT 草稿的“游戏镜头”按 `GameTick.UpdateCameraAndBgAnimation` 的候选、state 14 特例、朝向补偿、C# 整数截断、`/14` 与 `/7` 平滑推进；但尚未实现 `CameraMaxOverride`，因此草稿镜头仍是近似，不可作为权威战斗结果。
- 游戏镜头必须依赖战场宽度。页面默认的 `1600` 只是尚未加载 `stage.dat` 时的**预览场宽**，不是当前关卡的权威宽度。
- “编辑器跟随”维护独立的 `viewerCameraX`，只把主角保持在画面中央；它不会回写主角、分身或任何 world 坐标。
- 循环基线、重置、跳帧和撤销重建都会恢复/重置游戏镜头速度、游戏 CameraX、编辑器 CameraX 和显示用 CameraZ，避免上一轮镜头状态泄漏。

## Naruto 最小 AI 测试子集

固定目标 AI **默认关闭**。它只用于验证分身生成后的 Z 运动，是一个明确受限的近似子集：

- 仅对 `opoint` 生成的 `type: 0` 子角色启用，team 继承生成者。
- 页面上的测试目标视为一个启用中的普通地面 team 1 目标，使用可编辑的 X/Z；不赋予特殊 state。
- 按权威普通目标阈值生成方向输入：X 为 `±60`，Z 为 `±3`。
- 仅实现 state 0/1 的 walk 与 state 2 的 running 子集，包括 DAT 的
  `walking_speed`、`walking_speedz`、`walking_frame_rate`、
  `running_speed`、`running_speedz`、`running_frame_rate`。
- state 2 在普通目标方向输入前按朝向预写反向键，保留双键触发 frame 218 转向的权威语义。
- AI 关闭时，子角色显示“无目标”，不会继续生成方向输入。
- state 3 的分身脚本阶段永远不会消费这个固定目标输入；frame 302/304 生成的 action 295/285 会先按各自 DAT `next` 脚本走完，回到可走路状态后才可能进入这个近似 AI。

这不是完整 `InputRuntime`。目前没有实现目标选择、随机决策、攻击决策、命中、碰撞、抓取和完整 holder/link/target 交互。测试目标只用于复现“分身出生后按方向输入走向不同 X/Z”的近似场景，**不能代表实际游戏中的分身站位**。默认关闭时，单次 Naruto Uj 在 frame 302/304 各生成一个 oid 33，先显示 DAT 脚本自身的两分身阵型；循环恢复会清除上一轮全部子实体，不累计残留。

## 调试入口

```js
window.__datSkillDebug.snapshot()
window.__datSkillDebug.step(10)
window.__datSkillDebug.jump(300)
window.__datSkillDebug.reset()
window.__datSkillDebug.verify()
```

`verify()` 检查游戏整数投影、`opoint Z + 1`、普通目标 X `±60` 阈值、running 反向键预写，以及目标 Z 差产生 Up/Down 与对应 Vz 符号的核心不变量。
