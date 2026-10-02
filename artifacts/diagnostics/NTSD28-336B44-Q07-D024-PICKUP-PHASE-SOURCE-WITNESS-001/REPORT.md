# Q07/D-024 原 Battle Scene 武器拾取相位与源坐标限定见证

状态：`VERIFIED_SCOPED_DIAGNOSTIC / WPOINT_PARENT_RUNTIME_PENDING`。规则身份为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；逐 tick 数据对照使用其声明 playable live source 在当前树重编的 OID2/OID120 24tick CSV。重编源码探针不等于根 EXE 本体同条件证明。

此前原 Battle Scene 的物理 J 键诊断在首 tick 相位0、鸣人动作60，50tick 未拾取。此轮只扩现有请求式 Editor 诊断探针，显式把世界 `InputPhase` 在受控角色/武器出生前从1配为0，并为临时 OID120 设置源规则 X/Z；生产输入映射、DAT 和 Scene 均未改。新唯一结果 [`naruto-physical-x190-336b44-phasepair-source-01.json`](../NTSD28-Q07-NARUTO-PHYSICAL-PICKUP-PLAY-001/naruto-physical-x190-336b44-phasepair-source-01.json) 记录 `PASS_SCOPED_PHYSICAL_CHAIN`、源出生已初始化。19个完整 Driver 相对tick中：第1 tick相位1/动作0，第2 tick相位0/动作115/link101/武器frame24；全局拾取tick7、站立tick12、空中tick19、鸣人动作30 tick21。J/K由诊断探针注入 InputSystem 物理键事件，非真人手动按键。

正式源码 CSV 与原Scene共同的前7相对tick使用相同 J×2→Neutral 输入。逐tick比较输入相位、角色动作/link/child、角色源X/Z、武器动作/link/holder和源X/Z共77格，其中75格原值相同；仅首tick空关系哨兵源码0、Unity -1两格不同，此时双方均未持有。正式源码第8tick后改用方向键，Unity此轮改用K跳跃，两流不再同输入，不能比较后续动作/源轨迹或称19tick完整规则同态。正式源码世界Z为542、原Scene Z为287；比较时按项目地图差值255平移Z，未要求采用原版背景。

场景持有的相对tick2～19共18tick，武器源规则坐标已初始化；每tick以双方源整数X/Z间距分别乘 `2048/1333`、`1152/730`，与物理X/Z间距比较，最大绝对误差X=`0.536384096024`、Z小于`1e-12`输出像素。tick2源X距13、画面距19.436609、目标19.972993，源Z距1、画面距1.578082、目标1.578082。只证此武器持有链与项目D-024比例；canonical非武器、更多武器/挂点、根EXE同输入及像素画面仍待。

验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` 退出0、253警告、0 error；原Editor PID105896脚本刷新后完成Play，最终idle/nonPlay/noncompiling，`sceneCleanAfter=true`、Battle Scene前后SHA均为`93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`。Battle/Menu/GameConfig/Mode Asset、旧失败JSON、当前源码CSV六保护SHA见 [`protected-before.json`](protected-before.json) 与 [`protected-after.json`](protected-after.json)，6/6不变。旧Temp请求在改写前复制为 [`request-before.json`](request-before.json)，新请求仅消耗`requested`布尔；新结果使用唯一runId，旧报告未覆盖。无项目资产删除。

结论：此前“不拾取”可由探针初始输入相位错拍复现并通过配对消除，不构成 WPOINT生产写者首差。WPOINT父包可补记“原Battle Scene此武器链限定通过”，但状态仍为`RUNTIME_PENDING`，Q07和总目标开放。
