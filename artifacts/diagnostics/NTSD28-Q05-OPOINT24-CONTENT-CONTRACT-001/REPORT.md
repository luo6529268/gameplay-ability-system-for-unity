# Q05-A2 OPoint24值与任务复制进度

状态：FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / SOURCE_INTEGRATION_PENDING。保持未关闭，待A2来源接线和Q05/Q06联合消费、身份、Play回访；不是完整Q05交付。

## 已实现

BattleObjectPointValue从8项扩为原版24项int32，全部参与Equals/hash。保留8参数旧调用，新16项默认0。ObjectPoint原有dvz，现在补其余15项，DTO包含24项内容和独立legacy runtime objectId。Adapter双向复制24项，dvz不再丢失，objectId继续排除。

新增LoganCombatRecordDecoder.ObjectPoint，逐项复用strict numeric helper，exact-case最后字段生效，未知字段忽略且原AST不变。原版kind/x/y/z/action/dvx/dvy/dvz/oid/facing/hp/mp/team/reserve/effect/pic/centerx/centery/centerz/framea/attacking/join/join_reserve/join_pic均已覆盖。不把source_line或objectid晋升内容。

已有OPointCreateTask/OPointCreateMultipleTask.Clear采用整个opoint=default，复制点是struct整值赋值。本包未改task/factory/队列/初始化顺序。已实际调用BattleLogicObjectPointRuntime.CopyMultipleTaskToSingle证明24项到单任务不丢失，point.dvz=8与spread task.dvz=2.5分开；task.team=99也不与point.team混同。真实BattleLogicReferencePool回收并再次借出同一single/multi对象，全部25个DTO字段清零，重新装载能完整往返。

renderer两处inline赋值仅静态确认，未将其写成真实renderer materializer/Play新字段消费证书。hp/mp/team/reserve/random/方向Z/初始注册的完整规则接线仍Q06；不能因保存字段就宣称对应规则已对齐。

## 验证

- 正式EXE为B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；复核原source-linked构建40项source/header输入无漂移，诊断EXE SHA与manifest一致。证据authority-identities.json。
- Native诊断43个纯block DAT成功，得到45个24字段向量。43个warning均为夹具省略bmp；0 parser errors，不是生产全DAT认证。native.jsonl/native.tsv和fixtures保存真实输出与输入hash，Python只组织/提取结果，不定义数值期望。
- RED job413bc5fe58be43cc92ab4b516ba1e73c：49/49失败，缺少decoder/24字段，RED.xml保存。
- GREEN jobc72b548b28244103afea368a5293bc8d：98/98。OPoint新49（原版45逐字段+identity/实际copy/pool/AST4）、旧OPoint6、共享decoder的CPoint43。24字段Value/DTO分别核对native，含单字段、负数、溢出、最后key、大小写、未知字段和重复OPoint。
- 旧OPoint测试保留原8字段golden文本/digest作为历史兼容向量；只更正当前属性数量和dvz清零预期。旧共享Converter仍未切native，这6项不是新24项完整凭据。
- dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj -v quiet：0warning/0error。
- 当前Unity138 DAT全部parse/Converter成功；与上一轮CPoint27后的capture对照，所比较frames/top/bmp/blocks/stats/sprites及BMP信息无新增差异。当前工具仍是19 CPoint/8 OPoint投影，所以仅用于旧加载回归，完整27/24投影必须在A2最终接线时回访。
- 完整BattleRuntimeSelfCheck：03:02:42.955744 UTC新request，最终结果mtime晚于请求且PASS；前次结果另存NOT-CURRENT。
- 最终CS0，NTSD_Battle isDirty=false/root14。保护3059基线文件3034不变、25个声明差异、0缺失；比CPoint轮新增ObjectPoint Value/Adapter/旧Editor测试三个基线差异，其余共享文件仅追加本包限定段落。新测试及前批new脚本另由Ledger覆盖。

## 边界与下一项

本轮准确七脚本；没有修改factory/队列/对象池生产逻辑、Scene/Prefab、DAT/图片、Gen/Plugins或非战斗功能。没有提交/push/删除。人工复核constructor映射、全部hash/equality、DTO字段、adapter次序与default reset边界；没有为静态整值复制增加生产重构。

CPoint和OPoint新内容ABI已在工作树，外层source/decode identity与entity/aggregate/checksum/shell仍旧，当前属于Q05中间状态，禁止发布半迁移baseline。正式资源未部署；33ms和十一阶段保持。

下一Task NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001：BDY深度/几何有效性及ITR z/有效性完整copy/reset/诊断合同。随后weapon-strength19+index、完整native来源转换入口与27/24投影、身份/载体/快照版本/真实Play仍需同Q05窗口完成。本OPoint及CPoint Change保持SOURCE_INTEGRATION_PENDING，完成后统一回访关闭。旧phase断言留Q12、landing除法一ULP留Q06、stage.dat暂缓保持。

最终Ledger校验实际PASS：477 records /70 governed code files（ledger-final.txt）；准确七脚本及现有共享文件分段复核完成，未触及任务factory/队列生产代码。
