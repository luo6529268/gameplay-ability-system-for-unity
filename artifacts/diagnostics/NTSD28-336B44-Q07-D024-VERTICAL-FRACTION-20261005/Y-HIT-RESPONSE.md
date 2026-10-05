# D-024 空中纵向边界：生产候选与伤害提交邻例

2026-10-05 状态：`FOCUSED_TEST_PASS / ORIGINAL_SCENE_AIRBORNE_HIT_PENDING`。这是共用Y矩形投影修改后的最窄必要回归，不是新增角色技能或扩大角色矩阵。沿用 [现有Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001.md) 和 [Change Record](../../../docs/ai/CHANGE-RECORDS/NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001.md) 在脚本前增补的测试所有权。

实际只修改 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestProductionIntegrationEditorTests.cs`，新增一个方法 `AirborneVerticalBoundaryPreservesCommittedDamageAcrossViewScale` 及专用位置助手。复用既有241帧、500HP、普通角色夹具，经过生产 `CaptureCollisionFrameSnapshotsAll → CollectCollisionCandidatesAll → PostInteractionTickAll → EndCollisionCandidateConsumption`，没有直接调用DamageWriter绕过候选。ForceRoleAware收集器使用已存在的诊断直达开关；两配置分别为恒等视野与2048×1152固定视野。

本夹具使用内存构造的 bdy/itr；没有加载正式角色DAT，也没有修改任何磁盘DAT。攻击者源Y=-40、itr源Y区间[-60,-20]，受击者源Y分别为-11/-10/-9，其bdy区间分别[-21,-1]/[-20,0]/[-19,1]。正式 `collision_geometry.cpp::overlaps_2d` 是严格相交，接触不构成相交；`hit_candidates.cpp` 使用源位置/centery构造矩形。两文件及 `battle_world.cpp` 均列入对应playable的build.ps1 core闭包（第71/72/79行）。

| 几何 | 两配置候选数 | 受击者HP | HitCount | 攻击者/受击者hold | arest/vrest | CRT增量 |
| --- | ---: | ---: | ---: | --- | --- | ---: |
| 源Y=-11，相交1像素 | 1 | 490 | 1 | +3 / -3 | 4 / 1 | 2 |
| 源Y=-10，仅接触 | 0 | 500 | 0 | 0 / 0 | 0 / 0 | 0 |
| 源Y=-9，分离 | 0 | 500 | 0 | 0 / 0 | 0 / 0 | 0 |

六例均检查两实体源Y保持-40及各自输入值。高度比例没有导致重复扣血、接触误命中、源高度改写或额外火花随机消费；停顿和攻击/受击间隔也在这条C14提交链中验证。

## 实际运行与错误记录

- 生成Editor首次构建报两个CS1061：测试误读 `NativeRandom.CrtCalls`，修为既有 `CaptureScalarState().CrtCalls`；没有生产编译错误或改动。
- 首轮只含伤害/计数/源Y/CRT断言的原Editor job `b5d4063ddab746fbb5fe8015b79e044c` 为6/6 PASS，[原件](original-editor-y-hit-response-result-20261005.json)。
- 追加停顿/rest断言后，job `b9cc60e2fcbd426386c4ff058a1026dc` 4/6 PASS；两个相交例的攻击者预期误写为-3，实际+3，[失败原件](original-editor-y-hit-response-final-result-20261005.json)保留。追当前正式 `battle_world.cpp` 第7044～7068行调用分支确认，`release_native_attacker_motion_hold` 的正值反转仅属于target type3，普通角色保持+3。第3939行孤立注释的“default final -3”不定义全类型规则。只修正测试预期，未修改生产。
- 最终原Editor精确 `testNames` job `966d8e3d84964d8dad3f3ba3dab55153` **6/6 PASS、0 fail**，[最终原件](original-editor-y-hit-response-corrected-result-20261005.json)。工具progress.total为项目发现总数8837，实际result.summary.total=6，未运行全套suite。
- 最终 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 退出0、301 warnings/0 errors；原Editor每次均通过MCP刷新导入，末次Console0error、单一Battle Scene clean/nonPlay，磁盘SHA保持 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`，[末态原件](original-editor-y-hit-response-poststate-20261005.json)。
- `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 退出0、PASSED，18个当前diff脚本均有Record覆盖；仍存在历史Record声明路径不在当前diff的警告。没有运行SelfCheck、完整Driver、Play或正式EXE空中同输入重放。

## 限制与恢复

这只关闭内存夹具在原Editor的**共用纵向候选→普通角色伤害提交邻例**，不能推出原Battle Scene或正式根EXE空中命中、全部对象类型、全部模式、完整World/checksum、帧反应/速度全部字段或GPU画面已一致。此前C040第25tick为先命中后抓取，Y=-14是抓取后的末态，不作为空中命中前置证据。原Scene空中命中和R120中间alpha继续按总表终验/真实首差条件门，父Record仍RUNTIME_PENDING，Q07/Q09/Q12及总目标开放。

用户批准的1.5倍战斗实体显示尺寸、DAT不改值、固定完整背景、项目地图、非战斗和Unity/GAS框架边界保持。回滚仅移除本新增测试与专用助手，保留原测试及用户其它修改；旧失败、结果和资源不删除。
