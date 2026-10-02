# Q09 自然可见实体源坐标插值门（2026-10-02）

状态：`VERIFIED_SCOPED_NATURAL_FRAME / Q09_OPEN`。当前正式根 EXE 身份为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本包只检验原 Unity Battle Scene 一个自然同帧的可见实体源坐标门；没有正式 EXE 实际像素配对，也不关闭所有传送、抓取、出生或死亡断点。

原 Editor PID 105896、Unity 2022.3.62f3、原 `NTSD_Battle.unity`。只改已有请求式 Editor 诊断 `NTSD28Q09NameplateNaturalPlayProbeEditor.cs` 的唯一 Game View 报告：从同一被暂停的发布/捕获帧枚举有 `Entity` 绘制命令的 handle，在前帧同槽查找运动状态，记录两侧 `HasSourceRulePosition` 和现有 `BattlePresentationMotionSampler.Sample` 的状态。采样 alpha=0.5、显示比例1/1仅用于决定状态，不改实际画面、世界或生产采样器。

## 原场景结果

- [成功 JSON](natural-nameplate-20261002-004743-366-996e805edb434e0a93fb4ff418e6a173.json) SHA-256 `1B954AF583B7AFE6E192069D0ED061B48A83B9B152EF724728F4C8902032A48A`，`PASS`。Game View 中自然 P1 D 键从 X800 移至 X856，开始 tick2142，捕获/发布/中央计划/截图后均 tick2152，方向输入10个逻辑 tick。
- 同帧本体命令可见的两个实体分别为 `slot0/OID2`、`slot1/OID2`；二者前后 `HasSourceRulePosition=true`、采样状态均 `Sampled`。本样本 `visibleMotionCount=2`、`adjacentVisibleMotionCount=2`、`visibleMissingSourceCount=0`。它只排除**该帧这两个可见实体**触发 Unity 的缺源坐标拒绝门，不证明其它实体/出生帧或关系、传送断点无差异。
- [成功 PNG](natural-nameplate-20261002-004743-366-996e805edb434e0a93fb4ff418e6a173.png) SHA-256 `D1D92E63480ED434FF8EB86CE9222CAB9E6B4545C8C30D4E8B3A77F16CC40D9B`，1920×1080，2,230,788 字节。已目视确认完整项目背景、两名角色、阴影与 Unity 自有 HUD；这是 Unity 合成 Game View，不是正式 EXE 画面。

## 失败与焦点边界

三个独立唯一 JSON 在进入采样前 FAIL，均保留：

| JSON | SHA-256 | 观察 |
| --- | --- | --- |
| [首轮](natural-nameplate-20261002-004008-787-4818edbea55546e99ec2918ed14ff5cd.json) | `4B014B667D09DE081D6D92A39283C5F21DFECB531EC1769F59E882846CD29517` | MCP 经 Play 重载从端口6400改为6401，菜单启动过晚，start tick5304；鸣人X800不动，180 tick门FAIL。 |
| [新局早启](natural-nameplate-20261002-004205-992-5d747a62eaf94f72a1864943f6b0af69.json) | `E0BCA060F73B9A51AC6CC747206B21FC368D2C242B9B89308EEDF40C521BDDD3` | start tick2，179次采样X800不动，输入焦点未生效。 |
| [Play 前聚焦](natural-nameplate-20261002-004522-298-79995f98b4f841baa5f151b547def2bf.json) | `37D9436CEFFA19CF99266673874962693840E54775CC2F213899E6E7D91F323C` | start tick2，180次采样X800不动；Play 重载后 Editor 再度失焦。 |

成功轮在**同一 Play 内**通过 Editor MCP `Window/General/Game` 菜单使 Editor `is_focused=true`，随后同一脚本同一 D 键路径移动成功。前述 FAIL 属诊断输入焦点条件，不是已证战斗位移或插值生产首差。MCP 并行双连接曾导致短时连接丢失，此后串行调用；端口由原 Editor 监听状态定位，未启动第二个 Unity 实例。

## 验证和边界

生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q` exit0、251条既有 warning/0 error；原 Editor 刷新后程序集时间晚于脚本，真实 Play 成功轮 PASS。退出后 Editor `idle/nonPlay/noncompiling`、Battle Scene `isDirty=false`/13根对象。Battle/Menu/GameConfig/ProjectBattleModeConfig 的磁盘 SHA 分别保持 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`；`LoganRuntime` 限定 Git 状态为空。前次成功 JSON/PNG SHA 仍为 `968DA193193724F01324B9AA7C964B109533E66C408B3C6675A14370FC863D07`、`2BEF6A4E04B0718EDD31F0104827E10E50366BA4C1A061C90E8B1523CCCCE5B2`。

`git diff --check` exit0；`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` exit0/PASSED，1140 Records、16个当前脚本diff路径被覆盖。validator 对大量历史 Record 声明文件不在本次diff发出提示，不是本 Change 的未覆盖代码。本包不修改 DAT、角色图、背景/模式、生产战斗或非战斗功能。下一 Q09 真实出口为自然关系/出生/传送断点和正式 EXE 可比显示帧；如没有正式可达的缺源坐标阳性，不改共用采样器。
