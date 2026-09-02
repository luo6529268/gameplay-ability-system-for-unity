# CLAUDE.md

> **CURRENT AUTHORITY / 2026-09-02：** 本文件的任何旧规则若与根 `AGENTS.md` 或
> `docs/ai/CURRENT-AUTHORITY.md` 冲突，以后二者为准。NTSD 2.4 反汇编、伪 C、旧游戏目录、
> `ntsd_new.exe`、旧 `game_tick(...)`、固定 30 Hz、Authority400 和旧对齐结论全部是
> `NTSD24_AUTHORITY_SUPERSEDED / REBASELINE_REQUIRED` 历史材料，不能驱动当前实现。

## Project Goal（项目目标）

本项目的目标是：

- 复刻 NTSD（Naruto The Setting Dawn）游戏的核心逻辑与行为表现
- 通过对照分析，还原 NTSD 的真实实现方式：
  1. NTSD 2.8-Logan 正式发行 EXE 的实际可观察行为（**唯一最高行为权威**）
  2. 与正式 EXE 对应并进入 playable 构建闭包的 C++ 源码
  3. 正式启动参数和被正式 EXE 消费的 runtime 资源
- 推导并重建 NTSD 的：
  - 输入系统
  - 技能系统
  - 状态机
  - 动画驱动
  - 战斗与判定逻辑

在回答任何问题时，应始终以 **"严格复刻 NTSD 实际行为"** 作为最高优先级目标。

---

## Core Authority Principle（最高优先级原则）

**当前权威来源（唯一恢复入口：`docs/ai/CURRENT-AUTHORITY.md`）**：

1. `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\NTSD2.8-Logan.exe`
   的正式发行行为；正式身份由 SHA-256
   `1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75` 固定。
2. `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\source\README_SOURCE.md`
   声明对应当前发行 EXE、且实际进入 playable 构建闭包的 C++ 源码。
3. 正式启动参数及同一根目录的 `resources\runtime`。

NTSD 2.4 反汇编、伪 C、旧 release/C#、原始目录和旧 trace 只可用于历史比较、命名线索或
回归夹具。它们不是当前 authority；与 NTSD 2.8-Logan 冲突时必须舍弃旧结论。正式内容数值
权威仍暂按根 `AGENTS.md` 的 Direction B 合同处理，本次权威迁移不自动覆盖 Unity Config。

**对齐要求**：
- **能对齐的直接对齐**：逻辑、常量、字段读取顺序等，尽量与当前 playable live source 和正式 EXE 一致。
- **框架限制无法对齐时，只要求最终结果一致**：Unity 使用继承/组件/异步等不同于 C 结构体的架构，实现方式可以不同，但**运行时行为必须与反汇编等价**。
- **不得引用 FLF 或任何第三方项目作为依据**：任何逻辑来源必须能在当前权威 build closure 和正式行为中闭环，否则标注"待确认"，不得实现。

若无法在当前权威中确认逻辑来源，**必须暂停实现并说明原因**，不得猜测或创造逻辑。

---

## Project Semantic Map（项目语义映射）

### NTSD 2.4 反汇编 / 反编译文本（历史材料）
- 路径：
  - `J:\QQFile\NTSD2.4\ntsd24_full_disasm.txt`
  - `J:\QQFile\NTSD2.4\ntsd24_pseudoc.txt`
- 含义：从旧 `NTSD.exe` 中导出的反汇编与伪C文本，只用于历史比较；不能验证当前 NTSD 2.8-Logan 的正式运行逻辑
- 分析规则：
  - 仅基于文本进行静态分析
  - 不假设函数名、变量名一定正确
  - 细节以全反汇编为准，伪C仅作快速定位

