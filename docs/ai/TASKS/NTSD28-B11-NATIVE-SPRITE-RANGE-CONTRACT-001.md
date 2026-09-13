# Q02-C：原生 sprite declared/effective 范围接入

状态VERIFIED_SOURCE_RANGE_ADMISSION_ONLY；Q02-B raw PNG已VERIFIED。父目标NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-02 / Q02；关联D-04/D-06/P-20、R17。

## 写前冻结的实现与所有权

只改三个脚本：DatParser/Runtime/Parsing/Lf2DatParserV2.cs；Animation/Manager/CharacterAnimtorManager.cs；Test/Editor/NTSD28B11NativeSpriteRangeEditorTests.cs（均在Assets/NTSD/Scripts下）。Parser可委派单文件worker，主代理负责manager/tests/集成；共享工作树不回退他人修改。新test.meta仅Unity导入生成。文档/输出限本Task/Record、Ledger/STATE/handoff/authority/总表及artifacts/diagnostics/NTSD28-B11-NATIVE-SPRITE-RANGE-CONTRACT-001，夹具Temp/NTSD28SpriteRange。

Parser保留既有Parse(text,path)语义，新增ParseLoganContent(text,path)并共享ParseCore；后者保留native reversed与signed end声明，不覆盖AST StartIndex/EndIndex。新source解析与CharacterData builder必须成对，不能用旧Parse丢失sheet后的AST伪造native内容。

CharacterAnimtorManager保持原私有BuildCharacterDataFromDat的唯一名字/签名，现有SelfCheck/RawCapture反射不可破坏。增加BuildCharacterDataFromSource(rawText,datPath,BattleContentSource)及共用stateless core；新入口自己按source选parser。把已有仅写characterData的四个Extract/Apply helper改为static以供同一core使用，不拆模块、不新manager、不实现Mono分层计划。图片列表投影集中在BuildSpriteFilesForSource，native从0按max(row*col,0)累计，旧源声明直传。新source的sheet/head/small路径复用BattleContentSource；旧无source入口保持原路径。所有正式caller仍默认旧源，完整catalog/cache事务另包。

Lf2SpriteFileDef/SpriteFileInfo字段形状不变；declared留在AST、有效范围写现有内容字段，不进入runtime/snapshot/checksum/shell。静态审计确认本范围不触及D-022，维持12/20/23。溢出的异常输入明确失败，不借此定义C++未定义溢出行为。

测试先反射检查新入口/投影缺失形成RED，再验证declared保留、native累计、legacy保持、零容量/反向声明、现有真实SpriteCatalog的pic→sheet/rect。实际读取正式405 DAT，并对照Q01 native捕获的sheet投影（验证该native输入/source哈希不变），不能只用自写参考算法。真实Unity只创建自有Texture/Sprite并销毁，不创建/销毁manager或改全局publication，以避免manager.OnDestroy重置central系统。

## 已确认前置与首差

Q02-A-AUDIT.md及Q01 same-input-sprite-layout-differences.csv记录31文件55项差异。原生dat_parser.cpp:192-220保留声明端点、从0按row×col累计effective范围；render_snapshot.cpp:1104-1147据此取sheet。Unity ParserV2.TryParseSpriteFileRange拒绝reversed范围，BuildCharacterDataFromDat将文本StartIndex/EndIndex直接送进全部sprite/catalog消费方；实际代码没有后续统一修正。

其中25文件按现有代码推导的最终范围集合仍不同，19文件authored pic直接落入缺失集合；6文件只是容量裁剪后集合偶然一致，不证明sheet/rect/像素一致。正式PNG实际发布尚未发生，不能称这些推导为Play证据。

## 必须追踪后冻结的最小范围

- DatParser/Runtime/Models/Lf2SpriteFileDef.cs与Parsing/Lf2DatParserV2.cs：保留原文declared first/last，包括合法原生reversed声明，另有明确native effective语义；不能覆盖后丢掉原始证据。
- Animation/Manager/CharacterAnimtorManager.cs的BuildCharacterDataFromDat、BuildIndexedSpriteRects、BuildFirstDeclaredSpriteOwnership、BuildBattleSpriteCatalog、GetStartFrame：优先在数据接入处统一effective范围，复用后续consumer，禁止多处各算一套。
- SpriteFileInfo真实声明及实际serializer/hash读取者：写代码前定位准确路径与是否进入内容/shell协议；若触及D-022联合窗口字段，先按Q03合同安排，不提前升runtime版本。
- BattleSpriteOverlappingRangeEditorTests与SelfCheck.CheckSpriteFileRangeParsingContracts保留旧内容/Editor夹具含义；新内容采用明确来源/解析语义，不能未经审计把非战斗Editor预览或旧基线行为全局改掉。

当前新增BattleContentSource只是纯路径合同，不足以决定整个旧/新parser source策略。开始前从本Task继续源码审计并另建准确Change Record，不能直接机械修改全局StartIndex。

## 最低测试与出口

test-first覆盖原生已有file(20-19)、file(900-900)夹具，普通多sheet累计、文本重叠、反向声明、row×col容量及实际pic→sheet/rect映射；至少包括正式c/jug/jugo.dat、c/jug/jugoCS2.dat、c/shis/shis.dat、a/tra/tra.dat等来源片段。构建实际Unity SpriteCatalog消费链验证，不只比AST两个整数。Q02-B已提供PNG解码后，可用隔离资源验证，仍不部署正式DAT/PNG。

单独P-21/NTSD28-B11-PNG-SHEET-ALPHA-CONTRACT-001是原始PNG解码后的半透明损失，不混入本range Change；两包都有准确后继且在Q07内容可用前解决对应出口。catalog.csv/registry_index与source/cache/publication完整接线仍需要独立包，禁止以helper存在关闭Q02。

## 最终出口（覆盖上述写前计划状态）

真实Unity14/14，405DAT/773sheet与原生捕获一致；CS0/Scene dirtyfalse，schema12/20/23保持。Record与Q02-C-REPORT.md已归档；精确入口子包交付，Q02未交付。下一PNG-SHEET-ALPHA-CONTRACT-001，然后catalog/source/cache/publication；正式资源仍未迁移。原文“开始前审计/另建Record”等为已完成的写前约束，不是下一任务。
