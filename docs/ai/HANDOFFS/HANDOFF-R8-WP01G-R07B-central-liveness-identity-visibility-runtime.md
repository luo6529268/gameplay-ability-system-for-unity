# HANDOFF — R8-WP01G-R07B central liveness / identity / visibility

> 日期：2026-08-23  
> 状态：`COMPLETED / UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`

## Preflight result

- `data.txt`正式包含OID223、224，不需要改DAT；
- pending、generation、death/effect/hit-stop均有production producer；
- R07B禁止直接写结果字段或调用Hide/HideShadow制造PASS；
- exact shell/current-DAT mismatch继续由既有self-check负责，Play只证明真实current-DAT OID223/224路径；
- ordinary central Game/SceneView pixels已经S4，R07B只补特殊liveness/identity/visibility边界。
- 原合同曾同时要求R07B验证OID7/8→51 dormant/split又把R08列为out-of-scope，现已纠正：merge/dormant/
  split只由R08独立认证；R07B只处理pending/generation/T+1。R08未完成前不得整体关闭D-RENDER-003。

## Next action after approval

先建test-only Change Record，再判断能否复用现有WP01C/WP01D probe；现有opoint lifecycle可提供
pending/release/generation producer，central Game/SceneView probe可提供command/pixel观察。只有缺少联合观察点
时才新增一个Editor-only probe。发现production first difference立即拆repair包，不在认证脚本中修gameplay。

## Active execution

- 用户已批准执行R07B并恢复总目标；
- 现有probe只能分别给出producer/lifecycle或central submission证据，不能输出同一tick/handle/generation的
  联合first-difference，因此允许新增一个Editor-only联合probe；
- 脚本写入前已登记`R8-RENDERLIVE-001 / IN_PROGRESS`；production gameplay、DAT、scene和renderer保持不改；
- dormant/split仍只归R08，本包最多关闭`D-RENDER-003`的pending/generation/T+1子集。

## Completed evidence

- `R8-RENDERLIVE-001 / VERIFIED / TEST-ONLY`；production代码0改动；
- fresh Play PASS：tick202→203，pending `slot51/gen1`释放，late OID999同槽`gen2`；T冻结不受late污染，
  T+1只解析新generation；pool/slot/world/driver cleanup全部恢复；
- OID223/224 body snapshot/command/resource/submission均通过；shadow按current-DAT gate为
  `CommandSuppressed`且没有geometry/submission；baseline正式角色body/shadow均提交；
- actual Z order符合`ZInt→slot`；focused同Z slot tie通过；
- focused jobs：24/24、9/9、worker18/18；21:47:26 self-check PASS；final Console error0；
  Change Ledger 83 records / 98 governed code files PASS；
- 完整证据见`../RESEARCH/R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。

## Remaining boundary

- `D-RENDER-003`只关闭pending/generation/T+1子集，整体仍等待R08 dormant/split；
- R07C `D-RENDER-001`仍未执行；
- R1-WP02 C++ full trace仍BLOCKED；不得把本包扩大为整个render handoff或整个战斗系统完全对齐。

## Protected boundaries

C++只读；不改DAT/scene/URP；不回退Legacy；CentralOnly、Texture2DArray、Mesh、1.5×scale、fixed camera、
capacity、30Hz、FrameInputSet、SoA/ECS、worker、pool/0GC保持。
