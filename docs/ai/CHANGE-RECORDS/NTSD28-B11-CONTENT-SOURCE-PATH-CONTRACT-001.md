<!-- CHANGE-RECORD
id: NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001
status: VERIFIED
change-kind: BATTLE_CONTENT_SOURCE_FOUNDATION
code-path: Assets/NTSD/Scripts/Animation/BattleContentSource.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11ContentSourceEditorTests.cs
code-path: Tools/NTSD28ContentSourceTests/Program.cs
authority: User active total alignment goal and D-023; GameSession28 roots; ObjectDefinitionCatalog28 catalog/decoded_source_path; render_snapshot native_sprite_path; formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: VERIFIED_PURE_PATH_ONLY / TASK_BEFORE_CODE / RED_0_OF_20 / MONO_GREEN_24_OF_24 / UNITY_EDITMODE_24_OF_24 / COMPILER_ERRORS_0 / EXISTING_3059_HASH_UNCHANGED / SCENE_DIRTY_FALSE / LEDGER_463_9_PASS / NO_GLOBAL_SOURCE_SWITCH / PNG_AND_CONSUMERS_PENDING
-->

# NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001

准确范围、风险、测试及回滚见同ID Task。Unity原状：现有入口硬编码Config且各图片caller使用不同base；正式runtime的catalog/DAT/VFS根没有一个显式不可变合同。改后只增加由调用者携带的source路径定义，不接管现有cache、加载、Mono生命周期或非战斗caller。

实际符号预期：BattleContentSource.ForLoganRuntime/ForUnityProject、ResolveDatPath、ResolveImagePath、CatalogPath/DataIndexPath/DatRoot/ImageRoot/ProjectRoot。测试使用相同NUnit文件，由Unity Test Runner或源链接进程执行；本工具没有Unity行为stub，测试只涉及纯System路径合同。

副作用：只创建fixture/输出、新增源码与文档；source不持有worker/queue/renderer/World/pool，因此无新增shutdown owner或阶段调整。现有parser/loader/Sprite publication/UI缓存保持；consumer接线必须后继独立包并验证完整source一致性。

## 过程验证

### 最终出口（2026-09-13）

纯路径合同已VERIFIED，**不代表实际目录读取、PNG解码、生产source切换、cache/publication或Q02整组完成**。实际修改为本Record三脚本、工具csproj/.gitignore、Unity自动生成的新脚本两meta与治理/报告；已有production文件无差异。

1. 初次net10执行Unity自带net35 NUnit失败于Remoting CallContext不可用，20项环境失败，不能作为行为RED。记录red.txt。工具改为net472并使用现有Unity bundled Mono运行，不安装依赖。
2. 实际可用RED：dotnet build ContentSourceTests.csproj 0warning/0error；Unity Mono运行同一Editor NUnit fixture，生产类未实现时0/20，断言确认为缺少source class（red-mono.txt）。
3. 实现后源链接20/20，代码review后补来源类型IsLoganRuntime、ImageRoot允许域说明，以及旧Unity相对图片父目录/UNC/内部归一化测试；最终net472编译0warning/0error、源链接24/24（build-green.txt/green-mono.txt）。
4. Unity CLI status无Pipeline实例；pipeline list与实际PID49020确认目标2022.3.62f3编辑器确实存在，无第二实例启动。复用已有MCP TCP bridge，get_editor_state确认非Play/idle、NTSD_Battle dirtyfalse；refresh_unity实际导入/编译新source与tests。execute_code的Roslyn缺失与CodeDom命令行过长是工具诊断失败，不是源码编译错误，未修第三方或安装包。
5. 实际Unity TestRunner：初轮job fb0f2d0235a34c6184fefc419674025c完成20/20；review后最终job 6dee6bb30c5a4cfca4dc0b1b760dc5be **24/24、0fail/0skip**（unity-test-result-final.json）。进度total4902是发现总数，最终summary.total24才是本次过滤执行数量，不能报告全项目4902测试通过。read_console filter error CS返回0；domain已刷新且新source测试通过。
6. 场景前后NTSD_Battle、rootCount14、dirtyfalse；3059个既有保护文件hash完全不变；Ledger463records/9governedfiles PASS（含前轮Q01六脚本），diff检查PASS。

### 审阅处置及边界

来源公开标记IsLoganRuntime，不由caller猜nullable root；Unity ImageRoot是project允许域，实际DAT相对图片必须调用ResolveImagePath，允许项目内跨兄弟目录并拒绝项目外路径。ForLoganRuntime明确只覆盖当前正式unified portable布局，不为原生其他双root/debug布局发证。路径检查是词法边界，不认证symlink或文件存在；内容fingerprint/catalog validation由后继真实读取包负责，不填假值。

source不持有任何Unity对象，不接入Mono/World/shutdown，不改UI/menu/loader/BMP既有caller，因此本纯类没有需要Play复现的新战斗行为；未运行BattleRuntimeSelfCheck或整场Play，生产接线后仍必须按风险完成。测试反射定位+手动runner只支持当前纯fixture，不宣称通用NUnit运行器；真实Unity24项结果为独立验证。

下一NTSD28-B11-PNG-WORKER-DECODE-001（Q02-B）Task已准备；Q02-C range与catalog/config/cache/publication完整接线明确保留。R17尚未到整个Q02加载变化出口，不能以基础类存在关闭资源回访。
