# NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001 — Spark逻辑推进与发布边界审计

<!-- CHANGE-RECORD
id: NTSD28-B3-SPARK-ADVANCE-BOUNDARY-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan SimulationTickDriver28 C01 advance_native_sparks, hit-record writers and GameSession/main completed snapshot; Unity hit-record presentation cycle.
evidence: TASK-CONTRACT-CREATED / ACTUAL-FIRST-DIFFERENCE-SPARK-VS-COOLDOWN / AUTHORITY-C01-FULL-SLOT-ADVANCE / TERMINAL-LAST-DIGIT-9 / NONTAIL-TERMINAL-RETAINED / ONE-TAIL-POP-PER-PASS / NEW-HIT-AFTER-C01 / CAPACITY-10 / COMPLETED-SNAPSHOT-AFTER-SESSION-TAIL / UNITY-CAPTURE-THEN-WRITEBACK / UNITY-NO-PUBLICATION-SEPARATE-WRITER / WORKER-SAME-U25-WRITER / UNITY-VALID-AGE-RANGES-0-4-10-14-20-28-30-38 / RESOURCE-AVAILABILITY-CAN-FREEZE-LOGIC / FULL-SNAPSHOT-INCLUDES-HIT-RECORDS / LOCKSTEP-CHECKSUM-OMITS-HIT-RECORDS / EXISTING-CALL-NOT-SAFE-TO-MOVE / NEXT-NTSD28-B3-NATIVE-SPARK-LIFECYCLE-CORE-001 / NO-SOURCE-CHANGE / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / PRESENTATION-DRIVEN-LIFECYCLE-CONFIRMED / EXISTING-CALL-NOT-MOVABLE / NEXT-NATIVE-SPARK-LIFECYCLE-CORE`

## 当前事实

- Authority在C00后立即推进上tick sparks，之后才跑producer/input/motion/hit；本tick新spark不会被C01推进。
- Unity current U25先capture hit-record cycle，再在`FinalizePublishedHitRecordCycle`把captured age回写`+1`；
  `buildPresentation=false`使用另一条`AdvanceHitRecordsWithoutPublication`。
- B3 actual trace首差已经固定为expected spark / actual cooldown，但尚不能据此直接移动调用。

## 审计计划

- 追Authority age数组/删除、hit writer和snapshot builder。
- 追Unity Add/Advance/Remove、runtime data catalog、publication cycle、worker与snapshot/checksum。
- 输出single-writer和最小迁移包。

## 结果

- Authority：C01使用native id 0～99与末位9 terminal tail规则；hit在C14追加，因此new hit保持base一整tick；
  completed snapshot在Session post之后读取。
- Unity：U25 capture后由presentation finalize/no-publication分支修改逻辑age；new hit同tick被写回，且生命周期
  依赖资源catalog，valid age范围也不等于native terminal规则。
- worker与同步路径都受同一U25边界影响；full snapshot含hit-record，checksum暂未包含。
- 结论：不能仅移动现有方法。下一包先写精确native logical primitive，production unconnected；再另包接C01。

## 回滚

仅治理记录，无production改动。
