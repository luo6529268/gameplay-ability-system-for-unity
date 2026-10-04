# Q01 当前正式 WORDS1 的 Unity Game View 可见使用

2026-10-03 状态：`VERIFIED_SCOPED_WORDS1_UNITY_GAMEVIEW / Q01_Q09_Q12_OPEN`。本包以 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式根 EXE 和对应 playable 资源读口为当前权威；只验证 Unity 原项目已暂存的正式 `WORDS1.png` 在一次真实 Menu→Battle 战斗画面中被选择、发布并可见，不证明正式 EXE 同视口 GPU 像素或其它五张 WORDS/SPARK 的画面出口。

正式 `GameSession28::initialize_resources` 从 `resource.dat` 索引16～21选 WORDS0～5；本案例的 `WORDS1.png` 在正式 `resources/runtime/vfs/sprite/UI` 与项目 `Assets/NTSD/Content/LoganRuntime/vfs/sprite/UI` 当前均为 SHA-256 `E959F2C6A992ED75DA9AFB335F379D35ECAB686D1E9F4B3B60E240D084B265DC`。其余六张活跃图的逐项身份见 [七图矩阵](../NTSD28-336B44-Q01-RESOURCE-SECONDARY-CONSUMER-20261002/resource-55-path-matrix-v3.csv)。旧 B1E13 版 2026-09-27 字形截图只用于复用诊断方法，没有被晋升为当前版本验收。

原 Unity Editor 2022.3.62f3、原 `NTSD_Menu.unity` 进入 Play，经既有 `AppManager` additive 加载 `NTSD_Battle`，项目内容根为 `Assets/NTSD/Content/LoganRuntime`。第一次 [请求结果](q01-words-20261003-01.json) 为 `FAIL`：旧 Q09 探针在方向键 D 的174个观察 tick 后仍见 P1 X620，未到其 X>794 视口边缘门，故没有 PNG。失败不是 WORDS 图片像素差。退出仍为唯一、干净 Menu，Battle/Menu/GameConfig/ProjectBattleModeConfig 四文件 SHA 前后相同。原件保留，未改生产逻辑以迎合探针。

第二次仅启用测试探针的 Q01 可见字形分支，保留相同 Menu→Battle、发布帧/中央命令/相机公式/真实 `ScreenCapture.CaptureScreenshot`，不把移动作为 WORDS 资源前置条件。[请求结果](q01-words-20261003-02.json) 与 [同帧探针 JSON](natural-nameplate-20261003-144859-220-5786f96f4cc740efabb4891304bfebd2.json) 均为 `PASS`：初始/最终 P1 画面 X 均620，已送 D，方向 tick观察4；截图、中央发布与计划均为完整逻辑 tick7，截图返回后仍 tick7。该帧有 P1 body、shadow、`OverlayGlyph` 命令，选择 WORDS sheet1、字形 `1`，正式暂存绑定存在。命令投影字形约 `(580.55,320.625)`，角色体约 `(586.41,323.44)`，屏幕坐标左下原点。

[真实合成 Game View PNG](natural-nameplate-20261003-144859-220-5786f96f4cc740efabb4891304bfebd2.png) 为1920×1080、SHA-256 `DAE1C1FCBC41CD1B1E190175958638EF187A194B1F5DECF80665B02588307BFB`。已目视核对可见战斗体、项目背景/HUD与 P1 脚下蓝色 `1`；独立只读像素取样在命令投影邻域 `(565..596,744..775)`（PNG 左上原点）计得77个蓝色像素，边界 X575～585、Y754～767。此区域与命令位置及正式 WORDS1 资源绑定共同证明一次 Unity 可见使用；颜色阈值本身不能单独证明整图来源或正式 EXE 像素相等。

第二次请求记录 `enteredPlay=true / exitedPlay=true / finalScene=NTSD_Menu / finalSceneDirty=false / sceneCount=1 / protectedHashesStable=true`。四保护 SHA 为 Battle `934483…A974D7BF60`、Menu `1F6586…D5E571156`、GameConfig `0527D7…BEA7`、ProjectBattleModeConfig `B57CFE…EB82`，分别前后相同。生成 Editor 工程包含两个测试脚本，`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` 为0错误；原 Editor `refresh_unity` 后程序集时间/大小更新，真实 Play两次均实际执行。最终 `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <项目根>` 为PASS（1208 Records、四个现有code diff文件均覆盖）；最初将两个`code-path`以分号连写导致validator误判未覆盖，失败日志保留、改成两行metadata后复核通过。[最终账本输出](change-ledger-final-v2.txt)、[初次失败输出](change-ledger-final.txt)、[生成工程编译](compile-after-variant.txt)。仅对所改脚本和文档的`git diff --check`通过；全工作树该命令仍会报其它任务正在改的Menu Scene尾空格。其它角色全套案例未重跑。

边界与后续：只关闭 WORDS1 的**当前 Unity 可见使用**子门。第一轮 D 注入后 X620不动，第二轮在4个方向观察 tick 中也未动；这不能当成真实玩家物理 D 无效的正式结论，需独立记录键状态、`MoveAction` 值、采样帧输入和角色状态才能定位。Q09 原 X>794 视口门、正式根 EXE 的可比 Present 图、其它 WORDS/SPARK 可见出口、Q01/Q07/Q09/Q12 与总目标保持开放。没有改 DAT/PNG/WAV、Scene、相机、生产战斗或非战斗逻辑；仅新增请求桥并给既有 Editor 探针添加独立 Q01 测试分支。
