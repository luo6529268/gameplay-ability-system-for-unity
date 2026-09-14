<!-- CHANGE-RECORD
id: NTSD28-Q06-WEAPON-PIECE-SOURCE-WITNESS-001
status: VERIFIED
change-kind: ORIGINAL_WEAPON_PIECE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/weapon_piece_witness.cpp
authority: Formal playable BattleWorld28 materialize_weapon_piece_fragments and source catalog/initialization/random/lifecycle.
evidence: Exact native two-stage source read; current Unity OPoint percentage inheritance differs from generic fragment spawn.
-->

# 武器碎片原函数见证

当前状态：VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY。下方 IN_PROGRESS 为实施前记录。构建成功；synthetic.jsonl 为157例/762次成功出生，formal.jsonl 为3例/50次出生，两次执行输出逐字节相同。实际151=15+7、150=13+7、124=3+5；同步随机调用分别136/126/58，CRT均不消费。无槽双variant仍消费两次选择随机；碎片不应用ohp/omp百分比。完整结果及身份见同名 artifacts/diagnostics 目录的 validation.json、build-manifest.json 和 REPORT.md。Unity碎片生成尚未实施，完整SelfCheck仍保留失败，完整driver当tick参与尚未验证。

IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY。唯一新增脚本Tools/NTSD28AuthorityTrace/weapon_piece_witness.cpp；复用原catalog.upsert_definition/DatParser/BattleWorld.materialize_weapon_piece_fragments及resolve_pending_lifecycle，不复制expected算法，捕获wrapper原随机调用和全部生成实体。

覆盖14个内置/非内置OID、多seed、type/HP资格、pending/negative-link/facing、DAT单/多variant、目标定义缺失、完全/部分无槽、metadata ohp/omp与max_mp、实际Logan三weapon_piece定义。记录source结果、两阶段计数、source生命周期、每片raw50+sound/opoint latch、源RNG前后及变更调用列表。立即出生与lifecycle端点为本witness范围，不伪称完整driver高低slot当tick已测，后续Unity生产必须另验。

输出只在repo Temp/artifacts，正式EXE/75源码-header哈希由既有构建器核验；不修改Unity或资源，不晋升候选EXE。验收构建/矩阵完成、重复stdout字节相同、重要分支数据核对、实际catalog与构建身份及ledger。失败保留，不能用旧C#替代source。回滚须批准，仅本新增脚本差量。父WEAPON-PIECE-TRANSACTION和FRAME-TRANSACTION仍IN_PROGRESS，禁止computer-use。
