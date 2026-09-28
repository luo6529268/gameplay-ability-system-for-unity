# Q09/P-02 自然持武器目标像素归因：限定验收

状态：`VERIFIED_SCOPED_CENTRAL_TARGET_PIXEL`。唯一原 Battle Scene 请求 `q09-held-attrib-20260928-a` 的原始 JSON 为 `PASS_SCOPED_PHYSICAL_CHAIN`；这是中央战斗命令层的目标 GPU 像素证据，**不是**完整 Q09/P-02、正式根 EXE 同视口或全画面合成验收。

原项目自然物理输入链：正式 OID120 于 tick6 被鸣人拾取，tick11 站立、tick18 空中、tick20 持有空攻动作30。发布的前一运动 tick19→当前20。World 相机两次 `beginCameraRendering` 回调均执行；实际显示 alpha **0.2048696970→0.7049303036**，同一武器命令 X **-10.0540142059→-9.8850116730**。两次相机提交有效，终态目标诊断 `Submitted=true / Reason=None`。

每个相位都从该相位已冻结的生产中央命令顺序制作临时全量与仅去武器命令两个帧，使用当前生产 catalog、材质、draw mode 和 `BattleDynamicMeshBackend` 及与 `BattleRenderFeature` 一致的 segment 绘制。全量5命令解析5；去目标4命令解析4。各自按相同 World 相机矩阵离屏绘制并保存 PNG，不修改生产帧。两图大于2 RGBA 通道差的目标贡献像素：早相位 **11** 个、晚相位 **6** 个；两个贡献掩码的对称差 **17** 个。独立用 Pillow 从四张 PNG 重新计算得到完全相同的 11/6/17。

视觉 QA 发现离屏 CommandBuffer PNG 相对真实 World 相机 PNG 是 **上下翻转** 的。原离屏图按其自身自下而上坐标记录的目标边界为早 x84..88/y227..233、晚 x96..101/y228..233；竖直翻转到真实相机坐标后分别为早 x84..88/y306..312、晚 x96..101/y306..311，均落在该相位武器投影框内。更关键的是，翻转后的全命令离屏图在这 **11/11、6/6** 个目标差分像素处，RGBA 值均与同相位真实 World 相机 PNG **逐点完全相同**；去武器命令图在这些点不同。其余中央非白像素与完整相机图逐点相同1135/1142、1128/1135，不能把整个离屏层冒充全合成画面。该配对证据证明这次目标命令贡献确实出现在真实相机可见像素中，并随展示相位移动；不证明正式根 EXE 同视口或其它屏幕层的总体验收。旧“投影独占面积0所以无法归属”的结论仅限旧全图差分方法，已被本次目标命令有/无差分缩小。

两个相位 World parity checksum 与相机前后均为 `e54a96d35593761bc585fd9bf8418b1b0507a20e78b75dded3dd080ebec2bf0b`。原始报告记录 `stopped=true`、`worldDetached=true`、`exitedPlay=true`、`sceneCleanAfter=true`、World对象/槽/池借用0、焦点策略恢复；原 Editor PID11944 回到 `NTSD_Battle` idle、非 Play、非编译。Battle/Menu Scene、InputSystem 设置、项目模式 Asset 四个 SHA 与运行前一致，具体值见 Change Record。仅改可选 Editor 探针；生产、DAT数值、角色图片、Scene、模式 Asset 和非战斗逻辑未改。

剩余出口：完整自然合成画面与正式根 EXE 同视口表现、30/60/120 速率覆盖及其它 Q09/P-02 项仍须按总表验收。Q07/D-024 碰撞域选择、Q08、BATCH-04/05 和总目标保持开放。旧 `q09-held-unsat-20260928-a/-b` 原始 FAIL 不改写或冒充本包 PASS。

最终审计：生成 Editor 工程编译0错误/191警告；`Tools/Validate-ChangeLedger.ps1` PASSED（949 Records、17个当前差异代码文件），`git -c core.safecrlf=false diff --check` 退出0。未运行无关的全角色/全场景测试。
