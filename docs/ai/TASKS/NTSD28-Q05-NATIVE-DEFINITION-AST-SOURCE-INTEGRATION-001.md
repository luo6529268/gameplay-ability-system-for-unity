# Q05 Native definition AST与数据来源接线

状态 IN_PROGRESS / BMP_STATS_SOURCE_FIRST。父Task NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001继续。前置LoganDefinitionMetadata/FieldSet及optional数值已经存在，读取同ID字段合同REPORT；不重复建立字段容器或数值算法。

下一目标是完整、可对照的BMP/stats/sequence/armor/weapon_piece数据来源和实际manager接线。当前FieldSet尚未进入production loader，520角色精度、3096非角色缺省、stats/piece/表现载体差异仍未关闭。修改前按真实调用者列准确code-path、Task/Change、RED与回滚。

在既有ParserV2 native入口上保留准确metadata AST：literal key、namespace、重复last-win、ordered raw/列表、字段有效性及global stats规则；保留现有frame/strength已验证行为。BMP文件/sprite范围的Q02证据复用，只有新的语法差异证明需要时才改相关入口；不重做PNG/资源加载。旧Parse/Inspector/非战斗工具用途保持。

weapon_piece必须有明确根/group/variant及piece_end边界、group amount归属，不能从已经丢边界的旧扁平properties推测。模型与parser可以按闭合职责拆子Record，但同属本Task出口，不再额外开不兼容发布窗口。18条现有armor内容/序列/声音匹配结论保持；边界与literal字段解码仍以native为准。

把准确原始字段复制到不可变LoganDefinitionFieldSet/Bmp/Stats，之后在明确source.IsLoganRuntime入口关联到LF2CharacterData。字段集合已能提供预解码int/double有效性、原始非空记录和caller fallback，不再用零值代替absent/invalid。Native runtime数值从精确载体取得；legacy float字段的保留用途、projection/identity及Q06迁移读取者必须写清，不能只加一个无人读取的新字段就宣称角色运动已对齐。

验收包含原版metadata结构/typed值/optional/顺序及330真实candidate、非法candidate整体拒绝、旧138入口、原native frame/strength/numeric回归、编译/SelfCheck。所有既定native字段（含shadow/bound等非例外表现数据）及新载体进入Q05 identity清单。Q06运动/资源/piece与Q09/Q10消费者和Play仍按原依赖回访。

stats.y的平台取景、smallb等普通HUD、hidden/random选择流程维持排除，只保留raw/引用审计；不恢复相关非战斗功能。Scene旧精度差异、stage.dat暂停、33ms/3ms、十一阶段、Unity/GAS框架、外部包/Gen/Plugins边界保持。Q07资源部署和旧文件处置仍待原前置与具体清单。

当前准确子Change NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001先BMP/stats AST和metadata来源；weapon_piece/armor等未闭合，不能将本子包出口当父Task完成。

最新推进：NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001已限定交付，实际NativeMetadata已绑定；上文“未接manager”现仅历史。下一唯一NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001，armor/piece来源完成后再父metadata最终投影与identity窗口；不得重做BMP/stats或提前关闭Q05。

当前出口更正：NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001与前BMP/stats、typed frame、strength及模型子包完成本父Q05步骤1的来源门槛，SOURCE_MODEL_FOCUSED_PASS（非runtime全对齐）。下一唯一NTSD28-Q05-RETIRED-CARRIER-AND-2F8-MIGRATION-001，依父步骤2→3→4→5，不直接跳identity或部署。CPoint/OPoint/Geometry/ITR旧SOURCE_INTEGRATION_PENDING已有typed frame/manager证据满足来源子条件；identity/schema/consumer/Play仍待。禁止computer-use；任务外Foot18删除与新目录、Scene旧精度差异保护。
