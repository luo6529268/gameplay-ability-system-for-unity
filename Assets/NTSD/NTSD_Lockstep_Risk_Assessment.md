# NTSD 帧同步（Lockstep）风险评估清单

> 评估目标：识别现有代码中可能影响"严格 PVP 帧同步"确定性的风险点  
> 评估范围：`Assets/NTSD/Scripts/` 目录  
> 评估时间：2026-03-02

---

## 风险等级定义

- **🔴 Critical（必须修复）**：直接影响战斗权威状态，跨端必定不一致
- **🟠 High（应该修复）**：某些条件下会导致不同步，PVP 模式必须处理
- **🟡 Medium（可延后）**：表现层或非权威逻辑，可以暂时隔离
- **🟢 Low（无需修改）**：已经符合确定性要求或在 Simulation 层外

---

## 1. 随机数来源（Random）

### 🔴 Critical - 战斗逻辑使用 UnityEngine.Random

**文件位置**：
- `Scripts/Animation/LF2Objects/LF2Character.cs`
  - Line 985: `UnityEngine.Random.value < 0.5f` 选择武器攻击动作
  - Line 1013: `UnityEngine.Random.value < 0.5f` 选择挥拳动画
  - Line 1320: `UnityEngine.Random.value < 0.15f` AI 决策概率

**问题**：
- `UnityEngine.Random` 是全局静态随机源，无法保证跨端一致
- 每次调用会改变内部状态，调用顺序/次数不同会导致后续结果不同
- 这些随机调用直接影响战斗动作选择（权威逻辑）

**影响范围**：
- 角色攻击动作选择（60 vs 65 帧）
- 武器攻击类型选择
- AI 行为决策

**修复方案**：
1. 创建 `DeterministicRng` 类（基于固定种子的 System.Random 或自定义实现）
2. 在 `SimContext` 或 `SimulationWorld` 中持有唯一实例
3. 所有战斗逻辑改为：`context.Rng.NextFloat() < 0.5f`
4. 联机时服务器下发种子，所有端从同一种子开始

**预估工作量**：中等
- 创建 RNG 类：1-2 小时
- 替换所有调用点：需要全局搜索 `UnityEngine.Random`，逐个改为 context 注入

---

## 2. 时间依赖（Time）

### 🟢 Low - Simulation 层已无 Time 依赖

**扫描结果**：
- `Scripts/Simulation/` 目录下**未发现** `Time.time` / `Time.deltaTime` 使用
- 所有 Simulation 逻辑已经通过 `SimulationTickDriver` 的固定 tick 驱动

**发现的 Time 使用均在表现层/UI**：
- `Scripts/UI/SelectRoleItem.cs:309` - UI 闪烁动画
- `Scripts/UI/CharacterSelectionController.cs:247` - 倒计时 UI
- `Scripts/Test/ProCamera2DTestPanel.cs` - 测试工具
- `Scripts/Animation/LF2ObjectPool.cs:134,150,164` - 对象池过期清理（非权威）

**结论**：✅ 当前 Simulation 核心已正确隔离时间依赖，无需修改

---

## 3. 物理引擎依赖（Unity Physics）

### 🟢 Low - 已使用自定义碰撞检测

**扫描结果**：
- `Scripts/Simulation/` 和 `Scripts/Animation/` 核心战斗逻辑**未使用** Unity Physics/Rigidbody 作为权威
- 发现的 Physics 引用均为：
  - `Scripts/GAS/` - GAS 框架（非 NTSD 核心）
  - `Scripts/Test/` - 测试工具

**当前实现**：
- 碰撞检测：`BruteForceSceneQuery` 实现自定义 AABB 碰撞
- 物理状态：`PhysicsState` 类手动管理位置/速度/摩擦
- 位移推进：在 `SimTick` 中手动计算 `ps.x += ps.vx`

**优势**：
- 完全确定性（纯数学计算）
- 跨平台一致
- 可序列化/可回滚

