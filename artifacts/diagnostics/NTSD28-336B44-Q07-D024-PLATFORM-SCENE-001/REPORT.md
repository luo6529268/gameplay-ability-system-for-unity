# Q07/D-024 正式 frame182 原 Battle Scene 连续平台搬运

状态：`VERIFIED_SCOPED_CONTROLLED_SCENE`。沿用当前正式 OID56/action182 源 X200/Y0、OID2 目标 X205/Y−5、同 Z400、不同 team、中性输入的源码与根 EXE 受控阳性；正式源/根初态及 tick1–10 的选定九字段为 99/99。Unity 仍使用项目自有地图和固定完整背景，目标规则距离转换由 World `BattleSpatialProjection` 统一处理。

只新增 [Editor-only opt-in 探针](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q07D024PlatformBattlePlayProbeEditor.cs)，在用户确认保存、原 Editor 空闲后切到原 `NTSD_Battle.unity`。它从正式 LoganRuntime 注册两对象，初始化规则/物理双域，随后生产 `SimulationTickDriver.StepOneTick` 连续完成十 tick；没有中途写动作、坐标、链接或命中。原 Editor 实际导入，`Assembly-CSharp-Editor.dll` 时间晚于修后脚本，生成 Editor C# 工程最终编译0错/276警告。

首轮 [原始 FAIL](platform-182-20261003-01.json) 数据已记录正确的源位置与比例，但测试把规范化后平台槽1与实际诊断槽50比较，导致测试自身误判。仅修该断言后，第二唯一 [Play结果](platform-182-20261003-02.json) 为 `PASS_CONTROLLED_SCENE`：相对 tick0–10 源 OID56 保持frame182，目标第1 tick 建链，规则 X205→178；[与正式源码 99/99字段同值](platform-182-20261003-02-paired.json)、首差无。实际物理 X 与当前视口统一投影的最大差 `2.2737367544323206e-13` 输出像素；这属于数值残差，未取得 Game View 像素证明。

本次两轮 Play 均退出；探针对象数4→4、槽2→2、池借用2→2，测试对象已解除注册。最终 Editor 已恢复 `NTSD_Menu`、`isDirty=false`、非 Play；Menu/Battle 文件 SHA 分别保持 `5D79DBB7...EC0052D` 与 `93448372...7BF60`，没有改 Scene、DAT、图片、菜单或生产脚本。Ledger validator PASS，生成编译与 `git diff --check` 另见 Change Record。

本结果仅证明**受控初始动作**下的当前正式资源、生产完整 Driver 与 D-024 比例出口；普通玩家自然按键可达性、正式 EXE 实际画面像素、其它对象/视口边界及 Q07/Q12 仍开放。
