# Q05 Definition头部审计限定交付

状态 VERIFIED_AUDIT_ONLY / METADATA_GAPS_CONFIRMED。production/正式资源未改，总目标和Q05仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 核心结果

实际330 indexed候选构建后数据与当前原版definition输入对照，确认下一任务必须补native definition metadata，不能直接跳过内容门槛去发布身份/schema baseline。

- 16个移动字段共5280比较，2008相同。520个精度差异出现在156个type0角色定义（共158个角色），例如custom/1genma/genma.dat running_speedz：原版double3.3，Unity float升double为3.299999952316284。另2752个速度缺省与344个rate缺省差异全部来自172非角色定义；按正式reader数据合同记录，不能把3096项都声称为已复现的战斗故障。
- BMP普通int、已映射stats动作字段及rate共6270比较，5926相同，其余344是上述rate缺省。原版普通rate缺省1，Unity数据构造默认3；heavy/sequence回退必须按各自caller。
- stats raw字段顺序与Unity AST完全一致。158个定义声明max_mp:500，但LF2CharacterData未保留对应stats载体。原版还区分记录是否非空、字段是否能解析；初始化max_mp缺失/无效回退request.mp，不能与显式0混同，也不能只硬编码500。
- 3个indexed weapon_piece定义：w/9.dat id124、w/1.dat id150、w/l.dat id151，每个3组4variant。Unity AST为扁平属性，构建后没有weapon_piece内容载体；片段顺序/amount/variant边界必须补。
- BMP shadow30、bound98条是非例外战斗表现数据缺口，需保存供Q09。stats.y3条属于已排除平台取景；smallb154条普通HUD、hidden/random各158条选择流程，仅保留raw/引用审计，不能扩大到非战斗/已排除行为。
- 当前正式18条armor规范化记录、1320个sequence字段比较（68个非空声明）及990个weapon sound文本比较均一致。保留已有模型/行为与证据，不重复重写。边界语法不因当前素材相同就自动获得全域认证。

完整分域source入口、fallback、证据和后继见CONTRACT-MATRIX.md；每项数值首差、raw/presence、weapon_piece结构分别在同目录JSON。

## 实际证据与校正

1. 首次Unity capture jobdf09de7252504586b982fc68d8fc93ce 1/1通过，但记录的是BuildCharacterDataFromSource直接返回值，未包含catalog层type_sub=id覆盖。该工件留作unity-headers-before-catalog-overlay.json，不作为最终字段状态。调整诊断捕获到真实BuildCharacterFrameConfigsFromCatalog结果后，最终jobb29fbdd9d4de443394a2fbeee3972c79 1/1通过，CAPTURE.xml；330 id/type_sub全匹配。测试只证明捕获执行完整，不是parity PASS。
2. 复用已验证原版capture工具，实际新跑 `Temp/NTSD28ContentAudit/native/AuthorityContentCapture.exe --input-root <formal runtime/decoded_dat> --output Temp/NTSD28DefinitionAudit/native-fresh.jsonl`；405原版输出SHA5f5c6c34ce907bb6d7855b8a1e69d410b3cd577807f202332b8a0da30ed0146f与Q01旧raw捕获逐字节相同。comparator验证其build sources、31头文件、工具EXE，以及330当前DAT SHA。它复用同源raw证据，不复用旧对齐结论。
3. 数值不由Python自行推断：strict int32/有效性与float32诊断来自已验证AuthorityRawNumericWitness；BMP double.value_or0来自已验证TypedFrame witness的strict finite helper（与input_routing.field_double().value_or0一致）。两套工具针对本轮有效raw值均双跑稳定；输入/输出及EXE hash见audit-inputs、header-number/header-double工件。
4. `python Tools/NTSD28Q05DefinitionAudit/Compare-DefinitionHeaders.py`完整运行成功，comparison-summary.json报告差异而非假称PASS。首轮脚本有一处括号语法错误，修正后才实际比较；没有因此修改任何production或原版数值。armor frame区间仅将C#对象{first,last}规范为原版[first,last]数组后比较，不把存储形式差异当值差异。
5. literal reader inventory共75项，全部列入真正的ntsd28_playable/scripts/build.ps1 coreSources闭包。最初尝试不存在的CMake入口失败，已更正为实际build.ps1，不保留错误的构建参与性断言。统计不替代动态字段/调用链审查，具体关键入口已人工核对。
6. Unity编译后capture实际运行，最终error CS查询0，Scene isDirty=false/root14；Scene SHA仍a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f。既有UI精度差异来源pending继续保护。
7. 保护3059：3026相同/33既有差异/0缺失，相对前包无新增受保护基线差异。Change Ledger484/92通过。只新增两个诊断脚本，未运行新的完整SelfCheck/Play/整场trace，因为没有production行为修改；前轮通过证据不冒称本轮新运行。

## 下一步

下一唯一Task NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001，状态READY_FOR_EXACT_PRECHANGE_RECORD：原版metadata AST/有序与有效性合同、native double移动值/optional stats/weapon_piece载体、manager接线和完整投影。修改脚本前列准确路径、RED、验收与回滚，不拿本审计当目录级写入许可。

继续同一个Q05内容/runtime/schema窗口，之后才retired carrier/+2F8、semantic identity/capture guard/联合版本。Q06运动/资源/碎片、Q09非例外shadow/nameplate、Q10audio按原依赖回访。原六DAT九frame异常已在前包解决、330构建仍成功，不能重新开放已完成帧读取或用可构建代替完整metadata正确。所有用户例外、默认stage.dat暂缓、Unity/GAS与非战斗边界保持。
