<!-- CHANGE-RECORD
id: NTSD28-Q05-SPRITE-CORPUS-ADMISSION-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_ONLY_NATIVE_CORPUS_ADMISSION_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11NativeSpriteRangeEditorTests.cs
authority: Formal Logan capture SHA 5F5C6C34CE907BB6D7855B8A1E69D410B3CD577807F202332B8A0DA30ED0146F; Q05 native frame admission rejects 3 invalid non-gameplay DAT.
evidence: VERIFIED_TEST_ONLY / RED_CORPUS_ADMISSION_FAIL / FINAL_462_PASS / SELFCHECK_PASS
-->

# 旧sprite全语料夹具准入修正

原状：旧Q02测试无条件要求405文档均ParseNative成功，Q05已按原版parseSuccess拒绝INKHUD/INKHUD2/dataresource三份无效frame文档，当前回归因此失败。精确单脚本，只给NativeDocument加parseSuccess并对原版失败文件断言FormatException（反射wrapper），成功402仍逐sheet完整比较；405全访问、3正确拒绝计数必须断言，禁止简单跳过失败。不改任何production或冻结capture，不改变资源/例外/Unity框架。前置SHA b766eef5be5b435b6a747a9eaf5f8793c9792eb137eccf727971dfcba35b761e。

验收：本类全部14项及native BMP/stats、新版catalog回归；compile/SelfCheck/ledger。回滚须用户批准，只本测试差量，不恢复旧错误准入或变更production。无需shutdown接入，无runtime模块新增。

实施已写：NativeDocument.parseSuccess，原版失败检查wrapper.InnerException为FormatException，明确405/3计数；生产未修改。事前Record和Ledger已成功写入，STATE/handoff首次写入因宿主MemoryError失败，此处补齐并如实登记。Unity重载和回归待。

## 验证结果

最终本类14项与BMP/stats434+catalog14共462/462 PASS，前失败、GREEN XML和job在NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001 artifact目录。完整SelfCheck 08:09:08Z PASS，CS0。生产未改；原405全部访问、3按原版parseSuccess拒绝、其余完整逐sheet核验。VERIFIED_TEST_ONLY。

最终Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path：PASSED，487 Records / 98 governed code files；完整ledger-final.txt保留。
