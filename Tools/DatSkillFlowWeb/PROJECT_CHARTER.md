# Project Charter

## Project Identity

- Name: NTSD DAT 技能流程编辑器
- Owner: Logan
- Created: 2026-08-06
- Current version: 0.7 Phase 7
- Scale mode: `Standard`

## Source Brief and Maturity

- Original user brief: “我想要其实类似一个技能编辑器，我不知道你能了解，目前太简陋了。很多也不清晰，按钮的高亮和按下反馈不明显，然后预览区域的展示也很差。我们一步步的完善这个技能编辑器。”
- Input maturity: `Developing`
- Gate status: `Ready`
- Recommended scale mode: `Standard`
- Minimum vertical slice: 用户从当前 DAT 自动派生的状态/技能入口中选择一项，查看其真实 DAT 帧流程和跨技能目标，在单角色场景中播放和定位当前帧，修改一个结构化字段，并清楚识别未保存状态。
- High-impact decisions still required: 无。
- User-authorized defaults:
  - 当前 DAT 的 frame 标题段和跳转关系定义入口与首帧，不凭空推断中文技能语义。
  - 第一阶段采用单角色场景预览。
  - 第一阶段先完成编辑器基础闭环，再接入全部现有 DAT 块编辑。
  - 第一阶段包含现有 DAT 块的结构化查看、字段编辑和几何叠加。
  - `.dat-skill-flow/skills.json` 只保存入口别名、分组、顺序、置顶、隐藏和备注。
  - GPT 视觉稿作为修正后的桌面视觉方向，不照搬虚构数据或未接线功能。

## Mission

### Problem

当前工具具备真实 DAT 打开、预览、编辑和保存能力，但界面仍是技术验证页：技能概念缺失、信息层级混乱、交互状态不明显、预览空间利用差，用户无法快速理解当前角色、技能、帧、播放状态和保存状态。

### Desired Outcome

将现有工具逐步演进为专业、清晰、可验证的 NTSD DAT 技能流程编辑器，使用户能围绕“技能”组织和理解帧流程，同时保留真实 DAT 字段与 `ntsd_cpp` 权威预览。

### Primary Users

- 维护和研究 NTSD DAT 的开发者。
- 需要查看技能帧流程、跳转关系和移动表现的内容编辑者。

### Success Definition

- 用户能在一个视图中明确识别当前角色、技能、DAT 帧、逻辑 Tick、修改状态和保存状态。
- 状态/技能入口和首帧由当前 DAT 定义；sidecar 只改变编辑器显示。
- 用户能选择技能、播放单角色预览、定位帧、编辑字段并安全保存。
- 按钮的默认、悬停、按下、选中、禁用和加载状态可辨识。
- 用户可见功能由当前构建的真实渲染和关键交互自动验证，至少达到 `E4 Interaction`。

## Interpretation Boundary

