# Q06显示递推与slot边界阶段证据

父Task NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001仍IN_PROGRESS；当前实现阶段FOCUSED_TEST_PASS / SCOPED_PLAY_PASS。出生初始化未完成，不得将整个display任务、C25或Q06标为VERIFIED。

当前准确四脚本：新BattleNativeDisplayWriter只写四显示字段；SimulationWorld新增显示专用当前slot查询，保留注销/dormant排除但允许仍占slot的PendingFlushDestroy；BattleLateEntityLifecycleModule在原每slot恢复后、computer/frame前调用，早退的占槽pending只更新display；新Editor测试及请求探针。无全局active放宽、无新循环/manager/queue/可变字段/版本，所有HP/MP/累计真值与其他tail职责保持。

依据原版advance_native_display_values_slot及C25调用链、980原函数向量。累计显示在小于目标时按独立step加，HP显示在大于目标时减，另一分支赋目标；跨越不当次clamp，下一tick纠正。null slot不更新，不能使用HP资源的type0/current-frame/pending过滤。

## 运行证据

- 原10项RED job a5b754a410eb4660b01be35136b863f7：9FAIL/1PASS；缺writer、pending显示9不变、HP恢复107时display仍0。red-results.xml保留。
- 实施后原10/10 PASS：e95fe82ab9314b78af430a44d9132116；focused-initial-10.xml。
- 扩展为13个display测试，另联合HP/MP/两profile实际Logan/快照回放、C25 timers、显示载体与命中step：job5a7a3ed17dce44c49a0d3dece7815fd0总181，177PASS/4FAIL，新13全PASS。related-181-initial.xml保留。
- 四失败属于独立旧测试：C25 poison fixture未指定非HP恢复phase（3例HP多1）；carrier schema仍12/20/23（1例）。独立C25-POISON-PHASE-FIXTURE和C25-DISPLAY-SCHEMA-FIXTURE只改两个测试方法，不改生产，原poison/严格版本断言保留。
- 定向job169f777ca0a64f1ca6497c38ccb90e76的6/6通过覆盖所有4FAIL。merged-test-evidence.json按fullname核对181不同测试都有PASS证据；不能描述为单次181全绿。
- 完整SelfCheck新鲜PASS（SelfCheck-pass.result，请求UTC同目录）。编译CS错误0。
- 真实NTSD_Battle旧内容Scene，tick5后程序注入四字段与source，通过公开driver连续两tick：13/26/499/498→10/32/500/500，HP/HPBound/score/cost真值500/500/10/30保持；finally完整World/checksum恢复，对象4→4。play-display-pass.json。这不是出生初值、物理技能或正式图片表现证据。
- 既有Q05探针随后真实恢复并有序关闭：World/slot/logic borrowers/render borrowers全0，两帧仍Stopped；play-cleanup-pass.json。Editor正常退出Play。
- scene-final.json：用户HUDBg x30/Scene bcd1047b…保持，dirtyfalse/root14。workspace-protection.json：3059中2923保持，较HP新增变化路径仅declared LateModule及两个独立fixture，0新增缺失。

实际入口：Goal13_bridge.py refresh_unity/get_editor_state/read_console/run_tests/get_test_job；完整SelfCheck request；manage_editor play+DisplayPlay.request与Q05 ReplayPlay.request。一次get_test_job桥接观察超时后重查同job继续，未重启Editor或重复启动测试。全程禁止computer-use。

## 尚未完成的硬前置和下一动作

本轮补读source materialize_frame_spawns:7670-7706确认出生资源选择：正point.hp/mp优先，否则OID5/52=10/5、其余500/500，再用正stats.ohp/omp以int64乘除100；该HP成为spawn_at两个HP显示初值。Unity两PostInitLiving仍只硬写OID5/52，Other.InitializeHealth还以weapon_hp作HP。直接复制当前旧Health值不能证明完整出生显示对齐。

下一唯一Task NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001 / READY_FOR_EXACT_PRECHANGE_RECORD。先原函数完整materializer见证与两caller范围记录，再原子修出生HP/MP及display初始值；普通bootstrap/Stage/clone/snapshot-shell/复用各入口区分。该资源事务也是Q06 OPoint家族前置，应在此先处理，不等待Q07；完成后回到本父Task补完整出生初始化及联合验收，再接POST-DISPLAY-RESOURCE-TRANSACTION。

R05/R07仅本显示递推/实际时点子条件PARTIAL_RETURN；出生会再触发R09/R16。本轮R16清理实测通过，不代表将来出生改动免验。正式DAT/图片部署、post-display及其他Q06/全场仍待。13/21/24/2/2、33ms/3ms、十一阶段、Unity/GAS/非战斗和全部例外保持。

最终账本PASS：514 records/17 governed code diff，详见ledger-final.txt。父完整display仍IN_PROGRESS；下一出生资源事务具备继续条件，无用户输入阻塞。
