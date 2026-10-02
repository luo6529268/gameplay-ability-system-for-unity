# Q09 当前原 Battle Scene 的同帧合成画面

状态：`VERIFIED_SCOPED_UNITY_SAME_TICK_CAPTURE / FORMAL_EXE_PIXEL_PENDING / Q09_OPEN`。本包只补 Unity 侧画面和当前生产 tick 的同帧取证，不声称正式 336B44 EXE 同画面、名牌、普通 HUD 或整场表现已对齐。

## 原场景证据

原项目 Unity 2022.3.62f3 的活动 `NTSD_Battle.unity` 保存状态干净，直接 Battle Play 使用 `Assets/NTSD/Content/LoganRuntime`。日志确认 `BattleTestBootstrap` 预热完成、表现启用、生产 `SimulationTickDriver` 恢复。既有 Q09 探针经 MCP 菜单入口在 Play 中送出 P1 **D 键按住**，以当前画面角色本体为锚点，在中央发布完成后暂停 Driver、刷新画面计划并截图。探针完成后释放按键和恢复 Driver，随后 MCP 退出 Play。

- [成功 JSON](natural-nameplate-20261002-001136-501-3aa9f6ed8a2a44a1bad6acc14de079c6.json)，SHA-256 `968DA193193724F01324B9AA7C964B109533E66C408B3C6675A14370FC863D07`：`PASS`，开始 tick472，截图/发布/计划/截图完成后 tick 均为 **482**；9 个方向 tick，P1 实际整数 X `800→856`、同帧源规则 X `557.1890197694302`。中央帧含 3 实体、6 命令，P1 本体和阴影命令存在，角色图片绑定存在。
- [成功 PNG](natural-nameplate-20261002-001136-501-3aa9f6ed8a2a44a1bad6acc14de079c6.png)，SHA-256 `2BEF6A4E04B0718EDD31F0104827E10E50366BA4C1A061C90E8B1523CCCCE5B2`：独立文件解码为 **1920×1080**、`2,233,032` 字节；可见项目背景、战斗体及现有 Unity 自有 UI。P1 本体画面坐标 `(806.2532, 128.4375)`（Unity 画面左下原点），与同次报告的逻辑体/中央命令关联。
- 同帧相机：orthographic size `5.75999975`、aspect `1.77777779`、pixel rect 从 `(0,0)` 起为 `1920×1080`；画面映射 `viewportLeft=-11.99746513`、`viewportTop=-2.05000067`、`UnitsPerPixelX/Y≈0.01`，可见源像素 X 为 `-3…2044`。这是当前完整背景相机与 D-024 比例域的实际 Unity 读数，**不是**正式 EXE 1333×730 的同视口证明。

## 首轮失败与保护

[首轮失败 JSON](natural-nameplate-20261002-000658-653-667ea8e64ebb47ceadfeb2549223905d.json)，SHA-256 `A5091D20BFA0DE997EA2C4993ED360907A0EFE2E590E518886B9C3A7C5761297`，在输入前因原 Battle Scene 直接启动没有 slot 名牌而返回 `FAIL`；没有首轮 PNG。这只是历史名牌探针的前置条件不适用于直接 Battle 启动，不是战斗表现首差。后续只在直接 Battle 的 Game View 分支允许无名牌时按角色本体取证；有名牌或旧非截图分支仍执行原名牌合同。成功 JSON 如实记录 `hasLabelCommand=false`，名牌/WORDS0～5 的画面出口没有由本包验收；`hasSelectedWordBinding` 在空标签下没有名牌证明力。

2026-09-27 历史固定 JSON/PNG 的 SHA 仍分别为 `21115A769E20E5832A8F89D460FE34FE96FFF1E19F7069EE684A5A37E733F229`、`D1D34F7E461B6659BC9CA122A55FCC3F6756A3113EF484079E79B8862757AE95`。本包两次结果有不同唯一 ID，未覆盖历史原件。两次 Play 均经 MCP 退出；最后 Editor `idle/nonPlay/noncompiling`，Battle Scene `isDirty=false`、13 根对象。Battle Scene、Menu Scene、GameConfig、ProjectBattleModeConfig 的磁盘 SHA 分别保持 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`；`LoganRuntime` 限定 Git 状态为空。

## 验证层级与未验项

- 生成 Editor C# 工程两轮 `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 均 exit0/0 error，既有 warning251；原 Editor 两轮 MCP `refresh_unity` 后域重载完成、编译空闲，实际 Play 探针二轮为 FAIL→PASS。`git diff --check` exit0。Ledger validator 首次用嵌套 `powershell -File` 未显式传 `RepositoryRoot`，参数默认求值时 `$PSScriptRoot` 为空而失败；直接调用并显式传仓库根路径后 exit0/PASSED，1139 Records、16 当前 diff 代码文件被覆盖。失败命令不是脚本合同失败。
- 截图稳定在同一逻辑 tick 的前后检查，且中央发布 tick 与计划 tick 一致；这不自动证明每个屏幕像素和该 tick 的命令逐像素一一对应。没有相同 seed/初态/输入的正式 EXE 实际显示帧、设备渲染对照或其它 Q09 的阴影、火花、出血、地震、Legacy/插值断点出口。背景与普通 HUD 按用户例外，不用原版资源替换。
- 后续先选可被正式 336B44 根程序真实显示的同条件战斗帧及可比区域，保留本图作为 Unity 侧锚点；只有正式 EXE 取证和同条件比较完成，才能判断像素差及是否需要改生产表现。
