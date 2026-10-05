# 独立只读审阅

审阅者：/root/d024_noncharacter_remainder。仅阅读 diff、正式源码、新测试及 native v2 结果；未运行 Unity、测试或构建，未修改文件。

结论：未发现阻断；审阅时 GREEN 终态仍待，不能据审阅提前报告运行通过。

- 生产相对 before 仅修改 RunHitFa1FrameLogic：六处加速度／高度步长改 double、Vy /= 1.4、移除 Y cap1 和提前 YInt，并加一条合同注释。目标与 HP、source X/Z、±7 死区、两个独立高度比较顺序、非角色目标正 Y 加1、±13/±2 夹紧、朝向及其他行为均保持。保留的整数 float 常量可精确表示。
- 测试仅新增212行：五参数调用真实 FrameLogic，跨零例消费既有 mechanics，完整例调用一次生产 Driver，没有以测试算法替换生产逻辑；902/type3/别名902/frame0/state3000/2048×1152 等前置正确，无旧907转换残留。
- native v2 在 spawn 后恢复并守卫声明分数初态，调用实际 NativeAi 一次。boundary 输出 -11.2→-10、整数仍-11；cross 输出 -1.25→-0.050000000000000044、整数仍-1、Vx9/Vy-2/Vz0.3；positive 输出3.75→4.95、整数仍3。三模式 exit0，未复制算法或改正式出生逻辑。
- 五参数 try 内失败由 finally 注销双方；setup 在 try 外，不能声称 setup 异常全覆盖。完整 wrapper callback 失败会 Dispose，但失败后的零计数断言不继续执行；正常返回通过才覆盖正常关闭后置。
- native 证据仅三受控初态的一次 AI 消费者。Unity 完整例仅受控902的一次完整 tick；不能合称完整 native Driver 逐字段一致、Genma 自然输入、原 Scene Play 或正式 EXE 验收。
- v1 分数初态因 spawn 截断无效，原件保留且不参与规则裁决。
