# Task Contract — NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001

> 状态：`VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 / TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED / TYPE3_WRITER_NEXT`
> 依赖：`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 / VERIFIED`、
> `NTSD28-B4-REVIVAL-EXIT-AUDIT-001 / VERIFIED`。

## 目标

只纠正 B5 kind5/frozen-pair/negative-link consumer 的 linked-parent slot 绑定：Authority读取held attacker的
`linked_parent_slot`，Unity必须读取`Runtime.HolderStableId`并将未绑定/invalid值投影为native implicit slot0，
不得回退到legacy multiplexed `HolderCopySlot`。本包不删除HolderCopy carrier，不修改producer、stats、type3、
damage、group truth table或schema。

## Authority 与 Unity 原状

- Authority `BattleWorld28::rebuild_geometric_hit_candidates()`直接用attacker `linked_parent_slot`冻结
  `linked_holder_present/group`，并以同一slot处理negative-link nearest reference。
- Authority `resolve_standard_damage_interaction()`对kind5再次读同一`linked_parent_slot`并校验holder的
  `linked_child_slot == attacker.slot`后选择替代数据；不存在root-owner/HolderCopy fallback。
- Unity `ResolveReleaseNeutralHolderSlotOrImplicitZero()`只读`HolderCopySlot`；
  `ResolveReleaseNegativeLinkHolderSlotOrImplicitZero()`在`HolderStableId<0`时回退HolderCopy。
  frozen snapshot、BruteForce kind5/nearest/body和HitPlan kind5均经这两个helper消费错误slot。

## 允许修改

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
  - 两个release linked-holder helper改为`HolderStableId`/implicit-zero，移除HolderCopy读取。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind5LinkedParentSlotCorrectionEditorTests.cs`及`.meta`
- 仅为新exact-link合同修正的既有kind5 fixture：
  `Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs`、
  `Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitGroupEligibilityAtomicProductionEditorTests.cs`、
  `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。
- 本Task/Change与治理恢复文档。

## 不变量与排除

- 不改`HolderStableId`/`LinkState` producer、reciprocal validation、candidate顺序、kind5替代字段、group rule、
  RNG、damage、hit rest、owner/credit、type3 writer或任何HolderCopy producer。
- 不删除或重排HolderCopy runtime/ECS/snapshot/checksum/parity字段；carrier disposition仍需用户schema方向。
- 不修改content、Scene、Prefab、ProjectSettings、Authority目录或用户例外。
- type3额外HolderCopy writer由下一独立包退休；legacy damage stats与`+0x2F4`保持后置。

## Test-first 验收

1. RED必须证明当`HolderStableId != HolderCopySlot`时两个helper、frozen pair和kind5替代当前选择错误root；
   覆盖slot0/high slot、neutral implicit0、invalid stable id、不同group/frame。
2. 生产改动只触及两个helper；旧HolderCopy sentinel必须bitwise不变。
3. focused新测试、frozen-pair/hit-group/HitPlan/role-aware相关回归与build通过；full SelfCheck实际运行。
4. 真实`NTSD_Battle` Play执行exact-vs-root sentinel probe，Console0且Scene SHA/dirty/root不变。
5. 取得同一规范化linked-holder输出或等价逐字段证据后，才能恢复B5 linked-holder binding ready；不得据此恢复
   type3 whole-continuation或B5全域完成。

## 回滚

只恢复两个helper的旧HolderCopy读取并回退本包测试fixture；不得回退B4、exact relation producer或用户工作树。

## 实施与验证（2026-09-09）

- focused RED实际`0/5`：三个helper sentinel分别读出旧HolderCopy 7/7/5，frozen pair读错group15，kind5
  replacement读错root frame injury33；Authority期望为implicit0/0/slot77、group17、injury77。
- production只把两个helper的slot来源改为`Runtime.HolderStableId`，负值投影native implicit0；没有修改调用者、
  HolderCopy字段/producer、relation、group、candidate、RNG、damage或schema。
- 既有kind5 HitPlan fixture现显式声明`HolderStableId`；collision self-check使用exact holder并保留
  `HolderCopySlot=99`污染sentinel。unlinked slot0 frozen-pair旧测试按native implicit0更正为
  `present=true/group=attacker group7`。
- focused于05:23:51 +08实际`5/5`；相关回归分两批`34/34`与`246/246`，合计`280/280`，覆盖完整
  HitPlan、frozen-pair、atomic/pure group与role-aware collision。
- runtime build为`47 warnings / 0 errors`。并行editor build一次返回`47 warnings / 1 error`且详细输出被截断；
  随后串行复跑同一`Assembly-CSharp-Editor.csproj`为`104 warnings / 0 errors`，Unity程序集编译亦0 error。
- 05:29:02 +08真实`NTSD_Battle` Play执行5个exact-vs-root case通过；Console0，前后Scene SHA均
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`，`dirty=false`、root13且已退出Play。
- 05:30:04 +08 full SelfCheck仍提前失败于既有CPoint mode0 victim-Vz断言；本包直接focused/Play已覆盖，
  但不冒充full SelfCheck pass。
- 本包恢复B5 linked-parent consumer binding；旧hit-group family exit只恢复到该binding已纠正的范围。
  type3额外HolderCopy writer仍是下一包，stats/+0x2F4/carrier/schema继续后置。
- Change Ledger validator通过：`410 records / 342 governed code files`；本包新增路径无尾随空格。
