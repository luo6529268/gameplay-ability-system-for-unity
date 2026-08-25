# HANDOFF — R8-WP01G-R07A render pass / hit-record writeback joint runtime

> 日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE`

## Preflight result

- G4没有新发现的source-confirmed未实现production差异；缺口是特殊分支的联合运行证书；
- G4已拆成R07A writeback、R07B liveness/identity/visibility、R07C CentralOnly fail-closed ownership；
- R07A先处理`D-SCHED-009 + D-RENDER-002`，因为hit-record tail/count会影响下一tick kind0追加和两次RNG；
- Unity当前已把publication/no-publication writeback放回RenderDispatch，existing exact self-check覆盖
  age `[0,5,38,39]`、unavailable与幂等边界，但没有actual hit producer的同tick Play联合报告；
- WP01D-06/07只证明普通central command→draw→Game/SceneView pixel链存在，不自动关闭spark writeback。

## Next action after approval

先建立test-only Change Record；优先复用WP01C-04 actual collision/hit路径，只补pass内只读观察点。
若无需新增脚本即可形成结构化联合证据，则不创建probe。发现production first difference时停止并另建修复包。

## Resume checkpoint

- 用户已批准R07A并恢复目标；
- source/crosswalk复核确认production实现顺序未出现新的静态差异；
- existing worker lifecycle+full-capacity RNG exact baseline 2/2 PASS；
- `R8-HITWRITEBACK-001`已在脚本写入前建立，Editor-only完整tick Play witness现已写入；最终状态见下方
  Completion result的`VERIFIED`；
- 首次实际asset导入的test-only CS0102与后续CS0165均已修复，fresh compile为0 error；下一步执行
  结构化Play报告，不修改production。

## First Play correction checkpoint

- 场景未就绪的tick0调用已排除；场景就绪后从tick1510进入真实worker tick；
- 首轮实际报告只失败于probe把Unity-only`LastAdvanceTick`误当成presentation writeback合同；
- source确认正式writeback按C++规则推进age/tail，不维护该诊断字段；cleanup各项均恢复；
- 下一步只移除这条test-only越界断言，保留age/cycle/RNG/Late幂等验收并重跑，production不修改。
- 删除该断言后的worker Play已过actual hit/age/cycle前置；新FAIL是probe误查未物化的纯worker publication。
  下一步等待正式中央宿主写入`CurrentPixelFramePlan.CapturedFrame`后再验commands，不调用self-check
  materializer，仍不修改production。
- 正式central captured frame等待修正后，tick1018 actual hit/RNG/frozen-live age/command/Late已全PASS；
  第二tick同一pair受hit-rest抑制。下一步预建独立攻击者并逐tick启用新pair，不清rest、不直接写hit。
- 独立attacker共享victim仍受victim侧资格抑制；下一步改为4组预建独立pair，旧record继续参与每个
  render/no-publication cycle，仍不清rest/状态、不写hit。

## Completion result

- 4组独立pair最终production worker Play报告PASS；tick843～845发布1/2/3 owner/record/command，
  tick846 no-publication保持cycle845并把live ages推进为`[4,3,2,1]`；
- 每tick exact 2 RNG，Late幂等，warmed allocation violation delta0；world/slot/pool/RNG/stats/sounds/
  presentation owner/pause全部恢复；
- compile0；worker18/18、hit178/178、central13/13；20:25:11 full self-check PASS；final Console0；
  ledger82/97与scoped diff-check PASS；
- `R8-HITWRITEBACK-001 / VERIFIED`，但D-ID只到
  `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；R07B/R07C/R08未执行。

证据：`RESEARCH/R8-WP01G-R07A-render-writeback-joint-runtime-evidence-20260823.md`。

## Protected boundaries

C++只读；CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5× scale、fixed camera、扩展容量、30Hz、
FrameInputSet、SoA/ECS、worker、pool/0GC均保持。AI、P1/P2、T8、IL2CPP、Android和服务器不进入R07A。
