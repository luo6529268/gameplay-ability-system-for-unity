# NTSD 帧同步项目总览（跨对话入口）

> 本文档是项目的总入口，用于跨对话快速恢复上下文
> 最后更新：2026-03-02

---

## 快速开始（跨对话使用）

如果你是在新对话中接手这个项目，请按以下步骤：

### Step 1：阅读本文档
- 了解项目背景
- 查看当前进度
- 确认下一步任务

### Step 2：读取对应的详细文档
- 根据当前进度，读取对应的实施文档（见下方"文档索引"）

### Step 3：开始实施
- 按照文档中的代码模板和步骤执行

---

## 项目背景

### 项目信息
- **项目名称**：NTSD（格斗游戏）
- **项目路径**：`D:\Test\gameplay-ability-system-for-unity\Assets\NTSD`
- **Unity 版本**：2022.3+
- **目标**：为现有单机格斗游戏添加帧同步联机功能

### 目标效果
- 支持 P2P 直连（房主本地服务器）
- 王者荣耀风格的帧同步（一个人卡不影响其他人）
- 支持 2-20 人房间
- 输入预测 + 输入延迟窗口

### 技术方案
- **网络层**：Photon Fusion（免费 20 CCU）
- **同步方式**：帧同步（Lockstep）
- **核心思路**：基于现有 NTSD 架构 + 补齐帧同步能力

---

## 项目现状（已有的架构）

### ✅ 已具备的核心能力
1. **SimulationTickDriver**（固定 30Hz tick 驱动）
   - 路径：`Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
   - 功能：在 FixedUpdate 中以固定频率驱动模拟

2. **SimulationWorld**（确定性执行容器）
   - 路径：`Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
   - 功能：按 SimOrder + StableId 确定性排序执行

3. **SimInputBuffer**（输入缓冲）
   - 路径：`Assets/NTSD/Scripts/Simulation/Input/SimInputBuffer.cs`
   - 功能：按 tick 对齐输入

4. **PhysicsState + SceneQuery**（自定义物理）
   - 路径：`Assets/NTSD/Scripts/Animation/Character/PhysicsState.cs`
   - 功能：不依赖 Unity Physics 的自定义碰撞检测

### ❌ 缺少的能力（需要补齐）
1. 确定性随机数源（替换 `UnityEngine.Random`）
2. 网络房间与输入广播（Photon Fusion）
3. 输入延迟窗口（InputDelay）
4. 输入缺失时预测策略
5. 联机 StableId 分配规则
6. 一致性校验（Hash）
7. **ET8 关键保底措施**（输入确认/重传/追帧/帧号对齐等）

---

## 文档索引

所有文档路径：`D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\`

| 文档名 | 用途 | 何时阅读 |
|--------|------|---------|
| **NTSD_Lockstep_README.md** | 总入口（本文档） | 跨对话时首先阅读 |
| **PROGRESS.md** ✨ | **进度追踪（必读）** | **每次对话开始时读取** |
| **ISSUES.md** ✨ | **问题记录（必读）** | **遇到问题时查询** |
| **CONFIG.md** ✨ | **项目配置（必读）** | **修改配置时更新** |
| **NTSD_ET8_Deep_Analysis.md** 🔥 | **ET8 深度分析（核心）** | **遇到问题时必读** |
| NTSD_Lockstep_Framework_Plan.md | 架构设计与分层方案 | 理解整体架构 |
| NTSD_Lockstep_Risk_Assessment.md | 风险评估与改动规模 | 了解现状问题 |
| NTSD_Lockstep_Execution_Guide.md | 执行清单与任务分解 | 查看完整任务列表 |
| NTSD_Lockstep_Code_Templates.md | Phase A 代码模板 | 实施 Phase A |
| NTSD_Photon_Fusion_Integration_Guide.md | Phase B 网络层集成 | 实施 Phase B |
| NTSD_ET8_Critical_Features.md | ET8 关键保底措施 | 实施 Phase B/C/D 时参考 |

---

## 实施进度（手动更新）

### Phase A：确定性修复与验证
- [ ] **A1. 替换随机数**（1-2 天）
  - [ ] 创建 `DeterministicRng.cs`
  - [ ] 修改 `SimContext.cs`
  - [ ] 修改 `LF2Character.cs` 的 3 处随机调用
  - [ ] 验证：同 seed 同结果

- [ ] **A2. 录制回放测试**（2-3 天）
  - [ ] 创建 `InputRecorder.cs`
  - [ ] 创建 `InputReplayer.cs`
  - [ ] 创建 `WorldStateHasher.cs`
  - [ ] 验证：10 次回放 hash 一致

### Phase B：Photon Fusion 接入
- [ ] **B1. Fusion 配置**（1 天）
  - [ ] 注册 Photon 账号
  - [ ] 创建 Fusion 应用
  - [ ] 获取 App ID
  - [ ] 导入 Fusion SDK
  - [ ] 验证连接成功

- [ ] **B2. 输入同步**（3-4 天）
  - [ ] 创建 `NetworkInputData.cs`
  - [ ] 创建 `FusionLockstepManager.cs`
  - [ ] 实现输入采集与注入
  - [ ] 验证：2 人对战同步

- [ ] **B3. StableId 修复**（1-2 天）
  - [ ] 修改 `AllocateStableId` 逻辑
  - [ ] 实现 Host 统一分配
  - [ ] 验证：多端顺序一致

### Phase C：体验优化
- [ ] **C1. 输入延迟配置**（0.5 天）
  - [ ] 设置 `InputDelayTicks = 6`
  - [ ] 测试不同延迟值

- [ ] **C2. 输入预测**（2-3 天）
  - [ ] 实现输入缺失 fallback
  - [ ] 实现超时踢出
  - [ ] 验证：慢玩家不阻塞全局

### Phase D：一致性监控
- [ ] **D1. Hash 校验**（2-3 天）
  - [ ] 每 30 帧计算 hash
  - [ ] 实现 hash 上报与对比
  - [ ] 验证：持续一致

---

## 关键文件位置

### 需要修改的现有文件
```
Assets/NTSD/Scripts/
├── Simulation/
│   ├── SimulationWorld.cs           # 需修改构造函数（添加 seed 参数）
│   └── SimContext.cs                # 需修改（添加 Rng 字段）
└── Animation/
    └── LF2Objects/
        └── LF2Character.cs          # 需修改 3 处随机调用（Line 985, 1013, 1320）
