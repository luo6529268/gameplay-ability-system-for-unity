# 持有对象释放对照进度

状态：IN_PROGRESS，Q06仍未完成。

- 源1150双跑一致，独立296552检查通过；源诊断来自当前正式playable构建闭包，不是正式EXE画面验收。
- 正常factory：Authority400/MobileExtended各1150行before/立即/following零差异，job b573b82a834a415c9925f12441a11b0f，release-after-fix。
- 通用LF2OtherObject武器DAT适配：各664行零差异，job f60db972d4724aacb78ecd0a5c51aa1d，release-generic-pass。仅1150中type1/2/4/6子集，不证明正式factory自然生成组合或未投影private cache；没有renderer测试。
- 关系查询5/5、旧缺失帧及生命周期91/91通过。首次组合请求只实际发现5项，已修正两个类的namespace并单独实跑91项，没有将发现总数当执行数。
- Unity编译error CS为0；Validate-ChangeLedger exit0；git diff --check exit0（已有CRLF提示）。
- 最新完整SelfCheck于2026-09-14 20:35:34 UTC失败：R5-HOLD-002要求真实type1释放写Spawner，而source使用独立+2F8。原失败保留self-check-203534.result；测试oracle独立审计待处理，不能报告SelfCheck通过。
- Scene SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。

下一：独立核对并留痕修订旧held-release Spawner/+2F8 oracle，重新SelfCheck；然后本release子集snapshot/replay及真实Play/关闭验证。初次140旧证据保持但不能替代新release验收。refill、剩余live readers及display/post-display后继继续保留。Q07资源迁移尚未执行。

## 后续证据

旧字段oracle已在独立HELD-RELEASE-FIELD-SELF-CHECK-ORACLE-001限定修订并VERIFIED；20:39:17Z完整SelfCheck PASS。source1150两profile同World replay已2/2 PASS，共2300场景/4600重放tick。Play run-release4600仍在运行，当前不能宣称Play通过。

下一补给读者只读定位：source battle_world.cpp7876..7940先判断system_rules的HP/MP对象id及holder native state17；耗尽独立同步抽样callsite0x4181C9/0x4182C0、双方action0/counter0、child weaponHp0，不更改Zz。Unity ConsumeDrink仍有两个legacy held setter及BattleRandInt(0,7)和PS.zz=0。需同源向量实际比较后决定精确修改，不能由静态差异直接宣称全事务错误或重做已关闭refill职责。

## Release限定验收完成

真实Play4600 PASS（两profile两factory各1150 before/立即/following0）；Scenechecksum保持、Renderer2→2。20:45:41Z关闭PASS，restore4→4/worldslots两pool0/两帧Stopped。场景dirtyfalse/root14及SHA保持。旧Play运行中描述由此覆盖。release范围已验，父held任务继续refill与其它已登记读者；不代表整个Q06或正式资源/视听完成。
