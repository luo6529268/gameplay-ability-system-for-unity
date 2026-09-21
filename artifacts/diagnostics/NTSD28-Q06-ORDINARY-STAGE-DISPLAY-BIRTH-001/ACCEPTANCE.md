# 普通与Stage出生显示初值验收

2026-09-21，限定scope ORDINARY_STAGE_DISPLAY_BIRTH。父display仍有state9996 direct-spawn资源依赖；本包不关闭父display、Q06或总目标。

## 最终改动与权威

正式BattleWorld28.spawn_at把request.hp赋给两个HP显示，新EntityState的两个累计显示和四step为0。当前正式playable GameSession Stage SpawnRequest.hp为Stage最终HP。原display980向量身份仍为正式B1E13A…D2819033 / closure07CD47…778F；当前文件SHA449281AF341C5BB5574D144819FCE5274F5967FED5A532FF13420E8BC4941D14，与原测量一致，复用而不重建无变化source矩阵。

增加BattleNativeDisplayWriter.InitializeBirth(runtime,hp)，只写八个display字段；LF2Character.Initialize在health赋值后调用；Stage factory、最终Stage contract及Results reserve在最终HP写入后调用。未修改生命值公式、OPoint百分比/lives/weapon值、display递推、池Reset、融合或snapshot恢复。snapshot shell的临时初始化随后仍被完整runtime快照覆盖。

精确脚本4个：BattleNativeDisplayWriter.cs、LF2Character.cs、SimulationStageWaveModule.cs、新NTSD28Q06OrdinaryStageDisplayBirthEditorTests.cs。AppManager、Scene、资源、输入、普通HUD与框架不改。

## 实测

- 初始jobdbc0225d91f24b2e859214beab782b4f：8FAIL。七个真实差异：普通/direct Stage显示0而非137，污染显示101未清，factory/type3/reserve/pool复用显示500而非137。另一个是snapshot夹具错误要求不同CLR引用；原文件red/TestResults.xml保留。
- snapshot夹具改为capture→clear local shell引用→释放原实体并确认目标slot空→正式restore；允许池合法复用同CLR，断言八个非零marker保持。未修改生产restore，也没有恢复后补写字段。
- 联合jobf957ffb0f4f24a4aa217fbad037dd7a3：21/21PASS，0.9658268秒；新8（含factory type0/type3）＋原display13（含980向量）。joint-pass/TestResults.xml。
- 一次稳定fullSelfCheck PASS，UTC2026-09-21T08:15:15.4555115Z；full-selfcheck.result。
- 实际Play PASS2，UTC08:15:55.9229468Z：普通character Initialize137及Stage type3 factory final137，均带pooled Renderer；原scene checksum不变，render borrowers2→2；play.result.json。
- Q05关闭PASS，UTC08:15:56.5045127Z：恢复对象4→4；关闭World/slots/logic borrowers/render borrowers全0，Stopped保持两帧；closure.result.json。
- Editor编译结束，Console error CS匹配0；Scene dirtyfalse/root14，SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。
- 独立生产/fixture只读review PASS；root检查最终diff。Ledger PASS621 records/104 governed dirty scripts，计数是全工作树；git diff --check exit0。

## 限制与后继

Stage测试进入真实出生函数，不是整场波次或完整Stage规则验收；使用合成catalog，默认stage.dat部署仍USER_HOLD。Play检验出生八字段与Renderer生命周期，不是正式330资源或像素表现验收。先前OPoint/weapon-piece/fusion已验职责复用，不重做。

state9996当前Unity经普通OPoint后硬写HP/MP10，而正式direct clone采用500/500；必须用独立完整出生事务源见证核对stats绕过和后续字段，不能将其display改10来掩盖。该依赖闭合后回父NATIVE-DISPLAY-PROGRESSION，再接既有NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION。Q07正式DAT/角色图片未迁移；schema16/24/27/core12、raw47/3保持。
