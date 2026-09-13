# Q05 步骤4 trace/raw/source identity联合升级

READY_FOR_EXACT_PRECHANGE_RECORD，依赖NTSD28-Q05-JOINT-SNAPSHOT-CHECKSUM-VERSION-001限定验证出口。是同一Q05未发布窗口后继，不能称第二窗口或完整发布。先读CURRENT-AUTHORITY、Q03 VERSION-IDENTITY-AND-CAPTURE-CONTRACT全文、hash consumer审计REPORT、semantic identity和2F8 Record。

冻结目标：battle trace/descriptor/comparison/validation/self-test v2→v3；Unity raw header/tick与comparator v1→v2；authority source capture wrapper v1→v2；entities49→50，新增combat.objectAiExcludedGroupSourceSlot。native读object_ai_excluded_group_source_slot_2f8，Unity读ObjectAiExcludedGroupSourceSlot2F8，不用Spawner/Owner替代。新字段的载体与copy/reset/restore有证据，但Q06 producer/AI未接，不能由binding新增宣称behavior对齐。原6MISSING保留，不把null/默认冒充已知值。独立B0-domain/B2-input-RNG在payload没变时保持版本。

明确剩余问题：TraceContract.ContentPolicy及TraceComparator的content-strategy-pending仍为D-023之前状态；当前content只校验policy+manifestSha256，无decode/semantic组合。新头须绑定真实source/raw definition/decode tag/full semantic/联合schema证据；已有LoganContentIdentity V2确定字节与LE projection复用，同producer/双端独立核对。不将Unity assembly SHA与native source SHA当可直接相等值，它们是各自来源证据；共享内容身份才要求相等。content或schema不符先拒绝有意义比较，不能继续报策略待定/填零/混用旧49字段文件。CertificateEligible=false边界保持，source runner不晋升正式EXE证书。

实际路径从Tools/NTSD28Parity/{TraceContract,TraceComparator,EntityFieldContract,RawEntityCaptureComparator,AuthorityCaptureValidator,TraceContractSelfTest}.cs，Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp及Build-AuthoritySourceCapture.ps1，Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs和Test/Editor/NTSD28UnityRawCaptureEditor.cs继续追全部literal caller/自测/manifest/scenario头写入。此清单是起点，必须读实际文件后建立准确code-path Record/preimages；不凭此Task批量替换所有v1/v2/49/43。先设计真实content-root到header的绑定与验证，不能只向CLI增加可任意伪造的摘要就宣称内容已校验。

test-first：旧tag/缺字段/伪semantic/错raw/错decode/跨schema拒绝、新字段nondefault双端source-linked与Unity witness、原6MISSING保持、checker self-tests及实际capture解析/compare（首差如实保留）。正式权威树只读；native诊断在Temp构建且不覆盖正式EXE。编译、相关Unity tests、SelfCheck、ledger及保护清单；父步骤5完整同seed/input恢复回放/slot pool/Play仍随后执行，不能提前Q07。

禁止computer-use，保持Unity/GAS/非战斗/Scene/资源/Gen/Plugins/外部Server与所有例外。无新网络或恢复架构。脚本前准确Record，回滚需批准仅差量。
