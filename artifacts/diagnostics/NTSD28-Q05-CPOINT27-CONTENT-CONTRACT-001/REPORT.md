# Q05-A2 CPoint27内容契约进度报告

状态：FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / SOURCE_INTEGRATION_PENDING。不是完整Q05出口；本Change保持未关闭，待新来源接线及联合身份/Play验收回访。

## 实际改动

CatchPoint DTO及BattleCatchPointValue新增faction/baction/uzaction/dzaction/z/recover/drain/gain八个int，throwvx/vy/vz变float32。19参数旧构造调用仍可使用，新字段默认0；immutable value、adapter完整复制27项。Equals/hash对float采用同一raw-bit规则，+0/-0可区分。BattleCatchPointCatalog改为count+每项27个固定32bit单元，三个速度输出bit，不把小数取整。最终little-endian落盘及外层decode身份仍归Q05后继。

新增LoganCombatRecordDecoder.CatchPoint使用实际原版27字段映射，exact-case最后字段生效；strict int/finite float复用Q05-A1。hurt动作独立于injury/cover；未知字段不进入typed payload，原AST完全保留。DTO raw字典只保存实际识别的字段。

旧共享Lf2DatConverter显式调用ValidateLegacyPropertyName，保留19字段准入及原有alias，避免新DTO不完整接线时静默吞drain。该旧路径不是目标Logan行为，必须在A2最终来源接线时回访；本包没有切CharacterAnimtorManager或发布候选内容。新增字段的战斗producer留Q06。

实际生产throw reader BattleCpointWriter及LF2Entity本来就把速度写入double runtime；此次类型修正可直接提升float32，无需修改其算法。新增测试复用真实ThrowScope/World/角色链，覆盖right/left、1.5/-2.25/0.125和微小速度，断言最终Runtime Vx/Vy/Vz精确等于float32提升后的double。未新增Play Mode或GPU证据，完整发布验收仍待。

## 真实证据

- RED job6e9be7abbfe940b4b322dfaf8eed355f：43项42失败/1通过。缺失decoder、旧int类型、旧19项buffer准入均复现；原始RED.xml保留。
- GREEN job98077f38667547ab8ef83dc14cdb9417：97/97（CPoint新43、Q05数值43、实际throw11）。最初另两个旧类用了错误namespace，实际没有进入这97项，不能把它们计算在该次结果中。
- 更正精确namespace为NTSD.Test后，job3ab490daf0ab42ec9f0547d51a6e6c32：既有CPoint catalog/alias共13/13。总计110项实际通过。原行为测试保留，仅更新19→27 canonical容量/下标断言。
- 原版numeric37用于逐项DTO→immutable value→canonical float32 bits比对；27字段固定顺序、新8字段参与identity、负零、防御复制、unknown/case/重复key与旧入口拒绝新增字段均有定向检查。
- dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj -v quiet：0warning/0error；当前源码链接的内容工具可构建。
- Unity旧138 DAT：parse138/138、Converter138/138，0失败。与Q01旧投影比较，710处变化全部为CPoint throwvz的原整数-842150451转float32。实际位模式CE48C8C9，提升double的精确值是-842150464；JSON最短往返字符串写作-842150460，两者不能混淆。没有其他frame字段变化，其他比较section无差异。raw资源未改。见legacy-rounding-classification.json及完整差异。
- 旧内容工具最初拒绝本包artifact输出路径，随后使用其允许的Temp/NTSD28ContentAudit/unity/q05-cpoint27-legacy.jsonl；未改工具安全范围。当前工具仍投影旧19项CPoint，这是历史诊断形状，不作为新27项完整证书；A2最终投影更新必须回访。
- 完整BattleRuntimeSelfCheck：02:42:43.402682 UTC新请求，最终文件比请求新且PASS。前次结果保留并标NOT-CURRENT。
- 最终CS0，NTSD_Battle isDirty=false/root14。源载体/Scene/Prefab/DAT/图片/框架未作额外修改；3059保护文件3037不变/22有声明变化/0缺失，新增8个基线脚本差异均属本包，前14来自Q02/Q04。

## 未完成与下一步

当前CPoint内容ABI已在工作树变为27项，但外层解码身份和snapshot版本尚未统一；这是Q05协调窗口内的中间状态，不可发布或宣布全域已对齐。Entity12/aggregate20/checksum23/character1/base1未升级，正式DAT/图片未部署。

下一Task NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001：确认24项immutable value→ObjectPoint→单/多task完整复制和reset，不提前改materializer规则；随后BDY/ITR/strength、新来源转换入口、投影与identity必须同窗口闭合。CPoint本Change在生产接线/完整Q05验收时再回访关闭。旧phase断言留Q12、landing除法一ULP留Q06、stage.dat暂缓保持。

最终审计：Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path实际PASS，476 records /66 governed code files。Authority再核72输入无漂移、正式EXE SHA匹配；artifact为authority-final.json。十脚本diff已人工复核，无算法顺序/外部包/非战斗模块额外修改；新增native decoder/测试meta已由现有Editor生成。
