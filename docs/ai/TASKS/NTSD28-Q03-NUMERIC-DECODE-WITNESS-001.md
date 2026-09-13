# Q03 native数值解码位模式见证

状态VERIFIED_SOURCE_LINKED_NUMERIC_CAPTURE_ONLY；37输入native双跑稳定、Unity源链接37/37完成，333值113不同，见同ID artifacts/diagnostics/REPORT.md。父Q03整包未关闭。本包只新增workspace离线诊断工具，不修改生产解析器、Unity/权威资源、schema或Scene。

准确写范围：Tools/NTSD28Q03Numeric/AuthorityNumericWitness.cpp、Build-And-Capture.ps1、README.md；artifacts/diagnostics/NTSD28-Q03-NUMERIC-DECODE-WITNESS-001的合成DAT、manifest、实际输出/报告；对应Task/Change及治理文档。

链接正式Logan source的DatParser、FieldBag、CombatRecordDecoder，分别记录三个CPoint float32的原始bits、CPoint injury整数、WPoint X及ITR四类action的有效整数。涵盖signed zero、subnormal/underflow、舍入邻界、float溢出、NaN/Infinity、hex/指数、非法尾部、int边界、重复字段与packed action。输入同一份DAT，不直接调用运行库转换冒充game decoder。

验收：正式EXE hash，编译source/header/input hash前后稳定，native实际编译和两次输出一致；保存原始bits而非仅JSON小数，并与已批准的Q03规范对照。此证据不是正式EXE实时trace，也不是尚未实施的Unity新decoder通过。差异不得隐去；必要时据实际native行为修订合同。

副作用仅离线进程、workspace输出；无新的battle manager/queue/worker，无关闭顺序影响。回滚为经批准仅撤销本包新诊断文件，保留用户工作/历史证据，不重建或替换正式EXE。
