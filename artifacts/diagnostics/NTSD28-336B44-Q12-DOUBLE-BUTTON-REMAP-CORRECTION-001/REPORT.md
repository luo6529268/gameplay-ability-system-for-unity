# Q12 自然按键发现并修正人类三键双重换算

当前权威：正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，对应 playable `InputKey28::attack/jump/defend` 索引4/5/6。项目原 `NTSD_Battle` Scene、正式 LoganRuntime 内容、项目自有背景与模式 Asset。没有修改 DAT 数值、图片、音频、Scene、Prefab、菜单/结果页或 GAS 框架。

Q12 代表性自然链首先给出 RED：[修正前原始结果](physical-l-before-fix-20261004.json) SHA-256 `F9A79E19BFB6B9C450244E9561E2F4274DB523A921810798634E764632B2414D`。合成物理 L 通过已绑定的原 InputAction/LocalFreeRun 输入，`FrameInputSet` mask16、正式 proxy index4=1，角色动作60、combo1一直0；八次有限 L 脉冲后探针 FAIL。它不是测试失焦：键已进入 FrameInputSet，只是被当成 Attack。旧控制器本就将物理 J/K/L 编到内部 jump/def/att；前轮 Q07 下游又转了一次，导致三键错位。前轮直接注入 `SimulationInputButtons.Attack` 的22tick结果未经过该物理入口，旧“玩家输入修好”结论已标 `SUPERSEDED`。

修正只精确撤去这层多余换算，并把聚焦测试从真实 `CharacterInputModule.SetAttack/Jump/DefendActionPressed` 起步。生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`：exit0、0 error、332 warning。原 Editor 仅筛选六项 EditMode，job `fe325189d9cf4a20b59a06c57487770c`：[原始6/6 PASS](editmode-six-filtered-20261004.json)。首次筛选把 P1P2 命名空间写错而只执行5/5，首次 job 的延迟查询返回 `result:null` 原件仍保留；最终6/6是第二个准确筛选作业，不把总suite的8824项当成已运行。

[修正后原始结果](physical-l-after-fix-20261004.json) SHA-256 `16B33A5BCBA0A160B303674B3F8E870442F58E7A627574D43614FF7EBFA09D22`：原 Battle Scene 生产 Driver 起始tick878，防御消费tick880/action110/combo1，方向tick882/combo2，跳tick884/action240，首253=tick911；合成物理 J 在tick912/phase0进入 `FrameInputSet` 和正式 proxy，同tick转301，PP500→350→250。与当前336B44正式源码首253 CSV以相对tick配对，严格34tick×4字段135/136，唯一不同是探针开始后第一tick Unity已处于站立循环action3、正式action0；从防御实际消费相对tick2到转换tick34，动作/PP/combo1/输入相位[132/132零首差](first253-paired.json)。正式源码/根三窗口已有990/990，这次Unity只验证首个正窗口；不推断次253/254后真人按键或实际画面提示。

两次原Editor Play退出后都是非Play、唯一Battle Scene clean。每次运行前后四份保护文件的 SHA 未变：Battle `26432662102CC83F516D16260E040547C322BAA66A13F590011B8BE9EC6C995F`、Menu `9EAAA0B4782974D74A017C367C9D5C77326C31D4281A2D820D1CBBA76986C1BA`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`。没有保存/回退用户Scene。Change Ledger validator本轮exit0；仍有仓库历史 Record 声明路径警告。`git diff --check`对本Change两份脚本没有新增空白错误，但全工作树的并行Battle Scene含尾空格；保留该用户修改。

范围结论：双重换算的新生产首差已解决，Q12自然组合技首正窗口通过；新Q09直接注入诊断尚未重新运行，Q12完整随机表可比战斗、非例外画面、a7实际播放与有序退出重进仍待。只有可裁决的新首差才开后续代码包。
