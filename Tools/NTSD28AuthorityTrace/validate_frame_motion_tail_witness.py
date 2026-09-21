"""Independent source-API expectations; not a formal EXE or Unity certificate."""
import hashlib
import json
import sys
from pathlib import Path


def validate(path):
    data = path.read_bytes()
    rows = [json.loads(line) for line in data.splitlines()]
    base = [100, -10, 250, 100.75, -10.25, 250.5]
    # Explicit arithmetic expectations, independent of the captured after values.
    expected = [
        ('neutral', base, [4, -3, 2], -10, 0, True),
        ('delay_positive', base, [1, -.75, .5], -10, 1, True),
        ('delay_negative', base, [4, -3, 2], -10, -1, True),
        ('own_fractional_velocity_is_not_integer', base, [4, -3, 2], -10, 0, True),
        ('integer_velocity_depth', base, [4, -1, -3], -10, 0, True),
        ('position_x', [102, -10, 250, 102.5, -10.25, 250.5], [0, -3, 2], -10, 0, True),
        ('position_x_left', [98, -10, 250, 97.5, -10.25, 250.5], [0, -3, 2], -10, 0, True),
        ('position_all_quarter', [102, -12, 250, 102.5, -11.5, 250.5], [0, 0, 0], -10, 2, True),
        ('override_then_quarter', base, [.5, .25, -.5], -10, 1, True),
        ('linked_then_own_position', [104, -10, 248, 104.5, -9.5, 248.5], [0, 0, 0], -8, 0, True),
        ('pending_skips_all', base, [4, -3, 2], -10, 1, False),
        ('missing_link_still_own_position', [102, -10, 250, 102.5, -10.25, 250.5], [0, -3, 2], -10, 0, False),
    ]
    assert len(rows) == len(expected), len(rows)
    checks = 1
    for index, (row, (name, position, motion, reference, delay, success)) in enumerate(zip(rows, expected)):
        assertions = {
            'index': (row['index'], index),
            'name': (row['name'], name),
            'before': (row['before'], dict(position=base, motion=[4, -3, 2], reference=-10, delay=delay)),
            'success': (row['success'], success),
            'after': (row['after'], dict(position=position, motion=motion, reference=reference, delay=delay)),
        }
        for key, (actual, wanted) in assertions.items():
            assert actual == wanted, f'{name}/{key}: actual={actual!r}, expected={wanted!r}'
            checks += 1
    return dict(status='PASS', rows=len(rows), assertions=checks, sha256=hashlib.sha256(data).hexdigest())


if __name__ == '__main__':
    print(json.dumps(validate(Path(sys.argv[1])), indent=2))
