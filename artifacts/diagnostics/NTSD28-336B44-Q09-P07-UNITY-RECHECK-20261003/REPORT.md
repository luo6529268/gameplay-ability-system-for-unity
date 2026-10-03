# Q09/P-07：当前原 Unity Battle Scene 李子体与阴影复核

状态：`VERIFIED_SCOPED_CURRENT_UNITY_SCENE_P07`。本包验证当前原项目 Battle Scene 中李的自然子体本体可见、`shadow:1` 抑制生效及普通阴影仍在；不关闭 P-07、Q09、Q12 或总目标。

正式规则身份仍为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。同日[当前根/源码证据](../NTSD28-336B44-Q09-P07-WARP-RECHECK-20261003/REPORT.md)已分别证明正式根回放 tick6–13 的 40 个 OID204 子体身份/动作/图格与当前 playable 快照 160/160 字段一致，以及当前 playable D3D11/WARP 在 tick6 实际绘制五个子体本体、没有子体阴影命令。当前正式与 Unity 暂存 `a/cha/cha.dat`、`a/cha/cha.png` 均逐 SHA 一致；Unity 运行时报告选择的内容根是 `Assets/NTSD/Content/LoganRuntime`。

本轮通过本机 Unity MCP stdio **只连接已运行的原项目 Editor**。MCP `editor_state` 和 `manage_scene/get_active` 预检为 idle、非 Play、非编译、单一干净的 `NTSD_Menu`；[前置六文件哈希](preflight.json)已保存。MCP `manage_scene/load` 在源 Scene 干净时切到原 `NTSD_Battle`，确认仍为干净单 Scene；没有启动第二个 Editor。随后提交唯一 `lee-q09-p07-336b44-20261003-01` 请求，复用已存在的 `NTSD28Q07LeeJlBattlePlayProbeEditor`，未修改任何测试或生产脚本。请求原件为[request-before.json](request-before.json)，消费后原请求文件保留为 `requested:false`。

[原 Editor 完整 Play 报告](../NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001/lee-q09-p07-336b44-20261003-01.json) 为 `PASS`，SHA-256 `888DA4F003068650C6D528BE9F6A860BDD278D0624511591CF0D97BE93E1B436`。探针在原 Battle Scene 的空槽注册李/鸣人，并按既有桥接提交 J/L 对应的**测试用离散输入**；生产 Driver 从全局 tick5 到50，五个 OID204 子体在全局 tick12 首次出现。这证明完整 Driver 中的选招与出生链，不等于真人物理键入口。该帧五个子体均完成源码初始化、BMP `shadow:1` 为5/5、中央本体命令5、子体 shadow snapshot 五个且全部 `ShadowVisible=false`、子体 Shadow 绘制命令0，同时有五个普通对象 Shadow 命令作对照。当前 logic-only 物化下 `firstOid204RendererCount=0` 是该路径的物化方式，不代表中央本体命令未发布。探针完成有序关闭、池借用0、相机设置恢复，Battle Scene 哈希前后一致。

原 Unity 的独立 960×540 黑底中央相机[出生前](../NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001/lee-q09-p07-336b44-20261003-01-before.png)与[出生后](../NTSD28-Q07-LEE-CHILD-CAMERA-PIXEL-001/lee-q09-p07-336b44-20261003-01-after.png)两图已实际生成；探针选定 ROI 中有564个变化后非黑像素，独立[全图差异](image-diff.json)为2172像素、包围框 `[166,272,674,476]`。全图差异包含场景其它变化，不能全归因于五个子体；ROI 和中央命令共同支持本体可见性。画面是黑底隔离相机，不是项目背景/UI合成 Game View，也不是与根 EXE 同视口逐像素 A/B。

原 Editor 已先确认非 Play、Battle Scene 干净，再由 MCP 安全切回原 `NTSD_Menu`，返回后 Menu 仍干净。Scene、GameConfig、ProjectBattleModeConfig、李 DAT/PNG [六项前后哈希全部稳定](after-hashes.json)。本包未改 DAT 数值、图片、生产脚本、Scene 或非战斗功能。`unity` CLI 因项目没有 Unity Pipeline 包而无法连接；MCP 的短 C# `execute_code` 仍受旧 CodeDom 命令长度限制，但实际 Scene 查询/切换和请求探针均通过现有 MCP 接口执行，不将该独立工具限制当作战斗失败。

证据边界：正式根 trace 没有逐条阴影命令身份或实际 Present 像素；当前 Unity 的本体/阴影样本与正式源不是严格同一场景、相同起始 roster、相同 tick 的画面。P-07 可比条件的正式 EXE/Unity GPU 输出、其它非例外表现、Q09/Q12 仍开放。后续应优先针对可比较区域和用户保留的固定全背景例外设计同条件画面出口，不重复已通过的普通阴影命令计数。
