# 第67批：中央segment纹理属性准备复用限定评估

当前结论：SCOPED_CORRECTNESS_PASS / LOCAL_GAIN_SIGNAL / NOT_ADMITTED_FOR_PRODUCTION。默认OFF，H07/H11 OPEN、六阶段4/6、Goal active。原Editor测试完成后进程19040消失，关闭原因未知；当前无任何Unity.exe，没有自动启动/重启。最终live Scene/Console状态无法再验证，不把旧clean观察冒充关闭后的结果。

## 范围与实际修改

只两个准确C#：BattleRenderFeature、BattleCentralSameSamplePixelEditorTests。原body DrawMesh循环抽取internal AppendSegmentDrawCommands；同一次调用内连续相同实际Texture引用与BindingMode仅复用MPB Clear/SetTexture准备，第一有效segment或key变化必须准备。每有效segment仍录制原mesh/submesh/material/pass/Matrix的DrawMesh，无segment合并/跨chunk合并/顺序变化。Foot/Health、CommandBuffer/lease finally、RecordSubmission原链保持；Shader IDs同Feature上提共用，default false及两个primitive Last计数，invalid Execute入口清零。纯局部borrowed Texture引用不跨camera/slot/lease，无新常驻资源/实例buffer/热扩容。

Q06只状态/hash；不读方法体。Query/Driver/Suite/Observer/Harness/Benchmark/native、Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server与并行TMP不改。没有EXT1 M0/instancing/ATLAS/Mono或Role-aware推广。Task68仅新文档准备，未写后继脚本/启动实景。

## 实际test-first与作业证据

| 阶段 | 原Editor作业 | 实际结果 | callback XML SHA-256 |
|---|---|---|---|
| RED | df6f0b9d2a5444498e4265ace5fc4844 | 25实际case/0pass/25fail，24缺helper＋1缺opt-in | 9A5C4C7EE713AD176D7CA4970AD065966E2ACBB269C1592980B895B6CDD07086 |
| GREEN | e3579f27cc454ef1b34cec22b3b69c58 | 25/25 Passed/0skip，3.333055秒 | E0A36E30B2BF98E0A5A6A50A23B92C52D7FBB6EBB4407605F72C658D5437C069 |
| Cost | 29618ee5e63b445e932eb1fc70e23cff | 2/2 Passed/0skip，1.7088204秒 | FF445A47B66895526B5D65010AE7564205ECF11F759964532C0F8C6B99C1BCFA |

实际27去重case（新纯25＋成本2），不是整catalog9943，也不是把RED失败计通过。旧6 pixel case/断言/输出保持，未重复全历史。每次callback下一回调前已fresh复制/核SHA；所有请求/start/terminal/XML与discovery原件保留。重载时原6402 connection-refused两次只观察同PID/端口，待确实idle再启动作业；没重启、没把0case/编译失败算RED。首次fixture mode强转在import前重扫enum纠正为命名映射，不更改生产枚举。

纯资格覆盖defaultOFF、首次/重复准备3、全6mode转换、3真实纹理identity、新call3、非法skip/empty/Foot-body-Health3、三mode×两drawMode像素6。12固定alpha0.5同/异纹理样本均OFF/ON→独立quad最大通道误差0，内容1147px、反序负控810px；实际材质变体/array两slice、UV/Flip/tint原publication采样不变。新67 PNG已生成；本轮目视检查array/Strict/changed候选PNG为非空重叠像素，仅用于辅助，不替代数值或真实Battle画面/Q06全域排序验收。

## 唯一固定CPU命令录制成本

两pattern各2000有效physical segment，每侧2000 DrawMesh/168000录制字节完全相同，4warm＋8sample，balanced轮换先后。timer包含真实native CommandBuffer.DrawMesh录制，不含Build/清buffer/断言/输出，也没有执行GPU。冷CreateDelegate绑定一次，热录制无反射；完整生产0GC未测，GC=UNKNOWN。

| pattern | OFF均值ms | ON均值ms | 均值变化 | OFF中位ms | ON中位ms | 中位变化 | ON较低配对 |
|---|---:|---:|---:|---:|---:|---:|---:|
| constant | 5.2033875 | 3.6008500 | -30.7980% | 4.11735 | 3.25445 | -20.9577% | 6/8 |
| alternating | 3.9516875 | 3.9147875 | -0.9338% | 3.65605 | 3.70275 | 1.2773% | 4/8 |

constant：OFF准备2000/reuse0，ON准备1/reuse1999；alternating两侧准备2000/reuse0。均值受原OFF8.8304/10.9865ms等波动影响，保留全部原sample，不剔除/补采。中位是对同8raw的补充分析，不改变固定采样/验收。仅constant fixture存在积极信号，不能将30.8%外推所有战斗；alternating均值小降、中位小增，按收益不可辨。没有actual1000AI/Scene FPS改善、GPU batch减少、逻辑P95达标或可靠完整0GC证明，不切默认。

