# R4-COL-01 — C++ `hit_confirm2` character-target attacker abort

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（source、static、existing Unity Editor compile 和 full self-check已通过；仍缺 C++ runtime trace 与 Play Mode。）  
> 顶层目标：`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R4。  
> 唯一行为 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source。  
> 对应差异：`D-COL-001`。  
> 前置调查：`RESEARCH/R4-COL-01-hit-confirm2-attacker-abort-preflight-20260822.md`。

## Goal

在不改变 candidate collection、pass 顺序或消费者类型分派的前提下，使 Unity 的统一冻结
candidate consumer 按 C++ `collision.cpp:57-65` 复现：vrest/current-target recheck 通过后，
若 attacker 的 `HitConfirm2` 非零且当前 candidate target 的**当前 DAT type**为 character，则停止该
attacker 的剩余 candidate sequence，且不进入 runtime ITR replacement 或任何 hit writer。

## Scope

允许仅修改：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs`
   - 在 `TryConsumeCandidate(...)` 里插入 C07-A gate；
   - 仅调整 `CanConsumeRecordedCandidate(...)` 的读检查位置，以复现 C++ 的 vrest → abort 顺序；
   - 使用既有 `true` = sequence break 合同，不新增 consumer、接口、world pass 或缓存。
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
   - 新增 exact 与 shared-character-DAT 的 two-candidate focused fixture；
   - 在 `RunAllChecksStatic()` 注册该 fixture。
   - 若既有 kind5 fixture 因 C07-A 暴露“holder 先命中其持有武器”的未隔离前置条件，允许只将
     holder 设为 collect-time `AttackExempt>0` 并断言 weapon `HitConfirm2==0`；不得改其生产 writer。

禁止：

- 修改 C++ runtime、C++ source、可执行文件、资源、配置或向 authority 写入；
- 修改 `NTSDBattleTickSystem`、`SimulationWorld` pass 顺序、candidate collection、vrest decrement、
  candidate store、CPoint、WeaponSync、held/link、opoint、render/central renderer、容量、pool/worker；
- 同包处理 D-COL-002～005、D-HIT-001～003、R5+、T8、服务器或性能重构。

## Authority / Evidence

### VERIFIED — C++ release source

- release `Makefile` 包含 `collision.cpp` 和 `game_tick.cpp`；
- `collision.cpp:57-65`：vrest current-candidate skip 在前，`hit_confirm2 + target obj_type==0`
  whole-attacker abort 在后；
- `collision.cpp:1257`：`next_attacker` 在 pair loop 外；
- `collision.cpp:1263-1266`：Loop1/Loop2 共用该实现。

### VERIFIED — Unity source

- shared `BattleHitCandidateSequenceRunner` 当前未读取 `HitConfirm2`；
- `CanConsumeRecordedCandidate` 是非写入的 target/vrest consume recheck；
- `ResolveRuntimeItrForPair` 的现有实现不写 world/entity，但可能做 shallow copy，因此必须放在
  C07-A abort 之后；
- current-DAT type 应使用 `target.GetCurrentDataObjectTypeForSimulation()`，不能用 CLR class。

### UNKNOWN / RUNTIME_PENDING

- C++ executable trace 不可用（R1-WP02 BLOCKED）；
- real skill-triggered `HitConfirm2` Play Mode 时点未验证。

## Files likely involved

| 文件 | 责任 |
|---|---|
| `Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs` | 唯一 shared consume gate 实现。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | 最小冻结 candidate exact/shared 回归夹具。 |
| `docs/ai/CHANGE-RECORDS/R4-COL-001.md` | 改动、证据、失败和回滚记录。 |
| `docs/ai/CHANGE-LEDGER.md`、`STATE.md`、差异总台账、handoff、总计划 | 生命周期留痕与最终状态。 |

## Deliverables

1. C07-A 共享 gate 的最小代码改动；
2. exact/shared two-candidate fixture；
3. source/static evidence、existing Unity Editor scripts refresh/compile、full self-check、ledger validator、
   `git diff --check` 实际结果；
4. 更新 Change Record、ledger、STATE、总差异台账、主计划和 handoff。

## Verification

