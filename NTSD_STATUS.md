# NTSD 复刻项目 — 当前状态总览

> 最后更新：2026-03-01
> 分支：`NTSD_GAS`

---

## 一、整体架构

```
ILF2Object
└── LF2LivingObject          ← FLF: livingobject.js（基类）
    ├── LF2Character          ← FLF: character.js（主工作区）
    │   ├── LF2Character.cs                    核心：构造/初始化/武器/抓取
    │   ├── LF2Character.Generic.partial.cs    通用状态处理器（TU/Transit/Frame/Combo）
    │   └── LF2Character.States.partial.cs     各状态具体 Handler（Standing~Burning）
    └── LF2WeaponBase
        ├── LF2LightWeapon
        └── LF2HeavyWeapon
```

**Tick 生命周期（对齐 FLF）：**
```
SimTU()    → TUUpdate() → StateUpdate("TU_force") → StateUpdate("TU")
                        → ProcessEffects()  → ItrRest.Tick()
SimTransit → Transit()  → ComboUpdate()
                        → Trans.Trans()    → StateUpdate("transit")
```

---

## 二、完成度矩阵

| 模块 | 完成度 | 备注 |
|------|--------|------|
| 状态机框架（state 0~18 注册） | ✅ 100% | — |
| 连招/方向/移动（state 0/1/2） | ✅ ~90% | Rudolf DJA 未实现 |
| 跳跃/冲刺（state 4/5） | ✅ ~85% | — |
| 抓取系统（state 9/10） | ✅ ~80% | throwinjury 落地结算缺失 |
| 爬起/防御/受伤（state 6/7/8/11/16） | ✅ ~85% | — |
| **受击系统（`LF2Character.Hit()`）** | ✅ 95% | 主逻辑完整；音效/VisualEffect 仍为 TODO |
| **Fall/倒地（state 12/14）** | ❌ 10% | 见第四节 |
| HP/MP 计算 | 🟡 50% | `Injury()` 已实现（HP / HPLost / HPBound）；缺 HP/MP 自然恢复 |
| 特效系统（VisualEffect/BrokenEffect） | ❌ 5% | 两个方法内全是 TODO |
| OPoint 生成（`ObjectPointModule`） | 🟡 40% | 调用被注释掉 |
| 声音系统 | ❌ 0% | TODO 注释占位 |

---

## 三、受击系统架构修正（最高优先级）

### 3.1 当前错误架构

```csharp
// LF2Character.cs — OnGenericStateEvent
case "hit":
    return Generic_Hit(eventData);  // ← 错误：FLF 中不存在此模式
```

```csharp
// LF2Character.Generic.partial.cs — Generic_Hit
private bool Generic_Hit(object eventData)
{
    // ... 全部注释掉 ...
    return true;  // ← 完全失效，打击无响应
}
```

### 3.2 FLF 原版模式（`character.js:1893`）

FLF 的 `hit()` 是 **prototype 方法**，与 state_update 完全无关：

```js
character.prototype.hit = function(ITR, att, attps, rect) {
    // 1. vrest 冷却检查
    // 2. state 10（被抓）/ state 14（躺地）/ state 19（火免）前置分流
    // 3. kind 5000-5999 / 6000-6999 NTSD 特殊段
    // 4. kind 0/4/9 主流程（防御判定 + fall() + posteffect()）
    // 5. kind 10/11（笛力）/ kind 15（旋风）/ kind 16（冰冻）
    // 6. accepthit → itr_vrest_update → injury(inj)
    // 7. return accepthit ? inj : false
}
```

调用方式：`target.attacked(target.hit(ITR, attacker, pos, vol))`

FLF 的 `attacked()` 只做攻击统计记录，不含伤害逻辑。

### 3.3 正确的 C# 架构

```
LF2LivingObject.Hit()     ← 基类：仅 vrest 检查（已实现）
      ↓
LF2Character.Hit()        ← override：包含全部 hit 逻辑（待实现）
      ↓
LF2Character.Injury(inj)  ← HP 扣减 + hp_lost 记录（待实现）
```

**必须从 `OnGenericStateEvent` 删除 `"hit"` 分支**，`Generic_Hit` 方法整体废弃。

### 3.4 `LF2Character.Hit()` 需实现的完整流程

```
1. vrest 冷却检查（基类已有）
2. 前置状态分流
   ├── state 10 (BeingCaught)   → caught_cpointhurtable() 决定是否接受
   ├── state 14 (Lying)         → 直接 return false（躺地无敌）
   └── state 19 + att.state==3000 → return false（火焰奔跑免疫）
3. NTSD 特殊 kind 分支
   ├── kind 5000-5999  → HP -= (kind - 5000)，不进入 fall
   └── kind 6000-6999  → 跳转到帧 (kind - 6000)，需检查帧有效性
4. kind 0/4/9 主流程
   ├── 计算 attdir、compen、ef_dvx、ef_dvy、effectnum
   ├── 冰冻(state 13)免疫 effectnum==30
   ├── 燃烧(state 18/19)免疫 effectnum==20/21
   ├── state 7 正面防御分支
   │   ├── injury *= GC.defend.injury.factor
   │   ├── bdefend 累加
   │   ├── bdefend > DefendBreakLimit → Trans.Frame(112, 20)
   │   └── 否则 Trans.Frame(111, 20)，ef_dvx 减弱，ef_dvy=0
   └── 非防御分支
       ├── 重武器掉落（丢弃条件）
       ├── inj += ITR.injury
       ├── bdefend = 45（立即失去防御能力）
       └── fall()
5. fall() 局部逻辑
   ├── health.fall += ITR.fall ?? GC.default.fall.value
   ├── state 13 (Frozen)        → falldown()
   ├── ps.y < 0 || ps.vy < 0   → falldown()（空中）
   ├── hp - inj <= 0            → falldown()
   ├── fall 0~20  → Trans.Frame(220, 20)
   ├── fall 20~30 → Trans.Frame(222, 20)
   ├── fall 30~40 → Trans.Frame(224, 20)
   ├── fall 40~60 → Trans.Frame(226, 20)
   └── fall > FallKO            → falldown()
6. falldown() 局部逻辑
   ├── ef_dvy = GC.default.fall.dvy（若 ITR.dvy 未定义）
   ├── health.fall = 0，ps.vy = 0
   └── 正面(front) ? Trans.Frame(180,21) : Trans.Frame(186,21)
       特例：front && dvx<0 && bdefend>=60 → Trans.Frame(186,21)
7. kind 10/11 → FluteForce()；kind 15 → WhirlwindForce(rect)
8. kind 16    → Trans.Frame(200, 38)，inj = ITR.injury
9. posteffect(effectnum)
   ├── effectnum 0/1 → 掉落武器（倒地时）+ visualeffect
   ├── effectnum 2/20~23 → 掉落武器 + Trans.Frame(203,36) 燃烧
   ├── effectnum 3/30 → 掉落武器 + 冰冻(200)/碎裂(182) + 音效
   └── effectnum 4    → 掉落武器
10. accepthit → itr_vrest_update → injury(inj)
11. return accepthit ? inj : false（C# 中改为 bool）
```

