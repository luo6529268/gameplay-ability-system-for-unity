# Q07/C052 原 Battle Scene 正间隔后继候选首差

状态：`VERIFIED / SCOPED_FIRST_DIFFERENCE`。父 C052、Q07、总目标开放。正式 336B44 playable 的受控完整会话已证明 OID211/action161 双 effect21 ITR 可先命中目标一、再因正 rest 拒绝同目标第二候选、继续命中目标二；参见 [源码报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/REPORT.md)。Unity 共用 runner 的 effect21/state18 提前终止门静态缺少 rest 条件，尚未取得生产运行首差。

本包只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C052PositiveRestBattlePlayProbeEditor.cs` 及其 `.meta`。在**原项目**唯一干净 Battle Scene 的现有 Editor 中，以正式内容创建两名 OID2 角色和一名 OID211/action161 特殊攻击，seed682973786、mode0/difficulty0、同源 X500/500/530 与 Z400、500HP/MP，中性输入推进完整生产 Driver 三 tick。逐 tick 记录两目标动作、HP、各自 relation rest 和攻击者动作；远距 X650 是反例。证据需标明源 slot 顺序与 Unity slot 顺序不同，仅比较两目标可观察行为与候选因果，不宣称已做逐 slot 同态。若工厂或输入前置拒绝，保留首个失败结果，不强行改 DAT/Scene/资源/生产逻辑。

前置：已确认原 Editor idle、目标 Scene clean、正式内容根匹配、新脚本 Unity 导入编译0错。测试请求使用新的唯一文件名，**只将请求中的 requested 改为 false，不自动删除**；结果使用唯一文件名并拒绝覆盖旧证据。Play 完成后退出，检查 Battle/Menu/两个配置 Asset SHA、Scene dirty 与日志；针对本包运行最窄生成工程编译及 ChangeLedger validator。生产修复必须另立 Task/Change，并以实际首差为门槛。回滚仅限本包新增脚本和诊断结果；任何删除须先独立文件操作审计及必要授权。
