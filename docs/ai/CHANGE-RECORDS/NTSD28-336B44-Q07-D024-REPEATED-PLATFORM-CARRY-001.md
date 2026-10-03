<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
authority: 336B44 playable linked platform frame motion and user D-024 fixed full-view ratio
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001.md
-->

# NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001

状态 `IN_PROGRESS / STATIC_CANDIDATE`，脚本尚未修改。[Task](../TASKS/NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001.md) 定义当前权威、现状、精确路径、顺序、风险、验收和回滚。前置只读事实：正式 `BattleWorld28::apply_frame_motion` 的链接搬运从乘客规则整数坐标重基；Unity `LF2Entity.ApplyLinkedPlatformMotion` 的源域也如此，但物理域从自身取整值重基并乘比例。单次现有四例不能覆盖连续误差。首个脚本改动只在现有 Editor 测试加连续默认/配置视口左右 X/Z 守护；取得 RED 后才考虑生产共用写者。正式根/原 Scene 的具体可达性及全局 Q07 状态均未由静态复算证明。

2026-10-03 `CODE_WRITTEN / TEST_ONLY`：已在声明的 `NTSD28Q06FrameMotionTailEditorTests.cs` 新增 `RepeatedLinkedPlatformCarryPreservesSourceViewRatio` 四个参数化用例；原状只有单次平台比例断言，新测试用同一注册平台/乘客在默认及2048×1152视口、左右方向连续执行24次共用帧运动，逐步检查正式规则源X/Z，并累计物理到共享投影的最大误差。生产 `LF2Entity.cs` 尚未修改。生成编译与原Editor RED未运行，任何静态计算不可报实测差异。预期影响限Editor测试与诊断；不改资源、Scene或非战斗。若夹具前置条件不成立，先修测试并保留失败原件。

2026-10-03 原Editor真实 `RED`（覆盖上文“未运行”的初始快照）：生成 `Assembly-CSharp-Editor.csproj` 编译0错/275警告；原Editor经MCP脚本刷新并恢复连接，在干净Menu场景运行该方法四个参数例。默认视口两例通过，配置2048×1152视口右/左24次后的X最大投影偏差分别为3.6241560390097334/3.070517629407334输出像素，超过1像素门槛；Unity测试任务`20c52ca156b0436cb04ca97b80a13f29`报告failed且4例完成。原始MCP结果在`artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001/unity-red-result.json`。Z断言尚被X首差遮挡；未将静态估算或旧版平台夹具记为正式根证书。下一只改已声明 `LF2Entity.ApplyLinkedPlatformMotion` 的源已初始化物理X/Z增量并更正故意物理/源锚点不同的现有单次测试预期；后续编译、GREEN与原Scene待验。

2026-10-03 生产共用写者已修改：只在声明的 `LF2Entity.ApplyLinkedPlatformMotion` 两个非零 X/Z 分支，源规则坐标已初始化时先依正式整数规则基底写新精确源位置，再将“本步源精确变化量 × World已配置比例”累加到物理精确X/Z；源未初始化时保留旧物理整数重基。链接选择、帧DV解码、朝向、Y/参考高度、速度、源规则取整和平台→直接帧pass顺序未变；没有角色/OID特判。现有故意分离物理/源锚点的单次测试物理预期与新源增量不符，须先按Task更新测试并跑聚焦；编译、GREEN、完整Driver/Scene均尚未验证。

2026-10-03 同测试文件邻接断言更正：`LinkedPlatformCarryUsesViewRatioAndFacing` 故意将乘客物理锚点200/250与已初始化规则源小数-12.75/31.25及源整数-14/29分开，正式本步精确源变化量为右X+2.75/左X-5.25、Z-0.25，故其物理预期按这些源差乘世界比例；源规则预期未改。`PlatformThenDelayedDirectFrameUsesIndependentSourceIntegerBase` 已初始化分支的平台源差为X+1.75/Z-1.25，未初始化分支继续X+3/Z+1；后续直接帧位移及两域取整断言保留。只更正两个已声明现有夹具的物理预期，不改 DAT 或运行时规则。GREEN/相邻编译仍待。
2026-10-03 `RUNTIME_PENDING / FOCUSED_TEST_PASS`：生成Editor项目改后0错/306警告，原Editor帧运动整类39/39 PASS（新增连续平台4例及相邻单次/直接帧）；结果JSON任务status`succeeded`且summary39/39，MCP长输出在落盘后断开。完整SelfCheck菜单MCP回执超时，但本轮调用后结果文件新写`PASS`，前后临时结果均复制入本包证据。Menu/Battle磁盘SHA前后同，随后MCP只读场景为Menu clean/9根；多出的内存`BoundaryWallManager_AutoCreated`直接创建链未证，保留。当前正式decoded DAT静态枚举17帧平台关系ITR，唯一非零DVX为OID56/`c/hid/rea.dat` frame182的-3，未证它的自然链接碰撞或连续发生；合成测试不能晋升为根/原Scene证书。完整[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-PLATFORM-CARRY-001/REPORT.md)。Q07/D-024全实体、Q09/Q12和总目标仍开，无DAT/Scene/非战斗改动；回滚仍限本Change精确增改行。

2026-10-03 注释留痕：在同一已声明X分支保留原 `NTSD28-USER-SOURCE-COORDINATE-FRAME-MOTION-001` 源规则合同注释，并并列新增本Change比例出口注释；只改注释，不改变已通过测试的运行行为。
