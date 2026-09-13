# Q05 Native definition metadata接线

状态 IN_PROGRESS / FIELDSET_READY / AST_SOURCE_NEXT。前置审计NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001已取得实际330构建后数据与原版对照；先读取其REPORT、CONTRACT-MATRIX与JSON首差，不重复帧AST/typed、PNG/路径或已匹配的护甲内容审计。

必须解决的内容条件：

1. Native BMP移动参数保存为binary64，复用已验证LoganNumericDecoder；158角色中156个定义有520处float32精度损失。其余172非角色定义有2752个移动缺省值与344个rate缺省差异，作为内容合同记录，不能都叫作已观察到的战斗故障。Native普通rate缺省1，速度缺省0；heavy rate/sequence按实际reader回退，不把一个统一default当所有入口的规则。
2. 保存stats字段有效性及记录是否含任何字段，特别是158个定义的max_mp:500。初始化max_mp缺失/无效时使用request.mp，显式0不能与缺失混同；当前LF2CharacterData没有对应stats载体。其他已确认live stats键即使正式语料未声明也保留raw/optional读取能力，实际runtime接入仍Q06。不要仅增加一个固定500常量。
3. 为w/9.dat(id124)、w/1.dat(id150)、w/l.dat(id151)保存weapon_piece根字段、3个group/4个variant以及有序字段/amount归属。现AST扁平化且构建后无载体，不能凭丢失piece_end边界的旧properties猜结构。先读dat_parser.cpp实际weapon_piece分支与battle_world.cpp尾部consumer，再收敛准确parser/model/decoder路径。
4. BMP shadow(30)/bound(98)为非例外战斗表现数据，需保存供Q09接线；FootSelf、头顶血条等例外保持。stats.y(3)属于已排除的平台取景；smallb(154)属于普通HUD；hidden/random(各158)属于选择流程，均只保留原文/引用审计，不据此次内容决定恢复被排除的UI或取景行为。

实现须保留现有Unity/GAS与legacy入口/Inspector用途。可在LF2CharacterData增加明确native metadata/profile与精确值；不为精度修正批量重写所有旧float调用者。Native消费者在Q06按已确认读取语义接新值，旧源继续既有路径。模型选择、初始化/复制/身份字段、资源引用和数据入口必须在准确Change Record写清；本Task不是目录级修改许可。

先解决完整metadata AST与有序/有效性合同，再接精确值和manager。继续扩展既有ParserV2 native入口，不另造整套无关parser，也不把metadata伪装成frame喂旧转换器。stats/BMP/armor/weapon_piece的大小写、重复、结束边界、整数/列表语法需有native见证；现18条armor、1320项sequence投影和990项weapon-sound文本比较一致，保留既有模型/行为，只有新证据要求时才改相应细节。

每次脚本前单独准确Record与RED。验收以同source/input hash的完整metadata投影为准：双精度raw bits、optional/presence、stats、piece结构、330 actual candidate及非法candidate拒绝、旧138入口、相关focused/编译/SelfCheck。不能只让几个用例或全部DAT不抛异常，就宣称definition对齐。

本包必须回链Q05 identity/hash/数据契约清单，再继续原retired carrier、独立+2F8、OPoint双队列capture guard与entity13/aggregate21/checksum24/两shell2同窗口。Q06运动/资源/pieces、Q09非例外shadow/nameplate、Q10audio消费另验；当前source已加载不等于其runtime已接通。默认stage.dat、音频/其他图片迁移边界、Scene旧精度差异、33ms/3ms和十一阶段保持。不得提前部署Q07资源或扩大非战斗范围。

当前先执行 NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001：准确四脚本，数据模型/optional有效性，production manager未接；随后native metadata AST与结构接线继续，不跳过原顺序。

字段合同 NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001 已限定交付，486/4045/SelfCheck证据在REPORT。下一唯一 NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001，先准确Record后metadata AST/manager/piece；实际header差异仍待，不重做字段容器/数值。

最新推进：NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001已限定交付，实际NativeMetadata已绑定；上文“未接manager”现仅历史。下一唯一NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001，armor/piece来源完成后再父metadata最终投影与identity窗口；不得重做BMP/stats或提前关闭Q05。

当前出口更正：NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001与前BMP/stats、typed frame、strength及模型子包完成本父Q05步骤1的来源门槛，SOURCE_MODEL_FOCUSED_PASS（非runtime全对齐）。下一唯一NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，依父步骤2→3→4→5，不直接跳identity或部署。CPoint/OPoint/Geometry/ITR旧SOURCE_INTEGRATION_PENDING已有typed frame/manager证据满足来源子条件；identity/schema/consumer/Play仍待。禁止computer-use；任务外Foot18删除与新目录、Scene旧精度差异保护。
