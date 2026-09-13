# Q03：新版DAT与联合字段合同冻结

状态 DELIVERED_CONTRACT_ONLY / Q03_EXIT_AUDITED / IMPLEMENTATION_PENDING。总目标NTSD28-UNITY-BATTLE-REALIGNMENT-001，BATCH-02；Q01已交付，Q02加载基础出口完成后按队列进入。本Task覆盖Q03-A和Q03-B，不把六个Converter错误消失当作完整Q03出口。

## 唯一恢复入口和输入证据

先读docs/ai/CURRENT-AUTHORITY.md和对齐总表0.11～0.14；读取Q01报告及冻结的native/Unity capture、manifest和normalized projection。不恢复旧NTSD2.4/C# campaign，不重做已关闭B1/B2或已完成的行为退休。

正式逻辑权威仍为J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan中的正式EXE（精确SHA以CURRENT-AUTHORITY为准）及README_SOURCE证明进入playable构建的live C++。DAT/角色图按D-023同版本权威；旧Unity138-DAT只作迁移前比较，不反向裁决新值。

Q01实际六文件九frame拒绝如下，先复核原文与两端完整tokenizer/decoder/resolver消费链：

| DAT | frame数 | 当前首个拒绝 |
|---|---:|---|
| c/ank/ank.dat | 1 | WPoint effect |
| c/ank/ssnk.dat | 1 | WPoint解析出字段名7，先核对tokenizer |
| c/hir/hir.dat | 3 | WPoint dircontrol |
| c/min/min.dat | 1 | CPoint drain |
| c/min/sag.dat | 1 | CPoint drain |
| c/nar/nar.dat | 2 | WPoint dircontrol |

禁止盲加DTO字段、吞frame、填零、用raw字段存在推断规则消费。需区分原生忽略、默认、alias、重复覆盖/优先级、presence语义与实际战斗reader。Unity定位入口为DatParser/Runtime/Utils/Lf2DatConverter.cs、ParserV2，以及Simulation/DataContracts/CatchPoint、ObjectPoint、WeaponPoint及实际actor/field消费者。

## Q03-A：内容字段合同

- CPoint27与Unity当前19字段：逐项定义类型（含float32）、单位、默认/presence、alias优先级、边界/哨兵、转换与复制，不仅比较字段数量。
- OPoint24与Unity当前8字段：完整producer与materializer消费契约，生成时序/继承/link等副作用按live路径核验，不提前实现全部B7。
- WPoint六文件拒绝、throwvz哨兵、ITR zwidth默认、BDY最终几何与ITR caughtact/catchingact标量/数组表示差异：追到真正消费点再归类。原生generic parser对INKHUD/INKSTG/OPI的报错不能冒充正式游戏故障。
- 每项输出authority路径/符号及build参与性、Unity路径/符号、原始输入、预期与实际、first difference、所需数据/生命周期依赖、验证方法与精确后继Change。

## Q03-B：一次联合版本窗口

冻结+2F8、mass/reserved及上述字段在runtime、shell/ECS、capture/restore、copy/reset、hash/checksum和trace comparator的完整矩阵。每个字段写明语义、默认、首次producer、全部reader、对象复用reset、版本链与旧reader退休出口。不得仅凭类名推断owner。

复核D-022的一次协调窗口：Q03冻结合同，Q04按证据退休剩余旧行为，Q05一次协调schema迁移，Q06逐包生产接线。当前schema12/20/23保持，不在本审计提前升版或向旧snapshot填默认假装支持。旧快照拒绝/同seed输入回放和双端同版本capture的证明要求回链R13/R15。

## 范围与出口

这是只读合同审计，脚本/资源修改前另建精确Task/Change Record；不得直接改DAT/PNG/WAV/Scene/Prefab、默认stage.dat、Gen/Plugins或框架。Q07正式资源切换仍等待依赖，音频与既有表现例外不自动扩展。

