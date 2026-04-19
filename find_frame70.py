import re

text = open('I:/C++Test/NTSD/F.LF-master/LF/character.js', encoding='utf-8', errors='ignore').read()

keywords = ['super_punch', 'frame(70', 'punch_frame', 'hit_count', 'states[3]', 'state == 3', "case 'combo'"]
for kw in keywords:
    pos = text.find(kw)
    if pos >= 0:
        print('=== FOUND:', kw, 'at', pos, '===')
        print(text[max(0,pos-200):pos+400])
        print()
