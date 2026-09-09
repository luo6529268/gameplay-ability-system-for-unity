# Task Contract — NTSD28-B6-HELD-WPOINT-DORMANT-RULES-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TERMINAL_CORRECTED_CURRENT_REACHABLE / DORMANT_RULES_UNION_RECONFIRMED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001 / VERIFIED`

## 目标

继续审计 `BattleWorld28::settle_held_refill_objects()` 的 WPoint follow/release 尾部，区分当前正式路径、
Unity generic synthetic fallback、只在未来内容可达的规则差异，并冻结 terminal、cover=2、DVX Vz 与
damaged-frame extra branch 的后续 owner 边界。

## Authority 与 Unity 对照

### terminal weapon action

- Authority 在写 child action、frame/facing/位置之前判断 `parent_wpoint.weapon_action >= 1000`，直接通过
  `despawn(child_slot)` 移除 child 并清 world relation；不消费 RNG，不继续 DVX/kind3。
- Unity real/generic 都先把 `WeaponAct` 写进 child frame；没有 terminal structural branch。无对应 frame 时
  仍可能继续以空 frame/default WPoint 更新位置或 release。
- 该规则需要 world/registry structural owner，不能在 `DirectWriteHeldFramePreserveWaitCounter()` 内把 1000
  当普通 frame 或只清 link。

### cover=2 follow pose

- Authority 先把 child Z 设为 holder Z；仅当 `cover != 2` 时才执行 cover0 的 `Z+1/Y-1` 或其他 cover 的
  `Z-1/Y+1`。cover=2 保持同 Z、同计算 Y。
- Unity `BattleHeldObjectWriter.SyncHeldFrameAndPosition()` 与
  `LF2WeaponHeldStateResolver.ApplyHeldWPointSync()` 都把所有非零 cover 当 `Z-1/Y+1`，漏掉 cover2 no-offset。

### DVX depth velocity

- Authority type1/2/4/6 DVX release 仅在 depth-up XOR depth-down 时写 child Vz；无键、双键或 `dvz=0`
  都保留入场 Vz。
- production `LF2WeaponHeldStateResolver.ThrowHeldWeapon()` 已符合该合同。
- generic `BattleHeldObjectWriter.ThrowHeldObject()` 先无条件 `Vz=0`，只在 XOR 时覆盖；但 production pool 对
  type1/2/4/6 一律实例化 `LF2Weapon`，snapshot restore 也恢复为 weapon shell，因此 generic 分支当前仅由
  synthetic fixture 以非 weapon entity + 伪装 object type 进入。

### damaged-frame extra drop

- Authority WPoint follow 写 child `weapon_action` 后直接进入 facing/anchor/DVX/kind3；没有依据 child action
  state12/18随机0..15、复制 holder motion并提前清 relation 的分支。
- Unity real `LF2WeaponHeldStateResolver.Act()` 与 generic `BattleHeldObjectWriter.RunStep12()` 都在 follow 后
  检查 child action state12/18并执行这条额外 drop。
- 该分支是 legacy/synthetic extra，不能作为 Authority fallback；但当前与 release 所有被 WPoint 引用的
  type1/2/4/6 action 都没有 state12/18，因此现有内容不触发。

## 新鲜语料测量

以完整 WPoint block 读取并用各 corpus `data.txt` 的 type1/2/4/6 定义解析 referenced action state：

| Corpus | WPoint | distinct weaponact | weaponact>=1000 | cover=2 | type1/2/4/6 definitions | referenced state12/18 matches |
|---|---:|---:|---:|---:|---:|---:|
| Unity Direction B | 312 | 16 | 0 | 0 | 29 | 0 |
| 2.8 release decoded | 38163 | 100 | 0 | 0 | 20 | 0 |

因此 terminal、cover2、damaged-frame extra 在当前与 release 内容均 dormant；generic Vz 又不在当前
production class closure。它们是规则/未来内容安全差异，不能冒充当前可观察首差，也不能反过来写成已对齐。

## 后续四包

1. `NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-OWNER-AUDIT-001`：先冻结 Unity world/registry 的安全 despawn
   owner、同 tick slot reuse、relation cleanup、iteration continuation，再决定 production 实现。
2. `NTSD28-B6-WPOINT-COVER2-POSE-PRODUCTION-001`：real/generic 两个 pose writer 统一 cover2 no-offset；
   以 synthetic exact rule 验证，不修改 content。
3. `NTSD28-B6-GENERIC-DVX-VZ-PRESERVE-PRODUCTION-001`：仅修 generic fallback 的 no-XOR/dvz0 preserve，
   real weapon path保持；先用source guard证明production closure不变。
4. `NTSD28-B6-LEGACY-DAMAGED-HELD-DROP-RETIREMENT-001`：删除 real/generic state12/18 extra drop及误导性
   tests，证明当前/release corpus结果不变；不得删除其他明确 authority 的 weapon hit/landing drop。

上述包都排在现有 refill、kind3、weapon-HP、+0x2F8、relation lifecycle runtime 栈之后。terminal 还依赖
structural owner audit，不能直接实施。

## 验收矩阵

- terminal：slot0/high slot、child despawn、parent/third-party relation cleanup、无 RNG、同 tick next child继续、
  generation/reuse与ordered shutdown安全。
- cover：0/1/2/10逐项 X/Y/Z/facing/frame 对照；cover2精确无偏移。
- generic Vz：up/down/none/both × dvz zero/nonzero，sentinel preserve；real weapon output bitwise不变。
- damaged extra retirement：synthetic referenced state12/18不再draw/复制motion/清relation；current/release
  referenced-action corpus guard保持0；普通 DVX/kind3仍按各自规则 release。
- 每包独立 compile/focused/B6 regression/SelfCheck；涉及 structural 的 terminal 包必须加 Play 和
  same-tick reuse witness。在运行证据前不得标记 `VERIFIED`。

## 不变量 / 阻塞

- 不改 content/Scene/Prefab/importer、Authority、capacity、pass placement、refill、kind3、weapon HP、+0x2F8、
  catch relation或lifecycle cleanup。
- 不因为 dormant 就删除规则差异，也不因为 synthetic fixture 可造就声称 production 可达。
- 当前 Unity Test Runner/SelfCheck/Play 栈仍阻塞；本审计不修改脚本。

## 回滚

仅移除本治理记录；没有代码、content、Scene 或 Authority 回滚。

## Current corpus correction（2026-09-08）

current WPoint312/terminal0是单行漏计。正式projection全量7995；具备kind2 pickup的39个holder definitions含
7366 primary WPoint和28条`weaponact>=1000`。因此terminal不再dormant，立即提升为current reachable structural
owner audit；cover2与referenced state12/18仍为0，后两项dormant结论保留。generic Vz owner不受计数影响。

## Held relation producer-domain correction（2026-09-08）

OPoint kind2 direct-link使完整held union扩大为41 source definitions / 7,624 primary WPoint；terminal=33、
kind3=811、non-kind3 DVX=186。原39/7,366/28/770/184是pickup-only子集。terminal owner已闭合；
cover2与referenced state12/18降为union recheck，未复核前不再声称完整held域dormant。

## Full-union dormant recheck closure（2026-09-08）

`NTSD28-B6-WPOINT-MISSING-ACTION-CONTINUE-OWNER-AUDIT-001`已在41-source/668-edge union上复核：
source WPoint `cover=2`为0；所有target已声明且被引用action的state12/18匹配为0。故cover2与damaged-drop
恢复`DORMANT_CURRENT_AND_RELEASE`；terminal仍current reachable，generic Vz仍synthetic fallback规则差异。
