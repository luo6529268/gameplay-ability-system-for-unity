# Q05-A1 原版数值解码器

状态VERIFIED_NUMERIC_HELPER_ONLY，父窗口NTSD28-Q05-JOINT-CONTENT-RUNTIME-SCHEMA-MIGRATION-001。本包只建立加载期纯解码能力，尚不改变既有Converter/profile/caller；不是独立schema发布窗口，也不关闭Q05。

准确脚本范围：Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganNumericDecoder.cs；Assets/NTSD/Scripts/Test/Editor/NTSD28Q05NumericDecoderEditorTests.cs；Tools/NTSD28NumericDecoderTests/Program.cs；Tools/NTSD28NumericDecoderTests/AuthorityRawNumericWitness.cpp（原始FieldBag边界，Record已记录补充顺序纠正）。其他文件仅同目录csproj/README/.gitignore、meta、诊断数据及治理文档。旧parser/Converter/资源/Scene保持。

依据：Q03 numeric37的DatParser→FieldBag/CombatRecordDecoder实际输出。定义strict int32（可区分失败）、首整数、finite float32三入口，使用精确最后字段内容。float支持C locale十进制/hex/指数、负零/subnormal与nearest-even，非法/非有限归正零。用整数比值直接舍入binary32，避免先double再float造成二次舍入；BigInteger只用于异步内容加载，不进入tick/worker模拟路径。

验收：先通过reflection发现缺少类型/入口的RED；37原版输入所有有效值/位模式通过；补不同符号/指数/正常值/边界随机差分，复用原版诊断exe作oracle。源链接.NET与真实Unity Editor分别验证。空白/NUL/非法字符串遵守各native入口差异，不能共用宽松ParseInt。后续profile准入、CPoint27和caller接线另在父窗口登记。

无新production manager/queue/worker，不影响关闭顺序或框架。回滚经批准仅撤销本包新文件；没有资源部署、删除、提交/push。严禁以纯解码测试通过宣布六DAT已载入或整个Q05完成。


出口：RED41→Unity43/43、14742扩展native比较0差异、完整SelfCheck PASS、CS0/Scene clean、Ledger475/56PASS。详细REPORT同ID工件。生产接线尚未发生，父Q05仍IN_PROGRESS。
