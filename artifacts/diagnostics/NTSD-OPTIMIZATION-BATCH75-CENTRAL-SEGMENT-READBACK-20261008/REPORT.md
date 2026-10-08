# 第75批：中央segment实际绑定断点诊断

末交付审计（2026-10-08T11:03:46.5907298Z）：599保护0变化、原8副本与修补2副本均SHA核同、旧测试全文保持、最终source/publicXML SHA同记录、HEAD0e580f7bf94a7d645958f8dbace80a9abf9a4ae1不变/staged空；原EditorMenu savedclean8/nonPlay/idle/CS0。实际Tools/Validate-ChangeLedger.ps1 exit0/1371Records/4当前代码diff全covered/4323历史warning未清理；准确git diff --check exit0。final-audit-01.json记录证据；诊断收口不等于SetPass/0GC/H07/H11完成，不调用Goal状态变更。首次长guard JSON只读输出截断导致parse失败、零文件写；一次组合patch重复目标拒绝零应用，分拆正确hunk后完成，无覆写/删除/恢复用户改动。

## 最新终态：中央分段根因已定位（仅诊断VERIFIED）

窗口02 MEASUREMENTS_COMPLETED，真实OID1/1000 AI、120warm＋180sample完整，tick128/sample8唯一只读快照成功、CPU lease当回调归还且观察者detach；末对象/slot/borrower0、十一阶段关闭、双Scene同、原Menu savedclean8/idle/非Play/CS0。窗口01 PARTIAL原件和诊断SessionState缺陷历史保留，不消去失败、不追加第三窗。

本次重新扫描的直接证据：

- windows-02/00-combat1000-readback/report.json.central-segments.json:4为实际OrderedChunks，14为cpuLeaseReleased=true，17为实际Auto回退理由，20为145计划页，27为2024物理segment，28/29为2432696320计划atlas bytes /536870912预算。估算2320MiB（2.265625GiB）超512MiB；不是已分配图集或整进程内存实测。
- 同快照绑定全部SourceTexture2D；2026resolved/source command、1chunk、2024segment，相邻2023断点全mask4（仅Texture身份变化）。3张真实bound texture：s（ID40780，991段）、saku（ID-191750，991段）、SPARK_native（ID-244190，42段）。材质只ID37568、variant只1、shader只NTSD/BattleCentralTransparent；没有本帧Strict/chunk/material/binding/page/slice/command/quad gap断点。开头s/saku交替，s源Assets/NTSD/Sprite/XueYuan/s.png；Shadow.prefab:63的GUID04bffa57fa05fe44e8b259e1ba07141a与s.png.meta:2一致，确为既有shadow资源引用，不读Q06排序内部。
- CharacterAnimtorManager.cs:1467—1482从当前configSource复制stagedConfigs并遍历加载，该循环不以当前活动纹理为筛选依据；上游configSource是否已裁成最小完整本局依赖尚待资源专项核实，本帧3张纹理不等于完整依赖。2958—2970实际Auto分支按总计划bytes超budget回退SourceTexture2D，2985—3008保留角色/共同视觉source资源；本快照reason对应此分支。预算守卫不是错误，不允许用抬预算掩盖规划/合批问题。
- BattleDynamicMeshBackend.cs:17每chunk4096quad；258—265需同chunk/非Strict/IsCompatible/连续quad才能附加；475—486以Texture/Material/variant/mode等比较，array模式不因slice不同分段。BattleRenderFeature.cs:57—86每个有效segment一CommandBuffer.DrawMesh。实际资源纹理交替即当前CPU大量分段的直接机制，不是“中央系统天然等于一draw”、不是chunk容量不足。
- 完整02窗89已完成显示帧SetPass平均1995、segment1990.4943820224719、central submission CPU draw1991.4943820224719；不同指标/采样口径不可互等。真实GPU batch/SetPass组成未capture，不从CPU draw推GPU数。此窗含一次诊断冷分配/IO，不作FPS收益或完整0GC证书；没有SetPass下降/生产修复。
- tick300完整final-checksum.json bytes与72 Combat baseline逐字节SHA相同：E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12；只末态对照，不冒充逐tick/native全合同。

验收层级：新18有效RED→28/28，必要2SessionState RED→30/30最终GREEN；只准确Editor Suite新诊断。旧测试尾全文保持，sourceSHA43ECB5B1FA0570A6E5B413CE8FB28EC371B8255879978E0B5B2EC10254883D8C；窗口02成功和readback-summary-01.json独立汇总根因。实际597/599保护、10副本、validator/diff等末审计另追加。

下一方向：优先解决“本局完整视觉依赖与共同shadow/spark的预算内共享纹理绑定”；不能只选当前3张活跃帧纹理，须包含后续帧/OPoint完整依赖、现有fail-closed和双预算。现有array backend可消费同array不同slice，暂无证据必须先上GPU Instancing；不得改publication顺序、跨segment重排或直接增预算。具体资源/ATLAS实施仍在冻结边界，须准确方案并取得用户授权；此报告不升格EXT-1、不启动专项M0或改bank/资源格式。未获该范围授权只约束该修复，不停止已有Goal：74局部收益候选仍defaultOFF，后继同Brute Driver资格和H11必要问题仍可继续，不重复本根因取证/有效30 GREEN。

54已执行、阶段4/6、H07/H11 OPEN、父关闭0、Goal active；VERIFIED只本次诊断，绝非SetPass/H07/H11优化已完成。

## 窗口01失败与准确修补声明

窗口01终态PARTIAL，warm120/sample180/logic300、minAI1000，中央89已显示帧SetPass平均1995/segment1990.494382，未降；一次readback没有执行。Suite已十一阶段关闭，objects/slots/borrowers0、双Scene同、原Menu恢复，失败全部原件保留。
真实检测缺陷：SessionState经JsonUtility roundtrip把RunState.centralSegmentReadbackError的无错误null字符串恢复为""，当前callback用!=null前置拒绝、完成用==null也误拒；suite-result明确error=""。不能把这解释为RenderPass/GPUlease拒绝或渲染根因。修复仅本Editor诊断的空字符串合同，绝不放宽实际非空错误/lease/RenderPass。
下一修补前冻结2新CentralSegmentReadback_ErrorSessionRoundTrip（null/""），先现状实际RED，再用string.IsNullOrEmpty；原28受影响窄域保持，合计30 GREEN。已有01输出不覆盖，新窗口02同固定65字段仅outputPath变化；只在缺陷修复/编译/30窄门后作一次必要第二窗，不重复01尝试同逻辑、不采GPU/M0。
本次仍唯一Suite写域；常量/本批owned RequestOnlyOutput期望改02、合法terminal所有权仅本Batch75 artifact精确root（保留01且承认02），不泛化其他路径。操作修补前保存当前dirty Suite与本75公共terminal至before-repair-01准确ABSENT副本并SHA核同；原8副本/599保护不动。


PLANNED / DIAGNOSTIC_ONLY / PERFORMANCE_UNMEASURED。一个Combat1000 120warm+180sample、一次实际提交快照，代码仅现有Editor Suite及18窄新tests。不预设根因/SetPass下降，不运行GPU capture/EXT1 M0或改bank/budget/资源/排序/segment。
Task docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008.md；Change NTSD-OPT-H07-CENTRAL-SEGMENT-READBACK-075；Operation同Task。原Editor6402/PID78296当前Menu savedclean8/idle。失败原件与未知保留，具体结果后续追加；不是H07/H11完成。
