# 变身失败与 P2 受击框不同步诊断（2026-10-06）

状态：CONFIRMED_GIZMO_ADAPTER_GAP / USER_REPRO_PENDING。用户本轮请求定位未对齐部分；本轮没有修改生产脚本或 DAT，没有启动 Play，没有恢复已收尾的 Q01–Q12 campaign。

## 已确认的受击框绘制缺口

`NTSDHitboxGizmos.DrawEntity` 读取当前 `Frame.D`，通过 `PhysicsState.GetBodyVolumes/GetItrVolumes` 使用旧缓存 `SpriteX/Y/Z`。但正式角色的数据导向路径不再更新这些精灵原点缓存：`LF2Character.ApplyDynamics` 对注册的正式角色将 spriteWidthPx 设为0并跳过 UpdateSpriteOrigin；`CharacterMechanics.StepBattleLogic` 同样不 materialize 旧缓存。

原 Editor 单项 `CompatibilityStep_MaterializesLegacySpriteOriginOnlyAtAdapterBoundary` 实际 PASS，验证 battle SpriteX/Y/Z 为0、compatibility非零。这与仍读取该缓存的 Gizmo 构成已确认的接口不一致，会影响任何走该正式角色路径的实体，并非P2专用。

绘制入口另未接入 `BattleSpatialProjection`；当前2048×1152相对1333×730的倍率为X约1.536、Y/Z约1.578。真实候选/命中查询在 `BruteForceSceneQuery.LocalRectWorldRect/ItrWorldRectExeRaw` 读取 Runtime整数/SourceRule位置、碰撞快照帧和统一投影，不使用Gizmo的旧原点。角色本体另有用户保留的1.5倍显示与表现插值；不能为让框贴图而改DAT或战斗逻辑真相。

帧语义亦不同：Gizmo用 Frame.D，生产 GetCollisionFrameData 用 Frame.Prev2 的 native帧。尚未取到用户P2现场，因此没有证明其实际命中判定错位，也不能断言此次具体现象仅有上述原因。

修复归口：调试框的运行时坐标/碰撞帧/投影适配。应复用生产碰撞几何入口并在原Scene取证，不恢复写入旧Sprite缓存，不把Renderer Transform反写到战斗状态，不加P2/角色特例。本轮只定位，未实施这个修复。

## 已确认的变身机制与未确认的输入路径

正式原336B44、原runtime资源：
- 鸣人OID2/frame388/state8052 → tick1切到OID52/frame0；其后四tick计数1–4。
- 佐助OID11/frame342/state8038 → tick1切到OID38/frame0；其后四tick计数1–4。
- 两份正式报告均exit0/passed=true/completedTicks4，EXE SHA未变。

原Unity Editor，使用当前项目正式Logan DAT：
- 两份受控完整逻辑tick诊断均PASS；所查三tick的目标角色ID、action、state、frameCounter与上述正式样本一致。
- 三项已有C25聚焦检查3/3 PASS，覆盖source-next切换及999/1000重置action0。
- 这些样本直接从已著录变身帧启动，没有执行用户真实物理按键，不是自然技能、图像切换、输入窗口或全World对齐证书。初态Z/World/RNG不同，不能推广为所有字段同态。

Unity旧通用导出器仍要求历史三tick夹具schema token，且header中formalAuthorityExeSha256仍打印B1E13；这是已知旧元数据，不能当作当前行为权威。首个336B44-token/四tick请求在World创建前被格式验证拒绝，原FAIL保留；v2使用导出器既有三tick格式、实际当前Logan DAT，`certificateEligible=false / UNITY_CURRENT_RUNTIME_DIAGNOSTIC_ONLY`。没有修改导出器或将旧token晋升权威。正式证据独立来自实际336B44原件。

当前没有证据证明共用C25数据切换入口整体失效。用户所用角色、目标形态、按键序列以及失败时是否已经进入变身帧仍待回复；下一只复现该序列，定位输入路由/条件与资源消耗/数据提交/表现身份刷新中的首差，不展开角色矩阵。

## 原件与保护

- `formal-transform-summary.json`、两个目录的root-report/root-trace/argv/fixture-manifest。
- `controlled-transform-comparison.json`、两个unity-v2/result.txt与unity-raw.jsonl。
- `c25-result01.json`：job82d076367fa34c149da04fb0895b5d96，3/3。
- `sprite-origin-result01.json`：job3e9642c6d6694fa885dce0141965ce97，1/1。
- 原Editor6401：Battle Scene clean、非Play/非测试/非编译；C# error Console0；无第二Editor/新项目/computer-use。
- 文件请求消费和文档追加审计：docs/ai/FILE-OPERATIONS/NTSD28-336B44-TRANSFORM-HITBOX-DIAGNOSIS-REQUEST-20261006。所有旧音效修改/失败记录保留，无DAT、图片、WAV、Scene、相机或非战斗脚本修改。

