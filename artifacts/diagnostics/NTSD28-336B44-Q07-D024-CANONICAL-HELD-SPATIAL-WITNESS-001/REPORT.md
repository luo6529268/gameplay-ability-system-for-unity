# Q07/D-024 canonical 非武器持有挂点比例原场景见证

状态：`VERIFIED_SCOPED_CANONICAL_HELD_RATIO / WPOINT_PARENT_RUNTIME_PENDING`。规则权威仍是 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根正式 EXE 与对应 playable live source；原版背景和模式 DAT 按用户例外排除，画面 X/Z 采用已批准 D-024 统一比例。

正式内容 `OID8` 小樱在受控自然 frame256→257 的相对 tick6 生成 `OID420/type3` 子体并持有，tick28 与 `OID7` 李合体；OID420 的 Unity 类是 `LF2SpecialAttack`，持有时经过 `BattleHeldObjectWriter.RunStep12/SyncHeldFrameAndPosition`，与 LF2WeaponBase 的写者不同。本包只给既有 C043 原Battle Scene 请求式探针加小樱与子体的源规则/物理X/Z及初始化标志，接受唯一 `late-double-tap-d024-spatial-04` runId；未改生产写者、DAT、场景或输入。

[新场景结果](../NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/late-double-tap-d024-spatial-04.json) 为 `NATURAL_GATE_PASS`、完整60tick、原Battle Scene退出clean且借用0。真正持有且双方源坐标已初始化的 tick7～27 共21tick，子体与小樱的物理相对X/Z对源整数相对X/Z分别乘 `2048/1333`、`1152/730`；[逐tick计算](held-ratio-rows.csv)最大绝对误差X=`0.915228807202`、Z=`0.232876712329`输出像素，21/21均小于1。tick7源相对X=-9/画面-14.742686/目标-13.827457，源相对Z=-1/画面-1.810959/目标-1.578082；整数锚点解释该小于1像素的误差。tick6出生尚未走完首轮held-refill，tick28已融合，二者不混入21个持有采样。

本轮新JSON与[先前同一输入场景结果](../NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/late-double-tap-scene-03.json) 的原有22字段×60tick共1320/1320原值相同、零差；先前 C043 报告已有当前336B44源码与旧场景所选17字段×60tick：原值1015/1020，5差仅前5tick未建立关系的空槽哨兵0/-1，按已声明语义归一为1020/1020；根正式EXE的所选14字段×60tick为840/840。

随后从现有[根正式EXE trace](../NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001/root-01/root-trace.jsonl)发现其已含子体逐tick位置。当前根 EXE SHA重新核为`336B44…7BD3`，两次根trace字节SHA同为`C37FC492…04CE1`。根回放tick0小樱Z400、tick1起被载体放到Z542，原Unity受控Scene源Z400；只对Z扣去已观测的载体偏移142，未调整X。独立[逐项比较](root-unity-spatial-comparison.json)检查60tick小樱/子体存在性各60格、小樱X/Z各27格、OID420 X/Z各36格，共**246/246零差**；子体存在tick6～41，真正持有tick7～27。根回放报告标记`nativeParityClaim=false`，故这只证明指定LFR、初态、输入及所列坐标，不证明全部原生场景或全World。WPOINT父包与Q07仍保持开放。

验证：生成Editor工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q`退出0、253警告、0 error，日志见[原件](dotnet-build.log)；原项目Unity Editor刷新后Play并回到idle/nonPlay/noncompiling。结果记录`sceneCleanAfter=true`、`poolActiveAfterExit=0`；Battle/Menu/GameConfig/Mode Asset、旧场景JSON、当前正式源码CSV六保护SHA见[前](protected-before.json)/[后](protected-after.json)，6/6不变。旧Temp请求先复制为[副本](request-before.json)，仅写新请求并由探针消耗，不删除原件或覆盖旧结果。交付检查见同ID Change Record。
