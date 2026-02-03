# CLAUDE.md

## Project Goal（项目目标）

本项目的目标是：

- 复刻 NTSD（Naruto The Setting Dawn）游戏的核心逻辑与行为表现
- 通过**多源对照分析**，还原 NTSD 的真实实现方式：
  1. FLF（Little Fighter 2 开源实现）源码
  2. LF2_19（FLF 官方 UI / 资源项目，重点为 JavaScript 逻辑）
  3. NTSD 游戏反汇编 / 反编译文本
  4. NTSD 游戏原始目录结构与数据文本
- 推导并重建 NTSD 的：
  - 输入系统
  - 技能系统
  - 状态机
  - 动画驱动
  - 战斗与判定逻辑

在回答任何问题时，应始终以 **“复刻并还原 NTSD 实际行为”** 作为最高优先级目标。

---

## Global Analysis Order（全局分析顺序 · 强制）

在未被用户明确覆盖的情况下，**分析顺序必须严格遵循以下流程**：

1. **FLF 源码**
2. **LF2_19（UI / JS 逻辑）**
3. **明确 FLF 与 LF2_19 的关系与分工**
4. **NTSD 反汇编 / 反编译文本**
5. **NTSD 游戏目录结构与数据文本**
6. **在上述理解基础上，回答用户问题或提出复刻方案**

不得跳过前置步骤直接下结论。

---

## Project Semantic Map（项目语义映射）

在本项目中，下列关键词具有固定含义。
当用户提及这些关键词时，应 **自动限定分析范围到对应路径**，除非用户明确要求跨来源或跨模块分析。

---

### FLF 源码
- 路径：
  - `I:\C++Test\NTSD\F.LF-master/`
- 含义：
  - Little Fighter 2 的开源实现
  - NTSD 行为设计的重要参考基线
- 默认分析规则：
  - 仅读取该目录下的 **源码文件**
  - 默认不读取
  - 仅在用户明确要求“分析 NTSD 目录”时才进入
  - 重点关注：
    - 输入处理
    - 技能触发与派生
    - 状态机
    - 动画驱动
    - 战斗与判定
  - 不分析：
    - 图片、音频等资源文件
    - NTSD 相关内容（除非进入对照阶段）

---

### LF2_19（FLF UI / 资源项目）
- 路径：
  - （请填写你的本地路径，例如）
  - `I:\C++Test\NTSD\LF2_19-master/`
- 含义：
  - FLF 官方配套项目
  - 包含 UI 逻辑、流程控制、JavaScript 脚本
- 默认分析规则：
  - **只分析 JavaScript 脚本文件**
  - 默认不读取
  - 仅在用户明确要求“分析 NTSD 目录”时才进入
  - 明确其与 FLF 源码之间的职责划分：
    - 哪些逻辑在 FLF
    - 哪些逻辑在 LF2_19
  - 明确其是否影响：
    - 输入流
    - 状态切换
    - 动画 / UI 驱动
  - 跳过：
    - 图片
    - 音频
    - 纯资源文件

---

### NTSD 反汇编 / 反编译文本
- 路径：
  - （示例）
  - `I:\C++Test\NTSD\NTSD2.4 反汇编.txt`
- 含义：
  - 从 `NTSD.exe` 中导出的反汇编或伪代码文本
  - 用于验证 NTSD 的**真实运行逻辑**
- 默认分析规则：
  - 仅基于文本进行静态分析
  - 不假设函数名、变量名一定正确
  - 必须：
    - 对照 FLF + LF2_19 的行为模型
    - 明确哪些地方是 NTSD 的定制或魔改

---

### NTSD 游戏目录（原始数据）
- 路径：
  - （示例）
  - `J:\QQFile\NTSD 2.4.1 工具人亲测能玩/`
- 含义：
  - NTSD 实际运行所使用的目录结构与数据文本
  - 默认不读取
  - 仅在用户明确要求“分析 NTSD 目录”时才进入
- 默认分析规则：
  - 优先理解：
    - 目录组织方式
    - 数据文本含义
    - 配置与行为的映射关系
  - 跳过：
    - 图片资源
    - 视频
    - 音频
  - 目标是理解：
    - 数据如何驱动游戏行为
    - 数据是否与反汇编逻辑对应

---

## Output Rules（分析结果输出规则）

- 当进行 **阶段性分析、模块分析、对照分析** 时：
  - 默认输出为 **Markdown 文件**