- Confirmed real requirements:
  - 产品形态是技能编辑器，而不是 DAT 技术演示页。
  - Native Trace 只是技能编辑器的预览支撑；网页不重建完整 C++ 游戏、AI、战斗或双角色系统。
  - 左侧需要显示当前 DAT 的全部状态/技能入口及其首帧。
  - 第一阶段采用单角色场景预览。
  - 第一阶段优先完成编辑器基础闭环。
  - 第一阶段纳入 `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 的查看、编辑和几何叠加。
  - sidecar 只保存显示信息，不能创建、删除或改变 DAT 入口。
  - `hit_*` 技能首帧是跳转后的目标帧；跨技能目标在当前 Flow 中显示为可点击叶节点。
  - 按钮反馈、信息清晰度和预览质量必须显著改善。
  - 一键启动时由用户选择仓库正式项目或 LocalAppData 测试副本。
- Examples only:
  - “螺旋丸”“替身术”等名称仅用于解释技能列表，不是已确认的 Naruto 技能数据。
- Recommended defaults:
  - 延续 NTSD 深色、金色主强调和青绿色辅助的专业工具风格。
  - 中文说明后保留 DAT 原始键名。
  - 技能流从自动入口沿真实 `next` 展开；跨入口 `hit_*` 显示目标但不吞并其后续流程。
- Assumptions:
  - DAT frame 标题是默认入口名称；中文别名等非权威信息使用独立 sidecar。
  - 双角色战斗仍不属于当前范围；Canvas 只直接编辑 DAT 中已存在且获得 capability 的几何字段。
- Unknowns:
  - 后续复杂运行语义需要哪些 `ntsd_cpp` 输出扩展。

### Native Trace boundary

- `ntsd_cpp` 是行为和数据结构权威；网页侧只复刻技能编辑器预览所需的最小可观察结果。
- Trace 的主体结束、opoint 生成和投射物尾迹服务于技能选择、帧定位、预览和编辑验证，不改变产品定位。
- 不把网页实现成 C++ 游戏引擎，也不在当前范围实现完整 AI、战斗系统或双角色模拟。

## Scope

### In Scope

- 专业编辑器信息架构与自适应布局。
- DAT 自动状态/技能入口列表，以及纯展示别名、分组、顺序、置顶、隐藏和备注编辑。
- 由真实 DAT 跳转关系构成的技能帧流程。
- 单角色场景预览、播放、暂停、单步、循环、缩放和适应窗口。
- 当前帧结构化属性编辑。
- `itr`、`bdy`、`opoint`、`wpoint`、`bpoint`、`cpoint` 的结构化查看、编辑和预览叠加。
- 明确的交互、加载、错误、脏状态和保存状态。
- 中文界面并保留 DAT 原始键名。
- 当前构建的自动化渲染与关键交互验证。
- 完整 frame/block CST span 的模板式新建、复制和删除。
- Canvas 几何 move/resize、1/4px 网格、键盘微调和 Esc 取消。
- SVG Flow 已有跳转字段重定向，以及按 `max(1, wait)` 展开的 DAT wait 视觉时间轴。
- 一键启动的正式/测试模式选择；正式模式使用仓库根 workspace，测试重置不影响仓库 Config。

### Out of Scope

- 脱离 DAT 标题、state 和跳转关系凭空猜测或生成技能语义。
- 通过 sidecar 创建、复制或删除 DAT 入口。
- 双角色完整战斗模拟。
- 创建 DAT 中缺失的字段、空白结构默认模板或自动引用修复。
- 修改 `ntsd_cpp` 权威战斗逻辑。
- 游戏主菜单、商城、联机或与 DAT 编辑无关的功能。

### Deferred Candidates

- 缺少完整 x/y 或 x/y/w/h capability 的 block 专用几何交互。
- 双角色交互预览。
- 技能模板、对比和批量检查。

## Context

- Existing code and assets:
  - `index.html`、`src/client/main.ts`、`src/client/styles.css`
  - `/api/project/*`、`/api/assets/*`
  - 真实 `data.txt`、DAT、BMP 和 `ntsd_cpp/dat_preview_cli.exe`
- Runtime or deployment environment:
  - Windows 10
  - Node.js 24
  - 本地回环服务使用启动器分配的随机端口
- External dependencies:
  - `ntsd_cpp` 原生预览程序。
  - 用户提供的 GPT 视觉概念图仅作为设计输入，不作为运行时依赖。
- Compatibility requirements:
  - 不改变 DAT 权威字段语义。
  - 不依赖 Unity 运行。
  - Native preview 当前仅支持 Naruto OID 2。

## System Boundaries

```text
NTSD DAT 技能流程编辑器
├── 技能工作区
│   ├── DAT 自动入口（标题段 + hit_* 目标）
│   ├── sidecar 纯展示覆盖
│   └── SVG DAT 帧关系、跨技能叶节点与已有边重定向
├── 预览工作区
│   ├── ntsd_cpp 权威 Tick
│   ├── 2D 精灵与镜头投影
│   └── capability 约束的 Canvas 几何交互
├── 属性检查器
│   ├── DAT 字段能力
│   ├── batch 会话内无损编辑
│   └── 完整 CST span 结构事务
├── DAT wait 视觉时间轴与交互状态
└── 本地安全服务
    ├── 项目会话
    ├── 资源 capability
    └── 安全保存
```

- 技能工作区输入 DAT 投影并派生入口，sidecar 只覆盖显示；不得让 sidecar 决定入口是否存在。
- 预览工作区输入 `ntsd_cpp` Tick 和 BMP capability，输出表现画面；不得写回逻辑真值。
- 属性检查器输入服务器字段 capability，输出会话内编辑；只有显式保存才覆盖 DAT。
- 本地服务拥有路径、会话、资源和持久化边界，浏览器不得获得绝对路径。

## Non-Functional Goals

- Performance: 交互反馈在一个渲染帧内可见；长操作必须显示加载状态。
- Reliability: 资源缺失、模块加载失败和会话失效必须在页面中明确显示。
- Security: 延续回环 Host、Origin、状态令牌、opaque capability 和安全覆盖协议。
- Observability: 当前构建、载入阶段、错误资源和保存状态可观察。
- Maintainability: 技能、预览、检查器、时间轴和服务合同保持明确边界。
- Compatibility: 保留现有 DAT 无损编辑、Native preview 和 API 安全合同。

## Current Phase

- Phase: 阶段 8，桌面三栏拖动调宽完成 E4 验收。
- Phase goal: 在保持 DAT 自动入口、lossless 事务、真实预览和移动标签页边界的前提下，让桌面状态/技能区、预览区和属性区可安全分配宽度。
- Completed evidence:
  - Native preview 以稳定 slot 0 识别主实体。
  - 草稿跨导航保留，重复提交受 busy 状态保护。
  - Preview 单飞并只保留最后 pending 请求。
  - 隔离 DAT 完成显式覆盖、恢复备份和服务重启 E5。
  - 技能复制/删除/排序、frame/block span 事务、Canvas、SVG Flow 和 DAT wait 轴在 release build 完成 E4/E5。
  - 一键启动的正式/测试/取消交互、两种 workspace 接线和重置隔离已完成 E4。
  - Naruto 无 sidecar 自动生成 86 个入口；standing 展开 0→1→2→3，F300 为可点击跨技能叶节点。
  - sidecar 别名安全保存并重启恢复，Naruto DAT SHA-256 保持不变；1440/1024/390 无水平溢出。
  - 1440/1024 桌面两条 separator 支持 pointer、键盘、Esc 和 resize clamp；390 移动标签页隐藏 separator，全部视口无水平溢出。
- Next phase: 等待用户对默认栏宽和当前会话调宽体验的实际使用反馈；当前 REQ-001 至 REQ-018 已验证。
- Forbidden changes: sidecar 创建或删除 DAT 入口；脱离 DAT 猜测技能语义；伪造复杂运行语义；绕过 lossless CST 或安全保存边界。
- Stop conditions:
  - 视觉稿与真实 DAT 数据或现有服务能力冲突。
  - 需要改变公共 API、保存格式或权威预览边界但尚未确认。
