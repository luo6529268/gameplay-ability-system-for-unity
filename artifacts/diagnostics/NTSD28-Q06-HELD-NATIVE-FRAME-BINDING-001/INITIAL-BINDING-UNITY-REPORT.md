# 持有对象原生帧：初次绑定验收

状态：`IN_PROGRESS / INITIAL_BINDING_VERIFIED_RELEASE_READERS_PENDING`。正常持有的初次绑定已达到本轮出口，整个持有读帧任务仍有后续分支，不是完整 Q06。

## 实际改动

- `BattleHeldObjectWriter.RunStep12` 和 `LF2WeaponHeldStateResolver.Act` 的初次 WeaponAct 绑定及准入，共四处改为现有原生访问器。
- 独立关系查询修复：`LF2CharacterWeaponLinkResolver.GetHeldEntity` 在非持有者（负或零关系）时只清缓存，不改正式关系字段。正关系失效处理保持。
- 两个旧 Editor 测试补齐正式实体的 Trans／C25 前提，并纠正旧的缺失帧、延迟门和未标记生命周期预期；SelfCheck 的槽复用断言按原生清理值 0 修订。各自有独立 Record，原失败保留。

## 验证证据

源140：SHA `8ddb447a80caa5edb8a2c55f1cb2ceda35b3fed3503f57d614fd5589655b9610`。105 有效跟随、21 不可用返回、14 终止，原生字段与调用顺序见 source/validation.json。

Unity 首次完整 RED：两 profile 各 before0／立即728／following859；四处绑定修正后立即0／following90；独立查询修复后两 profile 的初值、立即结果和完整 following tick 全部零差异。分别归档在 logic-first-complete、after-initial-binding-fix、after-query-fix。

- 查询及槽复用专项：5/5 PASS。
- 旧相关 Editor 回归：91/91 PASS（原89项加2项延迟控制）。
- 同 World 恢复回放：280 个案例，560 个重放完整 tick，PASS；旧 RNG cursor 被拒绝。不证明跨 World epoch 恢复。
- 完整 SelfCheck：20:01:02Z PASS。
- 真实 Play：140×两 profile×两 factory＝560 案例 PASS，场景 checksum 保持，Renderer 借用数 2→2。
- 20:02:44Z 有序关闭：恢复4→4，World、slot、逻辑池与 Renderer 池全部归零，连续两帧 Stopped。场景文件 SHA 保持 BCD1047B…0E9FB6。

## 历史失败与范围

首个 Editor 夹具因 JSON 对象／null 比较器异常和 EditMode 未初始化的 Mono 对象池失败，未作为完整矩阵结果。比较器已修正；Renderer 检查在真实 Play 已完成，没有伪造 Awake 或修改池实现。

旧测试失败分批保留：空 Generic 没有 Trans／帧推进、777和857被误当缺失、未标 pending 的旧1100/1200反射行为、C25暂停计时未解除，以及槽复用0/-1旧断言。所有纠正都保持负例、安全条件和源端顺序，不改生产迎合旧断言。

本轮源矩阵是 kind1、无 DVX、cover0 的正常持有；不声明投掷、随机放下、补给所有帧写入都已对齐。下一步按同 Task 扩展这些实际后继分支，保留本轮证据。正式330内容尚未部署，也没有完整物理按键或视听对齐结论。
