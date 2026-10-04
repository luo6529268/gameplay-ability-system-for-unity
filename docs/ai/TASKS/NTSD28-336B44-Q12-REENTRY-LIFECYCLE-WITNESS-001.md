# NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001

2026-10-04 验收：同一原 Editor/原 Battle Scene 连续两轮真实 Play；live World 与 2 名角色分别运行到 tick 419、256，两次退出后 Scene clean、Scene Driver.World 已解绑、活动池对象/sprite 均 0。四保护文件和两个程序集的 SHA 在两轮前、轮间、轮后相同；LoganRuntime 未在时间窗写入。生成 Editor 工程在纳入新脚本后 0 错/299 警告，原 Editor 导入及四份唯一原件均已证。[原件和限定边界](../../../artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001/REPORT.md)。本包的重进子门已完成，不把 Scene 局部残留快照扩称全局十一阶段证书；较强关闭证据复用 C056。

状态：`VERIFIED / SCOPED_REENTRY_PASS`。父项 [Q12 代表矩阵](NTSD28-336B44-Q12-REPRESENTATIVE-MATRIX-001.md)，正式战斗规则权威仍为 336B44。本项只补 Q12 已列出的“同一冻结版本退出后重进及残留”子门；不要求再次注入物理组合键。现有自然鸣人首 253 正例和 C056 有序关闭原 Scene 零残留分别复用，边界不扩大。

原状与动机：合成键探针在显式 Dynamic 加临时失焦路由的有效 Play 中仍 30 tick 输入包全 0；这是诊断输入链的不可比样本，不构成战斗规则首差。Q12 重进子门真正要证明的是原 Battle Scene 的生产 World、角色和 tick 在第一次退出后能以同一代码、Scene、配置与内容身份再次建立。现有 C056 仅证单次退出。

唯一代码路径：新建 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q12ReentryLifecycleWitnessEditor.cs`，只读抓取 `SimulationTickDriver`、`SimulationWorld`、角色和池的已有诊断状态。两个 Editor 菜单分别在 Play 已完成 bootstrap 后保存 live 快照、在退出 Play 后保存 exit 快照；每份原件用 `FileMode.CreateNew` 写入独立文件，不清理、不覆盖，不设置输入或更改生产代码。脚本不得保存 Scene、创建战斗实体、触碰 DAT 或用户 UI。

验收：先证原 Editor 非 Play、唯一 Battle Scene clean，冻结 Battle/Menu/两配置及编译程序集 SHA；新脚本经生成 Editor 编译与原 Editor 导入。连续两轮原 Scene Play，每轮 live 快照须有生产 World、至少 1 个活动角色、tick 推进；退出快照须无 live World、池无活动 borrower/sprite，并核对 Scene clean。两轮之间及结束时保护 SHA 与程序集身份必须相同；否则只能报告各轮独立观察，不能宣称“同冻结版重进”。复用 C056 的十一阶段完整退出证据；本探针不重复 C056 全套对象组合。失败即保留原件并停止。

回滚：精准移除本新 Editor-only 脚本及 Unity 生成的关联 `.meta` 需按文件操作审计和用户授权执行；日常不主动清理。所有原件保留。此包不进入生产 Player 构建。