```

### 需要新增的文件
```
Assets/NTSD/Scripts/
├── Simulation/
│   ├── Core/
│   │   ├── DeterministicRng.cs          # Phase A1 新增
│   │   └── WorldStateHasher.cs          # Phase A2 新增
│   └── Input/
│       ├── InputRecorder.cs             # Phase A2 新增
│       └── InputReplayer.cs             # Phase A2 新增
└── Netcode/
    └── Fusion/
        ├── FusionConnectionTest.cs      # Phase B1 新增
        ├── NetworkInputData.cs          # Phase B2 新增
        └── FusionLockstepManager.cs     # Phase B2 新增
```

---

## 快速命令（跨对话）

### 🔴 标准启动流程（必须执行）

**每次新对话开始时，必须执行**：
```
请读取以下文件，了解项目当前状态：
1. D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_Lockstep_README.md
2. D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\PROGRESS.md
3. D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\ISSUES.md
4. D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\CONFIG.md
5. D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_ET8_Deep_Analysis.md（遇到问题时）
```

**Claude 读取后会知道**：
- ✅ 项目背景和目标
- ✅ 当前进度（做到哪了）
- ✅ 已解决的问题（避免重复踩坑）
- ✅ 项目配置（参数设置）

---

### 继续特定 Phase

#### 继续 Phase A1（替换随机数）
```
读取 PROGRESS.md 确认当前进度，然后读取：
- D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_Lockstep_Code_Templates.md

查看"Phase A：确定性修复代码模板"章节，帮我实施 Phase A1
```

#### 继续 Phase A2（录制回放）
```
读取 PROGRESS.md 确认当前进度，然后读取：
- D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_Lockstep_Code_Templates.md

查看"A4. WorldStateHasher.cs"、"A5. InputRecorder.cs"、"A6. InputReplayer.cs"章节
```

#### 继续 Phase B（Photon Fusion）
```
读取 PROGRESS.md 确认当前进度，然后读取：
- D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_Photon_Fusion_Integration_Guide.md
- D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_ET8_Critical_Features.md

从当前进度开始实施
```

---

### 排查问题
```
我遇到了 [具体问题描述]，请先读取：
- D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\ISSUES.md

