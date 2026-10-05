# Q09 鸣人／鼬全局 tick14：正式根逐 tick 对照（2026-10-05）

## 结果与首差

当前用户选定的正式 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式资源根的 [v4 headless LFR 回放](root-playback-report-v4.json)进程 exit0、`passed=true/failureCode=0`，声明末输入行14、按 LFR 终端语义执行15 tick。将其 tick1～14 与原 Unity Battle Scene 已保存的[全局 tick14 单例 JSON](../NTSD28-336B44-Q09-GLOBAL-TICK14-GAMEVIEW-001/idle-global14-336b44-20261005-061031-920004.json)逐值比较，12 个明确同名字段×14 tick 共168项：**140项相同；28项差异全部为双方 Z**。首差在 tick1：正式版鸣人/鼬 Z400→542，Unity 两人继续保持 Z400；tick1～14 均如此。两人的动作、X、HP、MP，以及 CRT RNG state/calls 各14 tick 无差异。[逐行机械结果](selected-comparison.json)保留全部值与两个输入 SHA。

正式 C++ `GameSession28::step()` 从所选原版背景构造 `StageBounds28`；`SimulationTickDriver28::step()` 在几何前调用 `BattleWorld28::clamp_type0_stage_depth`，对 type0 角色的 precise Z 施加原版背景近/远界。根 trace 的 tick0 两人 Z400、tick1 两人 Z542 与这个可达写者一致。Unity 使用项目自己的地图可行走边界，用户已明确排除原版背景和地图内容；Unity 本例 Z400 在其项目地图内不被这项原版边界提升。因此该首差属于**已批准的地图/场景内容边界差异**，不能当作共享战斗帧机、输入或 HP/MP 规则首差，也不能说 tick14 双方完整逻辑初态仍相同。它解释了两张不同场景图中角色纵向落点不可直接逐像素比较的一个具体来源；是否有其它表现差异，本报告不裁决。

## 输入载体与权威限制

- 原始载体是仓库既有鸣人 OID2／鼬 OID9 的只读 LFR；为了复用同一代表样本，新建独立的[派生载体 v4](idle-global14-derived-input-v4.lfr)。[逐版 manifest](carrier-manifest.json)及 v2～v4 manifest 记录原文件 SHA、所有变更的 manager 偏移及新文件 SHA：将目标 X 调为620、双方 Z 调为400、双方 MP调为200、目标基础HP调为500，设置正式 GUI 所报 `bgm\boss2.wma`，14行输入清零，并更正 LFR 自身的第0行 HP 校验及旧攻击留下的终端得分/伤害表头。这些只定义诊断输入；**规则权威仍是当前正式 EXE 的实际回放输出**，派生载体不是规则来源，也未修改原 LFR、DAT、资源或 Unity 生产代码。
- [正式调用参数](root-invocation-v4.json)、[正式 trace](root-trace-v4.jsonl)、[通过报告](root-playback-report-v4.json)均保留。trace tick0 显示 battleMode0、OID2/9、X500/620、Z400、HP500/MP200、BGM `bgm\boss2.wma`；tick14 两人动作3、X不变、HP500/MP200、Z542。原 GUI [窗口取证](../NTSD28-336B44-Q09-TICK-BOUND-CAPTURE-20261005-v3/REPORT.md)只在标题显示 tick14、动作3和 HP/MP，不显示 X/Z；**不能把 Unity 的 Z400 写成“与正式 GUI 标题同值”**。GUI 与 LFR 回放分别是当前正式 EXE 的可观察窗口和诊断入口；不声称两入口全部隐藏状态逐字节相同。
- 首次派生载体保留旧 HP 校验530，正式根在第0输入行后以 exit46 正确拒绝；v2 更正为1000后完成15 tick、旧终端表头得分未更新；v3 更正得分后仍留旧目标伤害表头；v4 同步这两项旧攻击账本后 exit0/PASS。v1～v3 的报告与 trace 原件保留，不作生产差异证书。没有为这些载体错误再启动 Unity Play。

本例既没有非地图例外战斗首差，也不能证明其它角色、技能、碰撞、全 World 或全帧画面完全一致。Q09/Q12/总目标仍开放；下一项应基于实际可复现的非例外首差，而非把已批准的原版舞台 Z 边界强加到项目地图。
