"""Validate source API platform fixtures; this is not a Unity parity certificate."""
import hashlib
import json
import sys
from pathlib import Path


def validate(path):
    data = path.read_bytes()
    rows = [json.loads(line) for line in data.splitlines()]
    assert len(rows) == 21
    assert len({row['name'] for row in rows}) == 21
    checks = 2
    for row in rows:
        p = row['params']
        before, contact, motion = (row[key] for key in ('before', 'afterCandidate', 'afterMotion'))
        rejected = row['name'] in {
            'strict_x_edge', 'strict_z_edge', 'previous_y_reject',
            'type5_reject30', 'same_group_reject50',
        }
        slot = p['sourceSlot']
        surface = -20 + round(p['itrDvy'])
        if p['extraY']:
            if p['extraY'] < surface or (p['extraY'] == surface and p['extraSlot'] < slot):
                surface, slot = p['extraY'], p['extraSlot']
        moves = not rejected and slot != 0 and not p['removeSource'] and not (
            row['targetType'] == 3 and row['targetState'] not in (3000, 3006, 3003)
        )
        expected_contact = [p['targetX'], -10 if rejected else surface, p['targetZ'],
                            p['targetX'], -10, p['targetZ']]
        expected_motion = list(expected_contact)
        if moves:
            x = p['targetX'] + (10.5 if p['splitFrame'] else -2.5 if p['left'] else 2.5)
            y, z = surface + 1.5, p['targetZ'] - 2.5
            expected_motion = [round(x), round(y), round(z), x, y, z]
        assertions = [
            row['candidateSuccess'] is True,
            row['motionSuccess'] == (not p['removeSource']),
            contact['position'] == expected_contact,
            contact['platformSlot'] == (0 if rejected else slot),
            contact['reference'] == (0 if rejected else surface),
            contact['shadow'] == (0 if rejected else surface),
            contact['previousXYZ'] == before['previousXYZ'],
            row['linkedMotion'] == moves,
            motion['position'] == expected_motion,
            motion['platformSlot'] == contact['platformSlot'],
            motion['shadow'] == contact['shadow'],
            motion['reference'] == (expected_motion[1] if moves else contact['reference']),
            motion['previousXYZ'] == contact['previousXYZ'],
        ]
        assert all(assertions), (row['name'], [i for i, passed in enumerate(assertions) if not passed])
        checks += len(assertions)
        if p['physicsAfter']:
            physics = row['afterPhysics']
            assert physics['previousXYZ'] == motion['position'][:3]
            assert physics['position'][:3] == [int(v) for v in motion['position'][3:]]
            checks += 2
    return {'status': 'PASS', 'cases': len(rows), 'checks': checks,
            'sha256': hashlib.sha256(data).hexdigest(),
            'scope': 'Source candidate/motion/physics API composition; not full-tick or Unity parity.'}


if __name__ == '__main__':
    result = validate(Path(sys.argv[1]))
    print(json.dumps(result, indent=2))
    if len(sys.argv) > 2:
        Path(sys.argv[2]).write_text(json.dumps(result, indent=2), encoding='utf-8')
