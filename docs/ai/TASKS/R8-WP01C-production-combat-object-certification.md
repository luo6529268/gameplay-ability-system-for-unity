# R8-WP01C — production combat object certification

> 日期：2026-08-23  
> 状态：`COMPLETE / 01-06 UNITY S4 VERIFIED / 07 SYNTHESIS COMPLETE`  
> 父任务：`R8-WP01-production-certification-orchestration`

## Goal

在当前 production Unity 战斗场景中，按 producer→consumer 的依赖顺序，为角色、武器、特殊攻击、
其他/效果对象及其生成、持有、投掷、抓取、碰撞、命中、死亡、复活和 late lifecycle 建立可重复的
Play Mode（S4）证据。认证只裁决实际执行到的对象、tick、slot、字段和可见状态，不把静态源码、
self-check 或旧性能报告扩大为 C++ runtime 完整对齐。

## Scope

1. 记录每个子流程的场景、角色/对象、DAT 前置、初始 slot、输入序列、逻辑 tick 和实际结果；
2. 观察 production producer 到 consumer 的完整链路，而非只确认对象“出现过”；
3. 对照已确认的 C++ release source contract，记录 slot/generation、frame、位置/速度、关系、
   HP/PP/耐久、统计、pending/active/destroy/reuse 和对象数量；
4. 每个 first-difference 绑定既有 D-ID；没有合适 D-ID 时建立新的差异条目；
5. 如果认证需要新增 probe/test 脚本，必须先建立独立 Change Record，再修改任何脚本。

## Authority / Evidence

- 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source；只读，不运行、构建、
  修改或写入；
- C++ source contract 只提供 S0；focused/self-check/joint fixture 分别提供 S1～S3；本任务目标是
  当前 Unity production scene 的 S4；
- `R1-WP02` full C++ trace 保持 `BLOCKED`，所以 S5 不可伪造；
- CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× visual scale、fixed-world camera、扩展实体容量、
  30 Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC 都是已批准且受保护的 Unity 适配边界。

## Work packages

### R8-WP01C-01 — opoint birth / newborn / basic lifecycle

**Goal**  
先认证后续所有交互依赖的对象出生、可见边界和基础生命周期。

**Scope**

- character、weapon、special attack、other/effect 的 opoint 生成；
- 出生 action frame、`Prev2=0`、runtime slot/generation；
- same-pass higher-slot 与 next-pass lower-slot 的可见边界；
- active/pending/destroy/reuse 和对象池复用后的字段重置。

**Primary evidence IDs**：`D-OP-001`、`D-SCHED-012`、`R5-LIFE-01B` lifecycle evidence。  
**Deliverable**：逐 tick 对象出生/消费/回收表和最小复现步骤。  
**Verification**：真实 `NTSD_Battle` Play Mode，至少覆盖四类 producer；记录 slot、generation、oid、frame、
对象数量和状态边界。  
**Stop condition**：任一 producer 无法到达 consumer、slot/generation/出生帧与 source contract 冲突，或需要改脚本。

### R8-WP01C-02 — pickup / held / throw / weapon landing

**Goal**  
认证世界武器从可拾取到 held、投掷、落地和解除关系的完整链。

**Scope**

- 随机/场景武器拾取；
- holder/held 关系与 CPoint/wpoint 逻辑坐标；
- type1/type2/type4/type6 的投掷差异、FrameDelay、spawner、速度和 link clear；
- 投掷武器落地，以及不得发生的 Unity-only immediate target hit。

**Primary evidence IDs**：`D-HOLD-001`、`D-HOLD-002`、`D-HOLD-003`、`D-COL-004B`、`D-MOV-004`。  
**Deliverable**：pickup→held→throw→landing 时序表。  
**Verification**：逻辑坐标、关系和状态用 S4 记录；像素挂点/前后层级只登记给 `R8-WP01D`，不在本包裁决。  
**Stop condition**：01 未通过，或关系/速度/落地 first-difference 需要 gameplay 修复。

### R8-WP01C-03 — grab / CPoint / link / held injury

**Goal**  
认证抓取、双向关系、CPoint/WeaponSync、held 伤害和逃脱/投掷尾链。

**Scope**

- valid grab、reciprocal mismatch、duration decrease/negative escape；
- throw/dircontrol tail、weapon-sync phase injury/stat ownership；
- positive/negative link invalidation residue；
- first-held → CPoint/weapon sync → positive link → second-held 的 pass 顺序。