**后续优化方向**（非风险项）：
- 将 `BruteForceSceneQuery` 替换为四叉树/空间哈希（性能优化，不影响确定性）

**结论**：✅ 已经是正确路线，无需修改

---

## 4. 集合遍历顺序（Dictionary/HashSet）

### 🟢 Low - 已使用 SortedDictionary

**扫描结果**：
- `SimulationWorld.cs` 使用 `SortedDictionary<int, Bucket>` 按 SimOrder 排序
- Bucket 内使用 `List<ISimObject>` + `OrderBy(obj => obj.StableId)` lazy sort

**当前实现**：
```csharp
private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();
// ...
bucket.items = items.OrderBy(obj => obj.StableId).ToList();
```

**结论**：✅ 已经保证确定性顺序，无需修改

**注意事项**（未来扩展时）：
- 如果其他地方新增 `Dictionary` / `HashSet` 遍历，需要确保：
  - 仅用于查找（不遍历）
  - 或遍历后排序再使用

---

## 5. StableId 分配机制

### 🟠 High - 联机时需要服务器统一分配

**当前实现**：
- `SimulationWorld.AllocateStableId()` 本地递增（从 100 开始）
- 注释中已说明："多人模式：服务器会显式设置 StableId"

**问题**：
- 单机模式：本地分配没问题
- 联机模式：如果各端独立分配，同一对象在不同端可能得到不同 StableId → 执行顺序不一致

**修复方案**：
1. 联机时禁用本地 `AllocateStableId()`
2. 服务器创建对象时分配 StableId 并广播
3. 客户端收到创建消息时使用服务器指定的 StableId

**预估工作量**：中等
- 需要在网络层实现"创建对象"消息（包含 StableId）
- 修改对象创建流程，区分单机/联机模式

---

## 6. 输入来源

### 🟡 Medium - 需要确认输入收集点是否已收敛

**当前架构**：
- ✅ `SimInputBuffer` 提供 tick 对齐的输入缓冲
- ✅ `EnqueueForNextTick` / `EnqueueForTick` 接口完善

**需要核实的点**（未在本次扫描中完全覆盖）：
1. 是否所有战斗对象都从 `SimInputBuffer.TryDequeueAll(tick)` 消费输入？
2. 是否还有地方在 `Update()` / `FixedUpdate()` 中直接读 Unity Input？

**建议行动**：
- 搜索所有 `Input.GetKey` / `Input.GetButton` / InputSystem 回调
- 确认它们只写入 `SimInputBuffer`，不直接驱动战斗逻辑

**预估工作量**：小到中等（取决于发现的直接输入点数量）

---

## 7. 快照与回滚能力

### 🟠 High - 当前不支持快照，需要设计

**当前状态**：
- ❌ 未发现 Snapshot / Serialize / Rollback 相关代码
- ✅ 核心状态集中在 `SimulationWorld` / `PhysicsState` / `LF2LivingObject`

**需要快照的关键状态**（初步清单）：
1. **SimulationWorld**
   - `_nextAutoStableId`（StableId 计数器）
   - 所有注册对象的引用列表
2. **每个 LF2LivingObject**
   - `PhysicsState`（位置/速度/朝向/摩擦）
   - `LF2FrameInfo`（当前帧号/动画状态）
   - `LF2Health`（HP/MP）
   - `LF2EffectState`（buff/debuff 状态）
   - 输入缓冲窗口（如果有）
3. **RNG 状态**（未来添加后）
   - 当前种子/内部状态

**不需要快照的内容**：
- 表现层：Animator / SpriteRenderer / VFX
- 资源引用：FrameData / CharacterData（从配置重建）
- UI 状态

**修复方案**：
1. 为核心类添加 `Serialize()` / `Deserialize()` 方法
2. 创建 `SnapshotStore` 保存最近 N tick 快照（例如 60-180 tick）
3. 实现 `RollbackManager.RestoreSnapshot(tick)` + 重演逻辑

**预估工作量**：大
- 设计序列化格式：1-2 天
- 实现所有核心类的序列化：3-5 天
- 测试回滚正确性：2-3 天