| 层级 | 验收条件 |
|---|---|
| S0 source order | 在保留既有 `ItrIndex` 防御性有效性检查的前提下，正常有效 candidate 的 Unity target resolve → vrest/validity → C07-A abort → runtime ITR 顺序可静态定位；没有其他 `HitConfirm2` consumer gate。 |
| S1 exact fixture | exact character attacker 在 two-character frozen range、`HitConfirm2=1` 时不 dispatch 任一 candidate。 |
| S2 shared fixture | shared character-DAT fallback attacker 得到同一 sequence-abort 结果。 |
| S3 preservation | candidate collect count 保持两条；`HitConfirm2=0` 的对照仍连续命中两条，证明没有把正常 multi-hit 全局关掉。 |
| S4 regression | existing Unity Editor scripts refresh/compile、full `BattleRuntimeSelfCheck`、ledger validator、`git diff --check` 通过。 |
| S5 boundary | 最高仅 `RUNTIME_PENDING`；C++ runtime trace / real Play Mode 仍明确未关闭。 |

## Stop conditions

- 找到 C++ release source 中比当前已记录更早的 `hit_confirm2` writer/read gate，且会改变本合同的
  前置顺序；
- 发现 `ResolveRuntimeItrForPair` 或 `CanConsumeRecordedCandidate` 有未被识别的 writer，导致仅移动读检查
  会改变 C++ 不可见状态；
- two-candidate fixture 需要修改 scheduler、candidate collection、CPoint、held/link 或 data contract 才能成立；
- existing self-check 失败且修复需越出本包范围；
- 需要回退 Unity 已批准的 CentralOnly、Texture2DArray、容量、30 Hz、FrameInputSet、SoA/pool 边界。

## Out of scope

R1-WP02、C++ runtime 执行、D-COL-002～005、所有 D-HIT、R5～R8、T8 default `stage.dat`、服务器、
Android、长时间性能与 Play Mode。

## 实施进度（2026-08-22）

- 已在 shared runner 中把既有 `CanConsumeRecordedCandidate(...)` 移到 runtime ITR resolution 前；它失败时
  仍只 skip 当前 candidate。
- 已在此 reader 成功后插入 `HitConfirm2 != 0 && current DAT type == Character` 的 sequence break；
  既有 `TryConsumeCaptured(...)` 会把该 break 映射为当前 attacker 的 candidate loop `break`。
- 已新增 exact / shared、abort / clear 的四个 two-candidate cases，并注册在 full self-check；
  代码尚未取得 Unity compile 或运行证据。
- 第一次 full self-check（03:40:54）在既有 `CheckHeldKind5ConsumesFrozenCandidates` 失败。C++ source
  表明该 fixture 的 holder 与 held light weapon 同 relation 时，holder 的 kind0 candidate 合法；holder
  先消费后会将 held weapon `hit_confirm2=1`，随后本包 C07-A 正确阻止 held weapon 对 character target。
  因而仅允许增加 collect-time `holder.AttackExempt>0` 来隔离 kind5 replacement，并在 object consume 前
  断言 held carrier 为0；这不是 production behavior 回退。

## 实际验证结果（2026-08-22）

- **source/static**：C++ `collision.cpp:57-65` 的 valid-candidate vrest skip → `hit_confirm2` character-target
  attacker abort → runtime ITR / dispatch顺序，和 1257 的 pair-loop 外 label 已复核；Unity shared runner现在
  只有一处 C07-A reader，且当前 DAT type而非 CLR class参与判断。
- **focused fixture**：新增 exact/shared 两路、`HitConfirm2=1` abort与`HitConfirm2=0` normal continuation
  四种 two-candidate case；full self-check PASS表示它们均完成断言。
- **existing regression correction**：首次 full self-check的 held-kind5 failure经 C++ candidate/consume source
  复核确认是fixture未隔离 holder→held weapon合法写入，已仅在fixture collect前设 holder `AttackExempt=1`，
  并断言 object consume前 weapon carrier为0。生产 runner没有为此加入豁免。
- **Unity compile / full self-check**：existing Unity Editor（MCP port 6401）03:44:14 +08:00 refresh/compile
  后 ready；`Temp/NTSD_BattleRuntimeSelfCheck.result=PASS`，最后写入 2026-08-22 03:44:57 +08:00。
- **边界**：没有运行、修改、构建或写入 C++ authority；没有做 C++ trace或 Play Mode。因此本包仅为
  `RUNTIME_PENDING`。
