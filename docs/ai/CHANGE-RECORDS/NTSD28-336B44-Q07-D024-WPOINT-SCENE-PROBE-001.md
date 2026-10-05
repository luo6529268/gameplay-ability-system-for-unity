<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-WPOINT-SCENE-PROBE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07AirborneIdleBattlePlayProbeEditor.cs
authority: User battle alignment goal and D-024 shared projection; current formal 336B44 BattleWorld28::settle_held_refill_objects reciprocal WPoint alignment; necessary original Scene consumer validation after WPOINT-VIEW-ANCHOR-001
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001.md
-->

# D-024 原 Battle Scene 持有挂点呈现出口

脚本前登记。本包只复验刚修改的共用挂点消费者，复用前包原Editor两例、正式源码自然拾取24tick定位与关系/动作证据。前包仅在独立生产Driver执行并直接调用几何函数，尚未证明原Battle Scene不可变快照→中央命令已带新补偿。本次只有一次两tick拾取Scene出口，不重开规则矩阵，不改变总目标或其它未关闭任务调度。

准确代码范围：现有 `NTSD28Q07AirborneIdleBattlePlayProbeEditor` 新增 `d024-held-anchor-` opt-in分支、报告字段及命令挂点取证辅助；旧OID85/32tick分支保持。只在原Scene Play克隆配置OID2/7普通初态、正式OID120地面武器及两tick离散Attack载体（canonical Jump），源X200/190、Z542，记录自然115/24、互持关系、源及视图位置与当前中央命令。用实际command Position/Size/Pivot/FlipX及正式帧WPoint算两端世界接触点，换为视图像素差；保持1.5图片和共用projection，不能直接调用被测补偿函数作为期望。

生产/DAT/图片/Scene/相机持久状态/非战斗不改，没有新runtime模块或关闭阶段变化。探针只在唯一干净原Battle Scene、非Play/非编译/无测试状态启动；Play克隆内数据随既有生命周期关闭，沿用有序shutdown owner，不自行重排关闭。请求覆盖/消费/恢复与新结果状态更新另按Operation审计。

验收：生成Editor/原Editor0error；只两完整Driver tick，115/24/sourceX201/214、sourceZ差1/sourceY0/7、互持关系保持；当前publication/plan同tick、双方各1有效command，显示挂点差≤0.0003px；退出Scene clean/hash不变/非Play/Console0error，请求逐字节恢复。若前置或运行失败，保存原件并按第一个失败定位，不盲重跑。GPU/正式根EXE同帧Present和真人输入仍未知，本包不宣称完整画面一致。

回滚：只手工撤销本Record新增opt-in/报告字段/辅助方法，保留同文件既有垂直和R120探针以及其它dirty work；不得restore/reset/clean。当前PLANNED，未改脚本。

2026-10-05 脚本前登记之后已CODE_WRITTEN：同文件新增runId opt-in、HeldAnchorSample、两tick普通初态/地面武器分支、实际central command挂点取证；沿用原Scene清洁/哈希保护与结束路径。生产未改。旧32tick不运行，只编译和新单项待验；没有GPU/真人输入证书。

生成编译：`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`退出0、301 warnings/0 errors、8.64秒。`Tools/Validate-ChangeLedger.ps1`由pwsh执行，1270Records/19diff代码文件通过。既有请求覆盖/消费/恢复Operation `NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001`已预登记；MCP刷新后还需原Editor编译/Scene前置核对，Play未执行。

2026-10-05 后继原Scene单次验收：[原件与限制](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/SCENE-ACCEPTANCE.md)，run `d024-held-anchor-20261005-01` PASS/DONE、global5→7，正式自然拾取115/24和互持关系正确，publication/plan/current均7且双方各1实际command，挂点X0/Y0。sourceX201/214、Y0/7、Z481/482；绝对Z由项目地图钳制，不能称formal夹具Z542同值。原EditorConsole0error、Scene clean/root11/非Play/SHA保持，请求68字节SHA严格恢复。单次探针出口限定VERIFIED，不将GPU/真人键/其它政策未知晋升；生产父Record仍RUNTIME_PENDING。ONE完成，不重复本包或旧矩阵。
