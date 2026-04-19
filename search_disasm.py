# -*- coding: utf-8 -*-
import re
import sys

def extract_func(content, funcname):
    marker = 'FUNCTION ' + funcname + ' at'
    start = content.find(marker)
    if start == -1:
        return 'NOT FOUND: ' + funcname
    sep = '\n' + '='*70 + '\nFUNCTION '
    next_func = content.find(sep, start + 1)
    if next_func == -1:
        return content[start:start + 8000]
    return content[start:next_func]

mode = sys.argv[1] if len(sys.argv) > 1 else 'list'

if mode == 'list':
    with open('J:/QQFile/NTSD2.4/ntsd24_pseudoc.txt', encoding='gbk', errors='replace') as f:
        content = f.read()
    funcs = re.findall(r'FUNCTION (\w+) at (0x[0-9A-Fa-f]+)', content)
    print('Total functions in pseudoc:', len(funcs))
    keywords = ['hit', 'fall', 'itr', 'frame', 'entity', 'state', 'attack',
                'interact', 'process', 'tick', 'input', 'gamemode', 'vrest',
                'serial', 'ai_', 'recover', 'lying', 'netinput', 'netsync']
    for name, addr in funcs:
        n = name.lower()
        if any(k in n for k in keywords):
            print(f'{addr}: {name}')

elif mode == 'list_full':
    with open('J:/QQFile/NTSD2.4/ntsd24_full_disasm.txt', encoding='gbk', errors='replace') as f:
        content = f.read()
    funcs = re.findall(r'FUNCTION (\w+) at (0x[0-9A-Fa-f]+)', content)
    print('Total functions in full_disasm:', len(funcs))
    keywords = ['hit', 'fall', 'itr', 'frame', 'entity', 'state', 'attack',
                'interact', 'process', 'tick', 'input', 'gamemode', 'vrest',
                'serial', 'ai_', 'recover', 'lying', 'netinput', 'netsync']
    for name, addr in funcs:
        n = name.lower()
        if any(k in n for k in keywords):
            print(f'{addr}: {name}')

elif mode == 'func':
    fname = sys.argv[2]
    src = sys.argv[3] if len(sys.argv) > 3 else 'pseudoc'
    fpath = 'J:/QQFile/NTSD2.4/ntsd24_pseudoc.txt' if src == 'pseudoc' else 'J:/QQFile/NTSD2.4/ntsd24_full_disasm.txt'
    with open(fpath, encoding='gbk', errors='replace') as f:
        content = f.read()
    result = extract_func(content, fname)
    print(result[:16000])

elif mode == 'search':
    term = sys.argv[2]
    src = sys.argv[3] if len(sys.argv) > 3 else 'pseudoc'
    fpath = 'J:/QQFile/NTSD2.4/ntsd24_pseudoc.txt' if src == 'pseudoc' else 'J:/QQFile/NTSD2.4/ntsd24_full_disasm.txt'
    with open(fpath, encoding='gbk', errors='replace') as f:
        lines = f.readlines()
    for i, line in enumerate(lines):
        if term in line:
            # Show context
            ctx_start = max(0, i - 2)
            ctx_end = min(len(lines), i + 3)
            print(f'--- Line {i+1} ---')
            for j in range(ctx_start, ctx_end):
                marker = '>>>' if j == i else '   '
                print(f'{marker} {j+1}: {lines[j].rstrip()}')
            print()

elif mode == 'addr':
    addr = sys.argv[2]  # e.g. '0x407B00'
    with open('J:/QQFile/NTSD2.4/ntsd24_full_disasm.txt', encoding='gbk', errors='replace') as f:
        content = f.read()
    # Find the function that contains this address
    pos = content.find(addr)
    if pos == -1:
        print(f'Address {addr} not found')
    else:
        # Find surrounding function
        sep = '\n' + '='*70 + '\n'
        func_start = content.rfind(sep, 0, pos)
        func_end = content.find(sep, pos)
        snippet = content[func_start:func_end if func_end != -1 else func_start + 10000]
        print(snippet[:12000])
