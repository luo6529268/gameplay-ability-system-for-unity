# Q05 WeaponState退休载体清理

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。仅在 ReleaseTick 包本轮 SelfCheck/Play出口证据齐备后选择；五类退休载体父任务继续，HolderCopy单独后继，Q05/总目标未完成。

先读 CURRENT-AUTHORITY、Q03 JOINT-FIELD-MATRIX、已 VERIFIED 的 NTSD28-B6-LEGACY-WEAPON-STATE-BEHAVIOR-RETIREMENT-PRODUCTION-001及 runtime 当前完整引用。参考前包 weapon-state-next-references.txt 仅定位，建立准确 Record 后方可修改脚本。

删除 NTSDEntityRuntime.WeaponState/default/init/copy/reset、Weapon 初始化、ECS fingerprint/checksum/parity 与实际别名/诊断载体；逐项追踪全部 partial/caller，不按名称批量替换。必须保留真实 frame.state、LF2States.WeaponThrowing 等常量和 GetResolvedWeaponStateForExternalUse 真实帧状态解析。测试/fixture 的 weaponState 参数若用来构造帧，则是有效状态，禁止删除。Goal20 probe 已用 frame 状态验证，应保留其实际动作与生命周期断言。

旧字段保留与 sentinel 断言先分别区分数据结构和已退休行为；新增 absence/真正帧状态测试 RED，再写生产。范围内 SelfCheck/旧测试/Play报告必须同步，尤其 lowercase JSON weaponState 与反射引用，不能只全字匹配。保留原失败证据，不用默认0伪造已删除字段。运行相关 prepass/held/hit/throw/snapshot/ECS/hash/完整SelfCheck和真实场景定向验证；保留 Link/Holder/Target、TrackerParent/Owner/Spawner/2F8。

当前12/20/23/1/1仍同Q05未发布中间态，后续HolderCopy、步骤3 identity/双OPoint guard、步骤4统一13/21/24/2/2、步骤5旧版本拒绝与回放/Play不可跳过。R13仅按子条件回访，不提前关闭R13/R15或发布Q07资源。

保持Unity/GAS、非战斗、33ms/3ms、十一阶段、Scene/InputActions/Gen/Plugins/外部Server包、stage.dat USER_HOLD和例外。禁止computer-use，仅桥接/日志/结果/进程；保留Foot18既有缺失/用户新图与Scene旧SHA。准确路径事前Record/预变更SHA/最小差量，回滚须明确批准，不清未知文件。

限定出口：282/282、完整SelfCheck、当前OID124两次pre-frame Play前后有效字段一致，对象4→4，附带四释放用例PASS。Record/artifact同ID；下一唯一NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001，联合后继不变。
