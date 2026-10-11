from pathlib import Path
import json, hashlib, shutil
root = Path(__file__).resolve().parent
repo = root.parents[2]
op = repo / 'docs/ai/FILE-OPERATIONS/NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010'
paths = [
 'docs/ai/CURRENT-AUTHORITY.md', 'docs/ai/STATE.md', 'Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md',
 'Assets/NTSD/Docs/battle-optimization-progress-tracker.md',
 'Assets/NTSD/Docs/android-mobile-readiness/m-03-dynamic-mesh-upload.md',
 'docs/ai/CHANGE-LEDGER.md',
 'docs/ai/CHANGE-RECORDS/NTSD-M03-UV-DIMENSION-REVIEW-001.md',
 'docs/ai/TASKS/NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010.md']
manifest = []
for i, relative in enumerate(paths):
    source = repo / relative
    backup = op / 'before' / f'final-{i:02d}.bin'
    assert not backup.exists()
    shutil.copyfile(source, backup)
    digest = hashlib.sha256(source.read_bytes()).hexdigest()
    assert hashlib.sha256(backup.read_bytes()).hexdigest() == digest
    manifest.append({'path': relative, 'sha256': digest, 'backup': str(backup.relative_to(repo))})
with (op / 'final-before-manifest.json').open('x', encoding='utf-8') as f:
    json.dump(manifest, f, ensure_ascii=False, indent=2)
with (op / 'RECORD.md').open('a', encoding='utf-8') as f:
    f.write('\n收尾写前：final-before-manifest保存8文档当前实际字节，前置最新有限结论，原状态/失败/实验全部保留。仅本新Change元数据PLANNED→VERIFIED（只诊断），Task→SCOPED_REVIEW_COMPLETE；派发以实际receipt为准。\n')
prefix = '''> **2026-10-10 M03独立评审收口：SCOPED_REVIEW_COMPLETE / 有界生产候选方案READY，性能门仍OPEN。** NTSD-M03-UV-DIMENSION-REVIEW-001 VERIFIED仅诊断；8项既有契约通过，新UV三版各41边界/共1408有效scope事件0。两槽轮换退化、array限定不通用、segment数学模型连续段有信号但逐quad换段负控5.0494%超过事前5%，整体未通过，不写生产收益。最终指令只利用既有canAppend分支：新段首quad/custom/Strict原路，真正append后段内准备尺寸，默认OFF，再验真实Backend/payload/像素/完整Build成本。未改生产、未开启下一点。Task NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010；报告/准确三文件修复指令位于artifacts/diagnostics/NTSD-M03-MATERIALIZATION-REUSE-REVIEW-20261010/REPORT.md与execution-handoff.txt；实际发送以dispatch-receipt/status为准。155保护无漂移、原Menu clean8。M03父项OPEN，H07/producer暂缓与95OFF等保持；下方是此前时点。

'''
for relative in paths[:6]:
    path = repo / relative
    path.write_text(prefix + path.read_text(encoding='utf-8-sig'), encoding='utf-8')
record = repo / paths[6]
text = record.read_text(encoding='utf-8-sig').replace('status: PLANNED', 'status: VERIFIED', 1)
text += '''\n## 最终证据与范围更正\n原两槽计划为V1历史；V1三纹理退化，不推广。V2锁首array不通用，V3已切为局部segment状态。三版各一次组，384+512+512可靠GC scopes通过、所有41语义比较通过，纯数学cost的整体门V2/V3均未通过。VERIFIED仅工具正确执行和诚实保留原件，不是候选收益/生产验收。\n首次新文件未导入job total0保留；线程bytes正校准失败发生在0成本窗，已改用未修改的BattleScopedGcAllocationRecorder；脚本最新job ae369f1ad0364fbca7ebe775537a1e06实际1/1。完整命令/输出/边界见REPORT与analysis-summary，生产未改。\n新的有界生产接法不在本Change实现，另Task/Change按execution-handoff声明三域，默认OFF并先真实Backend验证。\n'''
record.write_text(text, encoding='utf-8')
task = repo / paths[7]
task.write_text(task.read_text(encoding='utf-8-sig').replace('状态：IN_PROGRESS / CAUSAL_SOURCE_AND_EDITOR_REVIEW', '状态：SCOPED_REVIEW_COMPLETE / BOUNDED_IMPLEMENTATION_HANDOFF_READY_WITH_OPEN_PERFORMANCE_GATE', 1) + '\n准确结果REPORT.md；实际派发/新turn见dispatch-receipt.json、dispatch-status.json。本讨论会话仅诊断，未接生产。\n', encoding='utf-8')
print('Final documentation updated with backups; dispatch remains receipt-dependent.')
