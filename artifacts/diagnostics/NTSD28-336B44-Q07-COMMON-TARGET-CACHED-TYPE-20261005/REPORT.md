# Q07 共用缓存目标类型：限定修复结果

状态：RUNTIME_PENDING / SCOPED_CACHED_TARGET_DRIVER_PASS。实际首差已修复并通过本包限定出口；Q07、Q09、Q12 和总目标保持开放。

正式 native_ai.cpp 缓存有效性块不要求 type0，重新扫描才要求 type0。Unity ResolveFrameLogicTargetByHitFa 原在缓存块额外要求角色。当已追踪分身消失、槽被活体非角色对象复用时，正式保留缓存，Unity却改追另一侧角色。

## 实际首差与修复

当前 paired Core 诊断实际调用正式OPoint：鸣人2/frame193生成33/149到50，共用AI选50；受控399/action_latch399/counter3通过正式帧与生命周期释放50，缓存不清。实际玄间901/frame282生成902/40/type3/HP500复用50。明确准备位置/速度后执行一个完整SimulationTickDriver28.step。

Unity三项具名RED完成，两个差异案例失败，HP0控制未报失败。完整tick首差为target50/0、X500.85/499.15、Y-100/-98.8、Vx+0.85/-0.85。failed job不发布summary，未虚构RED通过数。

生产只删缓存有效性的一行IsCharacterFrameLogicTarget(target)谓词。扫描仍限角色；HP、倒地、相位、组、排名、源/视图读口、其它hitFa、2F8和此前脏改动保持。两个测试方法/三个案例插入217行，全部原行保留。

## 实际验证与原件

- native v2当前声明28 Core CPP闭包、C++17/O2无fastmath；compile/run exit0，76输入最终复核稳定。
- dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly：RED0错误301警告；GREEN0错误334警告。
- 原Unity Editor MCP两批各一次Refresh、Library程序集新鲜。RED job2513d9f526c941b583fb0b83709659f7完成3项；GREEN job5d3be5f85dc145d887e65a5b68f274f5为4/4 PASS、0失败0跳过，含既有共用局部SelfCheck，不是全量测试。
- native/Unity初态和完整tick各13个声明subject字段：RED20/26同，GREEN26/26同；统一view比例残差0。正常GREEN回调wrapper零残留断言通过；不声称RED抛错后尾部通过。
- 最终原Editor idle/nonPlay/noncompiling/无测试任务，Battle clean/root11、Console0error；四保护SHA保持。五权威、六DAT两端原SHA与九before备份稳定。独立审阅确认仅一行生产变化。
- 详细字段与边界：declared-field-comparison.json；实际job/editor/build原件在本目录。最终审计见GOVERNANCE-CHECK.md。

## 边界与有限后续

native是当前正式Core消费者，包含实际生成/释放/复用；399入口及tick前准备受控，不代表自然技能输入/自然399入口、根EXE、完整GameSession Host或GPU。Unity subject槽1/native51，备用角色身份及数量不同；只比声明subject字段，不称全World同态。旧Unity scenario seed/schema/Stage23只是载体。

首轮native未同步action_latch导致正式帧机重置counter，trace前失败；原件保留，v2只更正诊断held latch。首个GREEN刷新脚本默认GBK读中文失败发生在任何MCP调用前，后续UTF8才成功刷新，不算额外测试。

2F8候选有界核查见 [2F8-BOUNDED-REVIEW.md](2F8-BOUNDED-REVIEW.md)。普通投掷已同步组，排除通常冗余；OID124/Fa12走专用武器消费者，OPoint不自动写2F8/spawner。未取得共用消费者正式可达新首差，保持条件门，不开新实现/角色矩阵，不声称所有状态无差异。

本必要ONE完成，改为REUSE。当前228份同名Record/67未关，REUSE53/TRIGGER14，P0=DEP=ONE=0，不是67个必跑任务。自然/真人键/根EXE/GPU和未覆盖条件只在真实非例外首差或相关写者改动时回访。无DAT/Scene/图片/InputActions/非战斗流程修改，无删除/移动/Git丢弃。
