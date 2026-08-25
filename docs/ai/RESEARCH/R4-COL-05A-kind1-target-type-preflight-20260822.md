# R4-COL-05A — kind1 non-character target consume preflight

> 日期：2026-08-22  
> 类型：只读 source preflight；本文件建立时未修改 Unity / C++ gameplay。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 关联差异：`D-COL-005` 的 character-attacker / common writer 子范围。  

## 1. 结论

C++ release 对 kind1 与 kind3 的 target type 限制不相同：

- kind3、kind8在 candidate preflight 明确要求 `victim.char_data->obj_type == 0`；
- kind1没有这条 type gate，candidate成功后会进入 common `case 1`，直接读写通用 Entity runtime
  字段、frame、位置、caught/catcher relation和duration。

Unity frozen candidate preflight已正确只限制 kind3/8；但共同消费 writer
`BattleInteractionWriter.TryApplyGrab` 又对 `kind == 1 || kind == 3` 一并要求 Character target，
导致已记录的 kind1 non-character candidate被错误拒绝。最小修复是将该 type gate收窄为**仅 kind3**；
kind1只保留既有 null/runtime和字段写入条件。

## 2. C++ release 合同（VERIFIED）

`Makefile:11-35` 使 `collision.cpp` 与 `collision_collect.cpp` 参与正式 `ntsd_new.exe`。

### 2.1 collect gate

`collision_collect.cpp:264-335`：

- `kind == 3 && victim.obj_type != 0 → continue`；
- `kind == 8 && victim.obj_type != 0 → continue`；
- 没有 `kind == 1 && victim.obj_type != 0` 的排除。

相同类型分支也保留在 `collision.cpp:250-263`。所以本包不能把“kind1可能被后续选择拒绝”错误替换为
“kind1 target type必须为角色”。

### 2.2 kind1 consume

`collision.cpp:921-993` 的 `case 1` 对当前 candidate：

1. 清 attacker/victim `vx`；
2. 按 `x_int` 写双方 facing；
3. raw-write `itr.catchingact` / `itr.caughtact` frame；
4. 以当前 frame cpoint/center计算位置和整数 mirror；
5. 写 attacker `caught_idx`、victim `catcher_idx`、caught duration=300、victim fall=0；
6. 不在此 case之前增加 kind1 target `obj_type == 0` gate。

这是一条 Entity layout通用的 writer；C++不会因为 target是武器/other而在 type gate处提前跳过。

## 3. Unity 现状（VERIFIED）

### 3.1 正确的 collection side

`BruteForceSceneQuery.ItrAllowed:6516-6536` 已是：

```csharp
if ((kind == 3 || kind == 8) && targetType != Character)
    return false;
```

因此此包不改 candidate collector、kind1 nearest selector、RNG或 frozen sequence。

### 3.2 错误的 consume side

`BattleInteractionWriter.TryApplyGrab:14-75`：

```csharp
if (kind != 1 && kind != 3) return false;
if (ResolveCurrentDataObjectType(victim) != Character) return false;
```

该第二条件同时拒绝 kind1与kind3，和 C++ 的 kind3-only gate不一致。其后的 raw frame、位置、relation、
duration和fall字段写入本身可接受 `LF2Entity`，不依赖 CLR `LF2Character`。

`LF2CharacterInteractionResolver`、`LF2CharacterDatInteractionResolver`和`LF2SpecialAttack`的kind1/3
分支都委托到这个 writer，因此修复 writer能覆盖当前 character-current-DAT / shared-character-DAT
attacker的正式消耗入口。

## 4. 独立边界（不混入本包）

- `LF2WeaponInteractionResolver` 对 kind1仍转到 `LF2WeaponBase.HandlePreInteractionKind1`，属于 weapon
  target/pickup语义；
- `BruteForceSceneQuery.IsLeftPressed/IsRightPressed`当前只从 `LF2Character` 读取，因而 non-character
  attacker的kind1 candidate可达性需要独立追踪；
- 上述两项不是“target type writer gate”本身。它们被登记为 `D-COL-005B` 后续子范围，不能借本包直接
  影响 kind1/7 pickup或weapon流程。

## 5. 最小实现方向（PLANNED）

1. `BattleInteractionWriter.TryApplyGrab` 把 Character type rejection改为 `kind == 3` 专属；
2. 保留 kind1/3的 method-kind guard、raw writer、relation、duration、fall与snapshot顺序；
3. self-check通过正式 frozen candidate→character consumer→writer链测试：
   - kind1 character attacker + non-character target：candidate被记录且消费后产生 C++ case1 relation/frame；
   - kind3同类 target：仍由既有 C++ type gate拒绝，不产生 relation/frame；
4. 不改 weapon/special attacker可达性、selector、RNG、pickup、CPoint、held/link或render。

## 6. Unknown / stop conditions

- **UNKNOWN**：C++ runtime trace和真实 target DAT / Play Mode仍缺；
- 若为了测试而需改 kind1 selector、weapon kind1或pickup，停止并拆到 D-COL-005B；
- 若 non-character target在 generic writer后需要专有资源/生命周期动作，停止并由新的 target-type package处理；
- 任何 C++ / DAT / scheduler / central render改动均不在范围。

