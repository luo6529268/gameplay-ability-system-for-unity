# Definition内容合同与后继矩阵

| 域 | 正式reader/含义 | 当前实际证据 | 后继与边界 |
|---|---|---|---|
| BMP移动16值 | input_routing.cpp field_double(276)：strict finite strtod；walking/跑步/重物/跳跃/翻滚等caller通常value_or0 | 5280项中2008相同；156/158角色有520个float32精度差，172非角色缺省有2752差异 | Native binary64载体/精确解码，legacy float入口保留；Q06按具体reader接线，非角色缺省差异不自动叫作已观察战斗故障 |
| rate与sequence | input_routing.cpp1088/1254普通rate缺省1；heavy率优先heavy_*_frame声明再normal rate，movement_action按序列 | 344个缺省差异全部来自172非角色；1320个sequence字段比较一致，68个非空sequence声明 | 保存optional/有效性和正确caller fallback；现有顺序/序列行为不重写，native语法/计数边界另用fixture |
| BMP普通int | use_ai/property/effect/weapon_hp/weapon_drop_hurt等literal integer；weapon HP来自bmp，不能跨namespace | 加上12个已映射stats字段与rate共6270项，5926相同，只有344 rate缺省不同 | native profile按strict int/namespace处理；不是要求重做这些已匹配字段 |
| stats存在性/max_mp | battle_world.cpp1276 spawn时max_mp无效/缺失回退request.mp；2542正值cap；2191/2308判断stats.fields非空 | 158个正式定义max_mp均500；Raw stats与Unity AST顺序完全一致，但LF2CharacterData没有stats/对应max_mp载体 | 增native stats有效性/是否非空，不硬编码500，不把显式0与缺失混同；Q06初始化/恢复/资源 |
| stats其他live字段 | 75个literal reader记录及source哈希，含recmp/caughtact/attacking、bound/regen_*/bleed_hp/ohp/omp/defend；动态动作field由input_routing辅助caller | 正式当前stats除max_mp外只有y三条；当前输入动作映射值比较一致 | 未声明字段保存raw/optional能力以支持正式reader，但不得凭字段名补效果；consumer按原链逐包验证 |
| armor | CombatRecordDecoder28::armor：type有效整数优先、无效才回退ptype；ordered integer/range列表；last sound | 全18条规范化记录相同，包括完整列表与声音；非当前语料的边界语法未认证 | 复用模型及已实现行为，native metadata解析时精确处理边界；不能为审计重新实现armor runtime |
| weapon sound | battle_world.cpp bmp_string(700)从bmp.last取文本 | 990项相同 | 保持文本数据与现有Q10后继，不恢复未批准WAV整体迁移 |
| weapon_piece | dat_parser.cpp根/group/variant结构；battle_world片段producer在weapon_hp<0且type1/2/4/6时触发，内建碎片先于DAT碎片 | id124 w/9、id150 w/1、id151 w/l各3组4variant；Unity只有扁平raw properties且构建后无载体 | 准确结构AST+内容model，再Q06生成/RNG/slot尾部；不能将普通寿命结束当武器破碎，也不能丢内建先行部分 |
| BMP shadow/bound | render_snapshot.cpp1602 shadow gate、1864/1906贴地nameplate/owner命名gate | shadow30、bound98声明，构建后无对应载体 | 保存非例外战斗表现数据，Q09接线；FootSelf/头顶血条例外保持 |
| stats.y | render_snapshot.cpp1752，state9997 owner-relative平台取景偏移，非零优先否则owner frame y | c/dei/bir=-120、c/dei/ncb=-110、c/kon/ang=-60，当前无载体 | EXCLUDED_PLATFORM_VIEWING：不恢复已排除的平台取景行为，raw记录保持 |
| smallb | render_snapshot.cpp1416普通status HUD资源 | 154声明，构建后无对应字段 | EXCLUDED_ORDINARY_HUD_REVIEW_ONLY，保留引用审计，不改普通HUD |
| hidden/random | game_session.cpp4043/4066角色选择 | 各158声明 | EXCLUDED_SELECTION_FLOW，不能把它们计为当前战斗待修逻辑 |

本矩阵是源/内容审计，不是runtime parity。75个core literal reader全部在playable scripts/build.ps1的coreSources列表中；动态field参数及对象类型/模式分支仍需要按具体行为跟踪，不能用正则清单代替完整调用链证明。已有Q02 sprite/path/PNG范围引用原证据，不因BMP属性存储在不同对象而重复开启缺陷。
