# Q03 数值解码见证

2026-09-13。状态 VERIFIED_SOURCE_LINKED_NUMERIC_CAPTURE_ONLY。37份同输入DAT实际通过native DatParser→CombatRecordDecoder，输出三个CPoint float32原始位模式及六个整数/action值。native双跑一致；当前Unity生产Parser/Converter源链接程序也实际处理37份DAT，全部语法和Converter完成，不等于数值对齐。

333个有效值比较中113项数值不同；111个投掷速度槽位另标native float32对Unity int的类型差异，3项记录为Unity整数0无法保留native负零。分类见comparison.json，不把类型差异与数值差异重复相加。

## 实测规则

| 输入 | native float32 bits / 含义 | native普通int | native ITR action首整数 |
|---|---|---:|---:|
| -0 | 80000000 / 负零 | 0 | 0 |
| 1.5 | 3FC00000 / 1.5 | 0 | 1 |
| -2.25 | C0100000 / -2.25 | 0 | -2 |
| -842150451 | CE48C8C9 / -842150464 | -842150451 | -842150451 |
| 1.401298464324817e-45 | 00000001 / 最小正subnormal | 0 | 1 |
| -7e-46 | 80000000 / 负下溢零 | 0 | -7 |
| 1.000000059604644775390625 | 3F800000 / ties-to-even为1 | 0 | 1 |
| 1.0000000596046448 | 3F800001 / 紧邻上一个float | 0 | 1 |
| 3.4028234663852886e38 | 7F7FFFFF / 最大有限float | 0 | 3 |
| 1e39 | 00000000 / 非有限解码归零 | 0 | 1 |
| inf / -Infinity / NaN / nan(payload) | 00000000 | 0 | 0 |
| 0x1.8p+1 | 40400000 / 3 | 0 | 0 |
| 0x10 | 41800000 / 16 | 0 | 0 |
| -0x1p-149 | 80000001 / 最小负subnormal | 0 | 0 |
| +5 | 40A00000 / 5 | 0 | 0 |
| 12junk | 00000000 | 0 | 12 |
| 1,5 | 00000000 | 0 | 1 |
| 010 | 41200000 / 10 | 10 | 10 |
| 2147483648 | 4F000000 | 0 | 0（整数溢出） |

普通int要求完整有效十进制，不接受leading plus/小数/指数/尾随文本，也不做饱和溢出。ITR caughtact/catchingact/pickedact/pickingact使用有效最后field中的首整数，因此一些普通整数不接受的尾部仍会返回前缀；不能用一份宽松ParseInt覆盖两类。

重复字段：最后一次非法值会取代前面的有效值并得到0；不是忽略非法项而保留前值。packed动作 `130 131 / 403 404 / 200 201 / 100 101`分别得到130/403/200/100。大写THROWVX/INJURY/X/CAUGHTACT不匹配native的精确大小写key，输出0。以上均走完整DatParser而非直接构造FieldBag。

这次边界支持Q03的三套数值合同：strict int、ITR首整数、CPoint finite float32。C#后继实现必须支持native实际接受的hex和指数形式、保留float舍入/负零/subnormal，不能简单改成float.TryParse的默认参数就宣布完全兼容。完整输入/输出是Q05聚焦测试的参考，不以Python数值转换定义权威。

## 身份与执行证据

- 实际命令 `Tools/NTSD28Q03Numeric/Build-And-Capture.ps1` 编译调用未改写的dat_parser.cpp/dat_document.cpp/combat_records.cpp。正式EXE SHA强校验为B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；source/header/37fixture前后无漂移。见identity-and-stability.json与fixture-manifest.json。
- native.tsv与native-rerun.tsv SHA均 `77C8FF1D8BD71FF05EA462C99D8EB9B4712495477996C8EE148944779DD2C7F3`。
- 实际 `dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-restore` 0 warning/error；随后`dotnet run --no-build -- --input-root <本目录>/fixtures --input-mode plaintext --output <repo>/Temp/NTSD28ContentAudit/unity/q03-numeric.jsonl`，37/37 parse/Converter完成。
- Unity源链接输出SHA `FE32B2E0B227296CCB258850D2002BC05BEDF47B4A6910477C5878765A631F8B`，原始数据保留于工具允许的Temp路径。comparison.json记录逐字段实际值/bit/类型差异，未修改Q01工具来绕过输出限制。
- 台账检查PASS：472 records /45 governed code files。新脚本仅两个离线工具；没有Assets脚本、生产资源或版本变更。

## 限定出口

本包关闭“当前native数值边界输出可重放”的诊断职责，不关闭Unity新decoder实现、Q03整包、Q05 schema、整场Play或最终对齐。未运行Unity Editor/SelfCheck/Play，本轮.NET源链接0错误也不宣称Unity Editor新编译证据。

Q03出口审核时还发现旧Oscillate reader和base-shell恢复未退休，必须修正此前“base-shell不需升版”的暂定结论；这一项归父Q03合同补充/Q04剩余reader/Q05载体删除，数值见证本身不改这些脚本。
