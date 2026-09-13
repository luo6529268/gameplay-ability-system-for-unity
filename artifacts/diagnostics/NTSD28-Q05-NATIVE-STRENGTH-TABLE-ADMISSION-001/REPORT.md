# Q05 Native weapon strength整表加载出口

状态 FOCUSED_TEST_PASS / VERIFIED_TABLE_LOAD_ONLY / FULL_SOURCE_IDENTITY_PLAY_PENDING。总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE，Q05同一窗口继续。

本包在原ParserV2显式Logan入口新增物理行strength解析视图、typed rows和原文保留，复用原property模型与已验证单记录19字段decoder。编号仅1..9、严格整数、跨section重复和字段在entry前报错；caption不当字段、marker精确大小写前缀、EOF及#行行为按native。native区域不会泄漏到weapon_hp/移动metadata；legacy Parse/manager分支保持。CharacterAnimtorManager实际BuildCharacterDataFromSource现消费完整strength19。没有改候选/命中算法或部署DAT/图片，也未升schema。

## 新鲜验证

- Native source-linked witness：34语法夹具，双跑输出一致；涵盖caption/metadata namespace/entry前字段/多section/重复/编号边界/EOF/大小写/marker尾文字/#/ASCII与嵌入CR。权威formal EXE SHA仍B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。当前source、header、fixture hash无漂移，见identities.json/input-drift.json。
- 正式runtime 405 DAT递归审计双跑稳定，其中10个含strength定义共40条，native全部接受；Unity真实manager构建10个定义、逐条raw key/value/caption/index/line及19字段接线均通过。只认证strength域；不把其他frame旧converter当作全内容一致。
- RED job7f9a871c356340b4917459025decacfd：34/34失败，RED.xml。
- 首次绿色jobfdbc6b67c4b5449799e33e39dddb5548：189/189，组成45新整表/正式定义/legacy测试、120 ITR-record、24 source-path回归。第一次选择器误用了两个既有测试的命名空间，未执行项没有计作通过。
- 按实际namespace补跑job0ac3088d13714633ba205f208b6a54c6：74/74，45整表复验（包含细化的header CR-before-caption夹具）+15 catalog+14 cache/caller。两轮有45重复，共218不同测试，不能相加为263不同测试。
- `dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj --no-restore --verbosity quiet`为0warning/0error；同步新增production编译引用及source hash清单。
- `dotnet run --project Tools/NTSD28ContentAudit/UnityContentCapture.csproj -- --input-root Assets/NTSD/Config --input-mode unity --output Temp/NTSD28ContentAudit/unity/q05-strength-table-legacy138.jsonl`：138/138 parse/Converter通过。与上包逐记录比较只有source审计metadata变化，旧投影差异0；不是新27/24/40/19的完整投影认证。
- 完整SelfCheck新请求05:03:44.760955 UTC，结果05:04:38.856867 UTC为PASS；旧结果单独标记NOT-CURRENT。不是重用旧PASS。
- Unity实际重载成功、Console `error CS`查询0；scene isDirty=false/root14。后者不代表磁盘Scene unchanged。
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path`通过：481 records/85 governed script paths（最终文档更新后复验）。未运行本包新增定向Play：本包只认证加载入口；真实持有/命中与完整source/identity/Play保留在Q05/Q06/Q12。

## 场景与用户工作

保护清单3059：3026相同/33差异/0缺失。比前包新增的基线差异仅Lf2DatFile.cs与Lf2DatTokenizer.cs，均为事前声明脚本；其余沿用已存在工作。

先前场景HUDCamera/ScenesCamera/Canvas disabled在磁盘04:56:23 UTC保存后已恢复为HEAD的enabled状态，当前仅RectTransform y122.61→122.609985，SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f。来源仍未确认；本包无直接Scene编辑/保存或回退指令，不能由此推断这些变化的实际作者。scene-observed.diff及scene-baseline-observation.json保留观察。原异步确认仍待用户回答，不重复询问；独立parser工作继续，完整Play/资源迁移出口前必须解决。

## 下一唯一入口

NTSD28-Q05-NATIVE-FRAME-SOURCE-INTEGRATION-001，Task已准备，先准确code-path/Change Record，再核对native frame/FieldBag与实际转换入口。复用已写的CPoint27/OPoint24/ITR40/BDY模型；关闭六DAT九frame及WPoint9未知字段准入、补完整native投影。不要重做strength整表或数值helper。

内容身份、entity13/aggregate21/checksum24/character2/base2、carrier退休、+2F8、OPoint双队列capture guard和Play仍同Q05窗口待办；Q06算法与Q07正式资源后置。既有stage.dat暂缓、音频/图片例外、33ms/十一阶段、Unity/GAS和非战斗范围保持。没有提交或推送。
