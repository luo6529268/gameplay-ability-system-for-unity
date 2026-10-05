<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-PROBE-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HeldWeaponDualDomainEditorTests.cs
authority: User D-024 shared spatial projection and preserved BattleVisualScale 1.5; formal 336B44 BattleWorld28::settle_held_refill_objects WPoint alignment and render_snapshot source position
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001.md
-->

# D-024 持有武器显示挂点：两 tick 定向诊断

2026-10-05 最新：生产共用补偿修复后原Editor精确同两例job `c4e6daee940d483bbc32735e7242d0ce` 2/2 PASS（46.7945989秒），恒等挂点仍0/0，固定视野X6/Y4.04657→0/0；源/frame/view字段不变。生产Record独立RUNTIME_PENDING；本诊断仅证明当前正式自然拾取前2tick的生产几何，不是原Scene/GPU/其它朝向完整验收。[GREEN原件/对比/退出状态](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/REPORT.md)。前面的RED/生成编译事实保留；当前FOCUSED_TEST_PASS，不重新执行旧24tick或全套测试。

2026-10-05 原EditorRED证据：[报告与两个CSV](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/REPORT.md)。精确job23310520aeec408b9a11737b5f720400只完成2项，终态failed，配置视野挂点X断言actual6；完整汇总result=null，不写虚构pass计数。原identity几何差0/0、project6/4.04656982421875；源X/Z与当前对应源码CSV、frame115/24、reciprocal关系均先验通过，定位旧本地补偿不适配已投影显示差。前次25秒观察超时后重取同job终态，未重启。Editor退出/idle/Scene clean/root11/SHA稳，Console0error。当前COMPILE_PASS/RED已证，生产由独立Record登记再修。

2026-10-05 生成Editor编译：`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`退出0、301warnings/0errors（11.11秒）；本Record当前COMPILE_PASS。原Editor精确两参数例与实际几何读数待执行，不以编译判行为。

2026-10-05 脚本后即时登记：仅声明文件新增上述2参数方法、`RenderedWeaponPoint`专用助手与Animation/Linq/UnityEngine using；原24tick方法保持。复用正式fixture/生产临时Driver但只执行2tick；前置断言正式action115/24、sourceX/Z、关系成立后，调用共用生产pivot和held补偿、保留1.5倍本地尺寸，先独占创建唯一CSV保存字段/几何/差再断言WPoint接触。没有生产/资源/Scene改动，当前CODE_WRITTEN；生成/原Editor编译及RED证据待执行。

脚本修改前登记。既有原Editor24tick测试只检查source/view位置及运动采样，尚未比较本体图片上的双方WPoint。当前源坐标写者已将X/Z局部位置差按统一投影换算，源Y在新共用本体pivot出口投影；`LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels`仍调用以`visualScale-1`为补偿的旧函数。这是已有D-024消费者审计中持有挂点的具体公式疑点，不是已测的原Scene像素首差，不能先改生产或另开角色矩阵。

权威入口：当前正式336B44源码`battle_world.cpp::settle_held_refill_objects`约8094～8168行，在reciprocal关系中用双方正式frame center/WPoint令源挂点重合，cover仅改变Z/Y各一且二者屏幕和抵消；源码由playable build闭包消费。现有`native-336b44-24.csv`已提供当前对应源码自然OID2拾取OID120的tick2/action115/24/X201/214/Z542/543，同版本报告保留源探针并非正式根EXE证书的限制。

准确增量：仅现有`NTSD28Q07HeldWeaponDualDomainEditorTests.cs`新增参数化`NaturalPickupHeldWeapon_ViewWPointsCoincideAfterProjection(bool configuredView)`及本方法专属的挂点几何/独占CSV写入助手；原24tick方法不改。复用现有正式DAT加载、原场景无持久化的临时生产Driver及固定自然pickup输入；仅执行前2tick，在恒等与2048×1152视野检查当前两个正式frame、source X/Z与现有CSV同态，然后调用实际共用pivot/held补偿，计算图片上的持有者/武器WPoint屏幕差。当前PNG未参与加载，结果是生产几何出口而非GPU像素。原生产helper只读，不改DAT/图片/Scene/相机/输入或非战斗功能。

预期：源WPoint重合；两套视野及用户1.5倍显示尺寸下最终画面WPoint重合，检查前先将实际字段/两端frame/WPoint/pivot/offset/差值写入唯一UTC+GUID CSV，失败原件不覆盖。原EditorMCP精确testNames只运行此方法的两参数例，不全套。生成Editor0错、原Editor编译/结果、Scene clean/SHA保护、退出临时World零注册/零借用由既有scope验收。若配置视野出现非零画面差且同源输入成立，才为共用生产补偿建立独立Change；若前置/源规则失败则不归因显示。

风险：实际frame点/cover/整数取整改变结果；新样本不能覆盖缺WPoint、释放、其它实体关系、Legacy/GPU或全部武器。回滚仅手工移除此Record新增方法、专属助手与新增using，保留原测试和他人工作；不使用restore/reset/clean。无新增运行时模块，既有临时Driver关闭合同不变。当前PLANNED，尚未改脚本/运行。
