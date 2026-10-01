# C051 大蛇丸 effect23 原 Battle Scene 左右验证

状态：VERIFIED（限定原 Scene 出口）。父任务 Q07/C051 仍为 RUNTIME_PENDING；本包只关闭左右受控 Battle Scene Play 验证。

权威：当前根正式 `NTSD2.8-Logan.exe` SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；自然可达OID20/action288→OID888/action40在tick9 effect23/dvx-10命中OID2，右X550两人朝右/左X350两人朝左，正式源和根LFR各80tick×8字段同态。Unity raw限定比较在同seed682973786/mode0/正式DAT下左右各12tick×99字段同态。项目自有地图Z边界为批准例外。

只增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C051OroScenePlayProbeEditor.cs` 及新meta，遵循现有C053原Scene Probe启动/关闭架构。两个明确菜单入口分别在原Battle Scene Play clone中预Start配置正式OID20/2、暂停真实World、设置源X/朝向/队伍/seed/当前模式并以生产Driver中性输入推进12tick；每例落单独只写一次的结果JSON。不得写Scene、Prefab、DAT、GameConfig、非战斗代码。不得删除或覆盖任何文件；不用临时请求或computer-use。

先核原Editor idle、当前Scene路径及dirty false，保护Menu/Battle/GameConfig/Mode Asset的SHA；遇World/配置/角色不符则停止该例并写FAIL原件。每例验证出生OID888、tick9目标HP435/action180、tick12方向速度（右-10，左+10），退出Play后Scene clean/四SHA保持、借用与关闭状态按现有诊断可见字段记录。与根选定字段对照按既有raw证书的范围陈述，不把控制初态当自然物理键，也不凭两例关闭C051/Q07。先编译和定向运行一次各方向，失败保留原件；不扩角色矩阵。

回滚只移除此新增探针与meta（需按文件操作合同另行记录和授权），保留报告与所有既有用户文件。该探针只持有静态Editor状态，结束时清空引用；依赖既有BattleRuntime十一阶段关闭，不新增生产owner。

实际验收：原 Editor 导入新脚本和 meta 后，生成 Editor 工程 `dotnet build --no-restore -v:q -clp:ErrorsOnly` 为 0 error/235 warning；原 Battle Scene 右 X550、左 X350 分别 Play 12 个生产 Driver tick 并正常退出。两例子体 tick4 出生、tick8 action40、tick9 目标 action180/HP435、tick12 目标 Vx 右 -10／左 +10。两例各 78 个已导出的角色动作、HP、Vx、子体存在／动作／X 字段对当前根 trace 零差。两例报告 `SCOPED_PASS`、Scene clean、Menu/Battle/GameConfig/Mode Asset 四 SHA 前后相同；LoganRuntime 无 Git 差异。借用数及十一阶段各步未由本探针导出，不作为本包已验证项。证据见 `artifacts/diagnostics/NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001/ACCEPTANCE.md`。