### NTSD 2.4 游戏目录（历史材料）
- 路径：`J:\QQFile\NTSD 2.4.1 工具人亲测能玩/`
- 含义：旧版本实际运行所使用的目录结构与数据文本；不是当前 gameplay authority，也不自动改变当前 Direction B 内容权威
- 分析规则：
  - 优先理解目录组织方式、数据文本含义、配置与行为的映射关系
  - 跳过图片资源、视频、音频
  - 目标是理解数据如何驱动游戏行为，以及数据是否与反汇编逻辑对应

---

## Output Rules（分析结果输出规则）

- 当进行阶段性分析、模块分析时，默认输出为 **Markdown 文件**
- 默认输出目录：`I:\C++Test\NTSD\`
- 输出内容应包含：分析来源、结论、与 NTSD 行为的对应关系、可用于复刻的结论摘要

---

## File Access Policy（文件访问策略）

- 不得在会话初始化阶段主动扫描或读取任何目录
- 只有在用户明确点名某一来源或路径时，才允许读取对应内容
- **任何写入、生成、修改文件的行为，必须先征询用户确认**

---

## Default Analysis Constraints（默认分析约束）

- 不进行无目的全目录扫描
- 若信息不足：必须指出缺失内容，不进行臆测
- 忽略所有 `.meta` 文件、`Library/`、`Temp/`、图片、音频、视频等纯资源文件

---

## 🐛 Debugging & Refactoring Protocol

**触发条件**：用户报告 bug、异常或请求逻辑修改。

**禁止行为**：
- 不得添加随机 `if` 检查、硬编码 clamp 或特殊状态来修复表面症状
- 不得实现反汇编中不存在的逻辑

**强制修复流程**：
1. **溯源**：在反汇编中找到对应代码段
2. **差异分析**：对比当前 C# 实现与反汇编逻辑
3. **对齐修复**：修复必须来自还原反汇编逻辑，而非创造新逻辑

---

## Core Instruction for CodeX MCP

在任何时刻，你必须思考当前过程可以如何与 Codex 进行协作：

1. 在对用户需求形成初步分析后，将需求、初始思路告知 Codex，要求其完善需求分析和实施计划。
2. 在实施具体编码任务前，**必须向 Codex 索要代码实现原型（要求 Codex 仅给出 unified diff patch，严禁对代码做任何真实修改）**。获取原型后，以此为逻辑参考重写，形成生产级代码后才能实施修改。
3. 完成编码后，**必须立即使用 Codex review 代码改动和需求完成程度**。
4. Codex 只能给出参考，必须有自己的思考，必要时对 Codex 的回答提出质疑。

### Codex Tool 参数规范

**必选**：`PROMPT`（任务指令）、`cd`（工作目录）

**关键约束**：
- `sandbox="read-only"`（严禁 Codex 修改代码）
- 每次调用保存返回的 `SESSION_ID` 用于多轮交互
- 要求 Codex 仅给出 unified diff patch

---

## Agent Routing（子代理路由规则）

| 任务类型 | 使用代理 | 说明 |
|----------|----------|------|
| 快速文件查找、定位 | `explore` | 单一目标搜索 |
| NTSD 模块探索、反汇编分析 | `explore-medium` | 需要理解上下文的代码分析 |
| 跨模块深度对照分析 | `explore-high` | 反汇编 vs 当前实现对照 |
| C# 单文件或小范围实现 | `executor` | 标准功能实现 |
| 复杂游戏逻辑重构、多文件改动 | `executor-high` | 状态机、物理系统等大型重构 |
| Bug 分析、状态机追踪、逻辑溯源 | `architect-medium` | 调试与架构分析 |
| 系统级架构决策、跨模块影响评估 | `architect` | 大型设计决策 |
| Unity 编译错误、类型错误修复 | `build-fixer` | 构建问题快速修复 |
| 代码改动后的快速质量检查 | `code-reviewer-low` | 编码完成后立即执行 |
| UI / 菜单相关工作 | `designer` | 界面与交互 |

**强制规则**：
- 编码完成后，**必须**立即调用 `code-reviewer-low` 或使用 Codex review
- 多文件联动改动，**必须**使用 `executor-high`
