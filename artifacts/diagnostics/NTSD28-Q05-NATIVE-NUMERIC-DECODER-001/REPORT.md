# Q05-A1 原版数值解码器出口

状态：VERIFIED_NUMERIC_HELPER_ONLY。父Q05仍IN_PROGRESS，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

新增LoganNumericDecoder四入口：strict int32成功/失败、strict-or-zero、ITR首整数、finite float32。加载期以BigInteger有理数一次舍入到binary32，支持十进制/hex、负零、subnormal与nearest-even；非法/非有限返回正零。int不使用BigInteger。没有接入旧Converter或更改其默认语义，没有新runtime生命周期服务。

## 证据

- RED.xml：新类型不存在时41/41失败，2026-09-13 02:15 UTC，所有失败原因均为decoder missing。
- dotnet build Tools/NTSD28NumericDecoderTests -v quiet：0 warning/0 error。
- 原版Q03 DatParser→FieldBag/CombatRecordDecoder 37组固定见证，Unity初次GREEN41/41（37+4），job c58bc2508386438289f592a09f653017。
- 固定seed280513扩展3622输入，原版诊断exe与同源.NET decoder共10866比较0差异；见differential-input/native/managed/result。
- 直接FieldBag原始输入969项，共3876比较0差异，覆盖leading/trailing ASCII whitespace、NUL及strict成功状态；见raw-input/native/managed/result及raw-native-build-identity.json。此路径绕过DAT lexer，仅用于原始字段解码边界。
- 最终真实Unity43/43，job12eed8dde09846748143a29271cf76c3；新增两个批量测试分别在Unity运行3622与969全部见证，共14742扩展比较。不是仅.NET通过。
- 完整BattleRuntimeSelfCheck：02:26:41.311595 UTC新request，结果mtime晚于request，PASS；旧结果独立保存并标NOT-CURRENT。期间Console请求超时，SelfCheck实际完成后读取最终文件及Console，未因socket超时重复发起自检。
- authority-recheck.json：72个Q03源/头/fixture身份无漂移，正式EXE为B1E13A…9033，原诊断EXE一致。原版是source-linked实际输出，不用Python数值推断期望。
- console-final.json：CS错误0。scene-final.json：NTSD_Battle isDirty=false/root14。
- workspace-protection.json：3059基线文件中3045不变、14为既有Q02/Q04声明变更、缺失0；本子项仅新增decoder/test/tool。Ledger475 records/56 governed code files PASS。

## 实际范围、审阅和限制

四个脚本为LoganNumericDecoder.cs、新Editor测试、source-linked Program.cs和AuthorityRawNumericWitness.cpp；csproj仅链接该helper，无外部包，bin/obj窄忽略。手工审阅整数上下界、符号、原始串终止、指数饱和、direct rounding/carry/minnormal/overflow及加载期调用边界，结合上述双运行环境证据通过。原始FieldBag诊断的scope追加首次因GBK读取失败而未写入，后续patch已落盘，立即在Record补记顺序失误；未伪记为已成功事前登记。

该helper尚未被生产Converter消费，不表示CPoint27/OPoint24已经迁移，不表示六个正式DAT已载入或新资源已发布。本包未新增Play/GPU验收；因没有生产调用方，Play不能验证尚未接线的能力。schema保持entity12/aggregate20/checksum23/character1/base1；正式资源未迁移，33ms/关闭十一阶段/Unity和GAS框架保持。

下一唯一入口：docs/ai/TASKS/NTSD28-Q05-LOGAN-CONTENT-MODEL-INTEGRATION-001.md，先将115候选inventory收敛为准确模型/转换/caller及测试路径并建立Record，再在同一个Q05协调窗口推进；不重跑Q03或Q04已关闭职责。旧phase断言留Q12、landing division一ULP留Q06，默认stage.dat保持暂缓。