---

## 8. 浮点数确定性

### 🟡 Medium - 当前使用 float，需要评估跨平台一致性

**当前实现**：
- `PhysicsState` 所有字段均为 `float`
- 速度/位置计算使用 `Mathf` / 标准浮点运算

**风险评估**：
- **低风险场景**：同平台（都是 Windows x64 / 都是 Android ARM64）
- **中风险场景**：跨平台（PC vs 移动端）
- **高风险场景**：复杂物理模拟 + 长时间累积误差

**当前 NTSD 的情况**：
- 物理相对简单（2D 横版，无复杂刚体碰撞）
- 每 tick 重新设置速度（不是累积型物理）
- 有摩擦/重力但计算简单

**建议策略**（按优先级）：
1. **短期**：先用 float + 严格校验（每 N tick 对比 hash）
   - 如果发现不一致，记录并分析是否是浮点问题
2. **中期**：如果确认有浮点漂移，考虑：
   - 使用 fixed point 库（例如 FixMath.NET）
   - 或限制浮点运算（避免除法/三角函数，使用查表）
3. **长期**：如果要支持严格跨平台 PVP，最终可能需要 fixed point

**预估工作量**（如果需要迁移到 fixed point）：大
- 替换所有 float → FixedPoint：1-2 周
- 测试所有战斗逻辑：1-2 周

---

## 9. Transform 同步

### 🟢 Low - Transform 仅用于表现，不影响权威状态

**当前实现**：
- 权威位置存储在 `PhysicsState.x/y/z`（像素单位）
- `Transform.position` 从 `PhysicsState` 单向同步（表现层）

**代码证据**：
```csharp
// LF2Character.cs:2572
_CharacterHub.transform.position = new Vector3(
    PS.x / SimulationConstants.PIXELS_PER_UNIT,
    PS.y / SimulationConstants.PIXELS_PER_UNIT + groundY,
    _CharacterHub.transform.position.z
);
```

**结论**：✅ 正确的单向数据流（Sim → Presentation），无需修改

---

## 总结：改动规模评估

### 必须修复（联机前）
1. **🔴 随机数统一**：中等工作量（1-3 天）
2. **🟠 StableId 联机分配**：中等工作量（2-3 天）

### 应该修复（严格 PVP）
3. **🟠 快照与回滚**：大工作量（1-2 周）

### 可延后评估
4. **🟡 输入收敛检查**：小到中等（1-2 天）
5. **🟡 浮点确定性**：视测试结果决定（可能 0 天，也可能 2-4 周）

### 无需修改（已符合要求）
- ✅ 时间依赖（已隔离）
- ✅ 物理引擎（已自定义）
- ✅ 集合遍历（已排序）
- ✅ Transform 同步（已单向）

---

## 推荐实施路线

### Phase 1：单机可回放（1 周）
- 统一随机数源
- 实现输入录制/回放
- 每 tick 计算 hash 并记录

### Phase 2：本机双端验证（1 周）
- 实现 StableId 服务器分配
- 实现基础网络协议（输入上行/广播）
- 本机跑 server + client 验证一致性

### Phase 3：真联机 + inputDelay（1 周）
- 实现输入延迟窗口
- 缺帧等待策略
- 网络测试

### Phase 4：回滚与断线重连（2-3 周）
- 实现快照系统
- 实现回滚重演
- 实现断线重连

### Phase 5：跨平台验证（视情况）
- 如果发现浮点不一致，考虑 fixed point 迁移

---

## 附录：扫描命令记录

```bash
# 随机数
grep -rn "UnityEngine.Random\|System.Random\|new Random(" Scripts/

# 时间依赖
grep -rn "Time.time\|Time.deltaTime\|DateTime.Now" Scripts/

# 物理引擎
grep -rn "Physics\.\|Physics2D\.\|Rigidbody\|Collider" Scripts/

# 不稳定集合
grep -rn "Dictionary<\|HashSet<" Scripts/
```
