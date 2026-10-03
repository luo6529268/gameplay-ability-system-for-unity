# Q07/D-024 普通输入平台链原 Battle Scene 限定验收

状态：`VERIFIED_SCOPED_NATURAL_SCENE_NO_CARRY`。当前正式根 EXE 身份为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；同条件多由也36/飞段56/鸣人2普通动作、输入与96tick在正式源码/根各[960/960字段零首差](../NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-ENTRY-001/REPORT.md)，根报告 `nativeParityClaim=false`。本Scene包只扩既有 [C040 Editor-only 场景探针](../../../Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C040NaturalScenePlayProbeEditor.cs) 的独立 `mode=platform` 请求与结果路径，保留原C040模式。Unity继续用项目自有地图与完整背景；正式背景1仅用于权威诊断的共同Z400前置，不引入项目。

原项目 Editor 自动导入新增测试分支；生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 为 **0错误、276个既有/普通字段警告**，[完整构建输出](generated-editor-build.log)。唯一 [Scene Play结果](platform-natural-20261003-01.json) `CAPTURED/DONE`：从保存的Menu进原Battle，Play clone roster配置为OID36/56/2、规则X100/200/185、Y0/Z400、team1/2/1、mode0/seed `0x28A55A5A`；三人action0，多由也防御→上→攻击按正式2tick段，鸣人tick9–10跳跃，执行96个 `SimulationTickDriver.StepOneTick(FrameInputSet)`，全程没有中途改实体动作/位置/链接。正式按键到现有Unity旧字段桥的测试输入分别为源防御→`SimulationInputButtons.Attack`、源攻击→`Jump`、源跳跃→`Defend`，Up同名；这不是玩家硬件键盘的直接取证。

原Scene相对tick1–96与正式根trace按三实体动作、规则X/Y、HP及鸣人平台参考/来源/阴影15字段[1440/1440同值，无首差](paired-scene-selected-fields.json)。正式源码与原Scene均在tick22多由也243、tick23飞段182、tick29–31鸣人平台来源槽1；tick29–31碰撞参考−50/−58/−64，鸣人规则X185始终不动。三人 `viewX` 对 `sourceRuleX × SourceDeltaToViewX(1)` 的最大数值残差 `2.2737367544323206e-13` 输出像素；比例值 `1.536384096024006`。这检验逻辑/物理坐标，并非Game View实际像素验收。受控初态 frame182 可连续搬运十tick的既有证据不因本自然链未搬运而失效。

探针复用原有十一阶段有序关闭：`Completed/RuntimeMapCleared`，World对象、运行槽、池借用、活动池对象/Sprite均0，pool quiesced、World detached。退出回原 `NTSD_Menu.unity`，MCP实查 `isDirty=false`；Battle/Menu/GameConfig/ProjectBattleModeConfig四SHA在[前](protected-before.json)/[后](protected-after.json)逐项相同。没有修改正式DAT/图片、Unity生产、Scene、相机、菜单或其它非战斗逻辑。

本包只关闭**这条普通输入三人链的原Battle Scene字段/比例与有序关闭子门**。它没有自然非零平台搬运、正式根EXE实际Present像素、完整World所有字段、其它对象/时机或真实硬件物理键的证据；Q07/D-024、Q09/Q12及总目标仍开放。按总表停止扩张这一阴性矩阵，后续寻找另一条正式可达首差；若未来得到自然非零搬运样本，再复用同一共享投影入口做原Scene定向验收。
