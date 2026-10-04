# NTSD28-336B44-Q09-P08-NATURAL-BLEED-OFFSCREEN-001

状态：`DUPLICATE_VERIFICATION_NO_STATE_CHANGE`；P-08/Q09/Q12 和总目标仍开放。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-05/Q09/P-08。

权威：正式根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，其对应 playable 构建闭包和正式 `resources/runtime`。本包复用既有 `Tools/NTSD28Q09Diagnostics/ita_natural_bleed_warp_probe.cpp` 的鸣人普通攻击、鼬受击后自然血点样本。旧 B1E13 离屏证书只作为输入和方法参考，不自动晋升版本。

范围：只读复核正式源码、资源和既有探针身份；将既有编译参数的输出路径改为本包新诊断目录，不修改探针源码、正式源码、EXE、DAT、PNG、Unity 脚本、Scene、Asset 或非战斗逻辑。运行新诊断进程，保留逐 tick 原始日志和 1333×730 血点启用/去除成对图，独立计算像素差与哈希。新文件只写入本包新路径，拒绝覆盖已有输出。

验收：编译 exit0；探针实际经完整 `GameSession28::step()` 的普通攻击达成正 HP、站立、单血点；同一逻辑快照的两张 D3D11 离屏图只有所选血点贡献；报告完整限定：这不是根正式 EXE GPU Present，也不是原 Unity 同初态同视口画面比较。若当前新版行为不再满足旧探针断言，保存失败原件并审计首差，不修改 DAT 或用旧结论填补。原 Editor 当前编译状态异常，本包不运行它，也不触碰其内存 Scene。

风险与回退：本包只新增诊断文件；不需要回退用户工作。若输出不成立，维持 P-08/Q09/Q12 开放。提交/交接前核对 Git 状态与相关文档空白，并检查 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 的 SHA 不变。

2026-10-04 结果：[本版诊断报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P08-NATURAL-BLEED-OFFSCREEN-001/REPORT.md)。当前源码完整 GameSession 第 8 tick 自然扣鼬 HP180→160，第 22 tick 站立且有唯一 1×3 红色血点；当前 playable D3D11 同快照有/无该命令的两张 1333×730 图恰有 3 个像素差，坐标 `(461,595..597)`。两图分别与旧 B1E13 限定探针的 PNG 字节完全相同；这只给当前 playable 源码离屏 GPU 子门新增版本证据，仍缺根正式 EXE Present 与 Unity 同初态/同视口实图。既有探针源码、正式源码、DAT、Unity 项目和 Scene 均未改。

**同日纠正：** 阅读当前总表 P-08 索引后确认 [NTSD28-336B44-Q09-P08-PLAYABLE-BLEED-RECHECK-20261004](../../../artifacts/diagnostics/NTSD28-336B44-Q09-P08-PLAYABLE-BLEED-RECHECK-20261004/REPORT.md) 已在同版、同未改探针、同初态下取得相同三像素和两 PNG 哈希，且另有原 Unity 受控 GPU 图。上段“新增版本证据”应仅理解为本目录独立重跑原件，**不是新的未闭出口，也不提升任何状态**。保留本次文件供审计，不重复该案例；下一处理原 Editor 编译恢复后的正确 Attack 同输入 Play。
