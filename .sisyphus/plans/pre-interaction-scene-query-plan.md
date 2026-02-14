# 计划：Pre-Interaction 与 SceneQuery 对齐落地（FLF）

## 0. 已确认决策（来自你）
- 范围：**Character + Weapon + SpecialAttack 全部本轮接入**。
- 触发语义：**Post-combo only**（不引入额外持续检查）。
- 架构方向：`CollisionUtil`、`NTSDItrKindHandler` **不保留 static**，本轮直接抽象为可替换实例服务。
- 测试策略：**Tests-after**（先落地骨架，再补关键回归验证）。
- 触发点统一策略：**按现有时机接入**（Character 保持 `post_combo`；Weapon/SpecialAttack 使用现有 TU 时机，不做事件系统改造）。

## 1. 目标与非目标

### 目标
1. 落地 `pre_interaction` 最小骨架：
   - 获取 ITR 区域
   - 查询 body 重叠候选
   - 过滤（self/invalid/规则/arest-vrest）
   - 按 `itr.kind` 分发到占位处理入口
2. 保持 `SceneQuery` 为“几何/查询层”，移除业务 kind 语义耦合。
3. 把 `CollisionUtil`、`NTSDItrKindHandler` 抽象为可注入/可替换实例服务。

### 非目标（本轮不做）
- 抓取/拾取完整行为实现（只保留 kind 分发占位）。
- 大规模规则重构（仅做必要迁移与解耦）。
- 全量自动化测试体系建设（仅补关键回归）。

---

## 2. 关键假设（默认值，若你不反对则按此执行）
1. Character 的 `post_combo` 为主触发点保持不变。
2. Weapon/SpecialAttack 无独立 combo 管线时，采用其现有 TU 内交互时机作为“post-combo 对齐语义”落点（不新增新事件系统）。
3. arest/vrest 的检查顺序：
   - 命中候选前/分发前执行测试
   - 仅在交互真正成立后写回 update

---

## 3. 设计方案（最小可落地）

### 3.1 服务抽象（替代 static）
新增服务接口（命名可微调）：
- `IOverlapService`：负责体积相交判定（替代 `CollisionUtil.Intersect` 静态调用）。
- `IItrKindPolicyService`：负责 kind 语义分类与目标规则判断（承接 `IsPreInteractionKind/IsAttackKind` 与 `NTSDItrKindHandler` 相关入口）。

默认实现：
- `DefaultOverlapService`（纯计算实现，可为 `readonly struct` + service 包装，或 class 实现）。
- `DefaultItrKindPolicyService`（包含 pre/attack kind 分类与必要规则映射）。

### 3.2 SceneQuery 边界
- `ILF2SceneQuery` 保持不变（查询契约）。
- `BruteForceSceneQuery` 仅依赖 `IOverlapService` 做几何判定；移除/迁出 kind 业务判定函数。

### 3.3 PreInteraction 编排
在三类对象统一形成“同构骨架”：
1. 取当前帧 pre_interaction ITR（通过 kind policy 判定是否 pre kind）
2. 为 ITR 体积调用 `SceneQuery.QueryBodies(...)`
3. 候选过滤：self/invalid/team/dead/arest-vrest/扩展规则
4. `switch(itr.kind)` 分发到占位 handler（1/2/3/7）
5. 成功后再执行 `ItrArestUpdate/ItrVrestUpdate`

---

## 4. 文件级最小改动清单

## A. 新增（抽象层）
- `Assets/NTSD/Scripts/Animation/Services/IOverlapService.cs`
- `Assets/NTSD/Scripts/Animation/Services/DefaultOverlapService.cs`
- `Assets/NTSD/Scripts/Animation/Services/IItrKindPolicyService.cs`
- `Assets/NTSD/Scripts/Animation/Services/DefaultItrKindPolicyService.cs`

## B. 修改（查询层解耦）
- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
  - 仅保留查询/几何；移除 kind 业务静态判断。
  - 改为通过 `IOverlapService` 判定相交。

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
  - 在 world 构造时组装默认服务实例并注入 SceneQuery。

