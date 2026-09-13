# Q04-B 旧Oscillate consumer退休

状态：VERIFIED_LEGACY_OSCILLATE_READER_ONLY / Q05_CARRIER_PENDING。2026-09-13。

生产仅修改LF2LivingObject.ProcessEffects：删除旧幅度交替方向/SetXY分支，以及timeout清零旧幅度/SetXY分支；Blink由旧else-if改为独立if。保留TimeIn早退、TimeOut递减、Stuck/Super/Blink结束清理与延迟Dvx/Dvy写入。旧producer没有重做，Effect两字段/Reset/base-shell仍保留到Q05。

## 验证结果

- 正式EXE及render_snapshot/source身份见prechange-identities.json。当前权威body shake由render_phase_008和交替phase定义，不来自旧幅度字段；本包不实现或认证B9的native body shake。
- RED job6f244430c8d746b1bab53777408e4689：14项8失败/6通过；非零旧幅度覆盖Sprite偏移、阻止Blink、timeout清零偏移均实际复现。测试使用真实LF2Sprite记录状态，角色PS/runtime已正确绑定。
- GREEN job6ce4011338aa463c8f0df70eb974a981：28/28通过（本包14+旧producer10+base-shell4），见green-related.json。原carrier/schema保持，其他效果路径未回退。
- 完整SelfCheck：01:46:16 UTC新request，结果mtime晚于request且PASS，证据selfcheck-request-time.txt/selfcheck-result.txt。
- Play最终PASS：当前场景backend为CentralOnly、现有slot0角色有LF2Sprite但未绑定SpriteRenderer。直接调用同一production ProcessEffects，并由独立诊断coordinator调用共享生产快照捕获验证LocalOffsetPixels/EntityVisible。偏移、Blink、timeout、延迟速度与productionSnapshotPassed均true，finally恢复所有Effect属性、速度、显示状态及pause，cleanupPassed=true。未替换World backend、未添加组件、未注册新服务；本地coordinator.Reset释放自己的publication资源。结果play-final.json。
- Play最初把“必须有SpriteRenderer”作为前置，连续未满足；首份结果play-startup-precondition.json保留。复核代码发现生产快照直接消费LF2Sprite的managed状态，故修正验证路径，而非给场景添加renderer让测试通过。**本包没有取得SpriteRenderer Transform分支或GPU像素一致证书**；实际CentralOnly路径的状态/快照证据足以关闭本旧reader职责，完整画面仍归Q09/Q12。
- 最终CS错误查询0；退出Play后Scene isDirty=false、root14。Ledger实际PASS：474 records /52 governed code files。3059原保护文件中3045不变、14声明生产变更、0缺失；本包新增production变更只有LF2LivingObject，其他13项为前批。

## 审阅与实际范围

production diff为3新增/12删除行（包括summary与合同注释）；未改LF2Sprite、Renderer、材质、相机、DAT/图片、Scene/Prefab、输入、GAS框架或非战斗功能。新增两个Editor测试/Play诊断脚本和meta；没有新production manager/queue/worker，无十一阶段关闭变更，无commit/push/删除用户文件。

实际使用现有Editor refresh/run_tests/get_test_job、已有SelfCheck请求机制、manage_editor Play/Stop和该probe菜单；结果文件而非“菜单已调用”用于判定成功。重新编译只涉及probe纠正，production自28/28和SelfCheck后没有再改。

Q04-A/B均完成限定行为退休，Q04可交付；R13仍只完成行为子条件，mass/reserved/Oscillate载体和版本在Q05统一处理。Q04-A记录的旧phase断言和landing一ULP差异仍是明确后继，不因本包通过被关闭。

下一 `docs/ai/TASKS/NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001.md`：先建立准确code-path/Change Record，再按Q03冻结合同进行同一协调迁移。Q05未开始写代码，正式资源未迁移；总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，BATCH-02未完成。
