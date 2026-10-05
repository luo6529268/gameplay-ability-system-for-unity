# 原 Battle Scene 持有挂点命令出口

2026-10-05：`PASS_SCOPED_ORIGINAL_SCENE_COMMANDS`。前包直接几何RED→GREEN两参数证据复用；本包只运行一次修改后共用消费者的原Scene两完整Driver tick，不重跑角色/24tick/32tick/全套矩阵。

[运行原件](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-held-anchor-20261005-01.json)：原Unity PID19040、原Battle Scene、正式LoganRuntime/项目模式，run `d024-held-anchor-20261005-01` **PASS/DONE**、global5→7、两行离散输入。Play克隆在Start前配置OID2/7，普通鸣人和地面正式OID120由生产kind2自然拾取，未直接注入held关系。全局tick7/current publication7/central plan7、displayAlpha1，双方各1实际Entity command。

| 检查 | 原件实测 |
| --- | --- |
| 动作/类型 | holder115/type0、weapon24/type1 |
| source X | 201/214 |
| source Y | 0/7 |
| source Z | 481/482，差1 |
| reciprocal relation | holder link101/target50；weapon link-1/holder0 |
| projection | horizontal1.536384096024006、vertical1.5780821917808219 |
| 实际command挂点世界位置 | 两端均(-10.00246524810791,-6.465000629425049) |
| 挂点显示像素差 | X0/Y0 |

测量从实际`BattleRenderCommand.Position/Size/Pivot/FlipX`与正式帧WPoint反算世界接触点，再除当前UnitsPerPixel；未直接调用被测补偿函数作为期望。由完整Driver发布的不可变快照经中央命令消费者已带正确补偿；采样暂用既有30显示政策，结束恢复原政策。该政策不是设备真实FPS实测。

当前权威根EXE重算仍为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；源码`BattleWorld28::settle_held_refill_objects`的双端WPoint位置公式/cover0使Y7+Z差1等于本地Y差8，X差13，实测成立。初始Z542在项目地图tick1被钳为481，正式背景夹具为542；本报告只验证保留地图例外下的关系及相对挂点，不宣称两边绝对Z同值、同seed全World或GPU逐像素一致。源frame115/24与X201/214复用正式源码既有自然24tick证据，不重新构建正式源码/EXE。

生成Editor命令`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`退出0、301warnings/0errors、8.64秒；原EditorMCP刷新后DLL晚于脚本、Console0error。编译刷新期间一次连接拒绝，确认同PID/端口后重取连接，未重启Editor或测试。

[操作前/启动/退出原件](../NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001/)及[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD28-336B44-Q07-D024-HELD-ANCHOR-REQUEST-20261005-001/RECORD.md)：结束非Play/Scene clean/root11/Console0error，Scene SHA `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`保持；68字节旧请求逐字节恢复，SHA`7486F5BB3126869EC1D32C6033B68C6A94A19797573BCD0AEE21ED220B985BF1`。结果SHA`6EF6CB7855CFC6F521FF42621B1C1F7E8B2505B362B0D850104DD8A3371740BC`。没有删除文件、保存Scene或改生产/资源/非战斗。

本诊断Record限定VERIFIED；生产Record仍RUNTIME_PENDING，GPU、原版根同帧Present、真人键和其它显示政策/朝向未知，不以本包关闭Q07/Q09/Q12/总目标。必要ONE完成回0；按当前总表证据复用，新的真实非例外首差才扩大相邻回访。
