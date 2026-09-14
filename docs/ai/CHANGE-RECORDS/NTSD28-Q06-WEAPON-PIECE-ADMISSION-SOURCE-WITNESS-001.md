<!-- CHANGE-RECORD
id: NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001
status: VERIFIED
change-kind: WEAPON_PIECE_ADMISSION_SOURCE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/weapon_piece_admission_witness.cpp
authority: Formal battle_world.cpp spawn_at/materialize_weapon_piece_fragments and SimulationTickDriver28::step playable build closure.
evidence: Parent admission-edge Task; two Unity factories reject oid<=0, native generic spawn has catalog and frame gates instead.
-->

# 武器碎片准入原函数见证

修改前合同：仅新增一个workspace native runner，复用现有构建器链接未修改的正式源；不修改Unity/正式EXE/资源。现有157+3样例缺OID0、非法动作及同tick高低slot证据，不能作为完整生成闭合。

覆盖OID0/777/-1映射999及catalog缺失、类型0..6、动作-1/0/857/998/999/1000及999声明资格；阶段0直接fragment出生，阶段1独立相同初始状态完整driver。source20/70/998，空槽全可用/无/一个，高低slot扫描；多variant失败RNG前置，builtin与DAT各自失败策略。输出raw50、出生事件/最终存活、实际generation、frame事件槽、RNG calls及诊断，原stdout复跑相同。未覆盖pool实现失败，须Unity后继测试。

验证：Build-AuthoritySourceCapture.ps1独立Temp目录，runner两次stdout字节比较，结果归artifacts同名目录；准确结论区分直接出生和完整tick，不把driver额外诊断藏为成功。不晋升诊断binary为权威。关闭：无新增runtime服务、队列或资源；回滚仅新runner（删除须授权），不影响当前生产。后继生产变更须另立准确Record及先RED。

VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY：原1800行900出生+900独立完整driver，17286边界断言通过，复跑字节相同；最终build无warning，authority身份与manifest确认，失败事件完整保留。具体命令Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06PieceAdmission -RunnerSource Tools/NTSD28AuthorityTrace/weapon_piece_admission_witness.cpp -ExecutableName weapon_piece_admission_witness.exe；运行binary两次并JSON断言，见artifacts同名REPORT/validation/build-manifest。Unity oid0差异未修复，两factory与pool/highslot需后继RED、实施、验收；无新Unity Play结论。

后继比较发现诊断raw allocationEpoch误用了global presentation_generation，两者应分离：当前见证每slot只分配一次，raw epoch=1，独立generation字段保留。仅改同runner投影并重建/复跑；原spawn/RNG/slot证据保持，旧文件留证。

诊断epoch纠正已验证：原1800行逐项除allocationEpoch改1外完全相同，generation独立保留；新binary复跑字节一致。新raw SHA8bb6b14a…，前hash为纠正前投影；原17,286行为断言结论保持。
