from pathlib import Path
import json,hashlib,datetime
root=Path.cwd(); task="NTSD-OPTIMIZATION-CURRENT-STAGE-PRIORITIES-20261010"
op=root/"docs/ai/FILE-OPERATIONS"/task; out=root/"artifacts/diagnostics"/task
manifest=json.loads((op/"manifest.json").read_text(encoding="utf8")); rows=json.loads((out/"catalog.json").read_text(encoding="utf8"))
items={r[0]:r for r in rows}
groups=[("P1／当前优先评审",["M-03","H-08","H-10"]),("P2／当前后续评审",["M-12","M-05","M-01","M-02","M-06"]),("父项目标保留／子项暂不续办",["H-06","H-07"]),("随选定改动验证／不单独插队",["H-09","H-11","M-09","M-15","M-04","L-07"]),("已评审暂不实施",["M-13","M-14"]),("用户暂缓",["M-11"]),("后期平台适配与发布",["H-01","H-02","H-03","H-04","H-05","M-07","M-08","M-10","L-01","L-02","L-03","L-04","L-05","L-06","L-08"])]
flat=[i for _,ids in groups for i in ids];assert len(flat)==34 and set(flat)==set(items)
notes={
"M-03":"第1项：复用已完成metadata/相机消重/共享绑定；评审当前命令物化与顶点准备剩余工作，不把GPU认证缺失当当前优化内容。",
"H-08":"第2项：先核当前纹理、图集、解码暂存的实际驻留与生命周期；预印/ATLAS资源实施另有准确范围，不等待AB整体迁移。",
"H-10":"第3项：先核原clip、PCM、播放副本与暂存占用；不预设已有泄漏或收益，不改变声音语义。",
"M-12":"第4项：核解码到上传间的字节占用与释放，存在实际高峰后才定背压。",
"M-05":"第5项：核active/visible/inactive/logic-only真实shell与组件成本。",
"M-01":"第6项：核worker实际eligible/active/fallback/等待；不把默认开关当实际并行。",
"M-02":"第7项：复用已有合批，核多资源segment剩余成本；不重复共享绑定优化。",
"M-06":"第8项：按真实slot/command/shell峰值核预热和容量，避免无据扩大池。",
"H-06":"Role限定默认和FindEffect既有成果保留；父项开放，Role精确配对诊断不续办。",
"H-07":"33ms等父目标开放；输入约12ms未解决且当前无新有据方案；LATE已评审暂无必要改动。不重复旧候选。",
"M-13":"原有界评审未建立实际高价值热点，保留结论。",
"M-14":"保留已完成评审和可变Editor守卫；不原样重跑。",
"M-11":"USER_HOLD；架构专项不因重排解冻。",
"H-01":"本地AB仅内容载体草案；留到部署阶段，不作为当前CPU/驻留内存评审前置。",
"H-02":"Android构建、安装、冷启动在平台阶段处理。",
"H-04":"IL2CPP、ARM64、AOT与裁剪在平台适配阶段处理。",
"H-05":"Android真机及GPU认证在后期平台验收处理；当前CPU渲染优化可独立评审。",
"H-11":"当前选定改动继续守容量、适用CPU 0GC及关闭；GPU/native/device完整认证后置。",
"M-15":"当前修改所需Kernel身份/可复现证据随项维护；正式平台构建后置。"}
for i in flat:
 if i not in notes: notes[i]="随具体修改复用受影响证据，不启动独立全量任务。" if i in groups[3][1] else "待后期相应平台、设备或产品交付阶段处理，不抢当前战斗性能排程。"
stage={i:g for g,ids in groups for i in ids};rank={i:str(j+1) for j,i in enumerate(flat[:8])}
original=lambda i: "高" if i.startswith("H-") or i=="M-03" else "中" if i.startswith("M-") else "低"
assert [sum(original(i)==p for i in flat) for p in ["高","中","低"]]==[12,14,8]
common="""2026-10-10 用户更正：Android真机/GPU认证、IL2CPP/ARM64属于后期；AB部署、Android构建和正式发布随平台交付后置。当前按战斗CPU、CPU侧渲染工作和驻留内存问题排序。原34项ID及历史12高/14中/8低保留，原风险重要性不再冒充当前执行顺序。
当前P1：M-03 → H-08 → H-10；P2：M-12 → M-05 → M-01 → M-02 → M-06。此顺序是评审/投入顺序，不是收益排名，也不代表每项已确认必须改代码。
H07输入约12ms尚未解决、当前无新有据方案而暂缓；H07-LATE已评审暂无必要优化。H06既有成果保留，不续Role精确配对；producer暂缓、95和旧候选不自动开启。只处理原方案，不恢复旧Goal或34项自动队列。
本轮已先评审M03历史证据和当前生产入口。相同显示采样复用已存在；剩余先看命令物化与顶点准备是否有可消除重复，不能因GPU证书未完成就追加设备任务。脚本/资源/设置未改，未跑Unity或新测试/测量，未发送执行指令；监测不恢复。
"""
table="| 当前阶段 | 次序 | ID及单项方案 | 原风险等级 | 当前处理范围 |\n|---|---:|---|---|---|\n"
for i in flat:
 _,name,link=items[i];table+=f"| {stage[i]} | {rank.get(i,'—')} | [{i} {name}]({link}) | {original(i)} | {notes[i]} |\n"