出口是完整字段/reader/版本迁移矩阵、六文件九frame根因分类、可执行验证方案及准确后继顺序，回链对齐总表和R13/R15。不能以解析器能读、六错误变零或几个单元测试通过宣布B6/B7/整个战斗已对齐。

当前进展：正式EXE hash已复核；现有source-linked诊断exe及9source/31header共41身份检查无漂移。六原文件+六最小fixture实际双端capture12/12语法通过，Unity8文件Converter拒绝（含6正式+2fixture）。六正式文件九frame精确分类已生成，hir第三个拒绝为frame414 CPoint drain而非WPoint dircontrol。decoder27/9/24对Unity19/9/8矩阵已捕获；当前未完成完整consumer与Q03-B联合版本矩阵，不能关闭Q03。复用已存在CPoint27/mass owner审计并按D-023/Goal17~20当前状态纠正旧hold及consumer结论，不从头重做已退休功能。

恢复报告：`artifacts/diagnostics/NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001/Q03-PROGRESS-REPORT.md`；同目录 `JOINT-FIELD-MATRIX.md` 是进行中的+2F8/mass/reserved及snapshot/ECS/版本路径清单。`audit-source-identities.json`冻结本次读取文件；`joint-field-reference-inventory.txt`包含test/diagnostic和同名引用，尚未逐条分类，不能当作生产reader计数。

下一个最窄动作：确认BDY/ITR的live候选深度边界、负值/缺省和strength override，完成同输入定向证据；再闭合两条OPoint materializer、CPoint canonical float和完整联合字段矩阵。此时不回头重做Q02，也不提前执行Q04/Q05。

几何见证追加：`NTSD28-Q03-COLLISION-GEOMETRY-WITNESS-001 / VERIFIED_CAPTURE_ONLY` 已完成普通type0的14同DAT用例，Unity三模式共42项15同/27异。深度端点、BDY深度、ITR Z、负宽度与geometry presence已从静态候选升级为实测差异，见同ID artifacts/diagnostics/REPORT.md。下一继续未覆盖的held strength/deep cache路径与字段合同，普通14用例不重复执行。BDY zwidth/presence明确加入联合窗口待冻结内容，不额外开第二个破坏兼容窗口。

OPoint/held消费链追加：同目录 `OPOINT-AND-HELD-DEPTH-CONTRACT.md` 已记录24字段及两条factory/initializer/PostInit/copy/reset、native随机/循环终止/零帧默认和held strength选择。新增weapon_strength index+19项必须进入Q05内容窗口；兼容ProcessAttackInternal未发现生产caller，不接到该旧路径。当前下一最窄动作是CPoint canonical float、reserved/AI完整reader、content decode-version/Lockstep identity绑定以及capture边界是否排空OPoint；不要重复两条已读factory清单。held/dense运行时见证留准确Q06验收，不作为读取矩阵完成的替代。

版本/identity/capture追加：`VERSION-IDENTITY-AND-CAPTURE-CONTRACT.md`已记录12→13/20→21/23→24/character1→2、其他payload保持、OPoint双owner空队列guard（不Flush/丢弃）、原始/语义身份的确定字节编码、CPoint27顺序及3个float的bit规范。339处reserved/alias引用已分类（含271测试），task.holderCopySlot残留纳入Q05；两个object-AI Spawner reader映射+2F8，character AI OwnerSlot不混用。下一最窄动作为native数值语法/float32 bit witness（signed zero、subnormal、溢出、重复、hex/指数等），随后按本Task的Q03-A/B出口逐项核对并准确准备Q04/Q05。此时仍IN_PROGRESS，不把规范向量当生产验证。

最终出口见同ID artifacts/diagnostics/Q03-EXIT-REPORT.md。numeric37双跑和333值对照完成；出口复核补入Oscillate剩余reader及base-shell1→2，更正此前仅character-shell升版的暂定结论。下一唯一入口docs/ai/TASKS/NTSD28-Q04-MASS-FRICTION-GATE-RETIREMENT-001.md，先建Change Record再实施。R13/R15合同子条件PARTIAL_RETURN，BATCH-02与总目标未完成。
