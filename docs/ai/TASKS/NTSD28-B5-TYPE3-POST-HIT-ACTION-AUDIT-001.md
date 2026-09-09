# Task Contract — NTSD28-B5-TYPE3-POST-HIT-ACTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / IMPLEMENTATION_SPLIT_DEFINED`
> 依赖：`NTSD28-B5-KIND0-DIRECT-POST-EFFECT-ACTION-001 / VERIFIED`

## 目标

闭合正式 NTSD 2.8-Logan 的攻击者 type3 post-hit action 与 type3 target continuation，逐项映射 Unity
actual/shared/hit-plan 的动作选择、归属复制、冲量清理、kind transform 和后续顺序。本审计只拆分可独立实施的
Change，不修改战斗脚本、内容、Scene 或权威目录。

## Authority 合同入口

- `battle_world.cpp::apply_native_type3_post_hit_action(...)`
- `battle_world.cpp::apply_native_unarmored_attacker_post_hit(...)`
- `battle_world.cpp::apply_native_reduced_attacker_post_hit(...)`
- `battle_world.cpp::resolve_relation_hit(...)` 的 `0x0042F72B..0x0042FC78` type3 target continuation
- `kind_catalog.cpp`、`kind_catalog.h` 与正式 `resources/runtime/decoded_dat/data/kind.dat`

## Unity 审计入口

- `Assets/NTSD/Scripts/Animation/LF2FrameData.cs`
- `Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- 相关 focused Editor tests 与 `BattleRuntimeSelfCheck`

## 验收

- 区分攻击者 post-hit 与 target type3 continuation，不把两个动作写入合成一个包。
- 记录 state/cover、hit_Fj/hit_Uj、fallback、selected-frame dvx、frame counter 与调用顺序。
- 记录 state3005 skip、pending impulse total/count、battle group/owner/control、special-hit latch 与 holder resolution。
- kind catalog transform 必须按正式 bound/respond/frame/effect 数据审计，不能把旧 OID 特判直接视为等价。
- exact/shared/hit-plan 的现状和缺失字段必须分别记录。
- Direction B 下不覆盖 DAT/PNG/WAV、Prefab、Scene 或 importer。

## 回滚

删除本审计新增文档行即可；本包没有生产脚本或内容修改。

## 结果

- 攻击者侧 confirmed difference 可独立实施：Unity 四个 actual tail 与四个 hit-plan 投影把 state3000 固定写
  action 10，部分还把 frame10 `dvz` 写入 Z；authority 使用当前帧 `hit_Fj`（0 回退 10）并把新动作帧
  `dvx` 原样写入 Z，同时还定义 state3007 + frame-level cover 2/3 gate。
- `LF2FrameData`/converter 缺 frame-level `cover` carrier；正式 runtime corpus 有 902 条 `state: 3000` 命中、
  其中 187 条同帧带非默认 `hit_Fj`，故固定 action 10 是现实可达差异。state3007 共 21 条命中，但未发现
  同帧 cover 2/3，当前内容可达性与规则承载分开记录。
- target-side generic continuation 也不等价：Unity 固定 20/30，没有读取 target 当前帧 hit_Fj/hit_Uj；
  relation helper 只复制 team/holder-copy，缺 owner-slot/control-slot 的精确事务；motion reset 还错误清除
  runtime motion 和 attacking counter，而 authority 只清 pending impulse total、保留 contribution count。
- kind transform 不能由旧 OID 209/213/8 特判证明等价。正式 kind.dat 是一条 bound={8,209,213}、
  respond={200,203,205,206,207,215,216}、effect=209、frame=40 的数据记录；Unity 没有通用 kind catalog。
- 后续顺序固定为：先实施攻击者侧动作；再补 target generic 所需 owner/control/impulse carrier；最后独立实现
  kind-catalog transform。effect8..16 override 与 kind0 direct post-effect 保持在 type3 continuation 之后。
