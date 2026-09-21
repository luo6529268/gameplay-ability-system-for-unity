# NativeAI persisted alias 当前证据

当前状态：限定scope VERIFIED，最终证据以 ACCEPTANCE.md 为准。下文为阶段检查点，保留当时未完成项，不代表当前仍待这些验收。

## 已观察与已测量

- 正式 source closure / EXE 身份由 native-build-final/build-manifest.json 固定；构建仅 workspace diagnostic，不写 authority。
- Source18 双跑156167 bytes一致，SHA E3CCEFDA7A02AD530A5868B2C5375518D6434B8D118F1653090373992F36C07D；独立408检查PASS。分层为 profile7 / special6 / ordinary1 / main2 / tick2，不能称18个完整战斗场景。
- 初始 carrier RED：4/4失败，三条实际采集前置均成立；缺派生 alias 字段。
- 初次 carrier修后：5PASS/1FAIL；失败是full-row比较测试触发未初始化的无关candidate products。夹具明确隔离已独立拥有的products前置，仅该项重跑1/1PASS。失败保留。
- 初始 decision RED：15例7PASS/8FAIL；source indices0/2/4/6/7/9/11/14，包含额外RNG消费及错误移动。没有更改源输入规避。
- 修后联合 job01997c88d21741b18bc970fe3e6488df：27/27PASS，0.9922788秒；新21+既有特殊profile6（含零分配检查）。证据 decision-joint-pass/TestResults.xml。
- 四row文件与kernel分别独立只读review限定PASS；root检查diff。
- Scene文件SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持；本包不改Scene/资源/非战斗功能/框架。

## 实际代码改动

三条producer将持久NativeAiProfileObjectId写入独立派生数组；增长复制、stale检查和两种row比较包含该值。新增diagnostic enum62，不升级持久schema，不增加publisher pending字段。

kernel仅修改认可别名分类、match34/match1/match33及非零alias特殊决策停止；actual-ID1追击保持。停止返回复用FirstDecision出口，但该出口不再可被解释为“特殊技能成功”；成功须观察combo/source字段。同步输入边沿仍由后续sampler处理，不抛异常或中断tick。

## 未完成与验证范围限制

- 新增main正0x3c继续ordinary的代表测试待运行；它是调用者分支属性验证，不宣称与source13不同difficulty完整数值相同。
- 实际World两tick/source16/17、alias-only snapshot恢复后采集与stale检测fixture正在独立编写，尚未运行。
- 本批稳定后需要一次适当联合自检/运行时收口；当前不称已对齐。
- source本seed并未让所有0x39/0x3a/0x3b随机成功动作发生；sites准入证据有效但范围有限。
- independent model检查分支约束、完整同步数值/cursor算法及聚焦采样，不独立推导整个AI调用图或完整World状态。
- source持久alias手工设在spawn之后，出生/融合生产职责复用已验包，不将本fixture说成正式DAT出生全链。

Q06未完成，Q07 DAT与角色图片迁移未开始，总目标ACTIVE；已关闭融合不重跑。
