<!-- CHANGE-RECORD
id: NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: EXISTING_RETIREMENT_TEST_EXPECTATION_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: Existing verified NTSD28 B6 HolderCopy residual retirement and kind2 pickup atomic integration; live BattleDamageWriter and BattleHitCandidateSequenceRunner guards; current goal.
evidence: VERIFIED_TEST_ONLY / FOUR_OLD_FAILURES_CORRECTED / FINAL_375_PASS / FULL_SELFCHECK_PASS
-->

# Q05相关HitPlan旧夹具纠正

状态IN_PROGRESS / RED_OBSERVED。此包独立于几何内容变更，仅修改一个Editor测试文件。已实际RED：Geometry联合job03da83ed3c744e8885326a3fd1dc3de5共375项371PASS/4FAIL，原始GREEN-attempt1.xml保存于Geometry工件。具体是standard lethal一项、alternate两项和kind7 disposition一项。

依据：NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001第32/83行明确已退休holder.ComboCountAtk/KillStat额外写入并保留native knockout和world/victim accounting；当前BattleDamageWriter已无HolderCopy/ComboCountAtk/holder.KillStat写入。三测试仍在注入HolderCopy后期待旧holder统计增加。修正为fixture初始ComboAtk3/4及KillStat4保持，其他HP/HPBound/victim/world/native Knockout/RNG/frame断言原样保留。

kind7权威与既有退休见NTSD28-B6-KIND2-PICKUP-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001及当前BattleHitCandidateSequenceRunner约193行。production在ResolveCandidateDisposition前显式kind7→Unsupported，HitPlan预期同为Unsupported；旧ObserveSingleCandidate测试helper直接调用未受guard的compat分类器并期待Pickup。只在该helper复现已存在production guard，并将kind7 testcase期望改Unsupported，不改production helper或重做退休。

这四项涉及的production分类/统计方法不受本轮几何增量修改；三统计case在CurrentTickPlanValid和WriterDifferenceMask=0之后才失败，说明几何指纹比较已一致。不是将未知生产失败改成常量通过。修正前后都保留四个测试并运行完整HitPlan类及Geometry相关集合。

无production/资源/Scene/GAS/框架/版本变化，不增加生命周期owner。回滚经批准仅逆本测试增量，保留其他任务；不commit/push。验收：原4fail关闭、全部375重新实际通过及SelfCheck；若后续断言失败继续按当前权威核查，不批量降断言。


## 限定关闭

VERIFIED_TEST_ONLY。实际改动仅本测试文件的两个holder统计断言区域、kind7 testcase和ObserveSingleCandidate辅助guard。job66adde76e16648ddb66882e8856a13fe完整375/375；新鲜SelfCheck PASS，CS0/Scene clean。全部事实回链Geometry REPORT及GREEN-attempt1.xml/GREEN-final-job.json/selfcheck-result.txt；没有production分派/统计修改，不扩称新的战斗规则验证。

最终Ledger实际PASS：479 records /77 governed code files，见Geometry工件ledger-final.txt。几何九脚本与独立测试一脚本已人工复核；未声明脚本无本轮额外变化。