全部raw（每侧8）：
- constant OFF=[8.830400000000001, 4.274900000000001, 3.9598, 4.5667, 10.986500000000001, 3.3401, 2.9007, 2.7680000000000002]；ON=[4.5923, 3.0238, 2.6823, 2.5958, 5.253900000000001, 2.9278, 4.2458, 3.4851]。
- alternating OFF=[3.0171, 3.5628, 3.0934, 3.7493000000000003, 4.4126, 5.430000000000001, 4.8382000000000005, 3.5101]；ON=[3.9730000000000003, 3.726, 3.1343, 3.6795, 3.4155, 4.4682, 3.0116, 5.910200000000001]。

## 原失败与下一动作

65千人两计时OFF窗logic平均72.355/73.673ms、P9584.519/94.257ms、drop683/611、PairExactLoop42.118/44.256ms仍FAIL；显示平均233.802/217.173ms只是Editor frame delta。67未重采/更新这些实景值。66 NO_GAIN、不推广以及43迟发12camera event FAIL、47未复现、61byte预算UNKNOWN保持。M03/H06/M13/M14已完成限定评估不重开。

后继[Task68](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008.md)仅READY/未实施：用现有Suite一次四120+180 OFF/ON真实资源窗口验证复用率/显示收益，冻结准确新owner/字段/tests/副本后才能实施；不重复67cost/像素或先推广。当前Editor前置不可用，无法开始有效RED/实景；没有因此降低H07正式120+1800/logicP95<33/drop0等门，Goal继续active。

## 保护与验证（事前与中间快照）

事前9初始dirty/源码/公共XML副本于04:15:57.2335647Z逐SHA核同，266guards包含66原件/旧失败/Q06/Server/TMP/Scene/设置/authority。GREEN前04:29:20.0451295Z全266无漂移/HEAD527350afa08633357ba20e7453a9d260eaa2b97c同。初次诊断错误Join绝对J路径造成假missing，正确IsPathRooted重扫无缺失；没有恢复/移动/删除。准确两C# git diff --check已实际exit0/仅CRLF提示；完整最后validator/guard/backup/源码后置记录另追加。没有Git reset/checkout/clean/stash/删除/移动/commit/push或第二Editor。

## 最终核验与范围外变化

UTC2026-10-08T04:44:20.5582710Z terminal-audit-01.json为实际可解析JSON：HEAD同、9backup全部初始SHA同、两个C#与GREEN/Cost源码指纹同；264/266guards同，另两项字体assetSHA改变及旧Temp/stress result缺失不符，不称全保护PASS。公共Temp/XML也当前缺失，但所有原始RED/GREEN/Cost XML以及初始九号副本仍在。旧533B stress result有66第10副本且完整D3980E4A…7EAB核同；不恢复或覆盖。字体当前B1F4AD54…09FE/2127152B保留，执行者/原因未知，详[UNKNOWN_CAUSE事件](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH67-POSTEDITOR-PROTECTION-DRIFT-20261008/RECORD.md)。Scene/settings/Q06/authority/Server等其余保护保持，仅文件证据，不等于最终live Scene clean。

准确两个C#＋总表＋Task67的git diff --check实际exit0，3条CRLF提示；不是整个dirty工作树/旧治理EOF无warning。pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot 当前根实际exit0、1364Records/6governed diff files/4314WARNING/0ERROR，完整stdout未持久化，仅摘要。第一次摘要WARNING正则未含行首空格，错误0保存在validation-summary-01，02以实际重跑修正4314，仅计数更正，不改Record或弱化validator。

第一次终核tool stdout被git numstat的CRLF提示污染、JSON.parse拒绝，未保存为有效JSON；修正仅捕获stderr/不打印numstat后保存当前有效audit，原工具输出中的两变化照实保留。没有因工具观察/解析问题重启测试或补采。当前无Unity.exe/6402监听，live Scene/Console最终观察失败原件保留，无新编译0/运行时证书承诺；原Editor已实际执行本批27case提供对应编译/聚焦证据。

本轮PROGRESS：test-first C#实现、25GREEN与唯一2cost新证据并决定后继实际覆盖验证；非仅状态重述。六阶段仍4/6/H07/H11 OPEN/Goal active，不因当前Editor前置缺失、旧次数或本批收口标complete/paused/blocked。Task68实施前须以当前保留资产/dirty重新冻结，不把67初始字体指纹或旧Scene观察当新baseline。
