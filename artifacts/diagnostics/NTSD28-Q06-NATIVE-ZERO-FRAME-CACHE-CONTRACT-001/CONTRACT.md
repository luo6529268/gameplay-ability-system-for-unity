# 原版零帧访问合同阶段

IN_PROGRESS / ACCESSOR_CONTRACT_FROZEN，所有reader迁移尚未完成。native dat_document.cpp:82-116：先取声明frame，否则0..998返回id对应且所有values/subblocks为空的共享const零帧，999未声明则null。DAT parser允许0..999声明；越界仍留AST但诊断error，ObjectDefinitionCatalog拒绝document.ok=false，不能以该非法AST accessor结果定义正式可加载范围。生产core/playable源码declared_frame只由frame调用；枚举document.frames属于另一种声明遍历，不能与点查混同。

Unity当前LF2FrameCache为857数组；GetFrameDataById缺失返回共享frameId0/wait1 EmptyFrame，HasFrame只声明判定。LF2FrameData本身可变且含lazy Hit cache；不得用每tick创建零帧来规避共享，或更改现有接口让未审核调用方改变行为。新增NativeMaxFrameIdExclusive1000，原Max857与Legacy Get/Has/GetFirst语义保持，单一存储扩展到1000装入高位声明；native accessor在有效Wrapper上下文中返回声明值或999个按id的零值模板（wait0/UsesLoganFrameNumbers=true）。以显式static ctor保证缓存第一次构造前完成模板预热。模板纯定义数据，无World/entity/UnityObject引用，生命周期为进程；沿用只读definition合同，不能在运行时写数据帧。测试/AI ForSelfCheck的显式帧修改不等于合法修改原版const零帧，后继逐调用方处理。

第一实施包仅Native accessor及HP/MP共享资格/字段读取，保留已正确公式与World phase。先原函数边界见证，零帧7/857/998会被接纳且chp/cmp为0，未声明999和越界9999拒绝；声明999应保留。两旧HP/MP invalid fixture用7是错误前置，应独立改为真正越界9999，保留原拒绝断言。声明/空缓存/clear/reload/999与旧接口、0热路径分配、资源两caller及真实恢复都要测试。

全调用方清单在unity-reader-inventory.json；其中测试/Editor明确标记，代码片段仅检索证据，不视为逐项native对应证明。后继必须逐组核对并迁移：frame tick/direct/next/snapshot，输入/动作，collision/hit/cpoint/held，OPoint/bootstrap/render/音频范围。还需区分document.frames枚举与runtime点查，以及所有绝对frame上界和快照FrameDataId恢复。此阶段不改parser、内容hash、字段shape或schema；布局未变并不免除恢复/回放验证。保留13/21/24/2/2。

禁止computer-use、非战斗/框架/Scene/资源改动。父display和post仍待，HP/MP已降级资格范围保留到新证据闭合。当前仅原版访问合同已读，不宣称所有reader已对齐。