**Primary evidence IDs**：`D-SCHED-001`～`004`、`D-LINK-001`～`002`、`D-CPT-001`～`005`、`D-INP-001`。  
**Deliverable**：关系字段与 pass 边界的逐 tick 表。  
**Verification**：至少一组正向抓取与一组断链/逃脱负向流程。  
**Stop condition**：02 依赖未通过，或出现 cross-pass/order first-difference。

### R8-WP01C-04 — collision candidate / hit / damage / abort

**Goal**  
认证角色、武器和特殊攻击的候选、消费、命中、伤害、统计和 abort 语义。

**Scope**

- character/weapon/special attacker 与对应 victim；
- multi-candidate ordering、abort、`HitConfirm2`、caught/hurtable gate；
- effect21 state18/19 abort、kind1 generic target/reachability；
- raw frame response、HP/HP max、statistics、durability、vrest。

**Primary evidence IDs**：`D-COL-001`～`003`、`D-COL-005A/B`、`D-HIT-001`～`004`。  
**Deliverable**：candidate order→consume→field mutation 的 first-difference 友好报告。  
**Verification**：每类 attacker 至少一条正向命中；需要 gate/abort 的条目必须有负向 witness。  
**Stop condition**：候选顺序、RNG、abort 或字段首写时点出现差异，或正式 DAT 可达性仍为 UNKNOWN。

### R8-WP01C-05 — death / respawn / integer state / AI boundary

**Goal**  
认证致死到 cleanup、复活和 AI 输入边界的完整生命周期。

**Scope**

- lethal damage→death state→cleanup；
- 不得提前执行的 AI self-HP exclusion；
- 从预期 integer snapshot 计算 respawn coordinate；
- relation/holder/target cleanup 和复活后的 runtime 状态。

**Primary evidence IDs**：`D-INP-002`、`D-MOV-003` 及相关 lifecycle joint 条目。  
**Deliverable**：致死、清理、复活的逐 tick 状态表。  
**Verification**：至少一个 AI/角色完整死亡复活流程。  
**Stop condition**：04 的伤害前置未通过，或复活夹具受默认 stage.dat 缺失阻塞。

### R8-WP01C-06 — random weapon / late special / effect chain

**Goal**  
认证 random weapon、late special 和效果对象的 slot/RNG/lifecycle 顺序。

**Scope**

- random weapon source ordering 与生成；
- state9995→4000→8000 reload chain；
- state9996 的 4×217 + 1×218；
- slot/RNG order 和 exhaustion 行为。

**Primary evidence IDs**：`D-LATE-001`、random weapon/effect/lifecycle 条目。  
**Deliverable**：random/late producer 与对象消费序列。  
**Verification**：只使用当前场景或明确测试夹具；默认 `stage.dat` 继续排除。  
**Stop condition**：需要部署默认 stage.dat、改变 RNG/order、或修改 gameplay。

### R8-WP01C-07 — synthesis

**Goal**  
汇总 01～06 的证据，不掩盖失败或未覆盖项。

**Deliverables**

- 每个子包为 `PASS / FAILED / BLOCKED / NOT RUN`；
- 每个 D-ID 的最高证据层、报告文件、最小复现和未验证项；
- 新发现的 first-difference 及独立 repair WP 建议；
- 明确 `R8-WP01D` 负责中央像素、阴影、透明排序和挂点可见性；
- 明确 C++ full trace 仍 `BLOCKED`，不得宣称完整 C++ runtime 对齐。

## Execution order and isolation

固定顺序：`01 → 02 → 03 → 04 → 05 → 06 → 07`。01 的生产对象出生/回收是后续所有包的前置；
02～06 只能在依赖通过后执行。只读 source/文档准备可以并行，但会改变同一 Play world 的场景流程必须串行，
每组前恢复明确基线，避免前一组对象、slot、RNG 或关系残留污染下一组。

## Acceptance

- 每个实际结论都包含 scene、initial state、input、tick、slot/generation、expected、actual 和证据路径；
- producer 与 consumer 都被观察到，仅“对象生成了”不算通过；
- 正向、负向和边界 witness 按条目需要齐全；
- Console/compile/self-check 结果单独记录，不替代 Play Mode；
- 发现 production 差异后只登记并停止该子包，不在认证包内顺手修复；
- 修复必须另建 Task Contract 和 Change Record，并获得适用授权。

## Stop conditions

