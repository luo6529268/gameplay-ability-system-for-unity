<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: IMMUTABLE_NATIVE_DEFINITION_FIELDSET_AND_NUMERIC_VALIDITY
code-path: Assets/NTSD/Scripts/Animation/LoganDefinitionMetadata.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganNumericDecoder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05DefinitionFieldSetEditorTests.cs
code-path: Tools/NTSD28Q05DefinitionFieldSet/AuthorityOptionalNumberWitness.cpp
authority: Formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; FieldBag last/integer and input_routing.cpp field_double optional; prior header audit.
evidence: RED_4 / FOCUSED_486 / OPTIONAL_NATIVE_4045 / FULL_SELFCHECK_PASS / VERIFIED_FIELDSET_MODEL_ONLY / AST_MANAGER_PENDING
-->

# Definition字段集合与有效性事前合同

父Task NTSD28-Q05-NATIVE-DEFINITION-METADATA-INTEGRATION-001，先数据合同与有效性，再metadata AST/manager接线；不改变该硬顺序。准确四脚本，不改production调用者或资源。需要能表达absent、声明但invalid、显式0、非空stats及重复last-win；不能用ParseOrZero丢失fallback条件。

新增LoganDefinitionMetadata，包含Bmp/Stats两个不可变LoganDefinitionFieldSet。字段集合复制ordered KeyValuePair<string,string>，保留全部原始声明，Ordinal key、last text/int/double，预先解码有效int32/float64（不能在tick中解析/分配）。有效性与值独立，fallback由caller提供，不硬编码max_mp500或某个通用rate。公开只读集合不泄漏可写list/dictionary；输入列表/源对象后改不影响集合。无需新的manager/queue/worker/服务或停止阶段。

LoganNumericDecoder增加TryParseFiniteFloat64，返回valid与exactbits；既有32/64 OrZero行为逐bit保持。复用当前lexer/rounding，不使用double.Parse或复制第二套浮点算法。invalid/overflow false且输出+0；underflow可有效且保留signed zero，显式±0有效。native optional helper精确源码片段及FieldBag integer通过独立witness捕获valid+bits；fixtures覆盖NUL、空白、nonfinite、overflow/subnormal、boundary/random与旧样本。

先RED再模型/helper；实测原版optional与缓存last-win/absence/raw非空、输入隔离、只读防篡改、fallback（requestMP与rate）及原numeric43/14742回归。可从已冻结330头部raw输入构造模型验证值，但必须明确这只是数据合同测试，不是manager接线；metadata原始AST/weapon_piece结构与pipeline后继继续。必要源链接工具引用不在本包变更（当前numeric source独立编译已引用，新增metadata未被Converter引用）。

数据/接口不能偏离已批准例外。stats.y/platform、smallb/HUD、hidden/random/selection只保留raw，无行为。新集合之后须进入Q05 identity，Q06/Q09消费仍后继。Unity/GAS/非战斗/Scene/InputActions/33ms/十一阶段/stage暂缓保持。回滚经批准仅四脚本差量，保留用户/前包工作。

## RED与落盘

job6a5043f8476c4e6facebe349f27f6e71实际4/4 FAIL，RED.xml保留（模型与optional入口缺失）。随后按四路径实现不可变Bmp/Stats字段集合、预解码缓存及TryFinite64有效性；binary32/64 OrZero共享算法值不改，valid只标记invalid/overflow与有限数值。原版4045输入双跑稳定。尚未接manager/metadata AST/weapon_piece，不报告消除实际header差异。当前CODE_WRITTEN，下一编译/4新+原numeric/typed回归。

## 限定交付

四脚本真实变化：不可变metadata/fieldset、numeric valid输出、新测试和原版optional工具；行为/边界见前述合同，实际证据见同ID REPORT。RED4→486PASS，新4045 valid/bits及缓存/不可变性与旧numeric/typed回归，SelfCheck 2026-09-13T07:29:39.336596+00:00 PASS、CS0/dotnet0error、输入0漂移/保护无新增差异，Ledger485/95PASS。

状态FOCUSED_TEST_PASS/VERIFIED_FIELDSET_MODEL_ONLY，尚无manager/metadata AST绑定，不能关闭实际header差异。下一 NTSD28-Q05-NATIVE-DEFINITION-AST-SOURCE-INTEGRATION-001，同Q05继续。
