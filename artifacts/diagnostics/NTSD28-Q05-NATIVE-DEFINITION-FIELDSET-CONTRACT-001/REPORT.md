# Q05 Native definition字段集合与optional有效性限定出口

状态 FOCUSED_TEST_PASS / VERIFIED_FIELDSET_MODEL_ONLY / METADATA_AST_MANAGER_CONSUMERS_PENDING。Q05及总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

实现LoganDefinitionMetadata的Bmp/Stats不可变字段集合，复制原始ordered KeyValuePair；Ordinal key匹配，重复last-win，同时缓存text/int32/float64值及有效性。Count按全部声明而非有效字段计算，HasStatsRecord因此区分无字段与只有未知/无效字段。Get*OrDefault由调用者明确提供fallback，不硬编码500/1或混同absent/invalid/显式零；读取期只查预解码缓存，不解析/分配。对外只读集合，源list清空或外部尝试修改不会改变数据。

共享LoganNumericDecoder新增TryParseFiniteFloat64：无效或非finite/overflow返回false及+0，有限数值/underflow返回true并保留signed zero。沿用已有单次BigInteger舍入与词法，32/64 OrZero值保持；没有第二套近似double.Parse算法。新metadata模型尚未接入LF2CharacterData或manager，不能将这一步报告为修复上一轮520精度/3096默认值等实际header差异。

## 证据

- 原版optional helper精确文本来自input_routing.cpp field_double，另直接链接当前FieldBag.integer；源/头/EXE/hash与4045个输入已冻结，双跑byte-stable。输入联合此前binary64及本轮正式BMP/stats原始值，valid与bits都比较，不用Python猜有效性。该模型见证不是正式EXE的运行trace。
- RED job6a5043f8476c4e6facebe349f27f6e71：4/4 FAIL，缺模型/optional入口，RED.xml保留。
- 最终jobd5eeae98451a42b498105ba73ff04f38：486/486 PASS（新4+既有43 numeric+439 typed frame）。新4含4045项valid/bits及每项缓存int/double、重复失效覆盖、case敏感、requestMP fallback、负零、原始顺序、输入隔离/外部不可改、空stats与未知声明stats区分。原numeric14742展开回归及55348帧typed对照保持。
- dotnet build Tools/NTSD28NumericDecoderTests/NTSD28NumericDecoderTests.csproj --no-restore --verbosity quiet：0warning/0error。
- 新完整SelfCheck请求2026-09-13T07:28:46.464871+00:00，结果2026-09-13T07:29:39.336596+00:00为PASS且mtime晚于请求；旧结果另标NOT-CURRENT。
- Unity实际重载/测试完成，最终error CS查询0，Scene isDirty=false/root14。Scene SHA仍a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f，旧UI精度差异来源pending继续保护。
- 输入hash0漂移。保护3059：3026相同/33既有差异/0缺失，相对前包无新增受保护基线差异；本包只改准确四脚本。Ledger485/95通过。
- 没有新增Play/资源部署/身份或schema升级。生产OrZero调用者有完整回归，metadata模型本身仍仅数据合同通过。

## 下一唯一入口

NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001，准确Record前先读取父metadata Task/头部审计及本报告。补完整BMP/stats/sequence/armor/weapon_piece AST、有序与有效性合同，再复制到本不可变模型并接实际manager；不重建数值算法或另一套字段容器。

520角色精度、3096非角色缺省、stats.max_mp/weapon_piece及非例外shadow/bound实际接线差异仍待。18armor/已匹配序列/声音的既有行为保持，不恢复platform/HUD/selection排除。所有新metadata字段须进入同Q05 identity/hash/联合版本清单，Q06/Q09/Q10与Play按既定依赖回访。Unity/GAS、非战斗、33ms/3ms、十一阶段、stage.dat暂缓保持。