nav=f"# 当前战斗优化优先级（2026-10-10 用户阶段更正）\n\n{common}\n## 完整34项当前排程\n\n{table}\nP1三项、P2五项、父目标保留两项、伴随保障六项、已评审暂不实施两项、USER_HOLD一项、后期交付十五项：合计34，不增删父项。M-03编号沿用，2026-10-06已提升为原高等级，不能因M前缀再次降级。\n\n当前选择依据：M03已有历史千人CPU物化耗时线索且可先独立窄读；H08/H10是当前资源占用问题但实施预算/产物范围需形成方案。后续证据若不支持收益，应具名结束该候选；不为维持排序制造优化。\n\n[唯一进度](battle-optimization-progress-tracker.md) · [本轮评审与审计](../../../artifacts/diagnostics/{task}/REPORT.md)。\n\n## 以下为被本次阶段排程取代的历史导航与原登记\n\n下方旧“当前/首项/中优先级”等属于当时快照；最新阶段与顺序只以上表为准，原方案、旧结果与失败全部保留。\n\n"
headers={m['path']:None for m in manifest}
headers['Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md']=nav
for i in flat:
 p='Assets/NTSD/Docs/'+items[i][2]
 headers[p]=f"## 2026-10-10 当前阶段更正：{i}\n\n| 当前阶段 | 当前次序 | 原风险等级 |\n|---|---|---|\n| {stage[i]} | {rank.get(i,'不单独排队')} | {original(i)} |\n\n{notes[i]}\n\n本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。\n\n---\n\n"
extra={
'docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md':"当前本地CPU/内存评审不以AB部署、IL2CPP/ARM64、Android包或设备认证先完成为前提。真正依赖资源载体/新格式的实施应精确声明共享接口，不要求父项全完成。正式设备验收后置不等于取消；当前实际路径适用的正确性、容量、0GC与关闭仍按原范围验收。M03 publication不变但alpha改变仍需刷新显示，透明顺序/fail-closed不变。",
'docs/ai/OPTIMIZATION-REVIEW-20261009/NEXT-SESSION-PROMPT.md':"当前没有新的生产实施指令。禁止复制下方旧H06/H01/H07历史提示词执行。恢复时读总表文首与唯一进度，继续M03具名评审；仅在具体可消除工作、准确owner/路径、收益验证和结束条件明确后才形成实施任务，不自动排队其它点。",
'Assets/NTSD/Docs/battle-optimization-progress-tracker.md':"当前：M-03 / INITIAL_SOURCE_AND_EVIDENCE_REVIEW_COMPLETE / IMPLEMENTATION_NOT_SELECTED。下一步范围锁定原M03命令物化/顶点准备，先形成有据改法；H08/H10只是随后评审次序。历史REPORT中的Goal active/旧批数不是新任务。",
'docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md':"决定 CURRENT_STAGE_PRIORITY_FIRST：用户明确把平台交付后置；纠正此前M03/H08/H10统一标中、把Android高风险当当前优先实施的混淆。用户允许同一问题因果链内的必要调查，不扩成方案外的新方向。"}
for p in headers:
 if headers[p] is None and not p.endswith('FILE-OPERATIONS/INDEX.md'):
  headers[p]=f"## 2026-10-10 最新阶段与排程（{task}）\n\n{common}\n{extra.get(p,'优先级总表和34单项卡已同步，原证据/暂缓/失败保持。')}\n\n完整排程：Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md；唯一进度：Assets/NTSD/Docs/battle-optimization-progress-tracker.md；本轮报告：artifacts/diagnostics/{task}/REPORT.md。\n\n以下均保留此前时点；旧排程与当前状态冲突时以本段及当前总表为准。\n\n---\n\n"
# Validate pre-write identities; INDEX already has PLANNED prepend by setup.
for m in manifest:
 p=m['path'];original_bytes=(root/m['backup']).read_bytes();assert hashlib.sha256(original_bytes).hexdigest()==m['sha256']
 if p.endswith('FILE-OPERATIONS/INDEX.md'):assert original_bytes in (root/p).read_bytes()
 else:assert (root/p).read_bytes()==original_bytes,p
record=op/'RECORD.md'
with record.open('a',encoding='utf8') as f:f.write('\n状态追加：RUNNING；执行apply.py，全部原文SHA与备份一致后前置更正。\n')
for p,h in headers.items():
 if h is not None:
  f=root/p;f.write_bytes(h.encode('utf8')+f.read_bytes())
(out/'schedule.json').write_text(json.dumps([dict(id=i,stage=stage[i],order=rank.get(i),originalPriority=original(i),path='Assets/NTSD/Docs/'+items[i][2]) for i in flat],ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'updated_documents':43,'original_counts':[12,14,8],'ids':len(flat)},ensure_ascii=True))
