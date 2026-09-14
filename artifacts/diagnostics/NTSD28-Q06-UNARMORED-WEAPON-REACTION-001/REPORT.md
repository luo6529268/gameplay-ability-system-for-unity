# Unity 无护甲武器反应

VERIFIED / DECLARED_WEAPON_TRANSACTION_SCOPE。完整对齐总目标仍ACTIVE，Q07正式DAT/角色图片未部署。

本批生产准确三路径：BattleDamageWriter.ApplyWeaponDamage及局部Native水平/攻击者post函数；BruteForceSceneQuery effective kind0不再将heavy dvx/dvy减半；BattleEcsHitExecutionPlan对应预处理和独立writer预测。保留已有prelude、火花、HP/stat、C25和有序关闭所有权。不改Unity/GAS框架、Scene、资源、非战斗或Server。

已实现：武器reaction80保留；移除旧victim随机动作、阵营继承、自身rest尾部；按原双精度累积、type2低fall跳过Y/action、dvy0不夹紧；source raw frame写保留counter/latch；rest后state1002原同步0xEE/16及state3000/3007post；武器只发原type3攻击者broken事件（OID100另沿现有cue映射）。Shadow独立预测含native同步随机状态，保留所有旧差异和观察次数检查；瞬时观察字段不改变schema。

源依据：WEAPON-REACTION-SOURCE-WITNESS-001，2100/43202断言，两遍SHA28cad088cef6941c6b9c27cd4b2f0b5419cb8f47e106accd7078f9757e3eb55a。初值匹配后原Unity两profile各22105差异，见red-normalized；新direct两profile各2100全部0差异，见direct-first-pass。

测试更正记录：初始JSON double 1.0与整数1类型差异通过明确按源double读取消除，数值仍严格比较；Shadow旧测试仅调用Character pass漏48个非角色攻击者，失败保留shadow-entry-red。现在按原Character/Object pass观察同一冻结candidate，不更改生产调度或删断言。

后续验证在此追加。源fixture覆盖的合成DAT端点不等于物理按键/真实图片表现完全对齐；OID100、复杂held/broken-armor/effect等超出2100取样的边界不能以本报告宣称全部通过。跨World allocation epoch问题、raw三项MISSING与其他已登记依赖保持。

六项focused全部PASS（fe21d27b4a164f7ea9ca115d2b4c8964）：8400完整源对照，8个本地snapshot案例/16replayed ticks。父回归48项中42PASS/6FAIL：原34全部PASS、684早期四组和回放两组PASS、BDEFEND直接两组PASS；另两组BDEFEND仅type5各16个观察guard，完整984四组各846差异/108例，全部是已登记非角色reduced。无护甲weapon124已清零。完整SelfCheck10:16:55Z旧HitConfirm2断言FAIL，另建WEAPON-REACTION-SELF-CHECK-ORACLE-001修订测试，父仍IN_PROGRESS。

## 最终出口

- 最终EditMode `d8cf1026806f47f5a0555ecdb63f9e7a` 48/48 PASS，XML `final-tests-48-pass.xml`。包括原34、新四矩阵（source2100×2profile×direct/Shadow=8400）、两profile的本地回放（8case/16replayed ticks）、整数X边界两例、六个SelfCheck入口。每候选prelude和writer均观察一次，Shadow差异0。
- 完整SelfCheck 2026-09-14 10:33:50Z PASS。七个旧oracle方法由独立WEAPON-REACTION-SELF-CHECK-ORACLE-001纠正；三次旧FAIL及定向RED都保留，不修改生产来迎合旧权威。
- 真实NTSD_Battle Play 10:36:58Z PASS：两factory×direct/Shadow×2100=8400，before/after0差异，Scene逻辑checksum保持、Renderer借用2→2。使用合成Logan格式fixture；不证明物理按键或图片已一致。
- 有序关闭10:37:31Z PASS：原位snapshot restore4→4，World objects、slots、logic borrowers、render borrowers全0，连续两帧Stopped；自动退出Play。
- 最终Editor idle/未编译/未Play，Scene dirty=false、root14、SHA bcd1047bf912c6a4a8bc9f3a76eaf3fa954211ad064e0402b1c01bf3ba0e9fb6保持；用户HUDBg30保留。Console error条目0，生产和正式EXE文件hash验收前后保持。
- 验证方式为现有Editor MCP、request-file SelfCheck/Play、C++诊断构建/trace、git diff --check及Tools/Validate-ChangeLedger.ps1。没有使用computer-use、第二个Editor、提交或推送。

后继唯一执行顺序：TYPE5-HIT-PLAN-COVERAGE-AUDIT-001（每profile16个无armor观察缺口）→NONCHARACTER-REDUCED-HIT-TRANSACTION-001（108例）→完整父矩阵/reader/表现依赖→Q07资源。当前DAT与CLR类型不一致时，weapon Shadow旧CLR guard仍须随覆盖审计确认，不能由当前两factory矩阵推断已覆盖所有自定义shell。复杂armor/held/OID100/effect的未覆盖边界继续随相关父任务回访，不以本批记录关闭整个武器领域或B1-B12。