## C. 修改（编排层落地）
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
  - 实装 `Generic_PreInteraction()` 最小骨架（post_combo 调用点保持）。

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
  - 增加与 Character 同构的 pre_interaction 骨架入口（以现有 TU 交互时机接入）。

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
  - 增加同构 pre_interaction 骨架入口（以现有 TU 交互时机接入）。

## D. 迁移/适配（规则层）
- `Assets/NTSD/Scripts/NTSD_Extensions/NTSDItrKindHandler.cs`
  - 从静态工具迁移为策略服务（或被服务包装调用）。

- `Assets/NTSD/Scripts/NTSD_Extensions/NTSDCollisionExtensions.cs`
  - 由直接静态判定改为调用注入/可访问的 kind policy 服务入口。

---

## 5. 实施顺序
1. 先加服务接口与默认实现（不改行为）。
2. 替换 `BruteForceSceneQuery` 内静态几何调用为服务调用。
3. 迁出 kind 语义到 `IItrKindPolicyService`，保持旧结果一致。
4. 分别在 Character/Weapon/SpecialAttack 落地 pre_interaction 最小骨架。
5. 增加 tests-after 回归验证（关键路径）。

---

## 6. 验证与验收标准（Tests-after）

### 编译/诊断
- 变更文件 `lsp_diagnostics` 无新增 error。
- Unity 编译通过（无新增编译错误）。

### 行为用例（最小集合）
1. **无 ITR 帧**：不触发候选查询，不报错。
2. **有 ITR 但无重叠候选**：流程正常结束。
3. **单候选重叠且 arest/vrest 允许**：进入对应 kind 分发入口。
4. **arest 阻断**：不进入分发入口。
5. **vrest 阻断**：不进入分发入口。
6. **多候选重叠**：按既定顺序过滤并分发，无重复触发。

### 边界约束
- 不改变 attack interaction 既有行为。
- 不把业务 kind 规则留在 SceneQuery 几何层。
- 不引入 `as any/@ts-ignore`（C# 项目同理：不做类型逃逸）。

---

## 7. 风险与锁边界
1. **风险：三类对象触发语义不完全一致**
   - 锁边界：不新增事件系统；按现有调用点做最小接入。
2. **风险：规则分散回流**
   - 锁边界：kind 语义统一收敛到 `IItrKindPolicyService`。
3. **风险：双触发/重复结算**
   - 锁边界：arest/vrest 检查 + 成功后更新，保证节流。

---

## 8. 完成定义（DoD）
- Character/Weapon/SpecialAttack 均有 pre_interaction 最小骨架。
- SceneQuery 只承担查询/几何职责。
- `CollisionUtil`、`NTSDItrKindHandler` 已去 static 化并抽象为可替换服务入口。
- Tests-after 最小回归清单可执行并通过（或明确记录既有失败项）。

---

## 9. 自审结论（Gap Classification）

### Critical（必须明确，已处理）
1. **Weapon/SpecialAttack 触发点差异**
   - 已确认：按现有时机接入（Character=post_combo，Weapon/SpecialAttack=TU），本轮不改事件系统。
2. **服务化边界**
   - 已确认：`CollisionUtil`、`NTSDItrKindHandler` 不保留 static，改为可替换服务。

### Minor（可自动收敛）
1. ITR 多命中顺序与首次命中策略 -> 先保持现有遍历顺序，避免行为漂移。
2. 无 ITR 帧快速返回 -> 作为骨架固定分支。
3. self/dead/team 过滤顺序 -> 固化为统一过滤管线。

### Ambiguous / Defaulted（采用默认并披露）
1. 是否做“高精度二次审查（Momus）”
   - 默认：**先不启用**，直接进入实现；如你需要我可在实现后再补一轮 Momus 审查。
2. tests-after 的最小样例颗粒度
   - 默认：先覆盖 6 个关键行为用例（无 ITR / 无候选 / 命中 / arest 阻断 / vrest 阻断 / 多候选）。
