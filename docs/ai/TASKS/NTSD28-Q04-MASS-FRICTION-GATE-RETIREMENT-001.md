# Q04-A 剩余mass摩擦gate退休

状态 VERIFIED_MASS_GATE_ONLY / Q05_CARRIER_PENDING。依赖：Q03-EXIT-REPORT已交付。当前总目标已获启动授权，本Task可继续实施；修改任何脚本前先创建同ID Change Record。

权威：当前正式Logan playable physics_integrator.cpp对开始本步时已grounded实体执行摩擦，无mass>0 gate；33ms/积分/落地顺序保持。Unity CharacterMechanics.StepBattleLogic仍判断startedGrounded &&ctx.mass>0f，真实LF2Character _mass及ECS context均到此。

本包最小production范围：Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs，只移除mass条件；新增聚焦测试路径Assets/NTSD/Scripts/Test/Editor/NTSD28Q04MassFrictionGateEditorTests.cs及必要的已声明Play诊断。其他脚本如果必须改，先在Record准确追加理由，禁止顺手改NTSDSpec/全局配置或框架。

本阶段保留_mass/context/snapshot Mass载体，明确转为不再定义摩擦的过渡字段；Q05统一删除并character-shell1→2。此安排遵循当前用户Q04→Q05队列，取代旧owner审计“一包同时删gate和carrier”的实施拆分，不改变最终完整删除出口。

验收：先RED证明mass0/负值改变当前grounded X/Z速度，再GREEN确认与mass1相同并等于native；空中/刚落地/边界摩擦/速度正负/零保持预期。覆盖real character和ECS调用，compile/focused/相关SelfCheck，真实Play正常移动/落地与定向mass注入；未完成Play只记RUNTIME_PENDING。Q05字段删除测试另做，不以Q04通过宣布E或B6完成。

风险与副作用：只影响latent mass gate，无正式type0内容参数替换；无新runtime模块，无关闭阶段变化。回滚经用户批准仅反向本包最小脚本增量，保留其他未提交工作。禁止更改Scene/资源/输入资产/Gen/Plugins/非战斗行为。

当前Record已建且production单gate已改；RED6FAIL/3PASS→focused14/14，SelfCheck PASS。首轮Play因fixture Z200低于stage min237失败，摩擦4/-4正确且cleanup通过；probe现先warmup取真实stage中点，待重跑。不得从READY重新建立覆盖Record。

最终：Play重跑PASS，真实driver地面/落地三mass一致且cleanup通过；见同ID artifacts/diagnostics/REPORT.md。Q04-A退出，下一Q04-B。既有空World phase断言与独立landing一ULP首差已记录后继，不扩大本包修复范围或声称完整落地对齐。
