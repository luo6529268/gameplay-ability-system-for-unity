# NTSD28 B5 Type3 Kind-Catalog Transform Audit

## 1. Authority identity and playable path

- 正式 EXE：`B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。
- playable closure：`39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`。
- runtime `resources/runtime/decoded_dat/data/kind.dat`：229 bytes、18行、SHA-256
  `39E30DF8D86A5FC374B26C80BE358A6503B2C3096D8681C586478D73A0900011`。
- `game_session.cpp:1263+`优先解析正式runtime文件；缺失时使用同值
  `KindCatalog28::locked_runtime_table_2833()`，解析失败则启动失败；`build_step_options()`把catalog传入正式tick。
- `kind_catalog.cpp`、`hit_candidates.cpp`、`battle_world.cpp`均直接进入
  `source/ntsd28_playable/scripts/build.ps1 -Target playable`的core source闭包。

## 2. Exact locked record

| field | value |
|---|---|
| effect | 209 |
| frame | 40（运行分支仍规定0时回退40） |
| bound | 8, 209, 213 |
| respond | 200, 203, 205, 206, 207, 215, 216 |

文件顺序有语义：transform按record顺序扫描并在第一个`bound(attacker) && respond(target)`处停止。
Unity冻结`Assets/NTSD/Config`没有`kind.dat`，但上述10个OID均在`data.txt`存在。当前对齐可使用正式
one-record locked rule，无需覆盖Config；未来若用户在H选择整体release内容权威，再独立决定通用parser/部署。

## 3. Two different consumers

### 3.1 Candidate gate — currently equivalent

`hit_candidates.cpp::classify_kind_table_candidate`在普通kind/effect/type gate前执行：target必须是type3且OID等于
record.effect(209)，attacker OID在record.respond中时，除kind9外全部拒绝。Unity
`BruteForceSceneQuery.IsBlockedReleaseOidInteraction`使用当前data OID实现同一locked集合与kind9例外；当前缺的是
直接focused matrix，不是已确认的production行为差异。

### 3.2 Type3 transform — confirmed difference

`battle_world.cpp:6877+`仅在target current frame存在、state!=3005、attacker object_type==3，并命中
`bound(attacker OID) && respond(target OID)`时执行：

1. 先只清target pending impulse total，保留contribution count和当前motion。
2. group、owner直接复制attacker；negative-link parent不参与本分支。
3. target definition pointer、object_id、object_type全部复制attacker。
4. action/action_latch/previous_action写record frame（0回退40），frame_counter=0，special-hit latch=true。
5. 不写control slot、HolderCopy、weapon count、current motion或contribution count。
6. 随后仍执行type3 matching-state pair reset、motion-hold release和公共effect override。

Unity当前`ApplyKind0Type3Tail`虽能识别bound/respond集合，但事务不等价：先按parent复制
RelationTeam/HolderCopy、清Knockback与Runtime XYZ；固定选20/30；OID209才直接变209，OID8/213则扫描active OID209
并错误变成209；`TryApplyRuntimeIdentity`额外重载weapon_hp。HitPlan又拆成D1/active-D1/standard三套投影，继承相同旧行为。

## 4. Existing carrier mapping

| Authority write | Unity carrier | status |
|---|---|---|
| battle_group | `RelationTeam` | ready |
| owner_slot | `Runtime.OwnerSlotIndex` | ready |
| definition/object_id/type | attacker `FrameCache.Wrapper` + target `ObjectId`/identity metadata | ready；必须直接复用attacker wrapper，不依赖active OID209或catalog resolver |
| action | `Frame.N` + `Runtime.Frame` | ready |
| action_latch | `FrameTransistor.WaitCounter` | B0 verified |
| previous_action_078 | `Frame.Prev` | B0 verified |
| frame_counter | `AttackingCounter` | ready |
| special_hit_latch | `HitConfirm2` | B0 verified |
| pending total/count | `Knockback XYZ` / `HitCount` | ready |

HitPlan现有snapshot已覆盖ObjectId/DataObjectId/DataObjectType、RelationTeam/OwnerSlot、Frame/Prev/Wait、
Attacking、HitConfirm2、Knockback/Runtime velocity、HolderCopy、AnimCounter和WeaponCount；无需新增shadow字段。

## 5. Implementation split

下一单包`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001`：

- 以正式locked one-record decision统一bound/respond/effect/frame常量；candidate gate保持当前顺序并补矩阵测试。
- 新建单一actual transform transaction：预检source wrapper/frame40后再原子写入；保留HolderCopy、AnimCounter、
  WeaponCount、Runtime XYZ、HitCount与Prev2。
- HitPlan在D1/active-D1/state-sync之前识别并投影同一事务，退休/旁路两套OID209专用identity oracle。
- 覆盖bound 8/209/213、全部respond、negative-link direct-attacker ownership、state3005 skip、frame fallback、
  identity source不依赖active209/config resolver、pair reset后置顺序、missing source fail-closed与shadow parity。
- 不修改DAT/Config/Scene、真正kind9、generic continuation、effect override或pair-reset算法本体。

## 6. Implementation result

`NTSD28-B5-TYPE3-KIND-CATALOG-TRANSFORM-001 / VERIFIED`已完成：actual与HitPlan现统一复用attacker
definition/id/type并直接复制attacker group/owner，写action/latch/Prev40，只清pending total；保留HolderCopy、AnimCounter、
WeaponCount、Runtime XYZ、HitCount、PN和Prev2。matching-state pair reset仍后置，effect override projection改读转移后的
definition。旧active209扫描helper已退役。证据为focused190、B5+HitPlan367、exact101/742、SelfCheck/compile/CS0、
Scene/Ledger PASS。下一`NTSD28-B5-TYPE3-POST-HIT-EXIT-AUDIT-001`。
