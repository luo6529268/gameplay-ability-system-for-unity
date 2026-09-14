VERIFIED / SOURCE_MODEL_DIAGNOSTIC_ONLY；157+3例输出与复跑字节一致，详见同名Record/REPORT。以下为事前范围。

# 武器碎片原函数见证

IN_PROGRESS / SOURCE_MODEL_DIAGNOSTIC_ONLY。唯一新增脚本Tools/NTSD28AuthorityTrace/weapon_piece_witness.cpp；复用原catalog.upsert_definition/DatParser/BattleWorld.materialize_weapon_piece_fragments及resolve_pending_lifecycle，不复制expected算法，捕获wrapper原随机调用和全部生成实体。

覆盖14个内置/非内置OID、多seed、type/HP资格、pending/negative-link/facing、DAT单/多variant、目标定义缺失、完全/部分无槽、metadata ohp/omp与max_mp、实际Logan三weapon_piece定义。记录source结果、两阶段计数、source生命周期、每片raw50+sound/opoint latch、源RNG前后及变更调用列表。立即出生与lifecycle端点为本witness范围，不伪称完整driver高低slot当tick已测，后续Unity生产必须另验。

输出只在repo Temp/artifacts，正式EXE/75源码-header哈希由既有构建器核验；不修改Unity或资源，不晋升候选EXE。验收构建/矩阵完成、重复stdout字节相同、重要分支数据核对、实际catalog与构建身份及ledger。失败保留，不能用旧C#替代source。回滚须批准，仅本新增脚本差量。父WEAPON-PIECE-TRANSACTION和FRAME-TRANSACTION仍IN_PROGRESS，禁止computer-use。
