# Q07 / D-024 连续直接位移最终限定验收（2026-10-03）

本报告前向更正 [首版报告](REPORT.md) 的“Z400边界后物理残差待查”状态。首版的正式源/根、Unity初始RED、共同可走区证据和失败原件全部保留；新增证据证实边界后画面残差仍是同一个共用帧尾出口的问题，并已修复。状态为 `VERIFIED_SCOPED_DIRECT_MOTION`；Q07、D-024全实体、Q09实际像素、Q12整体验收继续开放。

## 修复依据与最终实现

正式 OID92/action580每tick从规则整数 Z 基底写`dz:4`。项目自有地图在初始Z400的tick21把规则精确Z限制到481.59722222222223，整数Z为481；下一tick源帧运动从整数481重新计算精确值。首轮共用物理位置精确累加原始`dz:4 × 1152/730`，未扣掉源域小数重基差，导致边界后4tick累计2.8273972602742106输出像素误差。项目地图与正式背景1的上界不同是用户允许的规则场景差异；画面与**项目当前规则源位置**不同步则是独立的D-024比例缺口。[改前/改后边界配对](root-unity-scene-comparison-04-boundary-after-fix.json)。

共用 `LF2Entity.ApplyNativeFrameMotionTail` 现在仅在源规则位置已初始化时，先照正式整数基底算出新精确源X/Z，再将“新源精确值－旧源精确值”通过World共用比例投影到物理X/Z；未初始化源域保留旧物理整数基底。它同时覆盖横向左右、纵深、重复帧和源小数前态，不含角色/OID特判。规则源字段、速度清零、Y、帧/平台pass、DAT、地图、相机、Scene、资源与非战斗逻辑未改。已有故意把物理/源锚点分开的单次夹具按实际源差更新物理预期，没有把规则预期改成画面值。

## 新鲜验证

- 旧生产写法下新增“小数源前态”左右两例在原Editor第一tick均RED，画面X多0.91756272401426像素。[RED原件](unity-fractional-red-result.json)。最后生成Editor项目编译0错误/306警告，原Editor完整帧运动类35/35 PASS，包含两例小数X/Z、正式OID92连续Z、左右X、平台叠加和历史原生夹具。[GREEN原件](unity-fractional-green-class-result-02.json)。
- 最终生产改动后，初始Z380共同可走区的正式源/根初始+24tick选定动作/整数Z/精确Z仍75/75同；原Battle Scene两角色初始+24tick×八字段400/400同根，最大物理投影误差约`1.1368683772161603e-13`输出像素，探针`CAPTURED/DONE`。[最终共同区配对](root-unity-scene-comparison-05-final.json)。根正式EXE SHA仍为`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；其报告`nativeParityClaim=false`，本处只主张明确比对的字段。
- 最终生产改动后初始Z400原Scene仍在tick21与正式背景1规则上界分叉；前0～20tick两槽八字段336/336同，之后八个规则Z字段不同。Unity物理Z在tick21到达760并于tick22～24保持760，24tick最大相对Unity规则源Z的投影误差约`1.1368683772161603e-13`像素。探针原始`MISMATCH/DONE`状态是把两种地图的规则Z强行比较，`error`文案是旧泛化标签；不能据此说画面投影失败。[最终边界逐字段与逐tick样本](root-unity-scene-comparison-04-boundary-after-fix.json)。
- 两次最终原Scene探针都完成有序关闭：World对象、运行槽、池借用、活动池对象与Sprite均0，回到干净Menu且Battle/Menu/GameConfig/Mode四保护文件SHA稳定。原Editor完整 `BattleRuntimeSelfCheck` 于17:52:37写出新`PASS`；MCP菜单回执超时，但结果文件修改时间晚于调用并已另存[结果](selfcheck-result-after-fractional-v2.txt)。覆盖前的临时结果另存[副本](selfcheck-result-before-fractional-v2.txt)，未删项目文件。

## 未关闭与操作状态

原Editor在最后一次SelfCheck后，Unity MCP场景查询一度超时且进程曾显示`Responding=false`；后续18:00只读查询已恢复，仍为未变脏的Menu Scene，但根对象从先前的8个变为9个。[只读层级](menu-after-selfcheck-hierarchy.json)识别新增内存根对象为`BoundaryWallManager_AutoCreated`；该对象出现于自检后，写入者/直接触发链未证，且Menu磁盘文件与四保护SHA不变。保留现场，不擅自删除或把它归为本包战斗World/池残留。两个原Scene探针自身的有序关闭零残留结论不变；若后续要处理这个Editor态对象，应另立任务核自检/单例生命周期。正式根EXE实际Present/GPU、其它实体位移入口、玩家物理按键及Q07/Q09/Q12总出口仍待。用户保留项目地图，不能以原版背景部署消除边界规则差异。
