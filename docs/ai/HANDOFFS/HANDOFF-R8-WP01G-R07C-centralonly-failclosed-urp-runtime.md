# HANDOFF — R8-WP01G-R07C CentralOnly fail-closed URP ownership

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY S4 CLOSED TO AVAILABLE EVIDENCE`

## Preflight result

- existing self-check已经有cold→ready→last-good→replacement exact ownership矩阵；
- WP01D-06/07证明真实Game/SceneView current central pixels存在；
- current code暴露Editor-only feature registration和stale failure boundary，可在不改URP asset的情况下设计Play；
- R07C必须在finally恢复原feature/material/draw mode，且任何状态都保持Legacy suppression；
- live URP自动重注册和cold全局状态仍是执行时unknown，不能用破坏scene/renderer asset的方式绕过。

## Next action after approval

先建test-only Change Record；优先复用现有ownership API和WP01D isolated pixel工具。若cold无法安全形成，
保留exact self-check并诚实标注cold Play未运行；current→stale→replacement仍可独立验收。

## Execution start

- 用户已于2026-08-23明确批准R07C；
- 已在脚本写入前建立`R8-CENTRALOWN-001 / IN_PROGRESS`；
- 允许的唯一代码范围是新Editor-only Play probe及meta；production renderer/gameplay/URP asset保持0改动。

## Execution result

- current/stale/replacement：真实URP Play 259 pixels、相同hash、owner/tick/gen/lease/checksum/cleanup PASS；
- cold：exact self-check PASS，Play未运行；
- focused29/29、full self-check、compile0、ledger84/99 PASS；
- final Play Console存在active submission后late capacity seal resize异常，故R07C不能VERIFIED；
- 已建立R07C-R01 repair合同，未获批准前停止，不进入R08。

## Closure update（2026-08-23）

历史blocker已由获批的R07C-R01关闭。最终normal Play保持`ScenesCamera.enabled=true`且Console0；R07C
current/stale/replacement三态、cold exact self-check、checksum/cleanup与1000/0GC非回归全部PASS。
下一步不自动进入R08；C++ full trace仍是独立blocked evidence层。

## Protected boundaries

C++只读；不改URP/scene/material asset；不回退Legacy；CentralOnly、Texture2DArray、Mesh、1.5×scale、
fixed camera、capacity、30Hz、FrameInputSet、SoA/ECS、worker、pool/0GC保持。
