# Q09/P-12 Legacy body 预热补件定向验收

状态：`VERIFIED_SCOPED_LEGACY_PLAY`。仅关闭通用Legacy本体SpriteRenderer缺件及本次香燐OID314同输入位置/朝向门；P-12、Q09、Q07/D-024和总目标仍开放。

原项目唯一交互Unity Editor PID11944、原保存`NTSD_Battle.unity`。RED原件[`karin-x500-state9997-legacyboot-02.json`](../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-x500-state9997-legacyboot-02.json)：从场景前内存GameConfig副本选LegacyOnly，完整Driver第3步自然生OID314/action50/state9997/owner8，`LF2ObjectRenderer`存在但body `SpriteRenderer`为空，返回FAIL。所选`EntityObject.prefab`和`LF2ObjectPool`的fallback均未创建body组件。

修复仅在`LF2ObjectPool.CreateNewObject`用与生产World相同的`BattlePresentationBackendResolver.Resolve(GameConfig.Instance)`选择非CentralOnly模式，并在对象预热时为缺失的`EntityModel`补一个`SpriteRenderer`。`Get`在容量seal后不能扩池；此组件不会在运行tick中创建。默认GameConfig Asset仍为CentralOnly，条件分支不会在默认中央预热中新增body组件；未另做中央Play回归，中央行为由现有同源命令原件及源码条件保护。没有改Prefab、DAT、Scene、Asset、相机、碰撞、模拟或非战斗。

生成`Assembly-CSharp-Editor.csproj --no-restore`编译exit0，227 warnings、0 errors，见`generated-editor-build.log`。原Editor通过MCP `refresh_unity`导入新Runtime程序集，域重载后经MCP确认idle且原Battle Scene clean。请求前后原字节存`request-before-postfix.json` SHA-256 `AFE23C87667F80EAD3B248C0AC5B17744F0A32678A58340A584656E514C1F038`、`request-postfix-submitted.json` SHA-256 `76A409C1FC211FF88334EBFBA73F21C013396779ADAD93922A171C1686AB6C8F`；没有覆盖旧请求原件。

GREEN原件[`karin-x500-state9997-legacybody-postfix-03.json`](../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-x500-state9997-legacybody-postfix-03.json) SHA-256 `18DCB79DBFD7C1B922E36BC5727ADE16F6025492A5F313BDAE56B74AE35139A0`：`LegacyOnly`、内存配置副本、非logic-only、非dedicated worker；同正式内容/项目etc-mode1/sourceX500，完整Driver第3步自然生成OID314/action50/state9997/owner8，逻辑dir左，真实body SpriteRenderer `enabled=true`、`flipX=false`。Legacy本体根世界坐标`(-4.309965133666992,-7.570000648498535)`，与此前同输入中央命令原件[`karin-x500-state9997-body-postfix-04.json`](../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-x500-state9997-body-postfix-04.json)的FlipX=false及X/Y逐值相同，差值0/0 Unity单位。两个run内容/模式指纹和角色/子体身份一致；它们是两次同输入原Battle Play，不是同一帧双backend GPU截图。

探针回收后对象/槽/池借用各4/2/2→4/2/2，子体与夹具释放、pause恢复。MCP再读Editor idle、非Play、active Battle Scene `isDirty=false`；Battle SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、GameConfig Asset `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`均与前态相同。

范围限制：尚未取Legacy真实Game View像素，也未取正式根EXE同视口截图；未覆盖全部Legacy实体或P-12无owner分支的Legacy像素。Q07 D-024统一碰撞域选择独立待用户决定。这里只把通用缺件和香燐自然可见本体/朝向/位置门记为通过。