---

## 四、Fall/倒地系统（State 12/14）待实现清单

### State 12（Falling）— `LF2Character.States.partial.cs:1239`

| 事件 | 状态 | 需实现内容 |
|------|------|-----------|
| `frame` | ❌ TODO | 基于 vy 的动画状态机（上浮/下落帧序列切换） |
| `TU` | ❌ TODO | fall 值递减 + 倒地无敌时间管理 |
| `combo` | 🟡 框架有 | 补充 fall 值 + HP 检查后才能起身 |
| `fell_onto_ground` | ❌ TODO | 落地判定（爬起 vs 躺地，基于总速度 + throwinjury） |
| `transit` | ❌ TODO | 爬起逻辑（速度系统） |

### State 14（Lying）— `LF2Character.States.partial.cs:1375`

| 事件 | 状态 | 需实现内容 |
|------|------|-----------|
| `state_entry` | ❌ TODO | fall/bdefend 重置、死亡检测、NPC 死亡闪烁 |
| `state_exit` | ❌ TODO | 30 帧无敌 + 闪烁效果 + super 状态 |

---

## 五、Generic_TU 待实现清单

> 文件：`LF2Character.Generic.partial.cs:119`

| 序号 | FLF 对应行 | 内容 | 状态 |
|------|-----------|------|------|
| 1 | 56-82 | 消失效果状态机 | ❌ TODO |
| 2 | 84-102 | 死亡闪烁效果 | ❌ TODO |
| 3 | 145-149 | HP 自然恢复（每 12 帧） | ❌ TODO |
| 4 | 152-160 | 治疗效果（每 8 帧） | ❌ TODO |
| 5 | 163-167 | MP 自然恢复 | ❌ TODO |
| 6 | — | fall/bdefend 随时间恢复 | ❌ TODO |

---

## 六、其他零散 TODO

| 位置 | 内容 | 优先级 |
|------|------|--------|
| `State_Falling:combo` | 起身后最小速度 vx=5*sign, vy=5*sign, vz=2*sign | 中 |
| `State_Frozen:state_exit` | 冰块碎裂特效（ID 212，音效 1/066） | 中 |
| `State_Burning:frame` | 持续燃烧每帧特效（ID 302） | 中 |
| `State_Catching:frame 240` | Rudolf 变身（id_update 机制） | 低 |
| `Generic_Combo` | Rudolf DJA 变身检查 | 低 |
| `LF2LivingObject.VisualEffectCreate` | 视觉特效实际创建 | 中 |
| `LF2LivingObject.BrokenEffectCreate` | 碎裂特效实际创建 | 中 |
| `Generic_Frame` | OPoint 生成（调用被注释） | 中 |
| `Generic_Frame` | MP 消耗空分支 | 中 |
| `LF2CharacterStateModule` | 纯占位符，实际功能未接入 | 低 |
| `Injury()` | 补充 hp_lost / hp_bound 字段 | 中 |

---

## 七、实施顺序建议

```
阶段 1（战斗闭环）
  ① 删除 OnGenericStateEvent 中的 "hit" 分支 + 废弃 Generic_Hit
  ② 实现 LF2Character.Hit() override（完整 FLF hit 逻辑）
  ③ 实现 Injury()（HP 扣减 + hp_lost + hp_bound）

阶段 2（倒地系统）
  ④ State 14 state_entry / state_exit
  ⑤ State 12 fell_onto_ground / TU / frame

阶段 3（完整性补全）
  ⑥ Generic_TU 恢复逻辑
  ⑦ 特效系统（VisualEffect/BrokenEffect）
  ⑧ OPoint 生成恢复
```

---

## 八、关键 FLF 参考位置

| 内容 | FLF 文件 | 行号 |
|------|---------|------|
| `character.prototype.hit` 完整实现 | `character.js` | 1893-2130 |
| `character.prototype.injury` | `character.js` | 2136-2145 |
| `character.prototype.attacked` | `character.js` | 2160-2168 |
| `fall()` / `falldown()` / `posteffect()` 局部函数 | `character.js` | 2024-2120 |
| State 12（Falling）完整逻辑 | `character.js` | 963-1089 |
| State 14（Lying）完整逻辑 | `character.js` | 1113-1138 |
| Generic TU 恢复逻辑 | `character.js` | 54-183 |