查看是否有类似问题的解决方案
```

---

### 查看整体进度
```
读取 D:\Test\gameplay-ability-system-for-unity\Assets\NTSD\PROGRESS.md
```

---

## 已知风险点（来自风险评估）

### 🔴 Critical（必须修复）
1. **随机数**：3 处 `UnityEngine.Random` 在战斗逻辑中
   - 位置：`LF2Character.cs` Line 985, 1013, 1320
   - 影响：跨端结果不一致

### 🟠 High（应该修复）
2. **StableId 分配**：联机时需要 Host 统一分配
   - 影响：执行顺序可能不一致

3. **快照与回滚**：当前不支持（Phase E 可选）
   - 影响：无法处理迟到输入

### 🟢 Low（无需修改）
- ✅ 时间依赖：Simulation 层已无 Time 依赖
- ✅ 物理引擎：已使用自定义碰撞检测
- ✅ 集合遍历：已使用 SortedDictionary
- ✅ Transform 同步：已单向数据流

---

## 工作量估算

| Phase | 内容 | 预估时间 | 难度 |
|-------|------|---------|------|
| Phase A | 确定性修复与验证 | 3-5 天 | ⭐⭐⭐ |
| Phase B | Fusion 接入 + 输入确认 | 6-8 天 | ⭐⭐⭐⭐ |
| Phase C | 体验优化 + 重传/追帧 | 5-7 天 | ⭐⭐⭐⭐ |
| Phase D | Hash 校验 + 帧号对齐 + 日志 | 3-4 天 | ⭐⭐⭐⭐ |
| **总计** | | **17-24 天** | |

*假设每天投入 4-6 小时*
*注：增加 ET8 关键措施后，工作量略有增加，但稳定性显著提升*

---

## 最小可行版本（MVP）

满足以下条件即可先上线小规模测试：
- [x] 已替换权威随机源
- [x] 单机回放 hash 一致
- [x] Fusion 房间创建/加入稳定
- [x] 2~4 人对战输入同步正确
- [x] 开启 InputDelay + 缺失输入预测
- [x] 慢玩家不阻塞全局推进

---

## 技术架构对比（NTSD vs ET8）

| 功能 | ET8 实现 | NTSD 实现 | 一致性 |
|------|---------|----------|--------|
| 确定性执行 | LSWorld | SimulationWorld | ✅ 思路一致 |
| Tick 驱动 | LSUpdater | SimulationTickDriver | ✅ 思路一致 |
| 输入缓冲 | FrameBuffer | SimInputBuffer | ✅ 思路一致 |
| 网络层 | ET 消息系统 | Photon Fusion | ⚠️ 可替换 |
| RNG | TrueSync.TSRandom | DeterministicRng | ⚠️ 可替换 |
| 物理 | TrueSync 物理 | PhysicsState | ⚠️ 可替换 |

**结论**：NTSD 的核心架构和 ET8 思路一致，只是网络层和部分实现不同。

---

## 联系方式与资源

### Photon 相关
- 官网：https://www.photonengine.com/
- Dashboard：https://dashboard.photonengine.com/
- Fusion 文档：https://doc.photonengine.com/fusion/current/getting-started/sdk-download

### ET8 参考
- 项目路径：`D:\Test\com.stone.et8-Release2.0`
- 可参考但不直接移植

---

## 更新日志

### 2026-03-02
- 创建项目文档
- 完成架构设计
- 完成风险评估
- 创建代码模板
- 创建 Fusion 集成指南
- 创建本 README

---

## 下一步行动

### 如果你是第一次看到这个文档
1. 阅读完本文档
2. 阅读 `NTSD_Lockstep_Framework_Plan.md`（理解架构）
3. 阅读 `NTSD_Lockstep_Risk_Assessment.md`（了解风险）
4. 开始 Phase A1

### 如果你正在实施中
1. **先读取 `PROGRESS.md`**，找到当前任务
2. 读取对应的详细文档
3. 按照代码模板实施
4. **完成后更新 `PROGRESS.md`**（打勾 + 添加时间）
5. **如果遇到问题，记录到 `ISSUES.md`**
6. **如果修改配置，更新 `CONFIG.md`**
7. **推送到 Git**

### 如果遇到问题
1. **先查看 `ISSUES.md`**，看是否有类似问题
2. 如果有，直接使用已有的解决方案
3. 如果没有，排查并解决后**必须记录到 `ISSUES.md`**
4. 推送到 Git

---

## 🔴 Claude 的核心责任（重要！）

### 每次帮助用户后，Claude 必须：

#### 1. 完成任务后
- ✅ 更新 `PROGRESS.md`（标记任务完成 + 添加时间）
- ✅ 提醒用户推送到 Git

#### 2. 解决问题后
- ✅ 记录到 `ISSUES.md`（问题编号 + 详细信息）
- ✅ 在 `PROGRESS.md` 的任务备注中引用问题编号
- ✅ 提醒用户推送到 Git

#### 3. 修改配置后
- ✅ 更新 `CONFIG.md`（新值 + 变更原因）
- ✅ 记录到"配置变更历史"
- ✅ 提醒用户同步代码中的硬编码值
- ✅ 提醒用户推送到 Git

#### 4. 每次新对话开始时
- ✅ 提醒用户先读取 `PROGRESS.md` / `ISSUES.md` / `CONFIG.md`
- ✅ 确认当前进度后再开始工作

---

## 🔄 跨电脑协作流程

### 电脑 A 完成工作
1. Claude 帮助完成任务
2. Claude 更新 `PROGRESS.md` / `ISSUES.md` / `CONFIG.md`
3. 用户推送到 Git：
   ```bash
   git add Assets/NTSD/*.md
   git commit -m "完成 Phase A1"
   git push
   ```

### 电脑 B 继续工作
1. 拉取最新代码：
   ```bash
   git pull
   ```
2. 开启新对话，让 Claude 读取：
   ```
   读取以下文件：
   1. D:\Projects\NTSD\Assets\NTSD\PROGRESS.md
   2. D:\Projects\NTSD\Assets\NTSD\ISSUES.md
   3. D:\Projects\NTSD\Assets\NTSD\CONFIG.md
   ```
3. Claude 了解当前进度，继续工作
4. 完成后重复"电脑 A"的流程

---

**祝实施顺利！🚀**
