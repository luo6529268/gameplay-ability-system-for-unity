# Q09/P-12 香燐 Legacy 本体相机像素定向验收

状态：`VERIFIED_SCOPED_LEGACY_CAMERA_GPU_PIXELS`。仅证明所选原Battle香燐OID314在LegacyOnly路径向真实World相机的offscreen GPU渲染贡献目标像素；不证明正式根EXE同视口画面或P-12/Q09整体完成。

原项目唯一交互Editor PID11944，原保存且clean的`NTSD_Battle.unity`。已有通用pool补件包先证明自然OID314有真实body SpriteRenderer、向右、与同输入中央命令X/Y差0。本次仅扩原有可选Editor探针（默认与旧请求不触发），请求前字节`request-before-gpu.json` SHA-256 `A0DBF231FDD512F71C4C59971C7F4FC759D800B283A816D525D3CF77FC7C12F2`，提交请求`request-gpu-submitted.json` SHA-256 `9EC65D944D37CC0397E8BD923913463BB2EF5887AE89F13B7B1DBC5A7AE2B497`。

生成`Assembly-CSharp-Editor.csproj --no-restore`编译exit0、197 warnings、0 errors，见`generated-editor-build.log`；原Editor经MCP `refresh_unity`重编测试程序集（时间晚于脚本），在idle/非Play/Battle Scene clean后只运行该请求。原始结果[`karin-x500-state9997-legacy-gpu-04.json`](../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-x500-state9997-legacy-gpu-04.json) SHA-256 `C11DD0EC8822EB60CEC7381C6FCDE7439BEF5FE1323E3E471443008A88CCA541`：正式LoganRuntime、项目etc-mode1和同指纹；LegacyOnly/内存GameConfig副本、非logic-only、非worker，完整Driver第3步/tick8自然OID314/action50/state9997/owner8，逻辑向左、body显示向右、SpriteRenderer启用。相机两次Render之间World tick保持8，唯一刻意改变的是该body组件`enabled`，随后立即恢复。

两个1280×720 PNG：[body on](karin-x500-state9997-legacy-gpu-04-body-on.png) SHA-256 `ACF3F331D381605D7E269385ADBC77D34B286D8F1FA19F44CF5EECEFEB1C8D8C`；[body off](karin-x500-state9997-legacy-gpu-04-body-off.png) SHA-256 `973917DA6B493EBD8D76ADF7D977CB52CAA9B8E1C754415612A307D82ACDF81E`。探针报告1,245个不同RGBA像素，Unity左下原点边界X459–514/Y187–255。独立从PNG逐像素重算`independent-pixel-analysis.json`得同样1,245与完全同界（PNG左上原点Y464–532）；`exactMatch=true`。轮廓差分只证这项本体的目标像素贡献；单通道最大差27、平均最大通道差约7.99，肉眼合成图中的本体较淡，原因及其与正式版颜色/透明度是否一致尚未审定。`diagnostic-body-diff-highlight.png`和两个放大裁切仅便于目视定位，不参与数值验收。

后续只读核对已说明素材本身为何偏淡：正式`c/kar/a/cha.dat`帧50为`pic:60`，来自`cha4.png`首个79×79单元；正式`resources/runtime/vfs/c/kar/a/cha4.png`与Unity所选LoganRuntime文件SHA-256同为`5BBD5C1AF4061F891FE6CAE1432498E2A7742908DB54CD5BBF5D650B58799654`。首单元正式图非零alpha像素1,433/6,241，alpha最大仅33/255、非零平均约10.30/255，见`formal-source-alpha-audit.json`。这证明低透明度存在于正式素材，不证明正式D3D11与Unity最终混色逐像素相同；上段“原因尚未审定”在此仅对最终混色与可见强度仍成立。

探针确认相机`targetTexture`、`RenderTexture.active`与body启用状态均在`finally`恢复；子体/夹具释放，对象/槽/池借用各4/2/2→4/2/2，pause恢复。MCP复查Editor idle/非Play、Battle Scene `isDirty=false`。Battle Scene SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、GameConfig Asset `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`均未变。

范围：offscreen真实World相机渲染不是完整双相机Game View，也没有正式根EXE同世界/同视口GPU图片。香燐以外、Legacy无owner OID998、其它mode/owner负例和P-12/Q09汇总验收仍开放；Q07/D-024碰撞域选择另行待用户决定。没有改生产、DAT、图片、Scene、Asset、相机序列化、碰撞或非战斗脚本。
