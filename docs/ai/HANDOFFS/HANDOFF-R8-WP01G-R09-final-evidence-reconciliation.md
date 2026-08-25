# HANDOFF — R8-WP01G-R09 final evidence reconciliation

> 日期：2026-08-24  
> 状态：`COMPLETE AT APPROVED UNITY EVIDENCE / DOCUMENT-ONLY`

## Current position

- R05 candidate/PreInteraction、R06 P1/P2 physical input、R07A/B/C central handoff/ownership、R08 OID7/8→51
  merge/split均已达到各自限定的 Unity S4 证据层；
- R08-R03 已关闭 negative-height body self-check 分类阻塞；
- R08-R04 已让 production DAT fixture使用当前catalog路径，fresh compile0、focused1/1、完整self-check PASS；
- 当前没有新观察到的 production gameplay first difference；
- 但父编排、68项登记册和部分 synthesis 仍包含被后续证据取代的旧 pending/blocked 文本，需要正式对账。

## Planned package

`R8-WP01G-R09-final-evidence-reconciliation.md` 是纯文档、无运行、无脚本修改的最终证据对账包。它不会
自动把所有 `RUNTIME_PENDING` 改成 `VERIFIED`，也不会把 Unity S4 证据写成 C++ runtime full-trace 证书。

## Approval boundary

用户已于2026-08-24明确批准`R8-WP01G-R09`并恢复目标。本包现只执行已批准的文档证据对账；任何脚本、
资源、运行或范围扩大仍不在授权内。

## Persistent boundaries

- `R1-WP02` full trace：`BLOCKED`；
- T8默认 `stage.dat`：暂缓；
- AI C++ parity：用户取消，未来走Unity状态树/行为树并保持FrameInputSet边界；
- F1/F2调试步进：用户明确不需要；
- IL2CPP：用户明确不处理；
- Android与服务器：不属于当前包；
- C++ authority：只读，不运行、不构建、不修改、不写入。

## Final reconciliation

- register 68项与R09分类集合一致：43/5/1/9/1/3/6，missing0、extra0、duplicate0；
- `D-LIFE-001`、`D-RENDER-003`已更新为Unity joint S4；
- `D-SCHED-008`、`D-SCHED-010`、`D-STEP-001`保留source difference并按用户决定退出normal-combat backlog；
- `R8-SPRITERANGE-001`完成normal Play、R08、full self-check与validator证据对账，升级VERIFIED；
- 没有新的normal-combat production gameplay修复项；脚本、scene、config、resource、C++改动和新运行均为0。

## Final wording boundary

允许表述：`R8在批准范围和当前可取得Unity证据层完成`。

禁止表述：`整个C++ executable runtime已取得full-trace完整对齐证书`。R1-WP02 full trace仍BLOCKED；
五个exact DAT/fixture witness、人手硬件edge、F7～F9 policy、T8、AI未来设计、Android和服务器均按记录保留。

Final verification：68/68、missing0、extra0、duplicate0；Ledger validator 93/111 PASS；scoped diff check PASS。

## Post-R09 correction — 2026-08-24

R09是当时的证据快照，后续R11/R12已取代其中三条pending表述：

- 正式weapon/object state2000 Play witness已PASS；不存在的type0 state2000样板不再等待；
- 恢复资源中8个authored state8xxx已重新确认，OID32 state8032 Central full-tick Play已PASS；
- F7/F8/F9已按GameConfig exact mode和LocalFreeRun fixed-tick边界实现，focused、snapshot回归、Play与
  full self-check均PASS。

最新恢复点应读取R11/R12 handoff与STATE尾部；R1-WP02、T8和用户排除项边界不变。
