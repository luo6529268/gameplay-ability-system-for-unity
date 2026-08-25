# R8-WP01G-R07B — central liveness / identity / visibility joint runtime certification

> 建立日期：2026-08-23  
> 状态：`COMPLETED / UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`  
> D-ID：`D-RENDER-003`、`D-RENDER-004`、`D-RENDER-005`

## Goal

以C++ Release active selection、shadow gate和slot-stable painter合同为规则，在真实CentralOnly Play中证明：
pending实体退出对应central capture；late opoint/replacement只在C++对应的下一次RenderDispatch可见；同槽
generation不会复活旧command；current-DAT OID223/224正确关闭shadow；production visibility writer不会额外
隐藏仍应由C++显示的body/shadow。OID7/8→51 dormant/split的正式producer与同槽恢复由独立R08负责，R07B
只消费其未来证据，不重复制造merge/split Play。

## Scope

### 允许

1. 只读复核C++ `game_tick.cpp`、`renderer.cpp`与entity lifecycle；
2. 使用正式`data.txt`中OID223、224及现有DAT，不新增或修改DAT；
3. 复用WP01C-01 opoint/release/generation与WP01D-06/07 central diagnostic/pixel能力；
4. 必要时在批准后先建test-only Change Record，再增加一个Editor-only联合Play probe；
5. 通过正式producer形成状态：next/destroy形成pending，opoint/pool形成replacement generation，正式
   OID223/224实体形成shadow gate，death/effect/hit-stop形成visibility变化；
6. 记录每tick active/query/slot/generation/snapshot/command/resource/segment/submitted/pixel与cleanup；
7. exact shell/current-DAT mismatch继续使用既有self-check，Play只证明current-DAT真实对象路径；
8. fresh compile、focused、self-check、Play、Console0、ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不直接写`PendingFlushDestroy`、`OidMergeDormant`、generation、EntityVisible/ShadowVisible或调用
  `Hide()/HideShadow()`制造主要Play结果；
- 不硬编码角色技能来绕过统一OID/DAT/lifecycle合同；
- 不修改DAT、scene、URP asset、CentralOnly/Texture2DArray/Mesh、camera、visual scale或capacity；
- 不恢复Legacy pixel owner；
- 不处理R07A writeback、R07C fail-closed ownership、P1/P2、AI、T8、IL2CPP、Android或服务器。

## Authority / Evidence

- C++ `src/render/renderer.cpp:517-685,1300-1438`；
- C++ `src/entity/game_tick.cpp:1008-1154,2061-2083`；
- Unity `SimulationWorld.Passes.partial.cs::Oid5152RuntimeMaintenanceAll`；
- Unity registry/pending/free/runtime slot generation；
- Unity `BattlePresentationShadowBuild` capture/current-DAT/visibility gates；
- `Assets/NTSD/Config/data.txt`：OID7/8/51/223/224均为正式条目；
- existing W05 opoint lifecycle、presentation reuse、OID5152与P7 identity/visibility self-check；
- WP01D-06/07 Game/SceneView central evidence。

## Acceptance

1. actual pending entity在对应RenderDispatch不进入snapshot/command/pixel；
2. dormant partner/同槽split恢复不在本包重复执行；该子分支最终证据必须来自独立R08。R08未完成前，
   `D-RENDER-003`只能标为pending/generation/T+1子集通过，不能整体关闭；
3. actual late opoint/replacement在T帧冻结后不污染T，在T+1/T+2对应边界首次出现；
4. stale handle/old generation不能解析为current command或submission；
5. formal OID223与224实体的body可按frame gate显示，但shadow command/pixel不存在；普通control有shadow；
6. exact shell/current-DAT双向identity self-check继续PASS；Play不可达时不得伪造mismatch；
7. death/effect/hit-stop/pool reuse期间EntityVisible/ShadowVisible与C++ own gates相同，不产生额外隐藏；
8. same-Z painter order与slot tie-break不因state移除/恢复改变；
9. cleanup恢复world/slot/pool/driver/central plan，Console0；fresh compile/focused/self-check/ledger PASS。

## Files likely involved

- 现有opoint/lifecycle/central Play probe（优先扩展或复用）；
- 如确有必要：一个新的Editor-only联合Play probe及`.meta`；
- Task/Research/Change Record/Ledger/STATE/register/main plan/handoff；
- production gameplay/renderer默认不应修改。

## Unknowns

- 正式场景当前角色/技能是否自然触发OID223/224；若不能，可在测试初始spawn边界通过正式factory按OID生成，
  但不得伪造current-DAT identity mismatch；
- dormant/split producer、可达性与pixel证据全部由R08 Task Contract裁决，R07B不得复制该fixture；
- production EntityVisible独立false writer当前inventory为空；若真实流程始终只通过C++等价death/destroy gate，
  结论应为source-closed，不得添加新writer只为测试。

## Verification

1. source/writer/reachability crosswalk；
2. existing W05、OID5152、presentation begin-frame、identity/visibility focused matrix；
3. actual lifecycle→central command→isolated pixel Play报告；
4. full `BattleRuntimeSelfCheck`；
5. `Tools/Validate-ChangeLedger.ps1`与scoped diff。

## Stop conditions

- 必须直接写结果字段才能形成witness；
- current DAT/scene无法形成某分支且正式factory也不可用；
- 发现production first difference；
- 需要修改production但没有独立repair Task/Change Record；
- 需要改变pool/generation、pass order、CentralOnly或其他保护边界；
- 只剩C++ full trace且R1-WP02仍BLOCKED。

## Out of scope

R07A、R07C、R08 OID7/8→51 merge/dormant/split、AI、P1/P2、debug keys、T8、IL2CPP、Android、服务器、
C++ executable/full trace。

## Authorization

用户已于2026-08-23明确批准执行`R8-WP01G-R07B`并恢复总目标。实施只允许覆盖本合同的
pending/generation/T+1、formal OID223/224 current-DAT shadow identity和production visibility联合证据；
已建立test-only Change Record `R8-RENDERLIVE-001`。本批准不授权执行R08；`D-RENDER-003` dormant子分支
必须等待R08独立批准和证据。

## Execution result — 2026-08-23

- 新增的联合probe只属于Editor/test；production gameplay、renderer、DAT、scene和URP均为0改动；
- Play报告PASS：pending old `slot51/gen1`经FrameLogic free，Late OID999以`slot51/gen2`复用；
  T冻结帧拒绝old/new，T+1只接受new generation并恢复body/shadow visibility；
- 正式OID223/224 body均进入central snapshot/command/resource/submission；shadow snapshot存在但因
  current-DAT gate为`CommandSuppressed`，无shadow command/submission；baseline正式角色保留body/shadow；
- 223/224正式DAT在tick内Z不同，实际顺序符合`ZInt→slot→stableId`；同Z slot tie focused PASS；
- focused 24/24 + 9/9 + worker18/18、full self-check、final Console0、ledger validator全部PASS；
- 本包只把`D-RENDER-003`的pending/generation/T+1子集提升到Unity联合S4；dormant/split仍归R08；
- 证据：`RESEARCH/R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。
