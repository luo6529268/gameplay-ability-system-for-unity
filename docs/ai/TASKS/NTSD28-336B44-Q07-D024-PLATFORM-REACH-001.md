# NTSD28-336B44-Q07-D024-PLATFORM-REACH-001

状态：`VERIFIED_SCOPED_CONTROLLED_REACH`。父项：336B44 总表 Q07 / D-024 / repeated platform carry。正式受控阳性、根 LFR 与有限阴性见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-REACH-001/REPORT.md)；自然玩家输入仍未知。

正式权威是根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 playable 源与正式 `resources/runtime`。当前 DAT 静态枚举中只有 OID56/frame182 的平台 ITR 同帧具有非零 `dvx:-3`；既有平台搬运 Unity 修复只通过合成连续测试，正式内容连续可达性未知。2026-10-03 原 Battle Scene 受控 frame130 单 tick 已证明当前内容的平台链接入口，仍不证明非零搬运。

只新增 `Tools/NTSD28Q07Diagnostics/d024_formal_platform_reach_probe.cpp`：用当前 `GameSession28` 完整 tick、正式资源及固定 mode0/seed/背景1，对 OID56/frame182 与 OID2 不同 team 的有限 X/Y 初态做受控扫描，记录 frame、规则位置、链接槽/碰撞参考、逐 tick 实际位移。只读正式源和 DAT；输出到本 ID 独立诊断目录，不写或重建正式 EXE。首个阳性另录同会话 LFR，交当前根正式 EXE 做同输入回放；旧筛选输出保持原样。将受控初态与玩家自然输入严格分开。若首阳性出现，再决定是否建立独立原 Battle Scene 同态探针；若无阳性，保留有界阴性并继续查自然入口，不据此宣告不可达。

不改 Unity 生产、DAT、图片、背景/模式、Scene、相机、菜单或现有用户文件；不复制或删除资源。验证为编译零错误、探针有限运行及原始 CSV/摘要、阳性 LFR 与根 EXE 回放、正式源码字段核对、`git diff --check` 与 Change Ledger 校验。回滚只用前向更正或在审查后处理本 ID 新文件，保留原始证据及其它用户工作。
