from pathlib import Path
import hashlib, json, math

root = Path(__file__).resolve().parent
repo = root.parents[2]
def read(name):
    return json.loads((root / name).read_text(encoding='utf-8-sig'))
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
def quantile(values, fraction):
    values = sorted(values)
    index = (len(values) - 1) * fraction
    low = int(index)
    high = min(low + 1, len(values) - 1)
    return values[low] + (values[high] - values[low]) * (index - low)

summary = {'scope': 'diagnostic validity and bounded implementation design; no production admission', 'versions': []}
for name in ['uv-scoped-local-cost.json', 'uv-latched-local-cost.json', 'uv-segment-local-cost.json']:
    report = read(name)
    assert report['calibrationBefore']['passed'] and report['calibrationAfter']['passed']
    windows = report['windows']
    assert len(windows) in (12, 16)
    row = {'file': name, 'status': report['status'], 'semanticComparisons': report['semanticComparisons'], 'scopeCount': 0, 'allocationEvents': 0, 'layouts': []}
    for index, window in enumerate(windows):
        assert window['reuse'] == ([False, True, True, False][index % 4])
        assert len(window['ticks']) == len(window['gcScopes']) == 32
        for scope in window['gcScopes']:
            assert scope['valid'] and scope['calibratedBefore'] and not scope['saturated'] and not scope['wrapped']
            assert scope['allocationEvents'] == 0
            row['scopeCount'] += 1
        values = window['ticks']
        scale = 1000 / report['frequency']
        assert math.isclose(sum(values) / len(values) * scale, window['meanMs'], abs_tol=1e-10)
        assert math.isclose(quantile(values, .95) * scale, window['p95Ms'], abs_tol=1e-10)
        assert math.isclose(quantile(values, .99) * scale, window['p99Ms'], abs_tol=1e-10)
    for index in range(0, len(windows), 4):
        block = windows[index:index+4]
        off = (block[0]['meanMs'] + block[3]['meanMs']) / 2
        on = (block[1]['meanMs'] + block[2]['meanMs']) / 2
        row['layouts'].append({'name': block[0]['layout'], 'offMeanMs': off, 'onMeanMs': on, 'relativeChangePercent': (on / off - 1) * 100})
    summary['versions'].append(row)

for result_name in ['focused-result.json', 'uv-scoped-test-result-final.json', 'uv-latched-test-result-01.json', 'uv-segment-test-result-final.json']:
    data = read(result_name)['result']['data']
    assert data['status'] == 'succeeded'
    counts = data['result']['summary']
    assert counts['total'] > 0 and counts['passed'] == counts['total']
    summary[result_name] = {'jobId': data['job_id'], 'summary': counts}

protected = read('protected-before.json')
changed = [path for path, digest in protected.items() if not (repo / path).exists() or sha(repo / path) != digest.lower()]
summary['protection'] = {'count': len(protected), 'changed': changed}
assert not changed
before_scene = read('scenes-before.json')['result']['data']['scenes']
after_scene = read('scenes-final.json')['result']['data']['scenes']
assert before_scene == after_scene and not after_scene[0]['isDirty']
summary['scene'] = after_scene
summary['diagnosticSha256'] = sha(repo / 'Assets/NTSD/Scripts/Test/Editor/M03MeshUvDimensionReuseDiagnosticsEditorTests.cs')
for assembly in ['Assembly-CSharp.dll', 'Assembly-CSharp-Editor.dll']:
    path = repo / 'Library/ScriptAssemblies' / assembly
    summary[assembly] = sha(path)

with (root / 'analysis-summary.json').open('x', encoding='utf-8') as handle:
    json.dump(summary, handle, ensure_ascii=False, indent=2)
print(json.dumps({'versions': summary['versions'], 'protection': summary['protection'], 'scene': summary['scene']}, ensure_ascii=False))
