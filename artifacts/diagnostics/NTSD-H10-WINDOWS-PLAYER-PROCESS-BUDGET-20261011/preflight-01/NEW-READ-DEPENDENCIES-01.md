# PRE-FLIGHT-01 具名补充读取依赖

状态：只读方案域；未授权实施/build/Player/Settings/sidecar。原R1-R3及R4前半已由主会话逐段供证，执行会话本轮未重新展开这些方法体。供证明细与采取决定记录于后续ADDENDUM，不把主会话阅读写成本会话实际阅读。

| 请求 | 路径/符号/最小候选范围 | 具体要裁决的事实 |
|---|---|---|
| R4A | `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`：`ResolveConfiguredLoganSource` 50–70；`PrewarmConfiguredLoganContentCoreAsync` 99–196 | Player相对formal root实际解析及当前预热直接消费者；只提取路径/调用边，不展开parser/writer/生成器。若还有raw UI loader，先给后继具名段。 |
| R4B | `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs`：常量及`OnPostprocessBuild` 1–71；`ReadManifest`/`VerifySourceSet`/`VerifyDestinationSet`及直接复制校验辅助72–150 | 已有Windows自动postprocess是否会在新builder之前部署同一路径，现用manifest及额外文件门；新builder必须复用/校验已有文件，不能把现有相同SHA文件当冲突，也不能绕开postprocess或改旧processor。实际入口额外调用则先定位报告。 |
| R5A | `Assets/NTSD/Scripts/UI/SelectRoleItem.cs`：`OnJoin`341–356、`OnConfirmCharacter`376–388、`OnConfirmTeam`409–420、`SelectCharacter`495–505、`SelectTeam`506到下一方法声明；`CharacterSelectionController.cs::SyncPlayersFromSlots`566–592 | 使用同一UI公共入口加入2/16/507三个人类槽、指定同队并按真实inputId落盘；不是修改MatchConfig players冒充UI。未读slot私有输入/导航正文。 |
| R5B | `Assets/NTSD/Scripts/Test/NTSD28Q07WindowsNaturalSkillProbe.cs`：`Start`87–136、`Update`137–220、`ResetPulse`/`PulsePendingKey`/`QueueKeys`221–256 | 现有Development Player的InputSystem投递时机、键盘所有权与LocalFreeRun关系。仅诊断入口，禁止照搬其循环重试或改变生产Driver；将决定一次性有限按键表能否保留正常驱动。若需生产Input接口，先给精确声明/范围另审，不改Input Actions。 |

R4B来自既有Q07 PORTABLE/MANIFEST-V2/MENU-FIRST任务与验收报告，而非猜测旧builder行为。文件/符号目前只rg定位，未打开上述新方法体。`CharacterAnimtorManager`准确路径包含`Animation/Manager`；此前少一级Manager的rg返回路径不存在，不是源码缺失。

优先复用现有manifest和Q07部署证据，不能为找copy理由去读不可达legacy GameDataManager。以上没有Q06/factory/StageWave/producer读取授权，也没有旧probe运行授权。
