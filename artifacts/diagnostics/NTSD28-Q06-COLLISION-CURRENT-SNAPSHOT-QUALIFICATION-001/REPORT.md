# 普通碰撞准入与冻结消费检查点

IN_PROGRESS / QUALIFICATION_RUNTIME_PASS_FULL_DRIVER_DEPENDENCIES。候选/配对分类差异已清零，但父包完整driver仍有两个独立领域的失败，不能报告完整对齐。

## 原证据和修改

COLLISION-QUALIFICATION-SOURCE-WITNESS-001的480原端点+2完整driver，两遍一致SHA175c1a7ece4067ff782d272fe07ea418bc72742e2a55f3b691537065ea08fb45，3020断言通过。原source ordinary scan由snapshot itr/body决定，platform另读current；缺失current/previous状态以0填入冻结pair。273候选/eligible在current改1000之后继续eligible；207过滤不变。

实际两个生产文件：BruteForceSceneQuery ordinary/cached/selector/immediate/consumer取消额外current itr/body/null准入，保留snapshot几何；nullable current state取0，Kind0EffectAllowed的previous点查Native，移除以current/快照相互回退假造状态的路径。PairSnapshotFactory允许current缺失并填state0，仍要求两snapshot存在。未改平台current逻辑或空间索引结构。CandidateAccepts最后一个current非null门在第一复跑后补齐，没有仅修上层而遗留尾部拦截。

## 实际自动结果

- RED 465a71b72a0a4b96a3a7c3e782ea5411：四组480均FAIL，各1390差异；before raw一致，错误涵盖候选、配对及current缺失后消费。red目录保留。
- 第一修复联合c7244b8f770b4e9aaf23983492a468a2：12FAIL，480每route剩128条（64候选缺失×候选数/载体2条），均attacker current不可用，锁定CandidateAccepts；原336仍各24候选首差。另4完整driver失败为bdefend与RNG。after-first-fix保留。
- 最后gate后106613fc5dd74f52954001db604d68bd：**12项中8PASS/4FAIL**。原336×4=1344与新480×4=1920，合3264端点候选、raw47/3、抓取、冻结pair和classification零差异；之前96候选首差全部消除。after-final-gate记录原XML、新480、原336报告。
- 完整driver两例×四组仍FAIL：每例只有target slot2 raw bdefendAccumulator0/45及RNG计数差异，合8raw首差+8RNG首差。HP499、caught current10/998、snapshot0与其余绑定raw一致；Shadow CurrentTickPlanValid=true、ObservedWriterEffectCount=2、mask0。此处再次证明内部Shadow0不能代替原源对照。
- 旧回归a4d9cf5d247647c3850301e086f6ad92实际70项69PASS/1FAIL，仅role矩阵false/current-empty,true/snapshot-role仍旧期望0。独立COLLISION-ROLE-MATRIX-ORACLE-001按原改1，04fec11524a442048c477aee3eb2e21b四组合PASS。未重复其余69，legacy-70-first与role-matrix-4-pass均保留。

## 运行验证

完整SelfCheck请求06:26:21Z、**06:27:14Z PASS**。之后仅增加Play probe及修正测试期望，生产未再改，未重复全量SelfCheck。最终C# error CS条目0、Editor idle非Play。

Play-480-pass.json：真实NTSD_Battle Scene中120代表原端点×双actual factory×普通/role-aware=480，通过candidate/pair/classification/raw比较；Renderer2→2，主Scene checksum不变。属于合成DAT端点，不是正式新DAT/图片或完整driver侧差异的通过证明。

Shutdown-pass.json于06:38:10Z通过：恢复4→4，World/slots/logic/render borrowers0，两帧Stopped。Scene dirty=false/root14，SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。接口本轮新连接为6402，使用状态文件重新发现；无运行中job/build/exec。

## 剩余与下一唯一任务

1. UNARMORED-BDEFEND-WRITER-AUDIT-001：原combat_records缺失bdefend读0；battle_world6743命中分支直接覆写bdefend_accumulator=45。Unity raw绑定Runtime.Bdefend，但writer/plan仍写另一个HitStateCount字段。先查全部消费者和恢复时序，再准确Record；**不能把DAT加45或更改converter默认值**。
2. HIT-SPARK-TRANSACTION-AUDIT-001：原spark先Y/X各一次CRT；Unity DatHitResolver.SpawnSpark828/829用BattleRandInt，实测legacy3/native0/CRT0，对照应legacy例外1/native0/CRT2。额外两次不属C17例外。须核整个spark事务/两factory、snapshot中心、容量在RNG前、itr index/编码/fall，再实现，不盲改全局随机API。
3. 两项完成后回上述4个完整driver失败，再关闭当前qualification与父COLLISION-FRAME-UNITY；之后继续原reader umbrella→display/post→Q07，不重做已清零3264端点作为替代进展。

本轮没有DAT/图片、Scene、非战斗、Unity/GAS框架、Server、schema或关闭顺序改动，无computer-use、提交或推送。用户HUDBg30保留；Q07未部署、raw3MISSING、epoch恢复缺口、stage.dat USER_HOLD及用户例外保持。总目标ACTIVE。
