# Q07/C051 大蛇丸 effect23 原 Battle Scene 限定验收

权威为 2026-09-30 用户选定的根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，对应当前 playable 源码与正式非排除 DAT；旧 B1E13 证据不裁决本次结果。

原 Unity Editor `2022.3.62f3` 在原项目原 `NTSD_Battle.unity` 运行新探针。左右初态都是 OID20 大蛇丸 action288、OID2 鸣人 action0、seed682973786、mode0、正式 `Assets/NTSD/Content/LoganRuntime`，受控设位置／朝向／队伍后，中性输入推进生产 Driver 12 tick。右侧目标 X550，左侧目标 X350；不是玩家自然物理按键。每次只写独立 JSON 原件，不覆盖或删除旧文件。

| 案例 | Unity 结果 | 当前根对照 | 场景与配置保护 |
|---|---|---|---|
| [右向 X550](right-x550-v1.json) | `SCOPED_PASS`；子体 tick4 出生、tick8 action40、tick9 目标 action180/HP435、tick12 Vx -10 | [根 v1](../NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001/right-x550-root-v1-trace.jsonl) 同初态；12 tick×所导出动作/HP/Vx/子体存在、动作、X = 78/78，首差无 | Play 退出、Scene clean；四 SHA 前后相同 |
| [左向 X350](left-x350-v1.json) | `SCOPED_PASS`；子体 tick4 出生、tick8 action40、tick9 目标 action180/HP435、tick12 Vx +10 | [根 v2](../NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001/left-x350-root-v2-trace.jsonl) 同初态；78/78，首差无。历史左根 v1 朝向不符，不参与本结论 | Play 退出、Scene clean；四 SHA 前后相同 |

逐字段对照及两个原件 SHA 记录在[比较原件](scene-root-selected-fields-comparison-v1.json)，两向各 78/78、`differences=[]`。

原 Editor 第二次 `refresh_unity` 才完成新脚本与 meta 导入；随后生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 为 0 error、235 warning。第一次刷新仅恢复 MCP 连接，之后旧生成工程的 0 error 不作为该探针编译证据。原 Editor 编译程序集在导入后更新。运行仅调用原 Editor 菜单，没有启动第二 Unity 实例，也没有使用 computer-use。

交付校验：`Tools/Validate-ChangeLedger.ps1` PASS（1100 Records、46 个 diff 脚本覆盖；历史 Record 路径警告保留在[日志](change-ledger-validation-v1.log)）；相关 `git diff --check` 无空白错误。

受保护文件 SHA-256：Menu `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、Battle `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。`LoganRuntime` Git 子树无变动。

本包可记 `VERIFIED / SCOPED_SCENE_PASS`，生产共用修复仍是 `RUNTIME_PENDING`。探针只导出选定字段，未导出对象池借用数及十一阶段关闭轨迹；effect22 正式根、护甲分支、自然物理按键和全部场景表现没有由此证明。C051、Q07 和总目标保持开放。
