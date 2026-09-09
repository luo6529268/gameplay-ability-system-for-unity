# Task Contract — NTSD28-B6-NTSDSPEC-MASS-CARRIER-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CHARACTER_MASS_RETIREMENT_PACKAGE_DEFINED / FORMAL_OUTPUT_UNCHANGED / PRODUCTION_HELD`
> 来源：`NTSD28-B6-NTSDSPEC-PRODUCTION-OWNER-INVENTORY-AUDIT-001 / VERIFIED`

## 目标

冻结Unity旧character mass载体的完整reader/writer/snapshot范围、正式语料可达性与退休包。只处理
`LF2Character.Initialize()`的`NTSDSpec.GetMassOrDefault`链；dead `LF2Entity.FluteForce()`中的另一个mass
lookup仍属impact/old-API retirement，不在本包混入。本审计不修改脚本、content、Scene或Authority。

## Authority 与Unity路径

- Authority `PhysicsIntegrator28::step()`在pre-step integer Y
  `position.y >= collision_y_reference`时无条件对motion X/Z各向零移动一单位；没有mass字段、默认值或gate。
- playable closure全`*.cpp/*.h`对独立`mass`单词零命中。
- Unity `CharacterMechanics.StepBattleLogic()`把同一grounded predicate再与`ctx.mass > 0`相与；这是额外规则。
- `LF2Character._mass`初始化为`NTSDGlobal.Default.Machanics.Mass=1f`，并在`Initialize(characterId)`从
  NTSDSpec重写。它只经`MassForFrameAdvance`传给character mechanics；唯一行为reader就是上述friction gate。
- generic current-character-DAT shell路径已经固定传default mass1，因此只有real `LF2Character`保留该额外状态。

## 正式可达性与当前影响

- NTSDSpec非空mass只属于旧ID100/101/150/201/202/213；当前与release catalog中这些均不是type0
  character定义（部分ID缺失或已指向完全不同对象）。
- 旧表中的type0 IDs 1与30均`Mass=null`；其余当前type0 IDs不在表中。因此production factory按object type
  创建的所有正式`LF2Character`都得到1f，grounded friction gate恒真。
- 正式当前/release输出没有被旧mass数值改变；差异只可由reflection/test把`_mass`设0/负值，或错误地用
  non-character ID初始化character shell触发。它仍是无Authority依据的latent state，必须退休，不能因恒真而保留。
- `_mass`不进入主lockstep checksum/parity字段；但character shell snapshot捕获/恢复它。若synthetic改变该值，
  当前能改变未来physics却不在常规checksum中完整表达，进一步证明它不应是正式规则状态。

## 完整Unity改动owner

`NTSD28-B6-NTSDSPEC-MASS-CARRIER-RETIREMENT-PRODUCTION-001`必须原子覆盖：

- `CharacterMechanicsContext.mass`字段和constructor参数；
- `CharacterMechanics.StepBattleLogic()`的`mass > 0`条件，保留原grounded snapshot predicate并无条件friction；
- `LF2Character._mass`、`MassForFrameAdvance`、Initialize lookup与snapshot restore赋值；
- `BattleEcsCharacterFrameAdvancePass`、real character与generic shell的context构造；
- `BattleCharacterShellSnapshot.Mass`、capture/restore及buffer schema；
- 所有production/test/self-check constructor调用和character-shell snapshot tests。

删除snapshot成员属于结构变化，`BattleWorldCharacterShellSnapshotBuffer.CurrentSchemaVersion`必须从1升2；
不得把旧buffer默认为兼容，也不得仅停止restore而继续capture无Authority字段。该buffer为进程内预分配对象，
没有磁盘迁移需求；旧schema在validation处按既有机制拒绝。

## 验收矩阵

- 先加RED：grounded runtime在旧context mass=0与负值时仍应执行native unit friction；旧代码必须失败。
- 改后grounded X/Z正负/小值/零、negative collision reference、blocked axis、airborne和landing边界保持B4证据；
  不改变既有epsilon实现或vertical rules。
- real `LF2Character`、generic current-character-DAT shell、DataOriented/Legacy profile相同输入同结果。
- current type0 roster全量初始化结果与改前一致；错误non-character ID不再改变character physics。
- reflection architecture guard确认`_mass`、`MassForFrameAdvance`、context mass和snapshot Mass均不存在；
  NTSDSpec character mass调用归零，dead Flute lookup仍由单独记录追踪。
- character shell schema=2，capture/restore/identity mismatch/zero-allocation tests通过；全snapshot restore与checksum
  broad tests不回退。
- isolated compile、focused B4/B6/lockstep、NTSD28、SelfCheck及ground movement Play；runtime前最多
  `RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不改unit-friction数值/epsilon、collision reference、gravity/landing、weapon physics、impact、held action、
  NTSDSpec其他字段、content/Scene/Authority。
- 不顺手删除`NTSDGlobal.Default.Machanics.Mass`或`NTSDSpec.cs`；它们在剩余引用归零后另行处置。
- 当前两个B6代码包尚未进入Unity Test Runner，本production包保持held；不重试已穷尽license路径。

## 回滚

本审计仅文档；回滚移除Task/Change/Ledger/STATE/handoff/总表增量。

