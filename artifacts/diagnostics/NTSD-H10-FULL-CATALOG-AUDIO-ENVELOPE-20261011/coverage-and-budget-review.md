# H10 按局音频依赖与预算边界（只读审阅）

独立只读 sidecar：01a126a6-b5e6-7540-ab88-b3e4e014767c。未读活跃 Q06 方法体、未运行 Unity 或修改文件。下面由主代理按本次实际清单校正口径，入口位置只证明可定位，不代表行为覆盖闭合。

当前完整回退：330 正式定义的真实 C# configs；CharacterAnimtorManager.CollectBattleSoundIds:4403 遍历 weapon_hit/drop/broken 和 frame sound/最多20 FrameSounds；4438 读取 PublishedLoganCatalog.ModeComboInput.KnockoutFeed 两条 KO。NTSDSoundPlayer:21 的19 builtin ID由public PrepareBattleCuesAsync:274/Core:320并入。本次实际1000 cue、978 formal identities；generic77条现存cue路由去重为72文件，旧72未失效。

| producer类别 | 完整回退能证明什么 | 候选仍缺什么 |
|---|---|---|
| stage/reserve | 本次实际绑定真实发布ModeComboInput，data/m_ok.wav与data/m_join.wav已进collector | stage/reserve OID选择、late birth/模式切换到声音请求的完整metadata合同未闭合 |
| OPoint | 已加载定义的frame/weapon字段在全量内；LF2ObjectPointModule:60为入口定位 | 所有child/external OID与下一代definition/cue闭合尚未证明；不展开Q06补写语义 |
| transform | 已加载目标定义只要在330中，其字段cue已收集 | 变身/转换动态目标的所有来源及音频依赖未闭合；Transform位置本身不等于音频producer合同 |
| random weapon | 全定义weapon_hit/drop/broken被收集；SimulationPassPipeline:115/126、World:3786/3797只作入口 | 模式随机表与所有可选OID/weapon-piece依赖未闭合，不读方法体推断 |
| respawn/late/external OID | 全正式定义是当前保守回退 | SimulationPassPipeline:62、World:2475/2890只作入口；外部OID、重生/late注入的封闭世界限制未知 |
| weapon-piece | LF2CharacterData:84公开声音字段，manager:1493解析，LF2Entity:4394调用位置 | piece child/parent身份及传播合同、外部piece OID未知，不能凭目录或类名裁剪 |
| builtin hit/landing/KO | 19内建ID以及真实mode两KO已进入本次manifest；原声音规则证据复用 | 全部变量分支与动态字符串请求到固定集合的静态证明仍缺 |
| string cue | LF2Entity.QueueBattleSound:5009、World.QueueSound:2059为已知入口，1000实际metadata cue均解析 | 变量cue/baseSound/soundPath、外部生产者完整枚举未知；subset出现不证明全部请求覆盖 |

玩家+全non0+static visual closure仍是未准入候选。旧示例144100664B只是候选payload估算；不当可实现收益。本轮没有裁剪，也没有承诺仅凭全量账本即可关闭producer覆盖。最小后继合同：明确当前允许的根集合（玩家/模式/stage/reserve/外部OID）、对动态spawn/transform/random weapon/respawn/weapon-piece声明目标定义集合及字符串cue来源，并对未声明来源保持完整回退。

预算检索：H10卡、原RESIDENT-CLOSURE/当前CATALOG-ENVELOPE-REVIEW以及DEPENDENCIES未发现当前阶段已批准steady/transition音频数字预算。历史209.99MiB、226264216B等均为payload估算；固定5文件848237B是局部对象驻留收益；98304B是converter临时数组约束；64是voices容量。都不是整局/设备内存预算。执行安全余量只用于本次运行前检查，不作产品超预算拒绝策略。

下一审阅应将已应用boundedPCM、formal session ownership、这次完整音频账本、仍未知的producer闭包/预算与后期设备认证分开；不自动恢复其他优化点或要求人类重复允许继续H10。