- 需要修改 gameplay、input、collision、held/link、opoint、render、pool、profile、scene 或资源；
- 需要运行、构建、修改或写入 C++ authority；
- 需要改变 approved Unity adapter、pass ordering、长期架构或验收标准；
- first-difference 指向当前子包之外的模块；
- 测试资源会触碰 T8 默认 stage.dat 或 Android 真机边界。

## Out of scope

- 修复认证中发现的差异；
- CentralOnly 像素、阴影、透明排序与挂点视觉认证（归 `R8-WP01D`）；
- 1000 实体性能、0 GC/capacity（归 `R8-WP01E`）；
- Windows Player/IL2CPP（归 `R8-WP01F`）；
- T8 默认 stage.dat、Android 真机、服务器、C++ executable/trace 替代方案。

## Approval boundary

本文档只完成 `R8-WP01C` 规划。首个可执行包是 `R8-WP01C-01`。在用户明确批准启动 01 前，
不得进入 Play Mode 认证、创建 probe 脚本、修改 gameplay/scene 或自动继续 02～07。

## Execution update

`R8-WP01C-01`已由用户批准并完成：`R8-OPLIFE-001 / VERIFIED`，fresh compile、W05 8/8、full
self-check和live production Play S4均PASS。`R8-WP01C-02`已于2026-08-23由用户明确批准并进入执行，
Task为`R8-WP01C-02-pickup-held-throw-landing-execution.md`，Change ID为`R8-HOLDPLAY-001`。
03～07仍未开始；02发现production first-difference时只登记repair WP并停止，不得顺手修复。

`R8-WP01C-02`现已完成：`R8-HOLDPLAY-001 / VERIFIED`（仅Unity S4）。四type pickup/held/throw/landing、
no-immediate-hit、cleanup与fresh验证均通过。用户已于2026-08-23明确批准`R8-WP01C-03`；独立执行合同为
`R8-WP01C-03-grab-cpoint-link-held-injury-execution.md`，Change ID为`R8-GRABPLAY-001`。03当前只允许
Editor-only live certification probe；发现production首差必须另拆repair。

`R8-WP01C-03`现已完成：`R8-GRABPLAY-001 / VERIFIED`（仅Unity S4）。valid grab/injury/stats、
mismatch throw、escape dircontrol/postprocess、positive/negative link residue和四pass表均在worker-active
clean Play通过；production0改动。下一包为04 collision/hit/damage/abort，必须独立授权和治理。
no-immediate-hit、cleanup、fresh compile、focused23/23、full self-check和治理均PASS。下一包是03
grab/CPoint/link/held injury，状态`APPROVAL PENDING`；不得自动进入。

`R8-WP01C-04`已由用户批准并完成：`R8-HITPLAY-001 / VERIFIED`（仅Unity S4）。10-candidate live
matrix覆盖character/weapon/special positive、HitConfirm2/caught/effect21、kind10 raw frame和
character→random no-op→object顺序；fresh compile、focused、clean Play cleanup、13:19:39 self-check和
治理均PASS，production0改动。当前hit-plan mode=Disabled、worker inactive，ShadowCompare/worker-active/
C++ full trace仍未取得。下一包05为`APPROVAL PENDING`，不得自动进入。

## 2026-08-23 continuous authorization supersede

用户已明确要求“直接推进 WP01C 剩余的三项即可，不需要我批准”。因此，上述逐包 approval pending
口径自本条起被 supersede：`R8-WP01C-05 → 06 → 07` 按固定依赖顺序连续执行，不再逐包等待批准。
这项授权不扩大既有 scope：发现 production first-difference、需要 gameplay repair、改变长期架构/
approved adapter、修改 scene/资源，或触及 C++ authority 的运行/构建/写入时，仍必须按 Stop conditions
登记并停止相应路径。当前进入 `R8-WP01C-05`。

`R8-WP01C-05`现已完成：`R8-DEATHPLAY-001 / VERIFIED`（仅Unity S4）。HP=0 AI、state14 countdown、
no-count/stored/free、stale integer/RNG、OID998与cleanup矩阵通过；production0改动。按连续授权直接进入06。

`R8-WP01C-06`现已完成：`R8-LATEPLAY-001 / VERIFIED`（仅Unity S4）。natural random、live五子、
synthetic full chain、authority400 exhaustion与cleanup通过；production0改动。当前进入07 synthesis。

`R8-WP01C-07` synthesis现已完成，见`R8-WP01C-07-synthesis.md`。01～06均PASS/Unity S4，六个认证
Change均VERIFIED且production gameplay0改动；未关闭D-ID、WP01D/E/F/G和C++ full trace边界已显式列出。
