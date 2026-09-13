# Q05-A2 几何内容契约进度

状态：FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / SOURCE_AND_ALGORITHM_INTEGRATION_PENDING。保持活跃；Q05整体、生产来源和Q06候选算法未完成。

## 实际实现与默认策略

BattleBodyBoxValue增加ZWidth和HasGeometry，所有字段参与值相等/hash；BodyBox DTO和Adapter双向保留深度及有效性。为保留现有合成数据语义，legacy default(Value)与new(0,0,0,0)仍表示显式零几何；内部geometryMissing编码其反值。native decoder永远根据四个strict int成功状态显式构造，缺失/非法几何为HasGeometry=false，与显式零不同，不return default掩盖失败。

InteractionArea增加独立z及hasGeometry；旧手工/legacy默认z0、有效true、zwidth15保持。新的ApplyInteractionGeometry读取原版raw zwidth/default0和z，不在数据层回退15，不混同dz/dvz。该函数仅负责几何，并保留target其余字段与原AST，不宣称已完整解码ITR。kind100100在几何不全时保留record/kind与false有效性；显式0/负尺寸保持原值，不归一化。

CopyFrom及已有MemberwiseClone保留新字段，反复覆盖可清除上一次z/无效状态。HitPlan ItrProjection及两路Fingerprint同步z/hasGeometry。现有kind5兼容替换保留源几何，投影与实际ShallowCopy一致；未增加native strength替换规则。没有改collector、空间查询或生成顺序，Q03候选42比较的27个旧首差仍未关闭。

## 验证证据

- 新C++ source-linked工具直接对每个bdy/itr调用CollisionGeometry28.decode。58文件88条见证双跑一致，含Q03原14对及缺失/非法/overflow/重复/大小写/packed/100100/深度/零/负尺寸。全部93个源/header/fixture输入最终hash稳定，正式EXE B1E13A…9033。见authority-identities.json/native.tsv。
- RED jobbc20a0f2174b473baa66adc4b82a1ea9：92/92缺入口/字段失败，RED.xml保存。
- 首次联合job03da83ed3c744e8885326a3fd1dc3de5：375项371PASS/4FAIL。新几何92、旧Body6、CPoint43和OPoint49全部通过；4失败在HitPlan旧夹具，原GREEN-attempt1.xml保留。
- 独立Change NTSD28-Q05-RELATED-HITPLAN-FIXTURE-CORRECTION-001仅修正Editor测试。3项仍要求已由Goal20 R5退休的holder.ComboCountAtk/KillStat额外写入；1项手工observer绕过production kind7→Unsupported guard。未改production统计/分派，保留所有HP/world/native knockout/帧/RNG断言，不以放宽断言代替修规则。
- 最终完整重跑job66adde76e16648ddb66882e8856a13fe：375/375（Geometry92+Body6+HitPlan185+CPoint43+OPoint49）。包括88原版几何逐字段、Body局部z忽略、identity、默认/缺失区分、CopyFrom/clone反复覆盖、projection及两路fingerprint一致。
- dotnet build Tools/NTSD28ContentAudit/UnityContentCapture.csproj -v quiet：0warning/0error。旧Unity138 DAT parse/Converter全成功，与OPoint轮比较各旧投影section无新增差异。旧工具尚未输出新geometry标记，故仅作旧加载回归，不能证明新数据全域一致。
- 完整BattleRuntimeSelfCheck：03:42:53.979888 UTC新request，结果mtime晚于request并PASS；旧结果另存NOT-CURRENT。
- 最终CS0、NTSD_Battle isDirty=false/root14。3059保护基线3029不变、30个声明脚本差异、0缺失；比OPoint轮新增5个基线脚本差异，均属几何/关联测试两个Record。

## 范围、审阅与后继

Geometry九脚本，加独立关联测试一脚本。人工复核新字段全copy/hash、native strict成功位、旧kind5保持、readonly/default策略、只读witness与旧断言修正。没有修改非战斗、Unity/GAS框架、Gen/Plugins、Scene/Prefab、正式DAT/图片、33ms或十一阶段；没有新的runtime owner/queue/worker，无commit/push/删除。

本包没有新Play/GPU证书。Source manager/完整ITR转换、外层解码身份、entity12→13/aggregate20→21/checksum23→24/character1→2/base1→2仍须同Q05窗口完成，禁止发布中间ABI。CPoint/OPoint/Geometry三Change在A2/Q05来源、identity、Play完成时统一回访，候选算法在Q06验证。

下一Task NTSD28-Q05-ITR-STRENGTH-CONTENT-CONTRACT-001：原版ITR40字段剩余drain/sound/cover及first-int动作表示、weapon_strength19+index完整值/复制/读取契约。不能把dz当z、gain当drain或frame.sound当ITR.sound；不要提前修strength行选择/候选算法。之后完整Logan转换入口与新投影/身份仍是明确后继。旧phase断言留Q12、landing除法一ULP留Q06、默认stage.dat暂缓保持。

最终Ledger实际PASS：479 records /77 governed code files，见Geometry工件ledger-final.txt。几何九脚本与独立测试一脚本已人工复核；未声明脚本无本轮额外变化。
