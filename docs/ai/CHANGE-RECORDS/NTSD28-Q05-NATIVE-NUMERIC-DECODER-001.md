<!-- CHANGE-RECORD
id: NTSD28-Q05-NATIVE-NUMERIC-DECODER-001
status: VERIFIED
change-kind: LOGAN_LOAD_TIME_NUMERIC_DECODER
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganNumericDecoder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05NumericDecoderEditorTests.cs
code-path: Tools/NTSD28NumericDecoderTests/Program.cs
code-path: Tools/NTSD28NumericDecoderTests/AuthorityRawNumericWitness.cpp
authority: Active Q05 user goal; Q03 numeric37 source-linked evidence and frozen VERSION-IDENTITY-AND-CAPTURE-CONTRACT; formal Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: VERIFIED_NUMERIC_HELPER_ONLY / RED_41_FAIL / UNITY_43_OF_43 / EXPANDED_NATIVE_14742_COMPARISONS_NO_DIFF / SELFCHECK_PASS / PARENT_Q05_ACTIVE
-->

# NTSD28-Q05-NATIVE-NUMERIC-DECODER-001

## 事前合同

同ID Task列准确scope、authority、验证与回滚。原状：旧ParseInt接受前缀并饱和，CPoint存int，不能提供已冻结的native数值结果。改后新增纯加载期decoder，先不改旧调用方，作为Q05同窗口的数据前置；不存在新的版本发布。

BigInteger用于加载期精确ratio/nearest-even，不依赖浮点运行库二次舍入或当前区域设置；int入口不使用BigInteger。函数无Unity对象、线程、单例、I/O、全局状态或资源写入，无关闭阶段接入要求。新增工具只读输入、stdout输出，编译产物留工具自己的bin/obj并按窄范围忽略；不添加依赖包。

测试前先写Record；取得RED后实现。实际文件/符号、结果、风险在下方追加。Q05后继模型、ABI、旧快照拒绝仍未完成，不能将本包单独称为正式内容可用。

## 实际验证

RED已实际运行：41/41因decoder缺失失败，原始XML保存于本包工件RED.xml。新增decoder及source-linked runner已写；dotnet build 0warning/0error。3622有效字段输入native差分10866项0差异，Unity GREEN job c58bc2508386438289f592a09f653017待收集。


## 原始字段边界补充与记录顺序纠正

新增准确code-path Tools/NTSD28NumericDecoderTests/AuthorityRawNumericWitness.cpp。原因：DAT lexer会归一化空白，需直接向权威FieldBag注入原始value以验证前后空白与嵌入NUL。此诊断源链接既有dat_document/combat_records，仅stdin读取id/hexbytes，stdout输出实际decoder及strict成功状态；不修改权威源码或生产caller，无runtime/关闭依赖，编译产物仅Temp，同本包回滚边界。事前写入补充的Python调用因默认GBK解码失败，但后续patch仍添加了该诊断文件；本条立即补记此顺序失误，不将其伪记为成功事前登记。尚未编译或执行该新诊断，后续继续前已修正准确scope。


## 最终验证与实际职责

最终限定VERIFIED_NUMERIC_HELPER_ONLY；完整事实、命令、失败重载请求、范围追加顺序纠正、代码审阅和未验证边界见 `artifacts/diagnostics/NTSD28-Q05-NATIVE-NUMERIC-DECODER-001/REPORT.md`。Unity最终43/43（含3622+969扩展见证）；完整SelfCheck PASS、CS0/Scene clean/root14；Ledger475/56PASS。BigInteger仅新decoder加载期使用，旧caller未接线，schema和正式资源保持；不声明运行时对齐。下一Q05内容模型/Logan入口Task，不得关闭父Q05。
