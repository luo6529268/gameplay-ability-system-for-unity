import sys, re

key = 'odBearBecauseHeIsVeryGoodSiuHungIsAGo'
dat_path = sys.argv[1] if len(sys.argv) > 1 else 'J:/QQFile/NTSD2.4/chars/naruto.dat'
frame_start = int(sys.argv[2]) if len(sys.argv) > 2 else 60
frame_end = int(sys.argv[3]) if len(sys.argv) > 3 else 70

with open(dat_path, 'rb') as f:
    buf = f.read()

head = buf[:64].decode('ascii', errors='ignore').strip()
if '<bmp_begin>' in head or '<frame>' in head:
    text = buf.decode('ascii', errors='ignore')
else:
    dec = bytearray(max(0, len(buf) - 123))
    for i in range(len(dec)):
        dec[i] = (buf[123 + i] - ord(key[i % len(key)])) & 0xFF
    text = dec.decode('ascii', errors='ignore')

frames = list(re.finditer(r'<frame>.*?<frame_end>', text, re.DOTALL))
for m in frames:
    ft = m.group()
    fn_match = re.match(r'<frame>\s*(\d+)', ft)
    if not fn_match:
        continue
    fn = int(fn_match.group(1))
    if frame_start <= fn <= frame_end:
        print(ft)
        print('---')
