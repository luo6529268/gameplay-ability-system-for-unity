# NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001

状态：`VERIFIED`（限60tick自然Scene选定字段、关系尾与退出保护）。父目标：新版 336B44 BATCH-04/Q07/C043仍开放。

目标：在原项目唯一 Unity Editor 的原 `NTSD_Battle` Scene Play 副本，以正式 Lee7/Chi8 DAT、同组、seed 682973786、Chi 初始动作256与正式根 LFR 相同的60 tick输入，验证自然 OID420 出生与持有、tick28融合失效关系尾以及次 tick 不重复。逐 tick 保存相应角色/子体选定字段，与已封存的正式源码和根程序结果配对。不得手工制造子体或破坏关系。

脚本范围：仅新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C043FusionHeldBattlePlayProbeEditor.cs` 及 `.meta`，通过独立 `Temp` 请求与诊断结果工作，不复用或覆盖 C056 探针和旧结果；不改生产脚本、DAT、Scene、Prefab、ProjectSettings、资源或非战斗逻辑。

权威与原状：正式源码和根程序已在 [C043 自然入口报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001/REPORT.md) 同 LFR 的60 tick选定840字段一致，tick6 OID420/slot50持有、tick28 7+8→51并清关系-1→0；Unity共用失效尾已有受控聚焦和Scene七例通过，尚无此自然生产链的原Scene同态证据。

边界与风险：原Scene启动前先核唯一Editor、非Play、无编译/导入、Scene clean；在 Play clone 的 Bootstrap.Start 前只覆盖双角色；每步由同一生产 Driver 推进。地图/表现坐标是用户保留的 D-024 例外，不能把屏幕比例误作源码规则坐标。若出生或融合时点不同，保留首差原件并回溯共享输入/帧/OPoint链，不调整 DAT 数值或为特定 OID 打补丁。现有未提交文件/用户内容是保护边界。

验收：原 Editor 编译0错、60个完整生产 tick 的字段与正式根配对、自然四门与诊断事件、退出后 Scene clean/原保护资产SHA不变、池借用与有序关闭可观察项；仅对通过的选定范围称限定通过，不称 C043/Q07或全战斗完成。先运行最窄生成工程编译与此Play；必要时才扩 SelfCheck。所有失败结果和旧证据留存。

回滚：精确审查本 ID 新增探针/请求/结果；删除或 Git restore 前按仓库规则另获批准。本任务不执行删除。

验收结果：首轮错误路径preflight未进入Play、第二轮tick42共用生命周期异常均保留；独立修复后第三轮原Scene60tick四门通过，正式源码17字段1020项仅5个无链接空槽哨兵原值差，限定归一化后1020/1020，退出池借用0/Scene clean/四保护SHA稳。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/REPORT.md)。完整World/物理键/SelfCheck与C043父项仍开放。

2026-10-02 后继更正：原Editor完整SelfCheck在按336B44修正五组旧测试断言后第八轮实际`PASS`，原件和逐轮首差见[SelfCheck报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/SELFCHECK-336B44-20261002.md)。上段“SelfCheck待”是当时快照；当前只剩C043全World/物理键等未闭，父项/Q07仍开放。
