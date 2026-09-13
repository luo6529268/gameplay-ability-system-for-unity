<!-- CHANGE-RECORD
id: NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001
status: VERIFIED
change-kind: LEGACY_EFFECT_READER_RETIREMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04OscillateConsumerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q04OscillatePlayProbeEditor.cs
authority: Active user realignment goal; Q03 exit and D-022; current formal NTSD2.8-Logan playable render_snapshot.cpp native_body_shake_x uses render_phase_008, not legacy Oscillate amplitude; Goal17 producer retirement already verified.
evidence: VERIFIED_LEGACY_OSCILLATE_READER_ONLY / RED_8_FAIL_6_PASS / RELATED_28_PASS / FULL_SELFCHECK_PASS / CENTRALONLY_PLAY_STATE_AND_PRODUCTION_SNAPSHOT_PASS / RESTORE_PASS / CS0_SCENE_CLEAN / Q05_BASE_SHELL_RESERVED
-->

# NTSD28-Q04-OSCILLATE-CONSUMER-RETIREMENT-001

## 事前合同

准确需求与出口见同ID Task。production仅LF2LivingObject.ProcessEffects：删除Oscillate交替方向/SetXY分支及timeout旧字段清零/SetXY分支；原else-if Blink改为独立if，旧字段不再抑制Blink。保持TimeIn gate、TimeOut递减、Stuck/Super/Blink清理、延迟Dvx/Dvy写入和其他TU行为。更新邻近summary不再声称处理旧震荡。

保留LF2EffectState.Oscillate/OscillateDirection及base-shell存储/恢复到Q05；不重做EffectCreate producer退休，不实现native B9 body shake，不改SpriteRenderer/材质/相机/资源/Scene/非战斗逻辑。现有URP保持。

测试使用真实LF2Sprite（无renderer也记录LocalOffsetPixels/EntityVisible）和派生character暴露protected ProcessEffects，先RED证明旧幅度仍影响offset/direction/Blink。Play probe暂停现有driver、选实际已注册且有SpriteRenderer的角色，保存Effect属性/速度/显示状态，调用同一production ProcessEffects验证offset/Blink/timeout，finally完整恢复。无新production manager/queue/worker，关闭十一阶段不变；probe没有异步尾部，不创建服务。

允许文件为metadata三脚本及meta、诊断工件和Task/Record/状态文档。原内容、框架、schema均保持。回滚经用户批准仅撤销本包最小reader改动，保留既有工作。风险是旧恢复字段的显示副作用被退休；Q05明确拒绝旧snapshot，不承诺旧行为重放等值。

## 实际验证

测试/production尚未写；先运行RED，再最小修改，compile/focused/旧producer+base-shell回归、完整SelfCheck和真实Play均有证据后才报告本reader已退休。

RED实际job6f244430c8d746b1bab53777408e4689：14项中8失败/6通过；非零旧幅度覆盖LocalOffsetPixels、交替方向、阻止Blink及timeout清零offset均实测；无旧幅度和TimeIn早退通过。测试初始化真实PS-runtime绑定，没有用未绑定PS制造失败。下一移除两处旧reader，carrier保留。

已写production两处reader移除及Blink独立if、邻近summary/合同注释；LF2EffectState/Reset/base-shell/producer未改。Play probe已写，选择现有World中有实际SpriteRenderer的角色，保存并finally恢复所有Effect可写属性、VX/VY、Sprite状态、Transform/renderer.enabled和driver pause；只调用production ProcessEffects，无新GameObject/服务。正在等待编译及GREEN，尚未执行Play或SelfCheck。

GREEN job6ce4011338aa463c8f0df70eb974a981实际28/28 PASS（本包14+已退休producer10+旧base-shell4）；证明carrier布局仍保留、旧producer不恢复、其他Effect路径与早退保留。完整SelfCheck已按新时间戳request提交，Play待执行。

SelfCheck新鲜PASS。Play前置两次未找到角色Sprite绑定的SpriteRenderer，首次结果保存play-startup-precondition.json；不能因此假造renderer。复核当前生产BattlePresentationCoordinator捕获路径明确从entity.Sprite读LocalOffsetPixels/EntityVisible并写表现快照，支持无renderer适配。仅调整probe：选择已注册实际Sprite，记录实际backend与renderer是否存在；用独立diagnostic coordinator调用同一生产快照捕获核验偏移/可见性，有renderer时追加Transform断言；finally Reset该本地coordinator并恢复角色所有状态，不替换world backend或注册新production服务。此验证不声称GPU画面完全对齐。

最终限定出口：Play slot0/实际backend CentralOnly PASS，未绑定SpriteRenderer；offset/Blink/timeout/延迟速度及共享production快照都通过，状态恢复/对象数保持通过。退出Play后CS0、Scene dirtyfalse/root14、Ledger474/52PASS；production最终3新增12删除行，只有两个reader分支和Blink条件调整。完整证据见同ID artifacts/diagnostics/REPORT.md。Q04交付，下一Q05准确inventory/Record；不声明B9完整表现或carrier删除完成。