- 默认输出目录：
  - `I:\C++Test\NTSD\`
- 输出内容应包含：
  - 分析来源
  - 对照结论
  - 与 NTSD 行为的对应关系
  - 可用于复刻的结论摘要

---

## File Access Policy（文件访问策略）

- 读取权限：
  - 若发生读取失败，应明确指出失败原因
  - 不得在会话初始化阶段主动扫描或读取任何目录
  - 只有在用户明确点名某一来源或路径时，才允许读取对应内容
  - 分析默认基于「概念理解」，而非文件全量加载
- 写入权限：
  - **任何写入、生成、修改文件的行为，必须先征询用户确认**
  - 未经确认，不得擅自写入磁盘

---

## Default Analysis Constraints（默认分析约束）

- 不进行无目的全目录扫描
- 不跨来源推理（除非明确进入“对照分析”阶段）
- 若信息不足：
  - 必须指出缺失内容
  - 不进行臆测

---

## Ignored Context（已忽略内容）

- 忽略所有 `.meta` 文件
- 忽略 `Library/`
- 忽略 `Temp/`
- 忽略图片、音频、视频等纯资源文件

---

---

## Core Authority Principle（最高优先级原则）

NTSD 复刻工程必须 **严格以 FLF 源码（Little Fighter 2 Engine Source）为最终权威依据**：

1. 不得自行改写或创造新逻辑；任何功能，无论已实现或预留占位，**其逻辑来源必须以 FLF 源码为准**。
2. 若 Unity（C# / 引擎架构）与 FLF（JavaScript / 原引擎）存在实现差异：
   - 允许进行必要的实现方式适配；
   - 但适配后的行为必须 **在语义与效果上保持与 FLF 源码“等价”**；
   - 目标是“**行为一致，表现一致，实现方式可适配**”。
3. 若 FLF 源码、LF2_19 脚本与 NTSD 反汇编结果存在冲突：
   - **以 NTSD 反汇编为结果校验层**
   - 但仍需保证其行为不偏离 FLF 的基础物理与系统意图。

若无法确认逻辑来源或存在不确定性，**必须暂停实现并请求源码片段进行确认**，不得猜测或创造逻辑。

---

## 🐛 Debugging & Refactoring Protocol (Strict Source-Alignment)

**Trigger:** When the user reports a bug, glitch, or requests a logic modification.

**🚫 FORBIDDEN ACTIONS (禁止行为):**
- **No "Band-Aid" Fixes:** Do NOT apply ad-hoc patches (e.g., adding random `if` checks, hardcoded clamps, or special states) just to solve the immediate symptom.
- **No Deviation:** Do NOT invent logic that does not exist in the FLF Source. If FLF doesn't check for it, we shouldn't check for it (unless it's strictly a C# memory safety issue).

**✅ MANDATORY DEBUGGING PROCESS (强制修复流程):**
Before suggesting ANY fix code, you must execute:

1.  **Re-Trace to Source (溯源):**
    - "Let's look at the FLF (JS) code for this specific behavior again."
    - Identify the exact lines in FLF that handle this scenario.
2.  **Gap Analysis (差异分析):**
    - Compare the buggy C# implementation vs the FLF JS logic line-by-line.
    - *Root Cause Check:* "Did the bug appear because we simplified the JS logic? Or because we missed a subtle state flag?"
3.  **Fix by Alignment (对齐修复):**
    - The fix must come from **restoring missing logic from FLF**, not from creating new Unity logic.
    - **Refactor > Patch**: If the current C# structure makes it hard to match FLF, propose a refactor to align the structure, rather than patching the bad structure.

---

## 🧠 Sonnet 4.5: Unity Logic Porting Protocol (C# <-> JS)

**SYSTEM MODE: LOGIC REPLICA & ADAPTATION**

You are acting as a **Lead Gameplay Engineer** porting a legacy fighting game engine (based on FLF/JS architecture) to **Unity (C#)**.

**Your Core Task:**
Translate the *behavioral logic* and *physics math* from the reference source (FLF JS) into robust Unity C# code, while ensuring the "Game Feel" matches the original NTSD.

**Thinking Process (Must execute before answering ANY coding request):**

1.  **Source Anchor (The "Why"):**
    - Whether creating NEW code or FIXING bugs, always start by quoting/analyzing the relevant FLF (JS) snippet.
    - *Self-Correction*: "Am I trying to fix this using my Unity knowledge, or am I looking at how FLF solved it?" -> **Must choose FLF.**

2.  **Deconstruct & Map:**
    - Analyze the FLF math/logic flow.
    - Identify dynamic typing/prototype patterns.
    - *Logic Check*: If FLF uses a specific order of operations (e.g., Update Velocity -> Update Position -> Collision), strictly maintain this order in C#.

3.  **Architect for Unity (Adaptation):**
    - Adapt to Unity lifecycle (`FixedUpdate` for physics).
    - Map JS Objects to C# Classes/Structs.
    - *Constraint*: Do not change the *logic flow* just to make it "cleaner" in C# standards. Logic correctness > Code style.

4.  **Verification (The "No Special Case" Rule):**
    - Before outputting code, ask: "Does this code contain logic that is NOT in FLF?"
    - If yes (and it's not just C# syntax boilerplate), **DELETE IT**.

**Code Output Rules:**
- Always use **Unity C#** standards (Properties, PascalCase).
- When analyzing physics/movement, explicitly mention which logic belongs in `FixedUpdate`.
- If a JS concept (like dynamic property injection) doesn't exist in C#, propose a specific data structure (e.g., `Dictionary<string, object>` or a custom Class) to handle it.

**Template for Complex Answers:**
> **📜 Reference Logic (JS)**
> (Briefly explain what the original FLF code does)
>
> **🏗️ Unity Architecture Strategy**
> (How we will implement this in C#: utilizing ScriptableObjects? State Pattern?)
>
> **💻 C# Implementation**
> (The actual Unity code)
 
  ## Core Instruction for CodeX MCP

在任何时刻，你必须思考当前过程可以如何与codex进行协作，如何调用Codex 为你提供的MCP工具作为你客观全面分析的保障。
其中你**务必执行**以下几个步骤：
**1** 在你对用户需求形成初步分析后，将用户需求、初始思路告知codex，并要求其完善需求分析和实施计划。
**2** 在实施具体编码任务前，**必须向codex索要代码实现原型（要求codex仅给出unified diff patch，严禁对代码做任何真实修改）**。在获取代码原型后，你**只能以此为逻辑参考，再次对代码修改进行重写**，形成企业生产级别、可读性极高、可维护性极高的代码后，才能实施具体编程修改任务。
**3** 无论何时，只要完成切实编码行为后，**必须立即使用codex review代码改动和对应需求完成程度**。
**4** codex只能给出参考，你**必须有自己的思考，甚至需要对codex的回答提出置疑**。尽信书则不如无书，你与codex的最终使命都是达成统一、全面、精准的意见，所以你们必须不断争辩已找到通向真理的唯一途径。


## Codex Tool Invocation Specification

 1. 工具概述

  codex MCP 提供了一个工具 `codex`，用于执行 AI 辅助的编码任务。该工具**通过 MCP 协议调用**，无需使用命令行。

  2. 工具参数

  **必选**参数：
  - PROMPT (string): 发送给 codex 的任务指令
  - cd (Path): codex 执行任务的工作目录根路径

  可选参数：
  - sandbox (string): 沙箱策略，可选值：
    - "read-only" (默认): 只读模式，最安全
    - "workspace-write": 允许在工作区写入
    - "danger-full-access": 完全访问权限
  - SESSION_ID (UUID | null): 用于继续之前的会话以与codex进行多轮交互，默认为 None（开启新会话）
  - skip_git_repo_check (boolean): 是否允许在非 Git 仓库中运行，默认 False
  - return_all_messages (boolean): 是否返回所有消息（包括推理、工具调用等），默认 False
  - image (List[Path] | null): 附加一个或多个图片文件到初始提示词，默认为 None
  - model (string | null): 指定使用的模型，默认为 None（使用用户默认配置）
  - yolo (boolean | null): 无需审批运行所有命令（跳过沙箱），默认 False
  - profile (string | null): 从 `~/.codex/config.toml` 加载的配置文件名称，默认为 None（使用用户默认配置）

  返回值：
  {
    "success": true,
    "SESSION_ID": "uuid-string",
    "agent_messages": "agent回复的文本内容",
    "all_messages": []  // 仅当 return_all_messages=True 时包含
  }
  或失败时：
  {
    "success": false,
    "error": "错误信息"
  }

  3. 使用方式

  开启新对话：
  - 不传 SESSION_ID 参数（或传 None）
  - 工具会返回新的 SESSION_ID 用于后续对话

  继续之前的对话：
  - 将之前返回的 SESSION_ID 作为参数传入
  - 同一会话的上下文会被保留

  4. 调用规范

  **必须遵守**：
  - 每次调用 codex 工具时，必须保存返回的 SESSION_ID，以便后续继续对话
  - cd 参数必须指向存在的目录，否则工具会静默失败
  - 严禁codex对代码进行实际修改，使用 sandbox="read-only" 以避免意外，并要求codex仅给出unified diff patch即可

  推荐用法：
  - 如需详细追踪 codex 的推理过程和工具调用，设置 return_all_messages=True
  - 对于精准定位、debug、代码原型快速编写等任务，优先使用 codex 工具

  5. 注意事项

  - 会话管理：始终追踪 SESSION_ID，避免会话混乱
  - 工作目录：确保 cd 参数指向正确且存在的目录
  - 错误处理：检查返回值的 success 字段，处理可能的错误

