# C043 正式自然合体持有链：原 Battle Scene 对照

状态：`SCOPED_SCENE_PASS`。当前规则权威是336B44正式根EXE及对应playable；正式源与根在先行[C043入口报告](../NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001/REPORT.md)的60tick×14字段840/840相同。本报告只验证Unity原Battle Scene生产Driver选定字段，不代表全World/checksum、实际物理键、最终画面/声音或Q07整体完成。

原项目唯一Editor、非Play且idle时刷新新探针，生成`Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly`为0错误。探针仅在Play副本于`BattleTestBootstrap.Start`前指定Lee7/Chi8，用正式LoganRuntime、mode/difficulty0、同组、HP/baseHP100、MP500、seed682973786、Lee action0/X304、Chi action256/X300/朝左；两个玩家逐tick共同按右：23～24及27～60，其余松开。原Scene项目地图与固定完整背景相机均保留，DAT数值及Scene序列化未改。每步由生产`SimulationTickDriver.StepOneTick`推进；Play前已有5个场景启动tick，报告relative tick1～60对应全局6～65。

第一次请求`scene-01`在进入Play前因探针把GameConfig保护路径误写到Resources而失败，Unity日志原件保留；修为真实`Config/GameConfig/GameConfig.asset`，独立v2请求。第二轮[scene-02](late-double-tap-scene-02.json)在前41tick与正式源17字段×41=697项仅前5tick未持有时`chi_child`空槽哨兵不同；tick42 OID420自然退场时，`SimulationRegistryModule.ReleaseRuntimeSlot`先清其它关系字段、后让统一AI旧行失效，抛旧代数异常。该轮状态FAIL；四保护SHA及Scene clean，但退出池借用5，不能计作通过。没有删除/覆盖任何失败结果。

独立通用修复 `NTSD28-336B44-Q07-C043-SLOT-RELEASE-PUBLISHER-001` 在新聚焦测试取得相同异常RED 1/1 后，只把既有`InvalidateAfterOccupancyChange()`移到`ClearReferencesToReleasedSlot`之前；原Editor同例GREEN 1/1，邻近占用epoch和解融合旧行2/2。另有首版聚焦测试因在AI producer内强行注销首先触发snapshot epoch门，未作为目标RED，保留其job和记录。

修后原Scene [scene-03](late-double-tap-scene-03.json) 完成60/60生产tick，tick6 Chi产生OID420/slot50、子关系-1/父1；tick28 Lee7→51/action290、Chi休眠、child action70且关系-1→0、失效计数1；tick29计数0；tick42 OID420正常消失、主角action7/X355，后续至tick60未抛异常。与正式源CSV对照[机器结果](source-unity-selected-comparison-03.json)：17字段×60=1020项原值仅5项不同，全部是tick1～5未建立关系时正式`chi_child=0`、Unity`TargetSlotIndex=-1`的空槽哨兵；仅在`chi_present=1且chi_link=0`时将Unity`-1`视作正式无链接`0`，语义化后1020/1020，活跃关系字段无归一化。正式源和根先行840/840证书仍限其所选14字段，不能把两种对照拼成完整World证明。

修后Play退出`NATURAL_GATE_PASS`、`exitedPlay=true`、Scene clean、四保护资产前后SHA逐一相同、池借用0；`shutdownStageAfterExit=-1`表示驱动对象已销毁，十一阶段内部状态本报告不可观察，不能声称完整关闭轨迹已证。Menu/Battle Scene、GameConfig、ProjectBattleModeConfig及LoganRuntime窄Git状态无改动。C043自然生产链的上述出口限定通过；完整SelfCheck、物理键、其它失效路径及全World未在本包完成，C043/Q07/总目标仍开放。

交付核验：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <原项目>` exit0/PASSED，新探针及两处脚本改动分别被其Change ID覆盖；`git diff --check` exit0，仅现有文件行尾转换警告。原Editor现为非Play，未开第二Unity、未使用computer-use，未删除文件或覆盖旧证据。
