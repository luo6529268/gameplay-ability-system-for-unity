# data.txt 角色 Item 生成结果

状态：VERIFIED（限定角色名单与菜单 Item 生成），2026-10-06。

Controller管理本项目槽位、选择阶段、弹窗、倒计时和MatchConfig；Board使用现有CharacterChoiceItem模板与MMMiniObjectPooler，点击仍交给当前SelectRoleItem。原名单来自全部已加载ID再按头像筛选，无法正确表示data.txt角色注册表。现在复用GameDataManager的已发布注册顺序，读取角色DAT元数据hidden，只生成默认可见type0，首项固定随机-1，不按ID排序，不因缺图剔除。默认隐藏门为0；不增加原生解锁系统或改本项目随机最终角色规则。

当前生产数据入口：Assets/NTSD/Content/LoganRuntime/decoded_dat/data/data.txt，由现有catalog/资源发布链读取；旧Assets/NTSD/Config/data.txt不被UI另起加载器读取。实际默认可见50角色，加随机共51项。预热完成/玩家加入/重新打开刷新，有效的具体选中ID在刷新中保留。Board普通小图优先SmallSprite、缺省HeadSprite，随机沿用现有模板图/缺省GameConfig.RandomIcon。没有新增Inspector绑定字段。

改动：Assets/NTSD/Scripts/UI/{CharacterSelectionController,CharacterSelectionBoard,SelectRoleItem}.cs；新增Assets/NTSD/Scripts/Test/Editor/CharacterSelectionBoardEditorTests.cs及Unity生成.meta。Scene/Prefab/DAT/InputActions/GameConfig未编辑。

验证：

- dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly，最后compile-04.txt：exit0，301warning / 0error，6.22秒。
- 原Unity Editor定向EditMode测试三项：默认类型/hidden过滤/注册顺序/无头像仍入名单；当前data.txt与catalog默认可见名单一致；真实NTSD_Menu生成/绑定/点击确认取消/重开实例复用。最终每项全部断言后输出passed-*.json，共三项直接通过证据。
- 原Menu Play从现有Loading面板完成预热后生成51项；加入时随机，点击鸣人后确认/取消保持项目状态机。重开51个实例ID及顺序与关闭前相同，默认恢复随机。实际Game View图：character-items-20261006-083841064.png。
- Menu/Battle Scene及GameConfig SHA不变；原Editor最终非Play/idle/无测试/无编译，Menu root8 / isDirty=false。所有前镜像SHA仍相同。详postchange-manifest.json / editor-final.json / scene-final.json。
- Tools/Validate-ChangeLedger.ps1与git diff --check最终结果各自保存，不清理仓库已有Shadow task工作。

失败/限制保留：初次测试漏MenuLoopCarousel命名空间已修；首轮未打开Loading导致等待预热超时，代码/场景未因此改变。后两轮桥在Play重载后丢回调，job的running不是验收结果；使用全部断言后的独立passed输出，确认Editor已无测试/非Play后只清理桥孤立元数据。CodeDom命令行过长/Roslyn缺失的辅助诊断失败，不安装或修改MCP依赖。首轮与桥原件均保留。没有完整多人输入/倒计时开战或设备验收，也不称Native整个选择界面/战斗权威对齐。

审计：docs/ai/CHANGE-RECORDS/NTSD-MENU-CHARACTER-DATA-ITEMS-001.md；docs/ai/TASKS/NTSD-MENU-CHARACTER-DATA-ITEMS-001.md；docs/ai/FILE-OPERATIONS/NTSD-MENU-CHARACTER-DATA-ITEMS-001-PREPARE/RECORD.md。逐文件备份与恢复边界见prechange-manifest.json。没有删除/移动、Git提交或push。
